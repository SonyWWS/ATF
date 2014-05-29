//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// A class to support passing DOM node graphs between applications. This
    /// class has methods to serialize the node graph to and from byte arrays, and
    /// is Serializable itself, so can be used as the data in an IDataObject.</summary>
    public class DomNodeSerializer
    {
        // TODO
        //  Right now, DomNode attributes are serialized as strings; it would be more efficient
        //   if attributes were serialized as bytes.

        /// <summary>
        /// Constructs the stream from nodes</summary>
        /// <param name="nodes">Nodes to serialize; there should be no duplicates</param>
        /// <returns>Data stream constructed from nodes</returns>
        public byte[] Serialize(IEnumerable<DomNode> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            byte[] result = EmptyArray<byte>.Instance;

            // convert nodes to a list to avoid repeatedly enumerating them
            List<DomNode> nodeList = new List<DomNode>(nodes);

            // assign node ids, so we can serialize internal references (and efficiently detect
            //  external references)
            Dictionary<DomNode, int> nodeIds = new Dictionary<DomNode, int>();
            AssignNodeIds(nodeList, nodeIds);

            // serialize nodes as a stream of bytes
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // write top level node count
                    writer.Write(nodeList.Count);

                    foreach (DomNode node in nodeList)
                        Serialize(node, nodeIds, writer);

                    result = stream.ToArray();
                }
            }

            return result;
        }

        /// <summary>
        /// Deserialize DomNodes from data</summary>
        /// <param name="data">Data to deserialize</param>
        /// <param name="getNodeType">Method returning type of DOM node to obtain from data</param>
        /// <returns>Deserialized nodes</returns>
        public IEnumerable<DomNode> Deserialize(byte[] data, Func<string, DomNodeType> getNodeType)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (getNodeType == null)
                throw new ArgumentNullException("getNodeType");

            List<DomNode> nodeList = new List<DomNode>();
            List<Reference> references = new List<Reference>();

            // convert bytes back into Dom nodes
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // read top level nodes
                    int count = reader.ReadInt32();
                    nodeList.Capacity = count;

                    for (int i = 0; i < count; i++)
                    {
                        DomNode node = Deserialize(reader, getNodeType, references);
                        nodeList.Add(node);
                    }
                }
            }

            FixReferences(nodeList, references);

            return nodeList;
        }

        // assign integer ids to all top level nodes and their subtrees
        private static void AssignNodeIds(IEnumerable<DomNode> nodes, Dictionary<DomNode, int> nodeIds)
        {
            int count = 0;
            foreach (DomNode node in nodes)
            {
                foreach (DomNode descendant in node.Subtree)
                {
                    if (nodeIds.ContainsKey(descendant))
                        throw new InvalidOperationException("duplicate nodes in stream");

                    nodeIds.Add(descendant, count++);
                }
            }
        }

        private static void Serialize(DomNode node, Dictionary<DomNode, int> nodeIds, BinaryWriter writer)
        {
            writer.Write(node.Type.Name);

            foreach (AttributeInfo info in node.Type.Attributes)
            {
                object value = node.GetLocalAttribute(info);

                // references are serialized as an integer id
                if (info.Type.Type == AttributeTypes.Reference)
                {
                    DomNode reference = value as DomNode;
                    int refId;
                    if (reference == null ||
                        !nodeIds.TryGetValue(reference, out refId))
                    {
                        // null, or reference was external to top level nodes and their subtrees
                        writer.Write(false);
                    }
                    else
                    {
                        writer.Write(true);
                        writer.Write(refId);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        writer.Write(false);
                    }
                    else
                    {
                        writer.Write(true);
                        writer.Write(info.Type.Convert(value));
                    }
                }
            }

            foreach (ChildInfo info in node.Type.Children)
            {
                if (info.IsList)
                {
                    IList<DomNode> children = node.GetChildList(info);
                    writer.Write(children.Count);
                    foreach (DomNode child in children)
                        Serialize(child, nodeIds, writer);
                }
                else
                {
                    DomNode child = node.GetChild(info);
                    if (child == null)
                    {
                        writer.Write(false);
                    }
                    else
                    {
                        writer.Write(true);
                        Serialize(child, nodeIds, writer);
                    }
                }
            }
        }

        private static DomNode Deserialize(BinaryReader reader, Func<string, DomNodeType> getNodeType, List<Reference> references)
        {
            string typeName = reader.ReadString();
            DomNodeType type = getNodeType(typeName);
            if (type == null)
                throw new InvalidOperationException("unknown node type");

            DomNode node = new DomNode(type);

            foreach (AttributeInfo info in type.Attributes)
            {
                bool hasAttribute = reader.ReadBoolean();
                if (hasAttribute)
                {
                    // references are reconstituted after all nodes are read
                    if (info.Type.Type == AttributeTypes.Reference)
                    {
                        int refId = reader.ReadInt32();
                        references.Add(new Reference(node, info, refId));
                    }
                    else
                    {
                        string valueString = reader.ReadString();
                        object value = info.Type.Convert(valueString);
                        node.SetAttribute(info, value);
                    }
                }
            }

            foreach (ChildInfo info in type.Children)
            {
                if (info.IsList)
                {
                    int count = reader.ReadInt32();
                    IList<DomNode> childList = node.GetChildList(info);
                    for (int i = 0; i < count; i++)
                    {
                        DomNode child = Deserialize(reader, getNodeType, references);
                        childList.Add(child);
                    }
                }
                else
                {
                    bool hasChild = reader.ReadBoolean();
                    if (hasChild)
                    {
                        DomNode child = Deserialize(reader, getNodeType, references);
                        node.SetChild(info, child);
                    }
                }
            }

            return node;
        }

        private static void FixReferences(List<DomNode> nodeList, List<Reference> references)
        {
            // make the mapping from int to node
            Dictionary<int, DomNode> idToNode = new Dictionary<int, DomNode>();
            int count = 0;
            foreach (DomNode node in nodeList)
                foreach (DomNode descendant in node.Subtree)
                    idToNode.Add(count++, descendant);

            foreach (Reference reference in references)
            {
                DomNode refNode = idToNode[reference.RefId];
                reference.Node.SetAttribute(reference.Info, refNode);
            }
        }

        // enough information to reconstitute reference after all nodes are read
        private class Reference
        {
            public Reference(DomNode node, AttributeInfo info, int refId)
            {
                Node = node;
                Info = info;
                RefId = refId;
            }
            public readonly DomNode Node;
            public readonly AttributeInfo Info;
            public readonly int RefId;
        }
    }
}
