/*
 * OpenTK implementation of Tesselation shaders from article:
 * http://steps3d.narod.ru/tutorials/tessellation-algos-tutorial.html
 * 
 * 
 */

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
        int TessControlShaderID;
        int TessEvaluationShaderID;
        int FragmentShaderID;

        int[] vaoHandle = new int[1];

        int modelviewMatrixLocation,
            projectionMatrixLocation;

        int innerShaderLocation;
        int innerValue = 1;

        int outerShaderLocation;
        int outerValue = 1;

        int nmMatrixLocation;
        
        //Треугольники
        Vector3[] positionVboData;
        int positionVboHandle;
        Vector3[] normalVboData;
        int normalVboHandle;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        Matrix4 projectionMatrix, modelviewMatrix;

        public Form1()
        {
            InitializeComponent();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.DontCare);

            InitShaders();
            CreateUniforms();
            CreateVBOs();
            CreateVAOs();
            SetupViewport();
            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                double milliseconds = ComputeTimeSlice();
                Accumulate(milliseconds);
                Render();
            }
        }

        private void SetupViewport()
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Black);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            Matrix4 rotation = Matrix4.CreateRotationZ(0.001f);
            Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);

            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

            Matrix4 inversed = modelviewMatrix.Inverted();
            Matrix3 nm = new Matrix3(inversed.M11, inversed.M21, inversed.M31, inversed.M12, inversed.M22, inversed.M32, inversed.M13, inversed.M23, inversed.M33);
            //Matrix3 nm = new Matrix3(inversed.M11, inversed.M12, inversed.M13, inversed.M21, inversed.M22, inversed.M23, inversed.M31, inversed.M32, inversed.M33);

            GL.UniformMatrix3(nmMatrixLocation, false, ref nm);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(vaoHandle[0]);

            GL.DrawArrays(PrimitiveType.Patches, 0, positionVboData.Length);

            glControl1.SwapBuffers();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            
            glControl1.Width = this.Size.Width - 340;
            glControl1.Height = this.Size.Height - 60;
            groupBox1.Location = new Point(this.Size.Width - 310, groupBox1.Location.Y);
            trackBar1.Location = new Point(this.Size.Width - 310, trackBar1.Location.Y);
            trackBar2.Location = new Point(this.Size.Width - 310, trackBar2.Location.Y);
            label1.Location = new Point(this.Size.Width - 310, label1.Location.Y);
            label2.Location = new Point(this.Size.Width - 310, label2.Location.Y);
            label3.Location = new Point(this.Size.Width - 310, label3.Location.Y);
            radioButton1.Location = new Point(this.Size.Width - 310, radioButton1.Location.Y);
            radioButton2.Location = new Point(this.Size.Width - 310, radioButton2.Location.Y);
            glControl1_Resize(sender, e);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            SetupViewport();
        }

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

        void CreateUniforms()
        {
            projectionMatrixLocation = GL.GetUniformLocation(ShaderProgramID, "projection_matrix");
            modelviewMatrixLocation = GL.GetUniformLocation(ShaderProgramID, "modelview_matrix");

            float aspectRatio = glControl1.Width / (float)(glControl1.Height);

            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, aspectRatio, 1, 100, out projectionMatrix);
            modelviewMatrix = Matrix4.LookAt(new Vector3(1, 2, 1), new Vector3(0, 0, 0), new Vector3(0, 0, 1));

            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

            innerShaderLocation = GL.GetUniformLocation(ShaderProgramID, "inner");
            GL.Uniform1(innerShaderLocation, innerValue);

            outerShaderLocation = GL.GetUniformLocation(ShaderProgramID, "outer");
            GL.Uniform1(outerShaderLocation, outerValue);

            nmMatrixLocation = GL.GetUniformLocation(ShaderProgramID, "nm");

        }

        void CreateVBOs()
        {
            positionVboData = getOctahedron();

            normalVboData = getOctahedron();

            positionVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                positionVboData, BufferUsageHint.StaticDraw);

            normalVboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(normalVboData.Length * Vector3.SizeInBytes),
                normalVboData, BufferUsageHint.StaticDraw);
        }

        void CreateVAOs()
        {
            vaoHandle[0] = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle[0]);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            GL.BindVertexArray(0);
        }

        private bool InitShaders()
        {
            ShaderProgramID = GL.CreateProgram();
            LoadShader("..\\..\\shaders\\vertex.glsl", ShaderType.VertexShader, ShaderProgramID, out VertexShaderID);
            /* Compute Bezier coeffs */
            LoadShader("..\\..\\shaders\\tesscontrol.glsl", ShaderType.TessControlShader, ShaderProgramID, out TessControlShaderID);
            /* Compute new points from Bezier coeffs */
            LoadShader("..\\..\\shaders\\tessevaluation.glsl", ShaderType.TessEvaluationShader, ShaderProgramID, out TessEvaluationShaderID);
            LoadShader("..\\..\\shaders\\fragment.glsl", ShaderType.FragmentShader, ShaderProgramID, out FragmentShaderID);

            GL.LinkProgram(ShaderProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(ShaderProgramID));
            GL.UseProgram(ShaderProgramID);
            return true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            GL.Uniform1(innerShaderLocation, trackBar1.Value);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            GL.Uniform1(outerShaderLocation, trackBar2.Value);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private double ComputeTimeSlice()
        {
            sw.Stop();
            double timeslice = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            return timeslice;
        }
        double accumulator = 0;
        int idleCounter = 0;
        private void Accumulate(double milliseconds)
        {
            idleCounter++;
            accumulator += milliseconds;
            if (accumulator > 1000)
            {
                this.Text = "FPS: " + idleCounter.ToString();
                accumulator -= 1000;
                idleCounter = 0; // don't forget to reset the counter!
            }
        }

        Vector3[] getIcosahedron()
        {
            Vector3[] res = new Vector3[60];
            float phi = (1.0f + (float)Math.Sqrt(5.0)) / 2.0f;
            float phi2 = (float)Math.Sqrt(1.0f + phi * phi);

            Vector3[] verts = new Vector3[]
                {
                     new Vector3(  phi,  1.0f,  0.0f),
                     new Vector3(  phi, -1.0f,  0.0f),
                     new Vector3( -phi, -1.0f,  0.0f),
                     new Vector3( -phi,  1.0f,  0.0f),
                     new Vector3( 0.0f,   phi,  1.0f),
                     new Vector3( 0.0f,  -phi,  1.0f),
                     new Vector3( 0.0f,  -phi, -1.0f),
                     new Vector3( 0.0f,   phi, -1.0f),
                     new Vector3( 1.0f,  0.0f,   phi),
                     new Vector3( 1.0f,  0.0f,  -phi),
                     new Vector3(-1.0f,  0.0f,  -phi),
                     new Vector3(-1.0f,  0.0f,   phi),
                };
            int[] faces = { 8, 1, 5, 8, 5, 4, 8, 4, 11, 8, 11, 10, 8, 10, 1,
                              6, 9, 12, 6, 12, 3, 6, 3, 7, 6, 7, 2, 6, 2, 9,
                              1, 9, 5, 9, 5, 12, 5, 12, 4, 12, 4, 3, 4, 3, 11,
                              3, 11, 7, 11, 7, 10, 7, 10, 2, 10, 2, 1, 2, 1, 9 };
            for (int i = 0; i < 60; i++)
            {
                int j = faces[i];
                res[i] = new Vector3(verts[j-1])/phi2;
            }
            return res;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            positionVboData = getIcosahedron();
            normalVboData = getIcosahedron();

            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                positionVboData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(normalVboData.Length * Vector3.SizeInBytes),
                normalVboData, BufferUsageHint.StaticDraw);
        }

        Vector3[] getOctahedron()
        {
            return new Vector3[]{
            new Vector3( 1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f,  1.0f,  0.0f),
            new Vector3( 0.0f,  0.0f,  1.0f),

            new Vector3( 0.0f,  1.0f,  0.0f),
            new Vector3(-1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f,  0.0f,  1.0f),

            new Vector3(-1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f, -1.0f,  0.0f),
            new Vector3( 0.0f,  0.0f,  1.0f),

            new Vector3( 0.0f, -1.0f,  0.0f),
            new Vector3( 1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f,  0.0f,  1.0f),

            new Vector3( 1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f,  0.0f, -1.0f),
            new Vector3( 0.0f,  1.0f,  0.0f),

            new Vector3( 0.0f,  1.0f,  0.0f),
            new Vector3( 0.0f,  0.0f, -1.0f),
            new Vector3(-1.0f,  0.0f,  0.0f),

            new Vector3(-1.0f,  0.0f,  0.0f),
            new Vector3( 0.0f,  0.0f, -1.0f),
            new Vector3( 0.0f, -1.0f,  0.0f),

            new Vector3( 0.0f, -1.0f,  0.0f),
            new Vector3( 0.0f,  0.0f, -1.0f),
            new Vector3( 1.0f,  0.0f,  0.0f)
             };
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            positionVboData = getOctahedron();
            normalVboData = getOctahedron();

            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                positionVboData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(normalVboData.Length * Vector3.SizeInBytes),
                normalVboData, BufferUsageHint.StaticDraw);
        }
    }
}