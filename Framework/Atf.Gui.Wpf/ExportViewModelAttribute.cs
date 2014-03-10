//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// ExportAttribute class for View model</summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ExportViewModelAttribute : ExportAttribute, IViewModelMetadata
    {
        /// <summary>
        /// Gets the metadata name</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor with metadata name</summary>
        /// <param name="name">Metadata name</param>
        public ExportViewModelAttribute(string name)
            : base(Contracts.ViewModel)
        {
            Name = name;
        }
    }
}
