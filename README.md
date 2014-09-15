ATF
===

### Please take the [Documentation Survey](https://github.com/SonyWWS/ATF/wiki/Documentation-Survey). We would like your feedback on how to improve ATF documentation. ###

### [What's New](https://github.com/SonyWWS/LevelEditor/wiki/LevelEditor-Open-Source-Press-Release) ###

Authoring Tools Framework (ATF) is a set of C#/.NET components for making tools on Windows&reg;. ATF has been used by most Sony Computer Entertainment first party game studios to make many custom tools such as Naughty Dog's level editor and shader editor for _The Last of Us_&trade;, Guerrilla Game's sequence editor for Killzone&trade; games (including the _Killzone: Shadow Fall_&trade; PS4&trade; launch title), an animation blending tool at Santa Monica Studios, a level editor at Bend Studio, a visual state machine editor for Quantic Dream, sound editing tools, and many others. ATF has been in continuous development in Sony Computer Entertainment's Worldwide Studios central tools group since early 2005.

![LittleBigPlanet Level Editor](https://raw.githubusercontent.com/wiki/SonyWWS/ATF/images/LBP_PSP_2.png?raw=true "LittleBigPlanet Level Editor")

There are many types of components in ATF. ATF Managed Extensibility Framework (MEF) components can be added to a .NET TypeCatalog with a single line of code that can be found in most of our sample applications, as in \Samples\CircuitEditor\Program.cs. Other components, like the DOM, are really a collection of related classes. Components include:

* Control Host Service -- for Windows docking and tracking active Controls.
* Command Service -- for registering commands in menus and on tool bars and associating keyboard shortcuts.
* Property Editor -- 2-column property editor for editing selected objects.
* Grid Property Editor -- spreadsheet-style property editor for selected objects.
* Tree Control and Tree List Control
* Property Editing Commands -- commands for editing properties, like Reset, Reset All, Copy Value, Paste Value, Copy All, Paste All.
* Many property editing controls -- bounded integer, bounded float, tuples, enums, flags, colors, collections of child properties.
* Document Object Model (DOM) -- an in-memory observable database, to help support document persistence and copy/paste and undo/redo. It's currently required for circuit editing.
* Direct2D -- a thin layer over native Direct2D resources.
* OpenGL&reg; 2.1 -- a thin wrapper to access native OpenGL resources.
* Open Sound Control (OSC) -- a general way of controlling a Windows application using a tablet computer (e.g., iPad&reg;), by sending name-value pairs.
* Circuit Editing -- circuits and groups of circuits and templates, wires, annotations.
* Timeline Editing -- timelines with intervals, keys, tracks, groups, and referenced child timelines.
* Settings Service -- for persisting settings and user preferences, plus a settings command and dialog.
* Window Layout Service -- allows user to switch between window layouts and to create or delete layouts.
* Unhandled Exception Service -- catches unhandled exceptions, displays info, and gives user a chance to save.
* File Dialog Service -- standard Windows file dialogs.
* Document Registry -- central document registry with change notification.
* Auto Document Service -- opens documents from last session, or creates a new document on startup.
* Recent Document Commands -- standard recent document commands in File menu.
* Standard File Commands -- standard File menu commands for New, Open, Save, SaveAs, Close.
* Tabbed Control Selector -- enable ctrl-tab selection of documents and controls within the app.
* Help About Command -- Help -> About command.
* Context Registry -- central context registry with change notification.
* Standard File Exit Command -- standard File exit menu command.
* Standard Edit Commands -- standard Edit menu commands for copy/paste.
* Standard Edit History Commands -- standard Edit menu commands for undo/redo.
* Standard Selection Commands -- standard Edit menu selection commands.
* Standard Print Commands -- does not currently work with Direct2D.
* Palette Service -- global palette, for drag/drop instancing.
* Outputs -- passes messages to all log writers.
* Python Service -- scripting service for automated tests.
* Script Console -- provides a dockable command console for entering script (e.g., Python) commands.
* Automation Service -- provides facilities to run an automated script using the .NET remoting service.
* Source Control Commands -- source control commmands to interact with Perforce service.

ATF is an open source project. See License.txt.

### Prerequisites ###
You must have .NET Framework 4.0 or greater installed in order to run the ATF applications. You can download it here:
http://www.microsoft.com/en-us/download/details.aspx?id=17851 

You must have Windows 7 or later to run the circuit, FSM, statechart, and timeline editors, due to their use of Direct2D.

To use Live Connect, a local network broadcasting service, you must have Bonjour installed. Bonjour is installed with iTunes and some other products. The installers, Bonjour64.msi (for 64-bit Windows) and Bonjour.msi (for 32-bit Windows), can be provided within Sony upon request, and are located in our non-public directory: \NoDistro\ThirdParty\Wws.LiveConnect.

Visual Studio 2010 or later is required to compile the ATF solution and project files.


### Getting Started ###
Our wiki-based documentation is extensive and it describes the different components that are available and how to make new Windows tools.
http://github.com/SonyWWS/ATF/wiki

Another good starting document is:
/Docs/Atf-GettingStarted.pdf

To try the ATF sample applications, open and build \Samples\Samples.vs2010.sln. The sample applications' executables are placed in the \bin folder.

To build all the samples, development tools, and unit tests, open and build \Test\Everything.vs2010.sln.


### Project Warnings ###
Although we don't ship with any C# compile warnings, we are still shipping with two project warnings that you may see when you open up a couple of our projects in Visual Studio. These are unlikely to cause problems, but here is an explanation for what is happening.

Atf.Core:  
This project has a warning about a possible run-time error due to a mismatch between the processor architecture with Wws.LiveConnect.dll. This is a false warning, because this DLL is not actually copied to the output directory. (The copy local setting is set to false.) The application's project, like TimelineEditor, has to reference the correct version of Wws.LiveConnect.dll (x86 or x64). These sample apps provide an example of how to conditionally reference the correct DLL based on the target processor architecture. For a future release of ATF, a new Wws.LiveConnect.dll should be available that will target AnyCPU, and so this warning will go away.

Atf.Gui.OpenGL:  
This project has a warning about a possible run-time error due to a mismatch between the processor architecture with DDSUtils.dll. There has been a very long-standing requirement that to use this NVidia utility (like for opening certain image files), you have to target the x86 processor architecture. New clients within Sony should use the Direct3D native C++ rendering of the new LevelEditor component in the WWS SDK.

Atf.Perforce:  
This project has a warning about not being able to link to the file p4bridge.dll. This is not a valid warning since there are two conditional links to p4bridge.dll, but both links can't be active at the same time. One conditional link is for 32-bit compiling and the other is for 64-bit.


### More Info ###
Documentation:  
https://github.com/SonyWWS/ATF/wiki  
Locally, we have PDF files in the /Docs directory.

Bugs, feature requests, and questions:  
https://github.com/SonyWWS/ATF/issues

Contact, in English or Japanese:  
&#097;&#116;&#102;&#095;&#105;&#110;&#102;&#111;&#064;&#112;&#108;&#097;&#121;&#115;&#116;&#097;&#116;&#105;&#111;&#110;&#046;&#115;&#111;&#110;&#121;&#046;&#099;&#111;&#109;
