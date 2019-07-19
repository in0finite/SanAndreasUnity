
## Weapons/damage TODO
<br>



### Weapons

- play separate anim for tec9 ?

- implement reloading

- when lerping upper arm transform toward ped forward vector, arm is going on the opposite side - it seems that it depends on world rotation of ped

- anim for minigun and flame thrower can't be loaded


### Damage system

- particles on place of hit

- display inflicted damage as on-screen message

- die animation - detach ped model before destroying ped ; send rpc to players to ~~do the same~~ create ped model only (not ped), and play anim ;

- we need to raycast against ped mesh, not capsule collider ; also need to detect which part of body was hit

- adapt vehicles to damage system - need explosion and smoke effects first

- decals


