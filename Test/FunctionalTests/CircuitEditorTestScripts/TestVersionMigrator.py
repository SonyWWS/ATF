#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import CircuitEditorUtil

import Sce.Atf.Applications

print("step 1 : open the v1 sample document that has a sub-circuit and its instances ")
cntOg = Test.GetEnumerableCount(atfDocReg.Documents)
samplePath = Test.GetDataFilePath("SubCircuits.circuit", "CircuitEditorTestData")
doc = atfDocService.OpenExistingDocument(editor, Uri(samplePath))
Test.Equal(cntOg + 1, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count increased")
numElemsAtStart = circuitContainer.Elements.Count
numEdgesAtStart = circuitContainer.Wires.Count

print("step 2: verify the sub-circuit has been converted to group &  template")
Test.Equal(numElemsAtStart,  circuitContainer.Elements.Count, "Verify same number of elements as original graph")
Test.Equal(numEdgesAtStart,  circuitContainer.Wires.Count, "Verify same number of edges as original graph")

numGroupRefs = 0
for element in circuitContainer.Elements:
    if (Sce.Atf.Adaptation.Adapters.Is[GroupReference](element)):
        numGroupRefs +=1
Test.Equal(numGroupRefs,  2, "Verify two group references have replaced two subcicuit instances ")

print("step 3: regression testing WWSATF-1494: group template reference changed from 'typeRef' attribute to 'guidRef' in ATF3.8")
samplePath = Test.GetDataFilePath("OldGroupReference.circuit", "CircuitEditorTestData")
doc = atfDocService.OpenExistingDocument(editor, Uri(samplePath))
Test.Equal(cntOg + 2, Test.GetEnumerableCount(atfDocReg.Documents), "Verify document count increased")

print(Test.SUCCESS)
