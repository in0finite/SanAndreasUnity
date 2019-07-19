

All geometry and some other assets should be loaded async-ly.

We already have a background thread in which some assets are loaded from disk. But, assets should also be converted to usable data in this thread.


***

Identified what is taking time:

- loading from disk (mostly takes around 20 ms, and goes up to 70 ms, but for some txds it goes to 100 ms)

- creating mesh (mostly around 10 ms, but can go up to 100 ms)

- attach collision model (mostly below the time to create mesh, but in some cases it goes to more than 100 ms)

- updating divisions - runs almost every frame (especially when loading something), even though you invoke it with 100 ms interval - takes around 30 ms


***

## TODO

- order in which Async functions will return is not equal to order of call to those functions - because some of loading objects may be cached - adjust Geometry.LoadAsync() when loading multiple textures

- create meshes asyncly (this means converting mesh and it's textures)

- attach collision model asyncly

- update divisions in separate thread ? or optimize division system ?


## TIPS

- attaching collision model takes a lot of memory - why ? - because it reads archive file

