#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

print "Nothing to do really, just make sure something is initialized:"
Test.NotNull(atfCommands)

print Test.SUCCESS
