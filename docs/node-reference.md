# Node reference

Every node in the package — 315 of them — with a line on what each one does.

Each name links to that node's own page, which is the same help Dynamo shows in
the panel beside the graph. Both are generated from the source, so what is written
here is what the node's own documentation comment says. Where a node has no
description the row says so rather than guessing.


## Revit

Nodes that read and change a Revit model.

### Applications

| Node | What it does |
|---|---|
| [`CloseDocument`](nodes/Rhythm.Revit.Application.Applications.CloseDocument.md) | This node will close the given document with the option to save. |
| [`GetOpenDocuments`](nodes/Rhythm.Revit.Application.Applications.GetOpenDocuments.md) | This node provides access to all of the open documents in revit. |
| [`OpenDocumentFile`](nodes/Rhythm.Revit.Application.Applications.OpenDocumentFile.md) | This node will open the given file in the background. |
| [`SwapDocument`](nodes/Rhythm.Revit.Application.Applications.SwapDocument.md) | Not implemented |
| [`UnloadRevitLinks`](nodes/Rhythm.Revit.Application.Applications.UnloadRevitLinks.md) | Unload revit links for given file path. |
| [`UpgradeFile`](nodes/Rhythm.Revit.Application.Applications.UpgradeFile.md) | This will try to open a file in the current version with various options. |

### Documents

| Node | What it does |
|---|---|
| [`ActiveView`](nodes/Rhythm.Revit.Application.Documents.ActiveView.md) | _Not documented yet._ |
| [`CopyDraftingViewsFromDocument`](nodes/Rhythm.Revit.Application.Documents.CopyDraftingViewsFromDocument.md) | This node will copy the given drafting views and their contents from the given document into the active document. |
| [`CopyElementsFromDocument`](nodes/Rhythm.Revit.Application.Documents.CopyElementsFromDocument.md) | This node will copy the given elements from the given document into the active document. |
| [`CopyElementsFromLinkedDocument`](nodes/Rhythm.Revit.Application.Documents.CopyElementsFromLinkedDocument.md) | This node will copy the given elements from the given linked document into the active document. |
| [`DbDocumentToDynamoDocument`](nodes/Rhythm.Revit.Application.Documents.DbDocumentToDynamoDocument.md) | Convert a db document to the Dynamo kind. |
| [`DynamoDocumentToDbDocument`](nodes/Rhythm.Revit.Application.Documents.DynamoDocumentToDbDocument.md) | Convert a Dynamo document to the db kind. |
| [`SaveAs`](nodes/Rhythm.Revit.Application.Documents.SaveAs.md) | This node will save the Revit document to another path. |
| [`SetStartingView`](nodes/Rhythm.Revit.Application.Documents.SetStartingView.md) | This node will set the starting view of the document, given the view element. |

### Workset

| Node | What it does |
|---|---|
| [`ByName`](nodes/Rhythm.Revit.Application.Workset.ByName.md) | Retrieves a workset by name. |
| [`Create`](nodes/Rhythm.Revit.Application.Workset.Create.md) | Creates a new user workset in the active document. |
| [`Delete`](nodes/Rhythm.Revit.Application.Workset.Delete.md) | Deletes a user workset from the active document. |
| [`GetAll`](nodes/Rhythm.Revit.Application.Workset.GetAll.md) | Returns all user worksets in the active document. |
| [`Rename`](nodes/Rhythm.Revit.Application.Workset.Rename.md) | Renames an existing user workset in the active document. |
| [`SetVisibleInView`](nodes/Rhythm.Revit.Application.Workset.SetVisibleInView.md) | Sets workset visibility in a view. |
| [`VisibleInView`](nodes/Rhythm.Revit.Application.Workset.VisibleInView.md) | Returns whether a workset is visible in a specific view. |

### ElementFilter

| Node | What it does |
|---|---|
| [`ByCategory`](nodes/Rhythm.Revit.ElementFilter.ElementFilter.ByCategory.md) | Provides element filtering options by category. |
| [`ByName`](nodes/Rhythm.Revit.ElementFilter.ElementFilter.ByName.md) | Provides element filtering options by name. |
| [`ByParameterNumericValue`](nodes/Rhythm.Revit.ElementFilter.ElementFilter.ByParameterNumericValue.md) | Provides element filtering options by parameter numeric value. |
| [`ByParameterStringValue`](nodes/Rhythm.Revit.ElementFilter.ElementFilter.ByParameterStringValue.md) | Provides element filtering options by parameter string value. |

### AreaTag

| Node | What it does |
|---|---|
| [`TaggedArea`](nodes/Rhythm.Revit.Elements.AreaTag.TaggedArea.md) | Retrieves the area that is tagged by the given area tag. |

### Areas

| Node | What it does |
|---|---|
| [`AreaAtPoint`](nodes/Rhythm.Revit.Elements.Areas.AreaAtPoint.md) | This will return the area at the given point. |
| [`AreaScheme`](nodes/Rhythm.Revit.Elements.Areas.AreaScheme.md) | Get the input area's scheme. |
| [`Boundaries`](nodes/Rhythm.Revit.Elements.Areas.Boundaries.md) | This node will retrieve the area's boundaries. |
| [`ContainsPoint`](nodes/Rhythm.Revit.Elements.Areas.ContainsPoint.md) | This will report whether or not the area contains the given point. |
| [`GetAreaAtPoint`](nodes/Rhythm.Revit.Elements.Areas.GetAreaAtPoint.md) | *BETA* This node will retrieve the area(s) at the given point. |
| [`OuterBoundary`](nodes/Rhythm.Revit.Elements.Areas.OuterBoundary.md) | This node will retrieve the area's outermost boundary. |
| [`Solid`](nodes/Rhythm.Revit.Elements.Areas.Solid.md) | This node will retrieve the area's solid geometry. |

### BeamSystem

| Node | What it does |
|---|---|
| [`DropBeamSystem`](nodes/Rhythm.Revit.Elements.BeamSystem.DropBeamSystem.md) | Drops the beam system. |
| [`Members`](nodes/Rhythm.Revit.Elements.BeamSystem.Members.md) | Obtains the individual beams within a beam system. |

### Categories

| Node | What it does |
|---|---|
| [`BuiltInCategoryName`](nodes/Rhythm.Revit.Elements.Categories.BuiltInCategoryName.md) | Get the built in category name. |
| [`CollectHatchPatternCategories`](nodes/Rhythm.Revit.Elements.Categories.CollectHatchPatternCategories.md) | Collects all surface pattern related categories for override. |
| [`CutLineweight`](nodes/Rhythm.Revit.Elements.Categories.CutLineweight.md) | Get the category cut lineweight. |
| [`LineColor`](nodes/Rhythm.Revit.Elements.Categories.LineColor.md) | Get the category line color as RGB string.. |
| [`LinePattern`](nodes/Rhythm.Revit.Elements.Categories.LinePattern.md) | Get the category line pattern. |
| [`Material`](nodes/Rhythm.Revit.Elements.Categories.Material.md) | Get the category material and name. |
| [`ProjectionLineweight`](nodes/Rhythm.Revit.Elements.Categories.ProjectionLineweight.md) | Get the category projection lineweight. |
| [`SetCutLineweight`](nodes/Rhythm.Revit.Elements.Categories.SetCutLineweight.md) | Set the category cut lineweight. |
| [`SetProjectionLineweight`](nodes/Rhythm.Revit.Elements.Categories.SetProjectionLineweight.md) | Set the category projection lineweight. |

### Ceiling

| Node | What it does |
|---|---|
| [`ByCurveLoops`](nodes/Rhythm.Revit.Elements.Ceiling.ByCurveLoops.md) | Create a ceiling by multiple curve loops. |
| [`DefaultCeilingType`](nodes/Rhythm.Revit.Elements.Ceiling.DefaultCeilingType.md) | Collect the first ceiling type available. |
| [`GetGridLines`](nodes/Rhythm.Revit.Elements.Ceiling.GetGridLines.md) | Returns ceiling grid lines, with the option to return the boundary as well. |

### CurtainGrid

| Node | What it does |
|---|---|
| [`AddGridLineByPoint`](nodes/Rhythm.Revit.Elements.CurtainGrid.AddGridLineByPoint.md) | This node will add a gridline at the specified place on the curtain wall grid. |
| [`ByCurtainSystem`](nodes/Rhythm.Revit.Elements.CurtainGrid.ByCurtainSystem.md) | This node will retrieve the curtain grid per face from the curtain system. |
| [`ByRoofElement`](nodes/Rhythm.Revit.Elements.CurtainGrid.ByRoofElement.md) | This node will retrieve the curtain grid and U/V Gridlines from the given wall |
| [`ByWallElement`](nodes/Rhythm.Revit.Elements.CurtainGrid.ByWallElement.md) | This node will retrieve the curtain grid and U/V Gridlines from the given wall |
| [`UGrids`](nodes/Rhythm.Revit.Elements.CurtainGrid.UGrids.md) | This node will retrieve the U gridlines from the curtain grid |
| [`VGrids`](nodes/Rhythm.Revit.Elements.CurtainGrid.VGrids.md) | This node will retrieve the V gridlines from the curtain grid |

### CurtainGridLine

| Node | What it does |
|---|---|
| [`AllSegmentCurves`](nodes/Rhythm.Revit.Elements.CurtainGridLine.AllSegmentCurves.md) | This node will retrieve the geometric curve segments from the curtain wall. |
| [`ExistingSegmentCurves`](nodes/Rhythm.Revit.Elements.CurtainGridLine.ExistingSegmentCurves.md) | This node will retrieve the geometric existing curve segments from the curtain wall. |
| [`FullCurve`](nodes/Rhythm.Revit.Elements.CurtainGridLine.FullCurve.md) | This node will retrieve the geometric curve from the curtain wall. |
| [`RemoveSegment`](nodes/Rhythm.Revit.Elements.CurtainGridLine.RemoveSegment.md) | This node will remove the given curve segments from the curtain grid line. |
| [`SetLocation`](nodes/Rhythm.Revit.Elements.CurtainGridLine.SetLocation.md) | This node will attempt to set the location of the given grid line to the given point. |
| [`SkippedSegmentCurves`](nodes/Rhythm.Revit.Elements.CurtainGridLine.SkippedSegmentCurves.md) | This node will retrieve the geometric skipped curve segments from the curtain wall. |

### CurtainPanels

| Node | What it does |
|---|---|
| [`IsolateInView`](nodes/Rhythm.Revit.Elements.CurtainPanels.IsolateInView.md) | This node will isolate the given curtain wall panels in the active view. |

### DimensionSegment

| Node | What it does |
|---|---|
| [`Above`](nodes/Rhythm.Revit.Elements.DimensionSegment.Above.md) | _Not documented yet._ |
| [`Below`](nodes/Rhythm.Revit.Elements.DimensionSegment.Below.md) | _Not documented yet._ |
| [`Prefix`](nodes/Rhythm.Revit.Elements.DimensionSegment.Prefix.md) | _Not documented yet._ |
| [`QueryData`](nodes/Rhythm.Revit.Elements.DimensionSegment.QueryData.md) | Get the data from the dimension segment |
| [`SetData`](nodes/Rhythm.Revit.Elements.DimensionSegment.SetData.md) | Set the data given the inputs. |
| [`Suffix`](nodes/Rhythm.Revit.Elements.DimensionSegment.Suffix.md) | _Not documented yet._ |
| [`Value`](nodes/Rhythm.Revit.Elements.DimensionSegment.Value.md) | _Not documented yet._ |
| [`ValueString`](nodes/Rhythm.Revit.Elements.DimensionSegment.ValueString.md) | _Not documented yet._ |

### DimensionTypes

| Node | What it does |
|---|---|
| [`UsesProjectSettings`](nodes/Rhythm.Revit.Elements.DimensionTypes.UsesProjectSettings.md) | Determine if the given dimension type uses project (default) settings. |

### Dimensions

| Node | What it does |
|---|---|
| [`AboveValue`](nodes/Rhythm.Revit.Elements.Dimensions.AboveValue.md) | Retrieve the dimension above value. |
| [`Accuracy`](nodes/Rhythm.Revit.Elements.Dimensions.Accuracy.md) | This node will return the accuracy for the given dimension. |
| [`BelowValue`](nodes/Rhythm.Revit.Elements.Dimensions.BelowValue.md) | Retrieve the dimension below value. |
| [`CenterTextOnLine`](nodes/Rhythm.Revit.Elements.Dimensions.CenterTextOnLine.md) | *BETA* This node will center the dimension's text on the line. |
| [`Color`](nodes/Rhythm.Revit.Elements.Dimensions.Color.md) | This node will return the color for the given dimension. |
| [`DisplayUnits`](nodes/Rhythm.Revit.Elements.Dimensions.DisplayUnits.md) | This node will return the display unit type for the given dimension. |
| [`DisplayValueString`](nodes/Rhythm.Revit.Elements.Dimensions.DisplayValueString.md) | Retrieve the actual dimension display value. |
| [`GetCurve`](nodes/Rhythm.Revit.Elements.Dimensions.GetCurve.md) | This node will get the dimension's line. |
| [`GetReferenceElements`](nodes/Rhythm.Revit.Elements.Dimensions.GetReferenceElements.md) | This node will retrieve the reference elements of the dimension. |
| [`IsOverriden`](nodes/Rhythm.Revit.Elements.Dimensions.IsOverriden.md) | This node will check if the dimension has any overrides in it of text. |
| [`NumberOfSegments`](nodes/Rhythm.Revit.Elements.Dimensions.NumberOfSegments.md) | This node will return the number of segments comprising the multi segment dimension. |
| [`Origin`](nodes/Rhythm.Revit.Elements.Dimensions.Origin.md) | This node will return the origin of the dimension. |
| [`Segments`](nodes/Rhythm.Revit.Elements.Dimensions.Segments.md) | This node will return the segments comprising the multi segment dimension. |
| [`SetAboveValue`](nodes/Rhythm.Revit.Elements.Dimensions.SetAboveValue.md) | This node will try to set the above value for the dimensions. |
| [`SetBelowValue`](nodes/Rhythm.Revit.Elements.Dimensions.SetBelowValue.md) | This node will try to set the below value for the dimensions. |
| [`SetFormat`](nodes/Rhythm.Revit.Elements.Dimensions.SetFormat.md) | _Not documented yet._ |
| [`SetTextLocation`](nodes/Rhythm.Revit.Elements.Dimensions.SetTextLocation.md) | This node will try to set the text location for the given dimensions. |
| [`TextPosition`](nodes/Rhythm.Revit.Elements.Dimensions.TextPosition.md) | This node will return the text position of the dimension. |
| [`ValueString`](nodes/Rhythm.Revit.Elements.Dimensions.ValueString.md) | This node will return the value (string) of the dimension. |

### Elements

| Node | What it does |
|---|---|
| [`AreaLocation`](nodes/Rhythm.Revit.Elements.Elements.AreaLocation.md) | *BETA* - This node will retrieve the closest area that an element resides in. |
| [`CreateParts`](nodes/Rhythm.Revit.Elements.Elements.CreateParts.md) | This node will convert the given elements to parts. |
| [`DependentElements`](nodes/Rhythm.Revit.Elements.Elements.DependentElements.md) | This node will report what elements depend on the input element. |
| [`DependentElementsOfCategory`](nodes/Rhythm.Revit.Elements.Elements.DependentElementsOfCategory.md) | This node will report what elements depend on the input element. |
| [`GetIntersectingElementsOfCategory`](nodes/Rhythm.Revit.Elements.Elements.GetIntersectingElementsOfCategory.md) | This will take a given element and category and grab the intersecting elements of that category. |
| [`GetIntersectingElementsOfCategoryLinkOption`](nodes/Rhythm.Revit.Elements.Elements.GetIntersectingElementsOfCategoryLinkOption.md) | This will take a given element and category and grab the intersecting elements of that category. |
| [`GetParameterValueByNameCaSeiNSeNSiTiVe`](nodes/Rhythm.Revit.Elements.Elements.GetParameterValueByNameCaSeiNSeNSiTiVe.md) | This node will get a parameter value by search string, regardless of case of the search string. |
| [`GetParameterValueByNameTypeOrInstance`](nodes/Rhythm.Revit.Elements.Elements.GetParameterValueByNameTypeOrInstance.md) | This node will get the parameter as instance or type. |
| [`IntersectingElementsOfCategoryBuffered`](nodes/Rhythm.Revit.Elements.Elements.IntersectingElementsOfCategoryBuffered.md) | This will take a given element and category and grab the intersecting elements of that category. |
| [`IsHiddenInView`](nodes/Rhythm.Revit.Elements.Elements.IsHiddenInView.md) | This node will report whether or not the given element is hidden in given views. |
| [`JoinedElements`](nodes/Rhythm.Revit.Elements.Elements.JoinedElements.md) | This node will report what elements are joined to the input element. |
| [`SetParameterByNameTypeOrInstance`](nodes/Rhythm.Revit.Elements.Elements.SetParameterByNameTypeOrInstance.md) | Set one of the element's parameters. |
| [`SetParameterValueByNameCaSeiNSeNSiTiVe`](nodes/Rhythm.Revit.Elements.Elements.SetParameterValueByNameCaSeiNSeNSiTiVe.md) | This node will set a parameter value by search string, regardless of case of the search string. |
| [`SetPinnedStatus`](nodes/Rhythm.Revit.Elements.Elements.SetPinnedStatus.md) | This node will change the pinned status of an element. |
| [`SetRotation`](nodes/Rhythm.Revit.Elements.Elements.SetRotation.md) | Rotate an element in Revit given the angle and an optional rotation vector. |
| [`ViewFinder`](nodes/Rhythm.Revit.Elements.Elements.ViewFinder.md) | This finds all the views an element appears in. |

### ElevationMarker

| Node | What it does |
|---|---|
| [`CreateElevationByMarkerIndex`](nodes/Rhythm.Revit.Elements.ElevationMarker.CreateElevationByMarkerIndex.md) | This node will add elevations on each side of the marker chosen. |
| [`CreateElevationMarker`](nodes/Rhythm.Revit.Elements.ElevationMarker.CreateElevationMarker.md) | This node will create an empty elevation marker at the given points. |

### FamilyInstances

| Node | What it does |
|---|---|
| [`ArrayInterrogator`](nodes/Rhythm.Revit.Elements.FamilyInstances.ArrayInterrogator.md) | Detect if the input family instance has arrays set to less than 1 |
| [`ByGeometry`](nodes/Rhythm.Revit.Elements.FamilyInstances.ByGeometry.md) | Create a familyInstance from Dynamo solid geometry. |
| [`ByRoom`](nodes/Rhythm.Revit.Elements.FamilyInstances.ByRoom.md) | This node will create and place a generic model family instance at all the room locations given the room element. |
| [`RetrieveNestedComponents`](nodes/Rhythm.Revit.Elements.FamilyInstances.RetrieveNestedComponents.md) | This node will find all deeply nested components in the given family instance. |
| [`Room`](nodes/Rhythm.Revit.Elements.FamilyInstances.Room.md) | This node will report the room the family instance resides in, (if available). |
| [`RoomInPhase`](nodes/Rhythm.Revit.Elements.FamilyInstances.RoomInPhase.md) | This node will report the room the family instance resides in, (if available). |
| [`Space`](nodes/Rhythm.Revit.Elements.FamilyInstances.Space.md) | This node will report the space the family instance resides in, (if available). |
| [`SpaceInPhase`](nodes/Rhythm.Revit.Elements.FamilyInstances.SpaceInPhase.md) | This node will report the space the family instance resides in, (if available). |

### FilledRegions

| Node | What it does |
|---|---|
| [`ByMultipleLoops`](nodes/Rhythm.Revit.Elements.FilledRegions.ByMultipleLoops.md) | This will create a filled region with multiple loops. |

### Floor

| Node | What it does |
|---|---|
| [`ByCurveLoops`](nodes/Rhythm.Revit.Elements.Floor.ByCurveLoops.md) | Create a floor with multiple loops. |
| [`DefaultFloorType`](nodes/Rhythm.Revit.Elements.Floor.DefaultFloorType.md) | Collect the first floor type available. |

### Group

| Node | What it does |
|---|---|
| [`ByElementsAndOrigin`](nodes/Rhythm.Revit.Elements.Group.ByElementsAndOrigin.md) | This node is a pretty neat group creator, that allows for you to pick an origin at creation time. |

### HostObject

| Node | What it does |
|---|---|
| [`BottomSurface`](nodes/Rhythm.Revit.Elements.HostObject.BottomSurface.md) | This node will return the bottom face or faces for the input host object. |
| [`ExteriorSurface`](nodes/Rhythm.Revit.Elements.HostObject.ExteriorSurface.md) | This node will return the exterior face or faces for the input host object. |
| [`InteriorSurface`](nodes/Rhythm.Revit.Elements.HostObject.InteriorSurface.md) | This node will return the interior face or faces for the input host object. |
| [`TopSurface`](nodes/Rhythm.Revit.Elements.HostObject.TopSurface.md) | This node will return the bottom face or faces for the input host object. |

### Leaders

| Node | What it does |
|---|---|
| [`GetLeaderElbow`](nodes/Rhythm.Revit.Elements.Leaders.GetLeaderElbow.md) | This will get the position of the leader's elbow. |
| [`GetLeaderEnd`](nodes/Rhythm.Revit.Elements.Leaders.GetLeaderEnd.md) | This will get the position of the leader's end. |
| [`SetLeaderElbowPosition`](nodes/Rhythm.Revit.Elements.Leaders.SetLeaderElbowPosition.md) | This will set a leader's elbow position. |
| [`SetLeaderEndPosition`](nodes/Rhythm.Revit.Elements.Leaders.SetLeaderEndPosition.md) | This will set a leader's end position. |

### Levels

| Node | What it does |
|---|---|
| [`HasView`](nodes/Rhythm.Revit.Elements.Levels.HasView.md) | Check to see if the level has a view created for it.😎 |

### Mullions

| Node | What it does |
|---|---|
| [`ByDirection`](nodes/Rhythm.Revit.Elements.Mullions.ByDirection.md) | This node will retrieve the mullions from the curtain wall grouped by direction. |

### Parts

| Node | What it does |
|---|---|
| [`DivideParts`](nodes/Rhythm.Revit.Elements.Parts.DivideParts.md) | This node will divide the given parts by reference planes. |
| [`GetSourceElement`](nodes/Rhythm.Revit.Elements.Parts.GetSourceElement.md) | Gets the collection of elements from which the parts were created. |

### ReferencePlanes

| Node | What it does |
|---|---|
| [`ByLine`](nodes/Rhythm.Revit.Elements.ReferencePlanes.ByLine.md) | This will create a reference plane by the given curve and the selected direction. |
| [`GetCurvesInView`](nodes/Rhythm.Revit.Elements.ReferencePlanes.GetCurvesInView.md) | This node will get the underlying curve of the reference plane in a given view. |

### RevitLink

| Node | What it does |
|---|---|
| [`GetDocument`](nodes/Rhythm.Revit.Elements.RevitLink.GetDocument.md) | This node will obtain the selected link's document. |

### RevitLinkType

Wrappers for elements

| Node | What it does |
|---|---|
| [`ReloadFrom`](nodes/Rhythm.Revit.Elements.RevitLinkType.ReloadFrom.md) | Reload link from another path. |

### Roofs

| Node | What it does |
|---|---|
| [`AddPoint`](nodes/Rhythm.Revit.Elements.Roofs.AddPoint.md) | This node will add a point to the given roof. |
| [`AddSplitLineWithElevation`](nodes/Rhythm.Revit.Elements.Roofs.AddSplitLineWithElevation.md) | This node will add a split line to the given roof with supplied line and elevation. |
| [`Footprint`](nodes/Rhythm.Revit.Elements.Roofs.Footprint.md) | Retrieve the footprint of any roof element |

### RoomTag

| Node | What it does |
|---|---|
| [`CenterOnRoomLocation`](nodes/Rhythm.Revit.Elements.RoomTag.CenterOnRoomLocation.md) | This node will set the room tag to the same as the room location. |
| [`PlaceOrUpdate`](nodes/Rhythm.Revit.Elements.RoomTag.PlaceOrUpdate.md) | Place or update an existing room tag. |
| [`TaggedRoom`](nodes/Rhythm.Revit.Elements.RoomTag.TaggedRoom.md) | This node will retrieve the room that a tag is tagging. |

### Rooms

| Node | What it does |
|---|---|
| [`ApproximateDimensions`](nodes/Rhythm.Revit.Elements.Rooms.ApproximateDimensions.md) | This will return the approximate room dimensions. |
| [`CenterRoom`](nodes/Rhythm.Revit.Elements.Rooms.CenterRoom.md) | This node will center the room. |
| [`CenterRoom2`](nodes/Rhythm.Revit.Elements.Rooms.CenterRoom2.md) | This node will center the room. |
| [`IntersectWithCurve`](nodes/Rhythm.Revit.Elements.Rooms.IntersectWithCurve.md) | Provides a more stable method of intersecting a curve with a room element for room renumbering workflows. |
| [`IntersectingElementsInRoom`](nodes/Rhythm.Revit.Elements.Rooms.IntersectingElementsInRoom.md) | This node will center the room. |
| [`Level`](nodes/Rhythm.Revit.Elements.Rooms.Level.md) | Get the level for the given room |
| [`RoomTagsInView`](nodes/Rhythm.Revit.Elements.Rooms.RoomTagsInView.md) | Return room tags for given room in given view |

### Sheet

| Node | What it does |
|---|---|
| [`Create`](nodes/Rhythm.Revit.Elements.Sheet.Create.md) | Creates a new sheet. |
| [`GetViewportsAndViews`](nodes/Rhythm.Revit.Elements.Sheet.GetViewportsAndViews.md) | This node will obtain viewports, views and schedules from a given sheet. |
| [`Titleblock`](nodes/Rhythm.Revit.Elements.Sheet.Titleblock.md) | This node will grab the titleblock from the given sheet. |

### Revit.Elements.SheetCollection

| Node | What it does |
|---|---|
| [`Sheets`](nodes/Rhythm.Revit.Elements.SheetCollection.Sheets.md) | Gets all sheets in the specified sheet collection. |

### SlopedGlazing

| Node | What it does |
|---|---|
| [`SetAnglesAndOffsets`](nodes/Rhythm.Revit.Elements.SlopedGlazing.SetAnglesAndOffsets.md) | Set the offset parameters |

### SpaceTag

| Node | What it does |
|---|---|
| [`PlaceOrUpdate`](nodes/Rhythm.Revit.Elements.SpaceTag.PlaceOrUpdate.md) | Place or update an existing space tag. |

### Spaces

| Node | What it does |
|---|---|
| [`SpaceTagsInView`](nodes/Rhythm.Revit.Elements.Spaces.SpaceTagsInView.md) | Return space tags for given space in given view |

### Tags

| Node | What it does |
|---|---|
| [`GetHeadPosition`](nodes/Rhythm.Revit.Elements.Tags.GetHeadPosition.md) | The position of the head of tag in model coordinates (if available). |
| [`GetLeaderElbow`](nodes/Rhythm.Revit.Elements.Tags.GetLeaderElbow.md) | The position of the elbow of the tag's leader. |
| [`GetLeaderEnd`](nodes/Rhythm.Revit.Elements.Tags.GetLeaderEnd.md) | The position of the leader end for a tag using free end leader behavior. |
| [`SetHeadPosition`](nodes/Rhythm.Revit.Elements.Tags.SetHeadPosition.md) | This will attempt to set the head position of the tag. |
| [`TagText`](nodes/Rhythm.Revit.Elements.Tags.TagText.md) | This will return the tag's text value |

### TextNotes

| Node | What it does |
|---|---|
| [`GetLeaders`](nodes/Rhythm.Revit.Elements.TextNotes.GetLeaders.md) | This node will return all of the leaders associated with the text note. |
| [`ToLower`](nodes/Rhythm.Revit.Elements.TextNotes.ToLower.md) | This node will convert the text note to lower with formatting. |
| [`ToUpper`](nodes/Rhythm.Revit.Elements.TextNotes.ToUpper.md) | This node will convert the text note to upper with formatting. |

### Viewport

| Node | What it does |
|---|---|
| [`AlignViewTitle`](nodes/Rhythm.Revit.Elements.Viewport.AlignViewTitle.md) | _Not documented yet._ |
| [`BoxCenter`](nodes/Rhythm.Revit.Elements.Viewport.BoxCenter.md) | This node will retrieve the viewport's box center. |
| [`Create`](nodes/Rhythm.Revit.Elements.Viewport.Create.md) | This node will place the given view on the given sheet, if possible. |
| [`GetView`](nodes/Rhythm.Revit.Elements.Viewport.GetView.md) | This node will obtain the view from the given viewport. |
| [`GetViewTitleLocation`](nodes/Rhythm.Revit.Elements.Viewport.GetViewTitleLocation.md) | Get a viewport's title location (relative to the boundary of the view) Revit 2022+. |
| [`LabelOutline`](nodes/Rhythm.Revit.Elements.Viewport.LabelOutline.md) | This node will obtain the outline of the Viewport title if one is used. |
| [`LocationData`](nodes/Rhythm.Revit.Elements.Viewport.LocationData.md) | This node will obtain the box location data from the provided viewport. |
| [`SetBoxCenter`](nodes/Rhythm.Revit.Elements.Viewport.SetBoxCenter.md) | This node will set the viewport's box center given the point. |
| [`SetLocationBasedOnOther`](nodes/Rhythm.Revit.Elements.Viewport.SetLocationBasedOnOther.md) | This node will set the child viewports box center given the parent viewport. |
| [`SetViewTitleLength`](nodes/Rhythm.Revit.Elements.Viewport.SetViewTitleLength.md) | Set a viewport's title length. |
| [`SetViewTitleLocation`](nodes/Rhythm.Revit.Elements.Viewport.SetViewTitleLocation.md) | Set a viewport's title location (relative to the boundary of the view) Revit 2022+. |

### Walls

| Node | What it does |
|---|---|
| [`Direction`](nodes/Rhythm.Revit.Elements.Walls.Direction.md) | This will estimate the wall's facing direction. |
| [`EditedProfile`](nodes/Rhythm.Revit.Elements.Walls.EditedProfile.md) | This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+ |
| [`HasEditedProfile`](nodes/Rhythm.Revit.Elements.Walls.HasEditedProfile.md) | This node will try to check if the walls profile has been modified using the dependent elements method available in Revit 2018.1+ |

### Windows

| Node | What it does |
|---|---|
| [`Direction`](nodes/Rhythm.Revit.Elements.Windows.Direction.md) | This will get the window's facing direction based on the FacingOrientation property. |

### Fabrication

| Node | What it does |
|---|---|
| [`ExportToPCF`](nodes/Rhythm.Revit.Fabrication.Fabrication.ExportToPCF.md) | This node exports a list of fabrication elements to a PCF (Piping Component File) format. |

### Revit.Helpers.Helpers

Helpers Wrapper

| Node | What it does |
|---|---|
| [`CurrentRevitVersion`](nodes/Rhythm.Revit.Helpers.Helpers.CurrentRevitVersion.md) | Returns the current Revit version |
| [`PurgeBindings`](nodes/Rhythm.Revit.Helpers.Helpers.PurgeBindings.md) | _Not documented yet._ |
| [`SimpleUserMessage`](nodes/Rhythm.Revit.Helpers.Helpers.SimpleUserMessage.md) | This provides a simple user message. |
| [`ToggleElementBinder`](nodes/Rhythm.Revit.Helpers.Helpers.ToggleElementBinder.md) | This allows you to turn off element binding in the DYN. |
| [`UserMessage`](nodes/Rhythm.Revit.Helpers.Helpers.UserMessage.md) | This provides a user message with the option to cancel the process downstream. |

### Modifiers

| Node | What it does |
|---|---|
| [`Rotate`](nodes/Rhythm.Revit.Ribbon.Modifiers.Rotate.md) | This will rotate your ribbon. |
| [`SetColor`](nodes/Rhythm.Revit.Ribbon.Modifiers.SetColor.md) | This will set the color of your ribbon. |
| [`SetFont`](nodes/Rhythm.Revit.Ribbon.Modifiers.SetFont.md) | This will set the font on your ribbon. |

### RibbonTab

| Node | What it does |
|---|---|
| [`GetTabs`](nodes/Rhythm.Revit.Ribbon.RibbonTab.GetTabs.md) | This will give you access to all tabs. |
| [`Name`](nodes/Rhythm.Revit.Ribbon.RibbonTab.Name.md) | This will get the tab's name. |
| [`SetEnabled`](nodes/Rhythm.Revit.Ribbon.RibbonTab.SetEnabled.md) | This will enable or disable the given tab. |
| [`SetName`](nodes/Rhythm.Revit.Ribbon.RibbonTab.SetName.md) | This will rename a tab given a new name. |
| [`SetVisibility`](nodes/Rhythm.Revit.Ribbon.RibbonTab.SetVisibility.md) | This will hide or show the given tab. |
| [`Visibility`](nodes/Rhythm.Revit.Ribbon.RibbonTab.Visibility.md) | This will get the tab's visibility status. |

### Collector

| Node | What it does |
|---|---|
| [`ElementsOfCategoryInDocument`](nodes/Rhythm.Revit.Selection.Collector.ElementsOfCategoryInDocument.md) | This node will collect all elements of the given category from given document. |
| [`ElementsOfTypeInDocument`](nodes/Rhythm.Revit.Selection.Collector.ElementsOfTypeInDocument.md) | This node will collect all elements of type from given document. |
| [`GroupByName`](nodes/Rhythm.Revit.Selection.Collector.GroupByName.md) | Collect a detail or model group by a given name in the current model. |

### Selection

| Node | What it does |
|---|---|
| [`FromLink`](nodes/Rhythm.Revit.Selection.Selection.FromLink.md) | Select stuff from a link. |
| [`IntersectingGridsByModelCurve`](nodes/Rhythm.Revit.Selection.Selection.IntersectingGridsByModelCurve.md) | This node will select grids along a model curve element ordered based on the start of the model curve. |
| [`Pick`](nodes/Rhythm.Revit.Selection.Selection.Pick.md) | Sometimes a pick selection is nicer. |
| [`RoomAtPoint`](nodes/Rhythm.Revit.Selection.Selection.RoomAtPoint.md) | Select a room at the corresponding point in the corresponding phase. |

### Batch

| Node | What it does |
|---|---|
| [`UpgradeFamilies`](nodes/Rhythm.Revit.Tools.Batch.UpgradeFamilies.md) | This tool with batch upgrade all the Revit families in a directory and delete the backup files that are generated. |

### Revit.Tools.Element

| Node | What it does |
|---|---|
| [`AnimateColor`](nodes/Rhythm.Revit.Tools.Element.AnimateColor.md) | Animate the color of an element. |
| [`AnimateNumericParameter`](nodes/Rhythm.Revit.Tools.Element.AnimateNumericParameter.md) | Animate a numeric parameter of an element. |
| [`AnimateTransparency`](nodes/Rhythm.Revit.Tools.Element.AnimateTransparency.md) | Animate the transparency of an element. |

### Tools

| Node | What it does |
|---|---|
| [`ThreeDeeRoomTags`](nodes/Rhythm.Revit.Tools.Tools.ThreeDeeRoomTags.md) | Create 3d room tags given the input rooms! |
| [`ThreeDeeSpaceTags`](nodes/Rhythm.Revit.Tools.Tools.ThreeDeeSpaceTags.md) | Create 3d space tags given the input spaces! |

### TableData

| Node | What it does |
|---|---|
| [`TableSectionData`](nodes/Rhythm.Revit.Views.TableData.TableSectionData.md) | _Not documented yet._ |

### TableSectionData

| Node | What it does |
|---|---|
| [`GetColumnWidth`](nodes/Rhythm.Revit.Views.TableSectionData.GetColumnWidth.md) | _Not documented yet._ |
| [`SetColumnWidth`](nodes/Rhythm.Revit.Views.TableSectionData.SetColumnWidth.md) | _Not documented yet._ |

### View

| Node | What it does |
|---|---|
| [`ConvertToIndependent`](nodes/Rhythm.Revit.Views.View.ConvertToIndependent.md) | This node will convert a dependent view to an independent. |
| [`CopyOrderedFiltersFromView`](nodes/Rhythm.Revit.Views.View.CopyOrderedFiltersFromView.md) | Revit 2021 - Copies view filters from the source view to the receiving view while preserving filter order. |
| [`GetCropRegionElement`](nodes/Rhythm.Revit.Views.View.GetCropRegionElement.md) | This node will obtain the crop region element from the view. |
| [`GetFilterVisibility`](nodes/Rhythm.Revit.Views.View.GetFilterVisibility.md) | This node will supply the visibility of the given filter in given view. |
| [`GetOrderedFilters`](nodes/Rhythm.Revit.Views.View.GetOrderedFilters.md) | Revit 2021 - Returns the filters in order for the given view. |
| [`GetWorksetVisibility`](nodes/Rhythm.Revit.Views.View.GetWorksetVisibility.md) | This node will supply the visibility of the given workset in given view. |
| [`HideElements`](nodes/Rhythm.Revit.Views.View.HideElements.md) | Hide the given elements in the given view. |
| [`IsFilterEnabled`](nodes/Rhythm.Revit.Views.View.IsFilterEnabled.md) | Revit 2021 - Checks if a view filter is enabled in the given view. |
| [`Origin`](nodes/Rhythm.Revit.Views.View.Origin.md) | Retrieve the input view's origin, (if available). |
| [`ParentView`](nodes/Rhythm.Revit.Views.View.ParentView.md) | Retrieve the input dependent view's parent, (if available). |
| [`SetElementProjectionLineweight`](nodes/Rhythm.Revit.Views.View.SetElementProjectionLineweight.md) | This node will override the given element's projection lineweight in given view. |
| [`SetFilterOrder`](nodes/Rhythm.Revit.Views.View.SetFilterOrder.md) | Set the view filter order for the view or view template. |
| [`ToggleFilterInView`](nodes/Rhythm.Revit.Views.View.ToggleFilterInView.md) | Revit 2021 - This attempts to enable or disable a filter for a given view. |
| [`UnhideElements`](nodes/Rhythm.Revit.Views.View.UnhideElements.md) | Unhide the given elements in the given view. |
| [`Viewport`](nodes/Rhythm.Revit.Views.View.Viewport.md) | Retrieve the view's viewport(s) if there is one. |

### ViewPlan

| Node | What it does |
|---|---|
| [`ByLevelTypeAndName`](nodes/Rhythm.Revit.Views.ViewPlan.ByLevelTypeAndName.md) | _Not documented yet._ |
| [`GetCropBox`](nodes/Rhythm.Revit.Views.ViewPlan.GetCropBox.md) | This node will get the bounds of the view in paper space (in feet). |
| [`GetOutline`](nodes/Rhythm.Revit.Views.ViewPlan.GetOutline.md) | This node will get the bounds of the view in paper space (in feet). |
| [`Rotate`](nodes/Rhythm.Revit.Views.ViewPlan.Rotate.md) | This node will attempt to rotate a plan view into a 3D view. |

### ViewSchedule

| Node | What it does |
|---|---|
| [`TableData`](nodes/Rhythm.Revit.Views.ViewSchedule.TableData.md) | _Not documented yet._ |

### ViewSection

| Node | What it does |
|---|---|
| [`CreateReferenceSection`](nodes/Rhythm.Revit.Views.ViewSection.CreateReferenceSection.md) | Creates a reference section. |
| [`LocationPoint`](nodes/Rhythm.Revit.Views.ViewSection.LocationPoint.md) | Retrieve the input view's origin, (if available). |
| [`OverrideCrop`](nodes/Rhythm.Revit.Views.ViewSection.OverrideCrop.md) | This node will override the crop region of the given section view based on the pen weight provided. |
| [`OverrideCropVersion2`](nodes/Rhythm.Revit.Views.ViewSection.OverrideCropVersion2.md) | This node will override the crop region of the given section view based on the pen weight provided. |

### Revit.Worksharing.Element

| Node | What it does |
|---|---|
| [`Creator`](nodes/Rhythm.Revit.Worksharing.Element.Creator.md) | This node will output the username of the creator of the element if it is available. |
| [`LastChangedBy`](nodes/Rhythm.Revit.Worksharing.Element.LastChangedBy.md) | This node will output the username of the person who last changed the element if it is available. |

## Revit UI

Dropdowns and selection nodes, which put a Revit list on the node itself.

### DesignOptions

| Node | What it does |
|---|---|
| [`Design Options`](nodes/RhythmUI.DesignOptions.md) | Displays design options with option set for your use. |

### Links

| Node | What it does |
|---|---|
| [`Links`](nodes/RhythmUI.Links.md) | Allows you to select a link instance from all of the Revit links in your file. |

### RoofTypes

| Node | What it does |
|---|---|
| [`Roof Types`](nodes/RhythmUI.RoofTypes.md) | Allows you to select a roof type from the types in your project. |

### ScheduleViews

| Node | What it does |
|---|---|
| [`Schedule Views`](nodes/RhythmUI.ScheduleViews.md) | Allows you to select a schedule view from the instances in your project. |

### ScopeBoxes

| Node | What it does |
|---|---|
| [`Scope Boxes`](nodes/RhythmUI.ScopeBoxes.md) | Allows you to select a scope box from all of the scope boxes in your project. |

### SelectElementInLink

| Node | What it does |
|---|---|
| [`Select Element from Link`](nodes/RhythmUI.SelectElementInLink.md) | This allows you to select an element from a link. |

### SelectElementsInLink

| Node | What it does |
|---|---|
| [`Select Elements from Link`](nodes/RhythmUI.SelectElementsInLink.md) | This allows you to select multiple elements from links. |

### RhythmUI.SheetCollection

| Node | What it does |
|---|---|
| [`Sheet Collections`](nodes/RhythmUI.SheetCollection.md) | Allows you to select a sheet collection from all sheet collections in your project. |

### Sheets

| Node | What it does |
|---|---|
| [`Sheets`](nodes/RhythmUI.Sheets.md) | Allows you to select a sheet from all of the sheets in your project. |

### TitleblockTypes

| Node | What it does |
|---|---|
| [`Titleblock Types`](nodes/RhythmUI.TitleblockTypes.md) | Allows you to select a titleblock type from your Revit file. |

### SelFilter

| Node | What it does |
|---|---|
| [`And`](nodes/RhythmUI.Utilities.SelFilter.And.md) | Creates a logical "and" filter |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28filter%29.md) | Creates a selection filter from an ElementFilter |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28id%2C%20ids%29.md) | Creates a selection filter that will let pass only the elements defined by the ids |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28ids%29.md) | Creates a selection filter that will let pass only the elements defined by the ids |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28allowedTypes%29.md) | Creates a selection filter, which elements of any Type in the collection will pass |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28filterMethod%29.md) | Creates a selection filter that will use the "filterMethod" to filter the elements |
| [`GetElementFilter`](nodes/RhythmUI.Utilities.SelFilter.GetElementFilter%28type%2C%20types%29.md) | Creates a selection filter, which elements of any of the given Types  will pass |
| [`GetFaceNormalFilter`](nodes/RhythmUI.Utilities.SelFilter.GetFaceNormalFilter.md) | Creates a selection filter that will let faces pass, if their normal vector at (0/0) is codirectional or parallel to the given normal vector |
| [`GetFilter`](nodes/RhythmUI.Utilities.SelFilter.GetFilter.md) | _Not documented yet._ |
| [`GetLogicalAndFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalAndFilter%28first%2C%20second%2C%20executeAll%29.md) | Creates a logical "and" filter |
| [`GetLogicalAndFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalAndFilter%28first%2C%20filters%29.md) | Creates a logical "and" filter |
| [`GetLogicalAndFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalAndFilter%28filters%29.md) | Creates a logical "and" filter |
| [`GetLogicalNotFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalNotFilter.md) | Creates a logical "not" filter |
| [`GetLogicalOrFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalOrFilter%28first%2C%20second%2C%20executeAll%29.md) | Creates a logical "or" filter |
| [`GetLogicalOrFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalOrFilter%28first%2C%20filters%29.md) | Creates a logical "or" filter |
| [`GetLogicalOrFilter`](nodes/RhythmUI.Utilities.SelFilter.GetLogicalOrFilter%28filters%29.md) | Creates a logical "or" filter |
| [`GetPlanarFaceFilter`](nodes/RhythmUI.Utilities.SelFilter.GetPlanarFaceFilter.md) | Creates a selection filter that will let only PlanarFace-References pass |
| [`GetReferenceFilter`](nodes/RhythmUI.Utilities.SelFilter.GetReferenceFilter.md) | Creates a selection filter that will use the "filterMethod" to filter the references |
| [`Not`](nodes/RhythmUI.Utilities.SelFilter.Not.md) | Creates a logical "not" filter |
| [`Or`](nodes/RhythmUI.Utilities.SelFilter.Or.md) | Creates a logical "or" filter |

### ViewFamilyTypes

| Node | What it does |
|---|---|
| [`ViewFamilyTypes`](nodes/RhythmUI.ViewFamilyTypes.md) | Allows you to select a view family type from your file |

### ViewTemplates

| Node | What it does |
|---|---|
| [`ViewTemplates`](nodes/RhythmUI.ViewTemplates.md) | Allows you to select a view template from the instances in your project. |

### Views

| Node | What it does |
|---|---|
| [`Views++`](nodes/RhythmUI.Views.md) | Allows you to select a view from the instances in your project. |

## Core

Geometry, text, numbers and helpers, with no reliance on Revit.

### About

| Node | What it does |
|---|---|
| [`AboutRhythm`](nodes/Rhythm.About.About.AboutRhythm.md) | This is mostly to show the icon in the Dynamo 2.0 library. |

### CirclePacking

| Node | What it does |
|---|---|
| [`ByBoundary`](nodes/Rhythm.GenerativeDesign.CirclePacking.ByBoundary.md) | Packs circles within a boundary polygon using a particle-spring simulation system. |
| [`ByCentersAndRadii`](nodes/Rhythm.GenerativeDesign.CirclePacking.ByCentersAndRadii.md) | Packs circles from given starting positions using a particle-spring simulation system. |

### GenerativeDesign

| Node | What it does |
|---|---|
| [`PackViewports`](nodes/Rhythm.GenerativeDesign.GenerativeDesign.PackViewports.md) | Packs viewport rectangles within a container rectangle, ensuring no overlap with container edges |

### RandomDistribution

| Node | What it does |
|---|---|
| [`NextGaussian`](nodes/Rhythm.GenerativeDesign.RandomDistribution.NextGaussian.md) | Return a number in the range (-1, +1) with a Normal distributed probability using the Marsaglia polar method. |

### Geometry

| Node | What it does |
|---|---|
| [`LunchboxShortestWalk`](nodes/Rhythm.Geometry.Geometry.LunchboxShortestWalk.md) | Find the 'Shortest Walk' within a curve network. |

### Point

| Node | What it does |
|---|---|
| [`Deconstruct`](nodes/Rhythm.Geometry.Point.Deconstruct.md) | Return the XYZ values of a given point. |

### PolyCurve

| Node | What it does |
|---|---|
| [`ByRandomPoints`](nodes/Rhythm.Geometry.PolyCurve.ByRandomPoints.md) | Creates a PolyCurve from a random distribution of points. |

### Polygon

| Node | What it does |
|---|---|
| [`GetPolyLabel`](nodes/Rhythm.Geometry.Polygon.GetPolyLabel.md) | _Not documented yet._ |
| [`MinimumCircle`](nodes/Rhythm.Geometry.Polygon.MinimumCircle.md) | _Not documented yet._ |

### ConvexHull

| Node | What it does |
|---|---|
| [`FromPoints`](nodes/Rhythm.Geometry.Tessellation.ConvexHull.FromPoints.md) | Generates a convex hull from given points. |

### Vector

| Node | What it does |
|---|---|
| [`Direction`](nodes/Rhythm.Geometry.Vector.Direction.md) | This will determine the vector's cardinal direction (N, S, E, W, NE, NW, SE, SW). |

### Helpers.Helpers

| Node | What it does |
|---|---|
| [`ThisOrThat`](nodes/Rhythm.Helpers.Helpers.ThisOrThat.md) | This provides a toggle input to select between 2 inputs. |
| [`Toggle`](nodes/Rhythm.Helpers.Helpers.Toggle.md) | This provides a toggle based on boolean input. |

### ImportExport

| Node | What it does |
|---|---|
| [`ScreenshotMainWindow`](nodes/Rhythm.Helpers.ImportExport.ScreenshotMainWindow.md) | Creates a full screenshot of the main window. |

### Helpers.System

| Node | What it does |
|---|---|
| [`CreateResxImageFile`](nodes/Rhythm.Helpers.System.CreateResxImageFile.md) | Creates a .Resx (resources) file from a directory of images for a Dynamo package. |
| [`CurrentUserAppData`](nodes/Rhythm.Helpers.System.CurrentUserAppData.md) | This returns the appdata path for the current user. |
| [`CurrentUserDomainName`](nodes/Rhythm.Helpers.System.CurrentUserDomainName.md) | Returns the domain name of the current user. |
| [`CurrentUserName`](nodes/Rhythm.Helpers.System.CurrentUserName.md) | Returns the current windows user. |
| [`CurrentUserTempFolder`](nodes/Rhythm.Helpers.System.CurrentUserTempFolder.md) | This returns the temporary path for the current user. |
| [`JiggleMouse`](nodes/Rhythm.Helpers.System.JiggleMouse.md) | This will move your mouse back and forth slowly while toggled true. |
| [`MachineMacAddress`](nodes/Rhythm.Helpers.System.MachineMacAddress.md) | Returns the current computer's mac address. |
| [`MachineName`](nodes/Rhythm.Helpers.System.MachineName.md) | Returns the current computer name. |
| [`SendToClipboard`](nodes/Rhythm.Helpers.System.SendToClipboard.md) | Send the given string to the clipboard |

### Image

| Node | What it does |
|---|---|
| [`ConvertToBase64`](nodes/Rhythm.Image.Image.ConvertToBase64.md) | Converts an input image to a base64 (string) representation. |

### MarkovChain

| Node | What it does |
|---|---|
| [`PredictNext`](nodes/Rhythm.Math.MarkovChain.PredictNext.md) | Prediction with a markov chain |

### Numbers

| Node | What it does |
|---|---|
| [`ToHeading`](nodes/Rhythm.Numbers.Numbers.ToHeading.md) | Convert the input numbers into headings: N,S,E,W or north, east, south or west. |
| [`ToHeadingArrow`](nodes/Rhythm.Numbers.Numbers.ToHeadingArrow.md) | Convert the input numbers into headings: ↑, →, ↓, ←. Made possible with Humanizer (https://github.com/Humanizr/Humanizer) |
| [`ToOrdinalWords`](nodes/Rhythm.Numbers.Numbers.ToOrdinalWords.md) | Convert the input numbers into ordinal words. |
| [`ToRoman`](nodes/Rhythm.Numbers.Numbers.ToRoman.md) | Convert the input numbers into roman numerals. |
| [`ToWords`](nodes/Rhythm.Numbers.Numbers.ToWords.md) | Convert the input numbers into words. |

### Inspect

| Node | What it does |
|---|---|
| [`LongestCommonSubstring`](nodes/Rhythm.String.Inspect.LongestCommonSubstring.md) | Find the longest common substring between two strings. |

### Modify

| Node | What it does |
|---|---|
| [`Camelize`](nodes/Rhythm.String.Modify.Camelize.md) | Camelize behaves identically to Pascalize, except that the first character is lower case. |
| [`Dasherize`](nodes/Rhythm.String.Modify.Dasherize.md) | Underscore separates the input words with a dash. |
| [`FormatWith`](nodes/Rhythm.String.Modify.FormatWith.md) | Format input string with arguments. |
| [`Humanize`](nodes/Rhythm.String.Modify.Humanize.md) | Humanize string extensions allow you turn an otherwise computerized string into a more readable human-friendly one. |
| [`MOcKtExt`](nodes/Rhythm.String.Modify.MOcKtExt.md) | This generates a "mocking text" case. |
| [`ParseRegularExpression`](nodes/Rhythm.String.Modify.ParseRegularExpression.md) | This will run a regular expression on a a string. |
| [`Pascalize`](nodes/Rhythm.String.Modify.Pascalize.md) | Pascalize converts the input words to UpperCamelCase, also removing underscores and spaces. |
| [`Pluralize`](nodes/Rhythm.String.Modify.Pluralize.md) | This will attempt to return a plural version of a word. |
| [`Singularize`](nodes/Rhythm.String.Modify.Singularize.md) | This will attempt to return a singular version of a word. |
| [`Titleize`](nodes/Rhythm.String.Modify.Titleize.md) | Titleize converts the input words to Title casing Made possible with Humanizer (https://github.com/Humanizr/Humanizer) |
| [`ToQuantity`](nodes/Rhythm.String.Modify.ToQuantity.md) | This will attempt to return a quantity, given a string and count. |
| [`ToSentence`](nodes/Rhythm.String.Modify.ToSentence.md) | Converts the input string to a title case. |
| [`ToTitle`](nodes/Rhythm.String.Modify.ToTitle.md) | Converts the input string to a title case. |
| [`Truncate`](nodes/Rhythm.String.Modify.Truncate.md) | This will truncate the given string, byt the given length. |
| [`Underscore`](nodes/Rhythm.String.Modify.Underscore.md) | Underscore separates the input words with underscore. |

### System.System

| Node | What it does |
|---|---|
| [`Compress`](nodes/Rhythm.System.System.Compress.md) | This will compress (zip) a given directory |
