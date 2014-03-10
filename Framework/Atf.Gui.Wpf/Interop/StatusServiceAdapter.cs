//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

#pragma warning disable 0067 // Event never used

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Service that manages a status display.
    /// Class to adapt Sce.Atf.Wpf.Applications.IStatusService to Sce.Atf.Applications.IStatusService.
    /// This allows WinForms-based applications to be run in a WPF based application.</summary>
    [Export(typeof(Sce.Atf.Applications.IStatusService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatusServiceAdapter : Sce.Atf.Applications.IStatusService
    {
        [Import]
        private IComposer m_composer = null;

        [Import]
        private IStatusService m_adaptee = null;

        #region IStatusService Members

        /// <summary>
        /// Displays status of an operation (e.g., "File successfully saved.")</summary>
        /// <param name="status">Status message</param>
        public void ShowStatus(string status)
        {
            m_adaptee.ShowStatus(status);
        }

        /// <summary>
        /// Adds a text status item to the status bar</summary>
        /// <param name="width">Text panel width</param>
        /// <returns>Text panel on status bar</returns>
        public Atf.Applications.IStatusText AddText(int width)
        {
            var statusText = new StatusText(width);
            m_composer.Container.ComposeExportedValue<IStatusItem>(statusText);
            return new StatusTextAdapter(statusText);
        }

        /// <summary>
        /// Adds an image status item to the status bar</summary>
        /// <returns>Image item on status bar</returns>
        public Atf.Applications.IStatusImage AddImage()
        {
            var statusImage = new StatusImage();
            m_composer.Container.ComposeExportedValue<IStatusItem>(statusImage);
            return new StatusImageAdapter(statusImage);
        }

        /// <summary>
        /// Begins progress display where client manually updates progress.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        public void BeginProgress(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begins progress display where progress is updated automatically.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        public void BeginProgress(string message, int expectedDuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begins progress display where client manually updates progress</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        public void BeginProgress(string message, bool canCancel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begins progress display where progress is updated automatically</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        public void BeginProgress(string message, int expectedDuration, bool canCancel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows progress</summary>
        /// <param name="progress">Progress, in the interval [0..1]</param>
        public void ShowProgress(double progress)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ends progress display</summary>
        public void EndProgress()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event that is raised when progress dialog is canceled</summary>
        public event EventHandler ProgressCancelled;

        #endregion

        private class StatusTextAdapter : Sce.Atf.Applications.IStatusText
        {
            public StatusTextAdapter(StatusText adaptee)
            {
                m_adaptee = adaptee;
            }
            private StatusText m_adaptee;

            #region IStatusText Members

            public string Text
            {
                get { return m_adaptee.Text; }
                set {  m_adaptee.Text = value; }
            }

            public System.Drawing.Color ForeColor
            {
                get
                {
                    var brush = m_adaptee.ForeColor as SolidColorBrush;
                    if(brush == null)
                        throw new InvalidOperationException("Not a solid color brush");

                    return System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
                set
                {
                    m_adaptee.ForeColor = new SolidColorBrush(Color.FromArgb(value.A, value.R, value.G, value.B));
                }
            }

            #endregion
        }

        private class StatusImageAdapter : Sce.Atf.Applications.IStatusImage
        {
            public StatusImageAdapter(StatusImage adaptee)
            {
                m_adaptee = adaptee;
            }

            private StatusImage m_adaptee;

            #region IStatusImage Members

            public System.Drawing.Image Image
            {
                get
                {
                    return m_adaptee.ImageSourceKey as System.Drawing.Image;
                }
                set
                {
                    // Ensure image is converted and added to app resources
                    if(value != null)
                        Util.GetOrCreateResourceForEmbeddedImage(value);

                    m_adaptee.ImageSourceKey = value;
                }
            }

            #endregion
        }
    }
}
