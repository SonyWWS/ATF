//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.Windows;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Locates the ViewModel used</summary>
    public class ViewModelLocator
    {
        #region ViewModel

        /// <summary>
        /// ViewModel attached dependency property</summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.RegisterAttached("ViewModel", typeof(string), typeof(ViewModelLocator),
                new PropertyMetadata((string)String.Empty,
                    OnViewModelChanged));

        /// <summary>
        /// Gets the ViewModel property. This dependency property indicates the ViewModel used.</summary>
        /// <param name="d">Dependency object to obtain property for</param>
        /// <returns>ViewModel property</returns>
        public static string GetViewModel(DependencyObject d)
        {
            return (string)d.GetValue(ViewModelProperty);
        }

        /// <summary>
        /// Sets the ViewModel property. This dependency property indicates the ViewModel used.</summary>
        /// <param name="d">Dependency object to set property for</param>
        /// <param name="value">ViewModel property</param>
        public static void SetViewModel(DependencyObject d, string value)
        {
            d.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the ViewModel property</summary>
        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string vmContractName = (string)e.NewValue;
            var element = d as FrameworkElement;
            AttachViewModel(element, vmContractName, false);
        }

        #endregion

        #region SharedViewModel

        /// <summary>
        /// SharedViewModel attached dependency property</summary>
        public static readonly DependencyProperty SharedViewModelProperty =
            DependencyProperty.RegisterAttached("SharedViewModel", typeof(string), typeof(ViewModelLocator),
                new PropertyMetadata((string)null,
                    OnSharedViewModelChanged));

        /// <summary>
        /// Gets the SharedViewModel property. This dependency property indicates the type of ViewModel used.</summary>
        /// <param name="d">Dependency object to obtain property for</param>
        /// <returns>SharedViewModel property</returns>
        public static string GetSharedViewModel(DependencyObject d)
        {
            return (string)d.GetValue(SharedViewModelProperty);
        }

        /// <summary>
        /// Sets the SharedViewModel property. This dependency property indicates the type of ViewModel used.</summary>
        /// <param name="d">Dependency object to set property for</param>
        /// <param name="value">SharedViewModel property</param>
        public static void SetSharedViewModel(DependencyObject d, string value)
        {
            d.SetValue(SharedViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the SharedViewModel property</summary>
        private static void OnSharedViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string vmContractName = (string)e.NewValue;
            var element = d as FrameworkElement;
            AttachViewModel(element, vmContractName, true);
        }

        #endregion

        private static void AttachViewModel(FrameworkElement element, string vmContractName, bool isShared)
        {
            if (element == null)
                throw new ArgumentException("Invalid element for attached property");

            try
            {
                if (!String.IsNullOrEmpty(vmContractName))
                {
                    ViewModelRepository.AttachViewModelToView(vmContractName, element, isShared);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while resolving ViewModel. " + ex);
            }
        }

    }
}
