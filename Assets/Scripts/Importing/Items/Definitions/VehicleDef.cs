using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    public enum VehicleType
    {
        Trailer,
        Bmx,
        Bike,
        Train,
        Boat,
        Plane,
        Heli,
        Quad,
        MTruck,
        Car
    }

    [Section("cars")]
    public class VehicleDef : Definition, IObjectDefinition
    {
        public readonly int Id;
		public readonly int HornId;

		public readonly Dictionary<int, int> HornDictionary = new Dictionary<int, int>()
		{
			{ 400, 7},
			{ 401, 2},
			{ 402, 2},
			{ 403, 9},
			{ 404, 7},
			{ 405, 3},
			{ 406, 4},
			{ 407, 4},
			{ 408, 5},
			{ 409, 2},
			{ 410, 1},
			{ 411, 8},
			{ 412, 2},
			{ 413, 2},
			{ 414, 7},
			{ 415, 8},
			{ 416, 7},
			{ 417, 10},
			{ 418, 1},
			{ 419, 2},
			{ 420, 5},
			{ 421, 3},
			{ 422, 7},
			{ 423, 5},
			{ 424, 6},
			{ 425, 10},
			{ 426, 8},
			{ 427, 9},
			{ 428, 4},
			{ 429, 3},
			{ 430, 10},
			{ 431, 5},
			{ 432, 9},
			{ 433, 9},
			{ 434, 3},
			{ 435, 10},
			{ 436, 1},
			{ 437, 5},
			{ 438, 4},
			{ 439, 7},
			{ 440, 1},
			{ 441, 10},
			{ 442, 5},
			{ 443, 9},
			{ 444, 9},
			{ 445, 3},
			{ 446, 10},
			{ 447, 10},
			{ 448, 1},
			{ 449, 10},
			{ 450, 10},
			{ 451, 8},
			{ 452, 10},
			{ 453, 10},
			{ 454, 10},
			{ 455, 9},
			{ 456, 4},
			{ 457, 1},
			{ 458, 3},
			{ 459, 2},
			{ 460, 10},
			{ 461, 6},
			{ 462, 1},
			{ 463, 1},
			{ 464, 10},
			{ 465, 10},
			{ 466, 5},
			{ 467, 2},
			{ 468, 6},
			{ 469, 10},
			{ 470, 9},
			{ 471, 1},
			{ 472, 10},
			{ 473, 10},
			{ 474, 4},
			{ 475, 8},
			{ 476, 10},
			{ 477, 3},
			{ 478, 2},
			{ 479, 7},
			{ 480, 8},
			{ 481, 0},
			{ 482, 7},
			{ 483, 2},
			{ 484, 10},
			{ 485, 1},
			{ 486, 9},
			{ 487, 10},
			{ 488, 10},
			{ 489, 5},
			{ 490, 5},
			{ 491, 3},
			{ 492, 7},
			{ 493, 10},
			{ 494, 3},
			{ 495, 5},
			{ 496, 8},
			{ 497, 10},
			{ 498, 3},
			{ 499, 2},
			{ 500, 6},
			{ 501, 10},
			{ 502, 3},
			{ 503, 7},
			{ 504, 2},
			{ 505, 5},
			{ 506, 4},
			{ 507, 7},
			{ 508, 4},
			{ 509, 0},
			{ 510, 0},
			{ 511, 10},
			{ 512, 10},
			{ 513, 10},
			{ 514, 9},
			{ 515, 9},
			{ 516, 4},
			{ 517, 8},
			{ 518, 3},
			{ 519, 10},
			{ 520, 10},
			{ 521, 7},
			{ 522, 6},
			{ 523, 6},
			{ 524, 4},
			{ 525, 4},
			{ 526, 3},
			{ 527, 8},
			{ 528, 5},
			{ 529, 2},
			{ 530, 1},
			{ 531, 7},
			{ 532, 4},
			{ 533, 2},
			{ 534, 3},
			{ 535, 7},
			{ 536, 2},
			{ 537, 10},
			{ 538, 10},
			{ 539, 10},
			{ 540, 3},
			{ 541, 2},
			{ 542, 7},
			{ 543, 7},
			{ 544, 9},
			{ 545, 8},
			{ 546, 3},
			{ 547, 1},
			{ 548, 10},
			{ 549, 2},
			{ 550, 8},
			{ 551, 6},
			{ 552, 7},
			{ 553, 10},
			{ 554, 7},
			{ 555, 2},
			{ 556, 9},
			{ 557, 9},
			{ 558, 8},
			{ 559, 3},
			{ 560, 7},
			{ 561, 3},
			{ 562, 3},
			{ 563, 10},
			{ 564, 10},
			{ 565, 2},
			{ 566, 3},
			{ 567, 7},
			{ 568, 7},
			{ 569, 10},
			{ 570, 10},
			{ 571, 1},
			{ 572, 1},
			{ 573, 4},
			{ 574, 5},
			{ 575, 3},
			{ 576, 5},
			{ 577, 10},
			{ 578, 4},
			{ 579, 7},
			{ 580, 2},
			{ 581, 6},
			{ 582, 4},
			{ 583, 1},
			{ 584, 10},
			{ 585, 8},
			{ 586, 7},
			{ 587, 5},
			{ 588, 4},
			{ 589, 6},
			{ 590, 10},
			{ 591, 10},
			{ 592, 10},
			{ 593, 10},
			{ 594, 10},
			{ 595, 10},
			{ 596, 2},
			{ 597, 2},
			{ 598, 7},
			{ 599, 4},
			{ 600, 7},
			{ 601, 9},
			{ 602, 3},
			{ 603, 8},
			{ 604, 5},
			{ 605, 7},
			{ 606, 10},
			{ 607, 10},
			{ 608, 10},
			{ 609, 3},
			{ 610, 10},
			{ 611, 10}
		};

		int IObjectDefinition.Id
        {
            get { return Id; }
        }

        public readonly string ModelName;
        public readonly string TextureDictionaryName;

        public readonly VehicleType VehicleType;

        public readonly string HandlingName;
        public readonly string GameName;
        public readonly string AnimsName;
        public readonly string ClassName;

        public readonly int Frequency;
        public readonly int Flags;
        public readonly int CompRules;

        public readonly bool HasWheels;

        public readonly int WheelId;
        public readonly float WheelScaleFront;
        public readonly float WheelScaleRear;

        public readonly int UpgradeId;

        public VehicleDef(string line) : base(line)
        {
            Id = GetInt(0);
			HornId = HornDictionary[Id];

			ModelName = GetString(1);
            TextureDictionaryName = GetString(2);

            VehicleType = (VehicleType)Enum.Parse(typeof(VehicleType), GetString(3), true);

            HandlingName = GetString(4);
            GameName = GetString(5);
            AnimsName = GetString(6);
            ClassName = GetString(7);

            Frequency = GetInt(8);
            Flags = GetInt(9);
            CompRules = GetInt(10, NumberStyles.HexNumber);

            HasWheels = Parts >= 15;

            if (HasWheels)
            {
                WheelId = GetInt(11);
                WheelScaleFront = GetSingle(12);
                WheelScaleRear = GetSingle(13);
                UpgradeId = GetInt(14);
            }
        }
    }
}