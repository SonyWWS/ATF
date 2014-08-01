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
    public class TargetDialogViewModel : DialogViewModelBase
    {
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

        public ObservableCollection<TargetViewModel> Targets { get; private set; }
        public ObservableCollection<IProtocol> Protocols { get; private set; }
       
        #region AddUserTargetCommand

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

        public ICommand FindTargetsCommand { get; private set; }

        private void FindTargets()
        {
            ShowFindTargetsDialog.Raise(this, new ShowDialogEventArgs(new FindTargetsViewModel(this)));
            EnsureSelection();
        }

        public event EventHandler<ShowDialogEventArgs> ShowFindTargetsDialog;

        #endregion

        private ITarget GetSelectedTarget()
        {
            var vm = m_targetCv.CurrentItem as TargetViewModel;
            return vm != null ? vm.Target : null;
        }

        internal IProtocol GetSelectedProtocol()
        {
            return m_protocolsCv.CurrentItem as IProtocol;
        }

        internal void EnsureSelection()
        {
            if (Targets.Count > 0 && !Targets.Any(x => x.IsSelected))
                Targets[Targets.Count - 1].IsSelected = true;
        }

        private readonly ICollectionView m_targetCv;
        private readonly ICollectionView m_protocolsCv;
    }
}
