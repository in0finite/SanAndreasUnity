

# TODO

- scene changing: When network (server/client) is stopped, offline scene should be loaded. But, when switching back to online scene, Loader should not load everything again. Instead, only Cell loading should be done, if the new scene is main scene. But, are old meshes/textures destroyed ? Do we leave memory behind, every time when network is stopped ? Or... just exit the game when network is stopped (display message box first ?).

- order of buttons in main menu

- adapt states: 

- send button input events to server: 

- Cell focus is not always assigned on client - it happens when a syncvar for current ped in Player script arrives after Ped.Start()

- OutOfRangeDestroyer script should be destroyed on clients (for vehicles) ; when player controls a vehicle, it should not be destroyed ; or simply make the script destroy objects only on server (if they are network objects) ;

- "Kill all peds" button should also kill local ped ; "Destroy all vehicles" button should also destroy controlled vehicle ;

- max num players should be configurable

- add current vehicle info to "misc" stats - it will display: name, velocity, num wheels, num doors, num peds inside, input, mass, drag, rigid body state, sync interval, 

- **vehicle is bumping on clients** - disable (or destroy) wheel colliders, and sync them - this should not be done on local player, see below

- when exit vehicle anim is finished on client, it is repeated - change wrap mode

- **vehicle syncing is too laggy** - local player must control the rigid body ; server will validate position/rotation/velocity changes, and correct them if needed ; when server detects collision, or applies force to rigid body, he will override state of rigid body ;

- try to sync rigid body forces, or just clear them on clients ? - will this help ? are forces cleared at the end of frame by physics engine ? - forces can not be accessed

- add option to disable wheel colliders

- display a message to user when network is stopped



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


