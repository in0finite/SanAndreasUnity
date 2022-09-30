using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;

namespace SanAndreasUnity.Stats
{
    public class WorldStats : MonoBehaviour
    {
        void Start()
        {
            UGameCore.Utilities.Stats.RegisterStat(new UGameCore.Utilities.Stats.Entry(){ category = "WORLD", getStatsAction = GetStats });
        }

        void GetStats(UGameCore.Utilities.Stats.GetStatsContext context)
        {
            var sb = context.stringBuilder;

            var cell = Cell.Instance;

            if (cell != null)
            {
                sb.Append($"nav mesh update percentage {cell.NavMeshUpdatePercentage}\n");
                sb.Append($"NumMapObjectsWithNavMeshToAdd {cell.NumMapObjectsWithNavMeshToAdd}\n");
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

        }

    }
}
