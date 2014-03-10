//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Extension methods for QueryNode and core inheriting classes</summary>
    public static class QueryTree
    {
#pragma warning disable 1587 // XML comment is not placed on a valid language element.

        /// <summary>
        /// Adds a child QueryNode</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="childNode">Node to be parented</param>
        /// <returns>The passed-in child node</returns>
        public static QueryNode Add(this QueryNode parentNode, QueryNode childNode)
        {
            parentNode.Children.Add(childNode);
            return childNode;
        }


        /// <summary>
        /// Adds a child QueryLabel node</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="text">Label text</param>
        /// <returns>Instance of the new QueryLabel child node</returns>

        public static QueryLabel AddLabel(this QueryNode parentNode, string text)
        {
            return parentNode.Add(new QueryLabel(text)) as QueryLabel;
        }

        /// <summary>
        /// Adds a child QuerySeparator node</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <returns>Instance of the new QuerySeparator child node</returns>
        public static QuerySeparator AddSeparator(this QueryNode parentNode)
        {
            return parentNode.Add(new QuerySeparator()) as QuerySeparator;
        }

        /// <summary>
        /// Adds a child QueryButton node</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <param name="text">Button text</param>
        /// <returns>Instance of the new QueryButton child node</returns>
        public static QueryButton AddButton(this QueryNode parentNode, string text)
        {
            return parentNode.Add(new QueryButton(text)) as QueryButton;
        }

        /// <summary>
        /// Adds a child QueryOption node, registering 'option changed' event to root node</summary>
        /// <param name="parentNode">Node to receive child</param>
        /// <returns>Instance of the new QueryOption child node</returns>
        public static QueryOption AddOption(this QueryNode parentNode)
        {
            QueryOption newQueryOption = parentNode.Add(new QueryOption()) as QueryOption;

            // register 'option changed' event to root node
            QueryRoot rootNode = newQueryOption.Root as QueryRoot;
            if (rootNode != null)
                rootNode.RegisterQueryOption(newQueryOption);

            return newQueryOption;
        }

        /// <summary>
        /// Adds a child QueryOptionItem node to a QueryOption node</summary>
        /// <param name="parentNode">QueryOption node to receive child</param>
        /// <param name="optionItemText">Text to be displayed on option item</param>
        /// <param name="tag">ID tag for option item</param>
        /// <returns>Instance of the new QueryOptionItem child node</returns>
        public static QueryOptionItem AddOptionItem(this QueryOption parentNode, string optionItemText, UInt64 tag)
        {
            QueryOptionItem newOptionItem = null;

            if (parentNode != null)
            {
                newOptionItem = parentNode.Add(new QueryOptionItem(optionItemText, tag)) as QueryOptionItem;
                parentNode.RegisterOptionItem(newOptionItem);
            }

            return newOptionItem;
        }

        /// <summary>
        /// Adds a child QueryTextInput node</summary>
        /// <param name="parentNode">QueryNode to receive child</param>
        /// <param name="textInput">Null, or instance of QueryTextInput to use as child (for sharing same text input instance between nodes)</param>
        /// <param name="isNumericalText">Whether text input to text box is numeric or not</param>
        /// <returns>Instance of the new (or passed in) QueryTextInput child node</returns>
        public static QueryTextInput AddTextInput(this QueryNode parentNode, QueryTextInput textInput, bool isNumericalText)
        {
            bool useExistingTextInput = (textInput != null);
            QueryTextInput newTextInput = parentNode.Add((useExistingTextInput) ? textInput : new QueryTextInput(isNumericalText))
                as QueryTextInput;

            return newTextInput;
        }

        /// <summary>
        /// Adds a child QueryTextInput node for string text input, registering 'search text changed' event to root node</summary>
        /// <param name="parentNode">QueryNode to receive child</param>
        /// <param name="textInput">Null, or instance of QueryTextInput to use as child (for sharing same text input instance between nodes)</param>
        /// <returns>Instance of the new (or passed in) QueryTextInput child node</returns>
        public static QueryTextInput AddStringSearchTextInput(this QueryNode parentNode, QueryTextInput textInput)
        {
            QueryTextInput newTextInput = AddTextInput(parentNode, textInput, false);
            if (newTextInput != null && newTextInput != textInput)
            {
                // register 'search text changed' event to root node
                QueryRoot rootNode = newTextInput.Root as QueryRoot;
                if (rootNode != null)
                    rootNode.RegisterSearchQueryTextInput(newTextInput);
            }

            return newTextInput;
        }

        /// <summary>
        /// Adds a child QueryTextInput node for numerical text input, registering 'search text changed' event to root node</summary>
        /// <param name="parentNode">QueryNode to receive child</param>
        /// <param name="textInput">Null, or instance of QueryTextInput to use as child (for sharing same text input instance between nodes)</param>
        /// <returns>Instance of the new (or passed in) QueryTextInput child node</returns>
        public static QueryTextInput AddNumericalSearchTextInput(this QueryNode parentNode, QueryTextInput textInput)
        {
            QueryTextInput newTextInput = AddTextInput(parentNode, textInput, true);
            if (newTextInput != null && newTextInput != textInput)
            {
                // register 'search text changed' event to root node
                QueryRoot rootNode = newTextInput.Root as QueryRoot;
                if (rootNode != null)
                    rootNode.RegisterSearchQueryTextInput(newTextInput);
            }

            return newTextInput;
        }

        /// <summary>
        /// Adds a child QueryTextInput node, registering 'replace text changed' event to root node</summary>
        /// <param name="parentNode">QueryNode to receive child</param>
        /// <param name="textInput">Null, or instance of QueryTextInput to use as child (for sharing same text input instance between nodes)</param>
        /// <param name="isNumericalText">Whether text input to text box is numeric or not</param>
        /// <returns>Instance of the new (or passed in) QueryTextInput child node</returns>
        public static QueryTextInput AddReplaceTextInput(this QueryNode parentNode, QueryTextInput textInput, bool isNumericalText)
        {
            QueryTextInput newTextInput = AddTextInput(parentNode, textInput, isNumericalText);
            if (newTextInput != null && newTextInput != textInput)
            {
                // register 'replace text changed' event to root node
                QueryRoot rootNode = newTextInput.Root as QueryRoot;
                if (rootNode != null)
                    rootNode.RegisterReplaceQueryTextInput(newTextInput);
            }

            return newTextInput;
        }

        /// <summary>
        /// Adds a QueryOption that is the root of a QueryTree that provides text input for numerical queries</summary>
        /// <param name="parentNode">QueryNode to receive child</param>
        /// <param name="numericalQueryOptions">Bitfield defining which numerical search types can be selected</param>
        /// <returns>Instance of the new QueryNumericalInput child node</returns>
        public static QueryOption AddNumericalQuery(this QueryNode parentNode, NumericalQuery numericalQueryOptions)
        {
            return new QueryNumericalInput(parentNode, numericalQueryOptions);
        }
#pragma warning restore 1587 // XML comment is not placed on a valid language element.
    }
}
