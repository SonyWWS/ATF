//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Drag and drop target that creates a new instance of the object on drop</summary>
    public class InstancingDropTargetBehavior : DropTargetBehavior<FrameworkElement>
    {
        /// <summary>
        /// Event fired when an item is dragged over the control</summary>
        /// <param name="e">Event args</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            if (ApplicationUtil.CanInsert(AssociatedObject.DataContext, null, e.Data))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event fired when an item is dropped onto the control</summary>
        /// <param name="e">Event args</param>
        protected override void OnDrop(DragEventArgs e)
        {
            if (ApplicationUtil.CanInsert(AssociatedObject.DataContext, null, e.Data))
            {
                var statusService = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
                ApplicationUtil.Insert(AssociatedObject.DataContext, null, e.Data, "Drag Drop".Localize(), statusService);
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Adapter to convert WPF clipboard data to WinForms clipboard data</summary>
    public class DataObjectAdapter : System.Windows.Forms.IDataObject, IDisposable
    {
        private IDataObject m_adaptee;

        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">The WPF clipboard data</param>
        public DataObjectAdapter(IDataObject adaptee)
        {
            m_adaptee = adaptee;
        }

        #region IDataObject Members

        /// <summary>
        /// Retrieves the data associated with the specified class type format.</summary>
        /// <param name="format">A Type representing the format of the data to retrieve</param>
        /// <returns>The data associated with the specified format, or null</returns>
        public object GetData(Type format)
        {
            return m_adaptee.GetData(format);
        }

        /// <summary>
        /// Retrieves the data associated with the specified data format.</summary>
        /// <param name="format">The format of the data to retrieve</param>
        /// <returns>The data associated with the specified format, or null</returns>
        public object GetData(string format)
        {
            return m_adaptee.GetData(format);
        }

        /// <summary>
        /// Retrieves the data associated with the specified data format, using a Boolean to 
        /// determine whether to convert the data to the format</summary>
        /// <param name="format">The format of the data to retrieve</param>
        /// <param name="autoConvert">true to convert the data to the specified format; 
        /// otherwise, false</param>
        /// <returns>The data associated with the specified format, or null</returns>
        public object GetData(string format, bool autoConvert)
        {
            return m_adaptee.GetData(format, autoConvert);
        }

        /// <summary>
        /// Determines whether data stored in this instance is associated with, or can be converted
        /// to, the specified format.</summary>
        /// <param name="format">A Type representing the format for which to check</param>
        /// <returns>true if data stored in this instance is associated with, or can be converted 
        /// to, the specified format; otherwise, false</returns>
        public bool GetDataPresent(Type format)
        {
            return m_adaptee.GetDataPresent(format);
        }

        /// <summary>
        /// Determines whether data stored in this instance is associated with, or can be converted
        /// to, the specified format.</summary>
        /// <param name="format">The format for which to check</param>
        /// <returns>true if data stored in this instance is associated with, or can be converted 
        /// to, the specified format; otherwise false</returns>
        public bool GetDataPresent(string format)
        {
            return m_adaptee.GetDataPresent(format);
        }

        /// <summary>
        /// Determines whether data stored in this instance is associated with the specified format,
        /// using a Boolean value to determine whether to convert the data to the format.</summary>
        /// <param name="format">The format for which to check</param>
        /// <param name="autoConvert">true to determine whether data stored in this instance can be
        /// converted to the specified format; false to check whether the data is in the specified 
        /// format</param>
        /// <returns>true if the data is in, or can be converted to, the specified format; 
        /// otherwise, false</returns>
        public bool GetDataPresent(string format, bool autoConvert)
        {
            return m_adaptee.GetDataPresent(format, autoConvert);
        }

        /// <summary>
        /// Returns a list of all formats that data stored in this instance is associated with 
        /// or can be converted to.</summary>
        /// <returns>An array of the names that represents a list of all formats that are
        /// supported by the data stored in this object.</returns>
        public string[] GetFormats()
        {
            return m_adaptee.GetFormats();
        }

        /// <summary>
        /// Gets a list of all formats that data stored in this instance is associated with or 
        /// can be converted to, using a Boolean value to determine whether to retrieve all formats
        /// that the data can be converted to or only native data formats.</summary>
        /// <param name="autoConvert">true to retrieve all formats that data stored in this instance
        /// is associated with or can be converted to; false to retrieve only native data formats.</param>
        /// <returns>An array of the names that represents a list of all formats that are supported 
        /// by the data stored in this object.</returns>
        public string[] GetFormats(bool autoConvert)
        {
            return m_adaptee.GetFormats(autoConvert);
        }

        /// <summary>
        /// Stores the specified data in this instance, using the class of the data for the 
        /// format.</summary>
        /// <param name="data">The data to store</param>
        public void SetData(object data)
        {
            m_adaptee.SetData(data);
        }

        /// <summary>
        /// Stores the specified data and its associated class type in this instance.</summary>
        /// <param name="format">A Type representing the format associated with the data</param>
        /// <param name="data">The data to store</param>
        public void SetData(Type format, object data)
        {
            m_adaptee.SetData(format, data);
        }

        /// <summary>
        /// Stores the specified data and its associated format in this instance.</summary>
        /// <param name="format">The format associated with the data</param>
        /// <param name="data">The data to store</param>
        public void SetData(string format, object data)
        {
            m_adaptee.SetData(format, data);
        }

        /// <summary>
        /// Stores the specified data and its associated format in this instance, using a 
        /// Boolean value to specify whether the data can be converted to another format.</summary>
        /// <param name="format">The format associated with the data</param>
        /// <param name="autoConvert">true to allow the data to be converted to another format; 
        /// otherwise, false</param>
        /// <param name="data">The data to store</param>
        public void SetData(string format, bool autoConvert, object data)
        {
            m_adaptee.SetData(format, data, autoConvert);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose of system resources</summary>
        public void Dispose()
        {
            var dispoable = m_adaptee as IDisposable;
            if (dispoable != null)
                dispoable.Dispose();
        }

        #endregion
    }
}
