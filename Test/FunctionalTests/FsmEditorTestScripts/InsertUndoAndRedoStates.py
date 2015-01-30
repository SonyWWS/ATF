#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

Test.Equal(0, fsm.States.Count, "Verify new document has no states")

statesCnt = 10
for i in range(statesCnt):
    editingContext.InsertState(50, 50 + 50*i, "state#" + unicode(i), 64)

Test.Equal(fsm.States.Count, statesCnt)

print "Start undo testing"
for i in range(statesCnt, 0, -1):
    Test.Equal(i, fsm.States.Count, "Verify states count before undo")
    hist.Undo()
    Test.Equal(i - 1, fsm.States.Count, "Verify states count after undo");
    
    
print "Start redo testing"
for i in range(statesCnt):
    Test.Equal(i, fsm.States.Count, "Verify states count before redo")
    hist.Redo()
    Test.Equal(i + 1, fsm.States.Count, "Verify states count after redo");

cnt = 0
for state in fsm.States:
    Test.Equal("state#" + unicode(cnt), state.Name, "Verify name of state is consistent after redo")
    cnt = cnt + 1

print Test.SUCCESS
