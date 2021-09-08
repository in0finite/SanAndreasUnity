using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;

namespace SanAndreasUnity.Stats
{
    public class WorldStats : MonoBehaviour
    {
        private readonly System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();


        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "WORLD", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {
            var sb = _stringBuilder;
            sb.Clear();

            var cell = Cell.Instance;

            if (cell != null)
            {
				sb.Append($"max draw distance {cell.MaxDrawDistance}\n");
                sb.Append($"num focus points {cell.FocusPointManager?.FocusPoints.Count ?? 0}\n");
                sb.Append($"num static objects {cell.NumStaticGeometries}\n");
                sb.Append($"num TOBJ objects {StaticGeometry.TimedObjects.Count}\n");
                sb.Append($"num active ENEX objects {EntranceExitMapObject.AllActiveObjects.Count}\n");
                sb.Append($"num active objects with lights {StaticGeometry.ActiveObjectsWithLights.Count}\n");
                sb.Append($"num active lights {StaticGeometry.ActiveObjectsWithLights.Sum(_ => _.NumLightSources)}\n");
                sb.Append($"geometry parts loaded {Importing.Conversion.Geometry.NumGeometryPartsLoaded}\n");

                sb.Append($"world systems\n");
                for (int i = 0; i < cell.WorldSystem.WorldSystems.Count; i++)
                {
                    sb.Append($"\tdistance level {cell.WorldSystem.DistanceLevels[i]}\n");
                    var worldSystem = cell.WorldSystem.WorldSystems[i];
                    sb.Append($"\tnum focus points {worldSystem.FocusPoints.Count}\n");
                    sb.Append($"\tnum areas {worldSystem.GetNumAreas(0)} {worldSystem.GetNumAreas(1)} {worldSystem.GetNumAreas(2)}\n");
                    if (Ped.Instance != null)
                    {
                        int numAreasVisible = 0,
                            numObjectsInVisibleAreas = 0,
                            maxNumFocusPointsThatSeeMe = 0,
                            minNumFocusPointsThatSeeMe = int.MaxValue;

                        worldSystem.ForEachAreaInRadius(
                            Ped.Instance.transform.position,
                            Mathf.Min(cell.WorldSystem.DistanceLevels[i], cell.MaxDrawDistance),
                            area =>
                            {
                                numAreasVisible++;
                                numObjectsInVisibleAreas += area?.ObjectsInside?.Count ?? 0;
                                maxNumFocusPointsThatSeeMe = Mathf.Max(maxNumFocusPointsThatSeeMe, area?.FocusPointsThatSeeMe?.Count ?? 0);
                                minNumFocusPointsThatSeeMe = Mathf.Min(minNumFocusPointsThatSeeMe, area?.FocusPointsThatSeeMe?.Count ?? 0);
                            }
                        );

                        sb.Append($"\tlocal ped visibility\n");
                        sb.Append($"\t\tnum areas visible {numAreasVisible}\n");
                        sb.Append($"\t\tnum objects in visible areas {numObjectsInVisibleAreas}\n");
                        sb.Append($"\t\tmax num focus points that see an area {maxNumFocusPointsThatSeeMe}\n");
                        sb.Append($"\t\tmin num focus points that see an area {minNumFocusPointsThatSeeMe}\n");
                    }
                    sb.Append($"\n");
                }
            }
            else
            {
                sb.Append($"World not loaded\n");
            }

            GUILayout.Label(sb.ToString());
        }

    }
}
