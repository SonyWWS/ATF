#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

doc = atfDocService.OpenNewDocument(editor)
CircuitEditorUtil.SetGlobals(schemaLoader, Schema)

modules = []
connections = []

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

logic = [btn2, btn3, btn4, btn5, and1, or1, or2]
layerLogic = layerLister.LayeringContext.InsertAuto(None, logic)
layerLogic.Name = "logic"

outputs = [speaker, light]
layerOutputs = layerLister.LayeringContext.InsertAuto(None, outputs)
layerOutputs.Name = "outputs"

print "Enable/disable layers and verify visibility"
for module in circuitContainer.Elements:
    Test.True(module.Visible, "Verifying all modules are visible at beginning: " + module.Name)

layerLister.ShowLayer(layerOutputs, False)
for module in outputs:
    Test.False(module.Visible, "Verify outputs are not visible after disabling outputs layer")

layerLister.ShowLayer(layerLogic, False)
for module in logic:
    Test.False(module.Visible, "Verify logic modules are not visible after disabling logic layer")
    
layerLister.ShowLayer(layerInputs, False)
for module in inputs:
    Test.False(module.Visible, "Verify inputs are not visible after disabling inputs layer")

print "Delete the layers"
Test.Equal(3, layerLister.LayeringContext.Layers.Count, "Verify layer count")

SelectionContexts.Set(layerLister.LayeringContext, layerOutputs)
layerLister.LayeringContext.Delete()
Test.Equal(2, layerLister.LayeringContext.Layers.Count, "Verify layer count after deleting a layer")
for module in outputs:
    Test.True(module.Visible, "Verify outputs are visible after deleting layer")

SelectionContexts.Set(layerLister.LayeringContext, layerLogic)
layerLister.LayeringContext.Delete()
Test.Equal(1, layerLister.LayeringContext.Layers.Count, "Verify layer count after deleting a layer")
for module in logic:
    Test.True(module.Visible, "Verify logic is visible after deleting layer")

SelectionContexts.Set(layerLister.LayeringContext, layerInputs)
layerLister.LayeringContext.Delete()
Test.Equal(0, layerLister.LayeringContext.Layers.Count, "Verify layer count after deleting a layer")
for module in inputs:
    Test.True(module.Visible, "Verify inputs are visible after deleting layer")    

for module in circuitContainer.Elements:
    Test.True(module.Visible, "Verifying all modules are visible after deleting all layers: " + module.Name)
    
print Test.SUCCESS
