using System;
using System.Collections.Generic;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class WorldSystem<T>
        where T : Component
    {
        public class Area
        {
            internal List<T> objectsInside;
            public IReadOnlyList<T> ObjectsInside => this.objectsInside;

            internal HashSet<long> focusPointsThatSeeMe;
            public IReadOnlyCollection<long> FocusPointsThatSeeMe => this.focusPointsThatSeeMe;

            internal bool isMarkedForUpdate;
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
                            throw new System.IndexOutOfRangeException("Invalid index");
                    }
                }
            }
        }

        private struct NewAreasResult
        {
            public (AreaIndexes areaIndexes, bool hasResult) x, y, z;

            public static NewAreasResult WithOne(AreaIndexes areaIndexes) => new NewAreasResult { x = (areaIndexes, true) };
        }

        public class ConcurrentModificationException : System.Exception
        {
            public ConcurrentModificationException()
                : base("Can not perform the operation because it would result in concurrent modification of collections")
            {
            }
        }

        private readonly Area[,,] _areas;
        private readonly List<FocusPoint> _focusPoints = new List<FocusPoint>(32);

        private readonly List<Area> _areasForUpdate = new List<Area>(128);

        private readonly float _worldMin;
        private readonly float _worldMax;
        private readonly float _worldHalfSize;
        private readonly float _areaSize;
        private readonly ushort _numAreasPerAxis;

        private readonly float _yWorldMin;
        private readonly float _yWorldMax;
        private readonly float _yWorldHalfSize;
        private readonly float _yAreaSize;
        private readonly ushort _yNumAreasPerAxis;

        private bool _isInUpdate = false;

        public readonly System.Action<Area, bool> onAreaChangedVisibility = (area, isVisible) => { };

        public WorldSystem(
            uint worldSize,
            ushort numAreasPerAxis,
            uint worldSizeY,
            ushort yNumAreasPerAxis)
        {
            _worldMin = - worldSize / 2f;
            _worldMax = worldSize / 2f;
            _worldHalfSize = worldSize / 2f;
            _areaSize = worldSize / (float)numAreasPerAxis;
            _numAreasPerAxis = (ushort) (numAreasPerAxis + 2); // additional 2 for positions out of bounds

            _yWorldMin = - worldSizeY / 2f;
            _yWorldMax = worldSizeY / 2f;
            _yWorldHalfSize = worldSizeY / 2f;
            _yAreaSize = worldSizeY / (float)yNumAreasPerAxis;
            _yNumAreasPerAxis = (ushort) (yNumAreasPerAxis + 2); // additional 2 for positions out of bounds

            _areas = new Area[_numAreasPerAxis, _yNumAreasPerAxis, _numAreasPerAxis];
        }

        public FocusPoint RegisterFocusPoint(float radius, Vector3 pos)
        {
            this.ThrowIfConcurrentModification();

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
            this.ThrowIfConcurrentModification();

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
            this.ThrowIfConcurrentModification();

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
                    // actually, it should not happen
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
            this.ThrowIfConcurrentModification();

            var area = GetAreaAt(pos);

            if (null == area.objectsInside)
                area.objectsInside = new List<T>();
            area.objectsInside.Add(obj);
        }

        public void RemoveObjectFromArea(Vector3 pos, T obj)
        {
            this.ThrowIfConcurrentModification();

            var area = GetAreaAt(pos);

            if (area.objectsInside != null)
            {
                area.objectsInside.Remove(obj);
            }
        }

        public void Update()
        {
            _isInUpdate = true;

            try
            {
                this.UpdateInternal();
            }
            finally
            {
                _isInUpdate = false;
            }
        }

        private void UpdateInternal()
        {
            // check areas that are marked for update
            for (int i = 0; i < _areasForUpdate.Count; i++)
            {
                var area = _areasForUpdate[i];

                if (!area.isMarkedForUpdate) // should not happen, but just in case
                    continue;

                this.NotifyAreaChangedVisibility(area, IsAreaVisible(area));

                area.isMarkedForUpdate = false;
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
            // Vector3 min = new Vector3(pos.x - radius, pos.y - radius, pos.z - radius);
            // Vector3 max = new Vector3(pos.x + radius, pos.y + radius, pos.z + radius);

            return new AreaIndexes
            {
                x = new Range { lower = GetAreaIndex(pos.x - radius), higher = GetAreaIndex(pos.x + radius) },
                y = new Range { lower = GetAreaIndexForYAxis(pos.y - radius), higher = GetAreaIndexForYAxis(pos.y + radius) },
                z = new Range { lower = GetAreaIndex(pos.z - radius), higher = GetAreaIndex(pos.z + radius) },
            };
        }

        private short GetAreaIndex(float pos)
        {
            if (pos < _worldMin)
                return 0;
            if (pos > _worldMax)
                return (short) (_numAreasPerAxis - 1);

            // skip 1st
            return (short) (1 + Mathf.FloorToInt((pos + _worldHalfSize) % _areaSize));
        }

        private short GetAreaIndexForYAxis(float pos)
        {
            if (pos < _yWorldMin)
                return 0;
            if (pos > _yWorldMax)
                return (short) (_yNumAreasPerAxis - 1);

            // skip 1st
            return (short) (1 + Mathf.FloorToInt((pos + _yWorldHalfSize) % _yAreaSize));
        }

        private (short x, short y, short z) GetAreaIndex(Vector3 pos)
        {
            return (GetAreaIndex(pos.x), GetAreaIndexForYAxis(pos.y), GetAreaIndex(pos.z));
        }

        private Area GetAreaAt(Vector3 pos)
        {
            var index = this.GetAreaIndex(pos);
            var area = _areas[index.x, index.y, index.z];
            if (null == area)
            {
                area = new Area();
                _areas[index.x, index.y, index.z] = area;
            }
            return area;
        }

        private NewAreasResult GetNewAreas(AreaIndexes oldIndexes, AreaIndexes newIndexes)
        {
            var xResult = GetAffectedAreasForAxis(oldIndexes.x, newIndexes.x);
            if (!xResult.hasIntersection)
                return NewAreasResult.WithOne(newIndexes);

            var yResult = GetAffectedAreasForAxis(oldIndexes.y, newIndexes.y);
            if (!yResult.hasIntersection)
                return NewAreasResult.WithOne(newIndexes);

            var zResult = GetAffectedAreasForAxis(oldIndexes.z, newIndexes.z);
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

        private void ThrowIfConcurrentModification()
        {
            if (_isInUpdate)
                throw new ConcurrentModificationException();
        }

        private void NotifyAreaChangedVisibility(Area area, bool visible)
        {
            F.RunExceptionSafe(() => this.onAreaChangedVisibility(area, visible));
        }
    }
}
