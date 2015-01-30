//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

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
            m_commandListbox = new CommandList();
            
            m_commandListbox.MouseDown += (sender, e) =>
                {
                    var item = m_commandListbox.GetItemAt(e.X, e.Y);
                    if (item == null) return;                   
                    int indexLastDone = m_commandHistory.Current - 1;
                    int cmdIndex = item.Index - 1;
                    if (cmdIndex <= indexLastDone)
                    {
                        while (cmdIndex < indexLastDone)
                        {
                            m_historyContext.Undo();
                            indexLastDone = m_commandHistory.Current - 1;
                        }
                    }
                    else
                    {
                        while (cmdIndex >= m_commandHistory.Current)
                            m_historyContext.Redo();
                    }
                };


            m_commandListbox.RetrieveCommandListItem += (sender, e) =>
                {
                    e.Item = e.ItemIndex == 0 ? new CommandListItem("< Clean State >".Localize())
                        : new CommandListItem(m_commandHistory[e.ItemIndex-1].Description);                    
                };
            
            m_commandListbox.DrawItem += (sender, e) =>
                {                    
                    if (e.Item.Index < 0) return;
                    int cmdIndex = e.Item.Index-1;
                    Rectangle bound = e.Item.Bounds;
                    var cmd = e.Item;
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
                    e.Graphics.DrawString(cmd.Text, m_commandListbox.Font, m_textBrush,
                            bound, StringFormat.GenericDefault);                    
                };

            ControlInfo cinfo = new ControlInfo("History", "Undo/Redo stack", StandardControlGroup.Right);
            m_controlHostService.RegisterControl(m_commandListbox, cinfo, null);
            m_documentRegistry.ActiveDocumentChanged += m_documentRegistry_ActiveDocumentChanged;

            m_commandListbox.BackColorChanged += (sender, e) => ComputeColors();
            m_commandListbox.ForeColorChanged += (sender, e) => ComputeColors();            
            ComputeColors();
        }

        #endregion
              
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
            if (m_commandHistory != null && m_commandHistory.Count > 0)
                m_commandListbox.VirtualListSize = m_commandHistory.Count + 1;
            else
                m_commandListbox.VirtualListSize = 0;                    
        }
        private void m_commandHistory_CommandUndone(object sender, EventArgs e)
        {
            m_commandListbox.Invalidate();            
        }
        private void m_commandHistory_CommandDone(object sender, EventArgs e)
        {
            m_commandListbox.VirtualListSize = m_commandHistory.Count + 1;
            m_commandListbox.Invalidate();
        }
        
        private void ComputeColors()
        {           
            m_undoForeColor = m_commandListbox.ForeColor;            
            m_undoBackColor = m_commandListbox.BackColor;

            m_redoForeColor = m_undoForeColor.GetBrightness() > 0.5f ?
                ControlPaint.Dark(m_undoForeColor, 0.3f): ControlPaint.Light(m_undoForeColor, 0.3f);

            m_redoBackColor = m_undoBackColor.GetBrightness() > 0.5f ?
                ControlPaint.Dark(m_undoBackColor, 0.15f) : ControlPaint.Light(m_undoBackColor, 0.15f);
            
        }

        [Import(AllowDefault = false)]
        private IDocumentRegistry m_documentRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;
        
        private CommandHistory m_commandHistory;
        private HistoryContext m_historyContext;
        private CommandList m_commandListbox;
        private SolidBrush m_fillBrush = new SolidBrush(Color.White);
        private SolidBrush m_textBrush = new SolidBrush(Color.White);
        private Color m_redoForeColor;
        private Color m_redoBackColor;
        private Color m_undoForeColor;
        private Color m_undoBackColor;
        
        #region CommandList classes

        private class RetrieveCommandListItemEventArgs : EventArgs
        {
            public RetrieveCommandListItemEventArgs(int index)
            {
                ItemIndex = index;
            }
            public readonly int ItemIndex;
            public CommandListItem Item;
        }
        private class DrawCommandListItemEventArgs : EventArgs
        {
            public DrawCommandListItemEventArgs(Graphics graphics, CommandListItem item)
            {
                Graphics = graphics;
                Item = item;                
            }

            public readonly Graphics Graphics;
            public readonly CommandListItem Item;                        
        }
        
        private class CommandListItem 
        {
            public CommandListItem(string text)
            {
                Text = text;
            }
            public readonly string Text;
            public Rectangle Bounds
            {
                get;
                private set;
            }
            public int Index
            {
                get;
                private set;
            }

            #region members only useb by CommandList
            public void SetBounds(Rectangle bounds)
            {
                Bounds = bounds;
            }
            public void SetIndex(int index)
            {
                Index = index;
            }
            #endregion
        }

        /// <summary>
        /// Simple item list view control</summary>
        private class CommandList : Control
        {
            public EventHandler<DrawCommandListItemEventArgs> DrawItem;
            public EventHandler<RetrieveCommandListItemEventArgs> RetrieveCommandListItem;

            public CommandList()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.UserPaint
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw
                   , true);

                m_vScrollBar = new VScrollBar();
                m_vScrollBar.Dock = DockStyle.Right;                
                Controls.Add(m_vScrollBar);

                SizeChanged += (sender, e) => UpdateScrollBar();
                m_vScrollBar.ValueChanged += (sender, e) => Invalidate();
            }
            
            public int VirtualListSize
            {
                get{return m_virtualListSize;}
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value");
                    if (value == m_virtualListSize)
                        return;
                    
                    m_virtualListSize = value;
                    UpdateScrollBar();                    
                    Invalidate();
                }
            }

            public CommandListItem GetItemAt(int x, int y)
            {                
                if (y < 0
                    || y >= ClientSize.Height
                    || VirtualListSize == 0)
                    return null;

                int startIndex = GetTopIndex();
                int itemWidth = GetItemWidth();
                int itemHeight = GetItemHeight();                                
                int itemNumber = y / itemHeight;
                int itemIndex = startIndex + itemNumber;
                if (itemIndex >= VirtualListSize) 
                    return null;

                var re = new RetrieveCommandListItemEventArgs(itemIndex);
                OnRetrieveCommandListItem(re);
                re.Item.SetIndex(itemIndex);
                re.Item.SetBounds(new Rectangle(0, itemNumber * itemHeight, itemWidth, itemHeight));
                return re.Item;
            }

            protected override void OnPaint(PaintEventArgs e)
            {                
                if (VirtualListSize == 0) return;
                int itemWidth = GetItemWidth();
                int itemHeight = GetItemHeight();
                int pagesize = (ClientSize.Height % itemHeight) == 0 ?
                    (ClientSize.Height / itemHeight) : (ClientSize.Height / itemHeight) + 1;
                int startIndex = GetTopIndex();
                int endIndex = Math.Min(startIndex+pagesize, VirtualListSize);

                Rectangle bound = new Rectangle(0, 0, itemWidth, itemHeight);

                for (int index = startIndex; index < endIndex; index++)
                {
                    var re = new RetrieveCommandListItemEventArgs(index);
                    OnRetrieveCommandListItem(re);
                    re.Item.SetIndex(index);
                    re.Item.SetBounds(bound);
                    var de = new DrawCommandListItemEventArgs(e.Graphics,re.Item);
                    OnDrawItem(de);
                    bound.Y +=itemHeight;
                }
            }

            private int GetItemHeight()
            {
                return (int)Font.GetHeight() + 2;
            }
            private void OnRetrieveCommandListItem(RetrieveCommandListItemEventArgs e)
            {
                RetrieveCommandListItem.Raise(this, e);
            }
            private void OnDrawItem(DrawCommandListItemEventArgs e)
            {                
                DrawItem.Raise(this, e);
            }

            private VScrollBar m_vScrollBar;
            private int m_virtualListSize;
            
            private int GetTopIndex()
            {
                return m_vScrollBar.Visible ? m_vScrollBar.Value : 0;
            }
            private int GetItemWidth()
            {
                return m_vScrollBar.Visible ? ClientSize.Width - m_vScrollBar.Width - 1
                    : ClientSize.Width - 1;
            }
            private void UpdateScrollBar()
            {                
                int itemHieght = GetItemHeight();               
                m_vScrollBar.Minimum = 0;
                m_vScrollBar.Maximum = Math.Max(VirtualListSize - 1, 0);
                m_vScrollBar.LargeChange = (ClientSize.Height / itemHieght);
                m_vScrollBar.Visible = m_vScrollBar.Maximum > 0 && m_vScrollBar.LargeChange <= m_vScrollBar.Maximum;
                if (!m_vScrollBar.Visible)
                {
                    m_vScrollBar.Value = 0;
                }
                else
                {
                    m_vScrollBar.Value = Math.Min(m_vScrollBar.Value, m_vScrollBar.Maximum-m_vScrollBar.LargeChange);
                }
            }
        }
        #endregion
    }
}