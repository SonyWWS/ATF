#Sony Computer Entertainment Confidential

import sys
sys.path.append("./CommonTestScripts")
import System
import Test
import TimelineEditorUtil

doc = atfDocService.OpenNewDocument(editor)
import time
time.sleep(2)

group = editingContext.Insert[Group](DomNode(Schema.groupType.Type), doc.DomNode)
track = editingContext.Insert[Track](DomNode(Schema.trackType.Type), group.DomNode)
editingContext.Insert[Track](DomNode(Schema.trackType.Type), group.DomNode)
editingContext.Insert[Track](DomNode(Schema.trackType.Type), doc.DomNode)
interval = editingContext.Insert[Interval](DomNode(Schema.intervalType.Type), doc.DomNode)
key = editingContext.Insert[Key](DomNode(Schema.keyType.Type), doc.DomNode)
marker = editingContext.Insert[Marker](DomNode(Schema.markerType.Type), doc.DomNode)
reference = editingContext.Insert[TimelineReference](DomNode(Schema.timelineRefType.Type), doc.DomNode)

group.Name = "this group has multiple tracks"
track.Name = "<!@#$%$^>>&*()?><m,xcvnmjklkjsdfaewouri"
interval.Name = "#$@!$#! sdfsdf"
key.Name = "lkj lkdfs)(*&????>>>"
marker.Name = "markie mark"
reference.Name = "upon request"

print "Editing is done.  Now save, close, reopen, and verify all the data"
groups = []
markers = []
references = []
for group in doc.Timeline.Groups:
    groups.append(group)
for marker in doc.Timeline.Markers:
    markers.append(marker)
for ref in doc.Timeline.References:
    references.append(ref)
    
filePath = Test.GetNewFilePath("EditAndSave.timeline")

editor.Save(doc, Uri(filePath))
Test.True(File.Exists(filePath), "Verify file saved")
editor.Close(doc)

docNew = editor.Open(Uri(filePath))
TimelineEditorUtil.VerifyTimeline(docNew.Timeline, groups, markers, references)

print Test.SUCCESS