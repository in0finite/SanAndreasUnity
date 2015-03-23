using System;
using System.IO;
using System.Linq;

namespace SanAndreasUnity.Importing.Archive
{
    public static class ImagePath
    {
        public const String GameDir = @"C:\Program Files (x86)\Steam\SteamApps\common\Grand Theft Auto San Andreas";

        public static String ModelsDir { get { return Path.Combine(GameDir, "models"); } }
        public static String DataDir { get { return Path.Combine(GameDir, "data"); } }

        public static String GetPath(params String[] relative)
        {
            return relative.Aggregate(GameDir, Path.Combine);
        }
    }
}
