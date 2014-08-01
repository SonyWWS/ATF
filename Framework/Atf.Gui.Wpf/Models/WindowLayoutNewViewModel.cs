//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Models
{
    internal sealed class WindowLayoutNewViewModel : DialogViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="existing">List of existing layout names</param>
        public WindowLayoutNewViewModel(IEnumerable<string> existing)
        {
            Title = "New Layout".Localize();
            m_existingItems = existing;
        }

        /// <summary>
        /// Gets and sets the layout name</summary>
        public string LayoutName
        {
            get { return m_layoutName.Trim(); }
            set
            {
                m_layoutName = value;
                RaisePropertyChanged("LayoutName");
            }
        }

        #region IDataErrorInfo Members

        /// <summary>
        /// Always returns null</summary>
        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the error message for the property with the given name</summary>
        /// <param name="columnName">Should be "LayoutName"</param>
        /// <returns>The error message, if any, about the validity of LayoutName</returns>
        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "LayoutName")
                {
                    if (!string.IsNullOrEmpty(LayoutName))
                        result = IsValidName(LayoutName);
                }

                return result;
            }
        }

        #endregion

        /// <summary>
        /// Indicates whether the dialog can be closed with an "OK" result</summary>
        /// <returns>Returns true if the currently specified layout name is valid 
        /// for a new layout </returns>
        protected override bool CanExecuteOk()
        {
            return IsValidName(LayoutName) == null;
        }

        string IsValidName(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                return "Layout name is empty";

            if (!WindowLayoutService.IsValidLayoutName(layoutName))
                return "Layout name contains invalid characters";

            if (m_existingItems.Contains(layoutName))
                return "Layout name already exists";

            return null;
        }

        private string m_layoutName = string.Empty;
        private readonly IEnumerable<string> m_existingItems;
    }
}
