#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import Test

cntOg = Test.GetEnumerableCount(atfDocService.OpenDocuments)
editor.DoCommand(CodeEditor.Command.NewLua)
Test.Equal(cntOg + 1, Test.GetEnumerableCount(atfDocService.OpenDocuments), "Verify document count increased")
Test.NotNull(atfDocService.ActiveDocument, "Verify we have an active document")

print Test.SUCCESS