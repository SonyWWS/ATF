//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls.DataEditing
{
    /// <summary>
    /// Editing boolean value using a check box representation</summary>
    public class BoolDataEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringDataEditor"/> class.</summary>
        /// <param name="theme">The visual theme to use</param>
        public BoolDataEditor(DataEditorTheme theme)
            : base(theme)
        {
        }

        /// <summary>
        /// The boolean value to display and edit.</summary>
        public bool Value;

        /// <summary>
        /// Parses the specified string representation and sets the data value.</summary>
        /// <param name="s">String to parse</param>
        public override void Parse(string s)
        {
            bool boolResult;
            if (bool.TryParse(s, out boolResult))
                Value = boolResult;
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }

        /// <summary>
        /// Implement the display of the value's representation.</summary>
        /// <param name="g">The Graphics object</param>
        /// <param name="area">Rectangle delimiting area to paint</param>
        public override void PaintValue(Graphics g, Rectangle area)
        {
            area.X += Theme.Padding.Left;
            area.Width -= Theme.Padding.Left + Theme.Padding.Right;
            const int checkBoxWidth = 12;
            if (area.Width >= checkBoxWidth)
            {
                // area.X += (area.Width - checkBoxWidth)/2;         
                CheckBoxState state = Value ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(g, area.Location, state);             
            }
        }

        /// <summary>
        /// Determines the editing mode from input position.</summary>
        /// <param name="p">Input position point</param>
        public override void SetEditingMode(Point p)
        {
            if (Bounds.Contains(p))
                EditingMode = EditMode.ByClick;
            else
                EditingMode = EditMode.None;
        }


        /// <summary>
        /// Performs custom actions when the user clicks the editor with either mouse button.</summary>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
            Value = !Value;
        }

        /// <summary>
        /// Begins an edit operation.</summary>
        public override void BeginDataEdit()
        {
            m_startValue = Value;
        }

        /// <summary>
        /// Ends an edit operation.</summary>
        /// <returns>
        /// 'true' if the change should be committed and 'false' if the change should be discarded</returns>
        public override bool EndDataEdit()
        {
            if (EditingMode == EditMode.ByClick)
                return m_startValue != Value;
            return false;
        }

        private bool m_startValue;
    }
}
