using System;
using System.Collections;
using System.Collections.Generic;

namespace Facepunch.Networking
{
    public class IdentifierSet : IEnumerable<uint>
    {
        private readonly List<uint> _offsets;
        private readonly uint _min;

        public IdentifierSet(uint min = 0)
        {
            _min = min;
            _offsets = new List<uint> { min, uint.MaxValue };
        }

        public uint Allocate()
        {
            lock (this) {
                var value = _offsets[0]++;

                if (_offsets[1] == _offsets[0]) {
                    _offsets[0] = _offsets[2];
                    _offsets.RemoveRange(1, 2);
                }

                return value;
            }
        }

        // TODO: Do an O(log n) search

        private bool TryInsert(uint ident, bool set)
        {
            if (ident < _min) return false;

            lock (this) {
                uint a = 0, b, p = 0;
                for (var i = set ? 1 : 0; i < _offsets.Count; i += 2) {
                    b = _offsets[i];

                    if (set) {
                        a = _offsets[i - 1];
                    } else {
                        p = _offsets[i + 1];
                    }

                    if (ident < a) return false;
                    if (ident >= b) {
                        a = p;
                        continue;
                    }

                    if (ident == a) {
                        _offsets[i - 1] = a + 1;
                        break;
                    }

                    _offsets[i] = ident;

                    if (ident == b - 1) {
                        break;
                    }

                    _offsets.InsertRange(i + 1, new[] { ident + 1, b });

                    break;
                }
            }

            return true;
        }

        public bool TryAssign(uint ident)
        {
            return TryInsert(ident, true);
        }

        public bool TryFree(uint ident)
        {
            return TryInsert(ident, false);
        }

        public IEnumerator<uint> GetEnumerator()
        {
            uint index = 0;
            for (var i = 0; i < _offsets.Count; i += 2) {
                for (; index < _offsets[i]; ++index) {
                    if (index >= _min) yield return index;
                }

                index = _offsets[i + 1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
