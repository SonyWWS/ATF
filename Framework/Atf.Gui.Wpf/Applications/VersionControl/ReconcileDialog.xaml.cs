//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.VersionControl
{
    /// <summary>
    /// Interaction logic for ReconcileDialog.xaml
    /// </summary>
    public partial class ReconcileDialog : CommonDialog
    {
        public ReconcileDialog()
        {
            InitializeComponent();
        }
    }

    internal class CheckableItem : NotifyPropertyChangedBase
    {
        private readonly ReconcileViewModel m_parent;

        public CheckableItem(ReconcileViewModel parent, Uri uri)
        {
            m_parent = parent;
            Uri = uri;
        }

        public Uri Uri { get; private set; }
        public string LocalPath { get { return Uri.LocalPath; } }

        public bool IsChecked
        {
            get { return m_isChecked; }
            set
            {
                if (m_isChecked != value)
                {
                    m_isChecked = value;
                    OnPropertyChanged(IsCheckedArgs);
                    m_parent.CheckAllSelected(value);
                }
            }
        }
        
        private bool m_isChecked = true;
        private static readonly PropertyChangedEventArgs IsCheckedArgs
            = ObservableUtil.CreateArgs<CheckableItem>(x => x.IsChecked);

        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    OnPropertyChanged(IsSelectedArgs);
                }
            }
        }

        private bool m_isSelected;
        private static readonly PropertyChangedEventArgs IsSelectedArgs
            = ObservableUtil.CreateArgs<CheckableItem>(x => x.IsSelected);
    }

    internal class ReconcileViewModel : DialogViewModelBase
    {
        public ReconcileViewModel(SourceControlService sourceControlService, IEnumerable<Uri> modified, IEnumerable<Uri> notInDepot)
        {
            Title = "Reconcile Offline Work".Localize();

            m_sourceControlService = sourceControlService;
            m_modified = modified.Select(x => new CheckableItem(this, x)).ToList();
            m_notInDepot = notInDepot.Select(x => new CheckableItem(this, x)).ToList();

            Modified = new ListCollectionView(m_modified);
            NotInDepot = new ListCollectionView(m_notInDepot);
        }

        protected override void OnCloseDialog(CloseDialogEventArgs args)
        {
            base.OnCloseDialog(args);
            if (args.DialogResult == true)
            {
                // check out files that are locally modified but not opened
                foreach (var item in m_modified)
                {
                    if (item.IsChecked)
                        m_sourceControlService.CheckOut(item.Uri);
                }

                // add files that are missing in the depot
                foreach (var item in m_notInDepot)
                {
                    if (item.IsChecked)
                        m_sourceControlService.Add(item.Uri);
                }
            }
        }

        public void CheckAllSelected(bool check)
        {
            m_modified
                .Where(x => x.IsSelected)
                .ForEach(x => x.IsChecked = check);
            m_notInDepot
                .Where(x => x.IsSelected)
                .ForEach(x => x.IsChecked = check);
        }

        public ICollectionView Modified { get; private set; }
        public ICollectionView NotInDepot { get; private set; }

        private readonly ISourceControlService m_sourceControlService;
        private readonly List<CheckableItem> m_modified;
        private readonly List<CheckableItem> m_notInDepot;
    }
}
