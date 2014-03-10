#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
import time
sys.path.append("./CommonTestScripts")
import Test

print "Sleeping for 5 minutes ..."
time.sleep(5 * 60)
print "5 minutes is up!"

# Should never get here (supposed to timeout in automation), 
# but just in case make sure to return/print success message
print Test.SUCCESS