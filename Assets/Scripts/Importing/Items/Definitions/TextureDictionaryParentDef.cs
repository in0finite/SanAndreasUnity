using SanAndreasUnity.Importing.Conversion;
using UnityEngine;

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
