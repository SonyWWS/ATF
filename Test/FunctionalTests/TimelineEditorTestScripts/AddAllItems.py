#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import System
import Test

doc = atfDocService.OpenNewDocument(editor)

grpCnt = 0
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify no groups at beginning")

group = editingContext.Insert[Group](DomNode(Schema.groupType.Type), doc.DomNode)
grpCnt = grpCnt + 1
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify group count after adding a group")

track = editingContext.Insert[Track](DomNode(Schema.trackType.Type), doc.DomNode)
grpCnt = grpCnt + 1
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify group count after adding a track")

interval = editingContext.Insert[Interval](DomNode(Schema.intervalType.Type), doc.DomNode)
grpCnt = grpCnt + 1
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify group count after adding an interval")

key = editingContext.Insert[Key](DomNode(Schema.keyType.Type), doc.DomNode)
grpCnt = grpCnt + 1
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify group count after adding a key")

Test.Equal(0, Test.GetEnumerableCount(doc.Timeline.Markers), "Verify no markers at beginning")
marker = editingContext.Insert[Marker](DomNode(Schema.markerType.Type), doc.DomNode)
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Markers), "Verify marker count after adding a marker")

Test.Equal(0, Test.GetEnumerableCount(doc.Timeline.References), "Verify no references at beginning")
reference = editingContext.Insert[TimelineReference](DomNode(Schema.timelineRefType.Type), doc.DomNode)
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.References), "Verify reference count after adding a reference")
Test.Equal(grpCnt, Test.GetEnumerableCount(doc.Timeline.Groups), "Verify group count after everything is added")

#This works, but will not go into the edit history
#group.Name = "guppie groupie"
#track.Name = "tracktor trailor"
#interval.Name = "i am an interval"
#key.Description = "keyed up"
#marker.Name = "markie mark"
#reference.Name = "upon request"

#Using this method will create a transaction, so the edit goes into the undo history:
editingContext.SetProperty(group.DomNode, Schema.groupType.nameAttribute, "guppie groupie")
editingContext.SetProperty(track.DomNode, Schema.trackType.nameAttribute, "tracktor trailor")
editingContext.SetProperty(interval.DomNode, Schema.intervalType.nameAttribute, "i am an interval")
editingContext.SetProperty(key.DomNode, Schema.keyType.descriptionAttribute, "keyed up")
editingContext.SetProperty(marker.DomNode, Schema.markerType.nameAttribute, "markie mark")
editingContext.SetProperty(reference.DomNode, Schema.timelineRefType.nameAttribute, "upon request")

#Groups always contain a track
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[0].Tracks))
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[1].Tracks))
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[2].Tracks))
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[3].Tracks))

#Verify the name of the group (group #0)
Test.Equal("guppie groupie", doc.Timeline.Groups[0].Name)
#Verify the name of the track (group #1)
Test.Equal("tracktor trailor", doc.Timeline.Groups[1].Tracks[0].Name)
#Verify the count and name of the interval (group #2)
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[2].Tracks[0].Intervals))
Test.Equal("i am an interval", doc.Timeline.Groups[2].Tracks[0].Intervals[0].Name)
#Verify the count and name of the key (group #3)
Test.Equal(1, Test.GetEnumerableCount(doc.Timeline.Groups[3].Tracks[0].Keys))
Test.Equal("keyed up", doc.Timeline.Groups[3].Tracks[0].Keys[0].Description)

#Name of the marker (no group)
Test.Equal("markie mark", doc.Timeline.Markers[0].Name)

#Verify the name of the reference (no group)
Test.Equal("upon request", doc.Timeline.References[0].Name)

print Test.SUCCESS