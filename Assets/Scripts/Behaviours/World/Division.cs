using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.World
{
    public class Division : MonoBehaviour, IEnumerable<Division>, IComparable<Division>
    {
        private static readonly Comparison<MapObject> _sHorzSort =
            (a, b) => Math.Sign(a.CellPos.x - b.CellPos.x);

        private static readonly Comparison<MapObject> _sVertSort =
            (a, b) => Math.Sign(a.CellPos.y - b.CellPos.y);

        public static Division Create(Transform parent)
        {
            var obj = new GameObject();
            var split = obj.AddComponent<Division>();

            obj.transform.SetParent(parent);

            return split;
        }

        private const int LeafObjectLimit = 127;

        private Division _childA;
        private Division _childB;

        private List<MapObject> _objects;
        public int NumObjects { get { return _objects.Count; } }

        public int NumObjectsIncludingChildren
        {
            get
            {
                int count = _objects.Count;
                if (_childA != null)
                    count += _childA.NumObjectsIncludingChildren;
                if (_childB != null)
                    count += _childB.NumObjectsIncludingChildren;
                return count;
            }
        }

        private bool _isVertSplit;
        private float _splitVal;

        private Vector3 _lastRefreshPos;

        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }
        private Bounds _bounds;

        public bool IsSubdivided { get { return _objects == null; } }

        internal float LoadOrder { get; private set; }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
            _bounds = new Bounds(((this.Min + this.Max) * 0.5f).ToVector3XZ(), (this.Max - this.Min).ToVector3XZ().WithY(10000f));

            var mid = (Max + Min) * .5f;

            if (float.IsNaN(mid.x) || float.IsInfinity(mid.x))
            {
                mid.x = 0f;
            }

            if (float.IsNaN(mid.y) || float.IsInfinity(mid.y))
            {
                mid.y = 0f;
            }

            transform.position = new Vector3(mid.x, 0f, mid.y);

            name = String.Format("Split {0}, {1}", min, max);

            _objects = _objects ?? new List<MapObject>();
        }

        private void Subdivide()
        {
            if (IsSubdivided)
            {
                throw new InvalidOperationException("Already subdivided");
            }

            if (_objects.Count == 0)
            {
                throw new InvalidOperationException("Cannot subdivide an empty leaf");
            }

            var min = Max;
            var max = Min;

            foreach (var obj in _objects)
            {
                var pos = obj.CellPos;
                min.x = Mathf.Min(pos.x, min.x);
                min.y = Mathf.Min(pos.y, min.y);
                max.x = Mathf.Max(pos.x, max.x);
                max.y = Mathf.Max(pos.y, max.y);
            }

            _isVertSplit = max.x - min.x >= max.y - min.y;

            _objects.Sort(_isVertSplit ? _sHorzSort : _sVertSort);

            _childA = Create(transform);
            _childB = Create(transform);

            var mid = _objects.Count / 2;
            var median = (_objects[mid - 1].CellPos + _objects[mid].CellPos) * .5f;

            if (_isVertSplit)
            {
                _splitVal = median.x;
                _childA.SetBounds(Min, new Vector2(_splitVal, Max.y));
                _childB.SetBounds(new Vector2(_splitVal, Min.y), Max);
            }
            else
            {
                _splitVal = median.y;
                _childA.SetBounds(Min, new Vector2(Max.x, _splitVal));
                _childB.SetBounds(new Vector2(Min.x, _splitVal), Max);
            }

            _childA._objects = _objects;
            _childB._objects = new List<MapObject>();

            _childB._objects.AddRange(_childA._objects.Skip(mid));
            _childA._objects.RemoveRange(mid, _objects.Count - mid);

            _objects = null;
        }

        private void AddInternal(MapObject obj)
        {
            if (IsSubdivided)
            {
                var comp = _isVertSplit ? obj.CellPos.x : obj.CellPos.y;
                (comp < _splitVal ? _childA : _childB).AddInternal(obj);
                return;
            }

            _objects.Add(obj);

            if (_objects.Count > LeafObjectLimit) Subdivide();
        }

        public void Add(MapObject obj)
        {
            AddInternal(obj);
            UpdateParents();
        }

        public void AddRange(IEnumerable<MapObject> objs)
        {
            foreach (var obj in objs.OrderBy(x => x.RandomInt))
            {
                AddInternal(obj);
            }

            UpdateParents();
        }

        private void UpdateParents()
        {
            if (IsSubdivided)
            {
                _childA.UpdateParents();
                _childB.UpdateParents();
            }
            else
            {
                if (_objects.Count == 0) return;

                var sum = _objects.Aggregate(new Vector2(), (s, x) => s + x.CellPos);
                transform.position = new Vector3(sum.x / _objects.Count, 0f, sum.y / _objects.Count);

                foreach (var obj in _objects)
                {
                    obj.transform.SetParent(transform, true);
                }
            }
        }

        public bool Contains(MapObject obj)
        {
            return Contains(obj.CellPos);
        }

        public bool Contains(Vector3 pos)
        {
            return pos.x >= Min.x && pos.z >= Min.y && pos.x < Max.x && pos.z < Max.y;
        }

        public bool Contains(Vector2 pos)
        {
            return pos.x >= Min.x && pos.y >= Min.y && pos.x < Max.x && pos.y < Max.y;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = ((this.Min + this.Max) * 0.5f).ToVector3XZ();
            Vector3 size = (this.Max - this.Min).ToVector3XZ();
            size.y = 100;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);

            F.HandlesDrawText(center, 
                string.Format("total objects {0}, my objects {1}, load order {2}", _objects != null ? this.NumObjectsIncludingChildren : 0, _objects != null ? this.NumObjects : 0, this.LoadOrder),
                Color.green);

            /*
            if (!IsSubdivided) return;

            Gizmos.color = Color.green;

            var min = new Vector2(Math.Max(Min.x, -8192f), Math.Max(Min.y, -8192f));
            var max = new Vector2(Math.Min(Max.x, +8192f), Math.Min(Max.y, +8192f));

            if (_isVertSplit)
            {
                Gizmos.DrawLine(new Vector3(_splitVal, 0f, min.y), new Vector3(_splitVal, 0f, max.y));
            }
            else
            {
                Gizmos.DrawLine(new Vector3(min.x, 0f, _splitVal), new Vector3(max.x, 0f, _splitVal));
            }
            */

        }

        public float GetDistance(Vector3 pos)
        {
            return Mathf.Sqrt(GetDistanceSquared(pos));
        }

        public float GetDistanceSquared(Vector3 pos)
        {
            pos.y = 0f; // only count X and Z axis
            // get the closest point on bounds
            // if position is inside bounds, the resulting distance will be 0
            Vector3 closestPos = _bounds.ClosestPoint(pos);
            return Vector3.SqrMagnitude(pos - closestPos);
        }

		public Vector3 GetClosestPosition (List<Vector3> positions)
		{
			Vector3 closestPos = positions [0];
			float smallestDist2 = float.MaxValue;
			Vector2 center = (this.Min + this.Max) * 0.5f;

			for (int i = 0; i < positions.Count; i++)
			{
				float dist2 = Vector2.SqrMagnitude (positions [i].ToVec2WithXAndZ() - center);
				if (dist2 <= smallestDist2)
				{
					smallestDist2 = dist2;
					closestPos = positions [i];
				}
			}

			return closestPos;
		}

        public bool RefreshLoadOrder(Vector3 from, out int numMapObjectsUpdatedLoadOrder)
        {
			UnityEngine.Profiling.Profiler.BeginSample ("Division.RefreshLoadOrder", this);

            var toLoad = false;
            numMapObjectsUpdatedLoadOrder = 0;

            if (GetDistanceSquared(from) <= Cell.Instance.maxDrawDistance * Cell.Instance.maxDrawDistance)
            {
                float divisionRefreshDistanceDeltaSquared = Cell.Instance.divisionRefreshDistanceDelta * Cell.Instance.divisionRefreshDistanceDelta;
                //	float factor = Cell.Instance.divisionLoadOrderDistanceFactor; //16;
                //	if (Vector3.SqrMagnitude(from - _lastRefreshPos) > GetDistanceSquared(from) / (factor*factor)) {
                if (Vector3.SqrMagnitude(from - _lastRefreshPos) > divisionRefreshDistanceDeltaSquared)
                {
                    _lastRefreshPos = from;
                    foreach (var obj in _objects)
                    {
                        bool b = obj.RefreshLoadOrder(from);
                        if (b)
                        {
                            toLoad = true;
                            numMapObjectsUpdatedLoadOrder++;
                        }
                    }
                }
                else
                {
                    toLoad = _objects.Any(x => !float.IsPositiveInfinity(x.LoadOrder));
                }
            }

            if (toLoad)
            {
                _objects.Sort();	// THIS MAY BE PERFORMANCE DROP
                LoadOrder = _objects[0].LoadOrder;
            }
            else
            {
                LoadOrder = float.PositiveInfinity;
            }

			UnityEngine.Profiling.Profiler.EndSample ();

            return toLoad;
        }

        public int LoadWhile(Func<bool> predicate)
        {
			UnityEngine.Profiling.Profiler.BeginSample ("LoadWhile", this);

            int numLoaded = 0;
            foreach (var toLoad in _objects)
            {
                if (float.IsPositiveInfinity(toLoad.LoadOrder))
                    break;

                if (toLoad.HasLoaded)
                {
                    // this object is loaded, just show it
                    toLoad.Show();
                }
                else
                {
                    // this object is still not loaded from disk
                    // check if we should load it
                    if (predicate())
                    {
                        toLoad.Show();
                        numLoaded++;
                    }
                }
            }

			UnityEngine.Profiling.Profiler.EndSample ();

            //    return predicate();
            //	return false ;
            return numLoaded;
        }

        public IEnumerator<Division> GetEnumerator()
        {
            if (IsSubdivided)
            {
                return _childA.Concat(_childB).GetEnumerator();
            }

            return new[] { this }.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CompareTo(Division other)
        {
            return LoadOrder > other.LoadOrder ? 1 : LoadOrder == other.LoadOrder ? 0 : -1;
        }
    }
}