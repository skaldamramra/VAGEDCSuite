using System;
using VAGSuite.MapViewerEventArgs;
using System.Data;
using System.Drawing;
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
        private int _shaderProgram;
        private int _vertexCount;
        private bool _buffersInitialized;
        private bool _firstPaint = true;

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
                    int posLoc = GL.GetAttribLocation(_shaderProgram, "aPosition");
                    int colLoc = GL.GetAttribLocation(_shaderProgram, "aColor");
                    
                    Matrix4 projection;
                    Matrix4 modelView;
                    GetMatrices(out projection, out modelView);

                    GL.UniformMatrix4(modelViewLoc, false, ref modelView);
                    GL.UniformMatrix4(projectionLoc, false, ref projection);

                    // Bind VBO
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                    GL.EnableVertexAttribArray(posLoc);
                    GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);

                    // Bind CBO
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _cbo);
                    GL.EnableVertexAttribArray(colLoc);
                    GL.VertexAttribPointer(colLoc, 4, VertexAttribPointerType.Float, false, 0, 0);

                    // Draw
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                    GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                    GL.DisableVertexAttribArray(posLoc);
                    GL.DisableVertexAttribArray(colLoc);
                    GL.UseProgram(0);
                }
            
                _glControl.SwapBuffers();
                // Console.WriteLine("Chart3DComponent: SwapBuffers completed");
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

            // Camera distance - normalized to the 10x10x10 volume we scale into
            float distance = 20f / _zoom;
            
            // Look at the center of our 10x10x10 coordinate system
            Vector3 target = new Vector3(0, 2, 0);
            Vector3 eye = new Vector3(distance, distance, distance);
            
            modelview = Matrix4.LookAt(eye, target, Vector3.UnitY);
            modelview *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_elevation));
            modelview *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotation));
        }

        private void DrawAxes()
        {
            GL.Disable(EnableCap.Lighting); // Axes shouldn't be lit
            GL.Begin(PrimitiveType.Lines);
            
            // X - Red
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(10, 0, 0);
            
            // Y - Green (Up)
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 10, 0);
            
            // Z - Blue
            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 10);
            
            GL.End();
            GL.Enable(EnableCap.Lighting);
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
            float scaleX = 15.0f / Math.Max(1, cols);
            float scaleZ = 15.0f / Math.Max(1, rows);
            float scaleY = 8.0f / range; // Auto-scale height to fit 8 units

            int totalVertices = rows * cols;
            Vector3[] vertices = new Vector3[totalVertices];
            Vector4[] colors = new Vector4[totalVertices];

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
                // Based on your feedback, Driver Wish (standard) was being flipped incorrectly
                if (_isUpsideDown)
                {
                    yPos = (maxZ - minZ) * scaleY - yPos;
                }
                
                vertices[i] = new Vector3(xPos, yPos, zPos);
                // Color should follow the visual height (Red = High, Blue = Low)
                colors[i] = GetColorForZ(val, minZ, maxZ, _isUpsideDown);
            }

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

            if (!_buffersInitialized)
            {
                GL.GenBuffers(1, out _vbo);
                GL.GenBuffers(1, out _cbo);
                GL.GenBuffers(1, out _ebo);
                _buffersInitialized = true;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * Vector4.SizeInBytes, colors, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
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
            return new Vector4(r, g, b, 1.0f);
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
            // Downgraded to GLSL 1.20 for maximum compatibility
            string vertexShaderSource = @"
                #version 120
                attribute vec3 aPosition;
                attribute vec4 aColor;
                varying vec4 vColor;
                uniform mat4 uModelView;
                uniform mat4 uProjection;
                void main() {
                    gl_Position = uProjection * uModelView * vec4(aPosition, 1.0);
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
                _rotation += (e.X - _lastMousePos.X) * 0.5f;
                _elevation += (e.Y - _lastMousePos.Y) * 0.5f;
                _lastMousePos = e.Location;
                RefreshChart();
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
