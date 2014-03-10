#Sony Computer Entertainment Confidential

import System
import Test
import Sce.Atf.Dom
import LevelEditorSample
from System import Environment
from System.IO import Path
from System.IO import File

def SetSchema(schema):
    global Schema
    Schema = schema
    return

def AddObjectAndVerify(editingContext, domNodeType, vector):
    domNode = Sce.Atf.Dom.DomNode(domNodeType)
    gameObject = editingContext.Insert(domNode, vector[0], vector[1], vector[2])
    
    Test.Equal(gameObject.DomNode.Type.Name, domNodeType.Name, "Verify the correct type was added")
    
    Test.Equal(System.Decimal(vector[0]), System.Decimal(gameObject.Translation[0]), "Verify new object X pos")
    Test.Equal(System.Decimal(vector[1]), System.Decimal(gameObject.Translation[1]), "Verify new object Y pos")
    Test.Equal(System.Decimal(vector[2]), System.Decimal(gameObject.Translation[2]), "Verify new object Z pos")    
    
    #More generic (and cluttered) way if domNode is returned from insertion
    #Test.Equal(domNode.GetAttribute(domNode.Type.GetAttributeInfo("name")), name, "Verify new object name")
    #Test.EqualSystem.(Decimal(domNode.GetAttribute(domNode.Type.GetAttributeInfo("translate"))[0]), System.Decimal(x), "Verify new object X pos")
    #...

    return gameObject
    
def AddObjectSetPropertiesAndVerify(editingContext, domNodeType, vTranslation, vScale, vRotation, vRotatePivot):
    domNode = Sce.Atf.Dom.DomNode(domNodeType)
    gameObject = editingContext.Insert(domNode, vTranslation[0], vTranslation[1], vTranslation[2])
    editingContext.SetProperty(gameObject.DomNode, Schema.gameObjectType.scaleAttribute, Test.ConstructArray([vScale[0], vScale[1], vScale[2]]))
    editingContext.SetProperty(gameObject.DomNode, Schema.gameObjectType.rotatePivotAttribute, Test.ConstructArray([vRotatePivot[0], vRotatePivot[1], vRotatePivot[2]]))
    editingContext.SetProperty(gameObject.DomNode, Schema.gameObjectType.rotateAttribute, Test.ConstructArray([vRotation[0], vRotation[1], vRotation[2]]))
    
    Test.Equal(gameObject.DomNode.Type.Name, domNodeType.Name, "Verify the correct type was added")

    Test.Equal(System.Decimal(vTranslation[0]), System.Decimal(gameObject.Translation[0]), "Verify new object X pos")
    Test.Equal(System.Decimal(vTranslation[1]), System.Decimal(gameObject.Translation[1]), "Verify new object Y pos")
    Test.Equal(System.Decimal(vTranslation[2]), System.Decimal(gameObject.Translation[2]), "Verify new object Z pos")
    
    Test.Equal(System.Decimal(vScale[0]), System.Decimal(gameObject.Scale[0]), "Verify new object X scale")
    Test.Equal(System.Decimal(vScale[1]), System.Decimal(gameObject.Scale[1]), "Verify new object Y scale")
    Test.Equal(System.Decimal(vScale[2]), System.Decimal(gameObject.Scale[2]), "Verify new object Z scale")
    
    VerifyAttributeAngle(gameObject.DomNode, Schema.gameObjectType.rotateAttribute, vRotation)
    
    Test.Equal(System.Decimal(vRotatePivot[0]), System.Decimal(gameObject.RotatePivot[0]), "Verify new object X rotate pivot")
    Test.Equal(System.Decimal(vRotatePivot[1]), System.Decimal(gameObject.RotatePivot[1]), "Verify new object Y rotate pivot")
    Test.Equal(System.Decimal(vRotatePivot[2]), System.Decimal(gameObject.RotatePivot[2]), "Verify new object Z rotate pivot")
    
    return gameObject
        
def VerifyAttribute(domNode, attr, expected):
    Test.Equal(expected, domNode.GetAttribute(attr), "Verify attribute")
    return

def VerifyAttributeVector(domNode, attr, vector):
    Test.Equal(System.Decimal(vector[0]), System.Decimal(domNode.GetAttribute(attr)[0]), "Verify X axis")
    Test.Equal(System.Decimal(vector[1]), System.Decimal(domNode.GetAttribute(attr)[1]), "Verify Y axis")
    Test.Equal(System.Decimal(vector[2]), System.Decimal(domNode.GetAttribute(attr)[2]), "Verify Z axis")

    return

#internally, angles are stored as radians, but the api/ui displays degrees
#need to convert the internal radian to degrees, and use a fuzzy compare
#to account for floating point precision differences
def VerifyAttributeAngle(domNode, attr, vector):
    Test.FuzzyCompare(System.Decimal(vector[0]), System.Decimal(domNode.GetAttribute(attr)[0]), "Verify X angle")
    Test.FuzzyCompare(System.Decimal(vector[1]), System.Decimal(domNode.GetAttribute(attr)[1]), "Verify Y angle")
    Test.FuzzyCompare(System.Decimal(vector[2]), System.Decimal(domNode.GetAttribute(attr)[2]), "Verify Z angle")

    return
    
#This function constructs the full path to a resource under the Data folder.
def GetResourceFilePath(relativeFilePath):
    #current directory is something like:
    #LevelEditor\bin\Debug.vs2010
    #Data directory is:
    #LevelEditor\Data
    
    #so the relative path to the data directory is:
    dataDir = Path.Combine(Environment.CurrentDirectory, "../../Data")
    resourcePath = Path.Combine(dataDir, relativeFilePath)
    resourcePath = Path.GetFullPath(resourcePath)
    
    return resourcePath

#Helper to convert an array of domNodes, sets each name based on its type, and returns a c# array of objects
def ConstructDomNodeArray(domNodes):
    ret = System.Array.CreateInstance(System.Object, domNodes.Count)
    for i in range(domNodes.Count):
        name = domNodes[i].Type.Name.Replace("gap:", "")
        domNodes[i].SetAttribute(LevelEditorSample.Schema.gameObjectType.nameAttribute, name)
        ret[i] = domNodes[i]
    
    return ret
    