namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Section("weap")]
    public class WeaponDef : Definition, IObjectDefinition
    {
        public readonly int Id;

        int IObjectDefinition.Id
        {
            get { return Id; }
        }

        public readonly string ModelName;
        public readonly string TextureDictionaryName;
        public readonly string AnimationFileName;
        public readonly int NumMeshes;  // always 1
        public readonly float DrawDistance;
        public readonly int Flags;  // doesn't seem to be read by the game

        public WeaponDef(string line)
            : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            AnimationFileName = GetString(3);
            NumMeshes = GetInt(4);
            DrawDistance = GetSingle(5);
            Flags = GetInt(6);
        }
    }
}