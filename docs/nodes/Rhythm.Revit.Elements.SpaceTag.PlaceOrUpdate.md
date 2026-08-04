## In Depth

`SpaceTag.PlaceOrUpdate(view, space, location: Rhythm.Utilities.MiscUtils.GetNull(), tagType: Rhythm.Utilities.MiscUtils.GetNull(), tryUpdateExisting: false)`

Place or update an existing space tag.

The inputs are:

- `view` (_FloorPlanView_) — Tags are view specific. This is the specific view to use.
- `space` (_Element_) — The space to be tagged.
- `location` (_Point_, defaults to `Rhythm.Utilities.MiscUtils.GetNull()`) — The location to place the tag. If null, this will place on space origin.
- `tagType` (_FamilyType_, defaults to `Rhythm.Utilities.MiscUtils.GetNull()`) — The tag type to use. If null, the default one is used
- `tryUpdateExisting` (_boolean_, defaults to `false`) — Toggle to true to try and update existing space tags in the view.

Returns `spaceTag` (_Element_) — The new or updated space tag.

Found in the library under **Actions**.
