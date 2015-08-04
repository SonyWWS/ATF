//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Direct2D;

namespace CircuitEditorSample
{
    /// <summary>
    /// A custom circuit renderer, demonstrating how to use a particular theme (D2dDiagramTheme)
    /// for particular types of circuit elements.</summary>
    public class CircuitRenderer : D2dCircuitRenderer<Module, Connection, ICircuitPin>
    {
        /// <summary>
        /// Initializes a new instance of this class</summary>
        /// <param name="theme">Diagram theme for rendering graph</param>
        /// <param name="documentRegistry">An optional document registry, used to clear the internal
        /// element type cache when a document is removed</param>
        public CircuitRenderer(D2dDiagramTheme theme, IDocumentRegistry documentRegistry) :
            base(theme, documentRegistry)
        {
            m_disabledTheme = new D2dDiagramTheme();
            m_disabledTheme.FillBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDark);
            m_disabledTheme.TextBrush = D2dFactory.CreateSolidBrush(SystemColors.InactiveCaption);
            D2dGradientStop[] gradstops = 
                { 
                    new D2dGradientStop(Color.DarkGray, 0),
                    new D2dGradientStop(Color.DimGray, 1.0f),
                };
            m_disabledTheme.FillGradientBrush = D2dFactory.CreateLinearGradientBrush(gradstops);

            // Set the pin colors
            m_disabledTheme.RegisterCustomBrush("boolean", D2dFactory.CreateSolidBrush(Color.LightGray));
        }

        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="element">Element to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(Module element, DiagramDrawingStyle style, D2dGraphics g)
        {
            // Use the "disabled" theme when drawing disabled circuit elements.
            if (!((ModuleElementInfo)element.ElementInfo).Enabled)
            {
                D2dDiagramTheme defaultTheme = Theme;
                Theme = m_disabledTheme;
                base.Draw(element, style, g);
                Theme = defaultTheme;
                return;
            }

            base.Draw(element, style, g);
        }

        private readonly D2dDiagramTheme m_disabledTheme;
    }
}
