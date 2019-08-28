
## TODO


- Multiplayer - see [Multiplayer.md](Multiplayer.md)

- **Async geometry loading** - currently, geometry is loaded synchronously, which seems like the main performance bottleneck ; see [AsyncAssetLoading.md](AsyncAssetLoading.md) ;

- Weapons and damage system - see [weapons.md](weapons.md)

- Unloading assets - need to keep track of references to loaded assets (meshes, textures) ; need ability to release references ; when a certain asset is no longer referenced, it can be unloaded ;

- Unload distant parts of the world - first need a better Cell system, which will tell which divisions are close and which are not ; unloading includes releasing reference to meshes, textures, materials, collision models ;

- Rigid body character

- Minimap - better info area ; input mouse position is not always correct ; see [Map.md](Map.md) ;

- Teleport : when ground is too far away (like on mountains), geometry around it will not be loaded, and raycast will not succeed ; when position is too low, player gets constantly respawned ; adapt all other teleport code ;


- Gravity setting failed to load on windows - instead of 9.81, it's loaded as 981 - maybe it happens when float.ToString() gives e

- Does 'O' button changes quality level ?

- Restore physics update rate to 50

- Chat

- Remove unused packages added by Unity

- Check what is causing huge FPS drops - it wasn't present before ; possibly it is GC ;

- Android: check Utilities size on 800x480 resolution ;

- Touch input: 

- Play sounds: horn ; empty weapon slot ; ped damage ; footsteps in run and sprint states ;

- Optimize Console.Update() - don't do trim excess for every log message

- don't fade high LOD meshes

- Validate path to GTA ?

- Split code into separate assemblies (using asmdef files)

- When raycasting with weapons, don't use ped's character collider, but use his mesh

- Vehicles window: it's too slow - use pages ; display additional info ;

- Non-working ped model ids: WMYST, 0, special peds at the end, 

- Create custom inspector for ped - it will display info from ped definition

- Pin windows - pinned windows are visible even when pause menu is not

- Remove unneeded assets: files from Resources, 

- Minimap size should depend on screen resolution

- Breakable objects - they should have a separate class which inherits MapObject ; spawn networked object with rigid body, when hit with enough force ;


- Navigation: build navmesh from static geometry at runtime

- Import: AI paths, ped spawn info, pickups, font, 

- In-game input settings

- Update audio library ; Don't use separate file for weapon sound timings ;


#### Vehicles

- Implement drive-by (free aiming/shooting from vehicle)

- Vehicle hijacking - throwing out other ped and entering vehicle at his seat

- Adapt to damage system (so that they can be damaged and destroyed)

- Implement other vehicles: airplanes, helicopters, bikes, boats

- Play radio stations

- Wheels should be excluded from damage effects

- Repair cars with key

