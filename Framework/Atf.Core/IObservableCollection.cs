//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf
{
    /// <summary>
    /// Interface shorthand for observable collections. Combines IList, INotifyPropertyChanged, 
    /// and INotifyCollectionChanged without adding any additional methods.</summary>
    public interface IObservableCollection : IList, INotifyPropertyChanged, INotifyCollectionChanged
    {
    }

    /// <summary>
    /// Interface shorthand for observable collections. Combines IList<T>, INotifyPropertyChanged, 
    /// and INotifyCollectionChanged without adding any additional methods.</summary>
    public interface IObservableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
    }
}
