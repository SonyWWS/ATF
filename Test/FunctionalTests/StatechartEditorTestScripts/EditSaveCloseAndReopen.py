#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import StatechartEditorUtil
import StatechartEditorSample

doc = atfDocService.OpenNewDocument(editor)

startState = editingContext.Insert[StartState](DomNode(Schema.startStateType.Type), 200, 100, None)
state1 = editingContext.Insert[State](DomNode(Schema.stateType.Type), 200, 200, None)
state1.Name = "State1"
historyState = editingContext.Insert[HistoryState](DomNode(Schema.historyStateType.Type), 200, 300, None)
conditionalState = editingContext.Insert[ConditionalState](DomNode(Schema.conditionalStateType.Type), 200, 400, None)
state2 = editingContext.Insert[State](DomNode(Schema.stateType.Type), 200, 500, None)
state2.Name = "State2"
state3 = editingContext.Insert[State](DomNode(Schema.stateType.Type), 200, 600, None)
state3.Name = "State3: !@#>$>%^&*()\\/?,.\'\"<<"
finalState = editingContext.Insert[FinalState](DomNode(Schema.finalStateType.Type), 200, 700, None)
comment1 = editingContext.Insert[StatechartEditorSample.Annotation](DomNode(Schema.annotationType.Type), 700, 100, None)
comment1.Text = "?>< I am a comment ?><!@#%$^(&*)){}\":"
stateParent = editingContext.Insert[State](DomNode(Schema.stateType.Type), 300, 500, None)
stateParent.Name = "Parent"
stateChild = editingContext.Insert[State](DomNode(Schema.stateType.Type), 300, 500, stateParent)
stateChild.Name = "Child"
comment2 = editingContext.Insert[StatechartEditorSample.Annotation](DomNode(Schema.annotationType.Type), 400, 500, None)
comment2.Text = "<---- State within a state"

print "Adding transitions"
startToState1 = editingContext.Connect(startState, BoundaryRoute(1.5), state1, BoundaryRoute(3.5), None)
state1ToHistoryState = editingContext.Connect(state1, BoundaryRoute(1.5), historyState, BoundaryRoute(3.5), None)
historyStateToConditionalState = editingContext.Connect(historyState, BoundaryRoute(1.5), conditionalState, BoundaryRoute(3.5), None)
conditionalStateToState2 = editingContext.Connect(conditionalState, BoundaryRoute(1.5), state2, BoundaryRoute(3.5), None)
state2ToState3 = editingContext.Connect(state2, BoundaryRoute(1.5), state3, BoundaryRoute(3.5), None)
state3ToFinalState = editingContext.Connect(state3, BoundaryRoute(1.5), finalState, BoundaryRoute(3.5), None)
state3ToState3 = editingContext.Connect(state3, BoundaryRoute(2.1), state3, BoundaryRoute(2.9), None)
startStateToParentState = editingContext.Connect(startState, BoundaryRoute(0.5), stateParent, BoundaryRoute(3.5), None)
state1ToChildState = editingContext.Connect(state1, BoundaryRoute(1.5), stateChild, BoundaryRoute(3.5), None)
parentStateToConditionalState = editingContext.Connect(stateParent, BoundaryRoute(0.5), conditionalState, BoundaryRoute(1.5), None)

Test.Equal(9, Test.GetEnumerableCount(stateChart.AllStates), "Verify state count")
Test.Equal(10, Test.GetEnumerableCount(editingContext.Edges), "Verify edge count")
Test.Equal(2, doc.Annotations.Count, "Verify comments count")

states = []
comments = []
edges = []

for state in stateChart.AllStates:
    states.append(state)
for comment in doc.Annotations:
    comments.append(comment)
for edge in editingContext.Edges:
    edges.append(edge)

print "Editing is done.  Now save, close, reopen, and verify all the data"
filePath = Test.GetNewFilePath("EditAndSave.statechart")

editor.Save(doc, Uri(filePath))
Test.True(File.Exists(filePath), "Verify file saved")

editor.Close(doc)
editingContext = None

editor.Open(Uri(filePath))

StatechartEditorUtil.VerifyStatechart(editingContext, states, comments, edges)

print Test.SUCCESS

