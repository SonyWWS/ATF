import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

CircuitEditorUtil.SetGlobals(schemaLoader, Schema)
doc = atfDocService.OpenNewDocument(editor)

#Verify no objects at the beginning
Test.Equal(0, Test.GetEnumerableCount(editingContext.Items), "verify no pre-existing objects")
Test.Equal(0, circuitContainer.Wires.Count, "verify no connections")

#add a button and verify
btn = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "benjamin button"), 50, 50)
Test.Equal(1, Test.GetEnumerableCount(editingContext.Items), "verify only one existing object")

#Cut button and verify
atfEdit.Cut()
Test.Equal(0, Test.GetEnumerableCount(editingContext.Items), "verify button has been cut")

#Paste button and verify
atfEdit.Paste()
Test.Equal(1, Test.GetEnumerableCount(editingContext.Items), "verify button is re-inserted")

#Reassign button name to button
btn = circuitContainer.Elements[0]

#Place button in original location
editingContext.SetProperty(btn.DomNode, Schema.moduleType.xAttribute, 50)
editingContext.SetProperty(btn.DomNode, Schema.moduleType.yAttribute, 50)

#add a light and verify
light = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "amarlight"), 200, 50)
Test.Equal(2, circuitContainer.Elements.Count, "verify 2 existing modules")

#Create a connection between the two and verify
btnToLight = editingContext.Connect(btn, btn.Type.Outputs[0], light, light.Type.Inputs[0], None)
editingContext.Selection.SetRange([btnToLight])
Test.Equal(1, circuitContainer.Wires.Count, "verify one (1) connection")

#Cut the connection and verify
atfEdit.Cut()
Test.Equal(0, circuitContainer.Wires.Count, "verify no connections")

#Paste, VERIFY NO CONNECTION
atfEdit.Paste()
Test.Equal(0, circuitContainer.Wires.Count, "verify no connection")

#Re-add connection, verify
btnToLight = editingContext.Connect(btn, btn.Type.Outputs[0], light, light.Type.Inputs[0], None)
Test.Equal(1, circuitContainer.Wires.Count, "verify connection")

#Cut two buttons and the connection between them, verify
atfSelect.SelectAll()
Test.Equal(1, circuitContainer.Wires.Count, "VERIFY THAT THIS IS 1")
atfEdit.Cut()
Test.Equal(0, circuitContainer.Elements.Count, "verify no remaining modules")
Test.Equal(0, circuitContainer.Wires.Count, "verify no remaining connections")

#Paste Circuit, verify
atfEdit.Paste()
Test.Equal(2, circuitContainer.Elements.Count, "verify modules re-instantiated")
Test.Equal(1, circuitContainer.Wires.Count, "verify connection re-instantiated")

#Add a comment
comment = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 300, 100)
editingContext.SetProperty(comment.DomNode, Schema.annotationType.textAttribute, "I am a comment")
Test.Equal(1, circuitContainer.Annotations.Count, "Verify annotation count after adding a comment")
Test.Equal(circuitContainer.Annotations[0].Text, "I am a comment", "Verify name")

#Cut comment and verify
atfEdit.Cut()
Test.Equal(0, circuitContainer.Annotations.Count, "Verify comment removed")

#Paste comment and verify
atfEdit.Paste()
Test.Equal(1, circuitContainer.Annotations.Count, "verify comment replaced")
Test.Equal(circuitContainer.Annotations[0].Text, "I am a comment", "Verify name")

print Test.SUCCESS