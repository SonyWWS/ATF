//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Obtains a TypeCatalog of all the Sce.Atf.Wpf.Interop namespace’s extensions for ready inclusion in an AggregateCatalog
    /// so that these extensions may be used easily</summary>
    public class StandardInteropParts
    {
        /// <summary>
        /// Gets type catalog for all components</summary>
        public static ComposablePartCatalog Catalog
        {
            get 
            { 
                return new TypeCatalog(
                    typeof(MainWindowAdapter),
                    typeof(CommandServiceAdapter),
                    typeof(ContextMenuService),
                    typeof(DialogService),
                    typeof(ControlHostServiceAdapter)
                );
            }
        }
    }
}
