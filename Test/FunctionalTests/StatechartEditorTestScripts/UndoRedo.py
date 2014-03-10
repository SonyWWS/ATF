#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import StatechartEditorSample

doc = atfDocService.OpenNewDocument(editor)

Test.Equal(0, Test.GetEnumerableCount(editingContext.Items), "Verify no items at beginning")
Test.Equal(0, stateChart.States.Count, "Verify no states at beginning")
Test.Equal(0, Test.GetEnumerableCount(editingContext.Edges), "Verify no edges at beginning")

def addObject(type, xPos, yPos):
    if type == "StartState":
        temp = editingContext.Insert[StartState](DomNode(Schema.startStateType.Type), xPos, yPos, None)
        return temp
    elif type == "State":
        temp = editingContext.Insert[State](DomNode(Schema.stateType.Type), xPos, yPos, None)
        return temp
    elif type == "HistoryState":
        temp = editingContext.Insert[HistoryState](DomNode(Schema.historyStateType.Type), xPos, yPos, None)
        return temp
    elif type == "ConditionalState":
        temp = editingContext.Insert[ConditionalState](DomNode(Schema.conditionalStateType.Type), xPos, yPos, None)
        return temp
    elif type == "FinalState":
        temp = editingContext.Insert[FinalState](DomNode(Schema.finalStateType.Type), xPos, yPos, None)
        return temp
    else:
        raise Exception("Not a valid type")
        
def addAnnotation(xPos, yPos, text):
    temp = editingContext.Insert[StatechartEditorSample.Annotation](DomNode(Schema.annotationType.Type), xPos, yPos, None)
    temp.Text = text
    return temp
    
#def addConnection(obj1, obj2, loc1, loc2):
    
print "\n"

#Add a comment and verify
comment = addAnnotation(500, 100, "New comment, woohoo")
Test.Equal(1, Test.GetEnumerableCount(editingContext.Items), "verify comment added successfully")

#Undo comment and verify
hist.Undo()
Test.Equal(0, Test.GetEnumerableCount(editingContext.Items), "verify comment undone")

#Redo comment and verify
hist.Redo()
Test.Equal(1, Test.GetEnumerableCount(editingContext.Items), "verify comment redone")

#Add start state and verify
theStartState = addObject("StartState", 100, 100)
Test.Equal(1, stateChart.States.Count, "start state added")

#Undo and verify
hist.Undo()
Test.Equal(0, stateChart.States.Count, "start state undone")

#Redo and verify
hist.Redo()
Test.Equal(1, stateChart.States.Count, "start state redone")

#Add a state and verify
theState = addObject("State", 200, 100)
Test.Equal(2, stateChart.States.Count, "state added")

#Undo twice and verify
hist.Undo()
hist.Undo()
Test.Equal(0, stateChart.States.Count, "states undone")

#Redo twice and verify
hist.Redo()
hist.Redo()
Test.Equal(2, stateChart.States.Count, "states redone")

#Add a final state and verify
theFinalState = addObject("FinalState", 200, 200)
Test.Equal(3, stateChart.States.Count, "final state added")

#Undo thrice and verify
hist.Undo()
hist.Undo()
hist.Undo()
Test.Equal(0, stateChart.States.Count, "all states undone")

#Redo thrice and verify
hist.Redo()
hist.Redo()
hist.Redo()
Test.Equal(3, stateChart.States.Count, "all states redone")

#Add a transition from start to state and verify
startToState = editingContext.Connect(theStartState, BoundaryRoute(.5), theState, BoundaryRoute(2.5), None)
Test.Equal(1, Test.GetEnumerableCount(editingContext.Edges), "edge created")

#The default editingContext.Connect function does not execute in a transaction, so cannot undo/redo
#Undo and verify
#hist.Undo()
#Test.Equal(0, Test.GetEnumerableCount(editingContext.Edges), "edge undone")

#Redo and verify
#hist.Redo()
#Test.Equal(1, Test.GetEnumerableCount(editingContext.Edges), "edge redone")

#Add another transition from start to state
startToState2 = editingContext.Connect(theStartState, BoundaryRoute(1.5), theState, BoundaryRoute(2.5), None)
Test.Equal(2, Test.GetEnumerableCount(editingContext.Edges), "another edge added")

#add a transition from state to final and verify
stateToFinal = editingContext.Connect(theState, BoundaryRoute(1.5), theFinalState, BoundaryRoute(0), None)
Test.Equal(3, Test.GetEnumerableCount(editingContext.Edges), "another edge added")

print Test.SUCCESS