//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestTree
    {
        [Test]
        public void TestEverything()
        {
            Tree<string> test = new Tree<string>("a");
            Assert.True(test.IsLeaf);
            Assert.True(test.Parent == null);
            Assert.True(test.DescendantCount == 1);
            Assert.True(test.Level == 0);
            Assert.True(test.IsDescendantOf(test));

            Tree<string> b = new Tree<string>("b");
            test.Children.Add(b);
            Assert.True(b.Parent == test);

            Tree<string> c = new Tree<string>("c");
            test.Children.Add(c);
            Assert.True(test.ToString() == "(a(b,c))");

            test.Children.Insert(1, new Tree<string>("x"));

            Assert.True(test.ToString() == "(a(b,x,c))");

            test.Children.RemoveAt(1);

            Assert.True(test.ToString() == "(a(b,c))");

            Tree<string> subtree = new Tree<string>("d");
            subtree.Children.Add(new Tree<string>("e"));
            subtree.Children.Add(new Tree<string>("f"));

            test.Children.Insert(1, subtree);

            Assert.True(test.ToString() == "(a(b,d(e,f),c))");
            Assert.True(test.DescendantCount == 6);
            Assert.True(subtree.IsDescendantOf(test));

            string s;

            s = "";
            foreach (Tree<string> t in test.Children)
                s += t.Value.ToString();
            Assert.True(s == "bdc");

            s = "";
            foreach (Tree<string> t in test.PreOrder)
                s += t.Value.ToString();
            Assert.True(s == "abdefc");

            s = "";
            foreach (Tree<string> t in test.PostOrder)
                s += t.Value.ToString();
            Assert.True(s == "befdca");

            s = "";
            foreach (Tree<string> t in test.LevelOrder)
                s += t.Value.ToString();
            Assert.True(s == "abdcef");
        }
    }
}
