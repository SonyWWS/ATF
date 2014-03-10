#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

#The DomTreeEditor only allows a single document open, so doc count should only
#increase if there is no document already open
cntOg = Test.GetEnumerableCount(atfDocReg.Documents)
if (cntOg == 0):
    cntExp = 1
else:
    cntExp = cntOg
    
doc = atfDocService.OpenNewDocument(editor)

Test.Equal(cntExp, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count (first pass)")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocReg.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.Uri.LocalPath, atfDocReg.ActiveDocument.Uri.LocalPath, "Verify new document is the active document")

#Run again, in case no document was opened at first    
doc = atfDocService.OpenNewDocument(editor)

Test.Equal(cntExp, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count (second pass)")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocReg.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.Uri.LocalPath, atfDocReg.ActiveDocument.Uri.LocalPath, "Verify new document is the active document")

print Test.SUCCESS
