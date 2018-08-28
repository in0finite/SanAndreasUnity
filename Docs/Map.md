
We'll have to convert map to use new unity UI system, because clipping doesn't work well with rotated objects in imGUI.

No big deal, all calculations are done, just change the way the map items are drawn.

Instead of drawing them every frame, we'll add/remove items from map.

AddMapItem (MapItem), RemoveMapItem (MapItem)

class MapItem {
	
	// use position from transform, or otherwise obtain it from delegates (this is useful if map item doesn't have it's game object)
	bool usePositionFromTransform;
	Transform transform;
	bool applyRotation;	// is map item rotated ?
	System.Action<Vector3> getPosition;
	System.Action<Quaternion> getRotation;
	bool isVisible;	// is it currently visible ?
	bool isClampedToEdge;	// if item is not visible, is it clamped to edge of map ?

}

Then, for every map item create sprite, position it and optionally rotate it every frame, and enable/disable it based on whether it is in visible part of the map.

We can still draw info area and some parts of the map using imGUI.

