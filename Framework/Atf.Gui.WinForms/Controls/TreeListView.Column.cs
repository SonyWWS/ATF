//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Partial implementation of the TreeListView class, which provides a tree ListView</summary>
    public sealed partial class TreeListView
    {
        /// <summary>
        /// Class representing a column</summary>
        public class Column
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="label">Column's label</param>
            public Column(string label)
            {
                Label = label;
                Width = DefaultWidth;
                ActualWidth = DefaultWidth;
            }

            /// <summary>
            /// Constructor</summary>
            /// <param name="label">Column's label</param>
            /// <param name="width">Column's width</param>
            public Column(string label, int width)
            {
                Label = label;
                m_width = width;
                ActualWidth = width;
            }


            /// <summary>
            /// Gets or sets the column's label</summary>
            public string Label
            {
                get { return m_label; }
                set
                {
                    if (string.Compare(m_label, value) == 0)
                        return;

                    m_label = value;

                    LabelChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets the column's width</summary>
            public int Width
            {
                get { return m_width; }
                set
                {
                    if (m_width == value)
                        return;

                    m_width = value;

                    WidthChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets the rendered width of this column.</summary>
            /// <remarks>This property is a calculated value based on other width inputs, 
            /// and the layout algorithm</remarks>
            public int ActualWidth { get; set; }

            /// <summary>
            /// Gets or sets whether to allow editing of properties in this column</summary>
            public bool AllowPropertyEdit
            {
                get { return m_allowPropertyEdit; }
                set
                {
                    if (m_allowPropertyEdit == value)
                        return;

                    m_allowPropertyEdit = value;

                    AllowPropertyEditChanged.Raise(this, EventArgs.Empty);
                }
            }
            
            /// <summary>
            /// Gets or sets optional user data</summary>
            public object Tag { get; set; }

            /// <summary>
            /// Event that is raised after the label changes</summary>
            internal event EventHandler LabelChanged;

            /// <summary>
            /// Event that is raised after the width changes</summary>
            internal event EventHandler WidthChanged;

            /// <summary>
            /// Event that is raised after the allow property edit property changes</summary>
            internal event EventHandler AllowPropertyEditChanged;

            /// <summary>
            /// The default column width</summary>
            public const int DefaultWidth = 60;

            private string m_label;
            private int m_width;
            private bool m_allowPropertyEdit;
        }

        /// <summary>
        /// A class representing a collection of columns</summary>
        public class ColumnCollection : ICollection<Column>
        {
            #region ICollection<Column> Interface

            /// <summary>
            /// Returns a column enumerator</summary>
            /// <returns>Column enumerator</returns>
            public IEnumerator<Column> GetEnumerator()
            {
                return m_columns.GetEnumerator();
            }

            /// <summary>
            /// Returns a column enumerator</summary>
            /// <returns>Column enumerator</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_columns.GetEnumerator();
            }

            /// <summary>
            /// Adds an item to the collection</summary>
            /// <param name="item">Column to add</param>
            public void Add(Column item)
            {
                var ea = new CancelColumnEventArgs(item);

                bool cancelled = ColumnAdding.RaiseCancellable(this, ea);
                if (cancelled)
                    return;

                m_columns.Add(item);

                ColumnAdded.Raise(this, new ColumnEventArgs(item));
            }

            /// <summary>
            /// Clears everything from the collection</summary>
            public void Clear()
            {
                ColumnClearAll.Raise(this, EventArgs.Empty);

                m_columns.Clear();
            }

            /// <summary>
            /// Returns whether the collection contains the item or not</summary>
            /// <param name="item">Column</param>
            /// <returns>True iff item already in the collection</returns>
            public bool Contains(Column item)
            {
                return m_columns.Contains(item);
            }

            /// <summary>
            /// Copy. Not implemented.</summary>
            /// <param name="array">Array of columns</param>
            /// <param name="arrayIndex">Starting index</param>
            public void CopyTo(Column[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Removes a column from the collection</summary>
            /// <param name="item">Column to remove</param>
            /// <returns>True iff the column was removed</returns>
            public bool Remove(Column item)
            {
                ColumnRemoving.Raise(this, new ColumnEventArgs(item));

                return m_columns.Remove(item);
            }

            /// <summary>
            /// Gets the number of items in the collection</summary>
            public int Count
            {
                get { return m_columns.Count; }
            }

            /// <summary>
            /// Gets whether the collection is read only or not</summary>
            public bool IsReadOnly
            {
                get { return false; }
            }

            #endregion

            /// <summary>
            /// Event that is raised when attempting to add a column to the collection</summary>
            internal event EventHandler<CancelColumnEventArgs> ColumnAdding;

            /// <summary>
            /// Gets the column at the specified index</summary>
            /// <param name="index">Index</param>
            /// <returns>Column</returns>
            internal Column this[int index]
            {
                get { return m_columns[index]; }
            }

            /// <summary>
            /// Event that is raised after a column has been added to the collection</summary>
            internal event EventHandler<ColumnEventArgs> ColumnAdded;

            /// <summary>
            /// Event that is raised when removing a column from the collection</summary>
            internal event EventHandler<ColumnEventArgs> ColumnRemoving;

            /// <summary>
            /// Event that is raised when the collection's Clear() method is called</summary>
            internal event EventHandler ColumnClearAll;

            private readonly List<Column> m_columns = new List<Column>();
        }

        /// <summary>
        /// Column event arguments</summary>
        internal class ColumnEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="column">Column</param>
            public ColumnEventArgs(Column column)
            {
                Column = column;
            }

            /// <summary>
            /// Gets the column</summary>
            public Column Column { get; private set; }
        }

        /// <summary>
        /// Cancelable column event arguments</summary>
        internal class CancelColumnEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="column">Column</param>
            public CancelColumnEventArgs(Column column)
                : base(false)
            {
                Column = column;
            }

            /// <summary>
            /// Gets the column</summary>
            public Column Column { get; private set; }
        }
    }
}