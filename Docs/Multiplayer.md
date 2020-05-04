

# Multiplayer TODO


### General

- scene changing: When network (server/client) is stopped, offline scene should be loaded. But, when switching back to online scene, Loader should not load everything again. Instead, only Cell loading should be done, if the new scene is main scene. But, are old meshes/textures destroyed ? Do we leave memory behind, every time when network is stopped ? Or... just exit the game when network is stopped (display message box first ?).

- error when spawning RCCAM

- all button events' Cmds on server should be enclosed with F.RunExceptionSafe()


### Vehicles

- **vehicle is bumping on clients** - disable (or destroy) wheel colliders, and sync them - this should not be done on local player, see below

- when exit vehicle anim is finished on client, it is repeated - change wrap mode

- **vehicle syncing is too laggy** - local player must control the rigid body ; server will validate position/rotation/velocity changes, and correct them if needed ; when server detects collision, or applies force to rigid body, he will override state of rigid body ;

- try to sync rigid body forces, or just clear them on clients ? - will this help ? are forces cleared at the end of frame by physics engine ? - forces can not be accessed

- add option to disable wheel colliders



