//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Singleton class to manage view models and their attachment to views</summary>
    [Export]
    [Export(typeof(IInitializable))]
    public class ViewModelRepository : IPartImportsSatisfiedNotification, IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component</summary>
        public void Initialize()
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the lazy indirect reference to an object and its associated metadata</summary>
        [ImportMany(Contracts.ViewModel, AllowRecomposition = true)]
        public IEnumerable<Lazy<object, IViewModelMetadata>> ViewModelsLazy { get; set; }

        #region IPartImportsSatisfiedNotification Members


        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        public void OnImportsSatisfied()
        {
            lock (s_unsatisfiedContracts)
            {
                for (int i = 0; i < s_unsatisfiedContracts.Count; i++)
                {
                    var reCompositionItem = s_unsatisfiedContracts[i];

                    var vm = GetViewModel(reCompositionItem.VMContract, reCompositionItem.IsShared);
                    if (vm != null)
                    {
                        if (reCompositionItem.Reference.IsAlive) // if the UI element is still alive
                        {
                            ((FrameworkElement)reCompositionItem.Reference.Target).DataContext = vm;
                        }
                        //remove the item from the list of unsatisfied contracts
                        s_unsatisfiedContracts.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets view model for given contract</summary>
        /// <param name="contract">Contract</param>
        /// <param name="isShared">Whether the model is a shared view or not</param>
        /// <returns>View model for given contract</returns>
        public object GetViewModel(string contract, bool isShared)
        {
            if (isShared)
            {
                var lazy = ViewModelsLazy.SingleOrDefault(v => v.Metadata.Name.Equals(contract));
                if (lazy != null)
                    return lazy.Value;
            }
            else
            {
                throw new NotSupportedException("Only shared view models are currently supported by ViewModelRepository");
            }

            return null;
        }

        /// <summary>
        /// Attaches a view model for a given contract to a view</summary>
        /// <param name="vmContract">Contract</param>
        /// <param name="view">FrameworkElement representing a view</param>
        /// <param name="isShared">Whether the model is a shared view or not</param>
        public static void AttachViewModelToView(string vmContract, FrameworkElement view, bool isShared)
        {
            if (Composer.Current != null)
            {
                var instance = Composer.Current.Container.GetExportedValueOrDefault<ViewModelRepository>();

                if (instance != null)
                {
                    var vm = instance.GetViewModel(vmContract, isShared);
                    if (vm != null)
                    {
                        view.DataContext = vm;
                    }
                    else
                    {
                        RegisterMissingViewModel(vmContract, view, isShared);
                    }

                    return;
                }
            }
                
            RegisterMissingViewModel(vmContract, view, isShared);
        }

        private static void RegisterMissingViewModel(string vmContractName, FrameworkElement view, bool isShared)
        {
            lock (s_unsatisfiedContracts)
            {
                s_unsatisfiedContracts.Add(new RecompositionItemPending(vmContractName, new WeakReference(view), isShared));
            }
        }

        private static readonly List<RecompositionItemPending> s_unsatisfiedContracts = new List<RecompositionItemPending>();

        private class RecompositionItemPending
        {
            public RecompositionItemPending(string vmContractName, WeakReference weakReference, bool isShared)
            {
                VMContract = vmContractName;
                Reference = weakReference;
                IsShared = isShared;
            }

            public string VMContract { get; private set; }
            public WeakReference Reference { get; private set; }
            public bool IsShared { get; private set; }
        }
    }
}
