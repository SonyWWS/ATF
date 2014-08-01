//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Collection to adapt an observable collection of references to an 
    /// observable collection of their reference targets
    /// TODO: does not currently deal with Reference Targets changing
    /// </summary>
    /// <typeparam name="TRefTarget">Type of reference target</typeparam>
    /// <typeparam name="U">Type of reference target</typeparam>
    public class ReferenceCollectionAdapter<TRefTarget, U> : ObservableCollectionAdapter<IReference<TRefTarget>, U>
        where TRefTarget : class
        where U : class
    {
        public ReferenceCollectionAdapter(IObservableCollection<IReference<TRefTarget>> collection, Func<U, IReference<TRefTarget>> createReference)
            : base(collection)
        {
            m_collection = collection;
            m_createReference = createReference;

            m_targetToRefMap = new Dictionary<U, IReference<TRefTarget>>();
            foreach (var item in m_collection)
            {
                m_targetToRefMap.Add(item.Target.As<U>(), item);
            }

            m_collection.CollectionChanged += CollectionCollectionChanged;
        }

        protected override U Convert(IReference<TRefTarget> item)
        {
            return item.Target.As<U>();
        }

        protected override IReference<TRefTarget> Convert(U item)
        {
            IReference<TRefTarget> result;
            if (!m_targetToRefMap.TryGetValue(item, out result))
            {
                if (m_createReference != null)
                    return m_createReference(item);
            }

            return result;
        }

        private void CollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IReference<TRefTarget> item in e.OldItems)
                    m_targetToRefMap.Remove(item.Target.As<U>());
            }

            if (e.NewItems != null)
            {
                foreach (IReference<TRefTarget> item in e.NewItems)
                    m_targetToRefMap.Add(item.Target.As<U>(), item);
            }
        }

        private readonly Dictionary<U, IReference<TRefTarget>> m_targetToRefMap;
        private readonly IObservableCollection<IReference<TRefTarget>> m_collection;
        private readonly Func<U, IReference<TRefTarget>> m_createReference;
    }
}
