//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;


using Sce.Atf.Dom;

namespace Sce.Atf.Applications
{   
    /// <summary>
    /// Provides visual representation of undo commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(HistoryLister))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HistoryLister : IInitializable
    {

        #region IInitializable Members        
        void IInitializable.Initialize()
        {            
            m_listbox = new CommandList();
            m_listbox.DrawMode = DrawMode.OwnerDrawFixed;
            m_listbox.BorderStyle = BorderStyle.None;
            m_listbox.SelectedIndexChanged += (sender, e) =>
                {
                    try
                    {
                        m_undoingOrRedoing = true;
                        int indexLastDone = m_commandHistory.Current - 1;
                        int cmdIndex = m_listbox.SelectedIndex + m_startIndex;


                        if (cmdIndex <= indexLastDone)
                        {
                            if (m_listbox.SelectedIndex == 0
                                && cmdIndex == indexLastDone
                                && m_commandHistory.Count > 1)
                            {
                                m_historyContext.Undo();                               
                            }
                            else
                            {
                                while (cmdIndex < indexLastDone)
                                {
                                    m_historyContext.Undo();
                                    indexLastDone = m_commandHistory.Current - 1;
                                }
                            }
                        }
                        else
                        {
                            while (cmdIndex >= m_commandHistory.Current)
                                m_historyContext.Redo();
                        }
                    }                    
                    finally
                    {
                        m_undoingOrRedoing = false;                        
                    }
                };


            m_listbox.DrawItem += (sender, e) =>
                {                    
                    if (e.Index < 0) return;                    
                    int cmdIndex = e.Index + m_startIndex;                    
                    Rectangle bound = e.Bounds;
                    Command cmd = (Command)m_listbox.Items[e.Index];
                    if(cmdIndex >= m_commandHistory.Current)
                    {
                        m_textBrush.Color = m_redoForeColor;
                        m_fillBrush.Color = m_redoBackColor;
                    }
                    else
                    {
                        m_textBrush.Color = m_undoForeColor;
                        m_fillBrush.Color = m_undoBackColor;
                    }

                    e.Graphics.FillRectangle(m_fillBrush, bound);                                       
                    e.Graphics.DrawString(cmd.Description, e.Font, m_textBrush,
                            bound, StringFormat.GenericDefault);                                       
                };

            ControlInfo cinfo = new ControlInfo("History", "Undo/Redo stack", StandardControlGroup.Right);
            m_controlHostService.RegisterControl(m_listbox, cinfo, null);
            m_documentRegistry.ActiveDocumentChanged += m_documentRegistry_ActiveDocumentChanged;

            m_listbox.BackColorChanged += (sender, e) => ComputeColors();
            m_listbox.ForeColorChanged += (sender, e) => ComputeColors();

            if (m_settingsService != null)
            {
                var descriptor = new BoundPropertyDescriptor(
                        this,
                        () => MaxCommandCount,
                        "Max Visual Command History Count".Localize(),
                        null,
                        "Maximum number of commands in the visual command history. Minimum value is 10".Localize());

                m_settingsService.RegisterSettings(this, descriptor);
                m_settingsService.RegisterUserSettings("Application", descriptor);

            }
            ComputeColors();
        }

        #endregion

        /// <summary>
        /// Gets or sets maximum number of history commands</summary>
        [DefaultValue(DefaultMaxCommandCount)]
        public int MaxCommandCount
        {
            get { return m_maxCommandCount; }
            set
            {
                m_maxCommandCount  = value;
                if (m_maxCommandCount < 10)
                    m_maxCommandCount = 10;
            }
        }
        private int m_maxCommandCount = DefaultMaxCommandCount;
        private const int DefaultMaxCommandCount = 150;
        private void m_documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (m_commandHistory != null)
            {
                m_commandHistory.CommandDone -= m_commandHistory_CommandDone;
                m_commandHistory.CommandUndone -= m_commandHistory_CommandUndone;
            }

            m_historyContext = m_documentRegistry.GetActiveDocument<HistoryContext>();
            m_commandHistory = m_historyContext == null ? null : m_historyContext.History;

            if (m_commandHistory != null)
            {
                m_commandHistory.CommandDone += m_commandHistory_CommandDone;
                m_commandHistory.CommandUndone += m_commandHistory_CommandUndone;
            }
            
            m_undoingOrRedoing = false;
            m_lastCmdCount = 0;
            m_startIndex = 0;
            m_listbox.Items.Clear();
            if(m_commandHistory != null)
                BuildList();            
        }
        private void m_commandHistory_CommandUndone(object sender, EventArgs e)
        {
            UpdatedSelectedIndex();          
        }
        private void m_commandHistory_CommandDone(object sender, EventArgs e)
        {
            if (m_lastCmdCount != m_commandHistory.Count)
            {
                m_lastCmdCount = m_commandHistory.Count;
                BuildList();            
            }
            else
            {
                UpdatedSelectedIndex();
            }
            
        }
        private void UpdatedSelectedIndex()
        {
            if (m_undoingOrRedoing) return;
            m_listbox.BeginUpdate();
            int selectedIndex = (m_commandHistory.Current - 1) - m_startIndex;
            if (selectedIndex < -1) selectedIndex = -1;
            m_listbox.SelectedIndex = selectedIndex;
            m_listbox.EndUpdate();
        }
        private void BuildList()
        {
            if (m_undoingOrRedoing) return;            
            int cmdCount = m_commandHistory.Count;
            m_startIndex = cmdCount > MaxCommandCount ? cmdCount - MaxCommandCount : 0;
            m_listbox.BeginUpdate();
            m_listbox.Items.Clear();
            m_listbox.ItemHeight = m_listbox.Font.Height + 2;
            for (int i = m_startIndex; i < cmdCount; i++)
            {
                Command cmd = m_commandHistory[i];
                m_listbox.Items.Add(cmd);
            }            
            m_listbox.EndUpdate();
            UpdatedSelectedIndex();
        }
        private void ComputeColors()
        {           
            m_undoForeColor = m_listbox.ForeColor;            
            m_undoBackColor = m_listbox.BackColor;

            m_redoForeColor = m_undoForeColor.GetBrightness() > 0.5f ?
                ControlPaint.Dark(m_undoForeColor, 0.3f): ControlPaint.Light(m_undoForeColor, 0.3f);

            m_redoBackColor = m_undoBackColor.GetBrightness() > 0.5f ?
                ControlPaint.Dark(m_undoBackColor, 0.15f) : ControlPaint.Light(m_undoBackColor, 0.15f);
            
        }

        [Import(AllowDefault = false)]
        private IDocumentRegistry m_documentRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;
        

        private int m_startIndex;
        private int m_lastCmdCount; // last command count.        
        private CommandHistory m_commandHistory;
        private HistoryContext m_historyContext;
        private CommandList m_listbox;
        private SolidBrush m_fillBrush = new SolidBrush(Color.White);
        private SolidBrush m_textBrush = new SolidBrush(Color.White);
        private Color m_redoForeColor;
        private Color m_redoBackColor;
        private Color m_undoForeColor;
        private Color m_undoBackColor;
        private bool m_undoingOrRedoing;        
        private class CommandList : ListBox
        {                       
            public CommandList()
            {
                SetStyle(ControlStyles.UserPaint
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw
                   | ControlStyles.Opaque
                   , true);
            }

            protected override bool IsInputKey(Keys keyData)
            {
                // Disable arrow keys,
                // undo/redo should be done via undo/redo 
                // shortcuts not up/down keys.                
                if (keyData == Keys.Up || keyData == Keys.Down)
                    return false;
                return base.IsInputKey(keyData);
            }
                        
            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                Invalidate();
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
                for (int i = 0; i < Items.Count; i++)
                {                    
                    var itemRect = GetItemRectangle(i);
                    itemRect.Height = ItemHeight;
                    if (e.ClipRectangle.IntersectsWith(itemRect))
                    {
                        if ((this.SelectionMode == SelectionMode.One && this.SelectedIndex == i)
                            || (this.SelectionMode == SelectionMode.MultiSimple && this.SelectedIndices.Contains(i))
                            || (this.SelectionMode == SelectionMode.MultiExtended && this.SelectedIndices.Contains(i)))
                        {                            
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                itemRect, i,
                                DrawItemState.Selected, this.ForeColor,
                                this.BackColor));
                        }
                        else
                        {                         
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                itemRect, i,
                                DrawItemState.Default, this.ForeColor,
                                this.BackColor)); 
                        }
                    }
                }// end of loop
            }

        }
    }
}