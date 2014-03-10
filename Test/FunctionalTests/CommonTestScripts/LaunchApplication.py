#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import Test

print "Nothing to do really, just make sure something is initialized:"
Test.NotNull(atfCommands)

print Test.SUCCESS
