//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sce.Atf;
using Sce.Atf.Applications;

namespace WinGuiCommon
{
    public class EditorBase
    {
        /// <summary>
        /// Document editor information for editor</summary>
        public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfo(
            Localizer.Localize("Gui App Data"),
            new string[] { ".gad" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);

        public void Initialize(ScriptingService scriptingService)
        {
            scriptingService.LoadAssembly(GetType().Assembly);
        }
    }
}
