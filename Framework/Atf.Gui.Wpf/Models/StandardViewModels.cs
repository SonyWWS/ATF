//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Obtains a TypeCatalog of all the Sce.Atf.Wpf.Models namespace’s extensions (plus Sce.Atf.Wpf.ViewModelRepository) 
    /// for ready inclusion in an AggregateCatalog so that these extensions may be used easily</summary>
    public class StandardViewModels
    {
        /// <summary>
        /// Gets type catalog for all components</summary>
        public static ComposablePartCatalog Catalog
        {
            get 
            { 
                return new TypeCatalog(
                    typeof(ViewModelRepository),
                    typeof(MainMenuViewModel),
                    typeof(ToolBarViewModel),
                    typeof(StatusBarViewModel));
            }
        }
    }
}
