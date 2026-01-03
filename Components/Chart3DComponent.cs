using System;
using System.Collections.Generic;
using VAGSuite.MapViewerEventArgs;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Components
{
    public enum RenderMode
    {
        Solid,
        Wireframe
    }

    /// <summary>
    /// Encapsulates the 3D surface chart for map visualization using OpenTK.
    /// </summary>
    public class Chart3DComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IChartService _chartService;
        
        // Reference to the external chart control (provided by MapViewerEx designer)
        private OpenTK.GLControl _glControl;
        
        // Modern OpenGL Fields
        private int _vbo;
        private int _cbo;
        private int _ebo;
        private int _wireframeEbo;  // Separate EBO for wireframe (quad edges only, no diagonals)
        private int _shaderProgram;
        private int _vertexCount;
        private bool _buffersInitialized;
        private bool _firstPaint = true;

        // Mesh bounds for camera positioning
        private Vector3 _meshMinBounds;
        private Vector3 _meshMaxBounds;
        private Vector3 _meshCenter;
        private float _dataMinZ;
        private float _dataMaxZ;

        // State references
        private int _tableWidth;
        private bool _isSixteenBit;
        private bool _isLoaded = false;
        private float _rotation = -45f;
        private float _elevation = 30f;
        private float _zoom = 1.2f;
        private RenderMode _renderMode = RenderMode.Solid;
        private ViewType _viewType;
        private string _mapName;
        private string _xAxisName;
        private string _yAxisName;
        private string _zAxisName;
        private byte[] _mapContent;
        private byte[] _originalContent;
        private byte[] _compareContent;
        private int[] _xAxisValues;
        private int[] _yAxisValues;
        private double _correctionFactor;
        private double _correctionOffset;
        private bool _isCompareViewer;
        private bool _onlineMode;
        private bool _overlayVisible;
        private bool _isUpsideDown;
        private Point _lastMousePos;

        // Rotation mode: true = rotate mesh, false = rotate camera
        private bool _rotateMesh = true;

        // For responsive rotation - throttle redraws
        private System.Windows.Forms.Timer _renderTimer;
        private bool _pendingRender;
        private DateTime _lastRenderTime;
        private const int MIN_RENDER_INTERVAL_MS = 16; // ~60 FPS max

        #endregion

        #region Events

        public event EventHandler<SurfaceGraphViewChangedEventArgsEx> ViewChanged;
        public event EventHandler RefreshRequested;

        #endregion

        #region Constructors

        public Chart3DComponent()
        {
            _chartService = new ChartService();
        }

        public Chart3DComponent(IChartService chartService)
        {
            _chartService = chartService ?? throw new ArgumentNullException("chartService");
        }

        /// <summary>
        /// Sets the external GLControl to use instead of creating a new one.
        /// </summary>
        public void SetChartControl(OpenTK.GLControl externalChart)
        {
            Console.WriteLine("Chart3DComponent: SetChartControl called");
            _glControl = externalChart;
            if (_glControl != null)
            {
                _glControl.Load += OnGLLoad;
                _glControl.Paint += OnGLPaint;
                _glControl.Resize += OnGLResize;
                _glControl.MouseDown += OnGLMouseDown;
                _glControl.MouseMove += OnGLMouseMove;
                _glControl.MouseWheel += OnGLMouseWheel;
                _glControl.KeyDown += OnGLKeyDown;
                _glControl.Dock = DockStyle.Fill;

                // If the control is already loaded or handle created, initialize now
                // Verified: OpenTK GLControl requires a handle to call MakeCurrent
                if (_glControl.IsHandleCreated)
                {
                    Console.WriteLine("Chart3DComponent: Handle already created, initializing...");
                    _isLoaded = true;
                    InitializeChart3D();
                    UpdateBuffers(); // Ensure buffers are filled if data was already loaded
                }
            }
        }

        private void OnGLLoad(object sender, EventArgs e)
        {
            Console.WriteLine("Chart3DComponent: OnGLLoad fired");
            if (!_isLoaded)
            {
                _isLoaded = true;
                InitializeChart3D();
                UpdateBuffers(); // Ensure buffers are filled on first load
            }
        }

        private void InitializeRenderTimer()
        {
            if (_renderTimer == null)
            {
                _renderTimer = new System.Windows.Forms.Timer();
                _renderTimer.Interval = MIN_RENDER_INTERVAL_MS;
                _renderTimer.Tick += (s, e) =>
                {
                    if (_pendingRender && _glControl != null)
                    {
                        _pendingRender = false;
                        _glControl.Invalidate();
                    }
                };
                _renderTimer.Start();
            }
        }

        private void OnGLPaint(object sender, PaintEventArgs e)
        {
            if (_glControl == null) return;
            if (_mapContent == null)
            {
                return;
            }
            
            // Log first paint
            if (_firstPaint) {
                Console.WriteLine($"Chart3DComponent: First Paint - BuffersInit: {_buffersInitialized}, VertexCount: {_vertexCount}, Shader: {_shaderProgram}");
                _firstPaint = false;
            }
            
            if (!_isLoaded) return;

            try
            {
                _glControl.MakeCurrent();
                CheckGLError("MakeCurrent");

                GL.ClearColor(Color.FromArgb(50, 50, 50));
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                CheckGLError("Clear");
            
                SetupViewport();
            
                // Draw Bounding Box and Grids
                DrawBoundingBox();

                if (_buffersInitialized && _vertexCount > 0 && _shaderProgram > 0)
                {
                    GL.UseProgram(_shaderProgram);

                    int modelViewLoc = GL.GetUniformLocation(_shaderProgram, "uModelView");
                    int projectionLoc = GL.GetUniformLocation(_shaderProgram, "uProjection");
                    int modelLoc = GL.GetUniformLocation(_shaderProgram, "uModel");
                    int posLoc = GL.GetAttribLocation(_shaderProgram, "aPosition");
                    int colLoc = GL.GetAttribLocation(_shaderProgram, "aColor");
                    
                    Matrix4 projection;
                    Matrix4 modelView;
                    GetMatrices(out projection, out modelView);

                    GL.UniformMatrix4(modelViewLoc, false, ref modelView);
                    GL.UniformMatrix4(projectionLoc, false, ref projection);
                    
                    // Model matrix is now Identity because rotation is unified in modelView
                    Matrix4 model = Matrix4.Identity;
                    GL.UniformMatrix4(modelLoc, false, ref model);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                    GL.EnableVertexAttribArray(posLoc);
                    GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, _cbo);
                    GL.EnableVertexAttribArray(colLoc);
                    GL.VertexAttribPointer(colLoc, 4, VertexAttribPointerType.Float, false, 0, 0);

                    if (_renderMode == RenderMode.Wireframe)
                    {
                        // Wireframe Mode: Only draw the edges
                        if (_wireframeEbo != 0)
                        {
                            GL.Disable(EnableCap.PolygonOffsetFill);
                            GL.LineWidth(1.0f);
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                            GL.DrawElements(PrimitiveType.Lines, (int)(_vertexCount * 1.33f), DrawElementsType.UnsignedInt, IntPtr.Zero);
                        }
                    }
                    else
                    {
                        // Solid Mode: Draw filled triangles + subtle wireframe overlay
                        GL.Enable(EnableCap.PolygonOffsetFill);
                        GL.PolygonOffset(1.0f, 1.0f);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                        if (_wireframeEbo != 0)
                        {
                            GL.Disable(EnableCap.PolygonOffsetFill);
                            GL.LineWidth(0.5f);
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                            GL.DrawElements(PrimitiveType.Lines, (int)(_vertexCount * 1.33f), DrawElementsType.UnsignedInt, IntPtr.Zero);
                        }
                    }

                    GL.DisableVertexAttribArray(posLoc);
                    GL.DisableVertexAttribArray(colLoc);
                    GL.UseProgram(0);
                }
            
                _glControl.SwapBuffers();
                
                // Draw axis labels using GDI+ after swap
                RenderAxisLabels();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: Paint error: " + ex.Message);
            }
        }

        private void CheckGLError(string location)
        {
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Console.WriteLine($"OpenGL Error at {location}: {err}");
            }
        }

        private void OnGLResize(object sender, EventArgs e)
        {
            if (_glControl == null) return;
            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
            SetupViewport();
        }

        private void SetupViewport()
        {
            Matrix4 projection;
            Matrix4 modelview;
            GetMatrices(out projection, out modelview);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

        private void GetMatrices(out Matrix4 projection, out Matrix4 modelview)
        {
            float aspectRatio = (float)_glControl.Width / Math.Max(1, _glControl.Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), aspectRatio, 0.1f, 1000f);

            // Camera distance - normalized to the mesh bounds
            float maxDimension = Math.Max(_meshMaxBounds.X - _meshMinBounds.X,
                                          Math.Max(_meshMaxBounds.Y - _meshMinBounds.Y,
                                                   _meshMaxBounds.Z - _meshMinBounds.Z));
            
            // Unified Zoom Constraint: 0.5 to 5.0
            float clampedZoom = Math.Max(0.5f, Math.Min(5.0f, _zoom));
            float distance = Math.Max(15f, maxDimension * 2.5f) / clampedZoom;
            
            // Look at the center of the mesh
            Vector3 target = _meshCenter;
            Vector3 eye = new Vector3(_meshCenter.X, _meshCenter.Y, _meshCenter.Z + distance);
            
            modelview = Matrix4.LookAt(eye, target, Vector3.UnitY);
            
            // Unified Rotation: Apply to the entire scene (Mesh + Box + Grids)
            // Elevation clamped to [-89, 89] to prevent gimbal lock
            float clampedElevation = Math.Max(-89f, Math.Min(89f, _elevation));
            
            // Pivot around the mesh center
            modelview = Matrix4.CreateTranslation(-_meshCenter) *
                        Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotation)) *
                        Matrix4.CreateRotationX(MathHelper.DegreesToRadians(clampedElevation)) *
                        Matrix4.CreateTranslation(_meshCenter) *
                        modelview;
        }

        /// <summary>
        /// Sets the rotation mode: true = rotate mesh, false = rotate camera (legacy)
        /// </summary>
        public void SetRotationMode(bool rotateMesh)
        {
            _rotateMesh = rotateMesh;
            RefreshChart();
        }

        /// <summary>
        /// Resets the view to default orientation and zoom.
        /// </summary>
        public void ResetView()
        {
            _rotation = -45f;
            _elevation = 30f;
            _zoom = 1.2f;
            RefreshChart();
        }

        private void DrawBoundingBox()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);

            Vector3 min = _meshMinBounds;
            Vector3 max = _meshMaxBounds;

            // Draw Back-face Grids first (so they are behind the mesh)
            DrawBackGrids(min, max);

            // Draw Bounding Box Edges
            GL.Color4(0.7f, 0.7f, 0.7f, 0.5f);
            GL.LineWidth(1.0f);
            GL.Begin(PrimitiveType.Lines);

            // Bottom
            GL.Vertex3(min.X, min.Y, min.Z); GL.Vertex3(max.X, min.Y, min.Z);
            GL.Vertex3(max.X, min.Y, min.Z); GL.Vertex3(max.X, min.Y, max.Z);
            GL.Vertex3(max.X, min.Y, max.Z); GL.Vertex3(min.X, min.Y, max.Z);
            GL.Vertex3(min.X, min.Y, max.Z); GL.Vertex3(min.X, min.Y, min.Z);

            // Top
            GL.Vertex3(min.X, max.Y, min.Z); GL.Vertex3(max.X, max.Y, min.Z);
            GL.Vertex3(max.X, max.Y, min.Z); GL.Vertex3(max.X, max.Y, max.Z);
            GL.Vertex3(max.X, max.Y, max.Z); GL.Vertex3(min.X, max.Y, max.Z);
            GL.Vertex3(min.X, max.Y, max.Z); GL.Vertex3(min.X, max.Y, min.Z);

            // Vertical pillars
            GL.Vertex3(min.X, min.Y, min.Z); GL.Vertex3(min.X, max.Y, min.Z);
            GL.Vertex3(max.X, min.Y, min.Z); GL.Vertex3(max.X, max.Y, min.Z);
            GL.Vertex3(max.X, min.Y, max.Z); GL.Vertex3(max.X, max.Y, max.Z);
            GL.Vertex3(min.X, min.Y, max.Z); GL.Vertex3(min.X, max.Y, max.Z);

            GL.End();
        }

        private void DrawBackGrids(Vector3 min, Vector3 max)
        {
            GL.Color4(0.4f, 0.4f, 0.4f, 0.2f);
            GL.Begin(PrimitiveType.Lines);

            int xDivs = 10;
            int yDivs = 5;
            int zDivs = 10;

            // XZ Plane (Bottom)
            for (int i = 0; i <= xDivs; i++)
            {
                float x = min.X + (max.X - min.X) * i / xDivs;
                GL.Vertex3(x, min.Y, min.Z); GL.Vertex3(x, min.Y, max.Z);
            }
            for (int i = 0; i <= zDivs; i++)
            {
                float z = min.Z + (max.Z - min.Z) * i / zDivs;
                GL.Vertex3(min.X, min.Y, z); GL.Vertex3(max.X, min.Y, z);
            }

            // XY Plane (Back)
            for (int i = 0; i <= xDivs; i++)
            {
                float x = min.X + (max.X - min.X) * i / xDivs;
                GL.Vertex3(x, min.Y, max.Z); GL.Vertex3(x, max.Y, max.Z);
            }
            for (int i = 0; i <= yDivs; i++)
            {
                float y = min.Y + (max.Y - min.Y) * i / yDivs;
                GL.Vertex3(min.X, y, max.Z); GL.Vertex3(max.X, y, max.Z);
            }

            // YZ Plane (Side)
            for (int i = 0; i <= zDivs; i++)
            {
                float z = min.Z + (max.Z - min.Z) * i / zDivs;
                GL.Vertex3(min.X, min.Y, z); GL.Vertex3(min.X, max.Y, z);
            }
            for (int i = 0; i <= yDivs; i++)
            {
                float y = min.Y + (max.Y - min.Y) * i / yDivs;
                GL.Vertex3(min.X, y, min.Z); GL.Vertex3(min.X, y, max.Z);
            }

            GL.End();
        }

        private void DrawAxisLabels()
        {
            // Use GDI+ for text overlay (simpler than OpenGL text)
            if (_glControl == null || !_glControl.IsHandleCreated) return;
            
            // This method is called after SwapBuffers to draw axis labels using GDI+
            // The actual drawing happens in OnGLPaint after SwapBuffers
        }

        /// <summary>
        /// Draws axis labels using GDI+ - call this after SwapBuffers
        /// Labels are placed at fixed screen positions around the control edges
        /// </summary>
        /// <summary>
        /// Draws axis labels using GDI+ anchored to 3D projected points on the bounding box.
        /// </summary>
        private void RenderAxisLabels()
        {
            if (_glControl == null || _glControl.IsDisposed) return;
            
            try
            {
                using (Graphics g = _glControl.CreateGraphics())
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    using (Font labelFont = new Font("Segoe UI", 9, FontStyle.Bold))
                    using (Font valueFont = new Font("Segoe UI", 7))
                    using (SolidBrush textBrush = new SolidBrush(Color.LightGray))
                    {
                        Vector3 min = _meshMinBounds;
                        Vector3 max = _meshMaxBounds;

                        // Determine which edges are closest to the viewer to anchor labels
                        // We check the 4 bottom corners to find the "front" ones
                        Vector3[] corners = new Vector3[] {
                            new Vector3(min.X, min.Y, min.Z),
                            new Vector3(max.X, min.Y, min.Z),
                            new Vector3(max.X, min.Y, max.Z),
                            new Vector3(min.X, min.Y, max.Z)
                        };

                        Matrix4 proj, mv;
                        GetMatrices(out proj, out mv);
                        int bestCornerIdx = 0;
                        float maxDepth = float.MinValue;

                        for(int i=0; i<4; i++) {
                            Vector4 viewPos = Vector4.Transform(new Vector4(corners[i], 1.0f), mv);
                            // In OpenTK's default coordinate system after LookAt,
                            // more positive Z is closer to the camera.
                            // To find the corner NEAREST to the viewer, we look for the largest Z.
                            if(viewPos.Z > maxDepth) {
                                maxDepth = viewPos.Z;
                                bestCornerIdx = i;
                            }
                        }

                        // Anchor points based on the closest corner
                        Vector3 anchor = corners[bestCornerIdx];
                        Vector3 xDir = (bestCornerIdx == 0 || bestCornerIdx == 3) ? Vector3.UnitX : -Vector3.UnitX;
                        Vector3 yDir = (bestCornerIdx == 0 || bestCornerIdx == 1) ? Vector3.UnitZ : -Vector3.UnitZ;

                        // 1. X-Axis Labels (Throttle Position)
                        if (_xAxisValues != null && _xAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(6, _xAxisValues.Length);
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_xAxisValues.Length - 1)) / (labelCount - 1);
                                float t = (float)i / (labelCount - 1);
                                Vector3 pos = new Vector3(min.X + (max.X - min.X) * t, min.Y, anchor.Z);
                                PointF screenPos = ProjectToScreen(pos);
                                if (screenPos != PointF.Empty)
                                {
                                    string val = FormatAxisValue(_xAxisValues[idx], _xAxisName);
                                    g.DrawString(val, valueFont, textBrush, screenPos.X - 10, screenPos.Y + 5);
                                }
                            }
                            PointF titlePos = ProjectToScreen(new Vector3((min.X + max.X) / 2, min.Y, anchor.Z));
                            g.DrawString(_xAxisName ?? "Throttle position", labelFont, textBrush, titlePos.X - 40, titlePos.Y + 20);
                        }

                        // 2. Y-Axis Labels (Engine Speed)
                        if (_yAxisValues != null && _yAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(6, _yAxisValues.Length);
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_yAxisValues.Length - 1)) / (labelCount - 1);
                                float t = (float)i / (labelCount - 1);
                                Vector3 pos = new Vector3(anchor.X, min.Y, min.Z + (max.Z - min.Z) * t);
                                PointF screenPos = ProjectToScreen(pos);
                                if (screenPos != PointF.Empty)
                                {
                                    string val = FormatAxisValue(_yAxisValues[idx], _yAxisName);
                                    g.DrawString(val, valueFont, textBrush, screenPos.X - 35, screenPos.Y);
                                }
                            }
                            PointF titlePos = ProjectToScreen(new Vector3(anchor.X, min.Y, (min.Z + max.Z) / 2));
                            g.DrawString(_yAxisName ?? "Engine speed (rpm)", labelFont, textBrush, titlePos.X - 60, titlePos.Y + 15);
                        }

                        // 3. Z-Axis Labels (Requested IQ - Vertical)
                        int zLabelCount = 5;
                        for (int i = 0; i <= zLabelCount; i++)
                        {
                            float t = (float)i / zLabelCount;
                            float y = min.Y + (max.Y - min.Y) * t;
                            PointF screenPos = ProjectToScreen(new Vector3(anchor.X, y, anchor.Z));
                            if (screenPos != PointF.Empty)
                            {
                                // Calculate actual map value for height
                                float range = Math.Max(1, _meshMaxBounds.Y - _meshMinBounds.Y);
                                float normalizedY = (y - min.Y) / range;
                                float actualVal = _dataMinZ + (_dataMaxZ - _dataMinZ) * normalizedY;
                                
                                string val = FormatAxisValue((int)actualVal, _zAxisName);
                                g.DrawString(val, valueFont, textBrush, screenPos.X - 25, screenPos.Y - 5);
                            }
                        }
                        PointF zTitlePos = ProjectToScreen(new Vector3(anchor.X, max.Y + 0.5f, anchor.Z));
                        g.DrawString(_zAxisName ?? "Requested IQ (mg)", labelFont, textBrush, zTitlePos.X - 40, zTitlePos.Y - 20);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: RenderAxisLabels error: " + ex.Message);
            }
        }

        private string FormatAxisValue(int value, string axisName)
        {
            // The value passed here is the raw axis value from the map
            // We should format it directly as it appears in the table
            
            if (axisName != null && axisName.ToLower().Contains("rpm"))
            {
                if (value > 1000) return $"{value / 1000.0:F1}k";
                return value.ToString();
            }
            
            return value.ToString();
        }

        /// <summary>
        /// Projects a 3D point to 2D screen coordinates, accounting for unified rotation.
        /// </summary>
        private PointF ProjectToScreen(Vector3 pos)
        {
            if (_glControl == null || _glControl.Width == 0 || _glControl.Height == 0) return PointF.Empty;

            Matrix4 projection, modelview;
            GetMatrices(out projection, out modelview);

            // Transform to clip space using the same unified matrix as the renderer
            Vector4 clipSpace = Vector4.Transform(new Vector4(pos, 1.0f), modelview * projection);
            
            if (clipSpace.W <= 0) return PointF.Empty;
            
            float ndcX = clipSpace.X / clipSpace.W;
            float ndcY = clipSpace.Y / clipSpace.W;
            
            float screenX = (ndcX + 1.0f) * 0.5f * _glControl.Width;
            float screenY = (1.0f - ndcY) * 0.5f * _glControl.Height;
            
            return new PointF(screenX, screenY);
        }

        private void DrawVertexPoints()
        {
            // Highlight vertices with white points
            GL.PointSize(4.0f);
            GL.Color4(1.0f, 1.0f, 1.0f, 0.8f);
            
            // Draw all vertices as points using the existing VBO
            GL.DrawArrays(PrimitiveType.Points, 0, _vertexCount / 6 + 1); // Approximate vertex count from triangle indices
        }

        private void UpdateBuffers()
        {
            if (_mapContent == null) { Console.WriteLine("Chart3DComponent: UpdateBuffers aborted - _mapContent is null"); return; }
            if (_tableWidth <= 0) { Console.WriteLine("Chart3DComponent: UpdateBuffers aborted - _tableWidth <= 0"); return; }
            if (_glControl == null) { Console.WriteLine("Chart3DComponent: UpdateBuffers aborted - _glControl is null"); return; }
            if (!_glControl.IsHandleCreated) { Console.WriteLine("Chart3DComponent: UpdateBuffers aborted - Handle not created"); return; }

            try
            {
                _glControl.MakeCurrent();

            int rows = _mapContent.Length / (_isSixteenBit ? 2 : 1) / _tableWidth;
            int cols = _tableWidth;
            
            // Find min/max for auto-scaling
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            float[] values = new float[rows * cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float val = GetZValue(_mapContent, r, c);
                    values[r * cols + c] = val;
                    if (val < minZ) minZ = val;
                    if (val > maxZ) maxZ = val;
                }
            }

            float range = Math.Max(1, maxZ - minZ);
            _dataMinZ = minZ;
            _dataMaxZ = maxZ;
            float scaleX = 10.0f / Math.Max(1, cols);
            float scaleZ = 10.0f / Math.Max(1, rows);
            float scaleY = 6.0f / range; // Auto-scale height to fit 6 units

            int totalVertices = rows * cols;
            Vector3[] vertices = new Vector3[totalVertices];
            Vector4[] colors = new Vector4[totalVertices];

            // Calculate mesh bounds
            _meshMinBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            _meshMaxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < totalVertices; i++)
            {
                int r = i / cols;
                int c = i % cols;
                float val = values[i];
                
                // Align 0,0 with lower-left (standard table orientation)
                float xPos = (c - (cols - 1) / 2.0f) * scaleX;
                float zPos = (r - (rows - 1) / 2.0f) * scaleZ;
                
                // Standard height calculation: High value = High peak
                float yPos = (val - minZ) * scaleY;
                
                // Honor the IsUpsideDown flag from the map configuration
                if (_isUpsideDown)
                {
                    yPos = (maxZ - minZ) * scaleY - yPos;
                }
                
                vertices[i] = new Vector3(xPos, yPos, zPos);
                
                // Update bounds
                if (xPos < _meshMinBounds.X) _meshMinBounds.X = xPos;
                if (yPos < _meshMinBounds.Y) _meshMinBounds.Y = yPos;
                if (zPos < _meshMinBounds.Z) _meshMinBounds.Z = zPos;
                if (xPos > _meshMaxBounds.X) _meshMaxBounds.X = xPos;
                if (yPos > _meshMaxBounds.Y) _meshMaxBounds.Y = yPos;
                if (zPos > _meshMaxBounds.Z) _meshMaxBounds.Z = zPos;
                
                // Color should follow the visual height (Red = High, Blue = Low)
                colors[i] = GetColorForZ(val, minZ, maxZ, _isUpsideDown);
            }

            // Calculate mesh center for camera targeting
            _meshCenter = (_meshMinBounds + _meshMaxBounds) / 2f;

            int totalIndices = (rows - 1) * (cols - 1) * 6;
            uint[] indices = new uint[totalIndices];
            int iIdx = 0;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = 0; c < cols - 1; c++)
                {
                    uint topLeft = (uint)(r * cols + c);
                    uint bottomLeft = (uint)((r + 1) * cols + c);
                    uint topRight = (uint)(r * cols + (c + 1));
                    uint bottomRight = (uint)((r + 1) * cols + (c + 1));

                    indices[iIdx++] = topLeft;
                    indices[iIdx++] = bottomLeft;
                    indices[iIdx++] = topRight;

                    indices[iIdx++] = bottomLeft;
                    indices[iIdx++] = bottomRight;
                    indices[iIdx++] = topRight;
                }
            }

            _vertexCount = totalIndices;
            Console.WriteLine($"Chart3DComponent: Buffers Calculated - Vertices: {vertices.Length}, Indices: {indices.Length}");
            Console.WriteLine($"Chart3DComponent: Mesh bounds - Min: {_meshMinBounds}, Max: {_meshMaxBounds}, Center: {_meshCenter}");

            if (!_buffersInitialized)
            {
                GL.GenBuffers(1, out _vbo);
                GL.GenBuffers(1, out _cbo);
                GL.GenBuffers(1, out _ebo);
                GL.GenBuffers(1, out _wireframeEbo);
                _buffersInitialized = true;
            }

            // Note: Mesh rotation is now handled via shader uniform uModel in OnGLPaint
            // This is more efficient as it doesn't require rebuilding buffers

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * Vector4.SizeInBytes, colors, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                // Create wireframe indices (quad edges only, no diagonals)
                // Each quad has 4 edges: top, right, bottom, left
                uint[] wireframeIndices = new uint[(rows - 1) * (cols - 1) * 8]; // 8 vertices per quad (2 per edge)
                int wIdx = 0;
                for (int r = 0; r < rows - 1; r++)
                {
                    for (int c = 0; c < cols - 1; c++)
                    {
                        uint topLeft = (uint)(r * cols + c);
                        uint topRight = (uint)(r * cols + (c + 1));
                        uint bottomLeft = (uint)((r + 1) * cols + c);
                        uint bottomRight = (uint)((r + 1) * cols + (c + 1));

                        // Top edge
                        wireframeIndices[wIdx++] = topLeft;
                        wireframeIndices[wIdx++] = topRight;
                        
                        // Right edge
                        wireframeIndices[wIdx++] = topRight;
                        wireframeIndices[wIdx++] = bottomRight;
                        
                        // Bottom edge
                        wireframeIndices[wIdx++] = bottomLeft;
                        wireframeIndices[wIdx++] = bottomRight;
                        
                        // Left edge
                        wireframeIndices[wIdx++] = topLeft;
                        wireframeIndices[wIdx++] = bottomLeft;
                    }
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, wireframeIndices.Length * sizeof(uint), wireframeIndices, BufferUsageHint.StaticDraw);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: UpdateBuffers failed: " + ex.Message);
            }
        }

        private float GetZValue(byte[] content, int r, int c)
        {
            int index = (r * _tableWidth + c) * (_isSixteenBit ? 2 : 1);
            if (index + (_isSixteenBit ? 1 : 0) >= content.Length) return 0;

            if (_isSixteenBit)
            {
                // VAG EDC maps are Big-Endian (Motorola)
                // Explicitly assemble the 16-bit value from two bytes
                int high = (int)content[index] << 8;
                int low = (int)content[index + 1];
                return (float)(high | low);
            }
            else
            {
                return (float)content[index];
            }
        }

        /// <summary>
        /// Calculates a "tamed" 6-stop gradient color for a given Z value.
        /// Verified stops: Blue -> Cyan -> Green -> Yellow -> Orange -> Red
        /// </summary>
        private Vector4 GetColorForZ(float z, float min, float max, bool inverted)
        {
            float range = max - min;
            if (range <= 0) range = 1;
            float normalized = (z - min) / range;
            if (inverted) normalized = 1.0f - normalized;

            // Tamed 6-stop gradient (Verified in Phase 2)
            Color[] stops = new Color[] {
                ColorTranslator.FromHtml("#206C7C"), // Blue
                ColorTranslator.FromHtml("#2EA9A1"), // Cyan
                ColorTranslator.FromHtml("#91EABC"), // Green
                ColorTranslator.FromHtml("#FFF598"), // Yellow
                ColorTranslator.FromHtml("#F7B74A"), // Orange
                ColorTranslator.FromHtml("#FF4818")  // Red
            };

            float scaled = normalized * (stops.Length - 1);
            int lowerIdx = (int)Math.Floor(scaled);
            int upperIdx = (int)Math.Ceiling(scaled);
            float fraction = scaled - lowerIdx;

            lowerIdx = Math.Max(0, Math.Min(stops.Length - 1, lowerIdx));
            upperIdx = Math.Max(0, Math.Min(stops.Length - 1, upperIdx));

            Color c1 = stops[lowerIdx];
            Color c2 = stops[upperIdx];

            float r = (c1.R + (c2.R - c1.R) * fraction) / 255f;
            float g = (c1.G + (c2.G - c1.G) * fraction) / 255f;
            float b = (c1.B + (c2.B - c1.B) * fraction) / 255f;

            return new Vector4(r, g, b, 0.85f); // Slightly higher alpha for "tamed" look
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads data into the 3D chart
        /// </summary>
        public void LoadData(MapViewerState state)
        {
            Console.WriteLine($"Chart3DComponent: LoadData called for map: {state.Metadata.Name}");
            _tableWidth = state.Data.TableWidth;
            _isSixteenBit = state.Data.IsSixteenBit;
            _viewType = state.Configuration.ViewType;
            _mapName = state.Metadata.Name;
            _xAxisName = state.Metadata.XAxisName;
            _yAxisName = state.Metadata.YAxisName;
            _zAxisName = state.Metadata.ZAxisName;
            _mapContent = state.Data.Content;
            _originalContent = state.Data.OriginalContent;
            _compareContent = state.Data.CompareContent;
            _xAxisValues = state.Axes.XAxisValues;
            _yAxisValues = state.Axes.YAxisValues;
            _correctionFactor = state.Configuration.CorrectionFactor;
            _correctionOffset = state.Configuration.CorrectionOffset;
            _isCompareViewer = state.IsCompareMode;
            _onlineMode = state.IsOnlineMode;
            _overlayVisible = true;
            _isUpsideDown = state.Configuration.IsUpsideDown; // Load IsUpsideDown from state
            Console.WriteLine($"Chart3DComponent: LoadData - Map: {state.Metadata.Name}, UpsideDown: {_isUpsideDown}");

            ConfigureChart();
            UpdateBuffers();
        }

        /// <summary>
        /// Initializes the 3D chart with basic settings and series.
        /// </summary>
        public void InitializeChart3D()
        {
            if (_glControl == null)
            {
                Console.WriteLine("Chart3DComponent: InitializeChart3D aborted - _glControl is null");
                return;
            }

            if (!_glControl.IsHandleCreated)
            {
                Console.WriteLine("Chart3DComponent: InitializeChart3D aborted - Handle not created");
                return;
            }

            if (_glControl.Context == null)
            {
                Console.WriteLine("Chart3DComponent: InitializeChart3D aborted - GLContext is null");
                return;
            }

            Console.WriteLine("Chart3DComponent: Initializing OpenGL state...");
            try
            {
                _glControl.MakeCurrent();
                CheckGLError("Init:MakeCurrent");

                GL.ClearColor(Color.FromArgb(50, 50, 50));
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
                
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                // Disable fixed-function lighting as we use custom shaders
                GL.Disable(EnableCap.Lighting);
                
                CheckGLError("Init:State");

                _shaderProgram = CreateShaderProgram();
                
                Console.WriteLine("Chart3DComponent: OpenGL initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: Initialization failed: " + ex.Message);
            }
        }

        private int CreateShaderProgram()
        {
            // GLSL 1.20 for maximum compatibility
            string vertexShaderSource = @"
                #version 120
                attribute vec3 aPosition;
                attribute vec4 aColor;
                varying vec4 vColor;
                uniform mat4 uModelView;
                uniform mat4 uProjection;
                uniform mat4 uModel;  // Model matrix for mesh rotation
                void main() {
                    gl_Position = uProjection * uModelView * uModel * vec4(aPosition, 1.0);
                    vColor = aColor;
                }";

            string fragmentShaderSource = @"
                #version 120
                varying vec4 vColor;
                void main() {
                    gl_FragColor = vColor;
                }";

            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compilation failed ({type}): {log}");
            }
            return shader;
        }

        /// <summary>
        /// Refreshes the chart with current data
        /// </summary>
        public void RefreshChart()
        {
            if (_glControl != null)
            {
                _glControl.Invalidate();
            }
        }

        /// <summary>
        /// Sets the view rotation, elevation, and zoom
        /// </summary>
        public void SetView(float rotation, float elevation, float zoom)
        {
            _rotation = rotation;
            _elevation = elevation;
            _zoom = zoom;
            RefreshChart();
        }

        /// <summary>
        /// Gets the current view parameters
        /// </summary>
        public void GetView(out float rotation, out float elevation, out float zoom)
        {
            rotation = _rotation;
            elevation = _elevation;
            zoom = _zoom;
        }

        /// <summary>
        /// Toggles between Solid and Wireframe render modes.
        /// </summary>
        public void ToggleRenderMode()
        {
            _renderMode = (_renderMode == RenderMode.Solid) ? RenderMode.Wireframe : RenderMode.Solid;
            RefreshChart();
        }

        /// <summary>
        /// Shows or hides the overlay (original map comparison)
        /// </summary>
        public void SetOverlayVisible(bool visible)
        {
            _overlayVisible = visible;
            RefreshChart();
        }

        private void OnGLMouseDown(object sender, MouseEventArgs e)
        {
            _lastMousePos = e.Location;
        }

        private void OnGLMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Update rotation angles - this now rotates the view responsively
                // because rotation is applied in GetMatrices via modelview matrix
                float deltaX = (e.X - _lastMousePos.X) * 0.5f;
                float deltaY = (e.Y - _lastMousePos.Y) * 0.5f;
                
                _rotation += deltaX;
                _elevation += deltaY;
                
                // Clamp elevation to prevent gimbal lock issues
                _elevation = Math.Max(-89f, Math.Min(89f, _elevation));
                
                _lastMousePos = e.Location;
                
                // Use throttled rendering for responsive but controlled updates
                RequestRender();
            }
        }

        private void RequestRender()
        {
            if (_glControl == null) return;
            
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - _lastRenderTime;
            
            if (elapsed.TotalMilliseconds >= MIN_RENDER_INTERVAL_MS)
            {
                // Render immediately if enough time has passed
                _glControl.Invalidate();
                _lastRenderTime = now;
            }
            else
            {
                // Mark as pending render, timer will handle it
                _pendingRender = true;
                InitializeRenderTimer();
            }
        }

        private void OnGLMouseWheel(object sender, MouseEventArgs e)
        {
            _zoom += e.Delta > 0 ? 0.1f : -0.1f;
            // Strict Zoom Constraint
            _zoom = Math.Max(0.5f, Math.Min(5.0f, _zoom));
            RefreshChart();
        }

        private void OnGLKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                ToggleRenderMode();
            }
            else if (e.KeyCode == Keys.R)
            {
                ResetView();
            }
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            // Don't create a new chart control - use the external one if provided
            // This component acts as a wrapper around an existing chart control
        }

        private void ConfigureChart()
        {
            InitializeChart3D();
        }

        private string ConvertYAxisValue(string currValue)
        {
            if (_yAxisValues == null || _yAxisValues.Length == 0) return currValue;
            
            // Apply correction factor
            try
            {
                float temp = (float)Convert.ToDouble(currValue);
                temp *= (float)_correctionFactor;
                temp += (float)_correctionOffset;
                return temp.ToString("F1");
            }
            catch
            {
                return currValue;
            }
        }

        private string ConvertXAxisValue(string currValue)
        {
            if (_xAxisValues == null || _xAxisValues.Length == 0) return currValue;
            
            // Apply correction factor
            try
            {
                float temp = (float)Convert.ToDouble(currValue);
                temp *= (float)_correctionFactor;
                temp += (float)_correctionOffset;
                return temp.ToString("F2");
            }
            catch
            {
                return currValue;
            }
        }

        private void RaiseViewChanged()
        {
            // TODO: Implement view changed event for OpenTK
        }

        #endregion
    }
}
