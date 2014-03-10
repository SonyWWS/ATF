//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component for standard printing commands: File/Print, File/PageSetup, and File/PrintPreview</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardPrintCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardPrintCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardPrintCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Register file print menu commands
            m_commandService.RegisterCommand(CommandInfo.FilePrint, this);
            m_commandService.RegisterCommand(CommandInfo.FilePageSetup, this);
            m_commandService.RegisterCommand(CommandInfo.FilePrintPreview, this);
        }

        #endregion

        /// <summary>
        /// Shows the print dialog</summary>
        public void ShowPrintDialog()
        {
            PrintDialog dialog = new PrintDialog();
            PrintDocument printDocument = GetPrintDocument();
            dialog.Document = printDocument;
            dialog.AllowCurrentPage = true;
            dialog.AllowSelection = true;
            dialog.AllowSomePages = true;
            dialog.UseEXDialog = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        /// <summary>
        /// Shows the page settings dialog</summary>
        public void ShowPageSettingsDialog()
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            PrintDocument printDocument = GetPrintDocument();
            pageSetupDialog.Document = printDocument;
            pageSetupDialog.ShowDialog();
        }

        /// <summary>
        /// Shows the print preview dialog</summary>
        public void ShowPrintPreviewDialog()
        {
            PrintPreviewDialog dialog = new PrintPreviewDialog();
            PrintDocument printDocument = GetPrintDocument();
            dialog.Document = printDocument;
            dialog.ShowDialog();
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool enabled = false;
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.PrintPreview:
                    case StandardCommand.PageSetup:
                    case StandardCommand.Print:
                        IPrintableDocument printableDocument = m_contextRegistry.GetActiveContext<IPrintableDocument>();
                        enabled = printableDocument != null;
                        break;
                }
            }
 
            return enabled;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.PageSetup:
                        ShowPageSettingsDialog();
                        break;

                    case StandardCommand.PrintPreview:
                        ShowPrintPreviewDialog();
                        break;

                    case StandardCommand.Print:
                        ShowPrintDialog();
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private PrintDocument GetPrintDocument()
        {
            IPrintableDocument printableDocument = m_contextRegistry.GetActiveContext<IPrintableDocument>();
            PrintDocument result = printableDocument.GetPrintDocument();
            if (result == null)
                throw new InvalidOperationException("Printable documents must produce a PrintDocument");

            return result;
        }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
