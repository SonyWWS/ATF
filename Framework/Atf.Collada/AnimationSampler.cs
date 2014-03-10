//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// Sampler of COLLADA animation</summary>
    class AnimationSampler : DomNodeAdapter, IAnimData
    {

        #region IAnimData Members

        /// <summary>
        /// Gets or sets the duration</summary>
        public float Duration
        {
            get
            {
                float min;
                float max = min = KeyTimes[0];
                foreach (float time in KeyTimes)
                {
                    if (time > max) max = time;
                    if (time < min) min = time;
                }

                return max - min;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the interpolation type string</summary>
        public string InterpolationType
        {
            get { return m_interpolation.NameData[0]; }
            set
            {
                int count = m_interpolation.NameData.Length;
                m_interpolation.NameData = Enumerable.Repeat(value, count).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the key stride</summary>
        public int KeyStride
        {
            get { return m_output.stride; }
            set {  }
        }

        /// <summary>
        /// Gets or sets the key time array</summary>
        public float[] KeyTimes
        {
            get { return m_input.Data; }
            set { m_input.Data = value; }
        }

        /// <summary>
        /// Gets or sets the child AnimData list</summary>
        public float[] KeyValues
        {
            get { return m_output.Data; }
            set { m_output.Data = value; }
        }

        /// <summary>
        /// Gets or sets the number of keys</summary>
        public int NumKeys
        {
            get { return KeyTimes.Length; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Get or sets the tangent array</summary>
        public float[] Tangents
        {
            get { return m_inTangent.Data; }
            set { m_inTangent.Data = value; }
        }

        /// <summary>
        /// Gets or sets the time offset</summary>
        public float TimeOffset { get; set; }

        #endregion

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            TimeOffset = 0;

            Parse();
        }

        private void Parse()
        {
            foreach (DomNode input in GetChildList<DomNode>(Schema.sampler.inputChild))
            {
                string semantic = input.GetAttribute(Schema.InputLocal.semanticAttribute) as string;
                Source source = input.GetAttribute(Schema.InputLocal.sourceAttribute).As<Source>();

                if (semantic == "INPUT")
                    m_input = source;
                else if (semantic == "OUTPUT")
                    m_output = source;
                else if (semantic == "IN_TANGENT")
                    m_inTangent = source;
                else if (semantic == "OUT_TANGENT")
                    m_outTangent = source;
                else if (semantic == "INTERPOLATION")
                    m_interpolation = source;
            }
        }

        private Source m_input;
        private Source m_inTangent;
        private Source m_interpolation;
        private Source m_output;
        private Source m_outTangent;
    }
}
