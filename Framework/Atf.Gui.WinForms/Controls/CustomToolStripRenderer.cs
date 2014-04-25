//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Extends .NET ToolStripProfessionalRenderer class, which 
    /// handles painting functionality for ToolStrip objects, 
    /// applying a custom palette and a streamlined style.
    /// Allows specifying the ProfessionalColorTable and arrow color.</summary>
    public class CustomToolStripRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>
        /// Constructor</summary>
        public CustomToolStripRenderer()
        {
        }

        /// <summary>
        /// Constructor with color table</summary>
        /// <param name="professionalColorTable">ProfessionalColorTable</param>
        public CustomToolStripRenderer(ProfessionalColorTable professionalColorTable)
            : base(professionalColorTable)
        {
        }

        /// <summary>
        /// Gets or sets the arror color</summary>
        public Color ArrowColor
        {
            get;
            set;
        }

        /// <summary>
        /// Raises RenderArrow event. Performs custom actions when arrow rendered.</summary>
        /// <param name="e">ToolStrip arrow render event args</param>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ArrowColor;
            base.OnRenderArrow(e);
        } 
    }
}
