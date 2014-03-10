#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test

Test.Equal(0, Test.GetEnumerableCount(atfDocReg.Documents))

atfFile.OpenExistingDocument(viewer, Uri(System.IO.Path.GetFullPath("./tests/Resources/bike.atgi")))

Test.Equal(1, Test.GetEnumerableCount(atfDocReg.Documents))

print Test.SUCCESS
