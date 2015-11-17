//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace CircuitEditorSample
{    
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ExpressionCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
             m_commandService.RegisterCommand(
                 ExprCommands.Expression,
                 StandardMenu.Edit,
                 StandardCommandGroup.EditOther,
                 "Expression".Localize(), 
                 "Create and edit expressions".Localize(), 
                 Keys.None,
                 null, 
                 CommandVisibility.All,
                 this);


             m_documentRegistry.DocumentAdded += (sender, e) =>
                     {
                         ExpressionManager em = e.Item.As<ExpressionManager>();
                         if (em != null) em.SetPythonScriptService(m_scriptingService);
                     };

             // run expressions if needed.
             Application.Idle += (sender, e) =>
             {
                 if (m_documentRegistry == null) return;
                 foreach (var doc in m_documentRegistry.Documents)
                 {
                     ExpressionManager em = doc.As<ExpressionManager>();
                     if (em != null) em.Update();
                 }
             };             
        }

        #endregion

        #region ICommandClient Members

        public bool CanDoCommand(object commandTag)
        {
            if (ExprCommands.Expression.Equals(commandTag))
                return m_documentRegistry.GetMostRecentDocument<ExpressionManager>() != null;            
            return false;
        }

        public void DoCommand(object commandTag)
        {
            if (ExprCommands.Expression.Equals(commandTag))
            {
                var mgr = m_documentRegistry.GetMostRecentDocument<ExpressionManager>();
                using (ExpressionDlg dlg = new ExpressionDlg(mgr))
                {
                    dlg.ShowDialog();
                }                               
            }
        }

        public void UpdateCommand(object commandTag, CommandState commandState)
        {
            
        }

        #endregion

        #region import required components.

        [Import(AllowDefault = false)]
        private IDocumentRegistry m_documentRegistry = null;

        [Import(AllowDefault = false)]
        private ICommandService m_commandService = null;

        [Import(AllowDefault = false)]
        private ScriptingService m_scriptingService = null;

        private enum ExprCommands
        {
            Expression,
        }


        #endregion

        #region IContextMenuCommandProvider Members

        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            if (CanDoCommand(ExprCommands.Expression))
                yield return ExprCommands.Expression;
        }

        #endregion
    }
}
