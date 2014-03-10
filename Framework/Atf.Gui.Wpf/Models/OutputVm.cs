//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models
{
    internal class OutputVm : NotifyPropertyChangedBase
    {
        public OutputVm()
        {
            OutputItems = new ObservableCollection<OutputItemVm>();
            
            CollectionViewSource.GetDefaultView(OutputItems).Filter 
                += obj => m_filterState[((OutputItemVm)obj).MessageType];

            ClearAllCommand = new DelegateCommand(() => OutputItems.Clear());
        }

        public ObservableCollection<OutputItemVm> OutputItems { get; private set; }

        public ICommand ClearAllCommand { get; private set; }

        public bool ShowWarnings
        {
            get { return m_filterState[OutputMessageType.Warning]; }
            set
            {
                m_filterState[OutputMessageType.Warning] = value;
                OnPropertyChanged(s_showWarningsArgs);
            }
        }

        private static readonly PropertyChangedEventArgs s_showWarningsArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowWarnings);

        public bool ShowErrors
        {
            get { return m_filterState[OutputMessageType.Error]; }
            set
            {
                m_filterState[OutputMessageType.Error] = value;
                OnPropertyChanged(s_showErrorsArgs);
            }
        }

        private static readonly PropertyChangedEventArgs s_showErrorsArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowErrors);

        public bool ShowInfo
        {
            get { return m_filterState[OutputMessageType.Info]; }
            set
            {
                m_filterState[OutputMessageType.Info] = value;
                OnPropertyChanged(s_showInfoArgs);
            }
        }

        private static readonly PropertyChangedEventArgs s_showInfoArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowInfo);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(OutputItems).Refresh();
            base.OnPropertyChanged(e);
        }

        private Dictionary<OutputMessageType, bool> m_filterState 
            = new Dictionary<OutputMessageType, bool>() 
            { 
                { OutputMessageType.Info, true }, 
                { OutputMessageType.Warning, true }, 
                { OutputMessageType.Error, true} 
            };
    }

    internal class OutputItemVm : NotifyPropertyChangedBase
    {
        public OutputItemVm(DateTime time, OutputMessageType messageType, string message)
        {
            Time = time;
            MessageType = messageType;
            Message = message.Replace(System.Environment.NewLine, string.Empty); ;
        }

        public DateTime Time { get; private set; }

        public OutputMessageType MessageType { get; private set; }

        public string Message { get; private set; }
    }
}
