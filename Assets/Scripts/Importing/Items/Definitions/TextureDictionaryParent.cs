using SanAndreasUnity.Importing.Conversion;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Section("txdp")]
    public class TextureDictionaryParent : Definition
    {
        public TextureDictionaryParent(string line)
            : base(line)
        {
            TextureDictionary.AddParent(GetString(0), GetString(1));
        }
    }
}
