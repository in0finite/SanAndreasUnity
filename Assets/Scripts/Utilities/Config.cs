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

        public static string TemplateFileName
        {
            get { return "config.template.json"; }
        }

        public static string FilePath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", FileName)); }
        }

        public static string TemplateFilePath
        {
            get { return Path.Combine(Application.dataPath, Path.Combine("..", TemplateFileName)); }
        }

        private static readonly JObject _root;

        static Config()
        {
            if (!File.Exists(FilePath) && File.Exists(TemplateFilePath)) {
                File.Copy(TemplateFilePath, FilePath);
            }

            _root = JObject.Parse(File.ReadAllText(FilePath));
        }

        public static JToken Get(string key)
        {
            return _root[key];
        }
    }
}
