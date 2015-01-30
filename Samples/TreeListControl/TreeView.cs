//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.DataEditing;

namespace TreeListControlDemo
{
    /// <summary>
    /// Provides a tree view of our XML document</summary>
    public class TreeView:  ITreeView, IItemView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeView"/> class</summary>
        /// <param name="xmlFilePath">The XML file path of the data document to load</param>
        /// <param name="theme">The visual theme</param>
        public TreeView(string xmlFilePath, DataEditorTheme theme)
        {
            m_xmlDoc = XDocument.Load(xmlFilePath);// loads the hierarchical data using Linq to XML API
            m_dataEditorTheme = theme;
        }

        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree view</summary>
        public object Root
        {
            get { return m_xmlDoc.Root; }
        }

        /// <summary>
        /// Obtains enumeration of the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            var node = parent as XElement;
            if (node == null)
                return Enumerable.Empty<object>();
            return node.Elements();
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            var node = item as XElement;
            if (node == null)
                return;
            info.IsLeaf = !node.HasElements;
            if (node.Name.LocalName == "suv")
            {
                var attibute = node.Attribute("name");
                if (attibute != null)
                {
                    info.Label = (string) attibute;

                    var mpg = new StringDataEditor(m_dataEditorTheme)
                    {
                        Owner = item,
                        Value = (string)node.Attribute("mpg"),
                        ReadOnly =  true,
                        Name = "MPG", // (column)name of the data value, should be unique among columns,
                    };

                    var weight = new StringDataEditor(m_dataEditorTheme)
                    {
                        Owner = item,
                        Value = (string)node.Attribute("weight"),
                        Name = "Weight", 
                    };

                    var color = new ColorDataEditor(m_dataEditorTheme)
                    {
                        Owner = item,
                        Name = "Color",
                    };
                    color.Parse((string) node.Attribute("color"));

                    var msrp = new FloatDataEditor(m_dataEditorTheme)
                    {
                        Owner =  item,
                        Value = (float) node.Attribute("msrp"),
                        Min =   (float) node.Attribute("min"),
                        Max =   (float) node.Attribute("max"),
                        Name = "MSRP", 
                        ShowSlider = true
                    };
                    info.Properties = new object[] { mpg, weight, color, msrp };
                }
            }
            else
                info.Label = node.Name.LocalName;
        }

        #endregion

        private readonly XDocument m_xmlDoc;
        private readonly DataEditorTheme m_dataEditorTheme;
    }
}
