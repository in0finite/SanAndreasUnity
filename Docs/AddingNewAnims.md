
## Adding/playing new anims


There are 2 ways to play anim:

- using it's anim group and index

- using it's ifp file name and anim name (needs testing)

<br>
### Anim groups

Anims can be placed in groups. The idea is that peds' anims can be grouped, so that using the same index, you can play different anims for different ped groups.

For example, if you use AnimGroup.WalkCycle as anim group, and AnimIndex.Walk as index, then when ped is a man, 'walk_civi' will be player, and when ped is a woman, 'woman_walknorm' will be played.

Anim groups are defined inside /Data/auxanimgrp.dat file. You can read more about file structure in that file. 

If you want to add new anims, you can add them to existing group, or create new group. After you added them to a file, you will need to update AnimGroup and AnimIndex enumerations, so that you can play those anims.

Example for playing walk anim:

	Pedestrian.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Walk)

<br>
### Ifp file name and anim name

This method is not tested, but in the future, it will be the main method for playing anims.

All you have to do when you want to play anim, is to call:

	Pedestrian.PlayAnim (animId)

where 'animId' is the anim you want to play.

For example: 

	Pedestrian.PlayAnim (new AnimId('ped', 'roadcross'))

this will play anim which is located in *ped.ifp*, and which is called *roadcross*

