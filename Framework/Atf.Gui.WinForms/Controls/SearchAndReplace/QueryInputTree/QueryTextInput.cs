//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// ToolStrip text box for search query</summary>
    public class QueryTextInput : QueryNode
    {
        /// <summary>
        /// Event that is raised when text entered in ToolStrip text box</summary>
        public event EventHandler TextEntered;

        /// <summary>
        /// Event that is raised when text value in ToolStrip text box changes</summary>
        public event EventHandler TextChanged;

        private QueryTextInput() { m_numericalTextInput = false; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="numericalTextInput">Whether text input to text box is numeric or not</param>
        public QueryTextInput(bool numericalTextInput) { m_numericalTextInput = numericalTextInput; }

        /// <summary>
        /// Gets text in ToolStrip text box</summary>
        public string InputText { get { return ToolStripTextBox.Text; } }

        private ToolStripTextBox ToolStripTextBox
        {
            get
            {
                if (m_toolStripSpringTextBox == null)
                {
                    m_toolStripSpringTextBox = new ToolStripTextBox();
                    m_toolStripSpringTextBox.Name = "toolStripTextBox1";
                    m_toolStripSpringTextBox.Size = new System.Drawing.Size(100, 25);
                    m_toolStripSpringTextBox.KeyDown += textBox_KeyDown;
                    m_toolStripSpringTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    m_toolStripSpringTextBox.BorderStyle = BorderStyle.FixedSingle;
                    m_toolStripSpringTextBox.TextBoxTextAlign = HorizontalAlignment.Center;
                    m_toolStripSpringTextBox.Margin = new Padding(6,2,6,2);
                    m_toolStripSpringTextBox.TextChanged += ToolStripSpringTextBoxOnTextChanged;
                }
                return m_toolStripSpringTextBox;
            }
        }

        private void ToolStripSpringTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            TextChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds ToolStrip text box for this instance to a list of ToolStrip items</summary>
        /// <param name="items">List of ToolStripItems</param>
        public override void GetToolStripItems(List<ToolStripItem> items)
        {
            items.Add(ToolStripTextBox);
        }

        /// <summary>
        /// Gets ToolStrip text box's ToolStripItem</summary>
        /// <returns>ToolStripTextBox for this QueryTextInput</returns>
        public override ToolStripItem GetToolStripItem()
        {
            return ToolStripTextBox;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_numericalTextInput)
                ValidateNumericalTextInput(e);
            else
                ValidateStringTextInput(e);
        }

        private void ValidateStringTextInput(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    TextEntered.Raise(this, EventArgs.Empty);
                    break;

                default:
                    break;
            }
        }

        private void ValidateNumericalTextInput(KeyEventArgs e)
        {
            bool suppressKey = false;

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    TextEntered.Raise(this, EventArgs.Empty);
                    suppressKey = false;
                    break;

                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.Delete:
                case Keys.Back:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    suppressKey = false;
                    break;

                case Keys.OemPeriod:
                case Keys.Decimal:
                    suppressKey = m_toolStripSpringTextBox.Text.Contains(".");
                    break;

                case Keys.Subtract:
                case Keys.OemMinus:
                    suppressKey = m_toolStripSpringTextBox.Text.Length != 0;
                    break;

                default:
                    suppressKey = true;
                    break;
            }

            e.SuppressKeyPress = suppressKey;
        }

        private ToolStripTextBox m_toolStripSpringTextBox;
        private readonly bool m_numericalTextInput;
    }

    /// <summary>
    /// ToolStrip drop down button for search query for non-numeric input</summary>
    public class QueryStringInput : QueryOption
    {
        /// <summary>
        /// Constructor</summary>
        private QueryStringInput() {}

        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Parent QueryNode</param>
        /// <param name="stringQueryOptions">String search types</param>
        public QueryStringInput(QueryNode parentNode, StringQuery stringQueryOptions)
        {
            m_textInput = null;

            parentNode.Add(this);

            // get root node
            QueryNode nodeAbove = parentNode;
            while (nodeAbove.Parent != null)
                nodeAbove = nodeAbove.Parent as QueryNode;

            // register 'option changed' event to root node
            QueryRoot rootNode = nodeAbove as QueryRoot;
            if (rootNode != null)
                rootNode.RegisterQueryOption(this);

            if (stringQueryOptions != StringQuery.None)
            {
                QueryOptionItem newOptionItem;

                // "regular expression"
                if ((stringQueryOptions & StringQuery.RegularExpression) != 0)
                {
                    newOptionItem = this.AddOptionItem("matches the regular expression", (UInt64)StringQuery.RegularExpression);
                    m_textInput = newOptionItem.AddStringSearchTextInput(m_textInput);
                }

                // "contains"
                if ((stringQueryOptions & StringQuery.Contains) != 0)
                {
                    newOptionItem = this.AddOptionItem("contains", (UInt64)StringQuery.Contains);
                    m_textInput = newOptionItem.AddStringSearchTextInput(m_textInput);
                }

                // "matches"
                if ((stringQueryOptions & StringQuery.Matches) != 0)
                {
                    newOptionItem = this.AddOptionItem("is", (UInt64)StringQuery.Matches);
                    m_textInput = newOptionItem.AddStringSearchTextInput(m_textInput);
                }

                // "begins with"
                if ((stringQueryOptions & StringQuery.BeginsWith) != 0)
                {
                    newOptionItem = this.AddOptionItem("begins with", (UInt64)StringQuery.BeginsWith);
                    m_textInput = newOptionItem.AddStringSearchTextInput(m_textInput);
                }

                // "ends with"
                if ((stringQueryOptions & StringQuery.EndsWith) != 0)
                {
                    newOptionItem = this.AddOptionItem("ends with", (UInt64)StringQuery.EndsWith);
                    m_textInput = newOptionItem.AddStringSearchTextInput(m_textInput);
                }
            }
        }

        /// <summary>
        /// Gets text in QueryTextInput's text box</summary>
        public string TextInput
        {
            get { return m_textInput.InputText; }
        }

        /// <summary>
        /// Builds search predicate for the text box entry</summary>
        /// <param name="predicate">Test conditions and value replacement info for query</param>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            // Override to create some sort of search predicate for the text box entry
        }

        private readonly QueryTextInput m_textInput;
    }

    /// <summary>
    /// ToolStrip drop down button for search query for numeric input</summary>
    public class QueryNumericalInput : QueryOption
    {
        /// <summary>
        /// Constructor</summary>
        private QueryNumericalInput() { }

        /// <summary>
        /// Constructor</summary>
        /// <param name="parentNode">Parent QueryNode</param>
        /// <param name="numericalQueryOptions">Numerical search types</param>
        public QueryNumericalInput(QueryNode parentNode, NumericalQuery numericalQueryOptions)
        {
            m_textInput1 = null;
            m_textInput2 = null;

            parentNode.Add(this);

            // get root node
            QueryNode nodeAbove = parentNode;
            while (nodeAbove.Parent != null)
                nodeAbove = nodeAbove.Parent as QueryNode;

            // register 'option changed' event to root node
            QueryRoot rootNode = nodeAbove as QueryRoot;
            if (rootNode != null)
                rootNode.RegisterQueryOption(this);

            if (numericalQueryOptions != NumericalQuery.None)
            {
                QueryOptionItem newOptionItem;

                // "equals"
                if ((numericalQueryOptions & NumericalQuery.Equals) != 0)
                {
                    newOptionItem = this.AddOptionItem("equals", (UInt64)NumericalQuery.Equals);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                }

                // "lesser than"
                if ((numericalQueryOptions & NumericalQuery.Lesser) != 0)
                {
                    newOptionItem = this.AddOptionItem("is less than", (UInt64)NumericalQuery.Lesser);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                }

                // "lesser than or equal to"
                if ((numericalQueryOptions & NumericalQuery.LesserEqual) != 0)
                {
                    newOptionItem = this.AddOptionItem("is lesser or equal to", (UInt64)NumericalQuery.LesserEqual);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                }

                // "greater than or equal"
                if ((numericalQueryOptions & NumericalQuery.GreaterEqual) != 0)
                {
                    newOptionItem = this.AddOptionItem("is greater or equal to", (UInt64)NumericalQuery.GreaterEqual);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                }

                // "greater than"
                if ((numericalQueryOptions & NumericalQuery.Greater) != 0)
                {
                    newOptionItem = this.AddOptionItem("is greater than", (UInt64)NumericalQuery.Greater);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                }

                // "between"
                if ((numericalQueryOptions & NumericalQuery.Between) != 0)
                {
                    newOptionItem = this.AddOptionItem("is between", (UInt64)NumericalQuery.Between);
                    m_textInput1 = newOptionItem.AddNumericalSearchTextInput(m_textInput1);
                    newOptionItem.AddLabel("and");
                    m_textInput2 = newOptionItem.AddNumericalSearchTextInput(m_textInput2);
                }
            }
        }
        
        /// <summary>
        /// Gets text in first QueryTextInput's text box</summary>
        public string TextInput1
        {
            get { return m_textInput1.InputText; }
        }

        /// <summary>
        /// Gets text in second QueryTextInput's text box</summary>
        public string TextInput2
        {
            get { return m_textInput2.InputText; }
        }

        /// <summary>
        /// Builds search predicate for the text box entries</summary>
        /// <param name="predicate">Test conditions and value replacement info for query</param>
        public override void BuildPredicate(IQueryPredicate predicate)
        {
            // Override to create some sort of search predicate for the text box entry
        }

        private readonly QueryTextInput m_textInput1;    // all numerical input queries use this
        private readonly QueryTextInput m_textInput2;    // only the 'between' input query uses this
    }
}
