## TODO

### Features

- Animations must be loaded/played by index – because different anim definition groups (man, woman, etc) use different anim names ( walk_civi, woman_walknorm, respectively).

- **Weapons. Aiming with weapons – aim animation. - [See weapons.md](weapons.md)**

- Rigidbody character

- Load map in editor

- **Async geometry loading - currently, geometry is loaded synchronously, which seems like the main performance bottleneck**

- Make everything networked

- **Map - better info area ; input mouse position is not always correct ; see Map.md ; **

- Teleport : when ground is too far away (like on mountains), geometry around it will not be loaded, and raycast will not succeed ; when position is too low, player gets constantly respawned ; adapt all other teleport code ;

- Ped should be rotated only around Y axis

- Vehicles window: it's too slow - use pages ; display additional info ;

- Jump ??

- Remove editor scripts for destroying player model

- Work on a menu like MTA:SA (F1)

- Create custom profiles for keyboard & mouse controls

- Health, stats (stamina, stength), money & armor system - no need for this until weapons are implemented

- Two driving camera modes (aim to front and free look)

- Multiple car cameras: exterior (3 distance switches), interior & cinematic

- AI System for Peds

- Peds (and peds insides banks, sex shops, ammunations, bars/restaurants, officines, workshops, etc...)

- Import: AI paths, character spawn info, item pickups, 

- Cars & fuel stations can explode

- Other vehicles: airplanes, helicopters, bikes, boats (Álvaro)

- Wheel explosion (we have to convert colliders, from a capsule collider generate a mesh collider with the shape of a capsule) (Álvaro)

- In-game input manager

- Repair cars with key

- CLI parameter for Console app to know where to connect (an IP)

- Read sounds from GTA SA Streams: http://www.lysator.liu.se/~creideiki/radio-free-san-andreas/

- Make a description on a MD file explaining this options: https://i.gyazo.com/07490f1d389fb3c4d6363e8d9810c0c1.png

- Implement scrolled map zoom (https://stackoverflow.com/questions/10694397/how-to-zoom-in-using-mouse-position-on-that-image)

### Effects & Enhancements

#### Player

* Jump, swim & fall animations (fall maybe reemplaced with rigidbody physics, when there is a big jump)

- Synchronize jump & walk events (you can jump while running, isn't realistic)

- Better velocimeter for Cars & Player

#### Weapons

* Decal system for Weapons

#### Environment

* Weather system

* Dust and water particles with vehicles (cars travelling through dirt and boats, respectively)

* Stars, clouds and enviroment enhances

* Water effects (Swim, bouyancy, get darker and blurry the deeper you get)

* Work on props (lamp-posts, fences, etc)

#### Vehicles

* Enhance car lights, turning, suspensions doors and braking

* Burnout trace

* Vehicle damage (finish detachments, wheels, doors (break and detach) and so on)

- Speed effect in cars
    
### Bugs to fix

- **Why is the sky always black ? - check quality settings** (This isn't a bug, this must be enhanced, because the sky draws black when it's night)

- Car lights can't be turned off (Alvaro has to review it, because, now, only one light is turned off)

- NRE is thrown when no scene is opened

- Improve profile devs (delete ids because now it's on config.user.json)

- Have to fix this problem, modifying somehow Assembly Importer GUI to add to mark or something like that is saved to avoid it's compilation (https://cdn.discordapp.com/attachments/454006273751515163/455029337821806592/unknown.png) - It's fixed, but now the inspector looks ugly (this is for the Socket console)

- Weird circular shadow appears when driving and the car passes next to a building that projects a shadow (Quality Settings: Simple)

- Car moves when the player is still getting inside it

- Auto-zoom with big vehicles

- Some cars have its suppension too low to allow them to move (Reming is an example):

![...](https://i.gyazo.com/1cec15e93e255c13ce63818bed46d40d.png)

- Cars sometimes spawn under other cars, which causes those cars to jump

- Make cars spawn in an appropiated zone (Tanks in LS?)

- When vehicles are damaged, light from materials get darken

- Once vehicles are despawned they don't re-spawn

- In some cases damages to vehicles aren't performed at first collision

- Colors for debug messages in console

- HTML (from logger) indent is not perfect

- Console Application is not launching on build (CLI was checked and works properly)

- Sometimes in old gpus, all goes black, like for example here (https://i.gyazo.com/b3a682b86ab0808ca132bad803194cab.mp4) the way of fixing this is running Unity Editor under `--force-glcore` and going to *Assets > Reimport all*

- When car collides lights doesn't break as expected

- Cars stop when they are falling:

![...](https://cdn.discordapp.com/attachments/454006273751515163/468968463302524928/unknown.png)

### Leaks

**Nothing. :)**

### Inneficiencies

- Current socket system only allows messages of 8KBs

- Files with .log extension is printed in hexadecimal on Sublime Text (change extension)

- Implement polar rotation system for player camera

```
elevation -= Input.GetAxis("Mouse Y");
polar = Mathf.Clamp(polar + Input.GetAxis("Mouse X"), -90.0f, 90.0f);
while (elevation < 0.0f)
{
    elevation += 360.0f;
}
while (elevation >= 360.0f)
{
    elevation -= 360.0f;
}
transform.rotation = Quaternion.AngleAxis(polar, Vector3.up) * Quaternion.AngleAxis(elevation, Vector3.right);
```

- Read all radar{xx}.txd that are available (so, we can read an GTA:SA modified version)

### Must be reviewed

- Check Profiler to see what is taking performance (in most cases is Editor)

- If you regenerate scripts while running, Unity Editor crashes (patch it?)

- Running with `--force-glcore` makes Unity Editor slower when near objects have to be rendered

- **Minimap size should depend on screen resolution**

- Check all `//Must review` in code
