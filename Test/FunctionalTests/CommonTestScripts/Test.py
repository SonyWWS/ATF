#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
import System
sys.path.append("./CommonTestScripts")

SUCCESS = "Success"

# ============================================================
# "Evaluate" functions, same idea as NUnit's Assert.*
# ============================================================
def Equal(val1, val2, msg = "Testing equality"):
    if (val1 == val2):
        print msg + ": " + unicode(val1) + "==" + unicode(val2)
    else:
            msg = "Error: " + msg + ": " + unicode(val1) + "!=" + unicode(val2)
            print msg
            raise Exception(msg)

def NotEqual(val1, val2, msg = "Testing inequality"):
    if (val1 != val2):
        print msg + ": " + unicode(val1) + "!=" + unicode(val2)
    else:
            msg = "Error: " + msg + ": " + unicode(val1) + "==" + unicode(val2)
            print msg
            raise Exception(msg)

def True(val, msg = "Testing true"):
    if (val):
        print msg + ": " + unicode(val) + " is true"
    else:
            msg = "Error: " + msg + ": " + unicode(val) + " should be true"
            print msg
            raise Exception(msg)

def False(val, msg = "Testing false"):
    if (not val):
        print msg + ": " + unicode(val) + " is false"
    else:
            msg = "Error: " + msg + ": " + unicode(val) + " should be false"
            print msg
            raise Exception(msg)

def NotNull(val, msg = "Testing not null"):
    #Python will throw a NameExcpetion if val is not defined, so no need to test the value
    print msg + ": " + unicode(val) + " is not null"

def GreaterThan(val1, val2, msg = "Testing greater than"):
    if (val1 > val2):
        print msg + ": " + unicode(val1) + " > " + unicode(val2)
    else:
        msg = "Error: " + msg + ": " + unicode(val1) + " <= " + unicode(val2)
        print msg
        raise Exception(msg)

#For comparing numbers that might not match exactly.
#(Try converting numbers to Decimal and comparing first, but there
#are some cases where we still need a fuzzy compare, such as if the numbers 
#are converted from radians to degrees)
def FuzzyCompare(val1, val2, msg = "Testing fuzzy equality", threshold = 0.001):
    if (System.Math.Abs(val1 - val2) <= threshold):
        print msg + ": " + unicode(val1) + "~=" + unicode(val2)
    else:
        msg = "Error in fuzzy compare: " + msg + ": " + unicode(val1) + "!~=" + unicode(val2)
        print msg
        raise Exception(msg)

# ============================================================
# Common file/path functions
# ============================================================
from System.IO import Path
from System.IO import Directory
from System.IO import File
from System import Environment

# Central function for defining a common output folder
def GetOutputDir():
    path = Path.Combine(Environment.CurrentDirectory, "tests/TestOutput")
    if (not Directory.Exists(path)):
        Directory.CreateDirectory(path) 
    return path

# Constructs a filepath based on the input name and standard folder.
# Also makes sure the file does not exist.
def GetNewFilePath(fileName, subDir="./"):
    path = Path.Combine(GetOutputDir(), subDir)
    if (not Directory.Exists(path)):
        Directory.CreateDirectory(path)
    path = Path.Combine(path, fileName)
    path = Path.GetFullPath(path)
    if (File.Exists(path)):
        File.Delete(path)    
    True(not File.Exists(path), "Verify file does not exist: " + path)
    return path

# ============================================================
# Miscellaneous functions
# ============================================================
def GetEnumerableCount(enumerable):
    cnt = 0
    for o in enumerable:
        cnt = cnt + 1

    return cnt

# Takes in a python array, and returns the same array as a C# array.
def ConstructArray(objects, type = System.Single):
    ret = System.Array.CreateInstance(type, objects.Count)
    for i in range(objects.Count):
        ret[i] = objects[i]
    
    return ret
