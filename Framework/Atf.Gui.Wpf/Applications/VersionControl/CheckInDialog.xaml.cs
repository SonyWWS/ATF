//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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
    /// Interaction logic for CheckInDialog.xaml
    /// </summary>
    public partial class CheckInDialog : CommonDialog
    {
        public CheckInDialog()
        {
            InitializeComponent();
        }
    }

    internal class CheckInItem : NotifyPropertyChangedBase
    {
        private readonly CheckInViewModel m_parent;

        public CheckInItem(CheckInViewModel parent, IResource resource)
        {
            m_parent = parent;
            Resource = resource;
        }

        public IResource Resource { get; private set; }
        public string LocalPath { get { return Resource.Uri.LocalPath; } }

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
            = ObservableUtil.CreateArgs<CheckInItem>(x => x.IsChecked);

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
            = ObservableUtil.CreateArgs<CheckInItem>(x => x.IsSelected);
    }

    internal class CheckInViewModel : DialogViewModelBase
    {
        public CheckInViewModel(SourceControlService sourceControlService, IEnumerable<IResource> toCheckIn)
        {
            Title = "Check In Files".Localize();

            m_sourceControlService = sourceControlService;
            m_checkInItems = toCheckIn.Select(x => new CheckInItem(this, x)).ToList();

            Items = new ListCollectionView(m_checkInItems);
        }
        
        public ICollectionView Items { get; private set; }
        public string Description { get; set; }

        protected override void OnCloseDialog(CloseDialogEventArgs args)
        {
            base.OnCloseDialog(args);
            if (args.DialogResult == true)
            {
                var uris = (from item in m_checkInItems
                            where item.IsChecked
                            select item.Resource.Uri);

                m_sourceControlService.CheckIn(uris, Description);
            }
        }

        public void CheckAllSelected(bool check)
        {
            m_checkInItems
                .Where(x => x.IsSelected)
                .ForEach(x => x.IsChecked = check);
        }

        private readonly ISourceControlService m_sourceControlService;
        private readonly List<CheckInItem> m_checkInItems;
    }
}
