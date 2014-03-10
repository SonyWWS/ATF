#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import System
import Test

def VerifyStatechart(editingContext, allStates, comments, edges):
    Test.Equal(allStates.Count, Test.GetEnumerableCount(editingContext.Statechart.AllStates), "Verify all states count")
    Test.Equal(comments.Count, editingContext.Document.Annotations.Count, "Verify comments count")
    Test.Equal(edges.Count, editingContext.Edges.Count, "Verify edge count")
    cnt = 0
    for state in editingContext.Statechart.AllStates:
        #Assuming the order will remain the same...
        VerifyState(allStates[cnt], state)
        cnt = cnt + 1
    for i in range(comments.Count):
        VerifyComment(comments[i], editingContext.Document.Annotations[i])
    for i in range(edges.Count):
        VerifyEdge(edges[i], editingContext.Edges[i])

    return
    
def VerifyState(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify state name")
    Test.Equal(expected.Locked, actual.Locked, "Verify state locked")
    Test.Equal(expected.IsPseudoState, actual.IsPseudoState, "Verify state IsPseudoState")
    Test.Equal(expected.Position.X, actual.Position.X, "Verify state x pos")
    Test.Equal(expected.Position.Y, actual.Position.Y, "Verify state Y pos")
    Test.Equal(expected.Type, actual.Type, "Verify state type")
    Test.Equal(expected.Size.Height, actual.Size.Height, "Verify state size height")
    Test.Equal(expected.Size.Width, actual.Size.Width, "Verify state size width")
    Test.Equal(expected.Indicators, actual.Indicators, "Verify state indicators")
    #Test.Equal(expected.Parent, actual.Parent, "Verify statechart parent")
    
    #Not members of StateBase:
    #Test.Equal(expected.EntryAction, actual.EntryAction, "Verify state entry action")
    #Test.Equal(expected.ExitAction, actual.ExitAction, "Verify state exit action")
    #Test.Equal(expected.IsSimple, actual.IsSimple, "Verify state is simple")
    #Test.Equal(expected.Reactions.Count, actual.Reactions.Count, "Verify reaction count")
    #Test.Equal(expected.Statechart.Count, actual.Statechart.Count, "Verify statechart count")
    #Test.Equal(expected.SubStates.Count, actual.SubStates.Count, "Verify sub states count")
    #for i in range(expected.SubStates.Count):
    #    VerifyState(expected[i], actual[i])
    #Test.Equal(expected.Text, actual.Text, "Verify state text")
    
    return

def VerifyComment(expected, actual):
    Test.Equal(expected.Text, actual.Text, "Verify comment name")
    Test.Equal(expected.Location.X, actual.Location.X, "Verify comment x pos")
    Test.Equal(expected.Location.Y, actual.Location.Y, "Verify comment Y pos")
    return

def VerifyEdge(expected, actual):
    VerifyState(expected.FromState, actual.FromState)
    VerifyState(expected.ToState, actual.ToState)
    Test.Equal(expected.FromPosition, actual.FromPosition, "Verify edge from position")
    Test.Equal(expected.ToPosition, actual.ToPosition, "Verify edge to position")
    return


