using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

using LearnOpenTK.Common;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_WPF
{
    public partial class anotherwindow : Window
    {
        public anotherwindow()
        {
            InitializeComponent();
        }


        public void TraceMessage(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Trace.WriteLine("message: " + message);
            Trace.WriteLine("member name: " + memberName);
            Trace.WriteLine("source file path: " + sourceFilePath);
            Trace.WriteLine("source line number: " + sourceLineNumber);
        }


        protected void OnUnload(EventArgs e)
        {
            TraceMessage("");
            //
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            //
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
        }


        private void drawLine(Vector3 from, Vector3 to)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(from);
            GL.Vertex3(to);
            GL.End();
        }


        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            //координаты
            drawLine(new Vector3(0), new Vector3(2f, 0f, 0f));
            drawLine(new Vector3(0), new Vector3(0f, 2f, 0f));
            drawLine(new Vector3(0), new Vector3(0f, 0f, 2f));

            //куб
            drawLine(new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f));
            drawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f));
            drawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f));

            drawLine(new Vector3(1f, 1f, 1f), new Vector3(0f, 1f, 1f));
            drawLine(new Vector3(1f, 1f, 1f), new Vector3(1f, 0f, 1f));
            drawLine(new Vector3(1f, 1f, 1f), new Vector3(1f, 1f, 0f));

            drawLine(new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f));
            drawLine(new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 1f));

            drawLine(new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f));
            drawLine(new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f));

            drawLine(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f));
            drawLine(new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f));

            _shader.Use();
            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            //
            _glControl.SwapBuffers();
        }


        private void GLControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));


            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            _camera = new Camera(Vector3.UnitZ * 3, _glControl.Width / _glControl.Height);
        }


        private void _glControl_Resize(object sender, EventArgs e)
        {
            TraceMessage("");
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }


        private void _glControl_OnKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            TraceMessage(e.KeyCode.ToString());
            var input = e.KeyCode;


            const float cameraSpeed = 0.1f;
            const float sensitivity = 0.5f;

            if (input == Keys.W)
            {
                _camera.Position += _camera.Front * cameraSpeed; // * (float) e.Time; // Forward
            }

            if (input == Keys.S)
            {
                _camera.Position -= _camera.Front * cameraSpeed; // * (float) e.Time; // Backwards
            }

            if (input == Keys.A)
            {
                _camera.Position -= _camera.Right * cameraSpeed; // * (float) e.Time; // Left
            }

            if (input == Keys.D)
            {
                _camera.Position += _camera.Right * cameraSpeed; // * (float) e.Time; // Right
            }

            if (input == Keys.Space)
            {
                _camera.Position += _camera.Up * cameraSpeed; // * (float) e.Time; // Up
            }

            if (input == Keys.LShiftKey)
            {
                _camera.Position -= _camera.Up * cameraSpeed; // * (float) e.Time; // Down
            }

            if (input == Keys.NumPad1)
            {
                _camera.Yaw -= sensitivity;
            }

            if (input == Keys.NumPad3)
            {
                _camera.Yaw += sensitivity;
            }

            if (input == Keys.NumPad5)
            {
                _camera.Pitch += sensitivity;
            }

            if (input == Keys.NumPad2)
            {
                _camera.Pitch -= sensitivity;
            }

            GLControl_Paint(null, null);
        }


        private void LighterXSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }


        private void LighterYSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }


        private void LighterZSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }


        #region Field

        private bool _firstMove = true;
        private Camera _camera;


        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

            -0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f
        };


        private int _elementBufferObject;

        private int _vertexArrayObject;

        private int _vertexBufferObject;

        private Shader _shader;

        #endregion
    }
}