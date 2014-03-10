#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

cntOg = Test.GetEnumerableCount(atfDocService.OpenDocuments)
if (cntOg == 0):
    cntExp = 1
else:
    cntExp = cntOg
    
doc = atfFile.OpenNewDocument(editor)
Test.Equal(cntExp, Test.GetEnumerableCount(atfDocService.OpenDocuments), "Verify document count (first pass)")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocService.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.PathName, atfDocService.ActiveDocument.PathName, "Verify new document is the active document")

#Run again, in case no document was opened at first
doc = atfFile.OpenNewDocument(editor)
Test.Equal(cntExp, Test.GetEnumerableCount(atfDocService.OpenDocuments), "Verify document count (second pass)")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocService.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.PathName, atfDocService.ActiveDocument.PathName, "Verify new document is the active document")

print Test.SUCCESS
