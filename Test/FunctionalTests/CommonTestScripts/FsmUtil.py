#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import Test

def AddNewStateAndVerify(editingContext, xPos, yPos, label, size=64):
	newState = editingContext.InsertState(xPos, yPos, label, size)
	Test.Equal(label, newState.Name, "Verify label")
	Test.Equal(xPos, newState.Position.X, "Verify x position")
	Test.Equal(yPos, newState.Position.Y, "Verify y position")
	return newState

def AddNewCommentAndVerify(editingContext, xPos, yPos, text):
    newComment = editingContext.InsertComment(xPos, yPos, text)
    Test.Equal(text, newComment.Text, "Verify text")
    #Difficult to verify the exact position because it is now the
    #center (previously was top left corner).  Needs a calculation
    #of the center of the comment based on the text length
    #Test.Equal(xPos, newComment.Location.X, "Verify x position")
    #Test.Equal(yPos, newComment.Location.Y, "Verify y position")
    return newComment

def AddNewTransitionAndVerify(editingContext, state1, state2):
    newTransition = editingContext.InsertTransition(state1, state2)
    Test.Equal(state1.Name, newTransition.FromState.Name, "Verify from state name")
    Test.Equal(state2.Name, newTransition.ToState.Name, "Verify to state name")
    return newTransition
