

# TODO

- scene changing: When network (server/client) is stopped, offline scene should be loaded. But, when switching back to online scene, Loader should not load everything again. Instead, only Cell loading should be done, if the new scene is main scene. But, are old meshes/textures destroyed ? Do we leave memory behind, every time when network is stopped ? Or... just exit the game when network is stopped (display message box first ?).

- add ability for client to request: 

- if ped model is changed while sitting in vehicle as a passenger, anim is set to idle

- roll state: client doesn't know the direction of rolling ; when doing more rolls one after another, client doesn't play anim anymore ; sometimes rolling continues even if WASD keys are not pressed ;

- shooting is inaccurate - clients should send fire event with fire pos and fire dir

- remove spamming logs: 

- sometimes, current vehicle is null on client

- vehicle states should handle situation when current vehicle is null (on client)

- play horn sound ?

***

- weapon sound should be 3d

***

- **vehicle is bumping on clients** - disable (or destroy) wheel colliders, and sync them - this should not be done on local player, see below

- when exit vehicle anim is finished on client, it is repeated - change wrap mode

- **vehicle syncing is too laggy** - local player must control the rigid body ; server will validate position/rotation/velocity changes, and correct them if needed ; when server detects collision, or applies force to rigid body, he will override state of rigid body ;

- try to sync rigid body forces, or just clear them on clients ? - will this help ? are forces cleared at the end of frame by physics engine ? - forces can not be accessed

- add option to disable wheel colliders




# Notes

- server will have multiple Cell focus points - the game can lag too much, so server has to run on a dedicated machine

- dedicated server doesn't need to load textures



