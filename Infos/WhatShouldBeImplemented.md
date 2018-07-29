
## TODO

### Features

- Animations must be loaded/played by index – because different anim definition groups (man, woman, etc) use different anim names ( walk_civi, woman_walknorm, respectively).

- **Weapons. Aiming with weapons – aim animation. - See weapons.md**

- Rigid body character

- Load map in editor

- **Async geometry loading - currently, geometry is loaded synchronously, which seems like the main performance bottleneck**

- Make everything networked

- **Map - better info area ; input mouse position is not always correct ; see Map.md ; **

- Teleport : when ground is too far away (like on mountains), geometry around it will not be loaded, and raycast will not succeed ; when position is too low, player gets constantly respawned ; adapt all other teleport code ;

- Vehicle entering/exiting must be bullet-proof

- VehicleController should give input only if vehicle is controlled by local player ;

- Ped window: spawn another ped (rotation is not corrected), display info about current ped, kill all peds, 

- Vehicles window: it's too slow - use pages ; display additional info ;

- Jump

- Remove editor scripts for destroying player model

- Work on a menu like MTA:SA (F1)

- Dev profiles for the keyboard & mouse controls - ??

- Health, stats (stamina, stength), money & armor system - no need for this until weapons are implemented

- Two driving camera modes (aim to front and free look)

- Multiple car cameras: exterior (3 distance switches), interior & cinematic

- AI System for Peds

- Peds (and worker peds) - ??

- Import: AI paths, character spawn info, item pickups, 

- Cars & fuel stations can explode

- Other vehicles: airplanes, helicopters, bikes, boats

- Wheel explosion (we have to convert colliders, from a capsule collider generate a mesh collider with the shape of a capsule)

- In-game input manager

- Repair cars with key

- CLI parameter for Console app to know where to connect (an IP)

- Collapse message logs

- Read sounds from GTA SA Streams: http://www.lysator.liu.se/~creideiki/radio-free-san-andreas/

- Make a paragraph for those categories: https://i.gyazo.com/07490f1d389fb3c4d6363e8d9810c0c1.png - ??

- Implement map zoom (https://stackoverflow.com/questions/10694397/how-to-zoom-in-using-mouse-position-on-that-image)

### Effects & Enhancements

* Jump, swim & fall animations (fall maybe reemplaced with rigidbody physics, when there is a big jump)

* Water effects (Swim, bouyancy, get darker and blurry the deeper you get)

* Work on props (lampposts, fences, etc)

* Decal system for Weapons

* Burnout trace

* Vehicle damage

* Dust and water particles with vehicles (cars travelling through dirt and boats, respectively)

* Stars, clouds and enviroment enhances

- Speed effect

### Environment

* Weather system

### Vehicles

* Enhance car lights, turning, doors and braking
    
### Bugs to fix

- **Why is the sky always black ? - check quality settings**

- car lights can't be turned off

- blinkers are not working correctly

- NRE is thrown when no scene is opened

- Delete profile devs

- Read all radarxx.txd that are available

- Have to fix this problem, modifying somehow Assembly Importer GUI to add to mark or something like that is saved to avoid it's compilation (https://cdn.discordapp.com/attachments/454006273751515163/455029337821806592/unknown.png) - It's fixed, but now the inspector looks ugly - ??

- Weird circular shadow appears when drving and the car passes next to a building that projects a shadow (low quality)

- Car moves when the player is still getting inside it

- Auto-zoom with big vehicles

- Some cars have its suppension too low to allow them to move

- Cars sometimes spawn under other cars, which causes those cars to jump

- Make cars spawn in their zone

- When vehicles are damaged, light goes weird

- Once vehicles are despawned they don't re-spawn

- In some cases damages to vehicles aren't performed at first collision

- Colors for debug messages in console

- HTML (from logger) indent is not perfect

- Console Application is not launching on build (CLI was checked and works properly)

- Sometimes in old gpus, all goes black, like for example here (https://i.gyazo.com/b3a682b86ab0808ca132bad803194cab.mp4) the way of fixing this is running Unity Editor under `--force-glcore` and going to *Assets > Reimport all*

- Cars stop when they are falling:

![...](https://cdn.discordapp.com/attachments/454006273751515163/468968463302524928/unknown.png)

### Leaks

- Lag when instantiating new lights

### Inneficiencies

- Curent socket system only allows messages of 8KBs

- Files with .log extension is printed in hexadecimal on Sublime Text (change extension)

- The scripts must read infinite radarXX.txd textures

- Implement polar rotation system for camera

### Must be reviewed

- Check Profiler to see what is taking performance

- If you regenerate scripts while running, Unity Editor crashes (patch it?)

- Running with `--force-glcore` makes Unity Editor slower when near objects have to be rendered

- Car break light system doesn't work fine ?

- HUGE REFACTOR (compiling Assembly into DLLs will solve most problems) - what problems ??

- **Minimap size should depend on screen resolution**