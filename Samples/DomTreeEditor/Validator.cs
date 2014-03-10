//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// DomNode adapter that ensures certain constraints on the UI data are met:
    /// 1) Resources that are referenced are available in the same package. If not
    /// they are cloned and added to the package.</summary>
    public class Validator : Sce.Atf.Dom.Validator
    {
        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnBeginning(object sender, System.EventArgs e)
        {
            m_referenceInserts = new List<ChildEventArgs>();
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, System.EventArgs e)
        {
            foreach (ChildEventArgs refInsert in m_referenceInserts)
            {
                UIPackage dstPackage = GetPackage(refInsert.Parent);
                UIRef uiRef = refInsert.Child.As<UIRef>();
                DomNode resource = uiRef.UIObject.DomNode;
                UIPackage srcPackage = GetPackage(resource);

                if (dstPackage != srcPackage)
                {
                    DomNode[] copies = DomNode.Copy(new DomNode[] { resource });
                    UIObject refObject = copies[0].As<UIObject>();
                    uiRef.UIObject = refObject;

                    // add the cloned ref object to the package in the first child array
                    //  of compatible type
                    foreach (ChildInfo childInfo in dstPackage.DomNode.Type.Children)
                    {
                        if (childInfo.Type.IsAssignableFrom(refObject.DomNode.Type))
                        {
                            dstPackage.DomNode.GetChildList(childInfo).Add(refObject.DomNode);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions on validation Ended events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnded(object sender, System.EventArgs e)
        {
            m_referenceInserts = null;
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, System.EventArgs e)
        {
            m_referenceInserts = null;
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            // if it's a ref, make sure the referenced resource is in this package
            UIRef uiRef = e.Child.As<UIRef>();
            if (uiRef != null)
                m_referenceInserts.Add(e);
        }

        private UIPackage GetPackage(DomNode node)
        {
            foreach (DomNode ancestor in node.Lineage)
            {
                UIPackage package = ancestor.As<UIPackage>();
                if (package != null)
                    return package;
            }
            return null;
        }

        private List<ChildEventArgs> m_referenceInserts;
    }
}
