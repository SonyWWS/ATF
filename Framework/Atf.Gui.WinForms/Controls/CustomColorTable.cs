//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// This class can be used to create a new ToolStripProfessionalRenderer
    /// (.NET class that handles painting functionality for ToolStrip objects, 
    /// applying a custom palette and a streamlined style).
    /// Normally, you can only change color settings by creating a custom class
    /// that derives from ToolStripProfessionalRenderer and another that derives
    /// from ProfessionalColorTable. To prevent that hassle, you can use this
    /// class and set the properties you like, then feed it to the overloaded
    /// ToolStripProfessionalRenderer(ProfessionalColorTable) constructor.</summary>
    /// <remarks>Some of the methods here are the same as in ProfessionalColorTable, where the descriptions are copied from. For more information on ProfessionalColorTable, 
    /// see http://msdn.microsoft.com/en-us/library/system.windows.forms.professionalcolortable%28v=vs.110%29.aspx. </remarks>
    public class CustomColorTable : ProfessionalColorTable
    {
        /// <summary>
        /// Constructor that instantiates and sets up ProfessionalColorTable object</summary>
        public CustomColorTable()
        {
            var allPropInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var properyMap = new Dictionary<string, PropertyInfo>();
            int count = "Settable".Length;
            foreach (var propInfo in allPropInfos)
            {
                if (propInfo.PropertyType != typeof(Color)) continue;
                if (propInfo.Name.StartsWith("Settable"))
                {
                    string name = propInfo.Name.Remove(0, count);
                    properyMap.Add(name, propInfo);

                }
            }

            ProfessionalColorTable colorTable = new ProfessionalColorTable();
            PropertyInfo[] originals = colorTable.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var propInfo in originals)
            {
                PropertyInfo settable;
                if (properyMap.TryGetValue(propInfo.Name, out settable))
                {
                    settable.SetValue(this, propInfo.GetValue(colorTable, null), null);
                }
            }

            SettableToolStripBorder = Color.Transparent;
           
            SettableImageMarginGradientBegin = Color.Transparent;
            SettableImageMarginGradientMiddle = Color.Transparent;
            SettableImageMarginGradientEnd = Color.Transparent;                                           
        }
        // Buttons
        /// <summary>Gets or sets the solid color used when the button is selected</summary>
        public Color SettableButtonSelectedHighlight            { get; set; }
        /// <summary>Gets or sets the border color to use with ButtonSelectedHighlight</summary>
        public Color SettableButtonSelectedHighlightBorder      { get; set; }
        /// <summary>Gets or sets the solid color used when the button is pressed</summary>
        public Color SettableButtonPressedHighlight { get; set; }
        /// <summary>Gets or sets the border color to use with ButtonPressedHighlight</summary>
        public Color SettableButtonPressedHighlightBorder { get; set; }
        /// <summary>Gets or sets the solid color used when the button is checked</summary>
        public Color SettableButtonCheckedHighlight { get; set; }
        /// <summary>Gets or sets the border color to use with ButtonCheckedHighlight</summary>
        public Color SettableButtonCheckedHighlightBorder { get; set; }
        /// <summary>Gets or sets the border color to use with the ButtonPressedGradientBegin, ButtonPressedGradientMiddle, and ButtonPressedGradientEnd colors</summary>
        public Color SettableButtonPressedBorder { get; set; }
        /// <summary>Gets or sets the border color to use with the ButtonSelectedGradientBegin, ButtonSelectedGradientMiddle, and ButtonSelectedGradientEnd colors</summary>
        public Color SettableButtonSelectedBorder { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used when the button is checked</summary>
        public Color SettableButtonCheckedGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used when the button is checked</summary>
        public Color SettableButtonCheckedGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used when the button is checked</summary>
        public Color SettableButtonCheckedGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used when the button is selected</summary>
        public Color SettableButtonSelectedGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used when the button is selected</summary>
        public Color SettableButtonSelectedGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used when the button is selected</summary>
        public Color SettableButtonSelectedGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used when the button is pressed</summary>
        public Color SettableButtonPressedGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used when the button is pressed</summary>
        public Color SettableButtonPressedGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used when the button is pressed</summary>
        public Color SettableButtonPressedGradientEnd { get; set; }
                                                             
        // Checks                                           
        /// <summary>Gets or sets the solid color to use when the button is checked and gradients are being used</summary>
        public Color SettableCheckBackground { get; set; }
        /// <summary>Gets or sets the solid color to use when the button is checked and selected and gradients are being used</summary>
        public Color SettableCheckSelectedBackground { get; set; }
        /// <summary>Gets or sets the solid color to use when the button is checked and selected and gradients are being used</summary>
        public Color SettableCheckPressedBackground { get; set; }
                                                            
        // Grips                                              
        /// <summary>Gets or sets the color to use for shadow effects on the grip (move handle)</summary>
        public Color SettableGripDark { get; set; }
        /// <summary>Gets or sets the color to use for highlight effects on the grip (move handle)</summary>
        public Color SettableGripLight { get; set; }
                                                            
        // Image Margins                                             
        /// <summary>Gets or sets the starting color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public Color SettableImageMarginGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public Color SettableImageMarginGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public Color SettableImageMarginGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        public Color SettableImageMarginRevealedGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        public Color SettableImageMarginRevealedGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        public Color SettableImageMarginRevealedGradientEnd { get; set; }
                                                            
        // Menus                                              
        /// <summary>Gets or sets the starting color of the gradient used in the MenuStrip</summary>
        public Color SettableMenuStripGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the MenuStrip</summary>
        public Color SettableMenuStripGradientEnd { get; set; }
        /// <summary>Gets or sets the solid color to use when a ToolStripMenuItem other than the top-level ToolStripMenuItem is selected</summary>
        public Color SettableMenuItemSelected { get; set; }
        /// <summary>Gets or sets the border color to use with a ToolStripMenuItem</summary>
        public Color SettableMenuItemBorder { get; set; }
        /// <summary>Gets or sets the color that is the border color to use on a MenuStrip</summary>
        public Color SettableMenuBorder { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used when the ToolStripMenuItem is selected</summary>
        public Color SettableMenuItemSelectedGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used when the ToolStripMenuItem is selected</summary>
        public Color SettableMenuItemSelectedGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public Color SettableMenuItemPressedGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public Color SettableMenuItemPressedGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public Color SettableMenuItemPressedGradientEnd { get; set; }
                                                            
        // Rafting Containers                                         
        /// <summary>Gets or sets the starting color of the gradient used in the ToolStripContainer</summary>
        public Color SettableRaftingContainerGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the ToolStripContainer</summary>
        public Color SettableRaftingContainerGradientEnd { get; set; }
                                                             
        // Separators                                             
        /// <summary>Gets or sets the color to use to for shadow effects on the ToolStripSeparator</summary>
        public Color SettableSeparatorDark { get; set; }
        /// <summary>Gets or sets the color to use to for highlight effects on the ToolStripSeparator</summary>
        public Color SettableSeparatorLight { get; set; }
                                                             
        // Status Strips                                            
        /// <summary>Gets or sets the starting color of the gradient used on the StatusStrip</summary>
        public Color SettableStatusStripGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used on the StatusStrip</summary>
        public Color SettableStatusStripGradientEnd { get; set; }
                                                            
        // Tool Strips                                       
        /// <summary>Gets or sets the border color to use on the bottom edge of the ToolStrip</summary>
        public Color SettableToolStripBorder { get; set; }
        /// <summary>Gets or sets the solid background color of the ToolStripDropDown</summary>
        public Color SettableToolStripDropDownBackground { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used in the ToolStrip background</summary>
        public Color SettableToolStripGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used in the ToolStrip background</summary>
        public Color SettableToolStripGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the ToolStrip background</summary>
        public Color SettableToolStripGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used in the ToolStripContentPanel</summary>
        public Color SettableToolStripContentPanelGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the ToolStripContentPanel</summary>
        public Color SettableToolStripContentPanelGradientEnd { get; set; }
        /// <summary>Gets or sets the starting color of the gradient used in the ToolStripPanel</summary>
        public Color SettableToolStripPanelGradientBegin { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the ToolStripPanel</summary>
        public Color SettableToolStripPanelGradientEnd { get; set; }
                                                             
        // Overflows                                            
        /// <summary>Gets or sets the starting color of the gradient used in the ToolStripOverflowButton</summary>
        public Color SettableOverflowButtonGradientBegin { get; set; }
        /// <summary>Gets or sets the middle color of the gradient used in the ToolStripOverflowButton</summary>
        public Color SettableOverflowButtonGradientMiddle { get; set; }
        /// <summary>Gets or sets the end color of the gradient used in the ToolStripOverflowButton</summary>
        public Color SettableOverflowButtonGradientEnd { get; set; }

        #region Overrides

        // Buttons
        /// <summary>Gets solid color used when button is selected</summary>
        public override Color ButtonSelectedHighlight { get { return SettableButtonSelectedHighlight; } }
        /// <summary>Gets the border color to use with ButtonSelectedHighlight</summary>
        public override Color ButtonSelectedHighlightBorder { get { return SettableButtonSelectedHighlightBorder; } }
        /// <summary>Gets the solid color used when the button is pressed</summary>
        public override Color ButtonPressedHighlight { get { return SettableButtonPressedHighlight; } }
        /// <summary>Gets the border color to use with ButtonPressedHighlight</summary>
        public override Color ButtonPressedHighlightBorder { get { return SettableButtonPressedHighlightBorder; } }
        /// <summary>Gets the solid color used when the button is checked</summary>
        public override Color ButtonCheckedHighlight { get { return SettableButtonCheckedHighlight; } }
        /// <summary>Gets the border color to use with ButtonCheckedHighlight</summary>
        public override Color ButtonCheckedHighlightBorder { get { return SettableButtonCheckedHighlightBorder; } }
        /// <summary>Gets the border color to use with the ButtonPressedGradientBegin, ButtonPressedGradientMiddle, and ButtonPressedGradientEnd colors</summary>
        //public override Color ButtonPressedBorder { get { return SettableButtonPressedBorder; } }
        public override Color ButtonPressedBorder { get { return SettableButtonCheckedHighlightBorder; } }
        /// <summary>Gets the border color to use with the ButtonSelectedGradientBegin, ButtonSelectedGradientMiddle, and ButtonSelectedGradientEnd colors</summary>
        //public override Color ButtonSelectedBorder { get { return SettableButtonSelectedBorder; } }
        public override Color ButtonSelectedBorder { get { return SettableButtonPressedBorder; } }
        /// <summary>Gets the starting color of the gradient used when the button is checked</summary>
        public override Color ButtonCheckedGradientBegin { get { return SettableButtonCheckedGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used when the button is checked</summary>
        public override Color ButtonCheckedGradientMiddle { get { return SettableButtonCheckedGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used when the button is checked</summary>
        public override Color ButtonCheckedGradientEnd { get { return SettableButtonCheckedGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used when the button is selected</summary>
        public override Color ButtonSelectedGradientBegin { get { return SettableButtonSelectedGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used when the button is selected</summary>
        public override Color ButtonSelectedGradientMiddle { get { return SettableButtonSelectedGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used when the button is selected</summary>
        public override Color ButtonSelectedGradientEnd { get { return SettableButtonSelectedGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used when the button is pressed</summary>
        public override Color ButtonPressedGradientBegin { get { return SettableButtonPressedGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used when the button is pressed</summary>
        public override Color ButtonPressedGradientMiddle { get { return SettableButtonPressedGradientMiddle; } }
        /// <summary>Gets the middle color of the gradient used when the button is pressed</summary>
        public override Color ButtonPressedGradientEnd { get { return SettableButtonPressedGradientEnd; } }

        // Checks
        /// <summary>Gets the solid color to use when the button is checked and gradients are being used</summary>
        public override Color CheckBackground { get { return SettableCheckBackground; } }
        /// <summary>Gets the solid color to use when the button is checked and selected and gradients are being used</summary>
        public override Color CheckSelectedBackground { get { return SettableCheckSelectedBackground; } }
        /// <summary>Gets the solid color to use when the button is checked and selected and gradients are being used</summary>
        public override Color CheckPressedBackground { get { return SettableCheckPressedBackground; } }

        // Grips
        /// <summary>Gets the color to use for shadow effects on the grip (move handle)</summary>
        public override Color GripDark { get { return SettableGripDark; } }
        /// <summary>Gets the color to use for highlight effects on the grip (move handle)</summary>
        public override Color GripLight { get { return SettableGripLight; } }

        // Image Margins
        /// <summary>Gets the starting color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public override Color ImageMarginGradientBegin { get { return SettableImageMarginGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public override Color ImageMarginGradientMiddle { get { return SettableImageMarginGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used in the image margin of a ToolStripDropDownMenu</summary>
        public override Color ImageMarginGradientEnd { get { return SettableImageMarginGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        //public override Color ImageMarginRevealedGradientBegin { get { return SettableImageMarginRevealedGradientBegin; } }
        public override Color ImageMarginRevealedGradientBegin { get { return SettableImageMarginGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        //public override Color ImageMarginRevealedGradientMiddle { get { return SettableImageMarginRevealedGradientMiddle; } }
        public override Color ImageMarginRevealedGradientMiddle { get { return SettableImageMarginGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used in the image margin of a ToolStripDropDownMenu when an item is revealed</summary>
        //public override Color ImageMarginRevealedGradientEnd { get { return SettableImageMarginRevealedGradientEnd; } }
        public override Color ImageMarginRevealedGradientEnd { get { return SettableImageMarginGradientEnd; } }

        // Menus
        /// <summary>Gets the starting color of the gradient used in the MenuStrip</summary>
        public override Color MenuStripGradientBegin { get { return SettableMenuStripGradientBegin; } }
        /// <summary>Gets the end color of the gradient used in the MenuStrip</summary>
        public override Color MenuStripGradientEnd { get { return SettableMenuStripGradientEnd; } }
        /// <summary>Gets the solid color to use when a ToolStripMenuItem other than the top-level ToolStripMenuItem is selected</summary>
        public override Color MenuItemSelected { get { return SettableMenuItemSelected; } }
        /// <summary>Gets the border color to use with a ToolStripMenuItem</summary>
        public override Color MenuItemBorder { get { return SettableMenuItemBorder; } }
        /// <summary>Gets the color that is the border color to use on a MenuStrip</summary>
        public override Color MenuBorder { get { return SettableMenuBorder; } }
        /// <summary>Gets the starting color of the gradient used when the ToolStripMenuItem is selected</summary>
        public override Color MenuItemSelectedGradientBegin { get { return SettableMenuItemSelectedGradientBegin; } }
        /// <summary>Gets the end color of the gradient used when the ToolStripMenuItem is selected</summary>
        public override Color MenuItemSelectedGradientEnd { get { return SettableMenuItemSelectedGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public override Color MenuItemPressedGradientBegin { get { return SettableMenuItemPressedGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public override Color MenuItemPressedGradientMiddle { get { return SettableMenuItemPressedGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used when a top-level ToolStripMenuItem is pressed</summary>
        public override Color MenuItemPressedGradientEnd { get { return SettableMenuItemPressedGradientEnd; } }

        // Rafting Containers
        /// <summary>Gets the starting color of the gradient used in the ToolStripContainer</summary>
        public override Color RaftingContainerGradientBegin { get { return SettableRaftingContainerGradientBegin; } }
        /// <summary>Gets the end color of the gradient used in the ToolStripContainer</summary>
        public override Color RaftingContainerGradientEnd { get { return SettableRaftingContainerGradientEnd; } }

        // Separators
        /// <summary>Gets the color to use to for shadow effects on the ToolStripSeparator</summary>
        public override Color SeparatorDark { get { return SettableSeparatorDark; } }
        /// <summary>Gets the color to use to for highlight effects on the ToolStripSeparator</summary>
        public override Color SeparatorLight { get { return SettableSeparatorLight; } }

        // Status Strips
        /// <summary>Gets the starting color of the gradient used on the StatusStrip</summary>
        public override Color StatusStripGradientBegin { get { return SettableStatusStripGradientBegin; } }
        /// <summary>Gets the end color of the gradient used on the StatusStrip</summary>
        public override Color StatusStripGradientEnd { get { return SettableStatusStripGradientEnd; } }

        // Tool Strips
        /// <summary>Gets the border color to use on the bottom edge of the ToolStrip</summary>
        public override Color ToolStripBorder { get { return SettableToolStripBorder; } }
        /// <summary>Gets the solid background color of the ToolStripDropDown</summary>
        public override Color ToolStripDropDownBackground { get { return SettableToolStripDropDownBackground; } }
        /// <summary>Gets the starting color of the gradient used in the ToolStrip background</summary>
        public override Color ToolStripGradientBegin { get { return SettableToolStripGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used in the ToolStrip background</summary>
        public override Color ToolStripGradientMiddle { get { return SettableToolStripGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used in the ToolStrip background</summary>
        public override Color ToolStripGradientEnd { get { return SettableToolStripGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used in the ToolStripContentPanel</summary>
        public override Color ToolStripContentPanelGradientBegin { get { return SettableToolStripContentPanelGradientBegin; } }
        /// <summary>Gets the end color of the gradient used in the ToolStripContentPanel</summary>
        public override Color ToolStripContentPanelGradientEnd { get { return SettableToolStripContentPanelGradientEnd; } }
        /// <summary>Gets the starting color of the gradient used in the ToolStripPanel</summary>
        public override Color ToolStripPanelGradientBegin { get { return SettableToolStripPanelGradientBegin; } }
        /// <summary>Gets the end color of the gradient used in the ToolStripPanel</summary>
        public override Color ToolStripPanelGradientEnd { get { return SettableToolStripPanelGradientEnd; } }

        // Overflows
        /// <summary>Gets the starting color of the gradient used in the ToolStripOverflowButton</summary>
        public override Color OverflowButtonGradientBegin { get { return SettableOverflowButtonGradientBegin; } }
        /// <summary>Gets the middle color of the gradient used in the ToolStripOverflowButton</summary>
        public override Color OverflowButtonGradientMiddle { get { return SettableOverflowButtonGradientMiddle; } }
        /// <summary>Gets the end color of the gradient used in the ToolStripOverflowButton</summary>
        public override Color OverflowButtonGradientEnd { get { return SettableOverflowButtonGradientEnd; } }

        #endregion
    }
}
