#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

cntOg = Test.GetEnumerableCount(atfDocReg.Documents)

print "Create a new state chart"
doc = atfDocService.OpenNewDocument(stateChartEditor)
Test.Equal(cntOg + 1, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count increased")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocReg.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.Uri.LocalPath, atfDocReg.ActiveDocument.Uri.LocalPath, "Verify new document is the active document")

print "Create a new circuit"
doc = atfDocService.OpenNewDocument(circuitEditor)
Test.Equal(cntOg + 2, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count increased")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocReg.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.Uri.LocalPath, atfDocReg.ActiveDocument.Uri.LocalPath, "Verify new document is the active document")

print "Create a new fsm"
doc = atfDocService.OpenNewDocument(fsmEditor)
Test.Equal(cntOg + 3, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count increased")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocReg.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.Uri.LocalPath, atfDocReg.ActiveDocument.Uri.LocalPath, "Verify new document is the active document")

print Test.SUCCESS
