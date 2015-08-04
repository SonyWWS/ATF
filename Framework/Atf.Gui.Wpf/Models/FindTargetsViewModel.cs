//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Interaction logic for the FindTargetsDialog that finds targets.</summary>
    class FindTargetsViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="parent">Parent target dialog view model</param>
        public FindTargetsViewModel(TargetDialogViewModel parent)
        {
            m_parent = parent;

            Title = "Find Targets".Localize();
            ToggleScanCommand = new DelegateCommand(ToggleScan);
            AddAllFoundTargetsCommand = new DelegateCommand(AddAllFoundTargets, CanAddAllFoundTargets, false);

            FoundTargets = new ObservableCollection<TargetViewModel>();
            FoundTargets.CollectionChanged += (s, e) => CommandManager.InvalidateRequerySuggested();
            m_foundTargetCv = CollectionViewSource.GetDefaultView(FoundTargets);
        }

        /// <summary>
        /// Gets the list of discovered targets</summary>
        public ObservableCollection<TargetViewModel> FoundTargets { get; private set; }

        /// <summary>
        /// Gets whether a target scan is currently in progress</summary>
        public bool IsScanning
        {
            get { return m_isScanning; }
            private set
            {
                m_isScanning = value;
                OnPropertyChanged(s_isScanningArgs);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Gets the ICommand to toggle the target scan on or off</summary>
        public ICommand ToggleScanCommand { get; private set; }

        /// <summary>
        /// Gets the ICommand to add a newly discovered target to the parent's list</summary>
        public ICommand AddFoundTargetCommand
        {
            get
            {
                return m_addFoundTargetCommand ??
                    (m_addFoundTargetCommand = new DelegateCommand(AddFoundTarget, CanAddFoundTarget, false));
            }
        }

        /// <summary>
        /// Gets the ICommand to add all discovered targets to the parent's list</summary>
        public ICommand AddAllFoundTargetsCommand { get; private set; }

        private void ToggleScan()
        {
            if (!IsScanning && m_worker == null)
            {
                IsScanning = true;
                FoundTargets.Clear();

                m_worker = new BackgroundWorker { WorkerSupportsCancellation = true };
                m_worker.DoWork += DoWork;
                m_worker.RunWorkerCompleted += RunWorkerCompleted;
                m_worker.RunWorkerAsync(this);
            }
            else if (IsScanning && m_worker != null)
            {
                m_worker.CancelAsync();
            }
        }

        private bool CanAddFoundTarget()
        {
            return m_foundTargetCv.CurrentItem is TargetViewModel;
        }

        private void AddFoundTarget()
        {
            var foundTargets = FoundTargets.Where(x => x.IsSelected).ToArray();
            foreach (var target in foundTargets)
            {
                AddFoundTarget(target);
            }
        }

        private void AddFoundTarget(TargetViewModel target)
        {
            if (target != null)
            {
                if (!m_parent.Targets.Select(x => x.Target).Contains(target.Target))
                {
                    FoundTargets.Remove(target);
                    target.IsSelected = false;
                    m_parent.Targets.Add(target);
                }
            }
        }

        private bool CanAddAllFoundTargets()
        {
            return FoundTargets.Count > 0;
        }

        private void AddAllFoundTargets()
        {
            foreach (var target in FoundTargets.ToArray())
                AddFoundTarget(target);
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            var result = new List<ITarget>();

            var protocol = m_parent.GetSelectedProtocol();
            foreach (var target in protocol.FindTargets())
            {
                if (bgw.CancellationPending)
                    return;

                result.Add(target);
            }

            e.Result = result;
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null && e.Result is IList<ITarget>)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var target in (IList<ITarget>)e.Result)
                    {
                        if (target != null)
                        {
                            var protocol = m_parent.Protocols.FirstOrDefault(x => x.Id == target.ProtocolId);
                            if (protocol != null)
                            {
                                FoundTargets.Add(new TargetViewModel(target, protocol));
                            }
                        }
                    }
                }));
            }

            m_worker.DoWork -= DoWork;
            m_worker.RunWorkerCompleted -= RunWorkerCompleted;
            m_worker = null;
            IsScanning = false;
        }

        private bool m_isScanning;
        private static readonly PropertyChangedEventArgs s_isScanningArgs
            = ObservableUtil.CreateArgs<FindTargetsViewModel>(x => x.IsScanning);

        private DelegateCommand m_addFoundTargetCommand;

        private readonly TargetDialogViewModel m_parent;
        private readonly ICollectionView m_foundTargetCv;
        private BackgroundWorker m_worker;
    }
}
