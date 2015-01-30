This sample application demonstrates the usage of
TreeListControl/TreeListItemRenderer that can display and edit
hierarchical data in a tree view with details in columns.

It loads the hierarchical data from CoolSUVs.xml using Linq to XML API:

    var xmlPath = Path.Combine(startupPath, "CoolSUVs.xml");
    m_xmlDoc = XDocument.Load(xmlPath); 

It uses TreeControlAdapter to populate the hierarchical data to the
TreeListControl, and displays the MSRP for a SUV on the right side of
the tree control. Both slider and textbox are used to display and edit
the MSRP.

The hierarchical data resides in m_xmlDoc, TreeView is the data
adapter that maps the XML node tree to the tree control. Note
TreeControlAdapter uses TreeView to adapt the data view.

In an ATF-based application the node data is normally obtained by the
call IItemView.GetInfo(). To display node details in columns, fill the
info.Properties from your actual node data in IItemView.GetInfo(). For
each node detail(column), you need to construct a DataEditor
derived data editor to display/edit the node data. For MSRP:

        public void GetInfo(object item, ItemInfo info)
        {
            ...
            var msrp = new FloatDataEditor
            {
                Owner =  item,
                Value = (float) node.Attribute("msrp"),
                Min =   (float) node.Attribute("min"),
                Max =   (float) node.Attribute("max"),
                Name = "MSRP",
                ShowSlider = true
            };
            info.Properties = new object[] { msrp };
            ...
        
        }
        
TreeListItemRenderer.DrawData() method extracts the node details from
info.Properties, then display the details in the right side of the
tree control.

When you change the node data via a textbox or slider, you need to
handle the data edited event to propagate the change back to your
application:

        treeListControl.NodeDataEdited += treeListControl_NodeDataEdited;
        
        void treeListControl_NodeDataEdited(object sender, 
                TreeListControl.NodeEditEventArgs e)
        {
            var editedElement = e.Node.Tag as XElement;
            if (e.EditedData.Name == "MSRP")
            {
                editedElement.Attribute("msrp").Value =
                    e.EditedData.Convert(
                    ((DataRecords.FLoatDataRecord) e.EditedData).Value);
            }
            e.Node.TreeControl.Invalidate();
            
        } 
        
e.EditedData contains the current value of the node detail from UI
editing. In the sample, we update the MSRP value matched by column
name, and the XML node's msrp attribute is overwritten with the
current value.

Note:

This sample app does not use MEF, DOM and other popular ATF
technologies. It is just a simple Windows Form based app to showcase
the TreeListControl.

To Review:
reuse nested class ColumnCollection & Column in TreeListView, should we pull out these 2 classes?



TODO:

• Make the column resizing controls always resize the column on the left,
leaving all other column widths unchanged. This is how MS Excel works, for example.
Currently, only the first (left-most) resizing control behaves this way.

• Add Bool, Enum and Color data editors( configurations of a car can be represented as enum)

• If users specify column names, but don't fill info.Properties, 
  try to use column name as a public property name to auto-fill column details 

• Theme support
