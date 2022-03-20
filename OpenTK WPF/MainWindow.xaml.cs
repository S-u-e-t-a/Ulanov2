using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

using LearnOpenTK.Common;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
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

        public float LighterPosX
        {
            get { return _lightPos.X; }
            set
            {
                var newPos = new Vector3(LighterPos);
                newPos.X = value;
                LighterPos = newPos;
                OnPropertyChanged();
            }
        }

        public float LighterPosY
        {
            get { return _lightPos.Y; }
            set
            {
                var newPos = new Vector3(LighterPos);
                newPos.Y = value;
                LighterPos = newPos;
                OnPropertyChanged();
            }
        }

        public float LighterPosZ
        {
            get { return _lightPos.Z; }
            set
            {
                var newPos = new Vector3(LighterPos);
                newPos.Z = value;
                LighterPos = newPos;
                OnPropertyChanged();
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


        public Vector3 LighterPos
        {
            get { return _lightPos; }
            set
            {
                _lightPos = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LighterPosX));
                OnPropertyChanged(nameof(LighterPosY));
                OnPropertyChanged(nameof(LighterPosZ));
                GLControl_Paint(null, null);
            }
        }


        public MainWindow()
        {
            _camera = new Camera(new Vector3(0), 0); // лютый костыль но шо поделать
            InitializeComponent();
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }


        protected void OnUnload(EventArgs e)
        {
            //
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            //
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
        }


        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vaoModel);

            _lightingShader.Use();

            _lightingShader.SetMatrix4("model", Matrix4.Identity);
            _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
            _lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetVector3("lightPos", _lightPos);
            _lightingShader.SetVector3("viewPos", _camera.Position);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.BindVertexArray(_vaoModel);

            _lampShader.Use();

            var lampMatrix = Matrix4.CreateScale(0.2f);
            lampMatrix = lampMatrix * Matrix4.CreateTranslation(_lightPos);

            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            //
            _glControl.SwapBuffers();
        }


        private void GLControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
            _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            {
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);

                var positionLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                // Remember to change the stride as we now have 6 floats per vertex
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

                // We now need to define the layout of the normal so the shader can use it
                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }

            {
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                var positionLocation = _lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                // Also change the stride here as we now have 6 floats per vertex. Now we don't define the normal for the lamp VAO
                // this is because it isn't used, it might seem like a waste to use the same VBO if they dont have the same data
                // The two cubes still use the same position, and since the position is already in the graphics memory it is actually
                // better to do it this way. Look through the web version for a much better understanding of this.
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                _camera = new Camera(Vector3.UnitZ * 3, _glControl.Width / _glControl.Height);
            }
        }


        private void _glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }


        private void _glControl_OnKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
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
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CameraPitch = -45;
            CameraYaw = -135;
            CameraPostion = new Vector3(2);
        }


        #region Field

        private readonly float[] _vertices =
        {
            // Position          Normal
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, // Front face
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f,

            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, // Back face
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,

            -0.5f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f, // Left face
            -0.5f, 0.5f, -0.5f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f,

            0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, // Right face
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f,

            -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, // Bottom face
            0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // Top face
            0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f
        };

        private Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private Shader _lampShader;

        private Shader _lightingShader;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        #endregion
    }
}