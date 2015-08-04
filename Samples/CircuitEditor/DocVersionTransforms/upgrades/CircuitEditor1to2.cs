//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace CircuitEditorSample
{
    /// <summary>
    /// Transforms XML document data from version 1 to 2. Version 2 is released with ATF3.10</summary>
    internal class CircuitEditor1to2 
    {
        readonly XNamespace ns = "http://sony.com/gametech/circuits/1_0"; // circuit xml document default namespace
        readonly XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public void Transform(XDocument doc)
        {
            ConvertSubCircuitToGroupTemeplate(doc);
            ConvertTemplateReferences(doc);

            // update xml document version
            XAttribute versionAttribute = doc.Root.Attribute("version");
            if (versionAttribute == null)
            {
                versionAttribute = new XAttribute("version", "2.0");
                doc.Root.Add(versionAttribute);
            }
            else
                versionAttribute.SetValue("2.0");
        }

        // Subcircuits were replaced by group templates in version 2.0
        void ConvertSubCircuitToGroupTemeplate(XDocument doc)
        {
    

            var templateFolder = doc.Root.Element(ns + "templateFolder");
            if (templateFolder == null)
            {
                templateFolder = new XElement(ns + "templateFolder", 
                                        new XAttribute("name", "_TemplateRoot_"));
                doc.Root.Add(templateFolder);
            }

            var subCircuitInstances = doc.Root.Elements(ns + "module").
                Where(x => x.Attribute(xsi + "type").Value == "subCircuitInstanceType");

            var subCircuitsToGroups = new Dictionary<string, XElement>();
 

            // for each SubCircuit, replace it with a group
            foreach (var subCircuit in doc.Descendants(ns + "subCircuit").ToArray()) // retrieve every SubCircuit within the xml
            {
                var groupElement = new XElement(ns + "module", 
                                        new XAttribute(xsi + "type", "groupType"),
                                        new XAttribute("name", subCircuit.Attribute("name").Value),
                                        new XAttribute("label", subCircuit.Attribute("name").Value),
                                        new XAttribute("validated", false) // raw group element is unvalidated 
                                       );

                foreach (var element in subCircuit.Descendants())
                    if (element.Name == ns + "module")
                        groupElement.Add(element);
                    else if (element.Name == ns + "connection")
                        groupElement.Add(element);
                    else if (element.Name == ns + "input")
                        // subcircuit is limited to 1 input pin, and only the type of the pin is persisted,
                        // no info about the input pin association with which internal node/pin. skip it, no practical usage
                        continue;
                    else if (element.Name == ns + "output")
                        // subcircuit is limited to 1 output pin, and only the type of the pin is persisted,
                        // no info about the output pin association with which internal node/pin. skip it, no practical usage
                        continue;

                // make a template to store the new group element for future references
                var template = new XElement(ns + "template",
                                      groupElement,
                                      new XAttribute("guid",  Guid.NewGuid().ToString()),
                                      new XAttribute("label", subCircuit.Attribute("name").Value));
                templateFolder.Add(template);

                subCircuitsToGroups.Add(subCircuit.Attribute("name").Value, template);
                subCircuit.Remove();

            }

            // update references to subcircuits
            foreach (var module in doc.Elements(ns + "module"))
            {
                foreach (var attibute in module.Attributes())
                    Console.WriteLine(attibute);
            }


  
            foreach (XElement instance in subCircuitInstances.ToArray())
            {
                XElement template = subCircuitsToGroups[instance.Attribute("type").Value];

                // The 'label' attribute does not always exist.
                XAttribute labelAttribute = instance.Attribute("label");
                string label = labelAttribute != null ? labelAttribute.Value : "";

                var groupReference = new XElement(ns + "module",
                                             new XAttribute(xsi + "type", "groupTemplateRefType"),
                                             new XAttribute("name", instance.Attribute("name").Value),
                                             new XAttribute("label", label),
                                             new XAttribute("x", instance.Attribute("x").Value),
                                             new XAttribute("y", instance.Attribute("y").Value),
                                             new XAttribute("guidRef", template.Attribute("guid").Value),
                                             new XAttribute("sourceGuid", template.Attribute("guid").Value)
                                        );
                instance.Remove();
                doc.Root.Add(groupReference);
            }
        }

        // from ATF3.7 to ATF3.8, template reference attribute name is changed from "typeRef" to guidRef"
        //     <xs:attribute name="typeRef" type="xs:IDREF"/> 
        //  => <xs:attribute name="guidRef" type="xs:IDREF"/> for group & module references
        void ConvertTemplateReferences(XDocument doc)
        {
            var groupReferences = doc.Descendants(ns + "module").
                Where(x => x.Attribute(xsi + "type").Value == "groupTemplateRefType");
            var moduleReferences = doc.Descendants(ns + "module").
                Where(x => x.Attribute(xsi + "type").Value == "moduleTemplateRefType");

            foreach (var groupReference in groupReferences)
            {
                var typeRefAttibute = groupReference.Attribute("typeRef");
                if (typeRefAttibute != null)
                {
                    var targetGroup = doc.Descendants(ns + "module").Where(x => x.Attribute("name").Value == typeRefAttibute.Value).Single();
                    typeRefAttibute.Remove();
                    var template = targetGroup.Parent;
                    var guid = template.Attribute("guid").Value;
                    groupReference.Add(new XAttribute("guidRef", guid));

                    // the GUID was also used as the id of the groupReference node, which is not a good idea ,
                    // since the circuit reader wants to resolve guid reference specially. 
                    // change the id and all the references to the id with prefix ""GroupReference_" 
                    foreach (var child in doc.Descendants())
                    {
                        foreach (var attribute in child.Attributes())
                        {
                            if (attribute.Name == "guid" || attribute.Name == "guidRef")
                                continue;
                            if (attribute.Value == guid)
                                attribute.Value = "GroupReference_" + guid;
                        }
                    }
                }
            }

            foreach (var moduleReference in moduleReferences)
            {
                var typeRefAttibute = moduleReference.Attribute("typeRef");
                if (typeRefAttibute != null)
                {
                    var targetModule = doc.Descendants(ns + "module").Where(x => x.Attribute("name").Value == typeRefAttibute.Value).Single();
                    typeRefAttibute.Remove();
                    var template = targetModule.Parent;
                    var guid = template.Attribute("guid").Value;
                    moduleReference.Add(new XAttribute("guidRef", guid));

                    // the GUID was also used as the id of the moduleReference node, which is not a good idea ,
                    // since the circuit reader wants to resolve guid reference specially. 
                    // change the id and all the references to the id with prefix ""ModuleReference_" 
                    foreach (var child in doc.Descendants())
                    {
                        foreach (var attribute in child.Attributes())
                        {
                            if (attribute.Name == "guid" || attribute.Name == "guidRef")
                                continue;
                            if (attribute.Value == guid)
                                attribute.Value = "ModuleReference_" + guid;
                        }
                    }
                }
            }
        }
     }
}
