//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Collections;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// This class wraps an IObservableCollection of one type to implement IObservableCollection of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an IObservableCollection of Type1 can be made to implement an IObservableCollection of Type2, as long as Type1
    /// implements or can be adapted to Type2.
    /// This class differs from AdaptableObservableCollection in that it always creates a complete new set of adapters
    /// for the underlying collection using the IAdapterCreator passed in to the constructor.
    /// This feature is useful for WPF MVVM situations where a collection must be adapted to a new collection of unique
    /// view models rather than shared view models. 
    /// For example a list of DomNodes could be visible in two WPF list views at the same time.  These may both require
    /// the same type of view model to adapt the DomNodes but each may require a separate copy of the view model rather 
    /// than both sharing the same as would happen with normal Dom adaptation. </remarks>
    public class AdaptableViewModelCollection<T, U> : AdaptableObservableCollection<T, U>
        where T : class
        where U : class, IAdapter, new()
    {
        public AdaptableViewModelCollection(IObservableCollection<T> collection)
            : this(collection, new AdapterCreator<U>())
        {
        }

        public AdaptableViewModelCollection(IObservableCollection<T> collection, IAdapterCreator adapterCreator)
            : base(collection)
        {
            m_adapterCreator = adapterCreator;
        }

        protected override U Convert(T item)
        {
            U value;
            if (!m_adapterVieModels.TryGetValue(item, out value))
            {
                value = m_adapterCreator.GetAdapter(item, typeof(U)) as U;
                m_adapterVieModels.Add(item, value);
            }
            return value;
        }

        private IAdapterCreator m_adapterCreator;
        private Dictionary<T, U> m_adapterVieModels = new Dictionary<T, U>();
    }
}
