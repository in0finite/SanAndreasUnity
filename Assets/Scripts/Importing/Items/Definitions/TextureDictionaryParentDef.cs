using SanAndreasUnity.Importing.Conversion;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Section("txdp")]
    public class TextureDictionaryParentDef : Definition
    {
        public TextureDictionaryParentDef(string line)
            : base(line)
        {
            TextureDictionary.AddParent(GetString(0), GetString(1));
        }
    }
}