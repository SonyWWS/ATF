//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Rename command dialog</summary>
    internal partial class RenameCommandDialog : Form
    {
        public RenameCommandDialog()
        {
            InitializeComponent();

            renameButton.Click += renameButton_Click;
        }

        /// <summary>
        /// Initializes this dialog with the required and optional contexts</summary>
        /// <param name="selectionContext">Selection context -- required</param>
        /// <param name="namingContext">Naming context -- required</param>
        /// <param name="transactionContext">Transaction context -- optional</param>
        /// <remarks>Combine with the constructor? This separate Set() only makes sense if this dialog box
        /// is floating or dockable.</remarks>
        public void Set(
            ISelectionContext selectionContext,
            INamingContext namingContext,
            ITransactionContext transactionContext)
        {
            m_selectionContext = selectionContext;
            m_namingContext = namingContext;
            m_transactionContext = transactionContext;
            UpdatePreview();
        }

        /// <summary>
        /// Gets or sets the persisted settings string</summary>
        public string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("RenameCommandDialogSettings");
                xmlDoc.AppendChild(root);

                root.SetAttribute("setBaseBtn", setBaseBtn.Checked.ToString());
                root.SetAttribute("prefix", prefixTextBox.Text);
                root.SetAttribute("baseName", baseNameTextBox.Text);
                root.SetAttribute("suffix", suffixTextBox.Text);
                root.SetAttribute("setNumericSuffix", numberCheckBox.Checked.ToString());
                root.SetAttribute("suffixNumber", firstNumericUpDown.Value.ToString());

                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                // Attempt to read the settings in the XML format.
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);
                XmlElement root = xmlDoc.DocumentElement;

                setBaseBtn.Checked = root.GetAttribute("setBaseBtn") == "True";
                prefixTextBox.Text = root.GetAttribute("prefix");
                baseNameTextBox.Text = root.GetAttribute("baseName");
                suffixTextBox.Text = root.GetAttribute("suffix");
                numberCheckBox.Checked = root.GetAttribute("setNumericSuffix") == "True";
                firstNumericUpDown.Value = Decimal.Parse(root.GetAttribute("suffixNumber"));
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            List<Pair<object, string>> newNames = CalculateNewNames(m_selectionContext.Selection);
            if (newNames.Count > 0)
            {
                // m_transactionContext can be null when using the DoTransaction() extension method.
                m_transactionContext.DoTransaction(delegate
                    {
                        foreach (Pair<object, string> pair in newNames)
                            m_namingContext.SetName(pair.First, pair.Second);
                    },
                    "Rename selection");
            }
        }

        private void UpdatePreview()
        {
            const int maxNumToPreview = 20;

            // Calculate the new names of renameable objects, up to the max #
            bool tooManyToPreview = false;
            var previewList = new List<object>();
            foreach (object item in m_selectionContext.Selection)
            {
                if (m_namingContext.CanSetName(item))
                {
                    if (previewList.Count == maxNumToPreview)
                    {
                        tooManyToPreview = true;
                        break;
                    }
                    previewList.Add(item);
                }
            }
            List<Pair<object, string>> newNames = CalculateNewNames(previewList);

            // Create the preview string
            var preview = new StringBuilder();
            foreach (Pair<object, string> pair in newNames)
            {
                string original = m_namingContext.GetName(pair.First);
                string newName = pair.Second;
                preview.AppendLine(original + " => " + newName);
            }
            if (tooManyToPreview)
                preview.AppendLine("...");

            // Update the text box and the rename button
            if (newNames.Count > 0)
            {
                previewTextBox.Text = preview.ToString();
                renameButton.Enabled = true;
            }
            else
            {
                renameButton.Enabled = false;
            }
        }

        // Calculates a list of objects that can be renamed, along with their new names.
        private List<Pair<object, string>> CalculateNewNames(IEnumerable<object> items)
        {
            var result = new List<Pair<object, string>>();
            bool setBaseName = setBaseBtn.Checked;
            string baseName = baseNameTextBox.Text;
            string prefix = prefixTextBox.Text;
            string suffix = suffixTextBox.Text;
            bool setNumericSuffix = numberCheckBox.Checked;
            long suffixNumber = (long)firstNumericUpDown.Value;

            foreach (object item in items)
            {
                if (m_namingContext.CanSetName(item))
                {
                    string original = m_namingContext.GetName(item);
                    string newName = RenameCommand.Rename(
                        original, prefix, setBaseName ? baseName : null,
                        suffix, setNumericSuffix ? suffixNumber : -1);
                    suffixNumber++;

                    result.Add(new Pair<object, string>(item, newName));
                }
            }

            return result;
        }

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    // this is to prevent the form from being disposed
        //    e.Cancel = true;
        //    Hide();

        //    base.OnFormClosing(e);
        //}

        private void setBaseBtn_CheckedChanged(object sender, EventArgs e)
        {
            keepBaseBtn.Checked = !setBaseBtn.Checked;
            baseNameTextBox.Enabled = setBaseBtn.Checked;
            UpdatePreview();
        }

        // Let setBaseBtn_CheckedChanged be in charge of calling UpdatePreview(). We don't want
        //  UpdatePreview() being called multiple times.
        private void keepBaseBtn_CheckedChanged(object sender, EventArgs e)
        {
            setBaseBtn.Checked = !keepBaseBtn.Checked;
        }

        private void numberCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            plusNumberLabel.Enabled = numberCheckBox.Checked;
            firstLabel.Enabled = numberCheckBox.Checked;
            firstNumericUpDown.Enabled = numberCheckBox.Checked;
            UpdatePreview();
        }

        private void prefixTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void baseNameTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void suffixTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void firstNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private ISelectionContext m_selectionContext;
        private INamingContext m_namingContext;
        private ITransactionContext m_transactionContext;//can be null
    }
}