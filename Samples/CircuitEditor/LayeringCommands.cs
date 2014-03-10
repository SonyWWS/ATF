//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using Sce.Atf.Applications;
using Sce.Atf.Dom;


namespace CircuitEditorSample
{
    [Export(typeof(LayeringCommands))]
    public class LayeringCommands : Sce.Atf.Controls.Adaptable.Graphs.LayeringCommands
    {
        [ImportingConstructor]
        public LayeringCommands(ICommandService commandService, IContextRegistry contextRegistry, LayerLister layerLister) :
            base(commandService, contextRegistry, layerLister)
        {
        }

        protected override DomNodeType LayerFolderType
        {
            get { return Schema.layerFolderType.Type; }
        }
    }
}
