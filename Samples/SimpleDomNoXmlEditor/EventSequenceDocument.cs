//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Text;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// Represents EventSequences documents, including persisting them in a custom (non-XML) file format</summary>
    public class EventSequenceDocument : DomDocument
    {
        /// <summary>
        /// Constructor</summary>
        public EventSequenceDocument()
        {
        }

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return Editor.DocumentClientInfo.FileType; }
        }

        /// <summary>
        /// Raises the UriChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnUriChanged(UriChangedEventArgs e)
        {
            UpdateControlInfo();

            base.OnUriChanged(e);
        }

        /// <summary>
        /// Raises the DirtyChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnDirtyChanged(EventArgs e)
        {
            UpdateControlInfo();

            base.OnDirtyChanged(e);
        }

        /// <summary>
        /// Reads in the data for an EventSequenceDocument from the given stream</summary>
        /// <remarks>This method proves the concept that a document can be persisted in a custom
        /// file format that is not XML.</remarks>
        /// <param name="stream">Stream with event sequence data</param>
        /// <returns>A valid EventSequenceDocument if successful or null if the stream's data is invalid</returns>
        public static EventSequenceDocument Read(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                if (line != "eventSequence")
                    return null;

                DomNode root = new DomNode(DomTypes.eventSequenceType.Type, DomTypes.eventSequenceRootElement);
                bool readLineForEvent = true;
                while (true)
                {
                    // The root has a sequence of children that are eventType nodes.
                    if (readLineForEvent)
                        line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;
                    readLineForEvent = true;
                    DomNode eventNode;
                    if (!ReadEvent(line, out eventNode))
                        break;
                    root.GetChildList(DomTypes.eventSequenceType.eventChild).Add(eventNode);

                    // Each eventType node may have zero or more resourceType nodes.
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            break;
                        DomNode resourceNode;
                        if (!ReadResource(line, out resourceNode))
                        {
                            // might be a line for an event
                            readLineForEvent = false;
                            break;
                        }
                        eventNode.GetChildList(DomTypes.eventType.resourceChild).Add(resourceNode);
                    }
                }

                return root.Cast<EventSequenceDocument>();
            }
        }

        /// <summary>
        /// Writes the given document to the given stream</summary>
        /// <param name="document">EventSequenceDocument</param>
        /// <param name="stream">Stream to write to</param>
        public static void Write(EventSequenceDocument document, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine("eventSequence");

                DomNode root = document.DomNode;
                foreach (DomNode eventNode in root.GetChildren(DomTypes.eventSequenceType.eventChild))
                    WriteEvent(eventNode, writer);
            }
        }

        private static bool ReadEvent(string line, out DomNode eventNode)
        {
            if (!line.StartsWith("event"))
            {
                eventNode = null;
                return false;
            }

            eventNode = new DomNode(DomTypes.eventType.Type);
            FindAttribute(line, "name", eventNode);
            FindAttribute(line, "time", eventNode);
            FindAttribute(line, "duration", eventNode);

            return true;
        }

        private static void WriteEvent(DomNode eventNode, StreamWriter writer)
        {
            StringBuilder lineBuilder = new StringBuilder(
                "event name=\"" + eventNode.GetAttribute(DomTypes.eventType.nameAttribute) + "\"");
            WriteAttribute(eventNode, "time", lineBuilder);
            WriteAttribute(eventNode, "duration", lineBuilder);
            writer.WriteLine(lineBuilder.ToString());

            foreach (DomNode resourceNode in eventNode.GetChildren(DomTypes.eventType.resourceChild))
                WriteResource(resourceNode, writer);
        }

        private static bool ReadResource(string line, out DomNode resourceNode)
        {
            resourceNode = null;
            if (!line.StartsWith("resource"))
                return false;

            string type;
            if (!FindAttribute(line, "type", out type))
                return false;
            if (type == DomTypes.geometryResourceType.Type.Name)
            {
                resourceNode = new DomNode(DomTypes.geometryResourceType.Type);
                FindAttribute(line, "bones", resourceNode);
                FindAttribute(line, "vertices", resourceNode);
                FindAttribute(line, "primitiveType", resourceNode);
            }
            else if (type == DomTypes.animationResourceType.Type.Name)
            {
                resourceNode = new DomNode(DomTypes.animationResourceType.Type);
                FindAttribute(line, "tracks", resourceNode);
                FindAttribute(line, "duration", resourceNode);
            }
            else
                return false;

            FindAttribute(line, "name", resourceNode);
            FindAttribute(line, "size", resourceNode);
            FindAttribute(line, "compressed", resourceNode);

            return true;
        }

        private static void WriteResource(DomNode resourceNode, StreamWriter writer)
        {
            StringBuilder lineBuilder = new StringBuilder( string.Format(
                "resource type=\"{0}\", name=\"{1}\"",
                resourceNode.Type.Name,
                resourceNode.GetAttribute(DomTypes.resourceType.nameAttribute)));

            WriteAttribute(resourceNode, "size", lineBuilder);
            WriteAttribute(resourceNode, "compressed", lineBuilder);

            if (resourceNode.Type == DomTypes.geometryResourceType.Type)
            {
                WriteAttribute(resourceNode, "bones", lineBuilder);
                WriteAttribute(resourceNode, "vertices", lineBuilder);
                WriteAttribute(resourceNode, "primitiveType", lineBuilder);
            }
            else if (resourceNode.Type == DomTypes.animationResourceType.Type)
            {
                WriteAttribute(resourceNode, "tracks", lineBuilder);
                WriteAttribute(resourceNode, "duration", lineBuilder);
            }
            else
                throw new InvalidOperationException("unknown resource type");
            
            writer.WriteLine(lineBuilder.ToString());
        }

        // Finds an attribute of the form "{name}={value}" and sets the corresponding
        //  attribute on the given DomNode.
        private static void FindAttribute(string line, string name, DomNode node)
        {
            AttributeInfo attributeInfo = node.Type.GetAttributeInfo(name);
            if (attributeInfo.Type.ClrType == typeof(int))
            {
                int value;
                if (FindAttribute(line, name, out value))
                    node.SetAttribute(attributeInfo, value);
            }
            else if (attributeInfo.Type.ClrType == typeof(bool))
            {
                bool value;
                if (FindAttribute(line, name, out value))
                    node.SetAttribute(attributeInfo, value);
            }
            else
            {
                string value;
                if (FindAttribute(line, name, out value))
                    node.SetAttribute(attributeInfo, value);
            }
        }

        private static bool FindAttribute(string line, string name, out string value)
        {
            value = string.Empty;

            int start, end;
            start = line.IndexOf(name);
            if (start >= 0)
            {
                start = line.IndexOf('\"', start + name.Length);
                if (start < 0)
                    return false;
                end = line.IndexOf('\"', start + 1);
                if (end < 0)
                    return false;
                value = line.Substring(start + 1, end - start - 1);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool FindAttribute(string line, string name, out bool value)
        {
            value = false;

            int start;
            start = line.IndexOf(name);
            if (start >= 0)
            {
                start = line.IndexOf('=', start + name.Length);
                if (start < 0)
                    return false;
                line = line.Substring(start+1);
                if (line.StartsWith("true"))
                    value = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static char[] s_intTerminators = new char[] { ',' };
        // e.g., if line is "event name="zzzz", duration=1" and name is "duration",
        //  then value will be 1 and true will be returned.
        private static bool FindAttribute(string line, string name, out int value)
        {
            value = 0;

            int start, end;
            start = line.IndexOf(name);
            if (start >= 0)
            {
                start = line.IndexOf('=', start + name.Length);
                if (start < 0)
                    return false;
                start++;
                end = line.IndexOfAny(s_intTerminators, start + 1);
                if (end < 0)
                    end = line.Length;
                value = int.Parse(line.Substring(start, end - start));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Appends the attribute, if it's not the default value, in the format ", {name}={value}".
        /// If the value is an integer, then no quotes are used.
        /// If the value is a bool, then true or false are written out, without quotes.
        /// Otherwise, the value is enclosed in quotes.</summary>
        /// <param name="node">Node whose attributes is written</param>
        /// <param name="name">Attribute name</param>
        /// <param name="lineBuilder">String builder to create string attribute data</param>
        private static void WriteAttribute(DomNode node, string name, StringBuilder lineBuilder)
        {
            AttributeInfo attributeInfo = node.Type.GetAttributeInfo(name);
            if (!node.IsAttributeDefault(attributeInfo))
            {
                object attribute = node.GetAttribute(attributeInfo);
                if (attribute is int)
                    lineBuilder.AppendFormat(", {0}={1}", name, ((int)attribute).ToString());
                else if (attribute is bool)
                    lineBuilder.AppendFormat(", {0}={1}", name, (bool)attribute == true ? "true" : "false");
                else
                    lineBuilder.AppendFormat(", {0}=\"{1}\"", name, attribute);
            }
        }

        private void UpdateControlInfo()
        {
            string filePath = Uri.LocalPath;
            string fileName = Path.GetFileName(filePath);
            if (Dirty)
                fileName += "*";

            EventSequenceContext context = this.As<EventSequenceContext>();
            context.ControlInfo.Name = fileName;
            context.ControlInfo.Description = filePath;
        }
    }
}
