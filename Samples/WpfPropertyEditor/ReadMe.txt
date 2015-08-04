Overview
========

ATF's PropertyGridView(Sce.Atf.Wpf.Controls.PropertyEditing.PropertyGridView)
is a WPF control that can largely be used as a standalone control to
view and edit the properties of a specific object instance, in any WPF
app.  In principle, you only need to set the EditingContext property of
PropertyGridView for it to function.  EditingContext property is of
type IPropertyEditingContext, which is not hard to implement yourself;
it provides the current selection of objects and their property
descriptors to be shown.

This sample application demonstrates the usage of PropertyGridView
with minimum dependencies on other regular tenants of a full-fledged
ATF-based application: control host service, docking service, MEF,
DOM, etc. Although we could supply another implementation of
IPropertyEditingContext in this sample, we simply reuse ATF's default
implementation of IPropertyEditingContext named
SelectionPropertyEditingContext here for following reasons:

1) SelectionPropertyEditingContext is a well-written,
easy-to-understand class. Code-reuse is always nice when it is simple
and straightforward. You can also derive from this class for
customized behaviors, such as filtering exposed properties by
overriding GetPropertyDescriptors().
 
2) It has a built-in adaptivity to any client-defined
ISelectionContext. ISelectionContext is a core interface used in many
ATF-based editors for object selections. Your editor will likely find
it helpful and convenient if the property editor uses the same object
selection mechanism.

3) SelectionPropertyEditingContext also has a built-in adaptivity for
IObservableContext, another ATF core interface for object update
notification. Although IObservableContext support is optional, it
would be handy when you need to track changes.

Note: we recommend Sce.Atf.Wpf.Applications.PropertyEditor if your
application uses the typical high level services like control host
service, because all you need to do is simply enlist PropertyEditor
for MEF composition.  The custom set up shown in this sample is for
client code that for various reasons just wants to use PropertyGridView
alone for property editing.


UI SetUp
========

The left column is a ListBox that displays a list of SUVs . When you
select a SUV in the list, its details (public properties) appear on the
right column that is a PropertyGridView.

Data model
----------

We define a Suv class that has public properties that we want to
display and edit in PropertyGridView. A collection of SUV data is
stored in CoolSUVs.xml. We use Linq to XML API to load the xml
data and build a list of SUVs in LoadSUVDataFromXML().

Set-up UI controls
------------------

A typical WPF window starts with a Grid control. In
CreateUIControls(), we programmatically add 1 row and 2 columns to the
grid. We then place a ListBox at the left column to display the names
of SUVs, and PropertyGridView to the right column.

Hook up PropertyGridView
------------------------

SetUpPropertyEditor() first fills the ListBox with SUV names, then set
PropertyGridView's EditingContext to be a
SelectionPropertyEditingContext. This property-editing context uses
ObjectSelectionContext for object selection , which is a boilerplate
implementation of ISelectionContext.

Now if you select a SUV name in the left ListBox, the selection change
of the ListBox will be tracked in the ObjectSelectionContext via
SelectionChanged event handler of the ListBox. The selection change in
ObjectSelectionContext will in turn update the PropertyGridView to
display properties for the newly selected object(SUV).

Set up custom type editor
=========================

The PropertyGridView has built-in editors for property values of type
bool, enum, and text. A textbox editor is the default value editor for
value types other than enum or bool. If you need to set a custom type
editor, like a slider editor for floats, you need to use an editor
type derived from ValueEditor. Currently 5 value editors are provided
in ATF: FilePathValueEditor, FolderPathValueEditor,
MultiLineTextEditor, RangeSliderValueEditor, SliderEditor.

This sample app hooks up SliderEditor(for the Price property of Suv)
as an example of how to set up ValueEditors:

1) If you use auto-generated(reflected) pds like this sample app,
instead of creating pds programmatically such as an
ICustomTypeDescriptor implementation, an easier way to set up a custom
editor for a pd is to create your PropertyFactory and set it to
PropertyGridView.PropertyFactory

2) In your custom PropertyFactory.CreateProperty(), you can call
PropertyNode.SetCustomEditor() for specific pds with the desired
ValueEditor


Required ATF assemblies
=======================
Atf.Gui
Atf.Gui.Wpf
Atf.Core:  SelectionPropertyEditingContext


TODO
====
• Add property descriptions so they can show up in PropertyGridView.
• Make the Color property use a custom editor.
• Support searching/filtering of properties.