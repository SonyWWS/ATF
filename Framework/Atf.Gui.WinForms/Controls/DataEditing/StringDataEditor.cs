//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Class to edit a string value using a Textbox.</summary>
    public class StringDataEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringDataEditor"/> class.</summary>
        /// <param name="theme">The visual theme to use</param>
        public StringDataEditor(DataEditorTheme theme)
            : base(theme)
        {
        }

        /// <summary>
        /// The string value to display and edit.</summary>
        public string Value;

        /// <summary>
        /// Layout: Measures the desired layout size of the data value and any child UI elements.</summary>
        /// <param name="g">Graphics that can be used for measuring strings</param>
        /// <param name="availableSize">The available size that this editor can give to child UI elements</param>
        /// <returns>
        /// The size that this editor determines it needs during layout, based on its calculations of child element sizes</returns>
        public override SizeF Measure(Graphics g,  SizeF availableSize)
        {
            SizeF size = g.MeasureString(Value, Theme.Font);
            size.Width += Theme.Padding.Left;
            return size;
        }


        /// <summary>
        /// Implement the display of the value's representation.</summary>
        /// <param name="g">The Graphics object</param>
        /// <param name="area">Rectangle delimiting area to paint</param>
        public override void PaintValue(Graphics g, Rectangle area)
        {
            if (ReadOnly)
                g.DrawString(Value, Theme.Font, Theme.ReadonlyBrush, area.Left + Theme.Padding.Left, area.Top); 
            else
                g.DrawString(Value, Theme.Font, Theme.TextBrush, area.Left + Theme.Padding.Left, area.Top); 
        }

        /// <summary>
        /// Determines the editing mode from input position.</summary>
        /// <param name="p">Input position point</param>
        public override void SetEditingMode(Point p)
        {
            if (Bounds.Contains(p))
                EditingMode = EditMode.ByTextBox;
        }

        /// <summary>
        /// Begins an edit operation.</summary>
        public override void BeginDataEdit()
        {
            if (EditingMode == EditMode.ByTextBox)
            {
                TextBox.Text = Value;
                TextBox.Bounds = Bounds;
                TextBox.SelectAll();
                TextBox.Show();
                TextBox.Focus();
            }
        }

        /// <summary>
        /// Ends an edit operation.</summary>
        /// <returns>
        /// 'true' if the change should be committed and 'false' if the change should be discarded</returns>
        public override bool EndDataEdit()
        {
            if (EditingMode == EditMode.ByTextBox)
            {
                Parse(TextBox.Text);
            }
            return true;
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Parses the specified string representation and sets the data value.</summary>
        /// <param name="s">String to parse</param>
        public override void Parse(string s)
        {
            Value = s;
        }

    }
}