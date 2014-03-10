#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

atfDocService.OpenNewDocument(editor)
anchorX = 20
anchorY = 20
spacing = 12

print "Start testing adding comments ..."
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY, "comment")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY + spacing, "c")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY + 2*spacing, "c")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY + 3*spacing, "c")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY + 4*spacing, "c")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX, anchorY + 5*spacing, "comment")

FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY, "ooooooo")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY + spacing, "o         o")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY + 2*spacing, "o         o")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY + 3*spacing, "o         o")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY + 4*spacing, "o         o")
FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 5*spacing, anchorY + 5*spacing, "ooooooo")

FsmUtil.AddNewCommentAndVerify(editingContext, anchorX + 10*spacing, anchorY, "mmm  you get the point ...")

print Test.SUCCESS
