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

startState = editingContext.Insert[StartState](DomNode(Schema.startStateType.Type), 100, 100, None)
Test.Equal(1, stateChart.States.Count, "Verify state count increased")

state1 = editingContext.Insert[State](DomNode(Schema.stateType.Type), 200, 100, None)
state1.Name = "State1"
Test.Equal(2, stateChart.States.Count, "Verify state count increased")

historyState = editingContext.Insert[HistoryState](DomNode(Schema.historyStateType.Type), 300, 100, None)
Test.Equal(3, stateChart.States.Count, "Verify state count increased")

conditionalState = editingContext.Insert[ConditionalState](DomNode(Schema.conditionalStateType.Type), 400, 100, None)
Test.Equal(4, stateChart.States.Count, "Verify state count increased")

state2 = editingContext.Insert[State](DomNode(Schema.stateType.Type), 500, 100, None)
state2.Name = "State2"
Test.Equal(5, stateChart.States.Count, "Verify state count increased")

finalState = editingContext.Insert[FinalState](DomNode(Schema.finalStateType.Type), 600, 100, None)
Test.Equal(6, stateChart.States.Count, "Verify state count increased")
Test.Equal(6, Test.GetEnumerableCount(stateChart.AllStates), "Verify AllStates count")
Test.Equal(6, Test.GetEnumerableCount(editingContext.Items), "Verify item count")

comment1 = editingContext.Insert[StatechartEditorSample.Annotation](DomNode(Schema.annotationType.Type), 700, 100, None)
comment1.Text = "<---- One of each"
Test.Equal(6, stateChart.States.Count, "Verify state count did not change when adding comment")
Test.Equal(7, Test.GetEnumerableCount(editingContext.Items), "Verify item count after adding comment")

stateParent = editingContext.Insert[State](DomNode(Schema.stateType.Type), 100, 200, None)
stateParent.Name = "Parent"
Test.Equal(7, stateChart.States.Count, "Verify state count increased")
Test.Equal(7, Test.GetEnumerableCount(stateChart.AllStates), "Verify AllStates count")
stateChild = editingContext.Insert[State](DomNode(Schema.stateType.Type), 100, 200, stateParent)
stateChild.Name = "Child"
Test.Equal(7, stateChart.States.Count, "Verify state count did not increase when adding to another state")
Test.Equal(8, Test.GetEnumerableCount(stateChart.AllStates), "Verify AllStates count increased when adding to another state")

comment2 = editingContext.Insert[StatechartEditorSample.Annotation](DomNode(Schema.annotationType.Type), 200, 200, None)
comment2.Text = "<---- State within a state"
Test.Equal(9, Test.GetEnumerableCount(editingContext.Items), "Verify item count")

print "Adding transitions"
startToState1 = editingContext.Connect(startState, BoundaryRoute(0.5), state1, BoundaryRoute(2.5), None)
Test.Equal(1, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

state1ToHistoryState = editingContext.Connect(state1, BoundaryRoute(0.5), historyState, BoundaryRoute(2.5), None)
Test.Equal(2, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

historyStateToConditionalState = editingContext.Connect(historyState, BoundaryRoute(0.5), conditionalState, BoundaryRoute(2.5), None)
Test.Equal(3, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

conditionalStateToState2 = editingContext.Connect(conditionalState, BoundaryRoute(0.5), state2, BoundaryRoute(2.5), None)
Test.Equal(4, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

state2ToFinalState = editingContext.Connect(state2, BoundaryRoute(0.5), finalState, BoundaryRoute(2.5), None)
Test.Equal(5, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

startStateToParentState = editingContext.Connect(startState, BoundaryRoute(1.5), stateParent, BoundaryRoute(3.5), None)
Test.Equal(6, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

state1ToChildState = editingContext.Connect(state1, BoundaryRoute(1.5), stateChild, BoundaryRoute(3.5), None)
Test.Equal(7, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

parentStateToConditionalState = editingContext.Connect(stateParent, BoundaryRoute(0.5), conditionalState, BoundaryRoute(1.5), None)
Test.Equal(8, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")

print Test.SUCCESS
