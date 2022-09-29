this is a design doc for ME.

# Saving

## Save
- dict of objects
- dict of instant objects?

AND THAT IS IT. make everything else a persistent object. even the game flags

## SavedObject
#### GetProperty
1. use the same as GetProperty<T> but try with a cast conversion
2. then catch any error and use `typeof(T).Equals(key.GetType())`

#### Save Hooks
- OnPreSave
- OnPostSave


## InstantSavedObject: SavedObject
this would be...speedrun timer, instant game flags?
- OnDestroy (called when unloaded)

## MapFog
- save these directly to the folder as a png or something. don't embed them in the save, that's dumb
- also...make the loading of said png happen async
- https://www.raywenderlich.com/26799311-introduction-to-asynchronous-programming-in-unity
- also use a cancellation token

## SaveContainer
have only one for runtime, and make it contain a single save reference

### Checking for Disk Load
1. if it's present on disk, and NOT editor, set the current save to It
2. if it's not present, who cares

## GameCheckpoint
- list of items
- list of game flags
- list of previous game checkpoints
### On Enable
1. add items and game flags silently
2. for every previous game checkpoint, call OnEnable as well

# Transitions

## TransitionManager
just take whatever was in GNASH. who cares, maybe add the subway shit

## LevelInfo
sceneData but actually useful
have the transition manager look for one and read it, you know
