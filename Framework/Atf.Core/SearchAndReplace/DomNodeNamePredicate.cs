//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// A simple class for producing search matches on DomNode names, and for applying a string replace on those search results</summary>
    public class DomNodeNamePredicate : IQueryPredicate
    {
        /// <summary>
        /// Constructor</summary>
        public DomNodeNamePredicate()
        {
            StringToMatch = "";
        }

        /// <summary>
        /// Gets an enumeration of properties for a specific DomNode</summary>
        /// <param name="node">DomNode on which to obtain properties</param>
        /// <returns>Enumeration of properties</returns>
        static internal PropertyDescriptorCollection GetDomNodeProperties(DomNode node)
        {
            if (node == null)
                return null;

            ICustomTypeDescriptor iCustomTypeDescriptor = node.GetAdapter(typeof(CustomTypeDescriptorNodeAdapter)) as ICustomTypeDescriptor;
            if (iCustomTypeDescriptor == null)
                return null;

            PropertyDescriptorCollection properties = iCustomTypeDescriptor.GetProperties();
            return properties;
        }

        /// <summary>
        /// Gets the value of the "Name" property for a specific DomNode</summary>
        /// <param name="domNode">DomNode on which to obtain Name property</param>
        /// <param name="nameProperty">Name property descriptor</param>
        /// <returns>Value of the Name property as string</returns>
        static internal string GetDomNodeName(DomNode domNode, out PropertyDescriptor nameProperty)
        {
            string name = "<UNKNOWN>";
            nameProperty = null;
            PropertyDescriptorCollection properties = GetDomNodeProperties(domNode);
            if (properties != null)
            {
                foreach (PropertyDescriptor property in properties)
                {
                    if (property.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        nameProperty = property;
                        name = property.GetValue(domNode) as string;
                        break;
                    }
                }
            }
            return name;
        }

        #region IQueryPredicate members
        
        /// <summary>
        /// Tries to match string in DomNode name</summary>
        /// <param name="item">DomNode object</param>
        /// <param name="matchList">DomNodePropertyMatch list</param>
        /// <returns>True iff string match</returns>
        public bool Test(object item, out IList<IQueryMatch> matchList)
        {
            DomNode domNode = item as DomNode;
            if (domNode == null)
                throw new InvalidOperationException("DomNodeSearchTextBox passed a test item that was not a DomNode");

            matchList = new List<IQueryMatch>();
            PropertyDescriptor namePd;
            string nodeName = GetDomNodeName(domNode, out namePd);
            if (StringToMatch != null && 
                StringToMatch.Length > 0 && 
                nodeName.IndexOf(StringToMatch, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                matchList.Add(new DomNodePropertyMatch(namePd, domNode));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies a replacement to a match list</summary>
        /// <remarks>Replaces strings matching regular expression, ignoring case</remarks>
        /// <param name="matchList">Match list</param>
        /// <param name="replaceValue">Replacement value</param>
        public void Replace(IList<IQueryMatch> matchList, object replaceValue)
        {
            foreach (IQueryMatch queryMatch in matchList)
            {
                string value = queryMatch.GetValue().ToString();
                queryMatch.SetValue(Regex.Replace(value, StringToMatch, replaceValue.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
            }
        }
        #endregion

        private String m_stringToMatch;

        /// <summary>
        /// Gets or sets matching string</summary>
        public String StringToMatch { get { return m_stringToMatch; } set { m_stringToMatch = value; } }
    }
}

