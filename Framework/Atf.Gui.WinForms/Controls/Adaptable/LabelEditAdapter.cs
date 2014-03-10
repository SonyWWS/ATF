//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter for adding in-place label editing to AdaptableControl</summary>
    public class LabelEditAdapter : ControlAdapter, ILabelEditAdapter, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public LabelEditAdapter()
        {
            m_labelEditTimer = new Timer();
            m_labelEditTimer.Tick += editLabelTimer_Tick;

            m_textBox = new TextBox();
            m_textBox.Visible = false;
            m_textBox.BorderStyle = BorderStyle.None;
            m_textBox.TextChanged += textBox_TextChanged;
            m_textBox.LostFocus += textBox_LostFocus;
            m_textBox.PreviewKeyDown += textBox_PreviewKeyDown;
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public virtual void Dispose()
        {
            m_labelEditTimer.Dispose();
        }

        #region ILabelEditAdapter Members

        /// <summary>
        /// Begins a label editing operation</summary>
        /// <param name="namingContext">Naming context that performs naming operations</param>
        /// <param name="item">Item with label</param>
        /// <param name="label">Information about label</param>
        public void BeginEdit(INamingContext namingContext, object item, DiagramLabel label)
        {
            m_namingContext = namingContext;
            m_item = item;
            m_label = label;
            m_labelBounds = label.Bounds;
            float fontScale = 1.0f;
            if (m_transformAdapter != null)
            {
                Matrix transform = m_transformAdapter.Transform;
                m_labelBounds = GdiUtil.Transform(transform, m_labelBounds);
                // in case of non-uniform scaling, prefer vertical (y) scale for magnification factor;
                //  Timeline control is the only example of non-uniform scale right now, and y-scale works
                //  better in this case.
                fontScale *= transform.Elements[3];
            }

            m_textBox.Text = m_namingContext.GetName(m_item);

            Font font = this.AdaptedControl.Font;
            m_textBox.Font = new Font(font.FontFamily, (int)(font.SizeInPoints * fontScale));

            TextFormatFlags flags = m_label.Format;
            m_textBox.Multiline = (flags & TextFormatFlags.SingleLine) == 0;

            HorizontalAlignment alignment = HorizontalAlignment.Left;
            if ((flags & TextFormatFlags.Right) != 0)
                alignment = HorizontalAlignment.Right;
            else if ((flags & TextFormatFlags.HorizontalCenter) != 0)
                alignment = HorizontalAlignment.Center;
            m_textBox.TextAlign = alignment;

            SizeTextBox();

            m_textBox.Visible = true;
            m_textBox.Focus();
                m_textBox.SelectAll();
        }

        /// <summary>
        /// Ends the current label editing operation</summary>
        public void EndEdit()
        {
            m_labelEditTimer.Enabled = false;

            if (m_namingContext == null || m_item == null)
                return;

            if (!m_textBox.Visible)
                return;

            string text = m_textBox.Text;
            if (text != m_namingContext.GetName(m_item))
            {
                var transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                // check that some other transaction hasn't ended our edit
                if (transactionContext == null ||
                    !transactionContext.InTransaction)
                {
                    TransactionContexts.DoTransaction(transactionContext,
                        delegate
                        {
                            m_namingContext.SetName(m_item, text);
                        },
                        "Edit Label".Localize());
                }
            }

            m_textBox.Visible = false;
            m_namingContext = null;
            m_item = null;
            AdaptedControl.Invalidate();
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();
            m_selectionAdapter = control.As<ISelectionAdapter>();
            if (m_selectionAdapter != null)
                m_selectionAdapter.SelectedItemHit += selectionAdapter_SelectedItemHit;

            control.ContextChanged += control_ContextChanged;
            control.Invalidated += control_Invalidated;
            control.KeyDown += control_KeyDown;

            control.Controls.Add(m_textBox);
        }

     

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= control_ContextChanged;
            control.Invalidated -= control_Invalidated;
            control.KeyDown -= control_KeyDown;

            control.Controls.Remove(m_textBox);
        }

        private void selectionAdapter_SelectedItemHit(object sender, DiagramHitEventArgs e)
        {

            m_itemHitRecord = e.HitRecord;
            // hit on diagram label part?
            DiagramLabel hitLabel = e.HitRecord.Part.As<DiagramLabel>();
            if (hitLabel != null)
            {
                INamingContext namingContext = AdaptedControl.ContextAs<INamingContext>();
                if (namingContext != null)
                {
                    // if label editing is enabled, mouse is over label, and item can be named, open it for edit
                    if (namingContext.CanSetName(e.HitRecord.Item))
                    {
                        m_hitLabel = hitLabel;
                        m_labelEditTimer.Interval = SystemInformation.DoubleClickTime;
                        m_labelEditTimer.Enabled = true;
                    }
                }
            }
        }

        private void editLabelTimer_Tick(object sender, EventArgs e)
        {
            m_labelEditTimer.Enabled = false;
            PrepareForEdit();

        }

        private void PrepareForEdit()
        {
            if (!AdaptedControl.Capture)
            {
                INamingContext namingContext = AdaptedControl.ContextAs<INamingContext>();
                BeginEdit(
                    namingContext,
                    m_itemHitRecord.Item,
                    m_hitLabel);
            }

            m_itemHitRecord = null;
            m_hitLabel = null;
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            EndEdit();
        }

        private void control_Invalidated(object sender, InvalidateEventArgs e)
        {
            EndEdit();
        }

        void control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F2)
            {
                if (m_itemHitRecord != null && m_itemHitRecord.DefaultPart.As<DiagramLabel>() != null)
                {
                    m_hitLabel = m_itemHitRecord.DefaultPart.As<DiagramLabel>();
                    PrepareForEdit();
                }

            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            SizeTextBox();
        }

        private void textBox_LostFocus(object sender, EventArgs e)
        {
            EndEdit();
        }

        /// <summary>
        /// Performs custom actions on PreviewKeyDown events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e"> PreviewKeyDown event args</param>
        void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                if (m_namingContext != null && m_item != null)
                {                    
                    m_textBox.Text = m_namingContext.GetName(m_item);
                    EndEdit();
                    AdaptedControl.Invalidate();
                }                              
            }
            else
            {
                if (e.KeyData == Keys.Enter)
                    EndEdit();
            }        
        }

        private void SizeTextBox()
        {
            // find size needed to fit text
            SizeF textSize = TextRenderer.MeasureText(
                m_textBox.Text, m_textBox.Font, m_textBox.ClientRectangle.Size, m_label.Format);

            // make text box big enough, but not smaller than diagram label bounds, so it won't show
            m_textBox.Size = new Size(
                Math.Max((int)textSize.Width, m_labelBounds.Width),
                Math.Max((int)textSize.Height, m_labelBounds.Height));

            // position text box in middle of requested bounds
            Size actualSize = m_textBox.Size;
            m_textBox.Location = new Point(
                m_labelBounds.X + m_labelBounds.Width / 2 - actualSize.Width / 2,
                m_labelBounds.Y + m_labelBounds.Height / 2 - actualSize.Height / 2);
        }

        private class TextBox : System.Windows.Forms.TextBox
        {
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.A:
                        SelectAll();
                        return true;

                    case Keys.Control | Keys.Z:
                        Undo();
                        return true;
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        
        private ITransformAdapter m_transformAdapter;
        private ISelectionAdapter m_selectionAdapter;

        private INamingContext m_namingContext;

        private readonly Timer m_labelEditTimer;
        private DiagramHitRecord m_itemHitRecord;
        private DiagramLabel m_hitLabel;
 
        private object m_item;
        private DiagramLabel m_label;
        private Rectangle m_labelBounds;
        private readonly TextBox m_textBox;
    }
}
