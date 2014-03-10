//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA static tools</summary>
    public static class Tools
    {
        /// <summary>
        /// Creates effects dictionary</summary>
        /// <param name="bindMtrl">Bind material DomNode</param>
        /// <returns>Effects dictionary of string/Effect pairs</returns>
        public static Dictionary<string, Effect> CreateEffectDictionary(DomNode bindMtrl)
        {
            if (bindMtrl == null)
                return null;

            var result = new Dictionary<string, Effect>();

            DomNode techCommon = bindMtrl.GetChild(Schema.bind_material.technique_commonChild);

            IList<DomNode> instMtrls = techCommon.GetChildList(Schema.bind_material_technique_common.instance_materialChild);
            foreach (DomNode instMtrl in instMtrls)
            {
                DomNode material = instMtrl.GetAttribute(Schema.instance_material.targetAttribute).As<DomNode>();
                DomNode instEffect = material.GetChild(Schema.material.instance_effectChild);
                Effect effect = instEffect.GetAttribute(Schema.instance_effect.urlAttribute).As<Effect>();

                string symbol = instMtrl.GetAttribute(Schema.instance_material.symbolAttribute) as string;
                if (!String.IsNullOrEmpty(symbol))
                    result.Add(symbol, effect);
            }

            return (result.Count > 0) ? result : null;
        }

        /// <summary>
        /// Finds DomNode with ID in tree containing give DomNode</summary>
        /// <param name="id">ID to find</param>
        /// <param name="domNode">DomNode in tree being searched</param>
        /// <returns>DomNode with ID in tree containing give DomNode</returns>
        public static DomNode FindNode(string id, DomNode domNode)
        {
            if (s_nodeDictionary == null || !s_lastRoot.Equals(domNode.GetRoot()))
            {
                s_nodeDictionary = new Dictionary<string, DomNode>();
                s_lastRoot = domNode.GetRoot();

                foreach (DomNode node in domNode.GetRoot().Subtree)
                    foreach (AttributeInfo attribute in node.Type.Attributes)
                        if (attribute.Name == "id")
                        {
                            string value = node.GetAttribute(attribute) as string;
                            if (!String.IsNullOrEmpty(value))
                                s_nodeDictionary[value] = node;
                            break;
                        }
            }

            return s_nodeDictionary[id];
        }

        /// <summary>
        /// Returns DomNode's color components in array</summary>
        /// <param name="domNode">DomNode whose color is obtained</param>
        /// <returns>Array of DomNode's color components</returns>
        public static float[] GetColor(DomNode domNode)
        {
            float[] value = null;
            if (domNode != null)
            {
                DomNode color = domNode.GetChild(Schema.common_color_or_texture_type.colorChild);
                if (color != null)
                    value = DoubleToFloat(color.GetAttribute(Schema.common_color_or_texture_type_color.Attribute) as double[]);
            }

            return value;
        }

        /// <summary>
        /// Gets float value associated with DomNode</summary>
        /// <param name="domNode">DomNode whose value is obtained</param>
        /// <returns>Float value associated with DomNode</returns>
        public static float GetFloat(DomNode domNode)
        {
            float value = default(float);
            if (domNode != null)
            {
                DomNode floatChild = domNode.GetChild(Schema.common_float_or_param_type.floatChild);
                if (floatChild != null)
                {
                    double v = (double)floatChild.GetAttribute(Schema.common_float_or_param_type_float.Attribute);
                    value = (float)v;
                }                
            }
            return value;
        }

        /// <summary>
        /// Gets Matrix4F associated with DomNode attribute</summary>
        /// <param name="domNode">DomNode</param>
        /// <param name="attribute">Attribute for Matrix4F</param>
        /// <returns>Matrix4F associated with DomNode attribute</returns>
        public static Matrix4F GetMatrix(DomNode domNode, AttributeInfo attribute)
        {
            return new Matrix4F(DoubleToFloat(domNode.GetAttribute(attribute) as double[]));
        }

        /// <summary>
        /// Gets Vec3F associated with DomNode attribute</summary>
        /// <param name="domNode">DomNode</param>
        /// <param name="attribute">Attribute for Vec3F</param>
        /// <returns>Vec3F associated with DomNode attribute</returns>
        public static Vec3F GetVector3(DomNode domNode, AttributeInfo attribute)
        {
            return new Vec3F(DoubleToFloat(domNode.GetAttribute(attribute) as double[]));
        }

        /// <summary>
        /// Gets Vec4F associated with DomNode attribute</summary>
        /// <param name="domNode">DomNode</param>
        /// <param name="attribute">Attribute for Vec4F</param>
        /// <returns>Vec4F associated with DomNode attribute</returns>
        public static Vec4F GetVector4(DomNode domNode, AttributeInfo attribute)
        {
            return new Vec4F(DoubleToFloat(domNode.GetAttribute(attribute) as double[]));
        }

        /// <summary>
        /// Gets value to convert degrees to radians</summary>
        public static float DegreeToRadian
        {
            get { return ((float)Math.PI / 180.0f); }
        }

        /// <summary>
        /// Gets value to convert radians to degrees</summary>
        public static float RadianToDegree
        {
            get { return (180.0f / (float)Math.PI); }
        }

        /// <summary>
        /// Converts double array to float array</summary>
        /// <param name="array">Double array</param>
        /// <returns>Float array</returns>
        public static float[] DoubleToFloat(double[] array)
        {
            int length = array.Length;
            var result = new float[length];
            for (int i = 0; i < length; i++)
                result[i] = (float)array[i];
            return result;
        }

        /// <summary>
        /// Converts ulong array to int array</summary>
        /// <param name="array">Ulong array</param>
        /// <returns>Int array</returns>
        public static int[] ULongToInt(ulong[] array)
        {
            int length = array.Length;
            var result = new int[length];
            for (int i = 0; i < length; ++i)
                result[i] = (int)array[i];
            return result;
        }

        private static DomNode s_lastRoot;
        private static Dictionary<string, DomNode> s_nodeDictionary;
    }
}
