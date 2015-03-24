using UnityEngine;

namespace SanAndreasUnity.Importing.Items
{
    [Section("zone")]
    internal class Zone : Item
    {
        public readonly string Name;

        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public Zone(string line) : base(line)
        {
            Name = GetString(0);

            Min = new Vector3(GetSingle(2), GetSingle(4), GetSingle(3));
            Max = new Vector3(GetSingle(5), GetSingle(7), GetSingle(6));
        }
    }
}
