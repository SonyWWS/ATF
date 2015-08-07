//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace TreeListControlDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage));
            LoadHierarchicalXmlData();
            Width = 680;
            Height = 300;
        }

        // Load hierarchical data from XML and display it in a TreeListControl
        void LoadHierarchicalXmlData()
        {
 
            var treeListControl = new TreeListControl();
      
            treeListControl.Dock = DockStyle.Fill;
            treeListControl.ShowRoot = false;
            treeListControl.LabelEditMode = TreeControl.LabelEditModes.EditOnF2 | TreeControl.LabelEditModes.EditOnClick;
            treeListControl.NodeDataEdited += treeListControl_NodeDataEdited;
            Controls.Add(treeListControl);

            Assembly assembly = Assembly.GetExecutingAssembly();
            string startupPath = Path.GetDirectoryName(new Uri(assembly.GetName().CodeBase).LocalPath);
            var xmlPath = Path.Combine(startupPath, "CoolSUVs.xml");

            var treeView = new TreeView(xmlPath, new DataEditorTheme(treeListControl.Font));
            var treeControlAdapter = new TreeControlAdapter(treeListControl);
            treeControlAdapter.TreeView = treeView;

            treeListControl.ItemRenderer = new TreeListItemRenderer(treeView);

            treeListControl.Columns.Add(new TreeListView.Column("MPG",  80));
            treeListControl.Columns.Add(new TreeListView.Column("Weight", 80));
            treeListControl.Columns.Add(new TreeListView.Column("AWD", 50));
            treeListControl.Columns.Add(new TreeListView.Column("Color", 80));
            treeListControl.Columns.Add(new TreeListView.Column("MSRP", 80));
            treeListControl.ExpandAll();
        }

        void treeListControl_NodeDataEdited(object sender, TreeListControl.NodeEditEventArgs e)
        {
            PrintData(e);
            var editedElement = e.Node.Tag as XElement;
            if (e.EditedData.Name == "MSRP")
            {
                editedElement.Attribute("msrp").Value = e.EditedData.ToString();
            }
            else if (e.EditedData.Name == "Weight")
            {
                editedElement.Attribute("weight").Value = e.EditedData.ToString();
            }
            else if (e.EditedData.Name == "Color")
            {
                editedElement.Attribute("color").Value = e.EditedData.ToString();
            }
            else if (e.EditedData.Name == "AWD")
            {
                editedElement.Attribute("awd").Value = e.EditedData.ToString();
            }
            e.Node.TreeControl.Invalidate();         
        }

        void PrintData(TreeListControl.NodeEditEventArgs e)
        {
            //var editedXMLNode = e.Node.Tag as XElement;
            //System.Diagnostics.Trace.TraceInformation(" {0}: {1} changed {2}", editedXMLNode.Attribute("name"), 
            //    e.EditedData.Name,e.EditedData.ToString());

        }
    }
}
