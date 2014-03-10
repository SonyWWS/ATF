#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)

Test.Equal(0, fsm.Annotations.Count, "Verify new document has no comments")

commentCnt = 10
for i in range(commentCnt):
    editingContext.InsertComment(50, 50 + 50*i, "comment#" + str(i))

Test.Equal(fsm.Annotations.Count, commentCnt)

print "Start undo testing"
for i in range(commentCnt, 0, -1):
    Test.Equal(i, fsm.Annotations.Count, "Verify comments count before undo")
    hist.Undo()
    Test.Equal(i - 1, fsm.Annotations.Count, "Verify comments count after undo");
    
    
print "Start redo testing"
for i in range(commentCnt):
    Test.Equal(i, fsm.Annotations.Count, "Verify comments count before redo")
    hist.Redo()
    Test.Equal(i + 1, fsm.Annotations.Count, "Verify comments count after redo");

cnt = 0
for comment in fsm.Annotations:
    Test.Equal("comment#" + str(cnt), comment.Text, "Verify name of comment is consistent after redo")
    cnt = cnt + 1

print Test.SUCCESS
