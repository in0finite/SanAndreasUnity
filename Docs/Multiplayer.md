

# What needs to be done before starting work on multiplayer

- create windows for: start new game ; join game ;

- all scripts should work when Camera.main is null

- killing local ped

- scene changing: When network (server/client) is stopped, offline scene should be loaded. But, when switching back to online scene, Loader should not load everything again. Instead, only Cell loading should be done, if the new scene is main scene. But, are old meshes/textures destroyed ? Do we leave memory behind, every time when network is stopped ? Or... just exit the game when network is stopped (display message box first ?).

- add OK button to message boxes

- player's ped must be created by server (when host starts) - it should be removed from scenes



# Potential problems

- server will have multiple Cell focus points - the game can lag too much, so server has to run on a dedicated machine



# Scripts that need to be synced across network


## Ped

variables:

- id
- transform
- is walking / running / sprinting
- heading
- is aiming
- is firing
- current weapon
- current vehicle - not needed - synced in Vehicle
- aim direction

events:

- fire
- enter/exit vehicle
- jump



## Vehicle

variables:

- id
- transform
- car colors
- linear velocity
- angular velocity
- steering angle
- acceleration
- brake
- peds which occupy seats



## Weapon

variables:

- id
- ammo in clip
- ammo outside of clip
- ped owner


