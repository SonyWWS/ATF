//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System;
using System.Collections.Generic;


namespace CircuitEditorSample
{
    public class TemplateFolder : Sce.Atf.Dom.TemplateFolder
    {
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.templateFolderType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.templateFolderType.nameAttribute, value); }
        }

        public override IList<Sce.Atf.Dom.Template> Templates
        {
            get { return GetChildList<Sce.Atf.Dom.Template>(Schema.templateFolderType.templateChild); }
        }

        public override IList<Sce.Atf.Dom.TemplateFolder> Folders
        {
            get { return GetChildList<Sce.Atf.Dom.TemplateFolder>(Schema.templateFolderType.templateFolderChild); }
        }

        public override Uri Url
        {
            get { return GetAttribute<Uri>(Schema.templateFolderType.referenceFileAttribute); }
            set { SetAttribute(Schema.templateFolderType.referenceFileAttribute, value); }
        }
    }
}
