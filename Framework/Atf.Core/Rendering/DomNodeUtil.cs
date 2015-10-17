//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Utility methods for storing graphics-related data in DomNodes</summary>
    public static class DomNodeUtil
    {
        /// <summary>
        /// Gets the DomNode attribute as a Vec3F. The attribute must exist on the DomNode.</summary>
        /// <param name="domNode">DomNode holding the attribute</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <returns>Attribute as a Vec3F</returns>
        public static Vec3F GetVector(DomNode domNode, AttributeInfo attribute)
        {
            return new Vec3F((float[])domNode.GetAttribute(attribute));
        }

        /// <summary>
        /// Gets the DomNode attribute as a Vec3F and returns true, or returns false if the attribute
        /// doesn't exist</summary>
        /// <param name="domNode">DomNode holding the attribute</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <param name="result">The resulting Vec3F. Is (0,0,0) if the attribute couldn't be found</param>
        /// <returns><c>True</c> if the attribute was found and was converted to a Vec3F</returns>
        public static bool GetVector(DomNode domNode, AttributeInfo attribute, out Vec3F result)
        {
            float[] floats = domNode.GetAttribute(attribute) as float[];
            if (floats != null)
            {
                result = new Vec3F(floats);
                return true;
            }

            result = new Vec3F();
            return false;
        }

        /// <summary>
        /// Sets the DomNode attribute to the given Vec3F</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <param name="v">DomNode attribute as Vec3F</param>
        public static void SetVector(DomNode domNode, AttributeInfo attribute, Vec3F v)
        {
            domNode.SetAttribute(attribute, v.ToArray());
        }

        /// <summary>
        /// Gets the DomNode attribute as a Matrix4F</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <returns>DomNode attribute as a Matrix4F</returns>
        public static Matrix4F GetMatrix(DomNode domNode, AttributeInfo attribute)
        {
            return new Matrix4F((float[])domNode.GetAttribute(attribute));
        }

        /// <summary>
        /// Sets the DomNode attribute to the given Matrix4F</summary>
        /// <param name="domNode">DomNode holding attribute</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <param name="m">Matrix4F value to be set</param>
        public static void SetMatrix(DomNode domNode, AttributeInfo attribute, Matrix4F m)
        {
            domNode.SetAttribute(attribute, m.ToArray());
        }

        /// <summary>
        /// Gets the DomNode attribute as a Sphere3F</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <returns>DomNode attribute as a Sphere3F</returns>
        public static Sphere3F GetSphere(DomNode domNode, AttributeInfo attribute)
        {
            Sphere3F s = new Sphere3F();
            float[] value = domNode.GetAttribute(attribute) as float[];
            if (value != null)
            {
                s.Center = new Vec3F(value[0], value[1], value[2]);
                s.Radius = value[3];
            }
            return s;
        }

        /// <summary>
        /// Sets the DomNode attribute value to the given Sphere3F</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <param name="s">Sphere3F value to be set</param>
        public static void SetSphere(DomNode domNode, AttributeInfo attribute, Sphere3F s)
        {
            float[] value = new float[4];
            value[0] = s.Center.X;
            value[1] = s.Center.Y;
            value[2] = s.Center.Z;
            value[3] = s.Radius;
            domNode.SetAttribute(attribute, value);
        }

        /// <summary>
        /// Gets the DomNode attribute as a Box</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <returns>DomNode attribute value as a Box</returns>
        public static Box GetBox(DomNode domNode, AttributeInfo attribute)
        {
            float[] value = domNode.GetAttribute(attribute) as float[];
            if (value != null)
            {
                return new Box(new Vec3F(value[0], value[1], value[2]),
                               new Vec3F(value[3], value[4], value[5]));
            }
            else return new Box();
        }

        /// <summary>
        /// Sets the DomNode attribute value to the given Box</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">Attribute of the DomNode that contains the data</param>
        /// <param name="b">Box</param>
        public static void SetBox(DomNode domNode, AttributeInfo attribute, Box b)
        {
            float[] value = new float[6];
            value[0] = b.Min.X;
            value[1] = b.Min.Y;
            value[2] = b.Min.Z;
            value[3] = b.Max.X;
            value[4] = b.Max.Y;
            value[5] = b.Max.Z;
            domNode.SetAttribute(attribute, value);
        }
    }
}
