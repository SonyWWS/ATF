//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
using Microsoft.Win32;
using Sce.Atf.Wpf.Models;
using System.Windows.Interop;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Miscellaneous extension methods</summary>
    public static class MiscExtensions
    {
        /// <summary>
        /// Try to parse a string into a GUID</summary>
        /// <remarks>.NET 3.5 does not have Guid.TryParse functionality. This should be moved somewhere more suitable.</remarks>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting GUID; is Guid.Empty if function returns false</param>
        /// <returns>True iff parsing successful</returns>
        public static bool GuidTryParse(string s, out Guid result)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            var format = new Regex(
                "^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");

            var match = format.Match(s);
            if (match.Success)
            {
                result = new Guid(s);
                return true;
            }

            result = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Binary search a list for a given value using a comparison function</summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="list">List to search</param>
        /// <param name="value">Value to search for</param>
        /// <param name="comparer">Comparison function</param>
        /// <returns>Zero-based index of nearest item to value in list; less than zero if out of list range</returns>
        public static int BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<T>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                
                if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~lower;
        }

        /// <summary>
        /// Display child window, updating its owner</summary>
        /// <param name="owner">Owner window</param>
        /// <param name="child">Child window to display</param>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowChildDialog(this Window owner, Window child)
        {
            Requires.NotNull(owner, "owner");
            Requires.NotNull(child, "child");

            child.DataContext = owner.DataContext;
            child.Owner = owner;
            return child.ShowDialog();
        }

        /// <summary>
        /// Create and display a dialog of a given type, setting owner</summary>
        /// <typeparam name="T">Dialog type</typeparam>
        /// <param name="owner">Dialog owner</param>
        /// <param name="e">ShowDialogEventArgs specifying dialog view model</param>
        public static void CreateAndShowChildDialog<T>(this Window owner, ShowDialogEventArgs e)
            where T : Window
        {
            Requires.NotNull(e, "e");

            T dlg = Activator.CreateInstance<T>();
            dlg.DataContext = e.ViewModel;
            dlg.Owner = owner;
            e.DialogResult = dlg.ShowDialog();
        }

        /// <summary>
        /// Show specified dialog, setting its owner to the active window</summary>
        /// <param name="dialog">Dialog to display</param>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowParentedDialog(this Window dialog)
        {
            dialog.Owner = DialogUtils.GetActiveWindow();
            return dialog.ShowDialog();
        }

        /// <summary>
        /// Extension method to show specified dialog, setting its owner to the active window</summary>
        /// <param name="dialog">Dialog to display</param>
        public static void ShowParented(this Window dialog)
        {
            dialog.Owner = DialogUtils.GetActiveWindow();
            dialog.Show();
        }

        /// <summary>
        /// Show specified dialog with view model</summary>
        /// <param name="dialog">Dialog to display</param>
        /// <param name="viewModel">View model</param>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowDialogWithViewModel(this Window dialog, object viewModel)
        {
            dialog.DataContext = viewModel;
            return dialog.ShowParentedDialog();
        }

        /// <summary>
        /// Show specified dialog</summary>
        /// <typeparam name="TViewModel">View model</typeparam>
        /// <param name="dialog">Dialog to display</param>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowDialogWithViewModel<TViewModel>(this Window dialog)
        {
            dialog.DataContext = Activator.CreateInstance<TViewModel>();
            return dialog.ShowParentedDialog();
        }

        /// <summary>
        /// Show a Common Win32 Dialog but with workaround for special case when
        /// shown from an already modal dialog.  Without woraround the CommonDialog
        /// gets shown as a modeless dialog.
        /// http://social.msdn.microsoft.com/Forums/en/wpf/thread/48ac4e21-ee16-4e64-b883-98b6c99ee1fa </summary>
        /// <param name="dialog">CommonDialog to show</param>
        /// <returns>Dialog result</returns>
        public static bool? ShowCommonDialogWorkaround(this CommonDialog dialog)
        {
            var mainWindow = Application.Current.MainWindow;
            var currentWindow = DialogUtils.GetActiveWindow();
            bool? result;
            try
            {
                Application.Current.MainWindow = currentWindow;
                HookWindowActivatedEvents();
                _isDialogOpen = true;
                result = dialog.ShowDialog();
            }
            finally
            {
                _isDialogOpen = false;
                UnhookWindowsActivatedEvents();
                Application.Current.MainWindow = mainWindow;
            }

            return result;
        }

        private static void UnhookWindowsActivatedEvents()
        {
            foreach (var obj in Application.Current.Windows)
            {
                var window = obj as Window;
                if (window != null)
                {
                    window.IsHitTestVisible = true;
                    window.Activated -= Window_Activated;
                }
            }
        }

        private static void HookWindowActivatedEvents()
        {
            foreach (var obj in Application.Current.Windows)
            {
                var window = obj as Window;
                if (window != null)
                {
                    window.IsHitTestVisible = false;
                    window.Activated += Window_Activated;
                }
            }
        }

        private static void Window_Activated(object sender, EventArgs e)
        {
            if (_isDialogOpen)
            {
                Application.Current.MainWindow.Activate();
            }
        }

        private static bool _isDialogOpen;
    }

    /// <summary>
    /// Dialog utility functions</summary>
    public static class DialogUtils
    {
        /// <summary>
        /// Display a given type of dialog with a view model type</summary>
        /// <typeparam name="TDialog">Type of dialog to display</typeparam>
        /// <typeparam name="TViewModel">Type of view model</typeparam>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowDialogWithViewModel<TDialog, TViewModel>()
            where TDialog : Window
        {
            TDialog dialog = Activator.CreateInstance<TDialog>();
            dialog.DataContext = Activator.CreateInstance<TViewModel>();
            return dialog.ShowParentedDialog();
        }

        /// <summary>
        /// Display a given type of dialog with a view model object</summary>
        /// <typeparam name="TDialog">Type of dialog to display</typeparam>
        /// <param name="viewModel">View model object</param>
        /// <returns>Nullable Boolean signifying how window was closed by user</returns>
        public static bool? ShowDialogWithViewModel<TDialog>(object viewModel)
           where TDialog : Window
        {
            TDialog dialog = Activator.CreateInstance<TDialog>();
            dialog.DataContext = viewModel;
            return dialog.ShowParentedDialog();
        }



        /// <summary>
        /// Shows the dialog in a non-modal fashion</summary>
        /// <typeparam name="TDialog">Dialog to display</typeparam>
        /// <param name="viewModel">View model</param>
        public static void ShowWithViewModel<TDialog>(object viewModel)
           where TDialog : Window
        {
            TDialog dialog = Activator.CreateInstance<TDialog>();
            dialog.DataContext = viewModel;
            dialog.ShowParented();
        }

        /// <summary>
        /// Get the active window of the current application, 
        /// or if no windows are active, get the MainWindow</summary>
        /// <returns>Active window of current application or MainWindow if none active</returns>
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

        /// <summary>
        /// Interop method to display a WinForms dialog, setting the owner to the active window</summary>
        /// <param name="dialog">WinForms dialog to display</param>
        /// <returns>System.Windows.Forms.DialogResult.OK if user clicks OK in the dialog box, 
        /// otherwise System.Windows.Forms.DialogResult.Cancel.</returns>
        public static System.Windows.Forms.DialogResult ShowParentedDialog(System.Windows.Forms.CommonDialog dialog)
        {
            Window window = GetActiveWindow();
            var helper = new WindowInteropHelper(window);
            return dialog.ShowDialog(new WindowWrapper(helper.Handle));
        }

        private class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            public WindowWrapper(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; private set; }
        }
    }
}
