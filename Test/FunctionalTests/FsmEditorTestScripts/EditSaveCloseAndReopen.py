#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test
import FsmUtil
from System.IO import File

doc = atfDocService.OpenNewDocument(editor)

states = []
comments = []
transitions = []
print "Add a few states ..."
states.append(FsmUtil.AddNewStateAndVerify(editingContext, 50, 50, "state", 64))
states.append(FsmUtil.AddNewStateAndVerify(editingContext, 300, 100, "statey", 80))
states.append(FsmUtil.AddNewStateAndVerify(editingContext, 100, 200, "super troopers", 100))

print "Add a few comments ..."
comments.append(FsmUtil.AddNewCommentAndVerify(editingContext, 30, 150, "do the "))
comments.append(FsmUtil.AddNewCommentAndVerify(editingContext, 30, 175, "can"))
comments.append(FsmUtil.AddNewCommentAndVerify(editingContext, 30, 200, "can"))
comments.append(FsmUtil.AddNewCommentAndVerify(editingContext, 30, 225, "can"))

print "Add a few transitions ..."
transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, states[0], states[1]))
transitions[0].Label = "a -> b"
transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, states[1], states[2]))
transitions[1].Label = "b -> c"
transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, states[2], states[0]))
transitions[2].Label = "c -> a"
transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, states[0], states[2]))
transitions[3].Label = "a -> c"
transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, states[0], states[1]))
transitions[4].Label = "a -> b(2)"

filePath = Test.GetNewFilePath("EditAndSave.fsm")

editor.Save(doc, Uri(filePath))
Test.True(File.Exists(filePath), "Verify file saved")

editor.Close(doc)

editor.Open(Uri(filePath))

Test.Equal(states.Count, fsm.States.Count, "Verify states count matches")
Test.Equal(comments.Count, fsm.Annotations.Count, "Verify comments count matches")
Test.Equal(transitions.Count, fsm.Transitions.Count, "Verify transitions count matches")
for i in range(states.Count):
    print "Testing state#" + unicode(i)
    Test.Equal(states[i].Name, fsm.States[i].Name, "Verify name")
    Test.Equal(states[i].Position.X, fsm.States[i].Position.X, "Verify X")
    Test.Equal(states[i].Position.Y, fsm.States[i].Position.Y, "Verify Y")
    Test.Equal(states[i].Size, fsm.States[i].Size, "Verify size")

for i in range(comments.Count):
    print "Testing comment#" + unicode(i)
    Test.Equal(comments[i].Text, fsm.Annotations[i].Text, "Verify text")
    Test.Equal(comments[i].Location.X, fsm.Annotations[i].Location.X, "Verify X")
    Test.Equal(comments[i].Location.Y, fsm.Annotations[i].Location.Y, "Verify Y")

for i in range(transitions.Count):
    print "Testing transition#" + unicode(i)
    Test.Equal(transitions[i].Label, fsm.Transitions[i].Label, "Verify label")
    Test.Equal(transitions[i].FromState.Name, fsm.Transitions[i].FromState.Name, "Verify FromState name")
    Test.Equal(transitions[i].ToState.Name, fsm.Transitions[i].ToState.Name, "Verify ToState name")
    
print Test.SUCCESS
    