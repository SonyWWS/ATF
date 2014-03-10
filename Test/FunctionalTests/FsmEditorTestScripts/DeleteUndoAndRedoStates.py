#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

Test.Equal(0, fsm.States.Count, "Verify new document has no states")

statesCnt = 10
states = []
for i in range(statesCnt):
    states.append(editingContext.InsertState(50 + 50*i, 50, "state#" + str(i), 64))

Test.Equal(fsm.States.Count, statesCnt)

print "Start deleting"
for i in range(statesCnt):
    Test.Equal(statesCnt - i, fsm.States.Count, "Verify states count before delete")
    editingContext.Selection.SetRange([states[i]])
    atfEdit.Delete()
    Test.Equal(statesCnt - i - 1, fsm.States.Count, "Verify states count after delete");
    
print "Start undo testing"
for i in range(statesCnt):
    Test.Equal(i, fsm.States.Count, "Verify states count before undo")
    hist.Undo()
    Test.Equal(i + 1, fsm.States.Count, "Verify states count after undo");

cnt = 0
for state in fsm.States:
    Test.Equal("state#" + str(cnt), state.Name, "Verify name of state is consistent after redo")
    cnt = cnt + 1

print "Start redo testing"
for i in range(statesCnt, 0, -1):
    Test.Equal(i, fsm.States.Count, "Verify states count before redo")
    hist.Redo()
    Test.Equal(i - 1, fsm.States.Count, "Verify states count after redo");

print Test.SUCCESS
