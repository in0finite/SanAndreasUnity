
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

- Reduce physics time step to 30 - make it configurable in options

- Chat

- Android: HUD must run before other UI scripts ; add perms for read/write access to storage ; forbid screen rotation ; disable left and right mouse click ; vehicle touch input: forward, backward, handbrake, left & right ; weapon switching buttons ; circular button for movement ; lock cursor when testing finishes ; must use new UI system ; don't report mouse move input while movement button is being pressed ; remove HUD code from state class ; event system should have SEO defined ;

- Play sounds: horn ; empty weapon slot ; ped damage ; footsteps in run and sprint states ;

- Script execution order: HUD before pause menu and windows ; fps counter after all ;

- Optimize Console.Update() - don't do trim excess for every log message

- Implement drive-by

- don't fade high LOD meshes

- Validate path to GTA ?

- Split code into separate assemblies (using asmdef files)

- When raycasting with weapons, don't use ped's character collider, but use his mesh

- Vehicles window: it's too slow - use pages ; display additional info ;

- Non-working ped model ids: WMYST, 0, special peds at the end, 

- Create custom inspector for ped - it will display info from ped definition

- Pin windows - pinned windows are visible even when pause menu is not

- Remove unneeded assets: files from Resources, 

- Bug when ped gets down to low heights (trying to move him back to starting location, and causing shaking)

- Minimap size should depend on screen resolution

- Vehicle hijacking - throwing out other ped and entering vehicle at his seat


- Import Vice City

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

