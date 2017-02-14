# San Andreas Unity

We're porting [GTA: San Andreas](http://www.rockstargames.com/sanandreas/) to Unity!

This won't be a complete re-implementation of the game, but we're hoping to build something similar to [Multi Theft Auto](http://www.mtasa.com/) with assets streamed from an existing installation of San Andreas.

## Setup Instructions

Before starting the game, create/edit config.user.json inside project folder to specify path to gta.

Also, after building binary, you need to copy 'Data' folder to 'game_name'_Data folder. The script which should do that, doesn't work right now.

## In game controls

V - spawn vehicle

P - change pedestrian model

L shift - run

mouse scroll - zoom in/out

## Here is a list of what I think should be done next

* Animations must be loaded/played by index – because different anim definition groups (man, woman, etc) use different anim names ( walk_civi, woman_walknorm, respectively).

* Weapons. Aiming with weapons – aim animation.

* What else can be imported: ai paths, character spawn info, item pickups, audio, 

* Rigid body character.

* Airplane.

* Load map in editor.

* Async geometry loading.

* Make everything networked.


## Media

![](/media/ak47.png)
![](/media/driving.png)
![](/media/lasventuras.png)
![](/media/modelviewer.png)
![](/media/vehicles.png)

### First networking test

[![First networking test](http://files.facepunch.com/ziks/2015/April/12/vidthumb1.png)](http://files.facepunch.com/ziks/2015/April/12/2015-04-12-2011-02.mp4)

### Basic car physics

[![Basic car physics](http://files.facepunch.com/ziks/2015/April/12/vidthumb2.png)](http://files.facepunch.com/layla/2015/April/06/2015-04-06_04-32-12.mp4)

## Resources

* [GTAModding Wiki](http://www.gtamodding.com/wiki/Main_Page)

