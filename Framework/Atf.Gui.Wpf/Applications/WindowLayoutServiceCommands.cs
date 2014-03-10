//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Window layout service commands</summary>
    /// <remarks>Simply piggy-backs the WinForms WindowLayoutServiceCommands for now, since
    /// implementing a pure WPF component seems impossible.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(WindowLayoutServiceCommandsBase))]
    [Export(typeof(WindowLayoutServiceCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindowLayoutServiceCommands : Sce.Atf.Applications.WindowLayoutServiceCommands
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="windowLayoutService">Window layout service to use</param>
        [ImportingConstructor]
        public WindowLayoutServiceCommands(IWindowLayoutService windowLayoutService)
            : base(windowLayoutService)
        {
        }

        /// <summary>
        /// Gets a screenshot of the current application</summary>
        /// <returns>Screenshot image of current application</returns>
        protected override System.Drawing.Image GetApplicationScreenshot()
        {
            return ImageUtil.Capture(Application.Current.MainWindow);
        }
    }
}
