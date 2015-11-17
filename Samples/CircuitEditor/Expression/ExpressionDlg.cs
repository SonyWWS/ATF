//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// A Simple dialog for adding/editing expressions.
    /// Users are encouraged to change this dialog to better
    /// meet specific use case.    
    /// </summary>
    partial class ExpressionDlg : Form
    {
        private ExpressionManager m_mgr;
        private int m_numOfOperation;

        // a simple counter to tell how many 
        // transactions are performed so far.
        // It is used for limiting the number of Undos
        // need to be performed when user cancels this dialog box.
        // Note: It is only needed because this is a modal dialog.
        private Expression m_currentExpression;


        public ExpressionDlg(ExpressionManager mgr)
        {
            if (mgr == null)
                throw new ArgumentNullException("mgr");

            m_mgr = mgr;
            InitializeComponent();
            m_objectAttribute.Text = string.Empty;
            m_copyBtn.Enabled = false;
            m_objectList.DisplayMember = "Id";
            m_exprList.DisplayMember = "Id";
            m_propList.DisplayMember = "Name";
            
            m_tabControl.Selected += (sender, e) => UpdateActiveTabPage();
            m_objectList.SelectedIndexChanged += m_objectList_SelectedIndexChanged;
            m_propList.SelectedIndexChanged += (sender, e) =>
                {
                    var node = (DynamicDomNode)m_objectList.SelectedItem;
                    var propDescr = (AttributePropertyDescriptor)m_propList.SelectedItem;
                    if (node != null && propDescr != null)
                        m_objectAttribute.Text = string.Format("editor.{0}.{1}", node.Id, propDescr.Name);
                    else
                        m_objectAttribute.Text = string.Empty;
                };

            m_objectAttribute.TextChanged += (sender, e) => m_copyBtn.Enabled = !string.IsNullOrWhiteSpace(m_objectAttribute.Text);
            m_copyBtn.Click += (sender, e) => Clipboard.SetText(m_objectAttribute.Text);
            m_applyBtn.Click += (sender, e) => ApplyChanges();
            m_okBtn.Click += (sender, e) => ApplyChanges();
            m_cancelBtn.Click += (sender, e) => UndoAll();
            m_exprList.SelectedIndexChanged += (sender, e) =>
                {
                    var expr = (Expression)m_exprList.SelectedItem;
                    SetActiveExpression(expr);
                };


            m_exprList.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Delete)
                    {
                        var expr = (Expression)m_exprList.SelectedItem;
                        DeleteExpression(expr);
                    }
                };



            var delExprMenu = new ToolStripMenuItem("Delete".Localize());
            delExprMenu.Click += (sender, e) =>
                {
                    var expr = (Expression)m_exprList.SelectedItem;
                    DeleteExpression(expr);
                };

            m_exprList.ContextMenuStrip = new ContextMenuStrip();            
            m_exprList.ContextMenuStrip.Opening += (sender, e) => e.Cancel = m_exprList.SelectedIndex < 0;
            m_exprList.ContextMenuStrip.Items.Add(delExprMenu);
            
            UpdateActiveTabPage();
            
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            // need to handle undo here because
            // this is a modal dialog.           
            Keys undoKey = Keys.Control | Keys.Z;
            if (keyData == undoKey)
            {
                if (m_numOfOperation > 0)
                {
                    m_numOfOperation--;
                    HistoryContext hc = m_mgr.Cast<HistoryContext>();
                    if(hc.CanUndo) hc.Undo();
                }
                UpdateActiveTabPage();    
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void UndoAll()
        {
            HistoryContext hc = m_mgr.Cast<HistoryContext>();
            while (m_numOfOperation > 0)
            {
                m_numOfOperation--;
                if (hc.CanUndo) hc.Undo();                
            }
        }

        private void ApplyChanges()
        {
            if (m_currentExpression != null)
            {
                if (m_currentExpression.Id != m_exprIdTxt.Text)
                    throw new Exception("UI is out of sync with data");
                // apply changes.
                m_mgr.As<ITransactionContext>().DoTransaction(delegate
                {
                    m_currentExpression.Label = m_exprLabelTxt.Text;
                    m_currentExpression.Script = m_exprTxt.Text;

                }, "Edit expression".Localize()
                );
                m_numOfOperation++;
            }
            else if(!string.IsNullOrWhiteSpace(m_exprTxt.Text))
            {
                
                Expression expr = m_mgr.CreateExpression();
                expr.Label = m_exprLabelTxt.Text;
                expr.Script = m_exprTxt.Text;
                m_mgr.As<ITransactionContext>().DoTransaction(delegate
                {
                    m_mgr.Expressions.Add(expr);                   
                }, "Add expression".Localize()
               );
                m_numOfOperation++;
                SetActiveExpression(expr);                
            }
        }

        private void DeleteExpression(Expression expr)
        {
            if (expr == null) return;
            m_mgr.As<ITransactionContext>().DoTransaction(delegate
               {
                   m_mgr.Expressions.Remove(expr);
               }, "Delete Expression");
            m_numOfOperation++;
            UpdateActiveTabPage();
        }
        private void SetActiveExpression(Expression expr = null)
        {            
            if (expr != null)
            {
                m_currentExpression = expr;
                m_exprLabelTxt.Text = expr.Label;
                m_exprIdTxt.Text = expr.Id;
                m_exprTxt.Text = expr.Script;
            }
            else
            {
                m_currentExpression = null;
                m_exprTxt.Text = string.Empty;
                m_exprLabelTxt.Text = string.Empty;
                m_exprIdTxt.Text = string.Empty;
            }
        }

        private void UpdateActiveTabPage()
        {
            if (m_tabControl.SelectedTab == m_objectPage)
            {
                // populated list of objects                
                m_objectList.BeginUpdate();
                m_objectList.Items.Clear();
                //m_objectList.Items.AddRange(m_mgr.ExpressionNodes);
                foreach (var exo in m_mgr.ExpressionNodes)
                    m_objectList.Items.Add(new DynamicDomNode(exo));
                m_objectList.EndUpdate();
                if (m_objectList.Items.Count > 0)
                    m_objectList.SelectedIndex = 0;
                //var circuit = m_mgr.as


            }
            else if (m_tabControl.SelectedTab == m_exprPage)
            {
                m_exprList.BeginUpdate();
                m_exprList.Items.Clear();
                foreach (var expr in m_mgr.Expressions)
                    m_exprList.Items.Add(expr);
                m_exprList.EndUpdate();
                SetActiveExpression();            
            }
        }

        void m_objectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var node = (DynamicDomNode)m_objectList.SelectedItem;
            m_propList.BeginUpdate();
            m_propList.Items.Clear();
            if (node != null)
                foreach (var descr in node.Descriptors)
                    m_propList.Items.Add(descr);
            m_propList.EndUpdate();

            if (m_propList.Items.Count > 0)
                m_propList.SelectedIndex = 0;

        }
    }
}
