#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import System
import Test
import Sce.Atf.Dom
import Sce.Atf.Applications

def SetGlobals(schemaLoader, schema):
    global SchemaLoader
    global Schema
    SchemaLoader = schemaLoader
    Schema = schema
    return

def CreateModuleNode(nodeType, name):
    domNodeType = SchemaLoader.GetNodeType(Schema.NS + ":" + nodeType)
    domNode = Sce.Atf.Dom.DomNode(domNodeType)
    domNode.SetAttribute(Schema.moduleType.nameAttribute, nodeType)
    domNode.SetAttribute(Schema.moduleType.labelAttribute, name)
        
    return domNode

def GetElement(circuitContainer, name):
    for element in circuitContainer.Elements:
      if (element.Name == name):
         return element
    return nil

def VerifyCircuit(newCircuitContainer, modules, annotations, connections):
    Test.Equal(modules.Count, newCircuitContainer.Elements.Count, "Verify modules count")
    Test.Equal(annotations.Count, newCircuitContainer.Annotations.Count, "Verify annotations count")
    Test.Equal(connections.Count, newCircuitContainer.Wires.Count, "Verify connections count")
    
    for i in range(modules.Count):
        VerifyModule(modules[i], newCircuitContainer.Elements[i])
    for i in range(annotations.Count):
        VerifyAnnotation(annotations[i], newCircuitContainer.Annotations[i])
    for i in range(connections.Count):
        VerifyConnection(connections[i], newCircuitContainer.Wires[i])
    
    return

def VerifyModule(expected, actual):
    Test.Equal(expected.Id, actual.Id, "Verify module id")
    Test.Equal(expected.Name, actual.Name, "Verify module name")
    Test.Equal(expected.Position.X, actual.Position.X, "Verify module X pos")
    Test.Equal(expected.Position.Y, actual.Position.Y, "Verify module Y pos")
    Test.Equal(expected.Type.Name, actual.Type.Name, "Verify module type name")
    Test.Equal(expected.Type.Inputs.Count, actual.Type.Inputs.Count, "Verify inputs count")
    Test.Equal(expected.Type.Outputs.Count, actual.Type.Outputs.Count, "Verify outputs count")
    for i in range(expected.Type.Inputs.Count):
        VerifyPin(expected.Type.Inputs[i], actual.Type.Inputs[i])
    for i in range(expected.Type.Outputs.Count):
        VerifyPin(expected.Type.Outputs[i], actual.Type.Outputs[i])
    
    return

def VerifyAnnotation(expected, actual):
    Test.Equal(expected.Text, actual.Text, "Verify annotation text")
    Test.Equal(expected.Location.X, actual.Location.X, "Verify annotation X pos")
    Test.Equal(expected.Location.Y, actual.Location.Y, "Verify annotation Y pos")
    
    return
    
def VerifyConnection(expected, actual):
    Test.Equal(expected.Label, actual.Label, "Verify connection label")
    VerifyModule(expected.OutputElement, actual.OutputElement)
    VerifyModule(expected.InputElement, actual.InputElement)
    VerifyPin(expected.OutputPin, actual.OutputPin)
    VerifyPin(expected.InputPin, actual.InputPin)
    
    return
    
def VerifyPin(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify pin name")
    Test.Equal(expected.TypeName, actual.TypeName, "Verify pin TypeName")
    Test.Equal(expected.Index, actual.Index, "Verify pin Index")
    
    return

def AddGroup(items, editingContext, grpCmds, name):
    cntBefore = editingContext.CircuitContainer.Elements.Count
    editingContext.Selection.SetRange(items)
    grpCmds.DoCommand(Sce.Atf.Applications.StandardCommand.EditGroup)
    
    group = editingContext.CircuitContainer.Elements[cntBefore - items.Count]
    group.Name = name
    Test.Equal(group.Elements.Count, items.Count, "Verify group has correct number of items")
    
    return group
    
    return group
    