#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

print "Cut and paste one state"
state1 = FsmUtil.AddNewStateAndVerify(editingContext, 100, 100, "cutMe-state", 70)
Test.Equal(1, fsm.States.Count)
atfEdit.Cut()
Test.Equal(0, fsm.States.Count)
atfEdit.Paste()
Test.Equal(1, fsm.States.Count)
Test.Equal(state1.Name, fsm.States[0].Name, "Verify pasted state's name")
Test.Equal(state1.Size, fsm.States[0].Size, "Verify pasted state's size")
Test.NotEqual(state1.Position.X, fsm.States[0].Position.X, "Verify pasted state's X moved")
Test.NotEqual(state1.Position.Y, fsm.States[0].Position.Y, "Verify pasted state's Y moved")
state2 = FsmUtil.AddNewStateAndVerify(editingContext, 100, 100, "state2", 80)

print "Create a transition for later (can't be copied on its own)"
transition1 = FsmUtil.AddNewTransitionAndVerify(editingContext, fsm.States[0], fsm.States[1])
transition1.Label = "cutMe - transition"

print "Cut and paste one comment"
comment1 = FsmUtil.AddNewCommentAndVerify(editingContext, 100, 150, "cutMe-comment")
Test.Equal(1, fsm.Annotations.Count)
atfEdit.Cut()
Test.Equal(0, fsm.Annotations.Count)
atfEdit.Paste()
Test.Equal(1, fsm.Annotations.Count)
Test.Equal(comment1.Text, fsm.Annotations[0].Text, "Verify pasted comment's text")
Test.NotEqual(comment1.Location.X, fsm.Annotations[0].Location.X, "Verify pasted comment's X moved")
Test.NotEqual(comment1.Location.Y, fsm.Annotations[0].Location.Y, "Verify pasted comment's Y moved")

print "Cut and paste everything"
#note: should have 2 states, 1 transition, and 2 comments right now
Test.Equal(2, fsm.States.Count, "Verify states count before cutting everything")
Test.Equal(1, fsm.Annotations.Count, "Verify comments count before cutting everything")
Test.Equal(1, fsm.Transitions.Count, "Verify transitions count before cutting everything")
atfSelect.SelectAll()
atfEdit.Cut()
Test.Equal(0, fsm.States.Count, "Verify states count after cutting everything")
Test.Equal(0, fsm.Annotations.Count, "Verify comments count after cutting everything")
Test.Equal(0, fsm.Transitions.Count, "Verify transitions count after cutting everything")
atfEdit.Paste()
Test.Equal(2, fsm.States.Count, "Verify states count after pasting everything")
Test.Equal(1, fsm.Annotations.Count, "Verify comments count after pasting everything")
Test.Equal(1, fsm.Transitions.Count, "Verify transitions count after pasting everything")

print "Now verify the objects properties have not changed"
Test.Equal(state1.Name, fsm.States[0].Name, "Verify pasted state1's name")
Test.Equal(state1.Size, fsm.States[0].Size, "Verify pasted state1's size")
Test.Equal(state2.Name, fsm.States[1].Name, "Verify pasted state2's name")
Test.Equal(state2.Size, fsm.States[1].Size, "Verify pasted state2's size")
Test.Equal(comment1.Text, fsm.Annotations[0].Text, "Verify pasted comment's text")

Test.Equal(transition1.Label, fsm.Transitions[0].Label, "Verify pasted all transition")
Test.Equal(transition1.FromState.Name, fsm.Transitions[0].FromState.Name, "Verify pasted all FromState name")
Test.Equal(transition1.ToState.Name, fsm.Transitions[0].ToState.Name, "Verify pasted all ToState name")

print Test.SUCCESS