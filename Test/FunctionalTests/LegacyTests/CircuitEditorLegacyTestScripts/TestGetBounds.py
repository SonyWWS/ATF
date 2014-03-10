#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.
import System.Xml
import Scea.Dom
import Test

#create new document
print editor
circuitDocument = atfFile.OpenNewDocument(editor)
circuitDocument.CircuitControl.Style.SnapToGrid = False

#create a button and a light
namespace = r'http://www.scea.com/Circuit/1_0'
buttonTypeName= System.Xml.XmlQualifiedName( 'buttonType', namespace)
buttonType =  DomSchemaRegistry.GetComplexType(buttonTypeName)
btnObject = DomObject(buttonType)

lightTypeName= System.Xml.XmlQualifiedName( 'lightType', namespace)
lightType =  DomSchemaRegistry.GetComplexType(lightTypeName)
lightObject = DomObject(lightType)
list = List[DomObject]()
list.AddRange([btnObject, lightObject])
editor.Insert(list)
Test.Equal(2, circuitDocument.Circuit.Elements.Count, "verify 2 elements inserted")

#place two elements apart from each other, note Element.Position only acccepts integers
circuitDocument.Circuit.Elements[0].Position = Point(96,  128)
circuitDocument.Circuit.Elements[1].Position = Point(192, 228)

bounds = circuitDocument.CircuitControl.GetBoundsF(circuitDocument.Circuit.Elements)
buttonBound  = circuitDocument.CircuitControl.GetBoundsF(circuitDocument.Circuit.Elements[0])
lightBound  = circuitDocument.CircuitControl.GetBoundsF(circuitDocument.Circuit.Elements[1])

# varify 
Test.True(bounds.Contains(buttonBound), "varify bounds enclose individual elements")
Test.True(bounds.Contains(lightBound), "varify bounds enclose individual elements")

print Test.SUCCESS