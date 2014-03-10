//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
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
    public class CustomColorTable : ProfessionalColorTable
    {
        // Buttons
        public Color SettableButtonSelectedHighlight            { get; set; }
        public Color SettableButtonSelectedHighlightBorder      { get; set; }
        public Color SettableButtonPressedHighlight             { get; set; }
        public Color SettableButtonPressedHighlightBorder       { get; set; }
        public Color SettableButtonCheckedHighlight             { get; set; }
        public Color SettableButtonCheckedHighlightBorder       { get; set; }
        public Color SettableButtonPressedBorder                { get; set; }
        public Color SettableButtonSelectedBorder               { get; set; }
        public Color SettableButtonCheckedGradientBegin         { get; set; }
        public Color SettableButtonCheckedGradientMiddle        { get; set; }
        public Color SettableButtonCheckedGradientEnd           { get; set; }
        public Color SettableButtonSelectedGradientBegin        { get; set; }
        public Color SettableButtonSelectedGradientMiddle       { get; set; }
        public Color SettableButtonSelectedGradientEnd          { get; set; }
        public Color SettableButtonPressedGradientBegin         { get; set; }
        public Color SettableButtonPressedGradientMiddle        { get; set; }
        public Color SettableButtonPressedGradientEnd           { get; set; }
                                                             
        // Checks                                           
        public Color SettableCheckBackground                    { get; set; }
        public Color SettableCheckSelectedBackground            { get; set; }
        public Color SettableCheckPressedBackground             { get; set; }
                                                            
        // Grips                                              
        public Color SettableGripDark                           { get; set; }
        public Color SettableGripLight                          { get; set; }
                                                            
        // Image Margins                                             
        public Color SettableImageMarginGradientBegin           { get; set; }
        public Color SettableImageMarginGradientMiddle          { get; set; }
        public Color SettableImageMarginGradientEnd             { get; set; }
        public Color SettableImageMarginRevealedGradientBegin   { get; set; }
        public Color SettableImageMarginRevealedGradientMiddle  { get; set; }
        public Color SettableImageMarginRevealedGradientEnd     { get; set; }
                                                            
        // Menus                                              
        public Color SettableMenuStripGradientBegin             { get; set; }
        public Color SettableMenuStripGradientEnd               { get; set; }
        public Color SettableMenuItemSelected                   { get; set; }
        public Color SettableMenuItemBorder                     { get; set; }
        public Color SettableMenuBorder                         { get; set; }
        public Color SettableMenuItemSelectedGradientBegin      { get; set; }
        public Color SettableMenuItemSelectedGradientEnd        { get; set; }
        public Color SettableMenuItemPressedGradientBegin       { get; set; }
        public Color SettableMenuItemPressedGradientMiddle      { get; set; }
        public Color SettableMenuItemPressedGradientEnd         { get; set; }
                                                            
        // Rafting Containers                                         
        public Color SettableRaftingContainerGradientBegin      { get; set; }
        public Color SettableRaftingContainerGradientEnd        { get; set; }
                                                             
        // Separators                                             
        public Color SettableSeparatorDark                      { get; set; }
        public Color SettableSeparatorLight                     { get; set; }
                                                             
        // Status Strips                                            
        public Color SettableStatusStripGradientBegin           { get; set; }
        public Color SettableStatusStripGradientEnd             { get; set; }
                                                            
        // Tool Strips                                       
        public Color SettableToolStripBorder                    { get; set; }
        public Color SettableToolStripDropDownBackground        { get; set; }
        public Color SettableToolStripGradientBegin             { get; set; }
        public Color SettableToolStripGradientMiddle            { get; set; }
        public Color SettableToolStripGradientEnd               { get; set; }
        public Color SettableToolStripContentPanelGradientBegin { get; set; }
        public Color SettableToolStripContentPanelGradientEnd   { get; set; }
        public Color SettableToolStripPanelGradientBegin        { get; set; }
        public Color SettableToolStripPanelGradientEnd          { get; set; }
                                                             
        // Overflows                                            
        public Color SettableOverflowButtonGradientBegin        { get; set; }
        public Color SettableOverflowButtonGradientMiddle       { get; set; }
        public Color SettableOverflowButtonGradientEnd          { get; set; }

        #region Overrides

        // Buttons
        public override Color ButtonSelectedHighlight               { get { return SettableButtonSelectedHighlight; } }
        public override Color ButtonSelectedHighlightBorder         { get { return SettableButtonSelectedHighlightBorder; } }
        public override Color ButtonPressedHighlight                { get { return SettableButtonPressedHighlight; } }
        public override Color ButtonPressedHighlightBorder          { get { return SettableButtonPressedHighlightBorder; } }
        public override Color ButtonCheckedHighlight                { get { return SettableButtonCheckedHighlight; } }
        public override Color ButtonCheckedHighlightBorder          { get { return SettableButtonCheckedHighlightBorder; } }
        public override Color ButtonPressedBorder                   { get { return SettableButtonCheckedHighlightBorder; } }
        public override Color ButtonSelectedBorder                  { get { return SettableButtonPressedBorder; } }
        public override Color ButtonCheckedGradientBegin            { get { return SettableButtonCheckedGradientBegin; } }
        public override Color ButtonCheckedGradientMiddle           { get { return SettableButtonCheckedGradientMiddle; } }
        public override Color ButtonCheckedGradientEnd              { get { return SettableButtonCheckedGradientEnd; } }
        public override Color ButtonSelectedGradientBegin           { get { return SettableButtonSelectedGradientBegin; } }
        public override Color ButtonSelectedGradientMiddle          { get { return SettableButtonSelectedGradientMiddle; } }
        public override Color ButtonSelectedGradientEnd             { get { return SettableButtonSelectedGradientEnd; } }
        public override Color ButtonPressedGradientBegin            { get { return SettableButtonPressedGradientBegin; } }
        public override Color ButtonPressedGradientMiddle           { get { return SettableButtonPressedGradientMiddle; } }
        public override Color ButtonPressedGradientEnd              { get { return SettableButtonPressedGradientEnd; } }

        // Checks
        public override Color CheckBackground                       { get { return SettableCheckBackground; } }
        public override Color CheckSelectedBackground               { get { return SettableCheckSelectedBackground; } }
        public override Color CheckPressedBackground                { get { return SettableCheckPressedBackground; } }
        
        // Grips
        public override Color GripDark                              { get { return SettableGripDark; } }
        public override Color GripLight                             { get { return SettableGripLight; } }

        // Image Margins
        public override Color ImageMarginGradientBegin              { get { return SettableImageMarginGradientBegin; } }
        public override Color ImageMarginGradientMiddle             { get { return SettableImageMarginGradientMiddle; } }
        public override Color ImageMarginGradientEnd                { get { return SettableImageMarginGradientEnd; } }
        public override Color ImageMarginRevealedGradientBegin      { get { return SettableImageMarginGradientBegin; } }
        public override Color ImageMarginRevealedGradientMiddle     { get { return SettableImageMarginGradientMiddle; } }
        public override Color ImageMarginRevealedGradientEnd        { get { return SettableImageMarginGradientEnd; } }

        // Menus
        public override Color MenuStripGradientBegin                { get { return SettableMenuStripGradientBegin; } }
        public override Color MenuStripGradientEnd                  { get { return SettableMenuStripGradientEnd; } }
        public override Color MenuItemSelected                      { get { return SettableMenuItemSelected; } }
        public override Color MenuItemBorder                        { get { return SettableMenuItemBorder; } }
        public override Color MenuBorder                            { get { return SettableMenuBorder; } }
        public override Color MenuItemSelectedGradientBegin         { get { return SettableMenuItemSelectedGradientBegin; } }
        public override Color MenuItemSelectedGradientEnd           { get { return SettableMenuItemSelectedGradientEnd; } }
        public override Color MenuItemPressedGradientBegin          { get { return SettableMenuItemPressedGradientBegin; } }
        public override Color MenuItemPressedGradientMiddle         { get { return SettableMenuItemPressedGradientMiddle; } }
        public override Color MenuItemPressedGradientEnd            { get { return SettableMenuItemPressedGradientEnd; } }

        // Rafting Containers
        public override Color RaftingContainerGradientBegin         { get { return SettableRaftingContainerGradientBegin; } }
        public override Color RaftingContainerGradientEnd           { get { return SettableRaftingContainerGradientEnd; } }

        // Separators
        public override Color SeparatorDark                         { get { return SettableSeparatorDark; } }
        public override Color SeparatorLight                        { get { return SettableSeparatorLight; } }

        // Status Strips
        public override Color StatusStripGradientBegin              { get { return SettableStatusStripGradientBegin; } }
        public override Color StatusStripGradientEnd                { get { return SettableStatusStripGradientEnd; } }

        // Tool Strips
        public override Color ToolStripBorder                       { get { return SettableToolStripBorder; } }
        public override Color ToolStripDropDownBackground           { get { return SettableToolStripDropDownBackground; } }
        public override Color ToolStripGradientBegin                { get { return SettableToolStripGradientBegin; } }
        public override Color ToolStripGradientMiddle               { get { return SettableToolStripGradientMiddle; } }
        public override Color ToolStripGradientEnd                  { get { return SettableToolStripGradientEnd; } }
        public override Color ToolStripContentPanelGradientBegin    { get { return SettableToolStripContentPanelGradientBegin; } }
        public override Color ToolStripContentPanelGradientEnd      { get { return SettableToolStripContentPanelGradientEnd; } }
        public override Color ToolStripPanelGradientBegin           { get { return SettableToolStripPanelGradientBegin; } }
        public override Color ToolStripPanelGradientEnd             { get { return SettableToolStripPanelGradientEnd; } }
        
        // Overflows
        public override Color OverflowButtonGradientBegin           { get { return SettableOverflowButtonGradientBegin; } }
        public override Color OverflowButtonGradientMiddle          { get { return SettableOverflowButtonGradientMiddle; } }
        public override Color OverflowButtonGradientEnd             { get { return SettableOverflowButtonGradientEnd; } }

        #endregion
    }
}
