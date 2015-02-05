//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// Document for managing event sequences</summary>
    public class EventSequenceDocument : DomDocument
    {
        /// <summary>
        /// Constructor</summary>
        public EventSequenceDocument()
        {
        }

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return Editor.DocumentClientInfo.FileType; }
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        private void UpdateControlInfo()
        {
            string filePath = Uri.LocalPath;
            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            EventSequenceContext context = this.As<EventSequenceContext>();
            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }
    }
}
