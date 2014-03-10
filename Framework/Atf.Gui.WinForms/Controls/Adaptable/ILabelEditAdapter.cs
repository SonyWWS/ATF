//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that can invoke label editing operations</summary>
    public interface ILabelEditAdapter
    {
        /// <summary>
        /// Begins a label editing operation</summary>
        /// <param name="namingContext">Naming context that performs naming operations</param>
        /// <param name="item">Item with label</param>
        /// <param name="label">Information about label</param>
        void BeginEdit(INamingContext namingContext, object item, DiagramLabel label);

        /// <summary>
        /// Ends the current label editing operation</summary>
        void EndEdit();
    }
}
