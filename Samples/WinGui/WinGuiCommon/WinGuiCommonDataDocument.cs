//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
/*
using System.Drawing;
using System.Windows.Forms;
*/

namespace WinGuiCommon
{
    /// <summary>
    /// Sample document</summary>
    public class WinGuiCommonDataDocument : DomDocument
    {
        /// <summary>
        /// Constructor</summary>
        public WinGuiCommonDataDocument()
        {
        }

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return Editor.DocumentClientInfo.FileType; }
        }

        /// <summary>
        /// Raises the UriChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        /// <summary>
        /// Raises the DirtyChanged event</summary>
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

            WinGuiCommonDataContext context = this.As<WinGuiCommonDataContext>();
            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }
    }
}
