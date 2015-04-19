using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SanAndreasUnity.Importing.Animation
{
    public enum AnimGroup
    {
        None = 0,
        WalkCycle = 1,
        Car = 2
    }

    public enum AnimIndex
    {
        None = -1,

        // AnimGroup.WalkCycle
        Walk = 0,
        Run = 1,
        Panicked = 2,
        Idle = 3,
        RoadCross = 4,
        WalkStart = 5,

        // AnimGroup.Car
        Sit = 0,
        SitPassenger = 1,
        DriveLeft = 2,
        DriveRight = 3,
        GetInLeft = 4,
        GetInRight = 5,
        GetOutLeft = 6,
        GetOutRight = 7,
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

        private static readonly Dictionary<string, Dictionary<AnimGroup, AnimationGroup>> _sGroups
            = new Dictionary<string, Dictionary<AnimGroup, AnimationGroup>>();

        public static void Load(string path)
        {
            using (var reader = File.OpenText(path)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var match = _sHeaderRegex.Match(line);
                    if (!match.Success) continue;

                    var group = new AnimationGroup(match, reader);

                    if (!_sGroups.ContainsKey(group.Name)) {
                        _sGroups.Add(group.Name, new Dictionary<AnimGroup,AnimationGroup>());
                    }

                    _sGroups[group.Name].Add(group.Type, group);
                }
            }
        }

        private static AnimationGroup GetInternal(string name, AnimGroup type)
        {
            Dictionary<AnimGroup, AnimationGroup> groupDict;
            if (!_sGroups.TryGetValue(name, out groupDict)) return null;

            AnimationGroup group;
            return groupDict.TryGetValue(type, out group) ? group : null;
        }

        public static AnimationGroup Get(string name, AnimGroup type)
        {
            return GetInternal(name, type) ?? GetInternal("default", type);
        }

        private readonly string[] _animations;

        public readonly string Name;
        public readonly string FileName;
        public readonly AnimGroup Type;

        private AnimationGroup(Match match, StreamReader reader)
        {
            Name = match.Groups["groupName"].Value;
            FileName = match.Groups["fileName"].Value;
            Type = (AnimGroup) Enum.Parse(typeof(AnimGroup), match.Groups["animType"].Value, true);

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

        public string this[AnimIndex type]
        {
            get { return _animations[(int) type]; }
        }
    }
}
