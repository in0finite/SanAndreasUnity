using SanAndreasUnity.Importing.Items.Placements;
using System.Collections.Generic;
using System.IO;

namespace SanAndreasUnity.Importing.Items
{
    public class WaterFile
    {
        public readonly WaterFace[] Faces;

        public WaterFile(string path)
        {
            var faces = new List<WaterFace>();

            using (var reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Length == 0) continue;
                    if (line.StartsWith("#")) continue;
                    if (line.StartsWith("processed")) continue;

                    faces.Add(new WaterFace(line));
                }
            }

            UnityEngine.Debug.Log("Loading water file: \"" + path + "\" with " + faces.Count + " faces");

            Faces = faces.ToArray();
        }
    }
}