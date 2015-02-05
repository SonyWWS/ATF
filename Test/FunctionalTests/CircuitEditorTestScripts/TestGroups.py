#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil
import Sce.Atf.Applications

doc = atfDocService.OpenNewDocument(editor)
CircuitEditorUtil.SetGlobals(schemaLoader, Schema)
grpCmds.CreationOptions = GroupingCommands.GroupCreationOptions.None

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
btn6 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("buttonType", "btn6"), 100, 600)

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

print "Creating groups"
#TODO: undo/redo, moving a group, adding/removing transition
baselineCnt = editingContext.CircuitContainer.Elements.Count

#to-do: find a way to make all pins visible when groups are created. I commented out references to
#	Outputs and Inputs because those properties get the list of visible pins, rather than all pins. --Ron

#note: group's location will be the upper left of all the items.
#So get X coordinate of left most item, and Y coordinate of the highest item
xpos = btn1.Position.X
ypos = btn1.Position.Y
#Group with one item
grp1 = CircuitEditorUtil.AddGroup([btn1], editingContext, grpCmds, "grp1")
Test.Equal(xpos, grp1.Position.X)
Test.Equal(ypos, grp1.Position.Y)
#Connections are actually the internal connections that are contained within the group
Test.Equal(grp1.Wires.Count, 0)
#Test.Equal(grp1.Inputs.Count, 0)
#Test.Equal(grp1.Outputs.Count, 1)
Test.Equal(baselineCnt, editingContext.CircuitContainer.Elements.Count)

#Group with two items
xpos = btn2.Position.X
ypos = btn2.Position.Y
grp2 = CircuitEditorUtil.AddGroup([btn2, btn3], editingContext, grpCmds, "grp2")
Test.Equal(xpos, grp2.Position.X)
Test.Equal(ypos, grp2.Position.Y)
Test.Equal(grp2.Wires.Count, 0)
#Test.Equal(grp2.Inputs.Count, 0)
#Test.Equal(grp2.Outputs.Count, 2)
Test.Equal(baselineCnt - 1, editingContext.CircuitContainer.Elements.Count)

#Group with no connections
xpos = btn6.Position.X
ypos = btn6.Position.Y
grp3 = CircuitEditorUtil.AddGroup([btn6], editingContext, grpCmds, "grp3")
Test.Equal(xpos, grp3.Position.X)
Test.Equal(ypos, grp3.Position.Y)
Test.Equal(grp3.Wires.Count, 0)
#Test.Equal(grp3.Inputs.Count, 0)
#Test.Equal(grp3.Outputs.Count, 1)
Test.Equal(baselineCnt - 1, editingContext.CircuitContainer.Elements.Count)

#Group with different types of objects
#save locations for when they are ungrouped
locations = [[sound.Position.X, sound.Position.Y], [and1.Position.X, and1.Position.Y], [and2.Position.X, and2.Position.Y], [or1.Position.X, or1.Position.Y], [or2.Position.X, or2.Position.Y]]
grp4 = CircuitEditorUtil.AddGroup([sound, and1, and2, or1, or2], editingContext, grpCmds, "grp4")
Test.Equal(locations[1][0], grp4.Position.X)
Test.Equal(locations[0][1], grp4.Position.Y)
Test.Equal(grp4.Wires.Count, 4)
#Test.Equal(grp4.Inputs.Count, 7)
#Test.Equal(grp4.Outputs.Count, 5)
Test.Equal(baselineCnt - 5, editingContext.CircuitContainer.Elements.Count)
#Group of groups
grp5 = CircuitEditorUtil.AddGroup([grp1, grp2], editingContext, grpCmds, "grp5")
Test.Equal(grp5.Wires.Count, 0)
#Test.Equal(grp5.Inputs.Count, 0)
#Test.Equal(grp5.Outputs.Count, 3)
Test.Equal(baselineCnt - 6, editingContext.CircuitContainer.Elements.Count)

#undo grouping
cntBefore = editingContext.CircuitContainer.Elements.Count
atfHistory.DoCommand(StandardCommand.EditUndo)
print cntBefore, editingContext.CircuitContainer.Elements.Count
Test.Equal(cntBefore + 1, editingContext.CircuitContainer.Elements.Count)

print("Ungrouping a couple groups")
baselineCnt = editingContext.CircuitContainer.Elements.Count
editingContext.Selection.SetRange([grp4])
grpCmds.DoCommand(Sce.Atf.Applications.StandardCommand.EditUngroup)
Test.Equal(baselineCnt + 4, editingContext.CircuitContainer.Elements.Count)
#after ungroup, the items should go back to their original locations
Test.Equal(locations[0][0], sound.Position.X)
Test.Equal(locations[0][1], sound.Position.Y)
Test.Equal(locations[1][0], and1.Position.X)
Test.Equal(locations[1][1], and1.Position.Y)
Test.Equal(locations[2][0], and2.Position.X)
Test.Equal(locations[2][1], and2.Position.Y)
Test.Equal(locations[3][0], or1.Position.X)
Test.Equal(locations[3][1], or1.Position.Y)
Test.Equal(locations[4][0], or2.Position.X)
Test.Equal(locations[4][1], or2.Position.Y)

editingContext.Selection.SetRange([grp5])
grpCmds.DoCommand(Sce.Atf.Applications.StandardCommand.EditUngroup)
Test.Equal(baselineCnt + 4, editingContext.CircuitContainer.Elements.Count)

print("Deleting groups")
editingContext.Selection.SetRange([grp3])
atfEdit.Delete()
Test.Equal(baselineCnt + 3, editingContext.CircuitContainer.Elements.Count)

editingContext.Selection.SetRange([grp1])
atfEdit.Delete()
Test.Equal(baselineCnt + 2, editingContext.CircuitContainer.Elements.Count)

    
print Test.SUCCESS
