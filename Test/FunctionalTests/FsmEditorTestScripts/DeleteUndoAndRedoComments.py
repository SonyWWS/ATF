#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

Test.Equal(0, fsm.Annotations.Count, "Verify new document has no comments")

commentsCnt = 10
comments = []
for i in range(commentsCnt):
    comments.append(editingContext.InsertComment(50 + 50*i, 50, "comment#" + str(i)))

Test.Equal(fsm.Annotations.Count, commentsCnt)

print "Start deleting"
for i in range(commentsCnt):
    Test.Equal(commentsCnt - i, fsm.Annotations.Count, "Verify comments count before delete")
    editingContext.Selection.SetRange([comments[i]])
    atfEdit.Delete()
    Test.Equal(commentsCnt - i - 1, fsm.Annotations.Count, "Verify comments count after delete");
    
print "Start undo testing"
for i in range(commentsCnt):
    Test.Equal(i, fsm.Annotations.Count, "Verify comments count before undo")
    hist.Undo()
    Test.Equal(i + 1, fsm.Annotations.Count, "Verify comments count after undo");

cnt = 0
for comment in fsm.Annotations:
    Test.Equal("comment#" + str(cnt), comment.Text  , "Verify name of comment is consistent after redo")
    cnt = cnt + 1

print "Start redo testing"
for i in range(commentsCnt, 0, -1):
    Test.Equal(i, fsm.Annotations.Count, "Verify comments count before redo")
    hist.Redo()
    Test.Equal(i - 1, fsm.Annotations.Count, "Verify comments count after redo");

print Test.SUCCESS
