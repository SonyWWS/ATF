//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.VectorMath;

using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// 3D graphics utilities</summary>
    public static class Util3D
    {
        /// <summary>
        /// Sets the current OpenGL color to the given color</summary>
        /// <param name="color">Color</param>
        public static void SetColor(Color color)
        {
            OTK.OpenGL.GL.Color3(color.R, color.G, color.B);
        }

        /// <summary>
        /// Draws a colored line from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="color">Line color</param>
        public static void DrawLine(Vec3F p1, Vec3F p2, Color color)
        {
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Color3(color.R, color.G, color.B);
            OTK.OpenGL.GL.Vertex3(p1.X, p1.Y, p1.Z);
            OTK.OpenGL.GL.Vertex3(p2.X, p2.Y, p2.Z);
            OTK.OpenGL.GL.End();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Draws a colored line from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>        
        public static void DrawLine(Vec3F p1, Vec3F p2)
        {
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(p1.X, p1.Y, p1.Z);
            OTK.OpenGL.GL.Vertex3(p2.X, p2.Y, p2.Z);
            OTK.OpenGL.GL.End();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Draws a 3D arrow from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="coneSize">Arrow head base diameter</param>
        public static void DrawArrow(Vec3F p1, Vec3F p2, float coneSize)
        {
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(p1.X, p1.Y, p1.Z);
            OTK.OpenGL.GL.Vertex3(p2.X, p2.Y, p2.Z);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.Translate(p2.X, p2.Y, p2.Z);
            OTK.OpenGL.GL.PushMatrix();
            Util3D.glMultMatrixf(LookAtMatrix(p2 - p1));
            DrawCone((double)coneSize * 2);
            OTK.OpenGL.GL.PopMatrix();
            OTK.OpenGL.GL.PopMatrix();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Utility rendering styles</summary>
        public enum RenderStyle
        {
            /// <summary>
            /// Wireframe</summary>
            Wireframe,

            /// <summary>
            /// Solid</summary>
            Solid
        }

        /// <summary>
        /// Draws a cube using OpenGL primitives. Consider using DrawCubeDisplayList.</summary>
        /// <param name="size">Dimension of side of cube</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawCube(double size, RenderStyle renderStyle)
        {
            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.Scale((float)size, (float)size, (float)size);
            DrawUnitBox((renderStyle == RenderStyle.Wireframe) ? (int)OTK.OpenGL.All.LineLoop : (int)OTK.OpenGL.All.Quads);
            OTK.OpenGL.GL.PopMatrix();

            Util3D.RenderStats.VertexCount += 6 * 4;
            Util3D.RenderStats.PrimCount += 6;
        }

        public static double[] CalculateFaceNormal(double[] pointa, double[] pointb, double[] pointc)
        {
            double[] vec1 = { pointb[0] - pointa[0],
                              pointb[1] - pointa[1],
                              pointb[2] - pointa[2] };
            double[] vec2 = { pointc[0] - pointa[0],
                              pointc[1] - pointa[1],
                              pointc[2] - pointa[2] };

            return new double[]
            {
                vec1[1] * vec2[2] - vec2[1] * vec1[2],
                -(vec1[0] * vec2[2] - vec2[0] * vec1[2]),
                vec1[0] * vec2[1] - vec2[0] * vec1[1]
            };
        }

        public static void DrawCone(double height)
        {
            int primCount = 0;
            int vertCount = 0;

            double[] baseVertices = { 0.0, height, 0.0 };

            double[] secVertices, terVertices;

            secVertices = new double[3];
            terVertices = new double[3];

            secVertices[1] = 0;
            terVertices[1] = 0;

            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Triangles);

            for(double angle = 0; angle <=360; angle +=45)
            {
                terVertices[0] = secVertices[0];
                terVertices[2] = secVertices[2];

                secVertices[0] = Math.Cos(OpenTK.MathHelper.DegreesToRadians(angle));
                secVertices[2] = Math.Sin(OpenTK.MathHelper.DegreesToRadians(angle));

                if(angle != 0)
                {
                    OTK.OpenGL.GL.Normal3(CalculateFaceNormal(baseVertices, secVertices, terVertices));
                    OTK.OpenGL.GL.Vertex3(baseVertices);
                    OTK.OpenGL.GL.Vertex3(secVertices);
                    OTK.OpenGL.GL.Vertex3(terVertices);

                    primCount += 1;
                    vertCount += 3;
                }
            }

            OTK.OpenGL.GL.End();

            Util3D.RenderStats.PrimCount += primCount;
            Util3D.RenderStats.VertexCount += vertCount;
        }

        /// <summary>
        /// Begins text drawing</summary>
        public static void BeginText()
        {
            OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.DepthTest);
            OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Projection);
            OTK.OpenGL.GL.LoadIdentity();
            OTK.OpenGL.GL.Ortho(-1, 1, -1, 1, -1, 1);
            OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Modelview);
            OTK.OpenGL.GL.LoadIdentity();
        }

        /// <summary>
        /// Ends text drawing</summary>
        public static void EndText()
        {
        }

        /// <summary>
        /// Draws a string at given 2D coordinates</summary>
        /// <param name="text">Text</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public static void DrawText(string text, int x, int y)
        {
            OTK.OpenGL.GL.RasterPos2(x, y);
            OpenGlCore.DrawText(text);
        }

        /// <summary>
        /// Draw a hatched frame for selection</summary>
        /// <param name="controlSize">Size of control displaying view</param>
        /// <param name="first">First (pinned) point</param>
        /// <param name="current">Current (moving) point</param>
        /// <param name="color">Frame color</param>
        public static void DrawHatchedFrame(Size controlSize, Point first, Point current, Color color)
        {
            OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Projection);
            OTK.OpenGL.GL.LoadIdentity();
            OTK.OpenGL.GL.Ortho(-1, 1, -1, 1, -1, 1);
            OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Modelview);
            OTK.OpenGL.GL.LoadIdentity();

            OTK.OpenGL.GL.PushAttrib(OTK.OpenGL.AttribMask.LineBit);
            OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Lighting);
            OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.DepthTest);
            OTK.OpenGL.GL.Enable(OTK.OpenGL.EnableCap.LineStipple);
            OTK.OpenGL.GL.LineWidth(1);
            unchecked { OTK.OpenGL.GL.LineStipple(4, (short)0xAAAA); }
            SetColor(color);

            float ooWidth = 1.0f / controlSize.Width;
            float ooHeight = 1.0f / controlSize.Height;
            float x1 = first.X * ooWidth * 2 - 1;
            float x2 = current.X * ooWidth * 2 - 1;
            float y1 = first.Y * ooHeight * -2 + 1;
            float y2 = current.Y * ooHeight * -2 + 1;

            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.LineLoop);
            OTK.OpenGL.GL.Vertex2(x1, y1);
            OTK.OpenGL.GL.Vertex2(x2, y1);
            OTK.OpenGL.GL.Vertex2(x2, y2);
            OTK.OpenGL.GL.Vertex2(x1, y2);
            OTK.OpenGL.GL.End();
            OTK.OpenGL.GL.PopAttrib();
            Util3D.RenderStats.VertexCount += 4;
        }

        /// <summary>
        /// Gets the view matrix corresponding to a view direction</summary>
        /// <param name="viewDirection">View direction</param>
        /// <returns>View matrix corresponding to the view direction</returns>
        public static Matrix4F LookAtMatrix(Vec3F viewDirection)
        {
            // check if viewDirection is non-zero length and normalize it if so.
            float length = viewDirection.Length;
            if (length == 0.0f)
                return new Matrix4F();
            viewDirection = viewDirection * (1.0f / length);

            // find a basis vector (x-axis or y-axis) that is not parallel to viewDirection.
            // give preference to y-axis for historical purposes.
            // Create 's' and 'u' that are orthonormal vectors, relative to viewDirection.
            // 'viewDirection' is like the z-axis. 's' is the x-axis, and 'u' is the y-axis.
            // We want: s X u = viewDir so that resulting coordinate system is right-handed.
            // Otherwise, you get the triangles all flipped.
            Vec3F s;
            Vec3F xAxis = new Vec3F(1, 0, 0);
            Vec3F yAxis = new Vec3F(0, 1, 0);
            if (Math.Abs(Vec3F.Dot(yAxis, viewDirection)) < 0.98)
            {
                s = Vec3F.Cross(yAxis, viewDirection); //x' = y X z
            }
            else
            {
                // viewDirection == yAxis
                s = Vec3F.Cross(viewDirection, xAxis); //x' = y X x
            }
            Vec3F u = Vec3F.Cross(viewDirection, s); //y' = z X x'

            s = s / s.Length;
            u = u / u.Length;

            Matrix4F T = new Matrix4F
                (
                s.X, s.Y, s.Z, 0,
                u.X, u.Y, u.Z, 0,
                viewDirection.X, viewDirection.Y, viewDirection.Z, 0,
                0, 0, 0, 1
                );

            return T;
        }

        /// <summary>
        /// Gets the view matrix corresponding to a "look at" point</summary>
        /// <param name="azimuth">Camera pan, in radians</param>
        /// <param name="elevation">Camera tilt, in radians</param>
        /// <param name="lookAt">"Look at" direction</param>
        /// <returns>View matrix corresponding to the "look at" point</returns>
        public static Matrix4F LookAtRotationMatrix(float azimuth, float elevation, Vec3F lookAt)
        {
            double sy = Math.Sin(azimuth);
            double cy = Math.Cos(azimuth);
            double sx = Math.Sin(elevation);
            double cx = Math.Cos(elevation);

            // Calc eye vector as the rotation matrix * (0,0,1)
            Vec3F eye = new Vec3F((float)sy, (float)(-cy * sx), (float)(cy * cx));
            eye = eye + lookAt;

            // Calc up vector as the rotation matrix * (0,1,0)
            Vec3F up = new Vec3F(0, (float)cx, (float)sx);

            Vec3F fvec = lookAt - eye;
            Vec3F f = Vec3F.Normalize(fvec);
            Vec3F s = Vec3F.Cross(f, up);
            Vec3F u = Vec3F.Cross(s, f);

            return new Matrix4F
            (
                s.X, s.Y, s.Z, 0,
                u.X, u.Y, u.Z, 0,
                -f.X, -f.Y, -f.Z, 0,
               0, 0, 0, 1
            );
        }

        /// <summary>
        /// Gets the view matrix corresponding to a "look at" point</summary>
        /// <param name="azimuth">Camera pan, in radians</param>
        /// <param name="elevation">Camera tilt, in radians</param>
        /// <param name="lookAt">"Look at" direction</param>
        /// <param name="dolly">Distance from "look at" point</param>
        /// <returns>View matrix corresponding to the "look at" point</returns>
        public static Matrix4F LookAtMatrix(float azimuth, float elevation, Vec3F lookAt, float dolly)
        {
            Matrix4F R = LookAtRotationMatrix(azimuth, elevation, lookAt);
            R.ZTranslation = -dolly;

            Matrix4F T = new Matrix4F
            (
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                -lookAt.X, -lookAt.Y, -lookAt.Z, 1
            );

            Matrix4F result = new Matrix4F();
            result.Mul(T, R);

            return result;
        }

        /// <summary>
        /// The rendering statistics for debugging and analysis purposes. Is used
        /// to display statistics in the Design view. Derive from the class
        /// RenderStats and assign an instance here to calculate and display
        /// custom statistics.</summary>
        public static RenderStats RenderStats
        {
            get { return m_renderStats; }
            set { m_renderStats = value; }
        }

        /// <summary>
        /// ATI's vendor string</summary>
        public const string AtiVendorString = "ATI Technologies Inc.";

        /// <summary>
        /// Nvidia's vendor string</summary>
        public const string NvidiaVendorString = "NVIDIA Corporation";

        /// <summary>
        /// Gets the current OpenGL context's vendor string</summary>
        public static string Vendor
        {
            get
            {
                return OTK.OpenGL.GL.GetString(OTK.OpenGL.StringName.Vendor);
            }
        }

        /// <summary>
        /// Gets whether ATI is the vendor</summary>
        public static bool IsAtiVendor
        {
            get { return (Vendor == AtiVendorString); }
        }

        /// <summary>
        /// Gets whether Nvidia is the vendor</summary>
        public static bool IsNvidiaVendor
        {
            get { return (Vendor == NvidiaVendorString); }
        }

        /// <summary>
        /// Casts a matrix to an IntPtr, which is useful for OpenGL calls</summary>
        /// <param name="m">Matrix to be cast</param>
        /// <returns>Matrix cast as IntPtr</returns>
        public static unsafe IntPtr CastToIntPtr(Matrix4F m)
        {
            fixed (float* firstElement = &m.M11)
                return new IntPtr((void*)firstElement);
        }

        #region Tao.OpenGl Extensions

        /// <summary>
        /// Multiplies the current matrix by a given matrix</summary>
        /// <param name="m">Matrix to multiply</param>
        public static void glMultMatrixf(Matrix4F m)
        {
            OTK.OpenGL.GL.MultMatrix(m.ToArray());
        }

        /// <summary>
        /// Loads matrix as the current matrix</summary>
        /// <param name="m">Matrix to load</param>
        public static void glLoadMatrixf(Matrix4F m)
        {
            OTK.OpenGL.GL.LoadMatrix(m.ToArray());
        }

        /// <summary>
        /// Gets state variable which was cast as float</summary>
        /// <param name="pname">Enum for state variable to be returned</param>
        /// <param name="m">State variable as Matrix4F</param>
        public static void glGetFloatv(int pname, Matrix4F m)
        {
            OTK.OpenGL.GL.GetFloat((OTK.OpenGL.GetPName)pname, m.ToArray());
        }

        #endregion

        /// <summary>
        /// The delegate definition for an OpenGL error handler</summary>
        /// <param name="errorId">The OpenGL error number</param>
        public delegate void ErrorHandlerCallback(int errorId);
        
        /// <summary>
        /// The delegate that is called in order to report OpenGL errors</summary>
        public static ErrorHandlerCallback ErrorHandler = DefaultErrorHandler;

        /// <summary>
        /// Translates an OpenGL error number into a human readable description of the error.
        /// See glGetError() in http://www.opengl.org/sdk/docs/man/ </summary>
        /// <param name="errorId">The Glenum as an integer that was returned by glGetError</param>
        /// <returns>Text describing the error</returns>
        /// <exception cref="InvalidOperationException">If the given error number is not recognized</exception>
        public static string GetErrorText(int errorId)
        {
            string errorText;

            switch (errorId)
            {
                case (int)OTK.OpenGL.ErrorCode.NoError:
                    errorText = "No error has been recorded.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.InvalidEnum:
                    errorText = "An unacceptable value is specified for an enumerated argument.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.InvalidValue:
                    errorText = "A numeric argument is out of range.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.InvalidFramebufferOperationExt:
                    errorText = "The specified operation is not allowed in the current state.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.StackOverflow:
                    errorText = "This command would cause a stack overflow.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.StackUnderflow:
                    errorText = "This command would cause a stack underflow.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.OutOfMemory:
                    errorText =
                        "There is not enough memory left to execute the command.\n" +
                        "The state of the GL is undefined,\n" +
                        "except for the state of the error flags,\n" +
                        "after this error is recorded.";
                    break;
                case (int)OTK.OpenGL.ErrorCode.TableTooLarge:
                    errorText = "The specified table exceeds the implementation's maximum supported table size.";
                    break;
                default:
                    throw new InvalidOperationException("unknown OpenGl enum");
            }

            return errorText;
        }

        /// <summary>
        /// Maximum number of OpenGL errors to report in a single call to ReportErrors</summary>
        public static int MaxErrorsToReport = 10;

        /// <summary>
        /// Reports OpenGL errors using the current ErrorHandler delegate, Util3D.ErrorHandler.
        /// Clears all OpenGL errors that have occurred since the last time Gl.glGetError was called.</summary>
        public static void ReportErrors()
        {
            int errorCount = 0;
            int errorId;
            while ((errorId = (int)OTK.OpenGL.GL.GetError()) != 0)
            {
                if (errorCount++ < MaxErrorsToReport)
                    ErrorHandler(errorId);
                else
                    break; //to avoid an infinite loop. http://sf.ship.scea.com/sf/go/artf36082
            }
        }

        /// <summary>
        /// The default error reporting method that is assigned to the ErrorHandler field</summary>
        /// <param name="errorNum">The OpenGL error number that comes from glGetError()</param>
        private static void DefaultErrorHandler(int errorNum)
        {
            string errorText = GetErrorText(errorNum);
            Outputs.WriteLine(OutputMessageType.Error, "OpenGl error: " + errorText);
        }

        // simply draws a box, each edge 1 unit in length, centered on the origin.
        //  does not update rendering statistics. Can be used in a display list.
        // 'type' should be Gl.GL_LINE_LOOP or Gl.GL_QUADS.
        private static void DrawUnitBox(int type)
        {
            v[0][0] = v[1][0] = v[2][0] = v[3][0] = -0.5f;
            v[4][0] = v[5][0] = v[6][0] = v[7][0] = 0.5f;
            v[0][1] = v[1][1] = v[4][1] = v[5][1] = -0.5f;
            v[2][1] = v[3][1] = v[6][1] = v[7][1] = 0.5f;
            v[0][2] = v[3][2] = v[4][2] = v[7][2] = -0.5f;
            v[1][2] = v[2][2] = v[5][2] = v[6][2] = 0.5f;

            for (int i = 5; i >= 0; i--)
            {
                OTK.OpenGL.GL.Begin((OTK.OpenGL.PrimitiveType)type);
                OTK.OpenGL.GL.Normal3(n[i]);
                OTK.OpenGL.GL.Vertex3(v[faces[i, 0]]);
                OTK.OpenGL.GL.Vertex3(v[faces[i, 1]]);
                OTK.OpenGL.GL.Vertex3(v[faces[i, 2]]);
                OTK.OpenGL.GL.Vertex3(v[faces[i, 3]]);
                OTK.OpenGL.GL.End();
            }
        }

        private static readonly float[][] v = new float[8][]
        {
            new float[3],
            new float[3],
            new float[3],
            new float[3],
            new float[3],
            new float[3],
            new float[3],
            new float[3]
        };
        private static readonly float[][] n = new float[6][]
        {
            new float[3] {-1.0f, 0.0f, 0.0f},
            new float[3] {0.0f, 1.0f, 0.0f},
            new float[3] {1.0f, 0.0f, 0.0f},
            new float[3] {0.0f, -1.0f, 0.0f},
            new float[3] {0.0f, 0.0f, 1.0f},
            new float[3] {0.0f, 0.0f, -1.0f}
        };

        private static readonly int[,] faces = new int[,]
        {
            {0, 1, 2, 3},
            {3, 2, 6, 7},
            {7, 6, 5, 4},
            {4, 5, 1, 0},
            {5, 6, 2, 1},
            {7, 4, 0, 3}
        };
        
        // key: OpenGl drawing mode. value: display list.
        private static readonly Dictionary<int,int> s_cubeDisplayLists = new Dictionary<int,int>();
        private static readonly Dictionary<int, int> s_coneDisplayLists = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> s_sphereDisplayLists = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> s_cylinderDisplayLists = new Dictionary<int, int>();

        private static RenderStats m_renderStats = new RenderStats();

        private const int FAST_CONE_SLICES = 8;
        private const int FAST_CONE_STACKS = 1;
        private const int FAST_SPHERE_SLICES = 10;
        private const int FAST_SPHERE_STACKS = 10;
        private const int FAST_CYLINDER_SLICES = 10;
        private const int FAST_CYLINDER_STACKS = 1;
    }
}
