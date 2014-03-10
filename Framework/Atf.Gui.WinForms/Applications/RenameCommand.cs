//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that defines a mass rename command</summary>
    [Export(typeof(RenameCommand))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenameCommand : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public RenameCommand(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Creates a new name by parsing 'original' and replacing pieces or adding to it.</summary>
        /// <param name="original">The original name. Can't be null.</param>
        /// <param name="prefix">Optional prefix that is placed at the beginning of the result. Can be null.
        /// If 'original' already has the prefix, it won't be added a 2nd time.</param>
        /// <param name="baseName">Optional base name that is concatenated with the prefix. If null, then
        /// the corresponding part of 'original' is used, otherwise 'original' is ignored.</param>
        /// <param name="suffix">Optional suffix that is placed after the base name. Can be null.</param>
        /// <param name="numericSuffix">Optional numeric suffix that is placed after 'suffix'.
        /// If negative, then this parameter is ignored. If non-negative, then any existing numeric
        /// suffix on 'original' will be removed.</param>
        /// <returns>The new name</returns>
        /// <note>I considered using a nullable 'long' for numericSuffix, but it's less convenient for
        /// the caller and for unit tests, and I think that negative #s for suffixes are very rare. --Ron</note>
        public static string Rename(string original,
            string prefix, string baseName, string suffix, long numericSuffix
            #if CS_4
            = -1
            #endif
            )
        {
            // Do we keep the existing base? If not, then set original to be just the base portion.
            if (baseName != null)
            {
                original = baseName;
            }
            else
            {
                // Remove prefix?
                if (!string.IsNullOrEmpty(prefix) && original.StartsWith(prefix))
                    original = original.Remove(0, prefix.Length);

                // Remove numeric suffix?
                if (numericSuffix >= 0)
                {
                    int lastNonDigitIndex = original.Length - 1;
                    while (lastNonDigitIndex >= 0 && char.IsDigit(original[lastNonDigitIndex]))
                        lastNonDigitIndex--;
                    if (lastNonDigitIndex < original.Length - 1)
                        original = original.Remove(lastNonDigitIndex + 1);
                }

                // Now that the #s are gone, should we remove the suffix, too?
                if (!string.IsNullOrEmpty(suffix) && original.EndsWith(suffix))
                    original = original.Remove(original.Length - suffix.Length, suffix.Length);
            }

            // 'original' is now the correct base.
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix))
                sb.Append(prefix);
            if (!string.IsNullOrEmpty(original))
                sb.Append(original);
            if (!string.IsNullOrEmpty(suffix))
                sb.Append(suffix);
            if (numericSuffix >= 0)
                sb.Append(numericSuffix.ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                new CommandInfo(
                    Command.Rename,
                    StandardMenu.Edit,
                    StandardCommandGroup.EditOther,
                    "Rename...".Localize("Rename selected objects"),
                    "Rename selected objects".Localize()),
                this);

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(this, () => this.Settings, "Settings", null, null));
            }
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            // The dialog box is modal and not dockable, so only allow it to pop up if it can be used.
            bool canDo = false;
            if (Command.Rename.Equals(commandTag))
            {
                // Note that the ITransactionContext can be null, so we don't need to check it here.
                var selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                var namingContext = m_contextRegistry.GetActiveContext<INamingContext>();

                if (selectionContext != null &&
                    namingContext != null)
                {
                    foreach (object item in selectionContext.Selection)
                    {
                        if (namingContext.CanSetName(item))
                        {
                            canDo = true;
                            break;
                        }
                    }
                }
            }
            return canDo;
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            var selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            var namingContext = m_contextRegistry.GetActiveContext<INamingContext>();
            var transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
            
            using (var dialog = new RenameCommandDialog())
            {
                dialog.Set(selectionContext, namingContext, transactionContext);
                dialog.Settings = Settings;

                // Keep the dialog box on top of this application only, not all apps. Make it modal
                //  unless we want to make it handle context switches and be dockable.
                dialog.ShowDialog(GetDialogOwner());
                Settings = dialog.Settings;
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private string Settings
        {
            get;
            set;
        }

        private IWin32Window GetDialogOwner()
        {
            if (m_mainWindow != null)
                return m_mainWindow.DialogOwner;
            else if (m_mainForm != null)
                return m_mainForm;

            return null;
        }

        private enum Command
        {
            Rename,
        }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;
    }
}

