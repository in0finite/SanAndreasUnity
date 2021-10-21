using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class Config
    {
        public const string const_game_dir = "game_dir";


        public static string FileName => "config.json";

        public static string UserFileName => "config.user.json";

        public static string ConfigFilesDirectoryPath
		{
			get {
				#if UNITY_EDITOR || UNITY_STANDALONE
				return Directory.GetCurrentDirectory ();
				#else
				return Application.persistentDataPath;
				#endif
			}
		}

        public static string FilePath => Path.Combine(ConfigFilesDirectoryPath, FileName);

        public static string UserFilePath => Path.Combine(ConfigFilesDirectoryPath, UserFileName);

        public static string GamePath => GetPath (const_game_dir);

		public static string DataPath
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Directory.GetCurrentDirectory(), "Data");
#elif UNITY_STANDALONE
                return Path.Combine(Application.dataPath, "Data");
#else
				return Path.Combine(Application.persistentDataPath, "Data");
#endif
            }
        }


		private static JObject _root = new JObject ();
		private static JObject _user = new JObject ();

		private static readonly Dictionary<string, string> _substitutions = new Dictionary<string, string> ();


        
        private static TVal ConvertVal<TVal>(JToken val)
        {
	        // note that if you pass string[] as type, it will fail on IL2CPP

            try
            {
                return (TVal)Convert.ChangeType(val, typeof(TVal));
            }
            catch
            {
                return val.ToObject<TVal>();
            }
        }

        private static TVal Get<TVal>(string key)
        {
            var userVal = _user[key];
            if (userVal != null)
	            return ConvertVal<TVal>(userVal);

            return ConvertVal<TVal>(_root[key]);
        }

        public static string GetString(string key)
        {
	        return Get<string>(key);
        }

        public static int GetInt(string key)
        {
	        return Get<int>(key);
        }

        public static bool GetBool(string key)
        {
	        return Get<bool>(key);
        }

        private static string GetSubstitution(string key)
        {
            if (_substitutions.ContainsKey(key)) return _substitutions[key];

            string subs;
            if (key == "data_dir")
            {
                subs = DataPath;
            }
            else
            {
                subs = ReplaceSubstitutions(GetString(key));
            }

            _substitutions.Add(key, subs);
            return subs;
        }

        private static readonly Regex _regex = new Regex(@"\$\{(?<key>[a-z0-9_]+)\}", RegexOptions.Compiled);

        private static string ReplaceSubstitutions(string value)
        {
            return _regex.Replace(value, x => GetSubstitution(x.Groups["key"].Value));
        }

        public static string GetPath(string key)
        {
            return ReplaceSubstitutions(GetString(key));
        }

        public static string[] GetPaths(string key)
        {
            return Get<JArray>(key)
                .Select(x => ReplaceSubstitutions((string)x))
                .ToArray();
        }

		public static void SetString (string key, string value)
		{
			_user [key] = value;
		}

		public static void Load ()
		{
			_root = new JObject ();
			_user = new JObject ();
			_substitutions.Clear ();


			_root = JObject.Parse (Resources.Load<TextAsset>("config").text);

			if (File.Exists (UserFilePath))
			{
				_user = JObject.Parse (File.ReadAllText (UserFilePath));
			}

		}

		public static void SaveUserConfig ()
		{
			File.WriteAllText (UserFilePath, _user.ToString (Newtonsoft.Json.Formatting.Indented));
		}

		public static void SaveUserConfigSafe ()
		{
			F.RunExceptionSafe (() => SaveUserConfig ());
		}

    }
}