//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace UnitTests.Atf.Wpf.Models
{
    [TestFixture]
    public class TestTreeViewModel
    {
        [Test]
        public void TestNullAdaptee()
        {
            var treeViewModel = new TreeViewModel();
            treeViewModel.Adaptee = null;
            treeViewModel.AutoExpand = AutoExpandMode.Disabled;
            treeViewModel.ShowRoot = true;
            treeViewModel.CollapseAll();
            treeViewModel.ExpandAll();
            treeViewModel.ExpandToFirstLeaf();
            Assert.False(treeViewModel.GetExpandedItems().Any());
            Assert.Null(treeViewModel.Adaptee);
            Assert.AreEqual(treeViewModel.AutoExpand, AutoExpandMode.Disabled);
            Assert.IsTrue(treeViewModel.ShowRoot);
        }

        [Test]
        public void TestAdapteeWithJustRoot()
        {
            var treeView = new MyTreeView(); //has a root MyNode

            var treeViewModel = new TreeViewModel();
            var treeViewWithSelection = new TreeViewWithSelection(treeView);
            treeViewModel.Adaptee = treeViewWithSelection;
            treeViewModel.AutoExpand = AutoExpandMode.Disabled;
            treeViewModel.ShowRoot = true;
            treeViewModel.CollapseAll();
            treeViewModel.ExpandAll();
            var expandedItem = treeViewModel.GetExpandedItems().First();
            Assert.AreEqual(expandedItem, treeView.Root);
            Assert.AreEqual(treeViewWithSelection, treeViewModel.Adaptee);
            Assert.AreEqual(treeViewModel.AutoExpand, AutoExpandMode.Disabled);
            Assert.IsTrue(treeViewModel.ShowRoot);
        }

        [Test]
        public void TestAdaptee()
        {
            // Construct a tree.
            //  root
            //      a
            //          aa
            //          ab
            //      b
            //          ba
            var treeView = new MyItemTreeView();
            var a = new MyNode("a");
            var aa = new MyNode("aa");
            var ab = new MyNode("ab");
            var b = new MyNode("b");
            var ba = new MyNode("ba");
            treeView.Root.Children.Add(a);
            a.Children.Add(aa);
            a.Children.Add(ab);
            treeView.Root.Children.Add(b);
            b.Children.Add(ba);

            var treeViewModel = new TreeViewModel();
            var treeViewWithSelection = new TreeViewWithSelection(treeView);
            treeViewModel.Adaptee = treeViewWithSelection;
            treeViewModel.AutoExpand = AutoExpandMode.Disabled;

            // Test ShowRoot being false.
            treeViewModel.ShowRoot = false;
            treeViewModel.CollapseAll();
            var expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 0);
            var rootModelNode = treeViewModel.Root;
            Assert.IsFalse(rootModelNode.IsVisible);
            Assert.IsFalse(rootModelNode.IsSelected);

            // Expand to first leaf ("aa") with ShowRoot being false.
            Node firstLeaf = treeViewModel.ExpandToFirstLeaf();
            Assert.IsTrue(firstLeaf.Adaptee == aa);
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 3); // the leaf node can be "expanded"
            Assert.IsTrue(expandedList[0] == treeView.Root);
            Assert.IsTrue(expandedList[1] == a);
            Assert.IsTrue(expandedList[2] == aa);

            // Test ShowRoot being true.
            treeViewModel.ShowRoot = true;
            treeViewModel.CollapseAll();
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 0);

            // Expand to first leaf ("aa") with ShowRoot being true.
            firstLeaf = treeViewModel.ExpandToFirstLeaf();
            Assert.IsTrue(firstLeaf.Adaptee == aa);
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 3); // the leaf node can be "expanded"
            Assert.IsTrue(expandedList[0] == treeView.Root);
            Assert.IsTrue(expandedList[1] == a);
            Assert.IsTrue(expandedList[2] == aa);

            // Test ExpandAll
            treeViewModel.ExpandAll();
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 6);

            Assert.AreEqual(treeViewWithSelection, treeViewModel.Adaptee);
            Assert.AreEqual(treeViewModel.AutoExpand, AutoExpandMode.Disabled);
            Assert.IsTrue(treeViewModel.ShowRoot);
        }

        [Test]
        public void TestGetInfoCalls()
        {
            // Construct a tree.
            //  root
            //      a
            //          aa
            //          ab
            //      b
            //          ba
            var treeView = new MyItemTreeView();
            var a = new MyNode("a");
            var aa = new MyNode("aa");
            var ab = new MyNode("ab");
            var b = new MyNode("b");
            var ba = new MyNode("ba");
            treeView.Root.Children.Add(a);
            a.Children.Add(aa);
            a.Children.Add(ab);
            treeView.Root.Children.Add(b);
            b.Children.Add(ba);

            var treeViewModel = new TreeViewModel(treeView);

            // Test ShowRoot being false.
            treeViewModel.ShowRoot = false;
            treeViewModel.CollapseAll();
            treeView.NodesQueried.Clear();
            treeViewModel.Refresh(treeView.Root); // to call IItemView.GetInfo()
            var expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 0);
            var rootModelNode = treeViewModel.Root;
            Assert.IsFalse(rootModelNode.IsVisible);
            Assert.IsFalse(rootModelNode.IsSelected);
            //IItemView.GetInfo() should not be called on collapsed nodes.
            Assert.IsFalse(treeView.NodesQueried.Contains(aa));

            // Expand to first leaf ("aa") with ShowRoot being false.
            Node firstLeaf = treeViewModel.ExpandToFirstLeaf();
            Assert.IsTrue(firstLeaf.Adaptee == aa);
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 3); // the leaf node can be "expanded"
            Assert.IsTrue(expandedList[0] == treeView.Root);
            Assert.IsTrue(expandedList[1] == a);
            Assert.IsTrue(expandedList[2] == aa);
            treeView.NodesQueried.Clear();
            treeViewModel.Refresh(treeView.Root); // to call IItemView.GetInfo()
            Assert.IsTrue(treeView.NodesQueried.Contains(aa));
            Assert.IsTrue(treeView.NodesQueried.Contains(a));
            Assert.IsFalse(treeView.NodesQueried.Contains(ba));

            // Test ShowRoot being true.
            treeViewModel.ShowRoot = true;
            treeViewModel.CollapseAll();
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 0);

            // Expand to first leaf ("aa") with ShowRoot being true.
            treeView.NodesQueried.Clear();
            firstLeaf = treeViewModel.ExpandToFirstLeaf();
            Assert.IsTrue(firstLeaf.Adaptee == aa);
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 3); // the leaf node can be "expanded"
            Assert.IsTrue(expandedList[0] == treeView.Root);
            Assert.IsTrue(expandedList[1] == a);
            Assert.IsTrue(expandedList[2] == aa);

            // Test ExpandAll
            treeView.NodesQueried.Clear();
            treeViewModel.ExpandAll();
            treeViewModel.Refresh(treeView.Root); // to call IItemView.GetInfo()
            expandedList = treeViewModel.GetExpandedItems().ToList();
            Assert.IsTrue(expandedList.Count == 6);
            Assert.IsTrue(treeView.NodesQueried.Count == 6);

            Assert.AreEqual(treeView, treeViewModel.Adaptee);
            Assert.IsTrue(treeViewModel.ShowRoot);
        }

        // A basic ITreeView that always has a root MyNode.
        private class MyTreeView : ITreeView
        {
            public MyNode Root = new MyNode("root");

            #region ITreeView members
            object ITreeView.Root
            {
                get { return Root; }
            }

            IEnumerable<object> ITreeView.GetChildren(object parent)
            {
                var parentNode = (MyNode) parent;
                if (parentNode != null)
                    return parentNode.Children;

                return null;
            }
            #endregion
        }

        // A basic ITreeView that also implements IItemView
        private class MyItemTreeView : MyTreeView, IItemView
        {
            public HashSet<MyNode> NodesQueried = new HashSet<MyNode>();
            public void GetInfo(object item, ItemInfo info)
            {
                var node = (MyNode) item;
                info.Description = node.Name;
                NodesQueried.Add(node);
            }
        }

        private class MyNode
        {
            public MyNode(string name)
            {
                Name = name;
            }
            public List<MyNode> Children = new List<MyNode>();
            public string Name;
        }
    }
}
