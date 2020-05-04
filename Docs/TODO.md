
## TODO


- Multiplayer - see [Multiplayer.md](Multiplayer.md)

- **Async geometry loading** - currently, geometry is loaded synchronously, which seems like the main performance bottleneck ; see [AsyncAssetLoading.md](AsyncAssetLoading.md) ;

- Weapons and damage system - see [weapons.md](weapons.md)

- Unloading assets - need to keep track of references to loaded assets (meshes, textures) ; need ability to release references ; when a certain asset is no longer referenced, it can be unloaded ;

- Unload distant parts of the world - first need a better Cell system, which will tell which divisions are close and which are not ; unloading includes releasing reference to meshes, textures, materials, collision models ;

- Rigid body character

- Minimap - better info area ; input mouse position is not always correct ; see [Map.md](Map.md) ;


- Does 'O' button changes quality level ?

- Check what is causing huge FPS drops ; it wasn't present before ? ; possibly it is GC ;

- Play sounds: horn ; empty weapon clip ; ped damage ; footsteps in run and sprint states ;

- Optimize Console.Update() - don't do trim excess for every log message

- don't fade high LOD meshes

- Validate path to GTA ?

- Vehicles window: it's too slow - use pages ; display additional info ;

- Non-working ped model ids: WMYST, 0, special peds at the end, 

- Create custom inspector for ped - it will display info from ped definition

- Pin windows - pinned windows are visible even when pause menu is not

- Remove unneeded assets: files from Resources, 

- Breakable objects - they should have a separate class which inherits MapObject ; spawn networked object with rigid body, when hit with enough force ;

- Android: check Utilities size on 800x480 resolution ;


- Navigation: build navmesh from static geometry at runtime


#### Vehicles

- Implement other vehicles: airplanes, helicopters, bikes, boats

- Wheels should be excluded from damage effects

- Repair cars with key

