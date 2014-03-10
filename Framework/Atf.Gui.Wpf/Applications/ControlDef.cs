//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Control definition class</summary>
    public class ControlDef
    {
        /// <summary>
        /// Gets or sets Control's name</summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Control's description</summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Control's image resource</summary>
        public object ImageSourceKey { get; set; }

        /// <summary>
        /// Gets or sets Control's ID</summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets StandardControlGroup, indicating where controls are initially docked</summary>
        public StandardControlGroup Group { get; set; }
    } 
}
