//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts a DomNode to IHelpContext using schema annotation help keys</summary>
    public class DomNodeHelpAdapter : DomNodeAdapter, IHelpContext
    {
        #region IHelpContext Members

        /// <summary>
        /// Searches Dom lineage for help keys</summary>
        /// <returns>help keys</returns>
        public string[] GetHelpKeys()
        {
            foreach (var node in DomNode.Lineage)
            {
                var ctxt = node.Type.GetTag<IHelpContext>();
                if (ctxt != null)
                {
                    return ctxt.GetHelpKeys();
                }
            }
            return null;
        }

        #endregion
    }
}
