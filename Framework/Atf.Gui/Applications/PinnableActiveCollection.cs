//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;

namespace Sce.Atf
{
    /// <summary>
    /// Collection representing active items that can be pinned. When a new active item is set,
    /// if the maximum number of entries in the collection has been reached, unpinned items are
    /// deleted before pinned items.</summary>
    /// <typeparam name="T">Type of item in collection, must be reference type</typeparam>
    public class PinnableActiveCollection<T> : ActiveCollection<T>
        where T : class
    {
        /// <summary>
        /// Constructor for a collection that can contain an effectively unlimited number
        /// of items (int.MaxValue)</summary>
        public PinnableActiveCollection()
        {
        }

        /// <summary>
        /// Constructor for a collection with a specified maximum size</summary>
        /// <param name="maximumCount">Maximum number of items that this collection can contain</param>
        public PinnableActiveCollection(int maximumCount)
            : base(maximumCount)
        {
        }

        /// <summary>
        /// Gets or sets the active item. If the item is not in the collection,
        /// it is added. If the maximum collection size is exceeded, the oldest
        /// non-pinned item in the list is removed.</summary>
        public override T ActiveItem
        {
            get { return base.ActiveItem; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                T oldItem = ActiveItem;
                if (value != oldItem)
                {
                    OnActiveItemChanging(EventArgs.Empty);

                    if (Contains(value))
                    {
                        T item = RawList[RawList.IndexOf(value)];
                        if ((item is IPinnable) && (value is IPinnable))
                        {
                            // Make sure to preserve the pinned state if this item was already in the list.
                            var pinnableItem = item as IPinnable;
                            var pinnableValue = value as IPinnable;
                            if (pinnableItem.Pinned != pinnableValue.Pinned)
                                pinnableValue.Pinned = pinnableItem.Pinned;
                        }

                        // already in collection, make it the most recent item
                        RawList.Remove(value);
                        RawList.Add(value);
                    }
                    else
                    {
                        // If maximum will be exceeded, remove the least recent non-pinned item.
                        if (Count == MaximumCount)
                        {
                            int indexToRemove = 0;
                            T itemToRemove = null;
                            foreach (var item in RawList)
                            {
                                if (!((item is IPinnable) && ((IPinnable)item).Pinned))
                                {
                                    itemToRemove = item;
                                    break;
                                }
                            }

                            if (itemToRemove != null)
                            {
                                indexToRemove = RawList.IndexOf(itemToRemove);
                                RawList.Remove(itemToRemove);
                            }
                            else
                            {
                                // All items are pinned, remove the oldest.
                                itemToRemove = RawList[0];
                                RawList.RemoveAt(0);
                            }
                            OnItemRemoved(new ItemRemovedEventArgs<T>(indexToRemove, itemToRemove));
                        }

                        // add new item to collection
                        RawList.Add(value);
                        OnItemAdded(new ItemInsertedEventArgs<T>(0, value));
                    }

                    OnActiveItemChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Utility for checking the pinned state of an item by URI</summary>
        /// <param name="uri">URI to look for</param>
        /// <returns>Null if the URI is not found in the list, otherwise returns the pinned state</returns>
        public bool? GetPinnedState(Uri uri)
        {
            foreach (object item in RawList)
            {
                if (!(item is IPinnable)) continue;

                var pinnableItem = item as IPinnable;
                MemberInfo[] memberInfos = pinnableItem.GetType().GetMember("Uri");
                foreach (MemberInfo memberInfo in memberInfos)
                {
                    if (!(memberInfo is PropertyInfo)) continue;

                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    MethodInfo methodInfo = propertyInfo.GetGetMethod();
                    if (methodInfo != null)
                    {
                        Uri itemUri = methodInfo.Invoke(pinnableItem, null) as Uri;
                        if ((itemUri != null) && (Uri.Equals(itemUri, uri)))
                        {
                            return pinnableItem.Pinned;
                        }
                    }
                }
            }

            return null;
        }
    }
}
