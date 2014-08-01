//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Interaction logic for the FindFileDialog that resolves missing files.</summary>
    public class FindFileDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        public FindFileDialogViewModel()
        {
            Title = "Find File".Localize();
            Action = FindFileAction.UserSpecify;
        }

        /// <summary>
        /// Gets and sets the path specified by the user</summary>
        public string OriginalPath
        {
            get { return m_originalPath; }
            set
            {
                m_originalPath = value;
                OnPropertyChanged(s_originalPathArgs);
            }
        }

        /// <summary>
        /// Gets and sets the path where the file was actually found</summary>
        public string SuggestedPath
        {
            get { return m_suggestedPath; }
            set
            {
                m_suggestedPath = value;
                if (!string.IsNullOrEmpty(m_suggestedPath))
                {
                    Action = FindFileAction.AcceptSuggestion;
                }

                OnPropertyChanged(s_suggestedPathArgs);
            }
        }

        /// <summary>
        /// Gets and sets what action to take to resolve the missing file</summary>
        public FindFileAction Action
        {
            get { return m_action; }
            set
            {
                m_action = value;
                OnPropertyChanged(s_actionArgs);
            }
        }

        private static readonly PropertyChangedEventArgs s_originalPathArgs
            = ObservableUtil.CreateArgs<FindFileDialogViewModel>(x => x.OriginalPath);
        private static readonly PropertyChangedEventArgs s_suggestedPathArgs
            = ObservableUtil.CreateArgs<FindFileDialogViewModel>(x => x.SuggestedPath);
        private static readonly PropertyChangedEventArgs s_actionArgs
            = ObservableUtil.CreateArgs<FindFileDialogViewModel>(x => x.Action);

        private string m_originalPath;
        private string m_suggestedPath;
        private FindFileAction m_action;
    }
}
