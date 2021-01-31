using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SanAndreasUnity.Importing.Animation
{
    public enum AnimGroup
    {
        None = 0,
        WalkCycle,
        Car,
        MyWalkCycle,

        Colt45,
		Silenced,
		Python,
		Shotgun,
		Buddy,
		Tec,
		Uzi,
		Rifle,
        Rocket,
		Flame,
        Grenade,

		Gun,
		Weapons,

        _Count
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

		// mywalkcycle
        IdleArmed = 0,
        FuckU = 1,
        GUN_STAND = 2,
		sprint_civi = 3,

        // AnimGroup.Car
        Sit = 0,
        SitPassenger = 1,
        DriveLeft = 2,
        DriveRight = 3,
        GetInLeft = 4,
        GetInRight = 5,
        GetOutLeft = 6,
        GetOutRight = 7,
		CAR_closedoor_LHS = 8,
		CAR_closedoor_RHS,
		CAR_close_LHS,
		CAR_close_RHS,

		// Gun
		GunMove_BWD = 2,
		GunMove_FWD,
		GunMove_L,
		GunMove_R,
		run_armed = 12,
		WALK_armed = 17,

		// Colt45
		colt45_fire = 0,
		COLT45_RELOAD,
		colt45_crouchfire,
		colt45_fire_2hands,
		twoguns_crouchfire,
		colt45_crouchreload,
		sawnoff_reload,

		// Silenced
		CrouchReload = 0,
		SilenceCrouchfire,
		Silence_fire,
		Silence_reload,

		// Python
		python_crouchfire = 0,
		python_crouchreload,
		python_fire,
		python_fire_poor,
		python_reload,

		// Shotgun
		shotgun_crouchfire = 0,
		shotgun_fire,
		shotgun_fire_poor,

		// Buddy
		buddy_crouchfire = 0,
		buddy_crouchreload,
		buddy_fire,
		buddy_fire_poor,
		buddy_fire_reload,

		// Tec
		TEC_crouchfire = 0,
		TEC_crouchreload,
		TEC_fire,
		TEC_reload,

		// Uzi
		UZI_crouchfire = 0,
		UZI_crouchreload,
		UZI_fire,
		UZI_fire_poor,
		UZI_reload,

		// Rifle
		RIFLE_crouchfire = 0,
		RIFLE_crouchload,
		RIFLE_fire,
		RIFLE_fire_poor,
		RIFLE_load,

		// Rocket
		idle_rocket = 0,
		RocketFire,
		run_rocket,
		walk_rocket,
		WALK_start_rocket,

		// Flame
		FLAME_fire = 0,

		// Grenade
		WEAPON_start_throw = 0,
		WEAPON_throw,
		WEAPON_throwu,

		// so we can dynamically access all anims of the anim group
        Index0 = 0,
		Index1,
		Index2,
		Index3,
		Index4,
		Index5,
		Index6,
		Index7,
		Index8,
		Index9,
		Index10,
		Index11,
		Index12,
		Index13,
		Index14,
		Index15,
		Index16,
		Index17,
		Index18,
		Index19,
		Index20,
		Index21,
		Index22,
		Index23,
		Index24,
		Index25,
		Index26,
		Index27,
		Index28,
		Index29,
		Index30,

    }

	public static class AnimIndexUtil
	{

		public static AnimIndex Get(int index)
		{
			string name = "Index" + index;
			return (AnimIndex)Enum.Parse(typeof(AnimIndex), name);
		}
	}

    /*
	public class AnimIndex {
		public static string Walk = "walk_civi";
		public static string Run = "run_civi";
		public static string Panicked = "sprint_panic";
		public static string Idle = "idle_stance";

		public static string Sit = "CAR_sit";
		public static string SitPassenger = "CAR_sitp" ;
		public static string DriveLeft = "Drive_L";
		public static string DriveRight = "Drive_R";
		public static string GetInLeft = "CAR_getin_LHS";
		public static string GetInRight = "CAR_getin_RHS";
		public static string GetOutLeft = "CAR_getout_LHS";
		public static string GetOutRight = "CAR_getout_RHS";

		public static string IdleArmed = "IDLE_ARMED";
	}
	*/

	public struct AnimId
	{
		private AnimGroup animGroup;
		public AnimGroup AnimGroup { get { return this.animGroup; } }

		private AnimIndex animIndex;
		public AnimIndex AnimIndex { get { return this.animIndex; } }

		private string fileName;
		public string FileName { get { return this.fileName; } }

		private string animName;
		public string AnimName { get { return this.animName; } }

		private bool usesAnimGroup;
		public bool UsesAnimGroup { get { return this.usesAnimGroup; } }


		public AnimId (AnimGroup animGroup, AnimIndex animIndex)
		{
			this.animGroup = animGroup;
			this.animIndex = animIndex;
			this.fileName = null;
			this.animName = null;
			this.usesAnimGroup = true;
		}

		public AnimId (string fileName, string animName)
		{
			this.animGroup = AnimGroup.None;
			this.animIndex = AnimIndex.None;
			this.fileName = fileName;
			this.animName = animName;
			this.usesAnimGroup = false;
		}

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
		
        public static readonly Dictionary<string, Dictionary<AnimGroup, AnimationGroup>> _sGroups
            = new Dictionary<string, Dictionary<AnimGroup, AnimationGroup>>();
		/// <summary>
		/// Key is a name of anim group. Value is dictionary, in which the key is anim group type and value is info about anim group.
		/// </summary>
		public static IEnumerable<KeyValuePair<string, Dictionary<AnimGroup, AnimationGroup>>> AllLoadedGroups
		{ get { return _sGroups; } }


		public static void Load(string fileName)
		{
			using (var stream = Archive.ArchiveManager.ReadFile(fileName))
			{
				using (var reader = new StreamReader(stream))
				{
					LoadFromStreamReader(reader);
				}
			}
		}

        public static void LoadFromStreamReader(StreamReader reader)
        {
            
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var match = _sHeaderRegex.Match(line);
                if (!match.Success) continue;

                var group = new AnimationGroup(match, reader);

                if (!_sGroups.ContainsKey(group.Name))
                {
                    _sGroups.Add(group.Name, new Dictionary<AnimGroup, AnimationGroup>());
                }

                _sGroups[group.Name].Add(group.Type, group);
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
        public string[] Animations { get { return _animations; } }

        public readonly string Name;
        public readonly string FileName;
        public readonly AnimGroup Type;


        private AnimationGroup(Match match, StreamReader reader)
        {
            Name = match.Groups["groupName"].Value;
            FileName = match.Groups["fileName"].Value;
            Type = (AnimGroup)Enum.Parse(typeof(AnimGroup), match.Groups["animType"].Value, true);

            var animCount = int.Parse(match.Groups["animCount"].Value);
            _animations = new String[animCount];

            var i = 0;
            string line;
            while ((line = reader.ReadLine()) != null && !_sEndRegex.IsMatch(line))
            {
                line = line.Trim();
                if (line.Length == 0) continue;
                _animations[i++] = line;
            }
        }

        public string this[AnimIndex type]
        {
            get { return _animations[(int)type]; }
        }

        public bool HasAnimation(string animName)
        {
            return System.Array.IndexOf(_animations, animName) >= 0;
        }
    }
}