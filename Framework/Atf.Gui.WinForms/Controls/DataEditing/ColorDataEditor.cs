//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Sce.Atf.Controls.DataEditing
{
    /// <summary>
    /// Editing color value using Microsoft's ColorDialog to visually pick a color.</summary>
    /// <remarks>Assume the color is stored as an ARGB int.</remarks>
    public class ColorDataEditor: DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDataEditor"/> class.</summary>
        /// <param name="theme">The visual theme</param>
        public ColorDataEditor(DataEditorTheme theme)
            : base(theme)
        {
        }

        /// <summary>
        /// The color value to display and edit.</summary>
        public Color Value;

        /// <summary>
        /// Parses the specified string representation and sets the data value.</summary>
        /// <param name="s">String to parse</param>
        public override void Parse(string s)
        {
            int color;
            Value = int.TryParse(s, out color) ? Color.FromArgb(color) : Color.FromName(s);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Value.ToArgb().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Implement the display of the value's representation.</summary>
        /// <param name="g">The Graphics object</param>
        /// <param name="area">Rectangle delimiting area to paint</param>
        public override void PaintValue(Graphics g, Rectangle area)
        {
            Theme.SolidBrush.Color = Value;
            area.X += Theme.Padding.Left;
            area.Width -= Theme.Padding.Left + Theme.Padding.Right;
            if (area.Width >0)
                g.FillRectangle(Theme.SolidBrush, area);
        }

        /// <summary>
        /// Determines the editing mode from input position.</summary>
        /// <param name="p">Input position point</param>
        public override void SetEditingMode(Point p)
        {
            if (Bounds.Contains(p))
                EditingMode = EditMode.ByExternalControl;
            else 
                EditingMode = EditMode.None;
        }

        /// <summary>
        /// Begins an edit operation.</summary>
        public override void BeginDataEdit()
        {
            m_startValue = Value;
            if (EditingMode == EditMode.ByExternalControl)
            {
                var colorDialog = new ColorDialog();
                colorDialog.SolidColorOnly = false;
                colorDialog.AnyColor = true;
                colorDialog.CustomColors = s_customColors;

                // Sets the initial color select to the current color.
                colorDialog.Color = Value;

                // Update the text box color if the user clicks OK  
                // TODO: position the dialog at the cell location. 
                var result = colorDialog.ShowDialog();
                s_customColors = colorDialog.CustomColors;
                if (result == DialogResult.OK)
                    Value = colorDialog.Color;

                if (FinishDataEdit != null)
                    FinishDataEdit();
            }
        }

        /// <summary>
        /// Ends an edit operation.</summary>
        /// <returns>
        /// 'true' if the change should be committed and 'false' if the change should be discarded</returns>
        public override bool EndDataEdit()
        {         
            return (m_startValue != Value);
        }

        private Color m_startValue;

        private static int[] s_customColors = new int[] {};
    }
}
