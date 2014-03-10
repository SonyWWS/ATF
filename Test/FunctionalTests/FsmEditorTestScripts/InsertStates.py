#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")

import Test
import FsmUtil

doc = atfDocService.OpenNewDocument(editor)
anchorX = 20
anchorY = 20
spacing = 60

print "Start testing adding states ..."
FsmUtil.AddNewStateAndVerify(editingContext, anchorX, anchorY, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX, anchorY + spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX, anchorY + 2*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX, anchorY + 3*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX, anchorY + 4*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + spacing, anchorY + 2*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 2*spacing, anchorY, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 2*spacing, anchorY + spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 2*spacing, anchorY + 2*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 2*spacing, anchorY + 3*spacing, "h")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 2*spacing, anchorY + 4*spacing, "h")

FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 4*spacing, anchorY, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 5*spacing, anchorY, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 6*spacing, anchorY, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 5*spacing, anchorY + spacing, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 5*spacing, anchorY + 2*spacing, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 5*spacing, anchorY + 3*spacing, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 5*spacing, anchorY + 4*spacing, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 4*spacing, anchorY + 4*spacing, "i")
FsmUtil.AddNewStateAndVerify(editingContext, anchorX + 6*spacing, anchorY + 4*spacing, "i")

print Test.SUCCESS
