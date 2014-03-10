#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import TimelineEditorUtil

#Create a child document that will have 2 tracks
docChild = atfDocService.OpenNewDocument(editor)
group = editingContext.Insert[Group](DomNode(Schema.groupType.Type), docChild.DomNode)
group.Name = "In child doc"
editingContext.Insert[Track](DomNode(Schema.trackType.Type), group.DomNode).Name = "child1 -->"
editingContext.Insert[Track](DomNode(Schema.trackType.Type), group.DomNode).Name = "<!-- child!@#$%^)(*&"

#Save, and leave open?
filePathChild = Test.GetNewFilePath("child.timeline", "subDir")

#Other tests reuse this script, so set the uri so the path can be known externally
docChild.Uri = Uri(filePathChild)
editor.Save(docChild, docChild.Uri)
Test.True(File.Exists(filePathChild), "Verify file saved")

#Create a new document that will be the parent
docParent = atfDocService.OpenNewDocument(editor)

reference = editingContext.Insert[TimelineReference](DomNode(Schema.timelineRefType.Type), docParent.DomNode)
reference.Uri = Uri(filePathChild)

filePathParent = Test.GetNewFilePath("parent.timeline")

editor.Save(docParent, Uri(filePathParent))
Test.True(File.Exists(filePathParent), "Verify file saved")
editor.Close(docParent)
docParent = editor.Open(Uri(filePathParent))

#Verify the reference document loaded properly
Test.Equal(1, docParent.Timeline.References.Count, "Verify reopened document has a reference")
Test.NotNull(docParent.Timeline.References[0].Target)
TimelineEditorUtil.VerifyTimeline(docParent.Timeline.References[0].Target, docChild.Timeline.Groups, docChild.Timeline.Markers, docChild.Timeline.References)

print Test.SUCCESS