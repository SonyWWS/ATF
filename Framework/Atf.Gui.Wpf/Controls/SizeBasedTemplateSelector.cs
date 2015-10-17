//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Selects a template from a list based on the display size</summary>
    public class SizeBasedTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Collection of TemplateSize objects</summary>
        public ObservableCollection<TemplateSize> TemplateSizes { get; set; }

        /// <summary>
        /// Constructor</summary>
        public SizeBasedTemplateSelector()
        {
            TemplateSizes = new ObservableCollection<TemplateSize>();
        }

        /// <summary>
        /// Selects a data template based on the available size in which to display it</summary>
        /// <param name="item">unused</param>
        /// <param name="container">ContentPresenter where the data will be displayed</param>
        /// <returns>An appropriately sized data template for the given space</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var contentPresenter = container as ContentPresenter;
            if (contentPresenter != null)
            {
                var templateWithSize = FindTemplateBySize(contentPresenter.ActualWidth, contentPresenter.ActualHeight);

                if (!m_elementSizeDictionary.ContainsKey(contentPresenter))
                {
                    contentPresenter.SizeChanged += TemplateContainerSizeChanged;
                    contentPresenter.Unloaded += RemoveContentControl;
                    m_elementSizeDictionary.Add(contentPresenter, templateWithSize);
                }
                else
                {
                    m_elementSizeDictionary[contentPresenter] = templateWithSize;
                }

                return templateWithSize.DataTemplate;
            }

            return null;
        }

        private void RemoveContentControl(object sender, RoutedEventArgs e)
        {
            var presenter = sender as ContentPresenter;
            if (presenter != null)
            {
                presenter.SizeChanged += TemplateContainerSizeChanged;
                presenter.Unloaded += RemoveContentControl;
                m_elementSizeDictionary.Remove(presenter);
            }
        }

        private TemplateSize FindTemplateBySize(double actualWidth, double actualHeight)
        {
            foreach (var templateWithSize in TemplateSizes)
            {
                if (templateWithSize.IsRightSize(actualWidth, actualHeight))
                {
                    //System.Diagnostics.Debug.WriteLine("An appropriately sized template was found for width {0} and height {1}", actualWidth, actualHeight);
                    return templateWithSize;
                }
            }

            //System.Diagnostics.Debug.WriteLine("No appropriately sized template was found for {0} {1} - using the first template instead", actualWidth, actualHeight);
            return TemplateSizes.First();
        }

        private void TemplateContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var contentControl = sender as ContentPresenter;
            if (contentControl != null)
            {
                TemplateSize templateWithSize;
                if (m_elementSizeDictionary.TryGetValue(contentControl, out templateWithSize))
                {
                    if (!templateWithSize.IsRightSize(contentControl.ActualWidth, contentControl.ActualHeight))
                    {
                        contentControl.ContentTemplateSelector = null;
                        contentControl.ContentTemplate = SelectTemplate(contentControl.DataContext, contentControl);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error occurred with template container size");
                }
            }
        }

        private readonly Dictionary<ContentPresenter, TemplateSize> m_elementSizeDictionary
            = new Dictionary<ContentPresenter, TemplateSize>();
    }

    /// <summary>
    /// Utility class that implements size restrictions on a data template</summary>
    public class TemplateSize
    {
        /// <summary>
        /// Gets and sets the data template</summary>
        public DataTemplate DataTemplate { get; set; }

        /// <summary>
        /// Gets and sets the minimum allowed size</summary>
        public Size? MinimumSize { get; set; }

        /// <summary>
        /// Gets and sets the maximum allowed size</summary>
        public Size? MaximumSize { get; set; }

        /// <summary>
        /// Checks whether the specified size is within the allowed size bounds</summary>
        /// <param name="width">Width of template</param>
        /// <param name="height">Height of template</param>
        /// <returns><c>True</c> if the size is within the allowed bounds, otherwise false</returns>
        public bool IsRightSize(double width, double height)
        {
            return ((!MinimumSize.HasValue || (MinimumSize.Value.Width <= width && MinimumSize.Value.Height <= height)) &&
                    (!MaximumSize.HasValue || (MaximumSize.Value.Width > width && MaximumSize.Value.Height > height)));
        }
    }
}