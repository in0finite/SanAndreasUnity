using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class WorldSystem<T>
        where T : Component
    {
        public class Area
        {
            public List<T> objectsInside;
            public HashSet<long> focusPointsThatSeeMe;
            public bool isMarkedForUpdate;
        }

        public class FocusPoint
        {
            private static long _lastId = 1;

            public long Id { get; } = _lastId++;
            public float Radius { get; internal set; }
            public Vector3 Position { get; internal set; }

            internal FocusPoint()
            {
            }

            public override int GetHashCode()
            {
                return this.Id.GetHashCode();
            }
        }

        private struct Range
        {
            public short lower, higher;
        }

        private struct AreaIndexes
        {
            public Range x, y, z;

            public Range this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return this.x;
                        case 1:
                            return this.y;
                        case 2:
                            return this.z;
                        default:
                            throw new System.IndexOutOfRangeException("Invalid index!");
                    }
                }
            }
        }

        private struct NewAreasResult
        {
            public (AreaIndexes areaIndexes, bool hasResult) x, y, z;

            public static NewAreasResult WithOne(AreaIndexes areaIndexes) => new NewAreasResult { x = (areaIndexes, true) };
        }

        private long _lastFocusPointId = 1;

        private readonly Area[] _areas;
        private readonly List<FocusPoint> _focusPoints = new List<FocusPoint>(32);

        private readonly List<Area> _areasForUpdate = new List<Area>(128);

        public System.Action<T, bool> onObjectChangedVisibility = (component, isVisible) => { };

        public WorldSystem(
            int worldSize,
            int areaSize)
        {
            int numAreasPerAxis = Mathf.CeilToInt(worldSize / (float)areaSize);
            _areas = new Area[numAreasPerAxis * numAreasPerAxis];
        }

        public FocusPoint RegisterFocusPoint(float radius, Vector3 pos)
        {
            var focusPoint = new FocusPoint();
            focusPoint.Radius = radius;
            focusPoint.Position = pos;

            _focusPoints.Add(focusPoint);

            this.ForEachAreaInRadius(pos, radius, area =>
            {
                AddToFocusPointsThatSeeMe(area, focusPoint.Id);
                this.MarkAreaForUpdate(area);
            });

            return focusPoint;
        }

        public void UnRegisterFocusPoint(long id)
        {
            int index = _focusPoints.FindIndex(f => f.Id == id);
            if (index < 0)
                return;

            var focusPoint = _focusPoints[index];
            _focusPoints.RemoveAt(index);

            this.ForEachAreaInRadius(focusPoint.Position, focusPoint.Radius, area =>
            {
                RemoveFromFocusPointsThatSeeMe(area, focusPoint.Id);
                this.MarkAreaForUpdate(area);
            });
        }

        public void FocusPointChangedPosition(FocusPoint focusPoint, Vector3 newPos)
        {
            AreaIndexes oldIndexes = GetAreaIndexesInRadius(focusPoint.Position, focusPoint.Radius);
            AreaIndexes newIndexes = GetAreaIndexesInRadius(newPos, focusPoint.Radius);

            if (!AreasEqual(oldIndexes, newIndexes))
            {
                // areas changed

                NewAreasResult newlyVisibleAreasResult = GetNewAreas(oldIndexes, newIndexes);
                NewAreasResult noLongerVisibleAreasResult = GetNewAreas(newIndexes, oldIndexes);

                this.ForEachArea(newlyVisibleAreasResult, area =>
                {
                    // this can happen multiple times per single area, but since we use hashset it should be no problem
                    AddToFocusPointsThatSeeMe(area, focusPoint.Id);
                    this.MarkAreaForUpdate(area);
                });

                this.ForEachArea(noLongerVisibleAreasResult, area =>
                {
                    RemoveFromFocusPointsThatSeeMe(area, focusPoint.Id);
                    this.MarkAreaForUpdate(area);
                });
            }

            focusPoint.Position = newPos;
        }

        public void AddObjectToArea(Vector3 pos, T obj)
        {
            var area = GetAreaAt(pos);

            if (null == area.objectsInside)
                area.objectsInside = new List<T>();
            area.objectsInside.Add(obj);

            this.onObjectChangedVisibility(obj, IsAreaVisible(area));
        }

        public void RemoveObjectFromArea(Vector3 pos, T obj)
        {
            var area = GetAreaAt(pos);

            if (area.objectsInside != null)
            {
                area.objectsInside.Remove(obj);
            }
        }

        public void Update()
        {
            // check areas that are marked for update
            for (int i = 0; i < _areasForUpdate.Count; i++)
            {
                var area = _areasForUpdate[i];
                area.isMarkedForUpdate = false;
                bool isVisible = IsAreaVisible(area);

                if (area.objectsInside != null)
                {
                    for (int j = 0; j < area.objectsInside.Count; j++)
                    {
                        this.onObjectChangedVisibility(area.objectsInside[j], isVisible);
                    }
                }
            }

            _areasForUpdate.Clear();

        }

        private void ForEachAreaInRadius(Vector3 pos, float radius, System.Action<Area> action)
        {
            AreaIndexes areaIndexesInRadius = GetAreaIndexesInRadius(pos, radius);
            this.ForEachArea(areaIndexesInRadius, action);
        }

        private void ForEachArea(AreaIndexes areaIndexes, System.Action<Area> action)
        {
            for (int x = areaIndexes.x.lower; x < areaIndexes.x.higher; x++)
            {
                for (int y = areaIndexes.y.lower; y < areaIndexes.y.higher; y++)
                {
                    for (int z = areaIndexes.z.lower; z < areaIndexes.z.higher; z++)
                    {
                        action(_areas[x, y, z]);
                    }
                }
            }
        }

        private void ForEachArea(NewAreasResult newAreasResult, System.Action<Area> action)
        {
            if (newAreasResult.x.hasResult)
                this.ForEachArea(newAreasResult.x.areaIndexes, action);
            if (newAreasResult.y.hasResult)
                this.ForEachArea(newAreasResult.y.areaIndexes, action);
            if (newAreasResult.z.hasResult)
                this.ForEachArea(newAreasResult.z.areaIndexes, action);
        }

        private AreaIndexes GetAreaIndexesInRadius(Vector3 pos, float radius)
        {

        }

        private Area GetAreaAt(Vector3 pos)
        {

        }

        private NewAreasResult GetNewAreas(AreaIndexes oldIndexes, AreaIndexes newIndexes)
        {
            var xResult = GetAffectedAreasForAxis(0, oldIndexes.x, newIndexes.x);
            if (!xResult.hasIntersection)
                return NewAreasResult.WithOne(newIndexes);

            var yResult = GetAffectedAreasForAxis(1, oldIndexes.y, newIndexes.y);
            if (!yResult.hasIntersection)
                return NewAreasResult.WithOne(newIndexes);

            var zResult = GetAffectedAreasForAxis(2, oldIndexes.z, newIndexes.z);
            if (!zResult.hasIntersection)
                return NewAreasResult.WithOne(newIndexes);

            if (!xResult.hasResult && !yResult.hasResult && !zResult.hasResult) // cubes are equal
                throw new System.Exception("Cubes appear to be the same, this should not happen");

            var toReturn = new NewAreasResult
            {
                x = (newIndexes, xResult.hasResult),
                y = (newIndexes, yResult.hasResult),
                z = (newIndexes, zResult.hasResult),
            };

            if (xResult.hasResult)
                toReturn.x.areaIndexes.x = xResult.affectedRange;
            if (yResult.hasResult)
                toReturn.y.areaIndexes.y = yResult.affectedRange;
            if (zResult.hasResult)
                toReturn.z.areaIndexes.z = zResult.affectedRange;

            return toReturn;
        }

        private (Range affectedRange, bool hasResult, bool hasIntersection) GetAffectedAreasForAxis(
            int axisIndex,
            Range oldRange,
            Range newRange)
        {
            // returns parts of new range (new cube)

            if (oldRange.lower == newRange.lower)
            {
                // same position along this axis
                return (default, false, true);
            }

            // check if there is intersection
            if (oldRange.lower < newRange.lower)
            {
                if (oldRange.higher < newRange.lower) // no intersection
                    return default;
            }
            else
            {
                if (oldRange.lower > newRange.higher) // no intersection
                    return default;
            }

            // find intersection which is edge of old range (old cube)

            short intersection;
            if (oldRange.lower < newRange.lower)
            {
                intersection = oldRange.higher;
                return (
                    new Range {lower = (short) (intersection + 1), higher = newRange.higher},
                    true,
                    true);
            }
            else
            {
                intersection = oldRange.lower;
                return (
                    new Range {lower = newRange.lower, higher = (short) (intersection - 1)},
                    true,
                    true);
            }

        }

        private static void EnsureFocusPointsCollectionInitialized(Area area)
        {
            if (null == area.focusPointsThatSeeMe)
                area.focusPointsThatSeeMe = new HashSet<long>();
        }

        private bool IsAreaVisible(Area area)
        {
            return area.focusPointsThatSeeMe != null && area.focusPointsThatSeeMe.Count > 0;
        }

        private void MarkAreaForUpdate(Area area)
        {
            if (area.isMarkedForUpdate)
                return;
            _areasForUpdate.Add(area);
            area.isMarkedForUpdate = true;
        }

        private static void AddToFocusPointsThatSeeMe(Area area, long id)
        {
            EnsureFocusPointsCollectionInitialized(area);
            area.focusPointsThatSeeMe.Add(id);
        }

        private static bool RemoveFromFocusPointsThatSeeMe(Area area, long id)
        {
            bool success = false;
            if (area.focusPointsThatSeeMe != null)
            {
                success = area.focusPointsThatSeeMe.Remove(id);
                if (area.focusPointsThatSeeMe.Count == 0)
                    area.focusPointsThatSeeMe = null;
            }
            return success;
        }

        private static bool AreasEqual(AreaIndexes a, AreaIndexes b)
        {
            return RangesEqual(a.x, b.x) && RangesEqual(a.y, b.y) && RangesEqual(a.z, b.z);
        }

        private static bool RangesEqual(Range a, Range b)
        {
            return a.lower == b.lower && a.higher == b.higher;
        }
    }
}
