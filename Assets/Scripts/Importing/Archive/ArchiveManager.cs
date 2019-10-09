using SanAndreasUnity.Importing.RenderWareStream;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace SanAndreasUnity.Importing.Archive
{
    public interface IArchive
    {
        IEnumerable<string> GetFileNamesWithExtension(string ext);

        bool ContainsFile(string name);

        Stream ReadFile(string name);
    }

	/// <summary>
	/// Handles archive loading and reading. You should never read from archives manually, but always use this class, because it provides thread safety.
	/// </summary>
    public static class ArchiveManager
    {
        public static string ModelsDir { get { return Path.Combine(Config.GamePath, "models"); } }
        public static string DataDir { get { return Path.Combine(Config.GamePath, "data"); } }

        public static string GetPath(params string[] relative)
        {
            return relative.Aggregate(Config.GamePath, Path.Combine).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetCaseSensitiveFilePath(string fileName)
        {
            string filePath = null;
            foreach(var archive in _sLoadedArchives.OfType<LooseArchive>())
            {
                if (archive.GetCaseSensitiveFilePath(fileName, ref filePath))
                    return filePath;
            }
            throw new FileNotFoundException(fileName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string PathToCaseSensitivePath(string path)
        {
            return ArchiveManager.GetCaseSensitiveFilePath(Path.GetFileName(path));
        }

        private static readonly List<IArchive> _sLoadedArchives = new List<IArchive>();

		[MethodImpl(MethodImplOptions.Synchronized)]
        public static LooseArchive LoadLooseArchive(string dirPath)
        {
            var arch = LooseArchive.Load(dirPath);
            _sLoadedArchives.Add(arch);
            return arch;
        }

		[MethodImpl(MethodImplOptions.Synchronized)]
        public static ImageArchive LoadImageArchive(string filePath)
        {
            var arch = ImageArchive.Load(filePath);
            _sLoadedArchives.Add(arch);
            return arch;
        }

		[MethodImpl(MethodImplOptions.Synchronized)]
        public static bool FileExists(string name)
        {
            return _sLoadedArchives.Any(x => x.ContainsFile(name));
        }

		[MethodImpl(MethodImplOptions.Synchronized)]
        public static Stream ReadFile(string name)
        {
            var arch = _sLoadedArchives.FirstOrDefault(x => x.ContainsFile(name));
            if (arch == null) throw new FileNotFoundException(name);

			// get a stream and build memory stream out of it - this will ensure thread safe access

			var stream = arch.ReadFile(name);

			byte[] buffer = new byte[stream.Length];
			stream.Read (buffer, 0, (int) stream.Length);

			stream.Dispose ();

			return new MemoryStream (buffer);
        }

		// this method should not be synchronized, because thread would block while 
		// archive is being read, but the thread only wants to register a job and continue
	//	[MethodImpl(MethodImplOptions.Synchronized)]
		public static void ReadFileAsync(string name, System.Action<Stream> onFinish)
		{
			Behaviours.LoadingThread.RegisterJob (new Behaviours.LoadingThread.Job<Stream> () {
				action = () => ReadFile( name ),
				callbackFinish = (stream) => { onFinish(stream); },
			});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]	// ensure section is read, before another thread can read archives
        public static TSection ReadFile<TSection>(string name)
            where TSection : SectionData
        {
            using (var stream = ReadFile(name))
            {
                var section = Section<SectionData>.ReadData(stream) as TSection;
                if (section == null)
                {
                    throw new ArgumentException(string.Format("File \"{0}\" is not a {1}!", name, typeof(TSection).Name), "name");
                }

                return section;
            }
        }
    }
}