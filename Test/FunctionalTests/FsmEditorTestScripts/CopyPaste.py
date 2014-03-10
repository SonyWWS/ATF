#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

print "Copy and paste one state"
state1 = FsmUtil.AddNewStateAndVerify(editingContext, 100, 100, "copyMe-state", 70)
atfEdit.Copy()
Test.Equal(1, fsm.States.Count)
atfEdit.Paste()
Test.Equal(2, fsm.States.Count)
state2 = fsm.States[1]
Test.Equal(state1.Name, state2.Name, "Verify copied state's name")
Test.Equal(state1.Size, state2.Size, "Verify copied state's size")
state2.Name = "pasted-state"
state2.Size = 100
print "Create a transition for later (can't be copied on its own)"
transition1 = FsmUtil.AddNewTransitionAndVerify(editingContext, state1, state2)
transition1.Label = "copyMe - transition"

print "Copy and paste one comment"
comment1 = FsmUtil.AddNewCommentAndVerify(editingContext, 100, 150, "copyMe-comment")
atfEdit.Copy()
Test.Equal(1, fsm.Annotations.Count)
atfEdit.Paste()
Test.Equal(2, fsm.Annotations.Count)
comment2 = fsm.Annotations[1]
Test.Equal(comment1.Text, comment2.Text, "Verify copied comment's text")
comment2.Text = "pasted-state"

print "Copy and paste everything"
#note: should have 2 states, 1 transition, and 2 comments right now
atfSelect.SelectAll()
atfEdit.Copy()
atfEdit.Paste()
Test.Equal(4, fsm.States.Count, "Verify states count after pasting everything")
Test.Equal(4, fsm.Annotations.Count, "Verify comments count after pasting everything")
Test.Equal(2, fsm.Transitions.Count, "Verify transitions count after pasting everything")

#assuming that the original objects are array indices 0/1, and the pasted objects will be 2/3
for i in range(2):
    Test.Equal(fsm.States[i].Name, fsm.States[i+2].Name, "Verify pasted all state name")
    Test.Equal(fsm.States[i].Size, fsm.States[i+2].Size, "Verify pasted all state size")
    Test.Equal(fsm.Annotations[i].Text, fsm.Annotations[i+2].Text, "Verify pasted all comment text")

Test.Equal(fsm.Transitions[0].Label, fsm.Transitions[1].Label, "Verify pasted all transition")
#This isn't that good of a test, since the name could be the original or the pasted
Test.Equal(fsm.Transitions[0].FromState.Name, fsm.Transitions[1].FromState.Name, "Verify pasted all FromState name")
Test.Equal(fsm.Transitions[0].ToState.Name, fsm.Transitions[1].ToState.Name, "Verify pasted all ToState name")

print Test.SUCCESS