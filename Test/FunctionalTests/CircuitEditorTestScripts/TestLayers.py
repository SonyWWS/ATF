#Sony Computer Entertainment Confidential

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
comment = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 100, 50)
editingContext.SetProperty(comment.DomNode, Schema.annotationType.textAttribute, "inputs")
comment2 = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 300, 50)
editingContext.SetProperty(comment2.DomNode, Schema.annotationType.textAttribute, "logic")
comment3 = editingContext.Insert[Annotation](DomNode(Schema.annotationType.Type), 500, 50)
editingContext.SetProperty(comment3.DomNode, Schema.annotationType.textAttribute, "outputs")

print "Adding modules"
btn1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn1"), 100, 100)
btn2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn2"), 100, 200)
btn3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn3"), 100, 300)
btn4 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn4"), 100, 400)
btn5 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn5"), 100, 500)

sound = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("soundType", "sounds of silence"), 300, 100)
and1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "and1"), 200, 250)
or1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("orType", "or1"), 200, 350)
or2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("orType", "or2"), 300, 300)
and2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "and2"), 400, 200)
speaker = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("speakerType", "speakeazy"), 500, 200)
light = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "lights out"), 500, 300)

print "Adding connections"
btn1ToSound = editingContext.Connect(btn1, btn1.Type.Outputs[0], sound, sound.Type.Inputs[0], None)
soundToAnd2 = editingContext.Connect(sound, sound.Type.Outputs[0], and2, and2.Type.Inputs[0], None)
and2ToSpeaker = editingContext.Connect(and2, and2.Type.Outputs[0], speaker, speaker.Type.Inputs[0], None)
btn2ToAnd1 = editingContext.Connect(btn2, btn2.Type.Outputs[0], and1, and1.Type.Inputs[0], None)
btn3ToAnd1 = editingContext.Connect(btn3, btn3.Type.Outputs[0], and1, and1.Type.Inputs[1], None)
and1ToOr2 = editingContext.Connect(and1, and1.Type.Outputs[0], or2, or2.Type.Inputs[0], None)
btn4ToOr1 = editingContext.Connect(btn4, btn4.Type.Outputs[0], or1, or1.Type.Inputs[0], None)
btn5ToOr1 = editingContext.Connect(btn5, btn5.Type.Outputs[0], or1, or1.Type.Inputs[1], None)
or1ToOr2 = editingContext.Connect(or1, or1.Type.Outputs[0], or2, or2.Type.Inputs[1], None)
or2ToAnd2 = editingContext.Connect(or2, or2.Type.Outputs[0], and2, and2.Type.Inputs[1], None)
or2ToLight = editingContext.Connect(or2, or2.Type.Outputs[0], light, light.Type.Inputs[0], None)

print "Create layers"
Test.Equal(0, layerLister.LayeringContext.Layers.Count, "Verify no layers at the beginning")

inputs = [btn1, btn2, btn3, btn4, btn5]
layerInputs = layerLister.LayeringContext.InsertAuto(None, inputs)
layerInputs.Name = "inputs"
Test.Equal(1, layerLister.LayeringContext.Layers.Count, "Verify layer count")

logic = [btn2, btn3, btn4, btn5, and1, or1, or2]
layerLogic = layerLister.LayeringContext.InsertAuto(None, logic)
layerLogic.Name = "logic"
Test.Equal(2, layerLister.LayeringContext.Layers.Count, "Verify layer count")

outputs = [speaker, light]
layerOutputs = layerLister.LayeringContext.InsertAuto(None, outputs)
layerOutputs.Name = "outputs"
Test.Equal(3, layerLister.LayeringContext.Layers.Count, "Verify layer count")

layerAllModules = layerLister.LayeringContext.InsertAuto(None, circuitContainer.Elements)
layerAllModules.Name = "All modules"
Test.Equal(4, layerLister.LayeringContext.Layers.Count, "Verify layer count")

layerLister.LayeringContext.InsertAuto(None, circuitContainer.Annotations)
Test.Equal(4, layerLister.LayeringContext.Layers.Count, "Annotations should not be added in layers")
layerLister.LayeringContext.InsertAuto(None, comment)
Test.Equal(4, layerLister.LayeringContext.Layers.Count, "Annotations should not be added in layers")
layerLister.LayeringContext.InsertAuto(None, circuitContainer.Wires)
Test.Equal(4, layerLister.LayeringContext.Layers.Count, "Connections should not be added in layers")
layerLister.LayeringContext.InsertAuto(None, btn1ToSound)
Test.Equal(4, layerLister.LayeringContext.Layers.Count, "Connections should not be added in layers")

print "Verify layers"
Test.Equal(inputs.Count, layerInputs.ElementRefs.Count, "Verify count for inputs layer")
Test.Equal(logic.Count, layerLogic.ElementRefs.Count, "Verify count for logic layer")
Test.Equal(outputs.Count, layerOutputs.ElementRefs.Count, "Verify count for outputs layer")
Test.Equal(circuitContainer.Elements.Count, layerAllModules.ElementRefs.Count, "Verify count for all modules layer")

print "Add to an existing layer"
layerNew = layerLister.LayeringContext.InsertAuto(None, btn1)
layerNew.Name = "newbie"
Test.Equal(1, layerNew.ElementRefs.Count, "Verify count for new layer")
layerLister.LayeringContext.InsertAuto(layerNew, btn2)
Test.Equal(2, layerNew.ElementRefs.Count, "Verify count for after adding to existing layer")
layerLister.LayeringContext.InsertAuto(layerNew, btn3)
Test.Equal(3, layerNew.ElementRefs.Count, "Verify count for after adding to existing layer")
layerLister.LayeringContext.InsertAuto(layerNew, btn1)
Test.Equal(3, layerNew.ElementRefs.Count, "Re-adding same object should not be added")

print "Enable/disable layers and verify visibility"
for module in circuitContainer.Elements:
    Test.True(module.Visible, "Verifying all modules are visible at beginning: " + module.Name)

layerLister.ShowLayer(layerAllModules, False)
for module in circuitContainer.Elements:
    Test.False(module.Visible, "Verifying all modules are invisible: " + module.Name)
layerLister.ShowLayer(layerAllModules, True)
for module in circuitContainer.Elements:
    Test.True(module.Visible, "Verifying all modules are visible: " + module.Name)
    
layerLister.ShowLayer(layerOutputs, False)
for module in outputs:
    Test.False(module.Visible, "Verify outputs are not visible after disabling outputs layer")

layerLister.ShowLayer(layerLogic, False)
for module in logic:
    Test.False(module.Visible, "Verify logic modules are not visible after disabling logic layer")
    
layerLister.ShowLayer(layerInputs, True)
for module in inputs:
    Test.True(module.Visible, "Verify inputs are visible after enabling inputs layer")

#Not tested: layers within layers, deleting layers, copy/paste layers
    
print Test.SUCCESS
