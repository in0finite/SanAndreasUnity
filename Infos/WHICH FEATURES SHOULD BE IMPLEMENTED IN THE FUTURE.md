This contains a little list with the features I think should be implemented on the future given its duration on its implementation:

### Vehicles

* Gear system (to make cars keep stoped on slopes)

* Airplanes interiors

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
    - Some game crossover:
        - 7DTD / Unturned (survival, crafting, building, advanced zombies AI, looting, interior random generation, skills, money, buffs / broken bones)
        - GMOD (models, physics, gamemodes, modding)
        - MC (cave generation, mining...)
        - UltraGTA
        
### Gamemodes

- Survival
- Deathmatch
- Sandbox
- Battleroyale

### Survival

- Broken bones

- Buffs (cold, hot, poison, etc... Look at Terraria)

- Gore

### Physics

- Building with physics (explosion / collision damages)

- Damage for airplanes (from its interiors and from outside)

### Environment

- Peds take flights in airplanes (and you can kill them inside an airplane)

- Make submarine and the two boats from SF driveable (maybe in a gamemode you need some skills to drive them)

### More platforms supported

- We have to work on a system that will allow to pack GTA SA game contents into a new platform. I was thinking in something that isn't illegal. It's easy to understand, we have a client application on PC (with a legal copy of the game) and a mobile application that scan some QR code or whatever from this client app. With a validation on the QR code, now, we can pack and copy the game contents to the new platform (mobile, ps or xbox...)

- To pack the content we can use an QR code system to pass in JWT or Base64 the data we need.

### Master server system

- I have been thinking that the server system would be done like the MTA one, an non-official remote server and an official room list taken from those remote server. Or maybe like Terraria system, a client that host the entire server (this have a problem, and is that won't support too many connections (if this client has a bad connection)). But also, I have been thinking one some kind of official server, so avoid that users need to host their own servers. But first, we need to make the MTA style server system, and later with this same server host one by us, but we need to make it affordable.