LightLOD is a configurable, dynamic quality adjustment utility for Realtime Lights. 

In many cases when you have a large number of realtime lights, the performance impact of drawing very high quality shadows is unreasonable. LightLOD allows you to customize every Realtime light's shadowResolution based on distance from the player. 
The idea of this is - the farther away the light, the less important the shadows are. 

To use it, merely attach LightLOD to an existing Light, or replace your Light(s) with one of the preconfigured Prefabs based on how aggressively you would like to scale the lights.
Then, modify LightLOD in 2 places:
* Line 20 - Replace `private FreeCamera LocalPlayer` with `private [Your Player Class Here] LocalPlayer` - so if your Player class is just `Player`, it will read: `private Player LocalPlayer`
* Line 44 - Replace `LocalPlayer = GameObject.FindObjectOfType<FreeCamera>();` with `LocalPlayer = GameObject.FindObjectOfType<[Your Player Class Here]>();` - agian, using Player as an example: `LocalPlayer = GameObject.FindObjectOfType<Player>();`

## Configuring LightLOD
LightLOD has 3 primary runtime configurations:

* Light Should Be On 
** For Static Lights (ones that cannot be toggled on/off at runtime) this should always be true. 
** For Dynamic Lights (ones that CAN be toggled on/off at runtime) you must update this when you switch `light.enabled = false`
* Shadow Quality LODs - a list of LOD states for your light. Each LOD has the following properties:
** Distance Range - when the player is within this range, the specified Shadow Resolution will be applied to the light. Note that it is Inclusive of the upper bound, and exclusive of the lower bound.
** Shadow Resolution - Unity's Shadow Resolution - the quality of shadows to use when player is within this distance range.
** Cast No Shadows - If the light should just not cast any shadow
** Debug Color - The color of the Ray drawn from the light to the player in the Unity Editor, and the color of the light if you toggle "Show Light Color As Debug Color"
* Update Delay - How frequently the lights should consider the player's location for updates. Lower values result in higher CPU usage and faster responsiveness for LOD range swapping

#### Debug Configurations/Serialized Properties
There are some debug configurations also availble
* Show Light Color As Debug Color - If you check this, it will force the `light.color` to be the same as `DebugColor` of the current Shadow Quality LOD
* (Readonly) Distance From Player - Displays current distance of player from the light
* (Readonly) IsClamped - Displays if the current Shadow Quality LOD's `ShadowResolution` is higher than the available `QualitySettings.shadowResolution`. If this is true, the `QualitySettings.shadowResolution` is the quality that is applied.
* (Readonly) LOD - the current Shadow Quality LOD index.

Open demo.unity and enable the Animator on the Camrea to get the loop shown in the video.

For a more stressful example open demo-2-many-lights.unity