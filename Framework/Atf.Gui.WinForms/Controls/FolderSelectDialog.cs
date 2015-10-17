//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.Reflection;

// ------------------------------------------------------------------
// Wraps System.Windows.Forms.OpenFileDialog to make it present
// a vista-style dialog.
// ------------------------------------------------------------------

namespace Sce.Atf.Controls.FolderSelection
{
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.</summary>
    public class FolderSelectDialog : IDisposable
    {
        // Wrapped dialog
        private readonly OpenFileDialog m_ofd;

        /// <summary>
        /// Default constructor</summary>
        public FolderSelectDialog()
        {
            m_ofd =
                new OpenFileDialog
                    {
                        Filter = @"Folders|\n",
                        AddExtension = false,
                        CheckFileExists = false,
                        DereferenceLinks = true,
                        Multiselect = false
                    };

            Disposed = false;
        }

        #region IDisposable
        /// <summary>
        /// Safely handle disposing of resources</summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// Gets/Sets the initial folder to be selected. A null value selects the current directory.</summary>
        public string InitialDirectory
        {
            get { return m_ofd.InitialDirectory; }
            set { m_ofd.InitialDirectory = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value; }
        }

        /// <summary>
        /// Gets or sets the description to show in the dialog</summary>
        public string Description
        {
            get { return m_ofd.Title; }
            set { m_ofd.Title = value ?? "Select a directory".Localize(); }
        }

        /// <summary>
        /// Gets the selected folder</summary>
        public string SelectedPath
        {
            get { return m_ofd.FileName; }
        }

        /// <summary>
        /// Gets whether instance has been disposed</summary>
        private bool Disposed { get; set; }

        /// <summary>
        /// Shows the dialog</summary>
        /// <returns>Dialog result of what the user clicked</returns>
        public DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Shows the dialog</summary>
        /// <param name="owner">Control to be parent</param>
        /// <returns><c>True</c> if the user presses OK else false</returns>
        public DialogResult ShowDialog(IWin32Window owner)
        {
            DialogResult result;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                var r = new Reflector("System.Windows.Forms");

                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(m_ofd, "CreateVistaDialog");
                r.Call(m_ofd, "OnBeforeVistaDialog", dialog);

                uint options = (uint)r.CallAs(typeof(FileDialog), m_ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                object pfde = r.New("FileDialog.VistaDialogEvents", m_ofd);
                object[] parameters = new[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", owner == null ? IntPtr.Zero : owner.Handle);
                    result = (0 == num2) ? DialogResult.OK : DialogResult.Cancel;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = Description;
                    fbd.SelectedPath = InitialDirectory;
                    fbd.ShowNewFolderButton = false;
                    result = fbd.ShowDialog(owner);
                    if (result == DialogResult.OK)
                        m_ofd.FileName = fbd.SelectedPath;
                }
            }

            return result;
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (m_ofd != null)
                        m_ofd.Dispose();
                }
                Disposed = true;
            }
        }

        /// <summary>
        /// Creates IWin32Window around an IntPtr</summary>
        private class WindowWrapper : IWin32Window
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="handle">Handle to wrap</param>
            public WindowWrapper(IntPtr handle)
            {
                Handle = handle;
            }

            /// <summary>
            /// Original ptr</summary>
            public IntPtr Handle { get; private set; }
        }


        /// <summary>
        /// This class is from the Front-End for Dosbox and is used to present a 'vista' dialog box to select folders.
        /// Being able to use a vista style dialog box to select folders is much better then using the shell folder browser.
        /// http://code.google.com/p/fed/
        ///
        /// Example:
        /// var r = new Reflector("System.Windows.Forms");</summary>
        private class Reflector
        {
            private readonly string m_ns;
            private readonly Assembly m_asmb;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="ns">The namespace containing types to be used</param>
            public Reflector(string ns)
                : this(ns, ns)
            { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="an">A specific assembly name (used if the assembly name does not tie exactly with the namespace)</param>
            /// <param name="ns">The namespace containing types to be used</param>
            private Reflector(string an, string ns)
            {
                m_ns = ns;
                m_asmb = null;
                foreach (AssemblyName aN in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    if (aN.FullName.StartsWith(an))
                    {
                        m_asmb = Assembly.Load(aN);
                        break;
                    }
                }
            }

            /// <summary>
            /// Return a Type instance for a type 'typeName'
            /// </summary>
            /// <param name="typeName">The name of the type</param>
            /// <returns>A type instance</returns>
            public Type GetType(string typeName)
            {
                Type type = null;
                string[] names = typeName.Split('.');

                if (names.Length > 0)
                    type = m_asmb.GetType(m_ns + "." + names[0]);

                for (int i = 1; i < names.Length; ++i)
                {
                    type = type.GetNestedType(names[i], BindingFlags.NonPublic);
                }
                return type;
            }

            /// <summary>
            /// Create a new object of a named type passing along any params
            /// </summary>
            /// <param name="name">The name of the type to create</param>
            /// <param name="parameters"></param>
            /// <returns>An instantiated type</returns>
            public object New(string name, params object[] parameters)
            {
                Type type = GetType(name);

                ConstructorInfo[] ctorInfos = type.GetConstructors();
                foreach (ConstructorInfo ci in ctorInfos)
                {
                    try
                    {
                        return ci.Invoke(parameters);
                    }
                    catch { }
                }

                return null;
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' passing parameters 'parameters'
            /// </summary>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object Call(object obj, string func, params object[] parameters)
            {
                return Call2(obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' passing parameters 'parameters'
            /// </summary>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            private object Call2(object obj, string func, object[] parameters)
            {
                return CallAs2(obj.GetType(), obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
            /// </summary>
            /// <param name="type">The type of 'obj'</param>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object CallAs(Type type, object obj, string func, params object[] parameters)
            {
                return CallAs2(type, obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
            /// </summary>
            /// <param name="type">The type of 'obj'</param>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object CallAs2(Type type, object obj, string func, object[] parameters)
            {
                MethodInfo methInfo = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return methInfo.Invoke(obj, parameters);
            }

            /// <summary>
            /// Returns an enum value
            /// </summary>
            /// <param name="typeName">The name of enum type</param>
            /// <param name="name">The name of the value</param>
            /// <returns>The enum value</returns>
            public object GetEnum(string typeName, string name)
            {
                Type type = GetType(typeName);
                FieldInfo fieldInfo = type.GetField(name);
                return fieldInfo.GetValue(null);
            }
        }
    }
}
