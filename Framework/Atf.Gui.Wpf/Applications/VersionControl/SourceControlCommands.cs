//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications.VersionControl;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Component that implements source control commands</summary>
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(SourceControlCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SourceControlCommands : SourceControlCommandsBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document tracking service</param>
        /// <param name="documentService">File menu service</param>
        [ImportingConstructor]
        public SourceControlCommands(
            ICommandService commandService,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
            : base(commandService, documentRegistry, documentService)
        {
        }

        /// <summary>
        /// Perform the Reconcile command</summary>
        /// <param name="doing">True to perform the Reconcile; false to test whether Reconcile can be done</param>
        /// <returns>True iff Reconcile can be done or was done</returns>
        protected override bool DoReconcile(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            if (!doing)
                return SourceControlContext.Resources.Any();

            var uris = SourceControlContext.Resources.Select(resource => resource.Uri).ToList();
            var modified = new List<Uri>();
            var localNotInDepot = new List<Uri>();

            //using (new WaitCursor())
            {
                foreach (Uri uri in SourceControlService.GetModifiedFiles(uris))
                {
                    if (SourceControlService.GetStatus(uri) != SourceControlStatus.CheckedOut)
                        modified.Add(uri);
                }

                foreach (Uri uri in uris)
                {
                    if (!modified.Contains(uri))
                        if (SourceControlService.GetStatus(uri) == SourceControlStatus.NotControlled)
                            localNotInDepot.Add(uri);
                }
            }

            var vm = new ReconcileViewModel(SourceControlService, modified, localNotInDepot);
            DialogUtils.ShowDialogWithViewModel<ReconcileDialog>(vm);

            return true;
        }

        /// <summary>
        /// Display Checkin dialog</summary>
        /// <param name="toCheckIns">List of resources to check in</param>
        protected override void ShowCheckInDialog(IList<IResource> toCheckIns)
        {
            var vm = new CheckInViewModel(SourceControlService, toCheckIns);
            DialogUtils.ShowDialogWithViewModel<CheckInDialog>(vm);
        }
    }
}
