

# What needs to be done before starting work on multiplayer

- create windows for: start new game ; join game ;

- all scripts should work when Camera.main is null

- killing local ped

- message boxes ; windows should be destroyable

- improve console

- update Unity to 2018.3



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


