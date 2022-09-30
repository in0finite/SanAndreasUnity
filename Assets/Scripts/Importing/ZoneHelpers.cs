using UGameCore.Utilities;

namespace SanAndreasUnity.Importing
{

    public class ZoneHelpers
    {
        public static Zone[] zoneInfoList =
        {
        new Zone(-2353, 2275, 0, -2153, 2475, 200, "Bayside Marina"),
        new Zone(-2741, 2175, 0, -2353, 2722, 200, "Bayside"),
        new Zone(-2741, 1268, 0, -2533, 1490, 200, "Battery Point"),
        new Zone(-2741, 793, 0, -2533, 1268, 200, "Paradiso"),
        new Zone(-2741, 458, 0, -2533, 793, 200, "Santa Flora"),
        new Zone(-2994, 458, 0, -2741, 1339, 200, "Palisades"),
        new Zone(-2867, 277, 0, -2593, 458, 200, "City Hall"),
        new Zone(-2994, 277, 0, -2867, 458, 200, "Ocean Flats"),
        new Zone(-2994, -222, 0, -2593, 277, 200, "Ocean Flats"),
        new Zone(-2994, -430, 0, -2831, -222, 200, "Ocean Flats"),
        new Zone(-2270, -430, 0, -2178, -324, 200, "Foster Valley"),
        new Zone(-2178, -599, 0, -1794, -324, 200, "Foster Valley"),
        new Zone(-2593, -222, 0, -2411, 54, 200, "Hashbury"),
        new Zone(-2533, 968, 0, -2274, 1358, 200, "Juniper Hollow"),
        new Zone(-2533, 1358, 0, -1996, 1501, 200, "Esplanade North"),
        new Zone(-1996, 1358, 0, -1524, 1592, 200, "Esplanade North"),
        new Zone(-1982, 1274, 0, -1524, 1358, 200, "Esplanade North"),
        new Zone(-1871, 744, 0, -1701, 1176, 300, "Financial"),
        new Zone(-2274, 744, 0, -1982, 1358, 200, "Calton Heights"),
        new Zone(-1982, 744, 0, -1871, 1274, 200, "Downtown"),
        new Zone(-1871, 1176, 0, -1620, 1274, 200, "Downtown"),
        new Zone(-1700, 744, 0, -1580, 1176, 200, "Downtown"),
        new Zone(-1580, 744, 0, -1499, 1025, 200, "Downtown"),
        new Zone(-2533, 578, 0, -2274, 968, 200, "Juniper Hill"),
        new Zone(-2274, 578, 0, -2078, 744, 200, "Chinatown"),
        new Zone(-2078, 578, 0, -1499, 744, 200, "Downtown"),
        new Zone(-2329, 458, 0, -1993, 578, 200, "King's"),
        new Zone(-2411, 265, 0, -1993, 373, 200, "King's"),
        new Zone(-2253, 373, 0, -1993, 458, 200, "King's"),
        new Zone(-2411, -222, 0, -2173, 265, 200, "Garcia"),
        new Zone(-2270, -324, 0, -1794, -222, 200, "Doherty"),
        new Zone(-2173, -222, 0, -1794, 265, 200, "Doherty"),
        new Zone(-1993, 265, 0, -1794, 578, 200, "Downtown"),
        new Zone(-1499, -50, 0, -1242, 249, 200, "Easter Bay Airport"),
        new Zone(-1794, 249, 0, -1242, 578, 200, "Easter Basin"),
        new Zone(-1794, -50, 0, -1499, 249, 200, "Easter Basin"),
        new Zone(-1620, 1176, 0, -1580, 1274, 200, "Esplanade East"),
        new Zone(-1580, 1025, 0, -1499, 1274, 200, "Esplanade East"),
        new Zone(-1499, 578, -79, -1339, 1274, 20, "Esplanade East"),
        new Zone(-2324, -2584, 0, -1964, -2212, 200, "Angel Pine"),
        new Zone(-1632, -2263, 0, -1601, -2231, 200, "Shady Cabin"),
        new Zone(-1166, -2641, 0, -321, -1856, 200, "Back o Beyond"),
        new Zone(-1166, -1856, 0, -815, -1602, 200, "Leafy Hollow"),
        new Zone(-594, -1648, 0, -187, -1276, 200, "Flint Range"),
        new Zone(-792, -698, 0, -452, -380, 200, "Fallen Tree"),
        new Zone(-1209, -1317, 114, -908, -787, 251, "The Farm"),
        new Zone(-1645, 2498, 0, -1372, 2777, 200, "El Quebrados"),
        new Zone(-1372, 2498, 0, -1277, 2615, 200, "Aldea Malvada"),
        new Zone(-968, 1929, 0, -481, 2155, 200, "The Sherman Dam"),
        new Zone(-926, 1398, 0, -719, 1634, 200, "Las Barrancas"),
        new Zone(-376, 826, 0, 123, 1220, 200, "Fort Carson"),
        new Zone(337, 710, -115, 860, 1031, 203, "Hunter Quarry"),
        new Zone(338, 1228, 0, 664, 1655, 200, "Octane Springs"),
        new Zone(176, 1305, 0, 338, 1520, 200, "Green Palms"),
        new Zone(-405, 1712, 0, -276, 1892, 200, "Regular Tom"),
        new Zone(-365, 2123, 0, -208, 2217, 200, "Las Brujas"),
        new Zone(37, 2337, 0, 435, 2677, 200, "Verdant Meadows"),
        new Zone(-354, 2580, 0, -133, 2816, 200, "Las Payasadas"),
        new Zone(-901, 2221, 0, -592, 2571, 200, "Arco del Oeste"),
        new Zone(-1794, -730, 0, -1213, -50, 200, "Easter Bay Airport"),
        new Zone(2576, 62, 0, 2759, 385, 200, "Hankypanky Point"),
        new Zone(2160, -149, 0, 2576, 228, 200, "Palomino Creek"),
        new Zone(2285, -768, 0, 2770, -269, 200, "North Rock"),
        new Zone(1119, 119, 0, 1451, 493, 200, "Montgomery"),
        new Zone(1451, 347, 0, 1582, 420, 200, "Montgomery"),
        new Zone(603, 264, 0, 761, 366, 200, "Hampton Barns"),
        new Zone(508, -139, 0, 1306, 119, 200, "Fern Ridge"),
        new Zone(580, -674, 0, 861, -404, 200, "Dillimore"),
        new Zone(967, -450, 0, 1176, -217, 200, "Hilltop Farm"),
        new Zone(104, -220, 0, 349, 152, 200, "Blueberry"),
        new Zone(19, -404, 0, 349, -220, 200, "Blueberry"),
        new Zone(-947, -304, 0, -319, 327, 200, "The Panopticon"),
        new Zone(2759, 296, 0, 2774, 594, 200, "Frederick Bridge"),
        new Zone(1664, 401, 0, 1785, 567, 200, "The Mako Span"),
        new Zone(-319, -220, 0, 104, 293, 200, "Blueberry Acres"),
        new Zone(-222, 293, 0, -122, 476, 200, "Martin Bridge"),
        new Zone(434, 366, 0, 603, 555, 200, "Fallow Bridge"),
        new Zone(-1820, -2643, 0, -1226, -1771, 200, "Shady Creeks"),
        new Zone(-2030, -2174, 0, -1820, -1771, 200, "Shady Creeks"),
        new Zone(-2533, 458, 0, -2329, 578, 200, "Queens"),
        new Zone(-2593, 54, 0, -2411, 458, 200, "Queens"),
        new Zone(-2411, 373, 0, -2253, 458, 200, "Queens"),
        new Zone(44, -2892, -242, 2997, -768, 900, "Los Santos"),
        new Zone(869, 596, -242, 2997, 2993, 900, "Las Venturas"),
        new Zone(-480, 596, -242, 869, 2993, 900, "Bone County"),
        new Zone(-2997, 1659, -242, -480, 2993, 900, "Tierra Robada"),
        new Zone(-2741, 1659, 0, -2616, 2175, 200, "Gant Bridge"),
        new Zone(-2741, 1490, 0, -2616, 1659, 200, "Gant Bridge"),
        new Zone(-2997, -1115, -242, -1213, 1659, 900, "San Fierro"),
        new Zone(-1213, 596, -242, -480, 1659, 900, "Tierra Robada"),
        new Zone(-1213, -768, -242, 2997, 596, 900, "Red County"),
        new Zone(-1213, -2892, -242, 44, -768, 900, "Flint County"),
        new Zone(-1132, -768, 0, -956, -578, 200, "Easter Bay Chemicals"),
        new Zone(-1132, -787, 0, -956, -768, 200, "Easter Bay Chemicals"),
        new Zone(-1213, -730, 0, -1132, -50, 200, "Easter Bay Airport"),
        new Zone(-2178, -1115, 0, -1794, -599, 200, "Foster Valley"),
        new Zone(-2178, -1250, 0, -1794, -1115, 200, "Foster Valley"),
        new Zone(-1242, -50, 0, -1213, 578, 200, "Easter Bay Airport"),
        new Zone(-1213, -50, 0, -947, 578, 200, "Easter Bay Airport"),
        new Zone(-2997, -2892, -242, -1213, -1115, 900, "Whetstone"),
        new Zone(1249, -2394, -89, 1852, -2179, 110, "Los Santos International"),
        new Zone(1852, -2394, -89, 2089, -2179, 110, "Los Santos International"),
        new Zone(930, -2488, -89, 1249, -2006, 110, "Verdant Bluffs"),
        new Zone(1812, -2179, -89, 1970, -1852, 110, "El Corona"),
        new Zone(1970, -2179, -89, 2089, -1852, 110, "Willowfield"),
        new Zone(2089, -2235, -89, 2201, -1989, 110, "Willowfield"),
        new Zone(2089, -1989, -89, 2324, -1852, 110, "Willowfield"),
        new Zone(2201, -2095, -89, 2324, -1989, 110, "Willowfield"),
        new Zone(2373, -2697, -89, 2809, -2330, 110, "Ocean Docks"),
        new Zone(2201, -2418, -89, 2324, -2095, 110, "Ocean Docks"),
        new Zone(647, -1804, -89, 851, -1577, 110, "Marina"),
        new Zone(647, -2173, -89, 930, -1804, 110, "Verona Beach"),
        new Zone(930, -2006, -89, 1073, -1804, 110, "Verona Beach"),
        new Zone(1073, -2006, -89, 1249, -1842, 110, "Verdant Bluffs"),
        new Zone(1249, -2179, -89, 1692, -1842, 110, "Verdant Bluffs"),
        new Zone(1692, -2179, -89, 1812, -1842, 110, "El Corona"),
        new Zone(851, -1804, -89, 1046, -1577, 110, "Verona Beach"),
        new Zone(647, -1577, -89, 807, -1416, 110, "Marina"),
        new Zone(807, -1577, -89, 926, -1416, 110, "Marina"),
        new Zone(1161, -1722, -89, 1323, -1577, 110, "Verona Beach"),
        new Zone(1046, -1722, -89, 1161, -1577, 110, "Verona Beach"),
        new Zone(1046, -1804, -89, 1323, -1722, 110, "Conference Center"),
        new Zone(1073, -1842, -89, 1323, -1804, 110, "Conference Center"),
        new Zone(1323, -1842, -89, 1701, -1722, 110, "Commerce"),
        new Zone(1323, -1722, -89, 1440, -1577, 110, "Commerce"),
        new Zone(1370, -1577, -89, 1463, -1384, 110, "Commerce"),
        new Zone(1463, -1577, -89, 1667, -1430, 110, "Commerce"),
        new Zone(1440, -1722, -89, 1583, -1577, 110, "Pershing Square"),
        new Zone(1583, -1722, -89, 1758, -1577, 110, "Commerce"),
        new Zone(1701, -1842, -89, 1812, -1722, 110, "Little Mexico"),
        new Zone(1758, -1722, -89, 1812, -1577, 110, "Little Mexico"),
        new Zone(1667, -1577, -89, 1812, -1430, 110, "Commerce"),
        new Zone(1812, -1852, -89, 1971, -1742, 110, "Idlewood"),
        new Zone(1812, -1742, -89, 1951, -1602, 110, "Idlewood"),
        new Zone(1951, -1742, -89, 2124, -1602, 110, "Idlewood"),
        new Zone(1812, -1602, -89, 2124, -1449, 110, "Idlewood"),
        new Zone(2124, -1742, -89, 2222, -1494, 110, "Idlewood"),
        new Zone(1812, -1449, -89, 1996, -1350, 110, "Glen Park"),
        new Zone(1812, -1100, -89, 1994, -973, 110, "Glen Park"),
        new Zone(1996, -1449, -89, 2056, -1350, 110, "Jefferson"),
        new Zone(2124, -1494, -89, 2266, -1449, 110, "Jefferson"),
        new Zone(2056, -1372, -89, 2281, -1210, 110, "Jefferson"),
        new Zone(2056, -1210, -89, 2185, -1126, 110, "Jefferson"),
        new Zone(2185, -1210, -89, 2281, -1154, 110, "Jefferson"),
        new Zone(1994, -1100, -89, 2056, -920, 110, "Las Colinas"),
        new Zone(2056, -1126, -89, 2126, -920, 110, "Las Colinas"),
        new Zone(2185, -1154, -89, 2281, -934, 110, "Las Colinas"),
        new Zone(2126, -1126, -89, 2185, -934, 110, "Las Colinas"),
        new Zone(1971, -1852, -89, 2222, -1742, 110, "Idlewood"),
        new Zone(2222, -1852, -89, 2632, -1722, 110, "Ganton"),
        new Zone(2222, -1722, -89, 2632, -1628, 110, "Ganton"),
        new Zone(2541, -1941, -89, 2703, -1852, 110, "Willowfield"),
        new Zone(2632, -1852, -89, 2959, -1668, 110, "East Beach"),
        new Zone(2632, -1668, -89, 2747, -1393, 110, "East Beach"),
        new Zone(2747, -1668, -89, 2959, -1498, 110, "East Beach"),
        new Zone(2421, -1628, -89, 2632, -1454, 110, "East Los Santos"),
        new Zone(2222, -1628, -89, 2421, -1494, 110, "East Los Santos"),
        new Zone(2056, -1449, -89, 2266, -1372, 110, "Jefferson"),
        new Zone(2266, -1494, -89, 2381, -1372, 110, "East Los Santos"),
        new Zone(2381, -1494, -89, 2421, -1454, 110, "East Los Santos"),
        new Zone(2281, -1372, -89, 2381, -1135, 110, "East Los Santos"),
        new Zone(2381, -1454, -89, 2462, -1135, 110, "East Los Santos"),
        new Zone(2462, -1454, -89, 2581, -1135, 110, "East Los Santos"),
        new Zone(2581, -1454, -89, 2632, -1393, 110, "Los Flores"),
        new Zone(2581, -1393, -89, 2747, -1135, 110, "Los Flores"),
        new Zone(2747, -1498, -89, 2959, -1120, 110, "East Beach"),
        new Zone(2747, -1120, -89, 2959, -945, 110, "Las Colinas"),
        new Zone(2632, -1135, -89, 2747, -945, 110, "Las Colinas"),
        new Zone(2281, -1135, -89, 2632, -945, 110, "Las Colinas"),
        new Zone(1463, -1430, -89, 1724, -1290, 110, "Downtown Los Santos"),
        new Zone(1724, -1430, -89, 1812, -1250, 110, "Downtown Los Santos"),
        new Zone(1463, -1290, -89, 1724, -1150, 110, "Downtown Los Santos"),
        new Zone(1370, -1384, -89, 1463, -1170, 110, "Downtown Los Santos"),
        new Zone(1724, -1250, -89, 1812, -1150, 110, "Downtown Los Santos"),
        new Zone(1463, -1150, -89, 1812, -768, 110, "Mulholland Intersection"),
        new Zone(1414, -768, -89, 1667, -452, 110, "Mulholland"),
        new Zone(1281, -452, -89, 1641, -290, 110, "Mulholland"),
        new Zone(1269, -768, -89, 1414, -452, 110, "Mulholland"),
        new Zone(787, -1416, -89, 1072, -1310, 110, "Market"),
        new Zone(787, -1310, -89, 952, -1130, 110, "Vinewood"),
        new Zone(952, -1310, -89, 1072, -1130, 110, "Market"),
        new Zone(1370, -1170, -89, 1463, -1130, 110, "Downtown Los Santos"),
        new Zone(1378, -1130, -89, 1463, -1026, 110, "Downtown Los Santos"),
        new Zone(1391, -1026, -89, 1463, -926, 110, "Downtown Los Santos"),
        new Zone(1252, -1130, -89, 1378, -1026, 110, "Temple"),
        new Zone(1252, -1026, -89, 1391, -926, 110, "Temple"),
        new Zone(1252, -926, -89, 1357, -910, 110, "Temple"),
        new Zone(1357, -926, -89, 1463, -768, 110, "Mulholland"),
        new Zone(1318, -910, -89, 1357, -768, 110, "Mulholland"),
        new Zone(1169, -910, -89, 1318, -768, 110, "Mulholland"),
        new Zone(787, -1130, -89, 952, -954, 110, "Vinewood"),
        new Zone(952, -1130, -89, 1096, -937, 110, "Temple"),
        new Zone(1096, -1130, -89, 1252, -1026, 110, "Temple"),
        new Zone(1096, -1026, -89, 1252, -910, 110, "Temple"),
        new Zone(768, -954, -89, 952, -860, 110, "Mulholland"),
        new Zone(687, -860, -89, 911, -768, 110, "Mulholland"),
        new Zone(737, -768, -89, 1142, -674, 110, "Mulholland"),
        new Zone(1096, -910, -89, 1169, -768, 110, "Mulholland"),
        new Zone(952, -937, -89, 1096, -860, 110, "Mulholland"),
        new Zone(911, -860, -89, 1096, -768, 110, "Mulholland"),
        new Zone(861, -674, -89, 1156, -600, 110, "Mulholland"),
        new Zone(342, -2173, -89, 647, -1684, 110, "Santa Maria Beach"),
        new Zone(72, -2173, -89, 342, -1684, 110, "Santa Maria Beach"),
        new Zone(72, -1684, -89, 225, -1544, 110, "Rodeo"),
        new Zone(72, -1544, -89, 225, -1404, 110, "Rodeo"),
        new Zone(225, -1684, -89, 312, -1501, 110, "Rodeo"),
        new Zone(225, -1501, -89, 334, -1369, 110, "Rodeo"),
        new Zone(334, -1501, -89, 422, -1406, 110, "Rodeo"),
        new Zone(312, -1684, -89, 422, -1501, 110, "Rodeo"),
        new Zone(422, -1684, -89, 558, -1570, 110, "Rodeo"),
        new Zone(558, -1684, -89, 647, -1384, 110, "Rodeo"),
        new Zone(466, -1570, -89, 558, -1385, 110, "Rodeo"),
        new Zone(422, -1570, -89, 466, -1406, 110, "Rodeo"),
        new Zone(647, -1227, -89, 787, -1118, 110, "Vinewood"),
        new Zone(647, -1118, -89, 787, -954, 110, "Richman"),
        new Zone(647, -954, -89, 768, -860, 110, "Richman"),
        new Zone(466, -1385, -89, 647, -1235, 110, "Rodeo"),
        new Zone(334, -1406, -89, 466, -1292, 110, "Rodeo"),
        new Zone(225, -1369, -89, 334, -1292, 110, "Richman"),
        new Zone(225, -1292, -89, 466, -1235, 110, "Richman"),
        new Zone(72, -1404, -89, 225, -1235, 110, "Richman"),
        new Zone(72, -1235, -89, 321, -1008, 110, "Richman"),
        new Zone(321, -1235, -89, 647, -1044, 110, "Richman"),
        new Zone(321, -1044, -89, 647, -860, 110, "Richman"),
        new Zone(321, -860, -89, 687, -768, 110, "Richman"),
        new Zone(321, -768, -89, 700, -674, 110, "Richman"),
        new Zone(2027, 863, -89, 2087, 1703, 110, "The Strip"),
        new Zone(2106, 1863, -89, 2162, 2202, 110, "The Strip"),
        new Zone(1817, 863, -89, 2027, 1083, 110, "The Four Dragons Casino"),
        new Zone(1817, 1083, -89, 2027, 1283, 110, "The Pink Swan"),
        new Zone(1817, 1283, -89, 2027, 1469, 110, "The High Roller"),
        new Zone(1817, 1469, -89, 2027, 1703, 110, "Pirates in Men's Pants"),
        new Zone(1817, 1863, -89, 2106, 2011, 110, "The Visage"),
        new Zone(1817, 1703, -89, 2027, 1863, 110, "The Visage"),
        new Zone(1457, 823, -89, 2377, 863, 110, "Julius Thruway South"),
        new Zone(1197, 1163, -89, 1236, 2243, 110, "Julius Thruway West"),
        new Zone(2377, 788, -89, 2537, 897, 110, "Julius Thruway South"),
        new Zone(2537, 676, -89, 2902, 943, 110, "Rockshore East"),
        new Zone(2087, 943, -89, 2623, 1203, 110, "Come-A-Lot"),
        new Zone(2087, 1203, -89, 2640, 1383, 110, "The Camel's Toe"),
        new Zone(2087, 1383, -89, 2437, 1543, 110, "Royal Casino"),
        new Zone(2087, 1543, -89, 2437, 1703, 110, "Caligula's Palace"),
        new Zone(2137, 1703, -89, 2437, 1783, 110, "Caligula's Palace"),
        new Zone(2437, 1383, -89, 2624, 1783, 110, "Pilgrim"),
        new Zone(2437, 1783, -89, 2685, 2012, 110, "Starfish Casino"),
        new Zone(2027, 1783, -89, 2162, 1863, 110, "The Strip"),
        new Zone(2027, 1703, -89, 2137, 1783, 110, "The Strip"),
        new Zone(2011, 2202, -89, 2237, 2508, 110, "The Emerald Isle"),
        new Zone(2162, 2012, -89, 2685, 2202, 110, "Old Venturas Strip"),
        new Zone(2498, 2626, -89, 2749, 2861, 110, "K.A.C.C. Military Fuels"),
        new Zone(2749, 1937, -89, 2921, 2669, 110, "Creek"),
        new Zone(2749, 1548, -89, 2923, 1937, 110, "Sobell Rail Yards"),
        new Zone(2749, 1198, -89, 2923, 1548, 110, "Linden Station"),
        new Zone(2623, 943, -89, 2749, 1055, 110, "Julius Thruway East"),
        new Zone(2749, 943, -89, 2923, 1198, 110, "Linden Side"),
        new Zone(2685, 1055, -89, 2749, 2626, 110, "Julius Thruway East"),
        new Zone(2498, 2542, -89, 2685, 2626, 110, "Julius Thruway North"),
        new Zone(2536, 2442, -89, 2685, 2542, 110, "Julius Thruway East"),
        new Zone(2625, 2202, -89, 2685, 2442, 110, "Julius Thruway East"),
        new Zone(2237, 2542, -89, 2498, 2663, 110, "Julius Thruway North"),
        new Zone(2121, 2508, -89, 2237, 2663, 110, "Julius Thruway North"),
        new Zone(1938, 2508, -89, 2121, 2624, 110, "Julius Thruway North"),
        new Zone(1534, 2433, -89, 1848, 2583, 110, "Julius Thruway North"),
        new Zone(1236, 2142, -89, 1297, 2243, 110, "Julius Thruway West"),
        new Zone(1848, 2478, -89, 1938, 2553, 110, "Julius Thruway North"),
        new Zone(1777, 863, -89, 1817, 2342, 110, "Harry Gold Parkway"),
        new Zone(1817, 2011, -89, 2106, 2202, 110, "Redsands East"),
        new Zone(1817, 2202, -89, 2011, 2342, 110, "Redsands East"),
        new Zone(1848, 2342, -89, 2011, 2478, 110, "Redsands East"),
        new Zone(1704, 2342, -89, 1848, 2433, 110, "Julius Thruway North"),
        new Zone(1236, 1883, -89, 1777, 2142, 110, "Redsands West"),
        new Zone(1297, 2142, -89, 1777, 2243, 110, "Redsands West"),
        new Zone(1377, 2243, -89, 1704, 2433, 110, "Redsands West"),
        new Zone(1704, 2243, -89, 1777, 2342, 110, "Redsands West"),
        new Zone(1236, 1203, -89, 1457, 1883, 110, "Las Venturas Airport"),
        new Zone(1457, 1203, -89, 1777, 1883, 110, "Las Venturas Airport"),
        new Zone(1457, 1143, -89, 1777, 1203, 110, "Las Venturas Airport"),
        new Zone(1457, 863, -89, 1777, 1143, 110, "LVA Freight Depot"),
        new Zone(1197, 1044, -89, 1277, 1163, 110, "Blackfield Intersection"),
        new Zone(1166, 795, -89, 1375, 1044, 110, "Blackfield Intersection"),
        new Zone(1277, 1044, -89, 1315, 1087, 110, "Blackfield Intersection"),
        new Zone(1375, 823, -89, 1457, 919, 110, "Blackfield Intersection"),
        new Zone(1375, 919, -89, 1457, 1203, 110, "LVA Freight Depot"),
        new Zone(1277, 1087, -89, 1375, 1203, 110, "LVA Freight Depot"),
        new Zone(1315, 1044, -89, 1375, 1087, 110, "LVA Freight Depot"),
        new Zone(1236, 1163, -89, 1277, 1203, 110, "LVA Freight Depot"),
        new Zone(964, 1044, -89, 1197, 1203, 110, "Greenglass College"),
        new Zone(964, 930, -89, 1166, 1044, 110, "Greenglass College"),
        new Zone(964, 1203, -89, 1197, 1403, 110, "Blackfield"),
        new Zone(964, 1403, -89, 1197, 1726, 110, "Blackfield"),
        new Zone(2237, 2202, -89, 2536, 2542, 110, "Roca Escalante"),
        new Zone(2536, 2202, -89, 2625, 2442, 110, "Roca Escalante"),
        new Zone(1823, 596, -89, 1997, 823, 110, "Last Dime Motel"),
        new Zone(1997, 596, -89, 2377, 823, 110, "Rockshore West"),
        new Zone(2377, 596, -89, 2537, 788, 110, "Rockshore West"),
        new Zone(1558, 596, -89, 1823, 823, 110, "Randolph Industrial Estate"),
        new Zone(1375, 596, -89, 1558, 823, 110, "Blackfield Chapel"),
        new Zone(1325, 596, -89, 1375, 795, 110, "Blackfield Chapel"),
        new Zone(1377, 2433, -89, 1534, 2507, 110, "Julius Thruway North"),
        new Zone(1098, 2243, -89, 1377, 2507, 110, "Pilson Intersection"),
        new Zone(883, 1726, -89, 1098, 2507, 110, "Whitewood Estates"),
        new Zone(1534, 2583, -89, 1848, 2863, 110, "Prickle Pine"),
        new Zone(1117, 2507, -89, 1534, 2723, 110, "Prickle Pine"),
        new Zone(1848, 2553, -89, 1938, 2863, 110, "Prickle Pine"),
        new Zone(2121, 2663, -89, 2498, 2861, 110, "Spinybed"),
        new Zone(1938, 2624, -89, 2121, 2861, 110, "Prickle Pine"),
        new Zone(2624, 1383, -89, 2685, 1783, 110, "Pilgrim"),
        new Zone(2450, 385, -100, 2759, 562, 200, "San Andreas Sound"),
        new Zone(1916, -233, -100, 2131, 13, 200, "Fisher's Lagoon"),
        new Zone(-1339, 828, -89, -1213, 1057, 110, "Garver Bridge"),
        new Zone(-1213, 950, -89, -1087, 1178, 110, "Garver Bridge"),
        new Zone(-1499, 696, -179, -1339, 925, 20, "Garver Bridge"),
        new Zone(-1339, 599, -89, -1213, 828, 110, "Kincaid Bridge"),
        new Zone(-1213, 721, -89, -1087, 950, 110, "Kincaid Bridge"),
        new Zone(-1087, 855, -89, -961, 986, 110, "Kincaid Bridge"),
        new Zone(-321, -2224, -89, 44, -1724, 110, "Los Santos Inlet"),
        new Zone(-789, 1659, -89, -599, 1929, 110, "Sherman Reservoir"),
        new Zone(-314, -753, -89, -106, -463, 110, "Flint Water"),
        new Zone(-1709, -833, 0, -1446, -730, 200, "Easter Tunnel"),
        new Zone(-2290, 2548, -89, -1950, 2723, 110, "Bayside Tunnel"),
        new Zone(-410, 1403, 0, -137, 1681, 200, "'The Big Ear'"),
        new Zone(-90, 1286, 0, 153, 1554, 200, "Lil' Probe Inn"),
        new Zone(-936, 2611, 0, -715, 2847, 200, "Valle Ocultado"),
        new Zone(1812, -1350, -89, 2056, -1100, 110, "Glen Park"),
        new Zone(2324, -2302, -89, 2703, -2145, 110, "Ocean Docks"),
        new Zone(2811, 1229, -39, 2861, 1407, 60, "Linden Station"),
        new Zone(1692, -1971, -20, 1812, -1932, 79, "Unity Station"),
        new Zone(647, -1416, -89, 787, -1227, 110, "Vinewood"),
        new Zone(787, -1410, -34, 866, -1310, 65, "Market Station"),
        new Zone(-2007, 56, 0, -1922, 224, 100, "Cranberry Station"),
        new Zone(1377, 2600, -21, 1492, 2687, 78, "Yellow Bell Station"),
        new Zone(-2616, 1501, 0, -1996, 1659, 200, "San Fierro Bay"),
        new Zone(-2616, 1659, 0, -1996, 2175, 200, "San Fierro Bay"),
        new Zone(-464, 2217, 0, -208, 2580, 200, "El Castillo del Diablo"),
        new Zone(-208, 2123, 0, 114, 2337, 200, "El Castillo del Diablo"),
        new Zone(-208, 2337, 0, 8, 2487, 200, "El Castillo del Diablo"),
        new Zone(-91, 1655, -50, 421, 2123, 250, "Restricted Area"),
        new Zone(1546, 208, 0, 1745, 347, 200, "Montgomery Intersection"),
        new Zone(1582, 347, 0, 1664, 401, 200, "Montgomery Intersection"),
        new Zone(-1119, 1178, -89, -862, 1351, 110, "Robada Intersection"),
        new Zone(-187, -1596, -89, 17, -1276, 110, "Flint Intersection"),
        new Zone(-1315, -405, 15, -1264, -209, 25, "Easter Bay Airport"),
        new Zone(-1354, -287, 15, -1315, -209, 25, "Easter Bay Airport"),
        new Zone(-1490, -209, 15, -1264, -148, 25, "Easter Bay Airport"),
        new Zone(1072, -1416, -89, 1370, -1130, 110, "Market"),
        new Zone(926, -1577, -89, 1370, -1416, 110, "Market"),
        new Zone(-2646, -355, 0, -2270, -222, 200, "Avispa Country Club"),
        new Zone(-2831, -430, 0, -2646, -222, 200, "Avispa Country Club"),
        new Zone(-2994, -811, 0, -2178, -430, 200, "Missionary Hill"),
        new Zone(-2178, -1771, -47, -1936, -1250, 576, "Mount Chiliad"),
        new Zone(-2997, -1115, -47, -2178, -971, 576, "Mount Chiliad"),
        new Zone(-2994, -2189, -47, -2178, -1115, 576, "Mount Chiliad"),
        new Zone(-2178, -2189, -47, -2030, -1771, 576, "Mount Chiliad"),
        new Zone(1117, 2723, -89, 1457, 2863, 110, "Yellow Bell Golf Course"),
        new Zone(1457, 2723, -89, 1534, 2863, 110, "Yellow Bell Golf Course"),
        new Zone(1515, 1586, -12, 1729, 1714, 87, "Las Venturas Airport"),
        new Zone(2089, -2394, -89, 2201, -2235, 110, "Ocean Docks"),
        new Zone(1382, -2730, -89, 2201, -2394, 110, "Los Santos International"),
        new Zone(2201, -2730, -89, 2324, -2418, 110, "Ocean Docks"),
        new Zone(1974, -2394, -39, 2089, -2256, 60, "Los Santos International"),
        new Zone(1400, -2669, -39, 2189, -2597, 60, "Los Santos International"),
        new Zone(2051, -2597, -39, 2152, -2394, 60, "Los Santos International"),
        new Zone(2437, 1858, -39, 2495, 1970, 60, "Starfish Casino"),
        new Zone(-399, -1075, -1, -319, -977, 198, "Beacon Hill"),
        new Zone(-2361, -417, 0, -2270, -355, 200, "Avispa Country Club"),
        new Zone(-2667, -302, -28, -2646, -262, 71, "Avispa Country Club"),
        new Zone(-2395, -222, 0, -2354, -204, 200, "Garcia"),
        new Zone(-2470, -355, 0, -2270, -318, 46, "Avispa Country Club"),
        new Zone(-2550, -355, 0, -2470, -318, 39, "Avispa Country Club"),
        new Zone(2703, -2126, -89, 2959, -1852, 110, "Playa del Seville"),
        new Zone(2703, -2302, -89, 2959, -2126, 110, "Ocean Docks"),
        new Zone(2162, 1883, -89, 2437, 2012, 110, "Starfish Casino"),
        new Zone(2162, 1783, -89, 2437, 1883, 110, "The Clown's Pocket"),
        new Zone(2324, -2145, -89, 2703, -2059, 110, "Ocean Docks"),
        new Zone(2324, -2059, -89, 2541, -1852, 110, "Willowfield"),
        new Zone(2541, -2059, -89, 2703, -1941, 110, "Willowfield"),
        new Zone(1098, 1726, -89, 1197, 2243, 110, "Whitewood Estates")
    };
    }

    public class Zone
    {
        public UnityEngine.Vector3 vmin, vmax;

        public UnityEngine.Vector2 minPos2D { get { return new UnityEngine.Vector2(this.vmin.x, this.vmin.z); } }
        public UnityEngine.Vector2 maxPos2D { get { return new UnityEngine.Vector2(this.vmax.x, this.vmax.z); } }
        public UnityEngine.Vector3 centerPos { get { return (this.vmin + this.vmax) * 0.5f; } }

        public float volume { get { UnityEngine.Vector3 size = this.vmax - this.vmin; return size.x * size.y * size.z; } }
        public float squaredSize { get { UnityEngine.Vector2 size = this.maxPos2D - this.minPos2D; return size.x * size.y; } }

        public string name;

        public static readonly string defaultZoneName = "San Andreas";

        public static Zone[] AllZones { get { return ZoneHelpers.zoneInfoList; } }


        public Zone(int x1, int z1, int y1, int x2, int z2, int y2, string n)
        {
            vmin = new UnityEngine.Vector3(x1, y1, z1);
            vmax = new UnityEngine.Vector3(x2, y2, z2);
            name = n;
        }

        public static string GetZoneName(Zone[] zones, UnityEngine.Vector3 worldPos)
        {
            float minVolume = float.PositiveInfinity;
            Zone targetZone = null;

            for (int i = 0; i < zones.Length; i++)
            {
                Zone zone = zones[i];

                if (!IsInside(worldPos, zone.vmin, zone.vmax))
                    continue;

                if (zone.volume < minVolume)
                {
                    minVolume = zone.volume;
                    targetZone = zone;
                }
            }

            if (targetZone != null)
                return targetZone.name;
            else
                return defaultZoneName;
        }

        public static string GetZoneName(Zone[] zones, UnityEngine.Vector2 worldPos2D)
        {
            float minSquaredSize = float.PositiveInfinity;
            Zone targetZone = null;

            for (int i = 0; i < zones.Length; i++)
            {
                Zone zone = zones[i];

                if (!IsInside(worldPos2D, zone))
                    continue;

                if (zone.squaredSize < minSquaredSize)
                {
                    minSquaredSize = zone.squaredSize;
                    targetZone = zone;
                }
            }

            if (targetZone != null)
                return targetZone.name;
            else
                return defaultZoneName;
        }

        public static string GetZoneName(UnityEngine.Vector3 worldPos, bool use2DPos = false)
        {
            if (use2DPos)
                return GetZoneName(worldPos.ToVec2WithXAndZ());
            return GetZoneName(ZoneHelpers.zoneInfoList, worldPos);
        }

        public static string GetZoneName(UnityEngine.Vector2 worldPos2D)
        {
            return GetZoneName(ZoneHelpers.zoneInfoList, worldPos2D);
        }

        public static bool IsInside(UnityEngine.Vector3 p, UnityEngine.Vector3 vmin, UnityEngine.Vector3 vmax)
        {
            return p.x >= vmin.x && p.x <= vmax.x && p.y >= vmin.y && p.y <= vmax.y && p.z >= vmin.z && p.z <= vmax.z;
        }

        public static bool IsInside(UnityEngine.Vector2 pos, Zone zone)
        {
            return pos.x >= zone.vmin.x && pos.x <= zone.vmax.x && pos.y >= zone.vmin.z && pos.y <= zone.vmax.z;
        }

    }

}
