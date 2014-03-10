#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

grpCmds.CreationOptions = GroupingCommands.GroupCreationOptions.None
doc = atfDocService.OpenNewDocument(editor)
CircuitEditorUtil.SetGlobals(schemaLoader, Schema)
numElemsAtStart = circuitContainer.Elements.Count
numEdgesAtStart = circuitContainer.Wires.Count

#set up
print("Step 1: Create 3 And elements, and an edge that connects the first 2")
A1 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "A1"), 96, 96)
A2 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "A2"), 226, 96)
A3 = editingContext.Insert[Module](CircuitEditorUtil.CreateModuleNode("andType", "A3"), 380, 96)
hist.Begin("Connecting A1 & A2")
A1ToA2= editingContext.Connect(A1, A1.Type.Outputs[0], A2, A2.Type.Inputs[0], None)
hist.End()
print("Step 2: group A1 , A2 ")
G12 = CircuitEditorUtil.AddGroup([A1, A2], editingContext, grpCmds, "G12")

print("Step 3: Copy/paste G12 and A3 ")
editingContext.Selection.SetRange([G12,A3])
atfEdit.Copy()
atfEdit.Paste()
G12Copy= circuitContainer.Elements[circuitContainer.Elements.Count-2]
A3Copy= circuitContainer.Elements[circuitContainer.Elements.Count-1]
G12Copy.Name = 'GC12'
A3Copy.Name= 'AC3'

#excise
print("Step 4: promote group G12 and A3 to template library")
hist.Begin("promote to template Library")
templateCmds.PromoteToTemplateLibrary([G12, A3])
templatingContext = templateCmds.TemplatingContext
Test.Equal(2, templatingContext.RootFolder.Templates.Count, "Varify two template items are created in the root folder of the template library")
hist.End()

print("Step 5: make edges that connect from regular elements to referencing elements and vice versa")
hist.Begin("Making more edges")
edgesBefore = editingContext.CircuitContainer.Wires.Count
G12Ref = CircuitEditorUtil.GetElement(circuitContainer, 'G12')
A3Ref = CircuitEditorUtil.GetElement(circuitContainer, 'A3')
#edge from a regular group to a referencing element
G12Module = Adapters.Cast[Module](G12Copy.DomNode)
A3RefModule = Adapters.Cast[Module](A3Ref.DomNode)
editingContext.Connect(G12Module, G12Module.Type.Outputs[0], A3RefModule, A3RefModule.Type.Inputs[0], None)
#edge from a regular group to a referencing group
G12RefModule = Adapters.Cast[Module](G12Ref.DomNode)
editingContext.Connect(G12Module, G12Module.Type.Outputs[1], G12RefModule, G12RefModule.Type.Inputs[0], None)
#edge from a referencing group to a referencing element
editingContext.Connect(G12RefModule, G12RefModule.Type.Outputs[1], A3RefModule, A3RefModule.Type.Inputs[1], None)
#edge from a referencing group to a regular element
editingContext.Connect(G12RefModule, G12RefModule.Type.Outputs[0], A3, A3.Type.Inputs[1], None)
Test.Equal(edgesBefore + 4, editingContext.CircuitContainer.Wires.Count)
hist.End()

print("Step 6: grouping: referencing group + regular group")
print(editingContext.CircuitContainer)
G22 = CircuitEditorUtil.AddGroup([G12Module, G12RefModule], editingContext, grpCmds, "G22")
Test.Equal( 5, G22.Type.Inputs.Count, "Varify the nested group has 5 inputs")
Test.Equal( 4, G22.Type.Outputs.Count, "Varify the nested group has 4 outputs")

print("Step 7: grouping: referencing module + regular module")
A22 = CircuitEditorUtil.AddGroup([A3Copy , A3RefModule], editingContext, grpCmds, "A22")
Test.Equal( 4, A22.Type.Inputs.Count, "Varify the nested group has 4 inputs")
Test.Equal( 2, A22.Type.Outputs.Count, "Varify the nested group has 2 outputs")

print("Step 8: grouping the top 2 groups")
G33 = CircuitEditorUtil.AddGroup([G22.DomNode , A22.DomNode], editingContext, grpCmds, "G33")
Test.Equal( 7, G33.Type.Inputs.Count, "Varify the nested group has 7 inputs")
Test.Equal( 6, G33.Type.Outputs.Count, "Varify the nested group has 6 outputs")

numElemsAfterEditing = circuitContainer.Elements.Count
numEdgesAfterEditing  = circuitContainer.Wires.Count

print("step 9: undo all editings")
while (hist.CanUndo):
    hist.Undo()
Test.Equal(numElemsAtStart,  circuitContainer.Elements.Count, "Verify same number of elements as original graph")
Test.Equal(numEdgesAtStart,  circuitContainer.Wires.Count, "Verify same number of edges as original graph")

print("step 10: redo all editings")
while (hist.CanRedo):
    hist.Redo()
Test.Equal(numElemsAfterEditing,  circuitContainer.Elements.Count, "Verify same number of elements as original graph")
Test.Equal(numEdgesAfterEditing,  circuitContainer.Wires.Count, "Verify same number of edges as original graph")

print(Test.SUCCESS)
