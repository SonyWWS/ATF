#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import Test

Test.Equal(1, 1)
Test.Equal(2, 2)

#Intentionally commented, we want this script to fail
#print Test.SUCCESS