using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SanAndreasUnity.Importing.Animation
{
    public enum AnimGroupType
    {
        WalkCycle
    }

    public enum AnimType
    {
        Walk = 0,
        Run = 1,
        Panicked = 2,
        Idle = 3,
        RoadCross = 4,
        WalkStart = 5
    }

    public class AnimationGroup
    {
        private static readonly Regex _sHeaderRegex = new Regex("^" +
            @"\s*(?<groupName>[a-z0-9_]+)\s*," + 
            @"\s*(?<fileName>[a-z0-9_]+)\s*," +
            @"\s*(?<animType>[a-z0-9_]+)\s*," +
            @"\s*(?<animCount>[0-9]+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _sEndRegex = new Regex(@"^\s*end\s*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Dictionary<string, AnimationGroup> _sGroups
            = new Dictionary<string,AnimationGroup>();

        public static void Load(string path)
        {
            using (var reader = File.OpenText(path)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var match = _sHeaderRegex.Match(line);
                    if (!match.Success) continue;

                    var group = new AnimationGroup(match, reader);
                    _sGroups.Add(group.Name, group);
                }
            }
        }

        public static AnimationGroup Get(string name)
        {
            return _sGroups.ContainsKey(name) ? _sGroups[name] : null;
        }

        private readonly string[] _animations;

        public readonly string Name;
        public readonly string FileName;
        public readonly AnimGroupType Type;

        private AnimationGroup(Match match, StreamReader reader)
        {
            Name = match.Groups["groupName"].Value;
            FileName = match.Groups["fileName"].Value + ".ifp";
            Type = (AnimGroupType) Enum.Parse(typeof(AnimGroupType), match.Groups["animType"].Value, true);

            var animCount = int.Parse(match.Groups["animCount"].Value);
            _animations = new String[animCount];

            var i = 0;
            string line;
            while ((line = reader.ReadLine()) != null && !_sEndRegex.IsMatch(line)) {
                line = line.Trim();
                if (line.Length == 0) continue;
                _animations[i++] = line;
            }
        }

        public string this[AnimType type]
        {
            get { return _animations[(int) type]; }
        }
    }
}
