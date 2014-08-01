//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Sce.Atf;
using Sce.Atf.Collections;

namespace Sce.Atf.Wpf.Collections
{
    internal static class RectExtensions
    {
        public static Point GetCenter(this Rect rect)
        {
            return new Point(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0);
        }

        public static bool IsDefined(this Rect rect)
        {
            return rect.Width >= 0.0 && rect.Height >= 0.0 && rect.Top < double.PositiveInfinity && rect.Left < double.PositiveInfinity &&
                   (rect.Top > double.NegativeInfinity || rect.Height == double.PositiveInfinity) &&
                   (rect.Left > double.NegativeInfinity || rect.Width == double.PositiveInfinity);
        }

        public static bool Intersects(this Rect self, Rect rect)
        {
            return self.IsEmpty || rect.IsEmpty ||
                   ((self.Width == double.PositiveInfinity || self.Right >= rect.Left) &&
                    (rect.Width == double.PositiveInfinity || rect.Right >= self.Left) &&
                    (self.Height == double.PositiveInfinity || self.Bottom >= rect.Top) &&
                    (rect.Height == double.PositiveInfinity || rect.Bottom >= self.Top));
        }
    }

    public class PriorityQuadTree<T> : IEnumerable<T>, IEnumerable
    {
        private class QuadNode
        {
            private readonly Rect _bounds;
            private readonly T _node;
            private readonly double _priority;

            public T Node
            {
                get { return _node; }
            }

            public Rect Bounds
            {
                get { return _bounds; }
            }

            public double Priority
            {
                get { return _priority; }
            }

            public QuadNode Next { get; set; }

            public QuadNode(T node, Rect bounds, double priority)
            {
                _node = node;
                _bounds = bounds;
                _priority = priority;
            }

            public QuadNode InsertInto(QuadNode tail)
            {
                if (tail == null)
                {
                    Next = this;
                    tail = this;
                }
                else
                {
                    if (Priority < tail.Priority)
                    {
                        Next = tail.Next;
                        tail.Next = this;
                        tail = this;
                    }
                    else
                    {
                        QuadNode quadNode = tail;
                        while (quadNode.Next != tail && Priority < quadNode.Next.Priority)
                        {
                            quadNode = quadNode.Next;
                        }
                        Next = quadNode.Next;
                        quadNode.Next = this;
                    }
                }
                return tail;
            }

            public IEnumerable<Tuple<QuadNode, double>> GetNodesIntersecting(Rect bounds)
            {
                QuadNode quadNode = this;
                do
                {
                    quadNode = quadNode.Next;
                    if (bounds.Intersects(quadNode.Bounds))
                    {
                        yield return Tuple.Create(quadNode, (quadNode != this) ? quadNode.Next.Priority : double.NaN);
                    }
                } while (quadNode != this);
                yield break;
            }

            public IEnumerable<Tuple<QuadNode, double>> GetNodesInside(Rect bounds)
            {
                QuadNode quadNode = this;
                do
                {
                    quadNode = quadNode.Next;
                    if (bounds.Contains(quadNode.Bounds))
                    {
                        yield return Tuple.Create(quadNode, (quadNode != this) ? quadNode.Next.Priority : double.NaN);
                    }
                } while (quadNode != this);
                yield break;
            }

            public bool HasNodesIntersecting(Rect bounds)
            {
                QuadNode quadNode = this;
                while (true)
                {
                    quadNode = quadNode.Next;
                    if (bounds.Intersects(quadNode.Bounds))
                    {
                        break;
                    }
                    if (quadNode == this)
                    {
                        return false;
                    }
                }
                return true;
            }

            public bool HasNodesInside(Rect bounds)
            {
                QuadNode quadNode = this;
                while (true)
                {
                    quadNode = quadNode.Next;
                    if (bounds.Contains(quadNode.Bounds))
                    {
                        break;
                    }
                    if (quadNode == this)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private class Quadrant : IEnumerable<QuadNode>, IEnumerable
        {
            private readonly Rect _bounds;
            private double _potential = double.NegativeInfinity;
            private int _count;
            private QuadNode _nodes;
            private Quadrant _topLeft;
            private Quadrant _topRight;
            private Quadrant _bottomLeft;
            private Quadrant _bottomRight;

            public Quadrant(Rect bounds)
            {
                _bounds = bounds;
            }

            internal Quadrant Insert(T node, Rect bounds, double priority, int depth)
            {
                _potential = Math.Max(_potential, priority);
                _count++;
                Quadrant quadrant = null;
                if (depth <= 50 && (bounds.Width > 0.0 || bounds.Height > 0.0))
                {
                    double num = _bounds.Width / 2.0;
                    double num2 = _bounds.Height / 2.0;
                    Rect bounds2 = new Rect(_bounds.Left, _bounds.Top, num, num2);
                    Rect bounds3 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                    Rect bounds4 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                    Rect bounds5 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                    if (bounds2.Contains(bounds))
                    {
                        if (_topLeft == null)
                        {
                            _topLeft = new Quadrant(bounds2);
                        }
                        quadrant = _topLeft;
                    }
                    else
                    {
                        if (bounds3.Contains(bounds))
                        {
                            if (_topRight == null)
                            {
                                _topRight = new Quadrant(bounds3);
                            }
                            quadrant = _topRight;
                        }
                        else
                        {
                            if (bounds4.Contains(bounds))
                            {
                                if (_bottomLeft == null)
                                {
                                    _bottomLeft = new Quadrant(bounds4);
                                }
                                quadrant = _bottomLeft;
                            }
                            else
                            {
                                if (bounds5.Contains(bounds))
                                {
                                    if (_bottomRight == null)
                                    {
                                        _bottomRight = new Quadrant(bounds5);
                                    }
                                    quadrant = _bottomRight;
                                }
                            }
                        }
                    }
                }
                if (quadrant != null)
                {
                    return quadrant.Insert(node, bounds, priority, depth + 1);
                }
                QuadNode quadNode = new QuadNode(node, bounds, priority);
                _nodes = quadNode.InsertInto(_nodes);
                return this;
            }

            internal bool Remove(T node, Rect bounds)
            {
                bool flag = false;
                if (RemoveNode(node))
                {
                    flag = true;
                }
                else
                {
                    double num = _bounds.Width / 2.0;
                    double num2 = _bounds.Height / 2.0;
                    Rect self = new Rect(_bounds.Left, _bounds.Top, num, num2);
                    Rect self2 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                    Rect self3 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                    Rect self4 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                    if (_topLeft != null && self.Intersects(bounds) && _topLeft.Remove(node, bounds))
                    {
                        if (_topLeft._count == 0)
                        {
                            _topLeft = null;
                        }
                        flag = true;
                    }
                    else
                    {
                        if (_topRight != null && self2.Intersects(bounds) && _topRight.Remove(node, bounds))
                        {
                            if (_topRight._count == 0)
                            {
                                _topRight = null;
                            }
                            flag = true;
                        }
                        else
                        {
                            if (_bottomLeft != null && self3.Intersects(bounds) && _bottomLeft.Remove(node, bounds))
                            {
                                if (_bottomLeft._count == 0)
                                {
                                    _bottomLeft = null;
                                }
                                flag = true;
                            }
                            else
                            {
                                if (_bottomRight != null && self4.Intersects(bounds) && _bottomRight.Remove(node, bounds))
                                {
                                    if (_bottomRight._count == 0)
                                    {
                                        _bottomRight = null;
                                    }
                                    flag = true;
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    _count--;
                    _potential = CalculatePotential();
                    return true;
                }
                return false;
            }

            internal IEnumerable<Tuple<QuadNode, double>> GetNodesIntersecting(Rect bounds)
            {
                double num = _bounds.Width / 2.0;
                double num2 = _bounds.Height / 2.0;
                Rect self = new Rect(_bounds.Left, _bounds.Top, num, num2);
                Rect self2 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                Rect self3 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                Rect self4 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                var priorityQueue = new PriorityQueue<IEnumerator<Tuple<QuadNode, double>>, double>(true);
                if (_nodes != null)
                {
                    priorityQueue.Enqueue(_nodes.GetNodesIntersecting(bounds).GetEnumerator(), _nodes.Next.Priority);
                }
                if (_topLeft != null && self.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_topLeft.GetNodesIntersecting(bounds).GetEnumerator(), _topLeft._potential);
                }
                if (_topRight != null && self2.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_topRight.GetNodesIntersecting(bounds).GetEnumerator(), _topRight._potential);
                }
                if (_bottomLeft != null && self3.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_bottomLeft.GetNodesIntersecting(bounds).GetEnumerator(), _bottomLeft._potential);
                }
                if (_bottomRight != null && self4.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_bottomRight.GetNodesIntersecting(bounds).GetEnumerator(), _bottomRight._potential);
                }
                while (priorityQueue.Count > 0)
                {
                    var key = priorityQueue.Dequeue().Key;
                    if (key.MoveNext())
                    {
                        Tuple<QuadNode, double> current = key.Current;
                        QuadNode item = current.Item1;
                        double item2 = current.Item2;
                        double num3 = (priorityQueue.Count > 0)
                                          ? ((!item2.IsNaN()) ? Math.Max(item2, priorityQueue.Peek().Value) : priorityQueue.Peek().Value)
                                          : item2;
                        if (num3 > item.Priority)
                        {
                            IEnumerator<Tuple<QuadNode, double>> enumerator =
                                Enumerable.Repeat(Tuple.Create(item, double.NaN), 1)
                                          .GetEnumerator();
                            priorityQueue.Enqueue(enumerator, item.Priority);
                        }
                        else
                        {
                            yield return Tuple.Create(item, num3);
                        }
                        if (!item2.IsNaN())
                        {
                            priorityQueue.Enqueue(key, item2);
                        }
                    }
                }
                yield break;
            }

            internal IEnumerable<Tuple<QuadNode, double>> GetNodesInside(Rect bounds)
            {
                double num = _bounds.Width / 2.0;
                double num2 = _bounds.Height / 2.0;
                Rect self = new Rect(_bounds.Left, _bounds.Top, num, num2);
                Rect self2 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                Rect self3 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                Rect self4 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                var priorityQueue = new PriorityQueue<IEnumerator<Tuple<QuadNode, double>>, double>(true);
                if (_nodes != null)
                {
                    priorityQueue.Enqueue(_nodes.GetNodesInside(bounds).GetEnumerator(), _nodes.Next.Priority);
                }
                if (_topLeft != null && self.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_topLeft.GetNodesInside(bounds).GetEnumerator(), _topLeft._potential);
                }
                if (_topRight != null && self2.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_topRight.GetNodesInside(bounds).GetEnumerator(), _topRight._potential);
                }
                if (_bottomLeft != null && self3.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_bottomLeft.GetNodesInside(bounds).GetEnumerator(), _bottomLeft._potential);
                }
                if (_bottomRight != null && self4.Intersects(bounds))
                {
                    priorityQueue.Enqueue(_bottomRight.GetNodesInside(bounds).GetEnumerator(), _bottomRight._potential);
                }
                while (priorityQueue.Count > 0)
                {
                    IEnumerator<Tuple<QuadNode, double>> key = priorityQueue.Dequeue().Key;
                    if (key.MoveNext())
                    {
                        Tuple<QuadNode, double> current = key.Current;
                        QuadNode item = current.Item1;
                        double item2 = current.Item2;
                        double num3 = (priorityQueue.Count > 0)
                                          ? ((!item2.IsNaN()) ? Math.Max(item2, priorityQueue.Peek().Value) : priorityQueue.Peek().Value)
                                          : item2;
                        if (num3 > item.Priority)
                        {
                            var enumerator =
                                Enumerable.Repeat(Tuple.Create(item, double.NaN), 1)
                                          .GetEnumerator();
                            priorityQueue.Enqueue(enumerator, item.Priority);
                        }
                        else
                        {
                            yield return Tuple.Create(item, num3);
                        }
                        if (!item2.IsNaN())
                        {
                            priorityQueue.Enqueue(key, item2);
                        }
                    }
                }
                yield break;
            }

            internal bool HasNodesInside(Rect bounds)
            {
                double num = _bounds.Width / 2.0;
                double num2 = _bounds.Height / 2.0;
                Rect rect = new Rect(_bounds.Left, _bounds.Top, num, num2);
                Rect rect2 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                Rect rect3 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                Rect rect4 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                return (_nodes != null && _nodes.HasNodesInside(bounds)) ||
                       (_topLeft != null && rect.Contains(bounds) && _topLeft.HasNodesInside(bounds)) ||
                       (_topRight != null && rect2.Contains(bounds) && _topRight.HasNodesInside(bounds)) ||
                       (_bottomLeft != null && rect3.Contains(bounds) && _bottomLeft.HasNodesInside(bounds)) ||
                       (_bottomRight != null && rect4.Contains(bounds) && _bottomRight.HasNodesInside(bounds));
            }

            internal bool HasNodesIntersecting(Rect bounds)
            {
                double num = _bounds.Width / 2.0;
                double num2 = _bounds.Height / 2.0;
                Rect self = new Rect(_bounds.Left, _bounds.Top, num, num2);
                Rect self2 = new Rect(_bounds.Left + num, _bounds.Top, num, num2);
                Rect self3 = new Rect(_bounds.Left, _bounds.Top + num2, num, num2);
                Rect self4 = new Rect(_bounds.Left + num, _bounds.Top + num2, num, num2);
                return (_nodes != null && _nodes.HasNodesIntersecting(bounds)) ||
                       (_topLeft != null && self.Intersects(bounds) && _topLeft.HasNodesIntersecting(bounds)) ||
                       (_topRight != null && self2.Intersects(bounds) && _topRight.HasNodesIntersecting(bounds)) ||
                       (_bottomLeft != null && self3.Intersects(bounds) && _bottomLeft.HasNodesIntersecting(bounds)) ||
                       (_bottomRight != null && self4.Intersects(bounds) && _bottomRight.HasNodesIntersecting(bounds));
            }

            private bool RemoveNode(T node)
            {
                bool result = false;
                if (_nodes != null)
                {
                    QuadNode quadNode = _nodes;
                    while (!object.Equals(quadNode.Next.Node, node) && quadNode.Next != _nodes)
                    {
                        quadNode = quadNode.Next;
                    }
                    if (object.Equals(quadNode.Next.Node, node))
                    {
                        result = true;
                        QuadNode next = quadNode.Next;
                        if (quadNode == next)
                        {
                            _nodes = null;
                        }
                        else
                        {
                            if (_nodes == next)
                            {
                                _nodes = quadNode;
                            }
                            quadNode.Next = next.Next;
                        }
                    }
                }
                return result;
            }

            private double CalculatePotential()
            {
                double num = double.NegativeInfinity;
                if (_nodes != null)
                {
                    num = _nodes.Next.Priority;
                }
                if (_topLeft != null)
                {
                    num = Math.Max(num, _topLeft._potential);
                }
                if (_topRight != null)
                {
                    num = Math.Max(num, _topRight._potential);
                }
                if (_bottomLeft != null)
                {
                    num = Math.Max(num, _bottomLeft._potential);
                }
                if (_bottomRight != null)
                {
                    num = Math.Max(num, _bottomRight._potential);
                }
                return num;
            }

            public IEnumerator<QuadNode> GetEnumerator()
            {
                Queue<Quadrant> queue = new Queue<Quadrant>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    Quadrant quadrant = queue.Dequeue();
                    if (quadrant._nodes != null)
                    {
                        QuadNode quadNode = quadrant._nodes;
                        do
                        {
                            quadNode = quadNode.Next;
                            yield return quadNode;
                        } while (quadNode != quadrant._nodes);
                    }
                    if (quadrant._topLeft != null)
                    {
                        queue.Enqueue(quadrant._topLeft);
                    }
                    if (quadrant._topRight != null)
                    {
                        queue.Enqueue(quadrant._topRight);
                    }
                    if (quadrant._bottomLeft != null)
                    {
                        queue.Enqueue(quadrant._bottomLeft);
                    }
                    if (quadrant._bottomRight != null)
                    {
                        queue.Enqueue(quadrant._bottomRight);
                    }
                }
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        //private const int MaxTreeDepth = 50;
        private Rect _extent;
        private Quadrant _root;

        public Rect Extent
        {
            get { return _extent; }
            set
            {
                if (value.Top < -1.7976931348623157E+308 || value.Top > 1.7976931348623157E+308 || value.Left < -1.7976931348623157E+308 ||
                    value.Left > 1.7976931348623157E+308 || value.Width > 1.7976931348623157E+308 || value.Height > 1.7976931348623157E+308)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _extent = value;
                ReIndex();
            }
        }

        public void Insert(T item, Rect bounds, double priority)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException("bounds");
            }
            if (_root == null)
            {
                _root = new Quadrant(_extent);
            }
            if (double.IsNaN(priority))
            {
                priority = double.NegativeInfinity;
            }
            _root.Insert(item, bounds, priority, 1);
        }

        public bool HasItemsInside(Rect bounds)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException("bounds");
            }
            return _root != null && _root.HasNodesInside(bounds);
        }

        public IEnumerable<T> GetItemsInside(Rect bounds)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException();
            }
            if (_root != null)
            {
                foreach (Tuple<QuadNode, double> current in _root.GetNodesInside(bounds))
                {
                    yield return current.Item1.Node;
                }
            }
            yield break;
        }

        public bool HasItemsIntersecting(Rect bounds)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException("bounds");
            }
            return _root != null && _root.HasNodesIntersecting(bounds);
        }

        public IEnumerable<T> GetItemsIntersecting(Rect bounds)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException();
            }
            if (_root != null)
            {
                foreach (Tuple<QuadNode, double> current in _root.GetNodesIntersecting(bounds))
                {
                    yield return current.Item1.Node;
                }
            }
            yield break;
        }

        public bool Remove(T item)
        {
            return Remove(item,
                               new Rect(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity));
        }

        public bool Remove(T item, Rect bounds)
        {
            if (bounds.Top.IsNaN() || bounds.Left.IsNaN() || bounds.Width.IsNaN() || bounds.Height.IsNaN())
            {
                throw new ArgumentOutOfRangeException("bounds");
            }
            return _root != null && _root.Remove(item, bounds);
        }

        public void Clear()
        {
            _root = null;
        }

        private void ReIndex()
        {
            Quadrant root = _root;
            _root = new Quadrant(_extent);
            if (root != null)
            {
                foreach (Tuple<QuadNode, double> current in root.GetNodesIntersecting(_extent))
                {
                    Insert(current.Item1.Node, current.Item1.Bounds, current.Item1.Priority);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_root != null)
            {
                foreach (QuadNode current in _root)
                {
                    yield return current.Node;
                }
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}