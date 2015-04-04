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

        public static string FilePath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", "config.json")); }
        }

        private static readonly JObject _root;

        static Config()
        {
            _root = JObject.Parse(File.ReadAllText(FilePath));
        }

        public static JToken Get(string key)
        {
            return _root[key];
        }
    }
}
