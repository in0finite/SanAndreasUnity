# San Andreas Unity

We're porting [GTA: San Andreas](http://www.rockstargames.com/sanandreas/) to Unity!

This won't be a complete re-implementation of the game, but we're hoping to build something similar to [Multi Theft Auto](http://www.mtasa.com/) with assets streamed from an existing installation of San Andreas.

## Setup Instructions

Before starting the game, create/edit config.user.json inside project folder to specify path to gta.

Also, after building binary, you need to copy 'Data' folder to 'game_name'_Data folder. The script which should do that, doesn't work right now.

[![CodeFactor](https://www.codefactor.io/repository/github/z3nth10n/sanandreasunity/badge/master)](https://www.codefactor.io/repository/github/z3nth10n/sanandreasunity) [![GitHub license](https://img.shields.io/github/license/z3nth10n/SanAndreasUnity.svg)](https://github.com/z3nth10n/SanAndreasUnity/blob/master/LICENSE) [![GitHub issues](https://img.shields.io/github/issues/z3nth10n/SanAndreasUnity.svg)](https://github.com/z3nth10n/SanAndreasUnity/issues) ![GitHub closed issues](https://img.shields.io/github/issues-closed-raw/z3nth10n/sanandreasunity.svg)  [![GitHub forks](https://img.shields.io/github/forks/z3nth10n/SanAndreasUnity.svg)](https://github.com/z3nth10n/SanAndreasUnity/network)  ![GitHub contributors](https://img.shields.io/github/contributors/https://img.shields.io/github/issues-closed-raw/z3nth10n/sanandreasunity.svg.svg) 

### Misc Stats

![GitHub top language](https://img.shields.io/github/languages/top/z3nth10n/sanandreasunity.svg)  ![Github search hit counter](https://img.shields.io/github/search/z3nth10n/sanandreasunity/goto.svg) ![GitHub language count](https://img.shields.io/github/languages/count/z3nth10n/sanandreasunity.svg)  ![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/z3nth10n/sanandreasunity.svg) ![GitHub repo size in bytes](https://img.shields.io/github/repo-size/z3nth10n/sanandreasunity.svg)

### Commit & issues & pull requests coverage

![Bitbucket open pull requests](https://img.shields.io/bitbucket/pr/z3nth10n/sanandreasunity.svg) ![GitHub issue age](https://img.shields.io/github/issues/detail/age/z3nth10n/sanandreasunity/979.svg) ![GitHub commit activity the past week, 4 weeks, year](https://img.shields.io/github/commit-activity/y/z3nth10n/sanandreasunity.svg)  ![GitHub last commit](https://img.shields.io/github/last-commit/z3nth10n/sanandreasunity.svg) ![GitHub last commit (branch)](https://img.shields.io/github/last-commit/z3nth10n/sanandreasunity/infra/config.svg)

### Some social networks buttons...

![GitHub stars](https://img.shields.io/github/stars/z3nth10n/sanandreasunity.svg?style=social&label=Stars) ![GitHub forks](https://img.shields.io/github/forks/z3nth10n/sanandreasunity.svg?style=social&label=Fork) ![GitHub watchers](https://img.shields.io/github/watchers/z3nth10n/sanandreasunity.svg?style=social&label=Watch) ![GitHub followers](https://img.shields.io/github/followers/espadrine.svg?style=social&label=Follow) [![Twitter](https://img.shields.io/twitter/url/https/github.com/z3nth10n/SanAndreasUnity.svg?style=social)](https://twitter.com/intent/tweet?text=Look this incredible project!&url=https%3A%2F%2Fgithub.com%2Fz3nth10n%2FSanAndreasUnity) ![Twitter Follow](https://img.shields.io/twitter/follow/espadrine.svg?style=social&label=Follow)


Coming soon: Release downloads


## In game controls

V - spawn vehicle

P - change pedestrian model

L - turn off / on car lights

L shift - run / fly fast

mouse scroll - zoom in/out

F1 - toggle debug menu

F2 - Open console

T - enable debug flying mode

R - enable debug noclip mode

backspace - fly up

delete - fly down

Z - fly very fast

E - Enter/exit vehicles

Space - Jump

F10 - Toggle FPS

F9 - Toggle velocimeter

O - Toggle quality

## CLI

* --handlelog // --h => Enables log handling (in HTML)

* --console // --c => Display a console to see what is Unity outputting

* --stopdebug // --s => Stop debug when reached x messages

* --html => Output log in html instead of text

## Here is a list of what I think should be done next

* Animations must be loaded/played by index – because different anim definition groups (man, woman, etc) use different anim names ( walk_civi, woman_walknorm, respectively).

* Weapons. Aiming with weapons – aim animation.

* What else can be imported: ai paths, character spawn info, item pickups, audio, 

* Rigid body character.

* Airplane.

* Load map in editor.

* Async geometry loading.

* Make everything networked.

## TODO

### Features

- Weapons

- Two driving camera modes (aim to front and free look)

- Multiple car cameras: exterior (3 distance switches), interior & cinematic

- See the entire map (pressing M)

- Work on a menu like MTA:SA (F1)

- Dev profiles (for paths) and for the keyboard & mouse controls

- Health, stats (stamina, stength), money & armor system

- AI System for Peds

- Peds (and worker peds)

- Cars can be damaged

- Cars & fuel stations can explode

- Wheel explosion (I have to convert colliders, from a capsule collider generate a mesh collider with the shape of a capsule)

- In-game input manager

- Flip & repair cars

- CLI for Console app to know where to connect

- Collapse message logs

- Maybe I will use my sockets implementation to do a server system (I will made a paid assets with [examples from Unity Forums](https://github.com/ZZona-Dummies/UnityForumsMultiplayer)

### Effects & Enhancements

* Jump, swim & fall animations

* Water effects (Swim, bouyancy, get darker and blurry the deeper you get)

* Work on props (lampposts, fences, etc)

* Decal system for Weapons

* Burnout trace

* Vehicle damage

* Dust and water particles with vehicles (cars travelling through dirt and boats, respectively)

* Stars, clouds and enviroment enhances

### Environment

* Weather

### Vehicles

* Enhance car lights, turning, doors and braking

* Gear system (to make cars keep stoped on slopes)

* Boats, airplanes and bikes (+ fall)

### Mods system

* Integrate a mod system that is capable of reading source files and managed compiled assemblies, some exaples of mods:
    - Access to all interiors (without any yellow marker) generating them
    - Speedometer and rpm meter (like FO2) + fuel
    - GMOD physics and prop menu
    - Map editor
    - Ragdoll effects (like GTA IV) when falling and dying
    - Broke car windshield on hard impact
    - Animals (mobs)
    - Terrain modificable through digging & explosions
    - Territory (gan wars) on Multiplayer
    - Integration of UltraGTA

### Bugs to fix

- Weird circular shadow appears when drving and the car passes next to a building that projects a shadow (low quality)

- Car moves when the player is still getting inside it

- Auto-zoom with big vehicles

- Some cars have its suppension to low to allow them to move

- Cars sometimes spawn under other cars, this makes that the cars jumps

- Make cars spawn appropiate for each zone

- When vehicles are damanged light goes weird

- Once vehicles are despawned they don't re-spawn

- In some cases damages to vehicles aren't performed at first collision

- When console is opened cursor ticks

- If you put the same close & open key for console, it never closes

- Colors for debug messages in console

- HTML (from logger) indent is not perfect

- Console Application is not launching on build (CLI was checked and work properly)

- Sometimes in old gpus, all goes black, like for example here (https://i.gyazo.com/b3a682b86ab0808ca132bad803194cab.mp4) the way of fixing this is running Unity Editor undr `--force-glcore` and going to **Assets > Reimport all**

### Leaks

- With the new car light system batches has aumented 

### Inneficiencies

- Curent socket system only allows messages of 8KBs

- Log extension is printed in hexadecimal on Sublime Text (change extension)

### Must be reviewed

- If you regenerate scripts while running Unity Editor crashes (patch it?)

- Find in the project for "me" keywords and delete them

- Running with `--force-glcore` makes Unity Editor slower when near objects has to be rendered

- I think that car light break system doesn't work fine

- HUGE REFACTOR (compiling Assembly into DLLs will solve most problems)

## Media

![screen shot 2017-04-01 at 00 01 31](https://cloud.githubusercontent.com/assets/557828/24571347/d95f11a0-1670-11e7-9e8e-d2a511d9f929.png)

![screen shot 2017-04-01 at 00 03 25](https://cloud.githubusercontent.com/assets/557828/24571348/d964f098-1670-11e7-8759-0160dbf5bcb5.png)

![screen shot 2017-04-01 at 00 02 13](https://cloud.githubusercontent.com/assets/557828/24571349/d96b7c24-1670-11e7-997d-ae15913481f8.png)

## Resources

* [GTAModding Wiki](http://www.gtamodding.com/wiki/Main_Page)

