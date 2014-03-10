import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

CircuitEditorUtil.SetGlobals(schemaLoader, Schema)
doc = atfDocService.OpenNewDocument(editor)

#Add a button and verify
btn1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "button 1"), 50, 75)
Test.Equal(1, circuitContainer.Elements.Count, "verify 1 button added")

#Copy/paste the button and verify
atfEdit.Copy()
atfEdit.Paste()
Test.Equal(2, circuitContainer.Elements.Count, "verify 1st button copy/pasted")

#Rename/replace/relabel new button
btn2 = circuitContainer.Elements[1]
editingContext.SetProperty(btn2.DomNode, Schema.moduleType.xAttribute, 65)
editingContext.SetProperty(btn2.DomNode, Schema.moduleType.yAttribute, 150)
editingContext.SetProperty(btn2.DomNode, Schema.moduleType.labelAttribute, "button 2")

#Add a third button and verify
btn3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "button 3"), 50, 215)
Test.Equal(3, circuitContainer.Elements.Count, "verify 3rd button")

#Copy/Paste button and verify
atfEdit.Copy()
atfEdit.Paste()
Test.Equal(4, circuitContainer.Elements.Count, "verify 3rd button copy/pasted")

#Rename/replace/relabel new button
btn4 = circuitContainer.Elements[3]
editingContext.SetProperty(btn4.DomNode, Schema.moduleType.xAttribute, 65)
editingContext.SetProperty(btn4.DomNode, Schema.moduleType.yAttribute, 300)
editingContext.SetProperty(btn4.DomNode, Schema.moduleType.labelAttribute, "button 4")

#Add an OR module and verify
orMod1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("orType", "or 1"), 150, 125)
Test.Equal(5, circuitContainer.Elements.Count, "verify OR module added")

#Copy/paste OR module, rename, replace, verify
atfEdit.Copy()
atfEdit.Paste()
orMod2 = circuitContainer.Elements[5]
editingContext.SetProperty(orMod2.DomNode, Schema.moduleType.labelAttribute, "or 2")
editingContext.SetProperty(orMod2.DomNode, Schema.moduleType.xAttribute, 160)
editingContext.SetProperty(orMod2.DomNode, Schema.moduleType.yAttribute, 215)
Test.Equal(6, circuitContainer.Elements.Count, "verify OR module copy/pasted")

#Add an AND module and verify
andMod = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "and"), 250, 170)
Test.Equal(7, circuitContainer.Elements.Count, "verify 7 modules")

#Add a light and verify
light = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("lightType", "amarlight"), 350, 170)
Test.Equal(8, circuitContainer.Elements.Count, "verify new light")

#Add connections (and verify)!
btn1Or1 = editingContext.Connect(btn1, btn1.Type.Outputs[0], orMod1, orMod1.Type.Inputs[0], None)
btn2Or1 = editingContext.Connect(btn2, btn2.Type.Outputs[0], orMod1, orMod1.Type.Inputs[1], None)
btn3Or2 = editingContext.Connect(btn3, btn3.Type.Outputs[0], orMod2, orMod2.Type.Inputs[0], None)
btn4Or2 = editingContext.Connect(btn4, btn4.Type.Outputs[0], orMod2, orMod2.Type.Inputs[1], None)
or1And = editingContext.Connect(orMod1, orMod1.Type.Outputs[0], andMod, andMod.Type.Inputs[0], None)
or2And = editingContext.Connect(orMod2, orMod2.Type.Outputs[0], andMod, andMod.Type.Inputs[1], None)
andLight = editingContext.Connect(andMod, andMod.Type.Outputs[0], light, light.Type.Inputs[0], None)
Test.Equal(7, circuitContainer.Wires.Count, "verify 7 new connections")

#Select all, copy
atfSelect.SelectAll()
atfEdit.Copy()

#Paste, verify
atfEdit.Paste()
Test.Equal(16, circuitContainer.Elements.Count, "verify 16 modules")
Test.Equal(14, circuitContainer.Wires.Count, "verify 14 connections")

print Test.SUCCESS