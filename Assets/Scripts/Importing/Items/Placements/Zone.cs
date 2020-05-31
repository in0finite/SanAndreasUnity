namespace SanAndreasUnity.Importing.Items.Placements
{
    [Section("zone")]
    public class Zone : Placement
    {
        public readonly string Name;

        public readonly UnityEngine.Vector3 Min;
        public readonly UnityEngine.Vector3 Max;

        public Zone(string line) : base(line)
        {
            Name = GetString(0);

            Min = new UnityEngine.Vector3(GetSingle(2), GetSingle(4), GetSingle(3));
            Max = new UnityEngine.Vector3(GetSingle(5), GetSingle(7), GetSingle(6));
        }
    }
}