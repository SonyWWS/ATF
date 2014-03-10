#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

doc = atfDocService.OpenNewDocument(editor)
CircuitEditorUtil.SetGlobals(schemaLoader, Schema)

modules = []
annotations = []
connections = []

print "Adding annotations"
comment = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 300, 100)
editingContext.SetProperty(comment.DomNode, Schema.annotationType.textAttribute, "I am a comment")
comment2 = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 400, 100)
editingContext.SetProperty(comment2.DomNode, Schema.annotationType.textAttribute, "!@#$%^&*()_+<>/.,;[]\\")

print "Adding modules"
btn = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "benjamin button"), 100, 100)
light = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "lights out"), 200, 100)
sound = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("soundType", "like a lion in zion"), 100, 200)
speaker = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("speakerType", "speakeazy"), 200, 200)
btn2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn2"), 100, 300)
btn3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn3"), 100, 400)
andObj = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "andONE"), 200, 300)
orObj = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("orType", "orca"), 200, 400)
light2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "light2"), 300, 300)
light3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "light3"), 300, 400)

print "Adding connections"
btnToLight = editingContext.Connect(btn, btn.Type.Outputs[0], light, light.Type.Inputs[0], None)
soundToSpeaker = editingContext.Connect(sound, sound.Type.Outputs[0], speaker, speaker.Type.Inputs[0], None)
btn2ToAnd = editingContext.Connect(btn2, btn2.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
btn2ToOr = editingContext.Connect(btn2, btn2.Type.Outputs[0], orObj, orObj.Type.Inputs[0], None)
btn3ToAnd = editingContext.Connect(btn3, btn3.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
btn3ToOr = editingContext.Connect(btn3, btn3.Type.Outputs[0], orObj, orObj.Type.Inputs[0], None)
btn2ToAnd = editingContext.Connect(btn2, btn2.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
andToLight2 = editingContext.Connect(andObj, andObj.Type.Outputs[0], light2, light2.Type.Inputs[0], None)
orToLight3 = editingContext.Connect(orObj, orObj.Type.Outputs[0], light3, light3.Type.Inputs[0], None)

for annotation in circuitContainer.Annotations:
    annotations.append(annotation)
for module in circuitContainer.Elements:
    modules.append(module)
for connection in circuitContainer.Wires:
    connections.append(connection)

filePath = Test.GetNewFilePath("EditAndSave.circuit")
atfFile.SaveAs(doc,Uri(filePath) )
Test.True(File.Exists(filePath), "Verify file saved")
atfFile.Close(doc)

docNew = atfFile.OpenExistingDocument(editor, Uri(filePath))
CircuitEditorUtil.VerifyCircuit(circuitContainer, modules, annotations, connections)

print Test.SUCCESS
