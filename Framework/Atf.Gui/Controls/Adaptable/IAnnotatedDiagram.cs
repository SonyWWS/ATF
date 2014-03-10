//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for diagrams that contain annotations</summary>
    public interface IAnnotatedDiagram
    {
        /// <summary>
        /// Gets the sequence of annotations in the context</summary>
        IEnumerable<IAnnotation> Annotations
        {
            get;
        }
    }
}
