using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class Config
    {
        public static string FileName
        {
            get { return "config.json"; }
        }

        public static string UserFileName
        {
            get { return "config.user.json"; }
        }

        public static string FilePath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", FileName)); }
        }

        public static string UserFilePath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", UserFileName)); }
        }

        private static readonly JObject _root;
        private static readonly JObject _user;

        static Config()
        {
            if (!File.Exists(UserFilePath)) {
                File.WriteAllText(UserFilePath, "{\r\n    // Specify overrides here\r\n}\r\n");
            }

            _root = JObject.Parse(File.ReadAllText(FilePath));
            _user = JObject.Parse(File.ReadAllText(UserFilePath));
        }

        public static JToken Get(string key)
        {
            return _user[key] ?? _root[key];
        }
    }
}
