

All geometry and some other assets should be loaded async-ly.

We need a background thread which loads assets from disk. It will have a queue, where any script can register it's file for loading.

We need a way to wait until specific asset is loaded.


***

First identify what is taking time using Profiler. Maybe not all time goes on loading from disk. Some of it may be spent on converting the mesh.

Identified what is taking time:

- loading from disk (mostly takes around 20 ms, and goes up to 70 ms, but for some txds it goes to 100 ms)

- creating mesh (mostly around 10 ms, but can go up to 100 ms)

- attach collision model (mostly below the time to create mesh, but in some cases it goes to more than 100 ms)

- updating divisions - runs almost every frame (especially when loading something), even though you invoke it with 100 ms interval - takes around 30 ms


***

## TODO

- create meshes asyncly (this means converting mesh and it's textures)

- attach collision model asyncly

- update divisions in separate thread ? or optimize division system ?

- unload map objects which are not visible to any focus point for given amount of time - this means destroying meshes, materials, textures, and removing references to loaded data in RAM


## TIPS

- attaching collision model takes a lot of memory - why ? - because it reads archive file

- dedicated server doesn't need textures - we can detect if we are running as dedicated server, and skip loading textures

