

# TODO

- scene changing: When network (server/client) is stopped, offline scene should be loaded. But, when switching back to online scene, Loader should not load everything again. Instead, only Cell loading should be done, if the new scene is main scene. But, are old meshes/textures destroyed ? Do we leave memory behind, every time when network is stopped ? Or... just exit the game when network is stopped (display message box first ?).

- display a message to user when network is stopped

- order of buttons in main menu

- adapt states: 

- send button input events to server: 

- add ability for client to request: destroy my vehicles ; spawn stalker ped ;

- port the whole UI to multiplayer


- add option to remove current weapon - for testing

- sync health

- gun flash doesn't work on client ?

- weapon sound should be 3d

- prevent tick rate override by Mirror


- **vehicle is bumping on clients** - disable (or destroy) wheel colliders, and sync them - this should not be done on local player, see below

- when exit vehicle anim is finished on client, it is repeated - change wrap mode

- **vehicle syncing is too laggy** - local player must control the rigid body ; server will validate position/rotation/velocity changes, and correct them if needed ; when server detects collision, or applies force to rigid body, he will override state of rigid body ;

- try to sync rigid body forces, or just clear them on clients ? - will this help ? are forces cleared at the end of frame by physics engine ? - forces can not be accessed

- add option to disable wheel colliders



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


