using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.GXT
{
	public class GXT : IDisposable
	{

		#region LoadPath
		private static string _language= "american";
		public static string Language
		{
			get => _language;
			set
			{
				var path = GetPathFromLanguage(value);
				if (File.Exists(path))
				{
					_language = value;
				}
			}
		}

		public static string GXTDir => Path.Combine(Config.GamePath, "text");
		private static string GetPathFromLanguage(string language) => Path.Combine(GXTDir, $"{language}.gxt");
		private static string LoadPath => GetPathFromLanguage(_language);

		#endregion

		#region GTXData

		private Int16 _version;
		public List<string> SubTableNames { get; } = new List<string>();
		public Dictionary<string, List<int>> TableEntryNameDict { get; } = new Dictionary<string, List<Int32>>();
		public Dictionary<int, string> EntryNameWordDict { get; } = new Dictionary<int, string>();

		private static GXT _gxt;
		public static GXT Gxt
		{
			get
			{
				if (_gxt == null)
				{
					GXT.Load();
				}

				return _gxt;
			}
		}

		private MemoryStream _rawData;

		#endregion

		public GXT(string fp)
		{
			var bytes = File.ReadAllBytes(LoadPath);
			_rawData = new MemoryStream(bytes, false);
		}

		public static void Load()
		{
			if (_gxt != null)
			{
				return;
			}
			_gxt = new GXT(LoadPath);
			_gxt.InternalLoad();
			_gxt._rawData.Dispose();
		}


		private void InternalLoad()
		{
			var encoding = LoadHeader();
			var tkeyEntryOffsets = LoadTableBlock(encoding);

			for (var i = 0; i < SubTableNames.Count; i++)
			{
				var subTableName = SubTableNames[i];
				_rawData.Seek(tkeyEntryOffsets[i], SeekOrigin.Begin);
				using (var binReader = new BinaryReader(_rawData, encoding, true))
				{
					if (i == 0) //parse tkey directly
					{
						LoadTKEY(binReader, subTableName);
					}
					else //skip tkeyname
					{
						_rawData.Seek(8, SeekOrigin.Current);
						LoadTKEY(binReader, subTableName);
					}
				}
			}
		}

		private void LoadTKEY(BinaryReader binaryReader, string tableName)
		{
			//Console.WriteLine(nameof(LoadTKEY));
			var tkey = new string(binaryReader.ReadChars(4));
			var blockSize = binaryReader.ReadInt32();

			//Console.WriteLine($"tkey:{tkey} blockSize:{blockSize}");
			if (tkey != "TKEY")
			{
				throw new ConstraintException();
			}

			var entryOffsets = new List<Int32>();
			var tdataEntryCount = blockSize / 8;
			var entryNames = new List<Int32>();
			for (int i = 0; i < tdataEntryCount; i++)
			{

				var entryOffset = binaryReader.ReadInt32();
				var crc32 = binaryReader.ReadInt32();
				entryNames.Add(crc32);
				entryOffsets.Add(entryOffset);
			}

			TableEntryNameDict[tableName] = entryNames;

			//read tdat
			var tdat = new string(binaryReader.ReadChars(4));
			if (tdat != "TDAT")
			{
				throw new ConstraintException();
			}
			var tdatBlockSize = binaryReader.ReadInt32();
			var bytes = binaryReader.ReadBytes(tdatBlockSize);
			var wordBytes = new List<byte>();
			for (int i = 0; i < entryOffsets.Count; i++)
			{
				var entryName = entryNames[i];
				//todo this should be double checked; I think it could be used a lib method.
				//but not familiar with relevant api. 
				for (int j = entryOffsets[i]; ; j++)
				{
					var c = Convert.ToChar(bytes[j]);
					if (c == '\0')
					{
						break;
					}
					wordBytes.Add(bytes[j]);
				}
				EntryNameWordDict[entryName] = Win1252ToString(wordBytes.ToArray());
				wordBytes.Clear();
			}
		}

		private string Win1252ToString(byte[] bytes)
		{
			Encoding win1252 = Encoding.GetEncoding(1252);
			var utf8bytes = Encoding.Convert(win1252, Encoding.UTF8, bytes);
			return Encoding.UTF8.GetString(utf8bytes);
		}

		private Encoding LoadHeader()
		{
			using (var binReader = new BinaryReader(_rawData, Encoding.ASCII, true))
			{
				_version = binReader.ReadInt16();
				var bitsPerChar = binReader.ReadInt16();
				if (bitsPerChar == 8)
				{
					return Encoding.ASCII;
				}
				else if (bitsPerChar == 16)
				{
					return Encoding.Unicode;
				}
			}
			return Encoding.ASCII;
		}

		private List<Int32> LoadTableBlock(Encoding encoding)
		{
			using (var binReader = new BinaryReader(_rawData, encoding, true))
			{
				var offsetList = new List<Int32>();
				var tablConst = binReader.ReadChars(4);
				if (new string(tablConst) != "TABL")
				{
					throw new ConstraintException();
				}

				var blockSize = binReader.ReadInt32();
				var entryCount = blockSize / 12;
				for (int i = 0; i < entryCount; i++)
				{
					var subtableName = new string(binReader.ReadChars(8));
					var offset = binReader.ReadInt32();

					//Console.WriteLine($"subtablename:{subtableName} offset:{offset}");
					SubTableNames.Add(subtableName);
					offsetList.Add(offset);
				}

				return offsetList;
			}
		}

		//todo this should be refactored; unity console cannot show too much string; 
		//maybe I should just show some items;
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"version:{_version}");
			foreach (var kv in TableEntryNameDict)
			{
				stringBuilder.AppendLine($"-----------table {kv.Key} starts------------");
				foreach (var entrykey in kv.Value)
				{
					stringBuilder.AppendLine($"entry:{entrykey} value:{EntryNameWordDict[entrykey]}");
				}

				stringBuilder.AppendLine($"-----------table {kv.Key} ends------------");
			}

			return stringBuilder.ToString();
		}


		public void Dispose()
		{
			if (_rawData != null)
			{
				_rawData.Dispose();
				_rawData = null;
			}
		}
	}
}
