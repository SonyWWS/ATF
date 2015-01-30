#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

statesLeft = []
statesRight = []
transitions = []
trnCnt = 10

print "First create a bunch of states"
for i in range(trnCnt):
    statesLeft.append(editingContext.InsertState(100, 100 + 50*i, "Left#" + unicode(i), 64))
for i in range(trnCnt):
    statesRight.append(editingContext.InsertState(300, 100 + 50*i, "Right#" + unicode(i), 64))

print "Now add the transitions"
for i in range(trnCnt):
    transitions.append(FsmUtil.AddNewTransitionAndVerify(editingContext, statesLeft[i], statesRight[i]))
    transitions[i].Label = "Transition#" + unicode(i)

for i in range(trnCnt):
    Test.Equal("Transition#" + unicode(i), transitions[i].Label)

print Test.SUCCESS
