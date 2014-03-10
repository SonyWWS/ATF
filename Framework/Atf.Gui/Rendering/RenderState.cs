//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// RenderState is an extensible platform-independent representation of a GPU render state.
    /// GPU state is represented by a combination of flags (RenderMode) and values that accompany flags (TextureName property).
    /// A RenderState is committed to the graphics device driver layer by way of RenderStateGuardian.Commit().
    /// RenderStates can be composed from parents. This behavior is controlled by the InheritState and OverrideChildState properties.</summary>
    public class RenderState : ICloneable
    {
        /// <summary>
        /// Constructor</summary>
        public RenderState()
        {
        }

        /// <summary>
        /// Constructor that copies another RenderState</summary>
        /// <param name="other">RenderState to copy</param>
        public RenderState(RenderState other)
        {
            Init(other);
        }

        /// <summary>
        /// Initializes this object from the given RenderState</summary>
        /// <param name="other">RenderState to copy</param>
        public void Init(RenderState other)
        {
            m_overrideChildState = other.m_overrideChildState;
            m_renderMode = other.m_renderMode;
            m_inheritState = other.m_inheritState;
            m_wireframeColor = other.m_wireframeColor;
            m_diffuseColor = other.m_diffuseColor;
            m_emissionColor = other.m_emissionColor;
            m_ambientColor = other.m_ambientColor;
            m_specularColor = other.m_specularColor;
            m_shininess = other.m_shininess;
            m_textureName = other.m_textureName;
            m_lineThickness = other.m_lineThickness;
        }

        /// <summary>
        /// Sets this RenderState to a combination of itself and the specified parent.
        /// The InheritState property of this RenderState and the OverrideChildState property of the parent
        /// are considered.</summary>
        /// <param name="parent">Parent RenderState to compose from</param>
        public virtual void ComposeFrom(RenderState parent)
        {
            RenderMode bitsFromChild =
                ((~parent.OverrideChildState & ~InheritState) | OverrideChildState) & RenderMode;

            RenderMode overrideFromParent = parent.OverrideChildState & ~OverrideChildState;
            RenderMode bitsFromParent = (overrideFromParent | InheritState) & parent.RenderMode;

            RenderMode = bitsFromChild | bitsFromParent;

            bool inheritSolidColor = ((overrideFromParent & RenderMode.SolidColor) != 0) ||
                                     ((InheritState & RenderMode.SolidColor) != 0);

            bool inheritWireColor = ((overrideFromParent & RenderMode.WireframeColor) != 0) ||
                                    ((InheritState & RenderMode.WireframeColor) != 0);

            bool inheritWireThickness = ((overrideFromParent & RenderMode.WireframeThickness) != 0) ||
                                        ((InheritState & RenderMode.WireframeThickness) != 0);

            bool parentIsTextured = ((parent.RenderMode & RenderMode.Textured) != 0);

            if (inheritSolidColor)
                SolidColor = parent.SolidColor;

            if (inheritSolidColor)
                SpecularColor = parent.SpecularColor;

            if (inheritWireColor)
                WireframeColor = parent.WireframeColor;

            if (inheritWireThickness)
                LineThickness = parent.LineThickness;

            if (parentIsTextured && (m_textureName == 0))
                m_textureName = parent.TextureName;
        }

        /// <summary>
        /// Commits each RenderMode bit to the specified RenderStateGuardian.
        /// Child RenderState classes can derive this to publish their extension bits.</summary>
        /// <param name="rsg">RenderStateGuardian to publish to</param>
        public virtual void CommitAllBitsToGuardian(RenderStateGuardian rsg)
        {
            for (int i = 0; (1<<i) <= (int)RenderMode.Max; i++)
                rsg.SetRenderStateByIndex(i, this);
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance</summary>
        /// <returns>New object that is a copy of this instance</returns>
        public virtual object Clone()
        {
            return new RenderState(this);
        }

        #endregion

        /// <summary>
        /// Gets or sets whether or not a child should be overridden</summary>
        public RenderMode OverrideChildState
        {
            get { return m_overrideChildState; }
            set { m_overrideChildState = value; }
        }

        /// <summary>
        /// Gets or sets the render mode</summary>
        public RenderMode RenderMode
        {
            get { return m_renderMode; }
            set { m_renderMode = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the RenderMode that is inherited</summary>
        public RenderMode InheritState
        {
            get { return m_inheritState; }
            set { m_inheritState = value; }
        }

        /// <summary>
        /// Gets or sets the solid color (aka DiffuseColor) as a Vec4F</summary>
        public Vec4F SolidColor
        {
            get { return m_diffuseColor; }
            set { m_diffuseColor = value; }
        }

        /// <summary>
        /// Gets or sets the wireframe color as a Vec4F</summary>
        public Vec4F WireframeColor
        {
            get { return m_wireframeColor; }
            set { m_wireframeColor = value; }
        }

        /// <summary>
        /// Gets or sets the diffuse color as a Vec4F</summary>
        public Vec4F DiffuseColor
        {
            get { return m_diffuseColor; }
            set { m_diffuseColor = value; }
        }

        /// <summary>
        /// Gets or sets the emission color as a Vec4F</summary>
        public Vec4F EmissionColor
        {
            get { return m_emissionColor; }
            set { m_emissionColor = value; }
        }

        /// <summary>
        /// Gets or sets the ambient color as a Vec4F</summary>
        public Vec4F AmbientColor
        {
            get { return m_ambientColor; }
            set { m_ambientColor = value; }
        }

        /// <summary>
        /// Gets or sets the specular color as a Vec4F</summary>
        public Vec4F SpecularColor
        {
            get { return m_specularColor; }
            set { m_specularColor = value; }
        }

        /// <summary>
        /// Gets or sets the shininess</summary>
        public float Shininess
        {
            get { return m_shininess; }
            set { m_shininess = value; }
        }

        /// <summary>
        /// Gets or sets the texture value</summary>
        public int TextureName
        {
            get { return m_textureName; }
            set { m_textureName = value; }
        }

        /// <summary>
        /// Gets or sets the line thickness, in pixels</summary>
        public int LineThickness
        {
            get { return m_lineThickness; }
            set { m_lineThickness = value; }
        }

        private RenderMode m_renderMode;
        private RenderMode m_inheritState;
        private RenderMode m_overrideChildState;

        private Vec4F m_wireframeColor = new Vec4F(0, 0, 0, 1);
        private Vec4F m_diffuseColor = new Vec4F(0, 0, 0, 1);
        private Vec4F m_emissionColor = new Vec4F(0, 0, 0, 1);
        private Vec4F m_ambientColor = new Vec4F(0, 0, 0, 1);
        private Vec4F m_specularColor = new Vec4F(0, 0, 0, 1);
        private float m_shininess;
        private int m_textureName;
        private int m_lineThickness = 1;
    }
}
