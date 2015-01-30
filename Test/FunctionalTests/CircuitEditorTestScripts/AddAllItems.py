#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System

import Test
import CircuitEditorUtil

doc = atfDocService.OpenNewDocument(editor)
CircuitEditorUtil.SetGlobals(schemaLoader, Schema)

Test.Equal(0, Test.GetEnumerableCount(editingContext.Items), "Verify no items at beginning")
Test.Equal(0, circuitContainer.Elements.Count, "Verify no modules at beginning")
Test.Equal(0, circuitContainer.Wires.Count, "Verify no connections at beginning")
Test.Equal(0, circuitContainer.Annotations.Count, "Verify no annotations at beginning")

btn = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "benjamin button"), 100, 100)
Test.Equal(1, circuitContainer.Elements.Count, "Verify module count after adding a button")
Test.Equal(circuitContainer.Elements[0].Name, "benjamin button", "Verify name")

light = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "lights out"), 200, 100)
Test.Equal(2, circuitContainer.Elements.Count, "Verify module count after adding a light")
Test.Equal(circuitContainer.Elements[1].Name, "lights out", "Verify name")

comment = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 300, 100)
editingContext.SetProperty(comment.DomNode, Schema.annotationType.textAttribute, "I am a comment")
Test.Equal(1, circuitContainer.Annotations.Count, "Verify annotation count after adding a comment")
Test.Equal(circuitContainer.Annotations[0].Text, "I am a comment", "Verify name")

comment2 = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 400, 100)
editingContext.SetProperty(comment2.DomNode, Schema.annotationType.textAttribute, "!@#$%^&*()_+<>/.,;[]\\")
Test.Equal(2, circuitContainer.Annotations.Count, "Verify annotation count after adding a comment")
Test.Equal(circuitContainer.Annotations[1].Text, "!@#$%^&*()_+<>/.,;[]\\", "Verify name")

sound = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("soundType", "like a lion in zion"), 100, 200)
Test.Equal(3, circuitContainer.Elements.Count, "Verify module count after adding a sound")
Test.Equal(circuitContainer.Elements[2].Name, "like a lion in zion", "Verify name")

speaker = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("speakerType", "speakeazy"), 200, 200)
Test.Equal(4, circuitContainer.Elements.Count, "Verify module count after adding a speaker")
Test.Equal(circuitContainer.Elements[3].Name, "speakeazy", "Verify name")

#add more buttons and lights for AND/OR
btn2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn2"), 100, 300)
Test.Equal(5, circuitContainer.Elements.Count, "Verify module count after adding another button")
Test.Equal(circuitContainer.Elements[4].Name, "btn2", "Verify name")

btn3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn3"), 100, 400)
Test.Equal(6, circuitContainer.Elements.Count, "Verify module count after adding another button")
Test.Equal(circuitContainer.Elements[5].Name, "btn3", "Verify name")

andObj = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "andONE"), 200, 300)
Test.Equal(7, circuitContainer.Elements.Count, "Verify module count after adding an AND")
Test.Equal(circuitContainer.Elements[6].Name, "andONE", "Verify name")

orObj = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("orType", "orca"), 200, 400)
Test.Equal(8, circuitContainer.Elements.Count, "Verify module count after adding an OR")
Test.Equal(circuitContainer.Elements[7].Name, "orca", "Verify name")

light2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "light2"), 300, 300)
Test.Equal(9, circuitContainer.Elements.Count, "Verify module count after adding another light")
Test.Equal(circuitContainer.Elements[8].Name, "light2", "Verify name")

light3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "light3"), 300, 400)
Test.Equal(10, circuitContainer.Elements.Count, "Verify module count after adding another light")
Test.Equal(circuitContainer.Elements[9].Name, "light3", "Verify name")

print "Adding connections"
btnToLight = editingContext.Connect(btn, btn.Type.Outputs[0], light, light.Type.Inputs[0], None)
Test.Equal(1, circuitContainer.Wires.Count, "Verify connection count after adding a connection")
btnToLight.Label = "button to light"

soundToSpeaker = editingContext.Connect(sound, sound.Type.Outputs[0], speaker, speaker.Type.Inputs[0], None)
Test.Equal(2, circuitContainer.Wires.Count, "Verify connection count after adding a connection")
soundToSpeaker.Label = "!@#$%^&*()_+-=[]\\\'\"/><,./"

btn2ToAnd = editingContext.Connect(btn2, btn2.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
Test.Equal(3, circuitContainer.Wires.Count, "Verify connection count after adding a connection")

btn2ToOr = editingContext.Connect(btn2, btn2.Type.Outputs[0], orObj, orObj.Type.Inputs[0], None)
Test.Equal(4, circuitContainer.Wires.Count, "Verify connection count after adding a connection")

btn3ToAnd = editingContext.Connect(btn3, btn3.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
Test.Equal(4, circuitContainer.Wires.Count, "Verify connection count after replacing a connection")

btn3ToOr = editingContext.Connect(btn3, btn3.Type.Outputs[0], orObj, orObj.Type.Inputs[0], None)
Test.Equal(4, circuitContainer.Wires.Count, "Verify connection count after replacing a connection")

btn2ToAnd = editingContext.Connect(btn2, btn2.Type.Outputs[0], andObj, andObj.Type.Inputs[0], None)
Test.Equal(4, circuitContainer.Wires.Count, "Verify connection count after replacing a connection")

andToLight2 = editingContext.Connect(andObj, andObj.Type.Outputs[0], light2, light2.Type.Inputs[0], None)
Test.Equal(5, circuitContainer.Wires.Count, "Verify connection count after adding a connection")

orToLight3 = editingContext.Connect(orObj, orObj.Type.Outputs[0], light3, light3.Type.Inputs[0], None)
Test.Equal(6, circuitContainer.Wires.Count, "Verify connection count after adding a connection")

totalItemCount = circuitContainer.Elements.Count + circuitContainer.Annotations.Count + circuitContainer.Wires.Count
Test.Equal(totalItemCount, Test.GetEnumerableCount(editingContext.Items), "Verify item count at end")

print Test.SUCCESS
