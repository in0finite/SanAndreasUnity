using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class Config
    {
        public static string ConfigPath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", "config.json")); }
        }

        private static readonly JObject _root;

        static Config()
        {
            _root = JObject.Parse(File.ReadAllText(ConfigPath));
        }

        public static JToken Get(string key)
        {
            return _root[key];
        }
    }
}
