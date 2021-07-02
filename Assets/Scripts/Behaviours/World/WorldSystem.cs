﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            public short Length => (short) (this.higher - this.lower);

            public Range(short lower, short higher)
            {
                if (lower > higher)
                    throw new ArgumentException($"lower {lower} is > than higher {higher}");
                this.lower = lower;
                this.higher = higher;
            }

            public bool EqualsToOther(Range other) => this.lower == other.lower && this.higher == other.higher;

            public bool Overlaps(Range other) => !(this.higher < other.lower || this.lower > other.higher);

            public bool IsInsideOf(Range other) => (this.lower >= other.lower && this.higher < other.higher)
                                                   || (this.lower > other.lower && this.higher <= other.higher);

            public bool IsEqualOrInsideOf(Range other) => this.EqualsToOther(other) || this.IsInsideOf(other);
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

            public int Volume => (this.x.Length + 1) * (this.y.Length + 1) * (this.z.Length + 1);

            public bool EqualsToOther(AreaIndexes other) => this.x.EqualsToOther(other.x) && this.y.EqualsToOther(other.y) && this.z.EqualsToOther(other.z);

            public bool Overlaps(AreaIndexes other) => this.x.Overlaps(other.x) && this.y.Overlaps(other.y) && this.z.Overlaps(other.z);

            public bool IsInsideOf(AreaIndexes other) => !this.EqualsToOther(other)
                                                         && (this.x.IsEqualOrInsideOf(other.x) && this.y.IsEqualOrInsideOf(other.y) && this.z.IsEqualOrInsideOf(other.z));
        }

        // private struct NewAreasResult
        // {
        //     public (AreaIndexes areaIndexes, bool hasResult) x, y, z;
        //
        //     public static NewAreasResult WithOne(AreaIndexes areaIndexes) => new NewAreasResult { x = (areaIndexes, true) };
        // }

        private struct AffectedRangesForAxis
        {
            // there can be max 3 ranges
            // each range can be intersection, or free
            // there can be max 2 free parts and max 1 intersection part
            public (Range range, bool isIntersectionPart, bool hasValues) range1, range2, range3;

            // is there intersection on this axis ? if not, cubes do not intersect, and other results from this struct should be ignored
            public bool hasIntersectionOnAxis;

            public IEnumerable<(Range range, bool isIntersectionPart, bool hasValues)> Ranges => new [] {range1, range2, range3};

            public void ForEachWithValue(Action<(Range range, bool isIntersectionPart)> action)
            {
                if (range1.hasValues)
                    action((range1.range, range1.isIntersectionPart));
                if (range2.hasValues)
                    action((range2.range, range2.isIntersectionPart));
                if (range3.hasValues)
                    action((range3.range, range3.isIntersectionPart));
            }

            public void ForEachFree(Action<Range> action)
            {
                if (range1.hasValues && !range1.isIntersectionPart)
                    action(range1.range);
                if (range2.hasValues && !range2.isIntersectionPart)
                    action(range2.range);
                if (range3.hasValues && !range3.isIntersectionPart)
                    action(range3.range);
            }
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

        // these buffers are reused every time to avoid memory allocations, but that makes this class non thread safe
        private AreaIndexes[] _bufferForGettingNewAreas = new AreaIndexes[27]; // 3^3
        private AreaIndexes[] _bufferForGettingOldAreas = new AreaIndexes[27]; // 3^3

        private readonly System.Action<Area, bool> _onAreaChangedVisibility = null;

        public WorldSystem(
            uint worldSize,
            ushort numAreasPerAxis,
            uint worldSizeY,
            ushort yNumAreasPerAxis,
            System.Action<Area, bool> onAreaChangedVisibility)
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

            _onAreaChangedVisibility = onAreaChangedVisibility;
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

                byte numNewAreaIndexes = GetNewAreas(oldIndexes, newIndexes, _bufferForGettingNewAreas);
                byte numOldAreaIndexes = GetNewAreas(newIndexes, oldIndexes, _bufferForGettingOldAreas);

                for (byte i = 0; i < numNewAreaIndexes; i++)
                {
                    var areaIndexes = _bufferForGettingNewAreas[i];

                    this.ForEachArea(areaIndexes, area =>
                    {
                        // this can happen multiple times per single area, but since we use hashset it should be no problem
                        // actually, it should not happen anymore with new implementation
                        AddToFocusPointsThatSeeMe(area, focusPoint.Id);
                        this.MarkAreaForUpdate(area);
                    });
                }

                for (byte i = 0; i < numOldAreaIndexes; i++)
                {
                    var areaIndexes = _bufferForGettingOldAreas[i];

                    this.ForEachArea(areaIndexes, area =>
                    {
                        RemoveFromFocusPointsThatSeeMe(area, focusPoint.Id);
                        this.MarkAreaForUpdate(area);
                    });
                }

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
            this.ThrowIfConcurrentModification();

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

        public void ForEachAreaInRadius(Vector3 pos, float radius, System.Action<Area> action)
        {
            AreaIndexes areaIndexesInRadius = GetAreaIndexesInRadius(pos, radius);
            this.ForEachArea(areaIndexesInRadius, action);
        }

        public List<Area> GetAreasInRadius(Vector3 pos, float radius)
        {
            var areas = new List<Area>();
            this.ForEachAreaInRadius(pos, radius, a => areas.Add(a));
            return areas;
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
            return (short) (1 + Mathf.FloorToInt((pos + _worldHalfSize) / _areaSize));
        }

        private short GetAreaIndexForYAxis(float pos)
        {
            if (pos < _yWorldMin)
                return 0;
            if (pos > _yWorldMax)
                return (short) (_yNumAreasPerAxis - 1);

            // skip 1st
            return (short) (1 + Mathf.FloorToInt((pos + _yWorldHalfSize) / _yAreaSize));
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

        private byte GetNewAreas(AreaIndexes oldIndexes, AreaIndexes newIndexes, AreaIndexes[] resultBuffer)
        {
            var xResult = GetAffectedRangesForAxis(oldIndexes.x, newIndexes.x);
            ValidateAffectedRangesForAxis(xResult);
            if (!xResult.hasIntersectionOnAxis)
            {
                resultBuffer[0] = newIndexes;
                return 1;
            }

            var yResult = GetAffectedRangesForAxis(oldIndexes.y, newIndexes.y);
            ValidateAffectedRangesForAxis(yResult);
            if (!yResult.hasIntersectionOnAxis)
            {
                resultBuffer[0] = newIndexes;
                return 1;
            }

            var zResult = GetAffectedRangesForAxis(oldIndexes.z, newIndexes.z);
            ValidateAffectedRangesForAxis(zResult);
            if (!zResult.hasIntersectionOnAxis)
            {
                resultBuffer[0] = newIndexes;
                return 1;
            }

            // for all combinations, if at least 1 range is free

            byte count = 0;

            xResult.ForEachWithValue(tupleX =>
            {
                yResult.ForEachWithValue(tupleY =>
                {
                    zResult.ForEachWithValue(tupleZ =>
                    {
                        if (!tupleX.isIntersectionPart || !tupleY.isIntersectionPart || !tupleZ.isIntersectionPart)
                        {
                            // at least 1 range is free

                            resultBuffer[count] = new AreaIndexes
                            {
                                x = tupleX.range,
                                y = tupleY.range,
                                z = tupleZ.range,
                            };

                            count++;
                        }
                    });
                });
            });

            if (count == 0)
            {
                // there are no free ranges - new cube is inside of old cube (or equal) - there are no new areas - return 0

                if (newIndexes.Volume > oldIndexes.Volume)
                    throw new Exception("New cube should be <= than old cube");
                if (!newIndexes.IsInsideOf(oldIndexes))
                    throw new Exception("New cube should be inside of old cube");
            }

            return count;
        }

        private AffectedRangesForAxis GetAffectedRangesForAxis(
            Range oldRange,
            Range newRange)
        {
            if (RangesEqual(oldRange, newRange))
            {
                // same position and size along this axis
                return new AffectedRangesForAxis
                {
                    hasIntersectionOnAxis = true,
                    range1 = (oldRange, true, true),
                };
            }

            // check if there is intersection
            if (oldRange.lower > newRange.higher)
                return default;
            if (newRange.lower > oldRange.higher)
                return default;

            // first find intersection part (max 1)

            var toReturn = new AffectedRangesForAxis { hasIntersectionOnAxis = true };
            Range totalRange = new Range(
                Min(oldRange.lower, newRange.lower),
                Max(oldRange.higher, newRange.higher));
            short minOfHighers = Min(oldRange.higher, newRange.higher);
            Range intersectionRange;

            if (oldRange.lower < newRange.lower)
            {
                // he is left
                intersectionRange = new Range(newRange.lower, minOfHighers);
            }
            else if (oldRange.lower == newRange.lower)
            {
                // they share left edge
                intersectionRange = new Range(newRange.lower, minOfHighers);
            }
            else
            {
                // his left edge is more to the right
                intersectionRange = new Range(oldRange.lower, minOfHighers);
            }

            // now find free range(s) based on total range and intersection range

            if (intersectionRange.Length >= totalRange.Length) // should not happen
                throw new Exception($"Intersection range length {intersectionRange.Length} is >= than total range length {totalRange.Length}");

            toReturn.range1 = (intersectionRange, true, true);

            if (RangesEqual(newRange, intersectionRange))
            {
                // new range is inside of old range
                // there are no free ranges
                return toReturn;
            }

            if (newRange.lower >= intersectionRange.lower)
            {
                Range freeRange = new Range((short) (intersectionRange.higher + 1), newRange.higher);
                toReturn.range2 = (freeRange, false, true);
                return toReturn;
            }

            // newRange.lower < intersectionRange.lower

            Range freeRange1 = new Range(newRange.lower, (short) (intersectionRange.lower - 1));
            toReturn.range2 = (freeRange1, false, true);

            if (newRange.higher > intersectionRange.higher)
            {
                Range freeRange2 = new Range((short) (intersectionRange.higher + 1), newRange.higher);
                toReturn.range3 = (freeRange2, false, true);
            }

            return toReturn;
        }

        private static void ValidateAffectedRangesForAxis(AffectedRangesForAxis affectedRangesForAxis)
        {
            int count = affectedRangesForAxis.Ranges.Count(r => r.hasValues);

            if (!affectedRangesForAxis.hasIntersectionOnAxis && count > 0)
                throw new Exception("Count > 0 and has no intersection");

            if (!affectedRangesForAxis.hasIntersectionOnAxis)
                return;

            var intersectionRanges = affectedRangesForAxis.Ranges
                .Where(r => r.hasValues && r.isIntersectionPart)
                .ToList();

            var freeRanges = affectedRangesForAxis.Ranges
                .Where(r => r.hasValues && !r.isIntersectionPart)
                .ToList();

            if (intersectionRanges.Count != 1)
                throw new Exception($"Num intersection ranges must be 1, found {intersectionRanges.Count}");

            if (freeRanges.Count > 2)
                throw new Exception($"Num free ranges is {freeRanges.Count}");

            var allRanges = intersectionRanges.Concat(freeRanges).ToList();
            for (int i = 0; i < allRanges.Count; i++)
            {
                for (int j = i + 1; j < allRanges.Count; j++)
                {
                    var r1 = allRanges[i].range;
                    var r2 = allRanges[j].range;
                    if (r1.Overlaps(r2))
                        throw new Exception($"Ranges overlap, {r1} and {r2}");
                }
            }

            // there must be no space between ranges
            var orderedRanges = allRanges.OrderBy(r => r.range.lower).ToList();
            for (int i = 0; i < orderedRanges.Count - 1; i++)
            {
                var r1 = orderedRanges[i].range;
                var r2 = orderedRanges[i+1].range;
                if (r1.higher + 1 != r2.lower)
                    throw new Exception($"There is space between ranges, higher is {r1.higher}, lower is {r2.lower}");
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
            this.ThrowIfConcurrentModification(); // just in case
            if (area.isMarkedForUpdate)
                return;
            _areasForUpdate.Add(area);
            area.isMarkedForUpdate = true;
        }

        private static void AddToFocusPointsThatSeeMe(Area area, long id)
        {
            EnsureFocusPointsCollectionInitialized(area);
            if (!area.focusPointsThatSeeMe.Add(id))
                throw new Exception($"Failed to add focus point with id {id} - it already exists");
        }

        private static void RemoveFromFocusPointsThatSeeMe(Area area, long id)
        {
            bool success = false;
            if (area.focusPointsThatSeeMe != null)
            {
                success = area.focusPointsThatSeeMe.Remove(id);
                if (area.focusPointsThatSeeMe.Count == 0)
                    area.focusPointsThatSeeMe = null;
            }

            if (!success)
                throw new Exception($"Failed to remove focus point with id {id} - it doesn't exist");
        }

        private static bool AreasEqual(AreaIndexes a, AreaIndexes b)
        {
            return RangesEqual(a.x, b.x) && RangesEqual(a.y, b.y) && RangesEqual(a.z, b.z);
        }

        private static bool RangesEqual(Range a, Range b)
        {
            return a.lower == b.lower && a.higher == b.higher;
        }

        private static short Min(short a, short b)
        {
            return a <= b ? a : b;
        }

        private static short Max(short a, short b)
        {
            return a >= b ? a : b;
        }

        private void ThrowIfConcurrentModification()
        {
            if (_isInUpdate)
                throw new ConcurrentModificationException();
        }

        private void NotifyAreaChangedVisibility(Area area, bool visible)
        {
            F.RunExceptionSafe(() => this._onAreaChangedVisibility(area, visible));
        }
    }
}