#Sony Computer Entertainment Confidential

import System
import Test
    
def VerifyGroup(expected, actual):
    print "Expected: " + expected.Name
    print "Actual: " + actual.Name
    Test.Equal(expected.Name, actual.Name, "Verify group name")
    Test.Equal(expected.Expanded, actual.Expanded, "Verify group expanded")
    #Test.Equal(expected.Timeline, actual.Timeline, "Verify group timeline")
    Test.Equal(expected.Tracks.Count, actual.Tracks.Count, "Verify group tracks count")
    for i in range(expected.Tracks.Count):
        VerifyTrack(expected.Tracks[i], expected.Tracks[i])
    
    return

def VerifyTrack(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify track name")
    Test.Equal(expected.Group, actual.Group, "Verify track group")
    Test.Equal(expected.Intervals.Count, actual.Intervals.Count, "Verify track intervals count")
    Test.Equal(expected.Keys.Count, actual.Keys.Count, "Verify track keys count")
    
    return
    
def VerifyInterval(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify interval name")
    Test.Equal(expected.Description, actual.Description, "Verify interval description")
    Test.Equal(System.Decimal(expected.Start), System.Decimal(actual.Start), "Verify interval start")
    Test.Equal(System.Decimal(expected.Length), System.Decimal(actual.Length), "Verify interval length")
    Test.Equal(expected.Color, actual.Color, "Verify interval color")
    Test.Equal(expected.Track, actual.Track, "Verify interval track")
    
    return

def VerifyKey(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify key name")
    Test.Equal(expected.Description, actual.Description, "Verify key description")
    Test.Equal(System.Decimal(expected.Start), System.Decimal(actual.Start), "Verify key start")
    Test.Equal(System.Decimal(expected.Length), System.Decimal(actual.Length), "Verify key length")
    
    return
    
def VerifyMarker(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify marker name")
    Test.Equal(expected.Description, actual.Description, "Verify key description")
    Test.Equal(System.Decimal(expected.Start), System.Decimal(actual.Start), "Verify key start")
    Test.Equal(System.Decimal(expected.Length), System.Decimal(actual.Length), "Verify key length")
    Test.Equal(expected.Color, actual.Color, "Verify reference color")
    #Test.Equal(expected.Timeline, actual.Timeline, "Verify group timeline")
    
    return
    
def VerifyReference(expected, actual):
    Test.Equal(expected.Name, actual.Name, "Verify reference name")
    Test.Equal(System.Decimal(expected.Start), System.Decimal(actual.Start), "Verify reference start")
    Test.Equal(System.Decimal(expected.Length), System.Decimal(actual.Length), "Verify reference length")
    Test.Equal(expected.Color, actual.Color, "Verify reference color")
    Test.Equal(expected.Target, actual.Target, "Verify reference target")
    #Test.Equal(expected.Parent, actual.Parent, "Verify reference parent")
    Test.Equal(expected.Uri, actual.Uri, "Verify reference uri")
    
    return

def VerifyTimeline(timeline, srcGroups, srcMarkers, srcReferences):    
    Test.Equal(srcGroups.Count, timeline.Groups.Count, "Verify group count")
    Test.Equal(srcMarkers.Count, timeline.Markers.Count, "Verify marker count")
    Test.Equal(srcReferences.Count, timeline.References.Count, "Verify reference count")
    for i in range(srcGroups.Count):
        VerifyGroup(srcGroups[i], timeline.Groups[i])
    for i in range(srcMarkers.Count):
        VerifyMarker(srcMarkers[i], timeline.Markers[i])
    for i in range(srcReferences.Count):
        VerifyReference(srcReferences[i], timeline.References[i])
    
    return
    