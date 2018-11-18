

All geometry and some other assets should be loaded async-ly.

We need a background thread which loads assets from disk. It will have a queue, where any script can register it's file for loading.

We need a way to wait until specific asset is loaded.


***

First identify what is taking time using Profiler. Maybe not all time goes on loading from disk. Some of it may be spent on converting the mesh.

***

What could take a lot of time:

- loading from disk
- converting mesh
- creating Mesh object (setting and applying vertex and index buffers)
- the same as above, but for textures

***

API:

LoadJob RegisterFileForLoading(string filePath)

LoadJob RegisterActionToRun(System.Action action)

void WaitForJobToFinish(LoadJob job)

class LoadJob
{
	// ...
	
}

