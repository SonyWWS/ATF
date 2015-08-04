//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Abstract base class that can provide a user interface (UI) for representing and editing 
    /// the values of objects of the supported data types.
    /// 
    /// To implement a custom data editor, you must at least perform the following tasks:
    ///  • Define a class that derives from DataEditor
    ///  • Override the Measure method to inform the parent control how much screen space it would like to have
    ///  • Override the PaintValue method to implement the display of the value's representation</summary>
    public abstract class DataEditor
    {
        /// <summary>
        /// Enumeration for editing mode.</summary>
        public enum EditMode
        {
            None,
            ByTextBox, // editing through the default text box
            ByClick,   // editing that occurs when the data is left-clicked, such as bool editor
            BySlider,  // editing by moving a value indicator
            ByExternalControl // editing that uses external, arbitrary control 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEditor"/> class.</summary>
        /// <param name="theme">The visual theme</param>
        protected DataEditor(DataEditorTheme theme)
        {
            m_theme = theme;
            EditingMode = EditMode.None;
        }

        /// <summary>
        /// Callback at the end of cell data editing. For cell editing that uses an external control,
        /// this callback allows the parent control to finish the cell edit operation.</summary>
        public Action FinishDataEdit;

        /// <summary>
        /// Gets the visual theme.</summary>
        public DataEditorTheme Theme { get { return m_theme; } }

        /// <summary>
        /// Determines whether the data editor wants to track mouse movement.</summary>
        /// <returns>Whether data editor wants to track mouse movement</returns>
        public virtual bool WantsMouseTracking()
        {
           return false; 
        }

        /// <summary>
        /// Layout: Measures the desired layout size of the data value and any child UI elements.</summary>
        /// <param name="g">Graphics that can be used for measuring strings</param>
        /// <param name="availableSize">The available size that this editor can give to child UI elements</param>
        /// <returns>The size that this editor determines it needs during layout, based on its calculations of child element sizes</returns>
        public virtual SizeF Measure(Graphics g,  SizeF availableSize)
        {
            return availableSize;
        }

        /// <summary>
        /// Sets the final layout and size for the data value and any child UI elements.</summary>
        /// <param name="finalSize">Size of final area within the parent that this element should use to arrange itself and its children</param>
        /// <returns>Size of final area within the parent that this element should use to arrange itself</returns>
        public virtual SizeF Arrange(SizeF finalSize)
        {
            return finalSize;
        }

        /// <summary>
        /// Implement the display of the value's representation.</summary>
        /// <param name="g">The Graphics object</param>
        /// <param name="area">Rectangle delimiting area to paint</param>
        public virtual void PaintValue(Graphics g,  Rectangle area)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating how to begin editing the data.</summary>
        public EditMode EditingMode { get; set; }

        /// <summary>
        /// Gets or sets a text box that allows the user to enter text.</summary>
        /// <remarks>Text editing mode is the fallback mode for any type.</remarks>
        public TextBox TextBox { get; set; }

        /// <summary>
        /// Gets or sets the size and location of the editor, in pixels, relative to the parent control.</summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Determines the editing mode from input position.</summary>
        /// <param name="p">Input position point</param>
        public virtual void SetEditingMode(Point p)
        {
        }
     
        /// <summary>
        /// Begins an edit operation.</summary>
        public virtual void BeginDataEdit()
        {
        }

        /// <summary>
        /// Ends an edit operation.</summary>
        /// <returns>'true' if the change should be committed and 'false' if the change should be discarded</returns>
        public virtual bool EndDataEdit()
        {
            return false;
        }

        /// <summary>
        /// Performs custom actions when moving the mouse pointer over this editor.</summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data</param>
        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions when the user clicks the editor with either mouse button.</summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data</param>
        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Parses the specified string representation and sets the data value.</summary>
        /// <param name="s">String to parse</param>
        public abstract void Parse(string s);

        /// <summary>
        /// The object that owns the editing data value.</summary>
        public object Owner;

        /// <summary>
        /// The column name of the data value; should be unique among columns.</summary>
        public string Name;

        /// <summary>
        /// Indicates whether this data value is read-only or not.</summary>
        public bool ReadOnly;

        private readonly DataEditorTheme m_theme;
    }
}

