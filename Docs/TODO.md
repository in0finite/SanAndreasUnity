
## TODO


- Weapons - see [weapons.md](weapons.md)

- **Rigid body character**

- Load map in editor ?

- **Async geometry loading** - currently, geometry is loaded synchronously, which seems like the main performance bottleneck

- Make everything networked

- Map - better info area ; input mouse position is not always correct ; see [Map.md](Map.md) ;

- Teleport : when ground is too far away (like on mountains), geometry around it will not be loaded, and raycast will not succeed ; when position is too low, player gets constantly respawned ; adapt all other teleport code ;

- **Ped state machine**: update controls window ; delete unused prefab ;

- Add option to change gravity

- don't fade high LOD meshes

- Validate path to GTA ?

- Anims must be played by their name

- Vehicles window: it's too slow - use pages ; display additional info ;

- Non-working ped model ids: WMYST, 0, special peds at the end, 

- Create custom inspector for ped - it will display info from ped definition

- Pin windows - pinned windows are visible even when pause menu is not

- Remove unneeded assets: files from Resources, 

- Bug when ped gets down to low heights (trying to move him back to starting location, and causing shaking)

- Minimap size should depend on screen resolution


- Navigation: build navmesh from static geometry at runtime

- Import: AI paths, ped spawn info, item pickups, 

- Implement other vehicles: airplanes, helicopters, bikes, boats

- In-game input settings

- Update audio library ; Don't use separate file for weapon sound timings ;


#### Vehicles

- Adapt to damage system (so that they can be damaged and destroyed)

- Wheels should be excluded from damage effects

- Remove car blinkers and associated shaders

- Some cars have its suspension too low to allow them to move

- Car lights can't be turned off

- Blinkers are not working correctly

- In some cases damage to vehicles isn't performed at first collision

- Repair cars with key

