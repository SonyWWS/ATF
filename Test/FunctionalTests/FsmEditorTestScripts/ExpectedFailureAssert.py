#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")

import Test

Test.Equal(1, 1)
print "Next line will fail"
Test.Equal(2, 3)

# Should never reach here:
print Test.SUCCESS
