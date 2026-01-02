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

        // State references
        private int _tableWidth;
        private bool _isSixteenBit;
        private bool _isLoaded = false;
        private float _rotation = -45f;
        private float _elevation = 45f;
        private float _zoom = 1.5f;
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
            
                // Draw Axes for orientation
                DrawAxes();

                if (_buffersInitialized && _vertexCount > 0 && _shaderProgram > 0)
                {
                    GL.UseProgram(_shaderProgram);

                    // Set Uniforms
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
                    
                    // Set model matrix for mesh rotation
                    Matrix4 model = Matrix4.Identity;
                    if (_rotateMesh)
                    {
                        model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_elevation));
                        model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotation));
                    }
                    GL.UniformMatrix4(modelLoc, false, ref model);

                    // Bind VBO
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                    GL.EnableVertexAttribArray(posLoc);
                    GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);

                    // Bind CBO
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _cbo);
                    GL.EnableVertexAttribArray(colLoc);
                    GL.VertexAttribPointer(colLoc, 4, VertexAttribPointerType.Float, false, 0, 0);

                    // Draw wireframe using quad edges only (no diagonals)
                    if (_wireframeEbo != 0)
                    {
                        GL.Disable(EnableCap.PolygonOffsetFill);
                        GL.Color4(0.0f, 0.0f, 0.0f, 0.8f); // Black wireframe
                        GL.LineWidth(1.5f);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                        GL.DrawElements(PrimitiveType.Lines, _vertexCount / 6 * 4, DrawElementsType.UnsignedInt, IntPtr.Zero); // 4 edges per quad
                    }
                    
                    // Draw filled triangles with transparency
                    GL.Enable(EnableCap.PolygonOffsetFill);
                    GL.PolygonOffset(1.0f, 1.0f);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                    GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    
                    // Reset polygon mode
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                    // Draw vertex points for highlighting
                    DrawVertexPoints();

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
            float distance = Math.Max(15f, maxDimension * 2.5f) / _zoom;
            
            // Look at the center of the mesh
            Vector3 target = _meshCenter;
            Vector3 eye = new Vector3(_meshCenter.X + distance, _meshCenter.Y + distance, _meshCenter.Z + distance);
            
            modelview = Matrix4.LookAt(eye, target, Vector3.UnitY);
            
            // Only apply rotation to camera if NOT rotating mesh
            // When rotating mesh, the rotation is applied to vertices in UpdateBuffers
            if (!_rotateMesh)
            {
                modelview *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_elevation));
                modelview *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotation));
            }
        }

        /// <summary>
        /// Sets the rotation mode: true = rotate mesh, false = rotate camera (legacy)
        /// </summary>
        public void SetRotationMode(bool rotateMesh)
        {
            _rotateMesh = rotateMesh;
            RefreshChart();
        }

        private void DrawAxes()
        {
            GL.Disable(EnableCap.Lighting); // Axes shouldn't be lit
            GL.Disable(EnableCap.DepthTest); // Draw axes on top
            
            // Calculate axis length based on mesh bounds
            float axisLength = Math.Max(_meshMaxBounds.X - _meshMinBounds.X,
                                        Math.Max(_meshMaxBounds.Y - _meshMinBounds.Y,
                                                 _meshMaxBounds.Z - _meshMinBounds.Z)) * 1.2f;
            
            GL.Begin(PrimitiveType.Lines);
            
            // X Axis - Red (horizontal, represents X axis values)
            GL.Color4(Color.Red.R, Color.Red.G, Color.Red.B, 0.8f);
            GL.Vertex3(_meshMinBounds.X, _meshMinBounds.Y, _meshMinBounds.Z);
            GL.Vertex3(_meshMaxBounds.X + axisLength * 0.1f, _meshMinBounds.Y, _meshMinBounds.Z);
            
            // Y Axis - Green (vertical, represents Z values in map)
            GL.Color4(Color.Green.R, Color.Green.G, Color.Green.B, 0.8f);
            GL.Vertex3(_meshMinBounds.X, _meshMinBounds.Y, _meshMinBounds.Z);
            GL.Vertex3(_meshMinBounds.X, _meshMaxBounds.Y + axisLength * 0.1f, _meshMinBounds.Z);
            
            // Z Axis - Blue (depth, represents Y axis values)
            GL.Color4(Color.Blue.R, Color.Blue.G, Color.Blue.B, 0.8f);
            GL.Vertex3(_meshMinBounds.X, _meshMinBounds.Y, _meshMinBounds.Z);
            GL.Vertex3(_meshMinBounds.X, _meshMinBounds.Y, _meshMaxBounds.Z + axisLength * 0.1f);
            
            GL.End();
            
            // Draw grid on the base plane (XZ plane at Y=min)
            DrawGrid(axisLength);
            
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
        }

        private void DrawGrid(float size)
        {
            GL.Color4(0.5f, 0.5f, 0.5f, 0.3f);
            GL.Begin(PrimitiveType.Lines);
            
            float step = size / 10f;
            float start = _meshMinBounds.X - size * 0.1f;
            float end = _meshMaxBounds.X + size * 0.1f;
            
            // Grid lines along X
            for (float z = _meshMinBounds.Z - size * 0.1f; z <= _meshMaxBounds.Z + size * 0.1f; z += step)
            {
                GL.Vertex3(start, _meshMinBounds.Y, z);
                GL.Vertex3(end, _meshMinBounds.Y, z);
            }
            
            // Grid lines along Z
            start = _meshMinBounds.Z - size * 0.1f;
            end = _meshMaxBounds.Z + size * 0.1f;
            for (float x = _meshMinBounds.X - size * 0.1f; x <= _meshMaxBounds.X + size * 0.1f; x += step)
            {
                GL.Vertex3(x, _meshMinBounds.Y, start);
                GL.Vertex3(x, _meshMinBounds.Y, end);
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
        private void RenderAxisLabels()
        {
            if (_glControl == null || _glControl.IsDisposed) return;
            
            try
            {
                using (Graphics g = _glControl.CreateGraphics())
                {
                    // Use a simple font for labels
                    using (Font labelFont = new Font("Arial", 9, FontStyle.Bold))
                    using (SolidBrush brushX = new SolidBrush(Color.Red))      // X-axis = Red
                    using (SolidBrush brushY = new SolidBrush(Color.Green))    // Y-axis = Green
                    using (SolidBrush brushZ = new SolidBrush(Color.Blue))     // Z-axis = Blue
                    {
                        float padding = 10f;
                        float bottomMargin = _glControl.Height - 25f;
                        float leftMargin = 15f;
                        
                        // Draw X-axis name at bottom-left
                        string xAxisLabel = !string.IsNullOrEmpty(_xAxisName) ? _xAxisName : "X Axis";
                        g.DrawString(xAxisLabel, labelFont, brushX, leftMargin, bottomMargin);
                        
                        // Draw X-axis values along bottom edge
                        if (_xAxisValues != null && _xAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(5, _xAxisValues.Length);
                            float labelSpacing = (_glControl.Width - leftMargin * 2) / (labelCount - 1);
                            
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_xAxisValues.Length - 1)) / (labelCount - 1);
                                string label = FormatAxisValue(_xAxisValues[idx], _xAxisName);
                                float xPos = leftMargin + i * labelSpacing;
                                g.DrawString(label, new Font("Arial", 7), brushX, xPos - 10, bottomMargin + 15);
                            }
                        }
                        
                        // Draw Z-axis name (Y in map terms) at top-left
                        string yAxisLabel = !string.IsNullOrEmpty(_yAxisName) ? _yAxisName : "Y Axis";
                        g.DrawString(yAxisLabel, labelFont, brushZ, leftMargin, padding);
                        
                        // Draw Z-axis values along left edge
                        if (_yAxisValues != null && _yAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(5, _yAxisValues.Length);
                            float labelSpacing = (bottomMargin - padding * 2) / (labelCount - 1);
                            
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_yAxisValues.Length - 1)) / (labelCount - 1);
                                string label = FormatAxisValue(_yAxisValues[idx], _yAxisName);
                                float yPos = bottomMargin - i * labelSpacing;
                                g.DrawString(label, new Font("Arial", 7), brushZ, leftMargin, yPos - 5);
                            }
                        }
                        
                        // Draw Z-axis (value/height) name at top-right
                        string zAxisLabel = !string.IsNullOrEmpty(_zAxisName) ? _zAxisName : "Value";
                        SizeF textSize = g.MeasureString(zAxisLabel, labelFont);
                        g.DrawString(zAxisLabel, labelFont, brushY, _glControl.Width - textSize.Width - padding, padding);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: DrawAxisLabels error: " + ex.Message);
            }
        }

        private string FormatAxisValue(int value, string axisName)
        {
            // Apply correction factor if available
            double corrected = value * _correctionFactor + _correctionOffset;
            
            // Format based on axis type
            if (axisName != null && axisName.ToLower().Contains("rpm"))
            {
                return $"{corrected / 1000:F0}k";
            }
            else if (axisName != null && axisName.ToLower().Contains("pressure") ||
                     axisName != null && axisName.ToLower().Contains("boost"))
            {
                return $"{corrected:F1}";
            }
            else
            {
                return $"{corrected:F0}";
            }
        }

        private PointF ProjectToScreen(Vector3 pos)
        {
            // Simple projection to screen coordinates
            float aspectRatio = (float)_glControl.Width / Math.Max(1, _glControl.Height);
            
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), aspectRatio, 0.1f, 1000f);
            float maxDimension = Math.Max(_meshMaxBounds.X - _meshMinBounds.X,
                                          Math.Max(_meshMaxBounds.Y - _meshMinBounds.Y,
                                                   _meshMaxBounds.Z - _meshMinBounds.Z));
            float distance = Math.Max(15f, maxDimension * 2.5f) / _zoom;
            Vector3 eye = new Vector3(_meshCenter.X + distance, _meshCenter.Y + distance, _meshCenter.Z + distance);
            Matrix4 modelview = Matrix4.LookAt(eye, _meshCenter, Vector3.UnitY);
            modelview *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_elevation));
            modelview *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotation));

            // Transform to clip space
            Vector4 transformed = Vector4.Transform(new Vector4(pos, 1.0f), projection * modelview);
            
            if (transformed.W <= 0) return new PointF(0, 0); // Behind camera
            
            // Convert to screen coordinates
            float ndcX = transformed.X / transformed.W;
            float ndcY = transformed.Y / transformed.W;
            
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
                
                float xPos = (c - (cols - 1) / 2.0f) * scaleX;
                float zPos = (r - (rows - 1) / 2.0f) * scaleZ;
                
                // Standard height calculation: High value = High peak
                float yPos = (val - minZ) * scaleY;
                
                // Only flip if explicitly requested AND it's not a standard map
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

        private Vector4 GetColorForZ(float z, float min, float max, bool inverted)
        {
            float range = max - min;
            if (range <= 0) range = 1;
            float normalized = (z - min) / range;
            
            // The heatmap should always represent the visual height
            // If the mesh is inverted, the color mapping must also invert to keep Red at the peaks
            if (inverted) normalized = 1.0f - normalized;

            // Heatmap: Blue (0.0) -> Green (0.5) -> Red (1.0)
            float r = 0, g = 0, b = 0;
            if (normalized < 0.5f)
            {
                float t = normalized * 2.0f; // 0 to 1
                b = 1.0f - t;
                g = t;
            }
            else
            {
                float t = (normalized - 0.5f) * 2.0f; // 0 to 1
                g = 1.0f - t;
                r = t;
            }
            // Semi-transparent color (alpha = 0.6f) for better visualization of 3D structure
            return new Vector4(r, g, b, 0.6f);
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
            _zoom = Math.Max(0.1f, Math.Min(10f, _zoom));
            RefreshChart();
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
