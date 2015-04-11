using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class Loader : MonoBehaviour
    {
        public static bool HasLoaded { get; private set; }

        private void Awake()
        {
            if (HasLoaded) return;

            var archivePaths = Config.GetPaths("archive_paths");

            IArchive[] archives;
            using (Utilities.Profiler.Start("Archive load time")) {
                archives = archivePaths.Select(x => 
                    File.Exists(x) ? (IArchive) ArchiveManager.LoadImageArchive(x)
                    : Directory.Exists(x) ? ArchiveManager.LoadLooseArchive(x)
                    : null).Where(x => x != null).ToArray();
            }
            
            using (Utilities.Profiler.Start("Collision load time")) {
                foreach (var archive in archives) {
                    foreach (var colFile in archive.GetFileNamesWithExtension(".col")) {
                        CollisionFile.Load(colFile);
                    }
                }
            }

            using (Utilities.Profiler.Start("Item info load time")) {
                foreach (var path in Config.GetPaths("item_paths")) {
                    var ext = Path.GetExtension(path).ToLower();
                    switch (ext) {
                        case ".dat":
                            Item.ReadLoadList(path); break;
                        case ".ide":
                            Item.ReadIde(path); break;
                        case ".ipl":
                            Item.ReadIpl(path); break;
                    }
                }
            }

            using (Utilities.Profiler.Start("Handling info load time")) {
                Handling.Load(Config.GetPath("handling_path"));
            }

            using (Utilities.Profiler.Start("Animation group info load time")) {
                AnimationGroup.Load(Config.GetPath("anim_group_path"));
            }

            HasLoaded = true;
        }
    }
}
