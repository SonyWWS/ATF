//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Provides Open Sound Control support that is compatible with the Lemur iPad app, made by
    /// the Liine company. http://liine.net/en/products/lemur/
    /// Consider also using OscCommands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IOscService))]
    [Export(typeof(LemurOscService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LemurOscService : OscService
    {
        /// <summary>
        /// Adds OSC addresses (one for the x-coordinate and one for the y-coordinate) for a list
        /// of DOM children that have attributes that are arrays of floats. Each array of floats
        /// represents a 2D point where the first float is the x coordinate and the second float
        /// is the y coordinate.</summary>
        /// <param name="childInfo">The child info which defines the list of children of a selected DomNode</param>
        /// <param name="childAttributeDesc">The attribute on each child that defines the array of floats</param>
        /// <param name="oscAddress">The base OSC address to use. "/x" and "/y" will be appended for
        /// the x-coordinate array and the y-coordinate array, which is how Lemur sends and receives
        /// 2-D point arrays.</param>
        /// <returns>The base OSC address, with possible changes to make it legal.</returns>
        public string Add2DPointProperty(ChildInfo childInfo, AttributePropertyDescriptor childAttributeDesc, string oscAddress)
        {
            oscAddress = OscServices.FixPropertyAddress(oscAddress);

            var xCoordDesc = new ChildListFloatingPointArrayDesc(childInfo, childAttributeDesc, 0);
            AddPropertyAddress(xCoordDesc, oscAddress + "/x");

            var yCoordDesc = new ChildListFloatingPointArrayDesc(childInfo, childAttributeDesc, 1);
            AddPropertyAddress(yCoordDesc, oscAddress + "/y");

            return oscAddress;
        }

        /// <summary>
        /// Transforms data, if necessary, before sending it to a connected OSC device</summary>
        /// <param name="data">The data taken from 'common', using 'info'</param>
        /// <param name="common">The object whose properties or data are being broadcast. This would have
        /// come from ObservableToCommon().</param>
        /// <param name="info">The OSC address info for this data</param>
        /// <returns>The transformed data, ready to be sent</returns>
        protected override object PrepareDataForSending(object data, object common, OscAddressInfo info)
        {
            // This is a Liine Lemur specific conversion of booleans to floats, even though OSC has a boolean type.
            if (data is bool)
                return (bool)data ? 1.0f : 0.0f;
            return base.PrepareDataForSending(data, common, info);
        }

        /// <summary>
        /// Gets a set of OSC addresses and associated data payloads to add to an update that will
        /// be sent due to changes in 'common'.</summary>
        /// <param name="common">The object whose properties or data are being broadcast. This would have
        /// come from ObservableToCommon().</param>
        /// <returns>Set of OSC addresses and associated data payloads</returns>
        /// <remarks>In Liine Lemur, this is a good opportunity to update the 'interface' value,
        /// which can switch GUI screens in Lemur.</remarks>
        protected override IEnumerable<Tuple<string, object>> GetCustomDataToSend(object common)
        {
            string interfaceName = GetLemurInterfaceName(common);
            if (interfaceName != null)
                yield return new Tuple<string, object>("/interface", interfaceName);
        }

        /// <summary>
        /// Gets a Lemur interface name that is appropriate for this 'common' object</summary>
        /// <param name="common">The object whose properties or data are being broadcast. This would have
        /// come from ObservableToCommon().</param>
        /// <returns>A Lemur interface name, or null</returns>
        protected virtual string GetLemurInterfaceName(object common)
        {
            return null;
        }

        /// <summary>
        /// Sends the pairs of OSC addresses and data objects to the current destination endpoints
        /// immediately, on the current thread</summary>
        /// <param name="addressesAndData">List of OSC address and data object pairs</param>
        /// <remarks>
        /// Liine Lemur specific problem? Bundles don't seem to always work. It's as if there's a limit
        /// to the number of OscMessages that can be contained in an OscBundle.</remarks>
        protected override void SendSynchronously(IList<Tuple<string, object>> addressesAndData)
        {
            // 32 is too high. To do: count bytes?! Ask Lemur about their limit?
            const int maxNumMessagesPerBundle = 16;

            int numRemaining = addressesAndData.Count;
            while (numRemaining > 0)
            {
                int numInPacket = numRemaining <= maxNumMessagesPerBundle
                                      ? numRemaining
                                      : maxNumMessagesPerBundle;
                SendPacket(addressesAndData, addressesAndData.Count - numRemaining, numInPacket);

                numRemaining -= numInPacket;
            }
        }

        private class ChildListFloatingPointArrayDesc : ChildAttributePropertyDescriptor
        {
            public ChildListFloatingPointArrayDesc(ChildInfo childInfo, AttributePropertyDescriptor childAttributeDesc, int coordinateIndex)
                : base(
                    childAttributeDesc.Name,
                    childAttributeDesc.AttributeInfo,
                    childInfo,
                    childAttributeDesc.Category,
                    childAttributeDesc.Description,
                    childAttributeDesc.IsReadOnly,
                    null, //editor
                    childAttributeDesc.Converter)
            {
                m_childInfo = childInfo;
                m_childAttributeDesc = childAttributeDesc;
                m_coordinateIndex = coordinateIndex;
            }

            public override object GetValue(object component)
            {
                DomNode domNode = component.As<DomNode>();
                if (domNode != null)
                {
                    IList<DomNode> children = domNode.GetChildList(m_childInfo);
                    float[] oneDimension = new float[children.Count];
                    for (int childIndex = 0; childIndex < children.Count; childIndex++)
                    {
                        DomNode childDomNode = children[childIndex];
                        float[] vectorArray = (float[])m_childAttributeDesc.GetValue(childDomNode);
                        oneDimension[childIndex] = vectorArray[m_coordinateIndex];
                    }
                    return oneDimension;
                }
                return null;
            }

            public override void SetValue(object component, object value)
            {
                DomNode domNode = component.As<DomNode>();
                if (domNode != null)
                {
                    IList<DomNode> children = domNode.GetChildList(m_childInfo);
                    float[] oneDimension = (float[])value;
                    for (int childIndex = 0; childIndex < children.Count; childIndex++)
                    {
                        DomNode childDomNode = children[childIndex];
                        float[] originalVectorArray = (float[])m_childAttributeDesc.GetValue(childDomNode);
                        if (childIndex >= oneDimension.Length)
                            break;
                        float[] newVectorArray = (float[])originalVectorArray.Clone();
                        newVectorArray[m_coordinateIndex] = oneDimension[childIndex];
                        m_childAttributeDesc.SetValue(childDomNode, newVectorArray);
                    }
                }
            }

            private readonly int m_coordinateIndex;
            private readonly AttributePropertyDescriptor m_childAttributeDesc;
            private readonly ChildInfo m_childInfo;
        }
    }
}
