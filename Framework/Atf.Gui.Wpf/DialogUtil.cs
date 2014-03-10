//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Linq;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Dialog utilities</summary>
    public static class DialogUtil
    {
        /// <summary>
        /// Gets the active window of the current application, 
        /// or, if no windows are active, gets the main window</summary>
        /// <returns>Active window of the current application, 
        /// or, if no windows are active, the main window</returns>
        public static Window GetActiveWindow()
        {
            Window activeWindow = Application.Current.MainWindow;

            if (!Application.Current.MainWindow.IsActive)
            {
                Window subWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);
                if (subWindow != null)
                    activeWindow = subWindow;
            }

            return activeWindow;
        }
    }
}
