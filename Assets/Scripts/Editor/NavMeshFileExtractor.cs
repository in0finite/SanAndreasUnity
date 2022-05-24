using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace SanAndreasUnity.Editor
{
    [InitializeOnLoad]
    public class NavMeshFileExtractor
    {
        private const string NavMeshesFolderPath = "NavMeshes/";
        private const string NavMeshesArchiveFileName = "NavMeshes.zip";


        static NavMeshFileExtractor()
        {
            string fullFolderPath = Path.Combine(Application.dataPath, NavMeshesFolderPath);

            if (!Directory.Exists(fullFolderPath))
            {
                Debug.LogError("Failed to extract nav mesh files: folder with nav meshes not found");
                return;
            }

            string zipFilePath = Path.Combine(fullFolderPath, NavMeshesArchiveFileName);
            if (!File.Exists(zipFilePath))
                return;

            Debug.Log("Attempting to extract nav mesh files");

            var extractedFileNames = new List<string>();
            long totalFileSizeExtracted = 0;

            using var zipArchive = new ZipArchive(File.OpenRead(zipFilePath), ZipArchiveMode.Read);
            foreach (var entry in zipArchive.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                    continue;

                using var writeStream = File.OpenWrite(Path.Combine(fullFolderPath, entry.Name));
                using var entryStream = entry.Open();
                entryStream.CopyTo(writeStream, 4 * 1024 * 1024);
                writeStream.Flush(true);

                writeStream.Dispose();
                entryStream.Dispose();

                extractedFileNames.Add(entry.Name);
                totalFileSizeExtracted += entry.Length;
            }

            // close archive so it can be deleted
            zipArchive.Dispose();

            // delete zip file to reduce size of project, and to prevent it from being extracted again
            File.Delete(zipFilePath);

            // also delete it's meta file
            string metaFilePath = zipFilePath + ".meta";
            if (File.Exists(metaFilePath))
                File.Delete(metaFilePath);

            AssetDatabase.Refresh();

            Debug.Log($"Successfully extracted nav mesh files, total extracted files' size {totalFileSizeExtracted}, " +
                $"files extracted:\n{string.Join("\n", extractedFileNames)}");
        }
    }
}
