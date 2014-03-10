//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.VectorMath;

using Tao.OpenGl;

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
            Gl.glColor3ub(color.R, color.G, color.B);
        }

        /// <summary>
        /// Draws a colored line from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="color">Line color</param>
        public static void DrawLine(Vec3F p1, Vec3F p2, Color color)
        {
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3ub(color.R, color.G, color.B);
            Gl.glVertex3d(p1.X, p1.Y, p1.Z);
            Gl.glVertex3d(p2.X, p2.Y, p2.Z);
            Gl.glEnd();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Draws a colored line from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>        
        public static void DrawLine(Vec3F p1, Vec3F p2)
        {
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3d(p1.X, p1.Y, p1.Z);
            Gl.glVertex3d(p2.X, p2.Y, p2.Z);
            Gl.glEnd();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Draws a 3D arrow from point 1 to point 2</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="coneSize">Arrow head base diameter</param>
        public static void DrawArrow(Vec3F p1, Vec3F p2, float coneSize)
        {
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3f(p1.X, p1.Y, p1.Z);
            Gl.glVertex3f(p2.X, p2.Y, p2.Z);
            Gl.glEnd();

            Gl.glPushMatrix();
            Gl.glTranslatef(p2.X, p2.Y, p2.Z);
            Gl.glPushMatrix();
            Util3D.glMultMatrixf(LookAtMatrix(p2 - p1));
            DrawCone((double)coneSize, (double)coneSize * 2, 8, 1, RenderStyle.Solid);
            Gl.glPopMatrix();
            Gl.glPopMatrix();
            Util3D.RenderStats.VertexCount += 2;
        }

        /// <summary>
        /// Draws a 3D arrow from point 1 to point 2 using an OpenGL display list for greatly improved
        /// performance. This method must not be called in between Gl.glNewList and Gl.glEndList.</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="coneSize">Arrow head base diameter</param>
        public static void DrawArrowDisplayList(Vec3F p1, Vec3F p2, float coneSize)
        {
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3f(p1.X, p1.Y, p1.Z);
            Gl.glVertex3f(p2.X, p2.Y, p2.Z);
            Gl.glEnd();

            Gl.glPushMatrix();
            Gl.glTranslatef(p2.X, p2.Y, p2.Z);
            Gl.glPushMatrix();
            Util3D.glMultMatrixf(LookAtMatrix(p2 - p1));
            DrawConeDisplayList(coneSize, coneSize * 2, RenderStyle.Solid);
            Gl.glPopMatrix();
            Gl.glPopMatrix();
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
            Gl.glPushMatrix();
            Gl.glScalef((float)size, (float)size, (float)size);
            DrawUnitBox((renderStyle == RenderStyle.Wireframe) ? Gl.GL_LINE_LOOP : Gl.GL_QUADS);
            Gl.glPopMatrix();

            Util3D.RenderStats.VertexCount += 6 * 4;
            Util3D.RenderStats.PrimCount += 6;
        }

        /// <summary>
        /// Draws a cube, using an OpenGL display list for greatly improved performance. This method must
        /// not be called in between Gl.glNewList and Gl.glEndList.</summary>
        /// <param name="size">Dimension of side of cube</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawCubeDisplayList(float size, RenderStyle renderStyle)
        {
            int displayList;
            Gl.glPushMatrix();
            Gl.glScalef(size, size, size);

            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;
            if (!s_cubeDisplayLists.TryGetValue(drawStyle, out displayList))
            {
                displayList = Gl.glGenLists(1);
                Gl.glNewList(displayList, Gl.GL_COMPILE);

                DrawUnitBox((renderStyle == RenderStyle.Wireframe) ? Gl.GL_LINE_LOOP : Gl.GL_QUADS);

                Gl.glEndList();
                s_cubeDisplayLists.Add(drawStyle, displayList);
            }
            Gl.glCallList(displayList);
            Gl.glPopMatrix();

            Util3D.RenderStats.VertexCount += 6 * 4;
            Util3D.RenderStats.PrimCount += 6;
        }

        /// <summary>
        /// Draws a sphere, slowly. Consider using DrawSphereDisplayList(), which has less
        /// flexibility but is much faster.</summary>
        /// <param name="radius">Sphere radius</param>
        /// <param name="slices">Number of longitudinal segments</param>
        /// <param name="stacks">Number of latitudinal segments</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawSphere(double radius, int slices, int stacks, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;
            Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
            Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
            Glu.gluSphere(s_quadric, radius, slices, stacks);

            Util3D.RenderStats.PrimCount += slices * stacks; //2 stacks are triangles; rest are quads.
            Util3D.RenderStats.VertexCount +=       //each stack is a separate tri fan / quad strip?
                (2 + slices) * 2 +                  //the two triangle fans at top and bottom of sphere.
                (2 + slices * 2) * (stacks - 2);    //the remaining stacks are quad strips.
        }

        /// <summary>
        /// Draws a sphere very quickly using an OpenGL display list. This method must not be called
        /// in between Gl.glNewList and Gl.glEndList.</summary>
        /// <param name="radius">Sphere radius</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawSphereDisplayList(float radius, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;

            Gl.glPushMatrix();
            Gl.glScalef(radius, radius, radius);

            int displayList;
            if (!s_sphereDisplayLists.TryGetValue(drawStyle, out displayList))
            {
                displayList = Gl.glGenLists(1);
                Gl.glNewList(displayList, Gl.GL_COMPILE);

                Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
                Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
                Glu.gluSphere(s_quadric, 1.0f, FAST_SPHERE_SLICES, FAST_SPHERE_STACKS);

                Gl.glEndList();
                s_sphereDisplayLists.Add(drawStyle, displayList);
            }

            Gl.glCallList(displayList);
            Gl.glPopMatrix();

            Util3D.RenderStats.PrimCount += FAST_SPHERE_SLICES * FAST_SPHERE_STACKS; //2 stacks are triangles; rest are quads.
            Util3D.RenderStats.VertexCount +=       //each stack is a separate tri fan / quad strip?
                (2 + FAST_SPHERE_SLICES) * 2 +                  //the two triangle fans at top and bottom of sphere.
                (2 + FAST_SPHERE_SLICES * 2) * (FAST_SPHERE_STACKS - 2);    //the remaining stacks are quad strips.
        }

        /// <summary>
        /// Draws a cone, without a bottom disk. For much better performance, use DrawConeDisplayList().</summary>
        /// <param name="baseRadius">Radius at base of cone</param>
        /// <param name="height">Cone height</param>
        /// <param name="slices">Number of longitudinal segments</param>
        /// <param name="stacks">Number of latitudinal segments</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawCone(double baseRadius, double height, int slices, int stacks, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;
            Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
            Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
            Glu.gluCylinder(s_quadric, baseRadius, 0.0, height, slices, stacks);

            // Note that we are not rendering the bottom. That would require a separate call to gluDisk.
            // Assuming that each stack is a quad strip. This cone could be more efficiently rendered
            // as a triangle fan.
            Util3D.RenderStats.PrimCount += slices * stacks;
            Util3D.RenderStats.VertexCount += (2 + slices * 2) * stacks;
        }

        /// <summary>
        /// Draws a cone very quickly using an OpenGL display list, with no bottom disk and
        /// 8 slices radiating from the tip. This method must not be called in between Gl.glNewList
        /// and Gl.glEndList.</summary>
        /// <param name="baseRadius">Radius at base of cone</param>
        /// <param name="height">Cone height</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawConeDisplayList(float baseRadius, float height, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;

            Gl.glPushMatrix();
            Gl.glScalef(baseRadius, baseRadius, height);

            int displayList;
            if (!s_coneDisplayLists.TryGetValue(drawStyle, out displayList))
            {
                displayList = Gl.glGenLists(1);
                Gl.glNewList(displayList, Gl.GL_COMPILE);

                Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
                Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
                Glu.gluCylinder(s_quadric, 1.0, 0.0, 1.0, FAST_CONE_SLICES, FAST_CONE_STACKS);

                Gl.glEndList();
                s_coneDisplayLists.Add(drawStyle, displayList);
            }

            Gl.glCallList(displayList);
            Gl.glPopMatrix();

            // Note that we are not rendering the bottom. That would require a separate call to gluDisk.
            // Assuming that each stack is a quad strip. This cone could be more efficiently rendered
            // as a triangle fan.
            Util3D.RenderStats.PrimCount += FAST_CONE_SLICES * FAST_CONE_STACKS;
            Util3D.RenderStats.VertexCount += (2 + FAST_CONE_SLICES * 2) * FAST_CONE_STACKS;
        }

        /// <summary>
        /// Draws a cylinder, slowly. For better performance, consider using DrawCylinderDisplayList.</summary>
        /// <param name="radius">Cylinder radius</param>
        /// <param name="height">Cylinder height</param>
        /// <param name="slices">Number of longitudinal segments</param>
        /// <param name="stacks">Number of latitudinal segments</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawCylinder(double radius, double height, int slices, int stacks, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;
            Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
            Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
            Glu.gluCylinder(s_quadric, radius, radius, height, slices, stacks);

            Gl.glPushMatrix();
            Gl.glRotatef(180.0f, 1, 0, 0);
            Glu.gluDisk(s_quadric, 0, radius, slices, 1);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(0, 0, (float)height);
            Glu.gluDisk(s_quadric, 0, radius, slices, 1);
            Gl.glPopMatrix();

            Util3D.RenderStats.PrimCount += slices * stacks + 2 * slices;
            Util3D.RenderStats.VertexCount +=
                (2 + slices * 2) * stacks + //quad strips for the sides of the cylinder
                (2 + slices) * 2;           //top and bottom triangle fans
        }

        /// <summary>
        /// Draws a cylinder very quickly using an OpenGL display list, with 10 slices along
        /// the cylinder's axis. This method must not be called in between Gl.glNewList and
        /// Gl.glEndList.</summary>
        /// <param name="radius">Cylinder radius</param>
        /// <param name="height">Cylinder height</param>
        /// <param name="renderStyle">Render style: Wireframe or Solid</param>
        public static void DrawCylinderDisplayList(float radius, float height, RenderStyle renderStyle)
        {
            int drawStyle = (renderStyle == RenderStyle.Wireframe) ? Glu.GLU_LINE : Glu.GLU_FILL;

            Gl.glPushMatrix();
            Gl.glScalef(radius, radius, height);

            int displayList;
            if (!s_cylinderDisplayLists.TryGetValue(drawStyle, out displayList))
            {
                displayList = Gl.glGenLists(1);
                Gl.glNewList(displayList, Gl.GL_COMPILE);

                Glu.gluQuadricDrawStyle(s_quadric, drawStyle);
                Glu.gluQuadricNormals(s_quadric, Glu.GLU_SMOOTH);
                Glu.gluCylinder(s_quadric, 1.0f, 1.0f, 1.0f, FAST_CYLINDER_SLICES, FAST_CYLINDER_STACKS);

                Gl.glPushMatrix();
                Gl.glRotatef(180.0f, 1, 0, 0);
                Glu.gluDisk(s_quadric, 0, 1.0f, FAST_CYLINDER_SLICES, 1);
                Gl.glPopMatrix();

                Gl.glPushMatrix();
                Gl.glTranslatef(0, 0, 1.0f);
                Glu.gluDisk(s_quadric, 0, 1.0f, FAST_CYLINDER_SLICES, 1);
                Gl.glPopMatrix();

                Gl.glEndList();
                s_cylinderDisplayLists.Add(drawStyle, displayList);
            }

            Gl.glCallList(displayList);
            Gl.glPopMatrix();

            Util3D.RenderStats.PrimCount +=
                FAST_CYLINDER_SLICES * FAST_CYLINDER_STACKS + 2 * FAST_CYLINDER_SLICES;
            Util3D.RenderStats.VertexCount +=
                (2 + FAST_CYLINDER_SLICES * 2) * FAST_CYLINDER_STACKS + //quad strips for the sides
                (2 + FAST_CYLINDER_SLICES) * 2;           //top and bottom triangle fans
        }

        /// <summary>
        /// Begins text drawing</summary>
        public static void BeginText()
        {
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(-1, 1, -1, 1, -1, 1);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
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
            Gl.glRasterPos2i(x, y);
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
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(-1, 1, -1, 1, -1, 1);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glPushAttrib(Gl.GL_LINE_BIT);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LINE_STIPPLE);
            Gl.glLineWidth(1);
            unchecked { Gl.glLineStipple(4, (short)0xAAAA); }
            SetColor(color);

            float ooWidth = 1.0f / controlSize.Width;
            float ooHeight = 1.0f / controlSize.Height;
            float x1 = first.X * ooWidth * 2 - 1;
            float x2 = current.X * ooWidth * 2 - 1;
            float y1 = first.Y * ooHeight * -2 + 1;
            float y2 = current.Y * ooHeight * -2 + 1;

            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glVertex2f(x1, y1);
            Gl.glVertex2f(x2, y1);
            Gl.glVertex2f(x2, y2);
            Gl.glVertex2f(x1, y2);
            Gl.glEnd();
            Gl.glPopAttrib();
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
                return Gl.glGetString(Gl.GL_VENDOR);
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
            Gl.glMultMatrixf(CastToIntPtr(m));
        }

        /// <summary>
        /// Loads matrix as the current matrix</summary>
        /// <param name="m">Matrix to load</param>
        public static void glLoadMatrixf(Matrix4F m)
        {
            Gl.glLoadMatrixf(CastToIntPtr(m));
        }

        /// <summary>
        /// Gets state variable which was cast as float</summary>
        /// <param name="pname">Enum for state variable to be returned</param>
        /// <param name="m">State variable as Matrix4F</param>
        public static void glGetFloatv(int pname, Matrix4F m)
        {
            Gl.glGetFloatv(pname, CastToIntPtr(m));
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
                case Gl.GL_NO_ERROR:
                    errorText = "No error has been recorded.";
                    break;
                case Gl.GL_INVALID_ENUM:
                    errorText = "An unacceptable value is specified for an enumerated argument.";
                    break;
                case Gl.GL_INVALID_VALUE:
                    errorText = "A numeric argument is out of range.";
                    break;
                case Gl.GL_INVALID_OPERATION:
                    errorText = "The specified operation is not allowed in the current state.";
                    break;
                case Gl.GL_STACK_OVERFLOW:
                    errorText = "This command would cause a stack overflow.";
                    break;
                case Gl.GL_STACK_UNDERFLOW:
                    errorText = "This command would cause a stack underflow.";
                    break;
                case Gl.GL_OUT_OF_MEMORY:
                    errorText =
                        "There is not enough memory left to execute the command.\n" +
                        "The state of the GL is undefined,\n" +
                        "except for the state of the error flags,\n" +
                        "after this error is recorded.";
                    break;
                case Gl.GL_TABLE_TOO_LARGE:
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
            while ((errorId = Gl.glGetError()) != 0)
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
                Gl.glBegin(type);
                Gl.glNormal3fv(n[i]);
                Gl.glVertex3fv(v[faces[i, 0]]);
                Gl.glVertex3fv(v[faces[i, 1]]);
                Gl.glVertex3fv(v[faces[i, 2]]);
                Gl.glVertex3fv(v[faces[i, 3]]);
                Gl.glEnd();
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

        private static readonly Glu.GLUquadric s_quadric = Glu.gluNewQuadric();
        
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
