using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Atf.Gui.Wpf")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SCEA")]
[assembly: AssemblyProduct("Atf.Gui.Wpf")]
[assembly: AssemblyCopyright("Copyright © 2014 Sony Computer Entertainment America LLC")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2af4ecab-1b92-4e4c-890a-7b64ecbbd142")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, app, or any theme specific resource dictionaries)
)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Map namespaces so WPF apps using this assembly
// don't need to define so many namespaces in their xaml files
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Models")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Controls")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Controls.PropertyEditing")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Markup")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.ValueConverters")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Behaviors")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Applications")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Skins")]
[assembly: XmlnsDefinition("http://www.sce.net/Atf.Gui.Wpf", "Sce.Atf.Wpf.Docking")]

#if !CS_4
// This line adds the public classes in this assembly and the System.Windows namespace to 
// the default WPF namespace. This makes it XAML compatible with Silverlight where VisualStateManager
// is part of the default namespace.
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows")]
#endif
