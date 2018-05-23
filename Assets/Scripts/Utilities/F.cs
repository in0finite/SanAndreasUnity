using HtmlAgilityPack;
using SanAndreasUnity.Behaviours.Vehicles;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Utilities
{
    //Static class with extra functions
    public static class F
    {
        //Returns the number with the greatest absolute value
        public static float MaxAbs(params float[] nums)
        {
            float result = 0;

            for (int i = 0; i < nums.Length; i++)
            {
                if (Mathf.Abs(nums[i]) > Mathf.Abs(result))
                {
                    result = nums[i];
                }
            }

            return result;
        }

        //Returns the topmost parent with a certain component
        public static Component GetTopmostParentComponent<T>(Transform tr) where T : Component
        {
            Component getting = null;

            while (tr.parent != null)
            {
                if (tr.parent.GetComponent<T>() != null)
                {
                    getting = tr.parent.GetComponent<T>();
                }

                tr = tr.parent;
            }

            return getting;
        }

        // WIP: This causes Unity to crash
        /*public static void OptimizeVehicle(this Vehicle v)
        {
            foreach (var col in v.gameObject.GetComponentsInChildren<Collider>())
            {
                if (!(col is MeshCollider))
                    Object.Destroy(col);
            }

            foreach (var go in v.gameObject.GetComponentsInChildren<MeshFilter>())
                go.gameObject.AddComponent<MeshCollider>();
        }*/

        public static void OptimizeVehicle(this Vehicle v)
        {
            var cols = v.gameObject.GetComponentsInChildren<Collider>().Where(x => x.GetType() != typeof(MeshCollider));
            foreach (var col in cols)
                col.enabled = false;

            var filters = v.gameObject.GetComponentsInChildren<MeshFilter>().Where(x => x.sharedMesh != null);
            foreach (var filter in filters)
                filter.gameObject.AddComponent<MeshCollider>();
        }

        public static Mesh GetSharedMesh(this Collider col)
        {
            if (col is MeshCollider)
            {
                return ((MeshCollider)col).sharedMesh;
            }
            else
            {
                // WIP: Depending on the collider generate a diferent shape
                MeshFilter f = col.gameObject.GetComponent<MeshFilter>();
                return f != null ? f.sharedMesh : null;
            }
        }

        public static bool BetweenInclusive(this float v, float min, float max)
        {
            return v >= min && v <= max;
        }

        public static bool BetweenExclusive(this float v, float min, float max)
        {
            return v > min && v < max;
        }

        public static double DateTimeToUnixTimestamp(this DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                     new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static string Nl2Br(this string str)
        {
            return str.Replace(Environment.NewLine, "<br>");
        }

        public static string OptimizeHTML(this string str)
        {
            return str.Replace("<", "&lt;").Replace(">", "&gt;").Nl2Br().Replace("<br>", "[br]");
        }

        public static string CleanElement(this string html, string element)
        {
            string orEl = string.Format(@"\[{0}\]", element);

            return Regex.Replace(html, orEl, (m) => { return Callback(m, element, html); });
        }

        private static string Callback(Match match, string element, string html)
        {
            int oc = FindOccurrences(html.Substring(0, match.Index + 1), match.Index);
            string befChar = html.Substring(match.Index - (element.Length + 3), 1);
            string sep = new string(Convert.ToChar(9), oc);
            return string.Format("{3}<{0}>{1}{2}", element, Environment.NewLine, sep, befChar.Replace(Environment.NewLine, " ").IsNullOrWhiteSpace() ? sep : "");
        }

        private static int FindOccurrences(string str, int maxIndex)
        {
            int lio = str.LastIndexOf("</");
            //Debug.LogFormat("LastIndexOf: {0}\nMatchIndex: {1}\nLength: {2}", lio, maxIndex, str.Length);
            return Regex.Matches(str.Substring(lio, maxIndex - lio), @"<\w+").Count;
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null) return true;
            return string.IsNullOrEmpty(value.Trim());
        }

        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 0;
        }

        public static string IndentHtml(this HtmlNode _node)
        {
            return _node.OuterHtml.XmlBeautifier().CleanElement("hr").CleanElement("br");
        }

        public static String NameOf<T>(this T obj)
        {
            return typeof(T).ToString();
        }

        public static T GetComponentWithName<T>(this Component root, string name) where T : Component
        {
            return root.GetComponentsInChildren<T>().FirstOrDefault(x => x.name == name);
        }

        public static void MakeChild(this Transform parent, GameObject[] children)
        {
            MakeChild(parent, children, null);
        }

        //Make the game objects children of the parent.
        public static void MakeChild(this Transform parent, GameObject[] children, Action<Transform, GameObject> actionPerLoop)
        {
            foreach (GameObject child in children)
            {
                child.transform.parent = parent;
                if (actionPerLoop != null) actionPerLoop(parent, child);
            }
        }

        public static object FromHex(this string hexString, Type type)
        {
            bool signed = Convert.ToBoolean(type.GetField("MinValue").GetValue(null));

            if (signed)
                return long.Parse(hexString, NumberStyles.AllowHexSpecifier);
            else
                return ulong.Parse(hexString, NumberStyles.AllowHexSpecifier);
        }

        public static void SafeDestroy<T>(this T obj) where T : Object
        {
            if (Application.isEditor)
                Object.DestroyImmediate(obj);
            else
                Object.Destroy(obj);
        }

        public static void SafeDestroyGameObject<T>(this T component) where T : Component
        {
            if (component != null)
                SafeDestroy(component.gameObject);
        }
    }
}