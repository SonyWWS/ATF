//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.ComponentModel.Composition;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Component that defines circuit-specific commands for group and ungroup. Grouping takes
    /// modules and the connections between them and turns those into a single element that is equivalent.</summary>
    [Export(typeof(GroupingCommands))]
    public class GroupingCommands : Sce.Atf.Controls.Adaptable.Graphs.GroupingCommands
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public GroupingCommands(ICommandService commandService, IContextRegistry contextRegistry) 
            : base(commandService, contextRegistry)           
        {
            CreationOptions = GroupCreationOptions.HideUnconnectedPins;
        }

        // provide required DomNodeType
        /// <summary>
        /// Gets type for Group</summary>
        protected override DomNodeType GroupType
        {
            get { return Schema.groupType.Type; }
        }
    }
}
