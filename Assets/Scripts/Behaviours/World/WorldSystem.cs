using System;
using System.Collections.Generic;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.WorldSystem
{
    public struct WorldSystemParams
    {
        public uint worldSize;
        public ushort numAreasPerAxis;
    }

    public interface IWorldSystem
    {
        void Update();

        FocusPoint RegisterFocusPoint(float radius, Vector3 pos);

        void UnRegisterFocusPoint(FocusPoint focusPoint);

        void FocusPointChangedParameters(FocusPoint focusPoint, Vector3 newPos, float newRadius);

        AreaIndex GetAreaIndex(Vector3 pos);
    }

    public interface IWorldSystem<T> : IWorldSystem
    {
        void AddObjectToArea(Vector3 pos, T obj);

        void RemoveObjectFromArea(Vector3 pos, T obj);
    }

    public static class WorldSystemExtensions
    {
        public static void FocusPointChangedPosition<T>(this IWorldSystem<T> worldSystem, FocusPoint focusPoint, Vector3 newPos)
            => worldSystem.FocusPointChangedParameters(focusPoint, newPos, focusPoint.Radius);

        public static void FocusPointChangedRadius<T>(this IWorldSystem<T> worldSystem, FocusPoint focusPoint, float newRadius)
            => worldSystem.FocusPointChangedParameters(focusPoint, focusPoint.Position, newRadius);
    }

    public sealed class FocusPoint
    {
        public long Id { get; } = FocusPointIdGenerator.GetNextId();
        public float Radius { get; internal set; }
        public Vector3 Position { get; internal set; }

        public FocusPoint(float radius, Vector3 position)
        {
            this.Radius = radius;
            this.Position = position;
        }

        public sealed override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }

    public struct AreaIndex
    {
        public short x, y, z;

        public AreaIndex(short x, short y, short z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool IsEqualTo(AreaIndex other) => this.x == other.x && this.y == other.y && this.z == other.z;

        public override string ToString() => $"({this.x}, {this.y}, {this.z})";
    }

    public class WorldSystemWithDistanceLevels<T> : IWorldSystem<T>
    {
        private readonly WorldSystem<T>[] _worldSystems;
        public IReadOnlyList<WorldSystem<T>> WorldSystems => _worldSystems;
        private readonly float[] _distanceLevels;
        public IReadOnlyList<float> DistanceLevels => _distanceLevels;
        private readonly Dictionary<long, FocusPoint[]> _focusPointsPerLevel = new Dictionary<long, FocusPoint[]>();

        public WorldSystemWithDistanceLevels(
            float[] distanceLevels,
            WorldSystemParams[] worldSystemParamsXZ,
            WorldSystemParams[] worldSystemParamsY,
            System.Action<WorldSystem<T>.Area, bool> onAreaChangedVisibility)
        {
            int num = distanceLevels.Length;
            if (num != worldSystemParamsXZ.Length || num != worldSystemParamsY.Length)
                throw new ArgumentException("Input arrays must be of same size");

            if (num == 0)
                throw new ArgumentException("You must specify distance levels");

            if (!distanceLevels.OrderBy(l => l).SequenceEqual(distanceLevels))
                throw new ArgumentException("Input arrays must be sorted ascending by distance level");

            if (distanceLevels.Distinct().Count() != distanceLevels.Length)
                throw new ArgumentException("Distance levels must be distinct");

            _distanceLevels = distanceLevels.ToArray();

            _worldSystems = new WorldSystem<T>[num];
            for (int i = 0; i < num; i++)
            {
                _worldSystems[i] = new WorldSystem<T>(worldSystemParamsXZ[i], worldSystemParamsY[i], onAreaChangedVisibility);
            }
        }

        public int GetLevelIndexFromDrawDistance(float drawDistance)
        {
            if (drawDistance <= 0)
                return 0;

            int index = System.Array.FindIndex(_distanceLevels, f => f > drawDistance);
            if (index >= 0)
                return index;

            // draw distance is higher than all levels
            // these object should go into last level
            return _distanceLevels.Length - 1;
        }

        public FocusPoint RegisterFocusPoint(float radius, Vector3 pos)
        {
            var focusPoints = new FocusPoint[_worldSystems.Length];
            for (int i = 0; i < _worldSystems.Length; i++)
                focusPoints[i] = _worldSystems[i].RegisterFocusPoint(Mathf.Min(radius, _distanceLevels[i]), pos);

            var focusPointToReturn = new FocusPoint(radius, pos);

            _focusPointsPerLevel[focusPointToReturn.Id] = focusPoints;

            return focusPointToReturn;
        }

        public void UnRegisterFocusPoint(FocusPoint focusPoint)
        {
            if (!_focusPointsPerLevel.TryGetValue(focusPoint.Id, out var focusPoints))
                return;

            for (int i = 0; i < _worldSystems.Length; i++)
                _worldSystems[i].UnRegisterFocusPoint(focusPoints[i]);
        }

        public void FocusPointChangedParameters(
            FocusPoint focusPoint,
            Vector3 newPos,
            float newRadius)
        {
            if (!_focusPointsPerLevel.TryGetValue(focusPoint.Id, out var focusPoints))
                return;

            Vector3 diff = focusPoints[0].Position - newPos;
            float radiusDiff = focusPoints[0].Radius - newRadius;
            if (diff.x == 0f && diff.y == 0f && diff.z == 0f && radiusDiff == 0f) // faster than calling '==' Vector3 operator
                return;

            for (int i = 0; i < _worldSystems.Length; i++)
                _worldSystems[i].FocusPointChangedParameters(focusPoints[i], newPos, Mathf.Min(newRadius, _distanceLevels[i]));

            focusPoint.Position = newPos;
            focusPoint.Radius = newRadius;
        }

        public AreaIndex GetAreaIndex(Vector3 pos)
        {
            int index = this.GetLevelIndexFromDrawDistance(float.PositiveInfinity);
            return _worldSystems[index].GetAreaIndex(pos);
        }

        public void AddObjectToArea(Vector3 pos, T obj)
        {
            throw new NotSupportedException($"You probably want to use {nameof(AddObjectToArea)}() with draw distance parameter");
        }

        public void RemoveObjectFromArea(Vector3 pos, T obj)
        {
            throw new NotSupportedException($"You probably want to use {nameof(RemoveObjectFromArea)}() with draw distance parameter");
        }

        public void AddObjectToArea(Vector3 pos, float drawDistance, T obj)
        {
            int levelIndex = this.GetLevelIndexFromDrawDistance(drawDistance);
            _worldSystems[levelIndex].AddObjectToArea(pos, obj);
        }

        public void RemoveObjectFromArea(Vector3 pos, float drawDistance, T obj)
        {
            int levelIndex = this.GetLevelIndexFromDrawDistance(drawDistance);
            _worldSystems[levelIndex].RemoveObjectFromArea(pos, obj);
        }

        public void Update()
        {
            for (int i = 0; i < _worldSystems.Length; i++)
                _worldSystems[i].Update();
        }
    }

    internal static class FocusPointIdGenerator
    {
        // static fields should not be in generic classes, and therefore we need non-generic class for storing last id
        private static long _sLastId = 0;

        internal static long GetNextId()
        {
            return ++_sLastId;
        }
    }

    internal static class AreaIdGenerator
    {
        // static fields should not be in generic classes, and therefore we need non-generic class for storing last id
        private static long _sLastId = 0;

        internal static long GetNextId()
        {
            return ++_sLastId;
        }
    }

    public class WorldSystem<T> : IWorldSystem<T>
    {
        public class Area
        {
            public long Id { get; } = AreaIdGenerator.GetNextId();

            public AreaIndex AreaIndex { get; }

            public WorldSystem<T> WorldSystem { get; }

            internal List<T> objectsInside;
            public IReadOnlyList<T> ObjectsInside => this.objectsInside;

            internal HashSet<FocusPoint> focusPointsThatSeeMe;
            public IReadOnlyCollection<FocusPoint> FocusPointsThatSeeMe => this.focusPointsThatSeeMe;

            internal bool isMarkedForUpdate;

            public bool WasVisibleInLastUpdate { get; internal set; } = false;

            internal Area(WorldSystem<T> worldSystem, AreaIndex areaIndex)
            {
                this.WorldSystem = worldSystem;
                this.AreaIndex = areaIndex;
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

        private struct AxisInfo
        {
            public float worldMin;
            public float worldMax;
            public float worldHalfSize;
            public float areaSize;
            public ushort numAreasPerAxis;
        }

        public class ConcurrentModificationException : System.Exception
        {
            public ConcurrentModificationException()
                : base("Can not perform the operation because it would result in concurrent modification of collections")
            {
            }
        }

        private readonly Area[,,] _areas;
        public int GetNumAreas(int axisIndex) => _areas.GetLength(axisIndex);

        private readonly HashSet<FocusPoint> _focusPoints = new HashSet<FocusPoint>();
        public IReadOnlyCollection<FocusPoint> FocusPoints => _focusPoints;

        private readonly List<Area> _areasForUpdate = new List<Area>(128);

        private readonly AxisInfo _xzAxisInfo;

        private readonly AxisInfo _yAxisInfo;

        private bool _isInUpdate = false;

        // these buffers are reused every time to avoid memory allocations, but that makes this class non thread safe
        private AreaIndexes[] _bufferForGettingNewAreas = new AreaIndexes[27]; // 3^3
        private AreaIndexes[] _bufferForGettingOldAreas = new AreaIndexes[27]; // 3^3

        private readonly System.Action<Area, bool> _onAreaChangedVisibility = null;

        public WorldSystem(
            WorldSystemParams worldSystemParamsXZ,
            WorldSystemParams worldSystemParamsY,
            System.Action<Area, bool> onAreaChangedVisibility)
        {
            _xzAxisInfo = CalculateAxisInfo(worldSystemParamsXZ);
            _yAxisInfo = CalculateAxisInfo(worldSystemParamsY);

            _areas = new Area[_xzAxisInfo.numAreasPerAxis, _yAxisInfo.numAreasPerAxis, _xzAxisInfo.numAreasPerAxis];

            _onAreaChangedVisibility = onAreaChangedVisibility;
        }

        private static AxisInfo CalculateAxisInfo(WorldSystemParams worldSystemParams)
        {
            if (worldSystemParams.worldSize <= 0)
                throw new ArgumentException("World size must be higher than 0");
            ushort maxNumAreasPerAxis = ushort.MaxValue - 10;
            if (worldSystemParams.numAreasPerAxis > maxNumAreasPerAxis)
                throw new ArgumentException($"Num areas per axis can not be higher than {maxNumAreasPerAxis}");

            AxisInfo axisInfo = new AxisInfo();
            axisInfo.worldMin = - worldSystemParams.worldSize / 2f;
            axisInfo.worldMax = worldSystemParams.worldSize / 2f;
            axisInfo.worldHalfSize = worldSystemParams.worldSize / 2f;
            axisInfo.areaSize = worldSystemParams.worldSize / (float)worldSystemParams.numAreasPerAxis;
            axisInfo.numAreasPerAxis = (ushort) (worldSystemParams.numAreasPerAxis + 2); // additional 2 for positions out of bounds
            return axisInfo;
        }

        public FocusPoint RegisterFocusPoint(float radius, Vector3 pos)
        {
            this.ThrowIfConcurrentModification();

            if (radius < 0)
                throw new ArgumentException("Radius can not be < 0");

            var focusPoint = new FocusPoint(radius, pos);

            if (!_focusPoints.Add(focusPoint))
                throw new Exception("Failed to add focus point to the collection");

            this.ForEachAreaInRadius(pos, radius, true, area =>
            {
                AddToFocusPointsThatSeeMe(area, focusPoint);
                this.MarkAreaForUpdate(area);
            });

            return focusPoint;
        }

        public void UnRegisterFocusPoint(FocusPoint focusPoint)
        {
            this.ThrowIfConcurrentModification();

            if (!_focusPoints.Remove(focusPoint))
                return;

            this.ForEachAreaInRadius(focusPoint.Position, focusPoint.Radius, false, area =>
            {
                if (null == area)
                    return;
                RemoveFromFocusPointsThatSeeMe(area, focusPoint);
                this.MarkAreaForUpdate(area);
            });
        }

        public void FocusPointChangedParameters(FocusPoint focusPoint, Vector3 newPos, float newRadius)
        {
            this.ThrowIfConcurrentModification();

            AreaIndexes oldIndexes = GetAreaIndexesInRadius(focusPoint.Position, focusPoint.Radius);
            AreaIndexes newIndexes = GetAreaIndexesInRadius(newPos, newRadius);

            if (!oldIndexes.EqualsToOther(newIndexes))
            {
                // areas changed

                byte numNewAreaIndexes = GetNewAreas(oldIndexes, newIndexes, _bufferForGettingNewAreas);
                byte numOldAreaIndexes = GetNewAreas(newIndexes, oldIndexes, _bufferForGettingOldAreas);

                for (byte i = 0; i < numNewAreaIndexes; i++)
                {
                    var areaIndexes = _bufferForGettingNewAreas[i];

                    this.ForEachArea(areaIndexes, true, area =>
                    {
                        // this can happen multiple times per single area, but since we use hashset it should be no problem
                        // actually, it should not happen anymore with new implementation
                        AddToFocusPointsThatSeeMe(area, focusPoint);
                        this.MarkAreaForUpdate(area);
                    });
                }

                for (byte i = 0; i < numOldAreaIndexes; i++)
                {
                    var areaIndexes = _bufferForGettingOldAreas[i];

                    this.ForEachArea(areaIndexes, false, area =>
                    {
                        if (null == area)
                            return;
                        RemoveFromFocusPointsThatSeeMe(area, focusPoint);
                        this.MarkAreaForUpdate(area);
                    });
                }

            }

            focusPoint.Position = newPos;
            focusPoint.Radius = newRadius;
        }

        public void AddObjectToArea(Area area, T obj)
        {
            this.ThrowIfConcurrentModification();

            if (null == area.objectsInside)
                area.objectsInside = new List<T>();
            area.objectsInside.Add(obj);
        }

        public void AddObjectToArea(Vector3 pos, T obj)
        {
            this.ThrowIfConcurrentModification();

            var area = this.GetAreaAt(pos, true);
            this.AddObjectToArea(area, obj);
        }

        public void RemoveObjectFromArea(Vector3 pos, T obj)
        {
            this.ThrowIfConcurrentModification();

            var area = GetAreaAt(pos, false);

            if (area != null && area.objectsInside != null)
            {
                area.objectsInside.Remove(obj);
                if (area.objectsInside.Count == 0)
                    area.objectsInside = null;
            }
        }

        public void RemoveAllObjectsFromArea(Area area)
        {
            this.ThrowIfConcurrentModification();

            area.objectsInside = null;
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

                bool isVisible = ShouldAreaBeVisible(area);

                this.NotifyAreaChangedVisibility(area, isVisible);

                area.WasVisibleInLastUpdate = isVisible;
                area.isMarkedForUpdate = false;
            }

            _areasForUpdate.Clear();
        }

        public void ForEachAreaInRadius(Vector3 pos, float radius, System.Action<Area> action)
        {
            this.ForEachAreaInRadius(pos, radius, false, action);
        }

        private void ForEachAreaInRadius(Vector3 pos, float radius, bool createIfNotExists, System.Action<Area> action)
        {
            this.ForEachArea(GetAreaIndexesInRadius(pos, radius), createIfNotExists, action);
        }

        public List<Area> GetAreasInRadius(Vector3 pos, float radius)
        {
            var areas = new List<Area>();
            this.ForEachAreaInRadius(pos, radius, false, a => areas.Add(a));
            return areas;
        }

        private void ForEachArea(AreaIndexes areaIndexes, bool createIfNotExists, System.Action<Area> action)
        {
            for (short x = areaIndexes.x.lower; x <= areaIndexes.x.higher; x++)
            {
                for (short y = areaIndexes.y.lower; y <= areaIndexes.y.higher; y++)
                {
                    for (short z = areaIndexes.z.lower; z <= areaIndexes.z.higher; z++)
                    {
                        if (!createIfNotExists)
                        {
                            action(_areas[x, y, z]);
                        }
                        else
                        {
                            var area = _areas[x, y, z];
                            if (null == area)
                                _areas[x, y, z] = area = new Area(this, new AreaIndex(x, y, z));
                            action(area);
                        }
                    }
                }
            }
        }

        private AreaIndexes GetAreaIndexesInRadius(Vector3 pos, float radius)
        {
            if (radius < 0)
                throw new ArgumentException("Radius can not be < 0");

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
            if (pos < _xzAxisInfo.worldMin)
                return 0;
            if (pos > _xzAxisInfo.worldMax)
                return (short) (_xzAxisInfo.numAreasPerAxis - 1);

            // skip 1st
            return (short) (1 + Mathf.FloorToInt((pos + _xzAxisInfo.worldHalfSize) / _xzAxisInfo.areaSize));
        }

        private short GetAreaIndexForYAxis(float pos)
        {
            if (pos < _yAxisInfo.worldMin)
                return 0;
            if (pos > _yAxisInfo.worldMax)
                return (short) (_yAxisInfo.numAreasPerAxis - 1);

            // skip 1st
            return (short) (1 + Mathf.FloorToInt((pos + _yAxisInfo.worldHalfSize) / _yAxisInfo.areaSize));
        }

        public AreaIndex GetAreaIndex(Vector3 pos)
        {
            return new AreaIndex(GetAreaIndex(pos.x), GetAreaIndexForYAxis(pos.y), GetAreaIndex(pos.z));
        }

        public Vector3 GetAreaCenter(Area area)
        {
            Vector3 center = new Vector3(
                GetAreaCenterForXZAxis(area.AreaIndex.x),
                GetAreaCenterForYAxis(area.AreaIndex.y),
                GetAreaCenterForXZAxis(area.AreaIndex.z));

            AreaIndex indexOfCenter = GetAreaIndex(center);
            if (!indexOfCenter.IsEqualTo(area.AreaIndex)) // just to be sure
                throw new Exception($"Index of area center {indexOfCenter} does not match original area index {area.AreaIndex}");

            return center;
        }

        private float GetAreaCenterForXZAxis(short indexForAxis)
        {
            if (indexForAxis <= 0) // left infinity
                return float.NegativeInfinity;

            if (indexForAxis >= _xzAxisInfo.numAreasPerAxis - 1) // right infinity
                return float.PositiveInfinity;

            return _xzAxisInfo.areaSize * (indexForAxis - 1) - _xzAxisInfo.worldHalfSize + _xzAxisInfo.areaSize * 0.5f;
        }

        private float GetAreaCenterForYAxis(short indexForAxis)
        {
            if (indexForAxis <= 0) // left infinity
                return float.NegativeInfinity;

            if (indexForAxis >= _yAxisInfo.numAreasPerAxis - 1) // right infinity
                return float.PositiveInfinity;

            return _yAxisInfo.areaSize * (indexForAxis - 1) - _yAxisInfo.worldHalfSize + _yAxisInfo.areaSize * 0.5f;
        }

        private Area GetAreaAt(Vector3 pos, bool createIfNotExists)
        {
            var index = this.GetAreaIndex(pos);
            var area = _areas[index.x, index.y, index.z];
            if (null == area && createIfNotExists)
            {
                area = new Area(this, index);
                _areas[index.x, index.y, index.z] = area;
            }
            return area;
        }

        public Area GetAreaAt(Vector3 pos)
        {
            return this.GetAreaAt(pos, false);
        }

        public Area GetAreaAt(AreaIndex areaIndex)
        {
            return _areas[areaIndex.x, areaIndex.y, areaIndex.z];
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
            if (oldRange.EqualsToOther(newRange))
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

            if (newRange.EqualsToOther(intersectionRange))
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
                area.focusPointsThatSeeMe = new HashSet<FocusPoint>();
        }

        private static bool ShouldAreaBeVisible(Area area)
        {
            return area.focusPointsThatSeeMe != null && area.focusPointsThatSeeMe.Count > 0;
        }

        private void MarkAreaForUpdate(Area area)
        {
            this.ThrowIfConcurrentModification(); // just in case

            if (area.isMarkedForUpdate)
                return;

            if (ShouldAreaBeVisible(area) == area.WasVisibleInLastUpdate) // visibility does not change
                return;

            _areasForUpdate.Add(area);
            area.isMarkedForUpdate = true;
        }

        private static void AddToFocusPointsThatSeeMe(Area area, FocusPoint focusPoint)
        {
            EnsureFocusPointsCollectionInitialized(area);
            if (!area.focusPointsThatSeeMe.Add(focusPoint))
                throw new Exception($"Failed to add focus point with id {focusPoint.Id} - it already exists");
        }

        private static void RemoveFromFocusPointsThatSeeMe(Area area, FocusPoint focusPoint)
        {
            bool success = false;
            if (area.focusPointsThatSeeMe != null)
            {
                success = area.focusPointsThatSeeMe.Remove(focusPoint);
                if (area.focusPointsThatSeeMe.Count == 0)
                    area.focusPointsThatSeeMe = null;
            }

            if (!success)
                throw new Exception($"Failed to remove focus point with id {focusPoint.Id} - it doesn't exist");
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
