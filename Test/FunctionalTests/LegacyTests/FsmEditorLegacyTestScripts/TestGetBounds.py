#Sony Computer Entertainment Confidential
import System.Xml
import Scea.Dom
import Test

#create new document
print editor
fsmDocument = atfFile.OpenNewDocument(editor)
fsmDocument.CircuitControl.Style.SnapToGrid = False

#create 2 states
namespace = r'http://www.scea.com/FSM/1_0'
stateTypeName= System.Xml.XmlQualifiedName( 'stateType', namespace)
stateType =  DomSchemaRegistry.GetComplexType(stateTypeName)
state1 = DomObject(stateType)
state2 = DomObject(stateType)

list = List[DomObject]()
list.AddRange([state1, state2])
editor.Insert(list)
Test.Equal(2, fsmDocument.Circuit.Elements.Count, "verify 2 elements inserted")

#place two elements apart from each other, note Element.Position only acccepts integers
fsmDocument.Circuit.Elements[0].Position = Point(96,  128)
fsmDocument.Circuit.Elements[1].Position = Point(192, 228)

bounds = fsmDocument.CircuitControl.GetBoundsF(fsmDocument.Circuit.Elements)
bound1  = fsmDocument.CircuitControl.GetBoundsF(fsmDocument.Circuit.Elements[0])
bound2  = fsmDocument.CircuitControl.GetBoundsF(fsmDocument.Circuit.Elements[1])

# varify bounds
Test.True(bounds.Contains(bound1), "varify bounds enclose individual elements")
Test.True(bounds.Contains(bound2), "varify bounds enclose individual elements")

print Test.SUCCESS