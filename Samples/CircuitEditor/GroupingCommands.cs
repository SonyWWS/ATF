//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.ComponentModel.Composition;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    [Export(typeof(GroupingCommands))]
    public class GroupingCommands : Sce.Atf.Controls.Adaptable.Graphs.GroupingCommands
    {
        [ImportingConstructor]
        public GroupingCommands(ICommandService commandService, IContextRegistry contextRegistry) 
            : base(commandService, contextRegistry)           
        {
            CreationOptions = GroupCreationOptions.HideUnconnectedPins;
        }

        // provide required DomNodeType
        protected override DomNodeType GroupType
        {
            get { return Schema.groupType.Type; }
        }
    }
}
