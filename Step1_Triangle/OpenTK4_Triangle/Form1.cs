using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK4_Triangle
{
    public partial class Form1 : Form
    {
        int ShaderProgramID;
        int VertexShaderID;
        int FragmentShaderID;

        int[] vaoHandle = new int[1];
        
        public Form1()
        {
            InitializeComponent();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            InitShaders();
            CreateBuffers();
            SetupViewport();
            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                Render();
            }
        }

        private void SetupViewport()
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.ClearColor(Color.LightBlue);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1, 1, -1, 1, 0.0, 4.0);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vaoHandle[0]);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            glControl1.SwapBuffers();
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            glControl1.Width = this.Size.Width - 40;
            glControl1.Height = this.Size.Height - 60;
            glControl1_Resize(sender, e);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            SetupViewport();
        }

        /// <summary>
        /// This creates a new shader (using a value from the ShaderType enum), loads code for it, compiles it, and adds it to our program.
        /// It also prints any errors it found to the console, which is really nice for when you make a mistake in a shader (it will also yell at you if you use deprecated code).
        /// </summary>
        /// <param name="filename">File to load the shader from</param>
        /// <param name="type">Type of shader to load</param>
        /// <param name="program">ID of the program to use the shader with</param>
        /// <param name="address">Address of the compiled shader</param>
        private void LoadShader(String filename, ShaderType type, int program, out int address)
        {

            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private void CreateBuffers()
        {
            var positionData = new float[] { -0.8f, -0.8f, 0.0f,
                                              0.8f, -0.8f, 0.0f,
                                              0.0f, 0.8f, 0.0f};
            var colorData = new float[]{ 1.0f, 0.0f, 0.0f,
                                         0.0f, 1.0f, 0.0f,
                                         0.0f, 0.0f, 1.0f};
            int[] vboHandles = new int[2];
            GL.GenBuffers(2, vboHandles);

            int positionBufferHandle = vboHandles[0];
            int colorBufferHandle = vboHandles[1];

            //Fill coordinates buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(positionData.Length * sizeof(float)), positionData, BufferUsageHint.StaticDraw);

            //Fill color buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(positionData.Length * sizeof(float)), colorData, BufferUsageHint.StaticDraw);

            //Create VAO
            GL.GenVertexArrays(1, vaoHandle);
            GL.BindVertexArray(vaoHandle[0]);

            //Activate arrays of vertex attribs
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            //Закрепить индекс 0 за буфером с координатами
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            //Закрепить индекс 1 за буфером с координатами
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferHandle);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

        }

        private bool InitShaders()
        {
            ShaderProgramID = GL.CreateProgram();
            LoadShader("..\\..\\vert_shader.glsl", ShaderType.VertexShader, ShaderProgramID, out VertexShaderID);
            LoadShader("..\\..\\frag_shader.glsl", ShaderType.FragmentShader, ShaderProgramID, out FragmentShaderID);
            GL.LinkProgram(ShaderProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(ShaderProgramID));
            GL.UseProgram(ShaderProgramID);
            return true;
        }
        
    }

}