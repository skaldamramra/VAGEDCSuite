using System;
using System.Collections.Generic;
using System.Threading;
using VAGSuite.MapViewerEventArgs;
using System.Data;
using System.Linq;
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
        private readonly IDataConversionService _dataConversionService;
        
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
        // Default view orientation: slightly isometric view (~25° elevation from top)
        // Rotation: Z-axis (horizontal spin), Elevation: X-axis limited to ±60°
        private float _rotation = 0f;
        private float _elevation = 10f; // Slightly isometric default view
        private float _zoom = 1.2f;
        private RenderMode _renderMode = RenderMode.Solid;
        private ViewType _viewType;
        private string _mapName;
        private string _xAxisName;
        private string _yAxisName;
        private string _zAxisName;
        private string _xAxisUnits;
        private string _yAxisUnits;
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
        private Point _currentMousePos;
        private int _hoveredVertexIndex = -1;
        private bool _showTooltips = true;

        // Rotation mode: true = rotate mesh, false = rotate camera
        private bool _rotateMesh = true;

        // Render synchronization - prevents overlapping renders
        private int _renderLock = 0;
        private DateTime _lastRenderTime;
        private const int MIN_RENDER_INTERVAL_MS = 16; // ~60 FPS max
        
        // Hover tooltip optimization - cached screen position to avoid recalculation
        private PointF _cachedHoverScreenPos = PointF.Empty;
        private bool _hoverTooltipDirty = false;

        #endregion

        #region Events

        public event EventHandler<SurfaceGraphViewChangedEventArgsEx> ViewChanged;
        public event EventHandler RefreshRequested;

        #endregion

        #region Constructors

        public Chart3DComponent()
        {
            _chartService = new ChartService();
            _dataConversionService = new DataConversionService();
        }

        public Chart3DComponent(IChartService chartService, IDataConversionService dataConversionService)
        {
            _chartService = chartService ?? throw new ArgumentNullException("chartService");
            _dataConversionService = dataConversionService ?? throw new ArgumentNullException("dataConversionService");
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

        /// <summary>
        /// Attempts to acquire the render lock. Returns true if lock was acquired.
        /// This prevents overlapping renders which cause flickering.
        /// </summary>
        private bool TryAcquireRenderLock()
        {
            return Interlocked.CompareExchange(ref _renderLock, 1, 0) == 0;
        }

        /// <summary>
        /// Releases the render lock acquired by TryAcquireRenderLock.
        /// </summary>
        private void ReleaseRenderLock()
        {
            Interlocked.Exchange(ref _renderLock, 0);
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

            // Acquire render lock to prevent overlapping renders
            if (!TryAcquireRenderLock())
            {
                // Another render is in progress, skip this one
                return;
            }

            try
            {
                _glControl.MakeCurrent();
                CheckGLError("MakeCurrent");

                GL.ClearColor(Color.FromArgb(50, 50, 50));
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                CheckGLError("Clear");
            
                SetupViewport();
            
                Matrix4 projection;
                Matrix4 modelView;
                GetMatrices(out projection, out modelView);

                // Draw Bounding Box and Grids
                DrawBoundingBox(modelView);

                if (_buffersInitialized && _vertexCount > 0 && _shaderProgram > 0)
                {
                    GL.UseProgram(_shaderProgram);

                    int modelViewLoc = GL.GetUniformLocation(_shaderProgram, "uModelView");
                    int projectionLoc = GL.GetUniformLocation(_shaderProgram, "uProjection");
                    int modelLoc = GL.GetUniformLocation(_shaderProgram, "uModel");
                    int posLoc = GL.GetAttribLocation(_shaderProgram, "aPosition");
                    int colLoc = GL.GetAttribLocation(_shaderProgram, "aColor");

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
                            GL.LineWidth(1.5f); // Increased for better visibility
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                            GL.DrawElements(PrimitiveType.Lines, (int)(_vertexCount * 1.33f), DrawElementsType.UnsignedInt, IntPtr.Zero);
                        }
                    }
                    else
                    {
                        // Solid Mode: Draw filled triangles + high-contrast wireframe overlay
                        GL.Enable(EnableCap.PolygonOffsetFill);
                        GL.PolygonOffset(1.0f, 1.0f);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                        if (_wireframeEbo != 0)
                        {
                            GL.Disable(EnableCap.PolygonOffsetFill);
                            GL.LineWidth(1.0f); // Increased from 0.5f
                            
                            // Use a fixed dark color for better contrast against the colored mesh
                            int wireframeColLoc = GL.GetAttribLocation(_shaderProgram, "aColor");
                            GL.DisableVertexAttribArray(wireframeColLoc);
                            GL.VertexAttrib4(wireframeColLoc, 0.1f, 0.1f, 0.1f, 0.6f); // Dark gray/black edges
                            
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _wireframeEbo);
                            GL.DrawElements(PrimitiveType.Lines, (int)(_vertexCount * 1.33f), DrawElementsType.UnsignedInt, IntPtr.Zero);
                            
                            GL.EnableVertexAttribArray(wireframeColLoc); // Restore for other drawing
                        }
                    }

                    GL.DisableVertexAttribArray(posLoc);
                    GL.DisableVertexAttribArray(colLoc);
                    GL.UseProgram(0);
                }
                
                // Update hover state and cache screen position for tooltip
                bool hoverChanged = UpdateHoverState();
                
                // Swap buffers first to show the 3D render
                _glControl.SwapBuffers();
                
                // Now draw GDI+ overlays on top of the rendered frame
                // Using CreateGraphics() after SwapBuffers is safe because we're drawing on the front buffer
                RenderAxisLabels();
                
                // Only render hover tooltip if enabled and needed
                if (_showTooltips && (_hoveredVertexIndex >= 0 || _hoverTooltipDirty))
                {
                    RenderHoverTooltip();
                    _hoverTooltipDirty = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: Paint error: " + ex.Message);
            }
            finally
            {
                ReleaseRenderLock();
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
            
            // For isometric view: position camera at an angle from above
            // Camera positioned along -Y axis, elevated based on default view
            float elevationRad = MathHelper.DegreesToRadians(_elevation);
            Vector3 eye = new Vector3(
                _meshCenter.X,
                _meshCenter.Y - distance * (float)Math.Cos(elevationRad),
                _meshCenter.Z + distance * (float)Math.Sin(elevationRad)
            );
            
            // Use Z-axis as "up" since Z is height (X/Y plane is horizontal)
            modelview = Matrix4.LookAt(eye, target, Vector3.UnitZ);
            
            // Unified Rotation: Apply to the entire scene (Mesh + Box + Grids)
            // Z-axis rotation: horizontal spin around the mesh
            // X-axis rotation: limited elevation (±60°) for viewing angle
            
            // Clamp elevation to prevent extreme flipping
            float clampedElevation = Math.Max(-30f, Math.Min(30f, _elevation));
            
            // Pivot around the mesh center - Z rotation first, then X for elevation
            modelview = Matrix4.CreateTranslation(-_meshCenter) *
                        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_rotation)) *
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
        /// Resets the view to default orientation: slightly isometric (~25° elevation).
        /// Z-axis rotation: 360° horizontal spin.
        /// X-axis elevation: limited to ±60° to prevent extreme flipping.
        /// </summary>
        public void ResetView()
        {
            _rotation = 0f;
            _elevation = 25f; // Slightly isometric view
            _zoom = 1.2f;
            RefreshChart();
        }

        private void DrawBoundingBox(Matrix4 modelView)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);

            Vector3 min = _meshMinBounds;
            Vector3 max = _meshMaxBounds;

            // Draw Back-face Grids first (so they are behind the mesh)
            DrawBackGrids(min, max, modelView);

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

        private void DrawBackGrids(Vector3 min, Vector3 max, Matrix4 modelView)
        {
            GL.Color4(0.4f, 0.4f, 0.4f, 0.2f);
            
            int xDivs = 10;
            int yDivs = 5;
            int zDivs = 10;

            // Define the 6 possible planes of the bounding box
            // Each plane is defined by its constant axis and value, and its center point for depth testing
            var planes = new[]
            {
                new { Axis = "Y", Val = min.Y, Center = new Vector3((min.X+max.X)/2, min.Y, (min.Z+max.Z)/2), Name = "Bottom" },
                new { Axis = "Y", Val = max.Y, Center = new Vector3((min.X+max.X)/2, max.Y, (min.Z+max.Z)/2), Name = "Top" },
                new { Axis = "Z", Val = min.Z, Center = new Vector3((min.X+max.X)/2, (min.Y+max.Y)/2, min.Z), Name = "Front" },
                new { Axis = "Z", Val = max.Z, Center = new Vector3((min.X+max.X)/2, (min.Y+max.Y)/2, max.Z), Name = "Back" },
                new { Axis = "X", Val = min.X, Center = new Vector3(min.X, (min.Y+max.Y)/2, (min.Z+max.Z)/2), Name = "Left" },
                new { Axis = "X", Val = max.X, Center = new Vector3(max.X, (min.Y+max.Y)/2, (min.Z+max.Z)/2), Name = "Right" }
            };

            // Calculate view-space Z for each plane center.
            // Smaller Z = farther from camera in OpenTK's default LookAt.
            var planeDepths = new List<Tuple<string, float, float>>();
            foreach (var p in planes)
            {
                float depth = Vector4.Transform(new Vector4(p.Center, 1.0f), modelView).Z;
                planeDepths.Add(new Tuple<string, float, float>(p.Axis, p.Val, depth));
            }

            var sortedPlanes = planeDepths.OrderBy(p => p.Item3).Take(3).ToList();

            GL.Begin(PrimitiveType.Lines);
            foreach (var plane in sortedPlanes)
            {
                string axis = plane.Item1;
                float val = plane.Item2;

                if (axis == "Y") // XZ Plane
                {
                    for (int i = 0; i <= xDivs; i++)
                    {
                        float x = min.X + (max.X - min.X) * i / xDivs;
                        GL.Vertex3(x, val, min.Z); GL.Vertex3(x, val, max.Z);
                    }
                    for (int i = 0; i <= zDivs; i++)
                    {
                        float z = min.Z + (max.Z - min.Z) * i / zDivs;
                        GL.Vertex3(min.X, val, z); GL.Vertex3(max.X, val, z);
                    }
                }
                else if (axis == "Z") // XY Plane
                {
                    for (int i = 0; i <= xDivs; i++)
                    {
                        float x = min.X + (max.X - min.X) * i / xDivs;
                        GL.Vertex3(x, min.Y, val); GL.Vertex3(x, max.Y, val);
                    }
                    for (int i = 0; i <= yDivs; i++)
                    {
                        float y = min.Y + (max.Y - min.Y) * i / yDivs;
                        GL.Vertex3(min.X, y, val); GL.Vertex3(max.X, y, val);
                    }
                }
                else if (axis == "X") // YZ Plane
                {
                    for (int i = 0; i <= zDivs; i++)
                    {
                        float z = min.Z + (max.Z - min.Z) * i / zDivs;
                        GL.Vertex3(val, min.Y, z); GL.Vertex3(val, max.Y, z);
                    }
                    for (int i = 0; i <= yDivs; i++)
                    {
                        float y = min.Y + (max.Y - min.Y) * i / yDivs;
                        GL.Vertex3(val, y, min.Z); GL.Vertex3(val, y, max.Z);
                    }
                }
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
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    using (Font labelFont = new Font("Segoe UI", 10, FontStyle.Bold)) // Slightly larger
                    using (Font valueFont = new Font("Segoe UI", 8, FontStyle.Bold)) // Larger and Bold
                    using (SolidBrush textBrush = new SolidBrush(Color.White)) // High contrast White
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
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

                        // 1. X-Axis Labels (Throttle Position - columns, left-right)
                        if (_xAxisValues != null && _xAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(6, _xAxisValues.Length);
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_xAxisValues.Length - 1)) / (labelCount - 1);
                                float t = (float)i / (labelCount - 1);
                                Vector3 pos = new Vector3(min.X + (max.X - min.X) * t, anchor.Y, min.Z);
                                PointF screenPos = ProjectToScreen(pos);
                                if (screenPos != PointF.Empty)
                                {
                                    string val = FormatAxisValue(_xAxisValues[idx], _xAxisName);
                                    // Draw shadow for better visibility
                                    g.DrawString(val, valueFont, shadowBrush, screenPos.X - 9, screenPos.Y + 6);
                                    g.DrawString(val, valueFont, textBrush, screenPos.X - 10, screenPos.Y + 5);
                                }
                            }
                            PointF titlePos = ProjectToScreen(new Vector3((min.X + max.X) / 2, anchor.Y, min.Z));
                            g.DrawString(_xAxisName ?? "Throttle position", labelFont, shadowBrush, titlePos.X - 39, titlePos.Y + 21);
                            g.DrawString(_xAxisName ?? "Throttle position", labelFont, textBrush, titlePos.X - 40, titlePos.Y + 20);
                        }

                        // 2. Y-Axis Labels (Engine Speed - rows, near-far)
                        if (_yAxisValues != null && _yAxisValues.Length > 0)
                        {
                            int labelCount = Math.Min(6, _yAxisValues.Length);
                            for (int i = 0; i < labelCount; i++)
                            {
                                int idx = (i * (_yAxisValues.Length - 1)) / (labelCount - 1);
                                float t = (float)i / (labelCount - 1);
                                Vector3 pos = new Vector3(anchor.X, min.Y + (max.Y - min.Y) * t, min.Z);
                                PointF screenPos = ProjectToScreen(pos);
                                if (screenPos != PointF.Empty)
                                {
                                    string val = FormatAxisValue(_yAxisValues[idx], _yAxisName);
                                    g.DrawString(val, valueFont, shadowBrush, screenPos.X - 34, screenPos.Y + 1);
                                    g.DrawString(val, valueFont, textBrush, screenPos.X - 35, screenPos.Y);
                                }
                            }
                            PointF titlePos = ProjectToScreen(new Vector3(anchor.X, (min.Y + max.Y) / 2, min.Z));
                            g.DrawString(_yAxisName ?? "Engine speed (rpm)", labelFont, shadowBrush, titlePos.X - 59, titlePos.Y + 16);
                            g.DrawString(_yAxisName ?? "Engine speed (rpm)", labelFont, textBrush, titlePos.X - 60, titlePos.Y + 15);
                        }

                        // 3. Z-Axis Labels (Requested IQ - height, vertical)
                        int zLabelCount = 5;
                        for (int i = 0; i <= zLabelCount; i++)
                        {
                            float t = (float)i / zLabelCount;
                            float z = min.Z + (max.Z - min.Z) * t;
                            PointF screenPos = ProjectToScreen(new Vector3(anchor.X, anchor.Y, z));
                            if (screenPos != PointF.Empty)
                            {
                                // Calculate actual map value for height
                                float range = Math.Max(1, _meshMaxBounds.Z - _meshMinBounds.Z);
                                float normalizedZ = (z - min.Z) / range;
                                float actualVal = _dataMinZ + (_dataMaxZ - _dataMinZ) * normalizedZ;
                                
                                // Use FormatZAxisValue to format based on current view type
                                string val = FormatZAxisValue(actualVal);
                                g.DrawString(val, valueFont, shadowBrush, screenPos.X - 24, screenPos.Y - 4);
                                g.DrawString(val, valueFont, textBrush, screenPos.X - 25, screenPos.Y - 5);
                            }
                        }
                        PointF zTitlePos = ProjectToScreen(new Vector3(anchor.X, anchor.Y, max.Z + 0.5f));
                        g.DrawString(_zAxisName ?? "Requested IQ (mg)", labelFont, shadowBrush, zTitlePos.X - 39, zTitlePos.Y - 19);
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
            // Use IDataConversionService to format values based on the current view type
            // This ensures consistency with the grid display
            
            if (axisName != null && axisName.ToLower().Contains("rpm"))
            {
                // Only apply "k" suffix for decimal/easy views, not hex
                if (_viewType == ViewType.Decimal || _viewType == ViewType.Easy)
                {
                    if (value > 1000) return $"{value / 1000.0:F1}k";
                }
                return _dataConversionService.FormatValue(value, _viewType, false);
            }
            
            return _dataConversionService.FormatValue(value, _viewType, false);
        }

        /// <summary>
        /// Formats a Z-axis value (map cell value) based on the current view type.
        /// For Easy view, applies correction factor and offset.
        /// </summary>
        private string FormatZAxisValue(double rawValue)
        {
            switch (_viewType)
            {
                case ViewType.Hexadecimal:
                    // Hexadecimal view: show as hex string
                    int intVal = (int)Math.Round(rawValue);
                    return _dataConversionService.FormatValue(intVal, _viewType, _isSixteenBit);
                    
                case ViewType.Decimal:
                    // Decimal view: show raw decimal value
                    return rawValue.ToString("F0");
                    
                case ViewType.Easy:
                    // Easy view: apply correction factor and offset
                    double corrected = _dataConversionService.ApplyCorrection((int)Math.Round(rawValue), _correctionFactor, _correctionOffset);
                    return corrected.ToString("F2");
                    
                case ViewType.ASCII:
                    // ASCII view: show as character if printable
                    int asciiVal = (int)Math.Round(rawValue);
                    if (asciiVal >= 32 && asciiVal < 127)
                        return ((char)asciiVal).ToString();
                    return ".";
                    
                default:
                    return rawValue.ToString("F0");
            }
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

        private bool UpdateHoverState()
        {
            if (_glControl == null || _mapContent == null || _tableWidth <= 0) return false;

            int rows = _mapContent.Length / (_isSixteenBit ? 2 : 1) / _tableWidth;
            int cols = _tableWidth;
            int totalVertices = rows * cols;

            float minDistance = 15.0f; // Hover threshold in pixels
            int bestIdx = -1;

            // We need to project vertices to find the one under the mouse
            // This is done by iterating through the grid.
            // Since we know the grid structure, we could optimize, but for typical map sizes (e.g. 16x16)
            // iterating all vertices is very fast.
            
            float scaleX = 10.0f / Math.Max(1, cols);
            float scaleY = 10.0f / Math.Max(1, rows);
            float range = Math.Max(1, _dataMaxZ - _dataMinZ);
            float scaleZ = 6.0f / range;

            for (int i = 0; i < totalVertices; i++)
            {
                int r = i / cols;
                int c = i % cols;
                float val = GetZValue(_mapContent, r, c);
                
                float xPos = (c - (cols - 1) / 2.0f) * scaleX;
                float yPos = (r - (rows - 1) / 2.0f) * scaleY;
                float zPos = (val - _dataMinZ) * scaleZ;
                if (_isUpsideDown) zPos = (range * scaleZ) - zPos;

                PointF screenPos = ProjectToScreen(new Vector3(xPos, yPos, zPos));
                if (screenPos != PointF.Empty)
                {
                    float dx = screenPos.X - _currentMousePos.X;
                    float dy = screenPos.Y - _currentMousePos.Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestIdx = i;
                    }
                }
            }

            bool changed = _hoveredVertexIndex != bestIdx;
            if (changed)
            {
                _hoveredVertexIndex = bestIdx;
                
                // Cache the screen position for tooltip rendering
                if (bestIdx >= 0)
                {
                    int r = bestIdx / cols;
                    int c = bestIdx % cols;
                    float val = GetZValue(_mapContent, r, c);
                    float xPos = (c - (cols - 1) / 2.0f) * scaleX;
                    float yPos = (r - (rows - 1) / 2.0f) * scaleY;
                    float zPos = (val - _dataMinZ) * scaleZ;
                    if (_isUpsideDown) zPos = (range * scaleZ) - zPos;
                    _cachedHoverScreenPos = ProjectToScreen(new Vector3(xPos, yPos, zPos));
                }
                else
                {
                    _cachedHoverScreenPos = PointF.Empty;
                }
                
                // Mark tooltip as dirty so it will be rendered in the current paint cycle
                _hoverTooltipDirty = true;
            }
            
            return changed;
        }

        private void RenderHoverTooltip()
        {
            if (_hoveredVertexIndex == -1 || _glControl == null || _mapContent == null) return;

            int cols = _tableWidth;
            int r = _hoveredVertexIndex / cols;
            int c = _hoveredVertexIndex % cols;

            float val = GetZValue(_mapContent, r, c);
            int xVal = (_xAxisValues != null && c < _xAxisValues.Length) ? _xAxisValues[c] : c;
            int yVal = (_yAxisValues != null && r < _yAxisValues.Length) ? _yAxisValues[r] : r;

            string xStr = FormatAxisValue(xVal, _xAxisName);
            string yStr = FormatAxisValue(yVal, _yAxisName);
            string zStr = FormatZAxisValue(val);

            string xLabel = _xAxisName ?? "X";
            string yLabel = _yAxisName ?? "Y";
            string zLabel = _zAxisName ?? "Z";

            if (!string.IsNullOrEmpty(_xAxisUnits)) xStr += " " + _xAxisUnits;
            if (!string.IsNullOrEmpty(_yAxisUnits)) yStr += " " + _yAxisUnits;

            string tooltip = $"{xLabel}: {xStr}\n{yLabel}: {yStr}\n{zLabel}: {zStr}";

            using (Graphics g = _glControl.CreateGraphics())
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    SizeF size = g.MeasureString(tooltip, font);
                    float padding = 5;
                    
                    // Use cached screen position if available, otherwise calculate
                    PointF tooltipPos = _cachedHoverScreenPos != PointF.Empty
                        ? new PointF(_cachedHoverScreenPos.X + 15, _cachedHoverScreenPos.Y - size.Height - 15)
                        : new PointF(_currentMousePos.X + 15, _currentMousePos.Y - size.Height - 15);
                    
                    RectangleF rect = new RectangleF(tooltipPos.X, tooltipPos.Y, size.Width + padding * 2, size.Height + padding * 2);

                    // Draw background
                    using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(220, 40, 40, 40)))
                    using (Pen borderPen = new Pen(Color.White, 1))
                    {
                        g.FillRectangle(backBrush, rect);
                        g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                    }

                    // Draw text
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString(tooltip, font, textBrush, rect.X + padding, rect.Y + padding);
                    }

                    // Draw a small circle at the vertex using cached position
                    if (_cachedHoverScreenPos != PointF.Empty)
                    {
                        g.DrawEllipse(Pens.White, _cachedHoverScreenPos.X - 4, _cachedHoverScreenPos.Y - 4, 8, 8);
                        g.FillEllipse(Brushes.Black, _cachedHoverScreenPos.X - 2, _cachedHoverScreenPos.Y - 2, 4, 4);
                    }
                }
            }
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
            float scaleX = 10.0f / Math.Max(1, cols);  // X-axis: columns (left-right)
            float scaleY = 10.0f / Math.Max(1, rows);  // Y-axis: rows (bottom-top/near-far)
            float scaleZ = 6.0f / range;               // Z-axis: values (height)

            int totalVertices = rows * cols;
            Vector3[] vertices = new Vector3[totalVertices];
            Vector4[] colors = new Vector4[totalVertices];

            // Calculate mesh bounds
            _meshMinBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            _meshMaxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < totalVertices; i++)
            {
                int r = i / cols;  // Row index (corresponds to Y-axis in table)
                int c = i % cols;  // Column index (corresponds to X-axis in table)
                float val = values[i];
                
                // Map table coordinates to 3D space to match table orientation:
                // - Table columns (X-axis values) → 3D X-axis (left-right)
                // - Table rows (Y-axis values) → 3D Y-axis (bottom-top/near-far)
                // - Table cell values (Z-axis values) → 3D Z-axis (height)
                
                float xPos = (c - (cols - 1) / 2.0f) * scaleX;  // Columns → X (left-right)
                float yPos = (r - (rows - 1) / 2.0f) * scaleY;  // Rows → Y (near-far)
                
                // Standard height calculation: High value = High peak
                float zPos = (val - minZ) * scaleZ;
                
                // Honor the IsUpsideDown flag from the map configuration
                if (_isUpsideDown)
                {
                    zPos = (maxZ - minZ) * scaleZ - zPos;
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
            _xAxisUnits = state.Metadata.XAxisUnits;
            _yAxisUnits = state.Metadata.YAxisUnits;
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
        /// Sets the view rotation, elevation, and zoom.
        /// Z-axis rotation: 360° horizontal spin.
        /// X-axis elevation: limited to ±60° to prevent extreme flipping.
        /// </summary>
        public void SetView(float rotation, float elevation, float zoom)
        {
            // Clamp rotation to 360° range (0-360 degrees)
            _rotation = rotation % 360f;
            if (_rotation < 0) _rotation += 360f;
            
            // Clamp elevation to ±60° to prevent extreme flipping
            _elevation = Math.Max(-60f, Math.Min(60f, elevation));
            
            // Zoom constraints remain unchanged
            _zoom = Math.Max(0.5f, Math.Min(5.0f, zoom));
            
            RefreshChart();
        }

        /// <summary>
        /// Gets the current view parameters.
        /// Z-axis rotation: 360° horizontal spin.
        /// X-axis elevation: limited to ±60°.
        /// </summary>
        public void GetView(out float rotation, out float elevation, out float zoom)
        {
            rotation = _rotation;
            elevation = _elevation; // Returns actual elevation (clamped to ±60°)
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

        /// <summary>
        /// Toggles the visibility of hover tooltips.
        /// </summary>
        public void ToggleTooltips()
        {
            _showTooltips = !_showTooltips;
            RefreshChart();
        }

        /// <summary>
        /// Toggles the visibility of hover tooltips.
        /// </summary>

        private void OnGLMouseDown(object sender, MouseEventArgs e)
        {
            _glControl.Focus(); // Ensure control has focus to receive key events
            _lastMousePos = e.Location;
        }

        private void OnGLMouseMove(object sender, MouseEventArgs e)
        {
            _currentMousePos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                // Update rotation angles - this now rotates the view responsively
                float deltaX = (e.X - _lastMousePos.X) * 0.5f;
                float deltaY = (e.Y - _lastMousePos.Y) * 0.5f;
                
                _rotation += deltaX;
                _rotation = _rotation % 360f;
                if (_rotation < 0) _rotation += 360f;
                
                _elevation += deltaY;
                _elevation = Math.Max(-60f, Math.Min(60f, _elevation));
                
                _lastMousePos = e.Location;
                RequestRender();
            }
            else
            {
                // Smart Hover Refresh:
                // Only check for hover changes if tooltips are enabled
                if (_showTooltips)
                {
                    // Check if the hover state would change without triggering a full render yet
                    if (CheckIfHoverChanged(e.Location))
                    {
                        // Only request a render if the hovered vertex actually changed
                        RequestRender();
                    }
                }
                else if (_hoveredVertexIndex != -1)
                {
                    // If we are already hovering the same vertex, we do NOT need to refresh.
                    // The tooltip is anchored to the 3D vertex position, not the mouse cursor.
                    // As long as the vertex hasn't changed and the mesh hasn't rotated,
                    // the render is identical.
                }
            }
        }

        /// <summary>
        /// Performs a lightweight check to see if the hovered vertex has changed
        /// based on the new mouse position, without modifying state.
        /// </summary>
        private bool CheckIfHoverChanged(Point mousePos)
        {
            if (_glControl == null || _mapContent == null || _tableWidth <= 0) return false;

            int rows = _mapContent.Length / (_isSixteenBit ? 2 : 1) / _tableWidth;
            int cols = _tableWidth;
            int totalVertices = rows * cols;

            float minDistance = 15.0f;
            int bestIdx = -1;

            float scaleX = 10.0f / Math.Max(1, cols);
            float scaleY = 10.0f / Math.Max(1, rows);
            float range = Math.Max(1, _dataMaxZ - _dataMinZ);
            float scaleZ = 6.0f / range;

            // We only check a subset of vertices or use the existing projection logic
            // to determine if the 'bestIdx' would be different from '_hoveredVertexIndex'
            for (int i = 0; i < totalVertices; i++)
            {
                int r = i / cols;
                int c = i % cols;
                float val = GetZValue(_mapContent, r, c);
                
                float xPos = (c - (cols - 1) / 2.0f) * scaleX;
                float yPos = (r - (rows - 1) / 2.0f) * scaleY;
                float zPos = (val - _dataMinZ) * scaleZ;
                if (_isUpsideDown) zPos = (range * scaleZ) - zPos;

                PointF screenPos = ProjectToScreen(new Vector3(xPos, yPos, zPos));
                if (screenPos != PointF.Empty)
                {
                    float dx = screenPos.X - mousePos.X;
                    float dy = screenPos.Y - mousePos.Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestIdx = i;
                    }
                }
            }

            return bestIdx != _hoveredVertexIndex;
        }

        private void RequestRender()
        {
            if (_glControl == null) return;
            
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - _lastRenderTime;
            
            // Check if we can render immediately
            if (elapsed.TotalMilliseconds >= MIN_RENDER_INTERVAL_MS)
            {
                // Try to acquire lock - if successful, render immediately
                if (TryAcquireRenderLock())
                {
                    try
                    {
                        _glControl.Invalidate();
                        _lastRenderTime = now;
                    }
                    finally
                    {
                        ReleaseRenderLock();
                    }
                }
                // If lock is held, the current render will pick up the changes
            }
            // If not enough time has passed, skip this request
            // The next paint will include all accumulated changes
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
            else if (e.KeyCode == Keys.T)
            {
                ToggleTooltips();
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
