//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for a target dialog</summary>
    public class TargetDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor with targets and protocols</summary>
        /// <param name="targets">Enumeration of targets</param>
        /// <param name="protocols">Enumeration of protocols for interacting with targets</param>
        public TargetDialogViewModel(IEnumerable<TargetViewModel> targets, IEnumerable<IProtocol> protocols)
        {
            Title = "Targets".Localize();
            Targets = new ObservableCollection<TargetViewModel>(targets);
            m_targetCv = CollectionViewSource.GetDefaultView(Targets);
            
            Protocols = new ObservableCollection<IProtocol>(protocols);
            m_protocolsCv = CollectionViewSource.GetDefaultView(Protocols);

            AddUserTargetCommand = new DelegateCommand(AddUserTarget, CanAddUserTarget, false);
            DeleteTargetCommand = new DelegateCommand(DeleteTarget, () => GetSelectedTarget() != null, false);
            EditUserTargetCommand = new DelegateCommand(EditUserTarget, CanEditUserTarget, false);
            FindTargetsCommand = new DelegateCommand(FindTargets, () =>
            {
                var protocol = GetSelectedProtocol();
                return protocol != null && protocol.CanFindTargets;
            });
            
        }

        /// <summary>
        /// Get targets</summary>
        public ObservableCollection<TargetViewModel> Targets { get; private set; }
        /// <summary>
        /// Get protocols for interacting with targets</summary>
        public ObservableCollection<IProtocol> Protocols { get; private set; }
       
        #region AddUserTargetCommand

        /// <summary>
        /// Get add user defined target command</summary>
        public ICommand AddUserTargetCommand { get; private set; }

        private bool CanAddUserTarget()
        {
            var protocol = GetSelectedProtocol();
            return protocol != null && protocol.CanCreateUserTarget;
        }

        private void AddUserTarget()
        {
            var protocol = GetSelectedProtocol();
            if (protocol != null)
            {
                var target = protocol.CreateUserTarget(null);
                if (protocol.EditUserTarget(target))
                {
                    Targets.Add(new TargetViewModel(target, protocol));
                    EnsureSelection();
                }
            }
        }

        #endregion

        #region DeleteTargetCommand

        /// <summary>
        /// Get delete target command</summary>
        public ICommand DeleteTargetCommand { get; private set; }

        private void DeleteTarget()
        {
            var vm = m_targetCv.CurrentItem as TargetViewModel;
            if (vm != null)
            {
                int currentIndex = Targets.IndexOf(vm);
                Targets.Remove(vm);

                // Select next item in the list
                currentIndex = Math.Min(currentIndex, Targets.Count - 1);
                if (currentIndex > 0)
                    Targets[currentIndex].IsSelected = true;
            }
        }

        #endregion

        #region EditUserTargetCommand

        /// <summary>
        /// Get edit target for user command</summary>
        public ICommand EditUserTargetCommand { get; private set; }

        private bool CanEditUserTarget()
        {
            var selected = m_targetCv.CurrentItem as TargetViewModel;
            return selected != null && selected.Protocol.CanCreateUserTarget;
        }

        private void EditUserTarget()
        {
            var selected = m_targetCv.CurrentItem as TargetViewModel;
            if (selected != null)
                selected.Protocol.EditUserTarget(selected.Target);
        }

        #endregion

        #region FindTargetsCommand

        /// <summary>
        /// Get find targets command</summary>
        public ICommand FindTargetsCommand { get; private set; }

        private void FindTargets()
        {
            ShowFindTargetsDialog.Raise(this, new ShowDialogEventArgs(new FindTargetsViewModel(this)));
            EnsureSelection();
        }

        /// <summary>
        /// Displaying found targets dialog event</summary>
        public event EventHandler<ShowDialogEventArgs> ShowFindTargetsDialog;

        #endregion

        private ITarget GetSelectedTarget()
        {
            var vm = m_targetCv.CurrentItem as TargetViewModel;
            return vm != null ? vm.Target : null;
        }

        /// <summary>
        /// Obtain the currently selected protocol</summary>
        /// <returns>Currently selected protocol</returns>
        internal IProtocol GetSelectedProtocol()
        {
            return m_protocolsCv.CurrentItem as IProtocol;
        }

        /// <summary>
        /// Ensure that last target is selected if no other targets are selected</summary>
        internal void EnsureSelection()
        {
            if (Targets.Count > 0 && !Targets.Any(x => x.IsSelected))
                Targets[Targets.Count - 1].IsSelected = true;
        }

        private readonly ICollectionView m_targetCv;
        private readonly ICollectionView m_protocolsCv;
    }
}
