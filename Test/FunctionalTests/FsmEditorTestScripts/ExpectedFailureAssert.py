#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")

import Test

Test.Equal(1, 1)
print "Next line will fail"
Test.Equal(2, 3)

# Should never reach here:
print Test.SUCCESS
