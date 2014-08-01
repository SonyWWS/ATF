//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for an output window</summary>
    public class OutputVm : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        public OutputVm()
        {
            OutputItems = new ObservableCollection<OutputItemVm>();
            
            CollectionViewSource.GetDefaultView(OutputItems).Filter 
                += obj => m_filterState[((OutputItemVm)obj).MessageType];

            ClearAllCommand = new DelegateCommand(() => OutputItems.Clear());
        }

        /// <summary>
        /// Items to display in output</summary>
        public ObservableCollection<OutputItemVm> OutputItems { get; private set; }

        /// <summary>
        /// Gets the command to clear all entries from the output</summary>
        public ICommand ClearAllCommand { get; private set; }

        /// <summary>
        /// Gets and sets whether warnings are shown in the output</summary>
        public bool ShowWarnings
        {
            get { return m_filterState[OutputMessageType.Warning]; }
            set
            {
                m_filterState[OutputMessageType.Warning] = value;
                OnPropertyChanged(s_showWarningsArgs);
            }
        }

        /// <summary>
        /// Gets and sets whether errors are shown in the output</summary>
        public bool ShowErrors
        {
            get { return m_filterState[OutputMessageType.Error]; }
            set
            {
                m_filterState[OutputMessageType.Error] = value;
                OnPropertyChanged(s_showErrorsArgs);
            }
        }

        /// <summary>
        /// Gets and sets whether info level messages are shown in the output</summary>
        public bool ShowInfo
        {
            get { return m_filterState[OutputMessageType.Info]; }
            set
            {
                m_filterState[OutputMessageType.Info] = value;
                OnPropertyChanged(s_showInfoArgs);
            }
        }

        /// <summary>
        /// Event fired when the OutputItems list is changed</summary>
        /// <param name="e">Event args</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(OutputItems).Refresh();
            base.OnPropertyChanged(e);
        }

        private static readonly PropertyChangedEventArgs s_showWarningsArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowWarnings);

        private static readonly PropertyChangedEventArgs s_showErrorsArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowErrors);

        private static readonly PropertyChangedEventArgs s_showInfoArgs
            = ObservableUtil.CreateArgs<OutputVm>(x => x.ShowInfo);

        private readonly Dictionary<OutputMessageType, bool> m_filterState 
            = new Dictionary<OutputMessageType, bool>() 
            { 
                { OutputMessageType.Info, true }, 
                { OutputMessageType.Warning, true }, 
                { OutputMessageType.Error, true} 
            };
    }

    /// <summary>
    /// View model for items in an output window</summary>
    public class OutputItemVm : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="time"></param>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        public OutputItemVm(DateTime time, OutputMessageType messageType, string message)
        {
            Time = time;
            MessageType = messageType;
            Message = message.Replace(Environment.NewLine, string.Empty);
        }

        /// <summary>
        /// Gets the timestamp</summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Gets the message type (error, warning, info)</summary>
        public OutputMessageType MessageType { get; private set; }

        /// <summary>
        /// Gets the output message</summary>
        public string Message { get; private set; }

        /// <summary>
        /// Converts the output item to a string with the timestamp and message</summary>
        /// <returns>The formatted string</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Time, Message);
        }
    }
}
