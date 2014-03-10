//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Specifies a hit on an item's tiltlebar part</summary>
    public class DiagramTitleBar
    {
        public DiagramTitleBar(object item)
        {
            Item = item;
        }

        public readonly object Item;
    }
}
