using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

using LearnOpenTK.Common;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using MessageBox = System.Windows.Forms.MessageBox;

namespace OpenTK_WPF
{
    public partial class anotherwindow : Window, INotifyPropertyChanged
    {
        public float CameraPitch
        {
            get { return _camera.Pitch; }
            set
            {
                _camera.Pitch = value;
                OnPropertyChanged();
                GLControl_Paint(null, null);
            }
        }

        public float CameraPostionX
        {
            get { return _camera.Position.X; }
            set
            {
                CameraPostion = new Vector3(value, CameraPostion.Y, CameraPostion.Z);
                OnPropertyChanged();
                GLControl_Paint(null, null);
            }
        }

        public float CameraPostionY
        {
            get { return _camera.Position.Y; }
            set
            {
                CameraPostion = new Vector3(CameraPostion.X, value, CameraPostion.Z);
                OnPropertyChanged();
                GLControl_Paint(null, null);
            }
        }

        public float CameraPostionZ
        {
            get { return _camera.Position.Z; }
            set
            {
                CameraPostion = new Vector3(CameraPostion.X, CameraPostion.Y, value);
                OnPropertyChanged();
                GLControl_Paint(null, null);
            }
        }

        public float CameraYaw
        {
            get { return _camera.Yaw; }
            set
            {
                _camera.Yaw = value;
                OnPropertyChanged();
                GLControl_Paint(null, null);
            }
        }

        public Vector3 CameraPostion
        {
            get { return _camera.Position; }
            set
            {
                _camera.Position = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CameraPostionX));
                OnPropertyChanged(nameof(CameraPostionY));
                OnPropertyChanged(nameof(CameraPostionZ));
                GLControl_Paint(null, null);
            }
        }


        public anotherwindow()
        {
            _camera = new Camera(new Vector3(0), 0); // лютый костыль но шо поделать
            InitializeComponent();
        }


        public event PropertyChangedEventHandler PropertyChanged;


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


        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
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
                CameraPostion += _camera.Front * cameraSpeed; // * (float) e.Time; // Forward
            }

            if (input == Keys.S)
            {
                CameraPostion -= _camera.Front * cameraSpeed; // * (float) e.Time; // Backwards
            }

            if (input == Keys.A)
            {
                CameraPostion -= _camera.Right * cameraSpeed; // * (float) e.Time; // Left
            }

            if (input == Keys.D)
            {
                CameraPostion += _camera.Right * cameraSpeed; // * (float) e.Time; // Right
            }

            if (input == Keys.Space)
            {
                CameraPostion += _camera.Up * cameraSpeed; // * (float) e.Time; // Up
            }

            if (input == Keys.LShiftKey)
            {
                CameraPostion -= _camera.Up * cameraSpeed; // * (float) e.Time; // Down
            }

            if (input == Keys.NumPad1)
            {
                CameraYaw -= sensitivity;
            }

            if (input == Keys.NumPad3)
            {
                CameraYaw += sensitivity;
            }

            if (input == Keys.NumPad5)
            {
                CameraPitch += sensitivity;
            }

            if (input == Keys.NumPad2)
            {
                CameraPitch -= sensitivity;
            }

            //GLControl_Paint(null, null);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CameraPitch = -45;
            CameraYaw = -135;
            CameraPostion = new Vector3(2);
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("3D куб в центре координат. \n"
                            + "Авторы: Хлебников Роман, Гусев Антон, Ермаков Даниил, Кувылькин Андрей, Трифонов Юрий \n"
                            + "Управление камерой: \n" +
                            "W - вперед \n" +
                            "S - назад \n" +
                            "A - влево \n" +
                            "D - вправо \n" +
                            "Пробел - вверх \n" +
                            "Левый ctrl - вниз \n" +
                            "Кпопки нумпада 1 и 3 - вращение против часовой и по часовой стрелке \n" +
                            "Кпопки нумпада 2 и 5 - вращение вниз и вверх \n", "О программе");
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