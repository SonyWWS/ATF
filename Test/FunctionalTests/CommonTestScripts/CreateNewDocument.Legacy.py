#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

cntOg = Test.GetEnumerableCount(atfDocService.OpenDocuments)
doc = atfFile.OpenNewDocument(editor)
Test.Equal(cntOg + 1, Test.GetEnumerableCount(atfDocService.OpenDocuments), "Verify document count increased")
Test.NotNull(doc, "Verify new document created")
Test.NotNull(atfDocService.ActiveDocument, "Verify we have an active document")
Test.Equal(doc.PathName, atfDocService.ActiveDocument.PathName, "Verify new document is the active document")

print Test.SUCCESS
