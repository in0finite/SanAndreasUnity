using HtmlAgilityPack;
using SanAndreasUnity.Behaviours.Vehicles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

		public static int RoundToInt(this float f)
		{
			return Mathf.RoundToInt (f);
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

		public static T GetOrAddComponent<T> (this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T> ();
			if (null == comp)
				comp = go.AddComponent<T> ();
			return comp;
		}

		public static T GetComponentOrThrow<T> (this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T> ();
			if (null == comp)
				throw new MissingComponentException (string.Format ("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name));
			return comp;
		}

		public static T GetComponentOrThrow<T> (this Component comp) where T : Component
		{
			return comp.gameObject.GetComponentOrThrow<T> ();
		}

		public static T GetComponentOrLogError<T> (this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T> ();
			if (null == comp)
				Debug.LogErrorFormat ("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name);
			return comp;
		}

		public static T GetComponentOrLogError<T> (this Component comp) where T : Component
		{
			return comp.gameObject.GetComponentOrLogError<T> ();
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

		public static void SetY(this Transform t, float yPos) {
			Vector3 pos = t.position;
			pos.y = yPos;
			t.position = pos;
		}

		public static float Distance(this Transform t, Vector3 pos)
		{
			return Vector3.Distance (t.position, pos);
		}

		public static Quaternion CreateRotationAroundAxes (Vector3 degrees)
		{
			Quaternion rotation = Quaternion.identity;

			if (degrees.x != 0)
				rotation *= Quaternion.AngleAxis (degrees.x, Vector3.right);

			if (degrees.y != 0)
				rotation *= Quaternion.AngleAxis (degrees.y, Vector3.up);

			if (degrees.z != 0)
				rotation *= Quaternion.AngleAxis (degrees.z, Vector3.forward);
			
			return rotation;
		}

		public static Vector3 TransformDirection (this Quaternion rot, Vector3 dir)
		{
			return rot * dir;
		}

		/// <summary>
		/// Transforms the rotation from local space to world space.
		/// </summary>
		public static Quaternion TransformRotation (this Transform tr, Quaternion rot)
		{
			Vector3 localForward = rot * Vector3.forward;
			Vector3 localUp = rot * Vector3.up;

			return Quaternion.LookRotation (tr.TransformDirection (localForward), tr.TransformDirection (localUp));
		}

		public static void SetGlobalScale (this Transform tr, Vector3 globalScale)
		{
			Vector3 parentGlobalScale = tr.parent != null ? tr.parent.lossyScale : Vector3.one;
			tr.localScale = Vector3.Scale (globalScale, parentGlobalScale.Inverted () );
		}

		public static Vector3 ClampDirection (Vector3 dir, Vector3 referenceVec, float maxAngle)
		{
			float angle = Vector3.Angle (dir, referenceVec);
			if (angle > maxAngle) {
				// needs to be clamped

				return Vector3.RotateTowards( dir, referenceVec, (angle - maxAngle) * Mathf.Deg2Rad, 0f );
			//	Vector3.Lerp( dir, referenceVec, );
			}

			return dir;
		}


        public static object FromHex(this string hexString, Type type, CultureInfo info)
        {
            var argTypes = new[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider) };

            var convert = type.GetMethod("Parse",
                            BindingFlags.Static | BindingFlags.Public,
                            null, argTypes, null);

            return convert.Invoke(null, new object[] { hexString, NumberStyles.HexNumber, info });
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

        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        public static string GetGameObjectPath(this GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }


		public static float GetTimePerc (this AnimationState state)
		{
			return state.time / state.length;
		}

		public static void SetTimePerc (this AnimationState state, float perc)
		{
			state.time = state.length * perc;
		}


		public static void RunExceptionSafe (System.Action function)
		{
			try {
				function();
			} catch(System.Exception ex) {
				try {
					Debug.LogException (ex);
				} catch {}
			}
		}

		public	static	void	Invoke( this Component component, string methodName, params object[] args ) {

			var method = component.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			if(method != null) {
				method.Invoke( component, args );
			}

		}

		public	static	void	InvokeExceptionSafe( this Component component, string methodName, params object[] args ) {

			try {
				component.Invoke( methodName, args );
			} catch (System.Exception ex) {
				Debug.LogException (ex);
			}

		}

		public static void SendMessageToObjectsOfType<T> (string msg, params object[] args) where T : UnityEngine.Component
		{
			var objects = UnityEngine.Object.FindObjectsOfType<T> ();

			foreach (var obj in objects) {
				obj.InvokeExceptionSafe (msg, args);
			}

		}


        public static bool IsCasteable<T>(this object input)
        {
            try
            {
                Convert.ChangeType(input, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T[] AddValue<T>(this T[] arr, T value)
        {
            return (new List<T>(arr.ToList()) { value }).ToArray();
        }

        public static T[] Add<T>(this T[] target, T item)
        {
            if (target == null)
                throw new ArgumentNullException();

            T[] result = new T[target.Length + 1];
            target.CopyTo(result, 0);
            result[target.Length] = item;

            return result;
        }

		public static IEnumerable<T> DistinctBy<T,T2>(this IEnumerable<T> enumerable, System.Func<T,T2> selector)
		{
			List<KeyValuePair<T,T2>> list = new List<KeyValuePair<T, T2>>();

			foreach (var elem in enumerable) {
				var value = selector (elem);
				if (!list.Exists (item => item.Value.Equals (value)))
					list.Add (new KeyValuePair<T, T2> (elem, value));
			}

			return list.Select (item => item.Key);
		}

		public static int FindIndex<T> (this IEnumerable<T> enumerable, System.Predicate<T> predicate)
		{
			int i = 0;
			foreach (var elem in enumerable) {
				if (predicate (elem))
					return i;
				i++;
			}
			return -1;
		}

		public static int IndexOf<T> (this IEnumerable<T> enumerable, T value)
		{
			return enumerable.FindIndex (elem => elem.Equals (value));
		}

		public static bool AddIfNotPresent<T> (this List<T> list, T item)
		{
			if (!list.Contains (item)) {
				list.Add (item);
				return true;
			}
			return false;
		}


        private static Dictionary<string, Texture2D> Texturemap = new Dictionary<string, Texture2D>();
        private static Texture2D Font;

        public static Color[] WriteLetterToTexture(this char chipName, int fontWidth = 12, int fontHeight = 18)
        {
            if (Font == null)
            {
                Debug.Log("Loaded chipfont!");
                Font = Resources.Load<Texture2D>("Textures/chipfont");
            }

            // Copy each letter to the texture
            int cur_id = (int)(chipName - '0');

            return Font.GetPixels(cur_id * fontWidth, 0, fontWidth, fontHeight);
        }

        // WIP: Vector2? offset = null
        public static Texture2D WriteTextToTexture(this string chipName, Texture2D texture, int fontWidth = 12, int fontHeight = 18)
        {
            if (Font == null)
                Font = Resources.Load<Texture2D>("Textures/chipfont");

            // If texture already exists, don't create it again

            int offset = 5;
            int offset_y = Font.height;

            // Copy each letter to the texture
            for (int i = 0; i < chipName.Length; i++)
            {
                int cur_id = (int)(chipName[i] - '0');
                for (int y = 0; y < fontHeight; y++)
                {
                    for (int x = 0; x < fontWidth; x++)
                    {
                        Color tempColor = Font.GetPixel(cur_id * fontWidth + x, offset_y - y);
                        texture.SetPixel(offset + x, offset_y - y + 10, tempColor);
                    }
                }
                offset += fontWidth;
            }

            texture.Apply();

            return texture;
        }

        public static Texture2D TextToTexture(this string chipName, int fontWidth = 12, int fontHeight = 18)
        {
            if (Font == null)
                Font = Resources.Load<Texture2D>("Textures/chipfont");

            // If texture already exists, don't create it again
            if (!Texturemap.ContainsKey(chipName))
            {
                int textureWidth = 100;
                // Generate the texture
                var texture = new Texture2D(textureWidth, 100, TextureFormat.ARGB32, false);

                for (int y = 0; y < 100; y++)
                {
                    for (int x = 0; x < textureWidth; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                int offset = 5;
                int offset_y = Font.height;

                // Copy each letter to the texture
                for (int i = 0; i < chipName.Length; i++)
                {
                    int cur_id = (int)(chipName[i] - '0');
                    for (int y = 0; y < fontHeight; y++)
                    {
                        for (int x = 0; x < fontWidth; x++)
                        {
                            Color tempColor = Font.GetPixel(cur_id * fontWidth + x, offset_y - y);
                            texture.SetPixel(offset + x, offset_y - y + 10, tempColor);
                        }
                    }
                    offset += fontWidth;
                }

                // Apply all SetPixel calls
                texture.Apply();

                Texturemap[chipName] = texture;
            }

            return Texturemap[chipName];
        }

        // Slow method
        public static int CountObjectsInLayer(int layer)
        {
            int i = 0;
            foreach (Transform t in Object.FindObjectsOfType<Transform>())
                if (t.gameObject.layer == layer)
                    ++i;

            return i;
        }

        public static bool IsGreaterOrEqual(this Vector2 local, Vector2 other)
        {
            if (local.x >= other.x && local.y >= other.y)
                return true;
            else
                return false;
        }

        public static bool IsLesserOrEqual(this Vector2 local, Vector2 other)
        {
            if (local.x <= other.x && local.y <= other.y)
                return true;
            else
                return false;
        }

        public static bool IsGreater(this Vector2 local, Vector2 other, bool orOperator = false)
        {
            if (orOperator)
            {
                if (local.x > other.x || local.y > other.y)
                    return true;
                else
                    return false;
            }
            else
            {
                if (local.x > other.x && local.y > other.y)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsLesser(this Vector2 local, Vector2 other, bool orOperator = false)
        {
            if (orOperator)
            {
                if (local.x < other.x || local.y < other.y)
                    return true;
                else
                    return false;
            }
            else
            {
                if (local.x < other.x && local.y < other.y)
                    return true;
                else
                    return false;
            }
        }

		public static Vector2 ToVec2WithXAndZ( this Vector3 vec3 ) {
			return new Vector2 (vec3.x, vec3.z);
		}

		public static Vector3 WithXAndZ( this Vector3 vec3 ) {
			return new Vector3 (vec3.x, 0f, vec3.z);
		}

		public static Vector3 Inverted (this Vector3 vec3)
		{
			return new Vector3 (1.0f / vec3.x, 1.0f / vec3.y, 1.0f / vec3.z);
		}

		public static Color OrangeColor { get { return Color.Lerp (Color.yellow, Color.red, 0.5f); } }

		/// <summary>
		/// Clamps all coordinates between 0 and 1.
		/// </summary>
		public static Rect Clamp01(this Rect rect) {

			float xMin = rect.xMin;
			float xMax = rect.xMax;
			float yMin = rect.yMin;
			float yMax = rect.yMax;

			xMin = Mathf.Clamp01 (xMin);
			xMax = Mathf.Clamp01 (xMax);
			yMin = Mathf.Clamp01 (yMin);
			yMax = Mathf.Clamp01 (yMax);

			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public	static	bool	Contains(this Rect rect, Rect other) {

			return rect.xMax >= other.xMax && rect.xMin <= other.xMin && rect.yMax >= other.yMax && rect.yMin <= other.yMin;

		}

		public	static	Rect	Intersection(this Rect rect, Rect other) {

			float xMax = Mathf.Min (rect.xMax, other.xMax);
			float yMax = Mathf.Min (rect.yMax, other.yMax);

			float xMin = Mathf.Max (rect.xMin, other.xMin);
			float yMin = Mathf.Max (rect.yMin, other.yMin);

			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public	static	Rect	Normalized(this Rect rect, Rect outter) {

			float xMin = (rect.xMin - outter.xMin) / outter.width;
			float xMax = (rect.xMax - outter.xMin) / outter.width;

			float yMin = (rect.yMin - outter.yMin) / outter.height;
			float yMax = (rect.yMax - outter.yMin) / outter.height;

			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public	static	Rect	CreateRect(Vector2 center, Vector2 size) {
			return new Rect (center - size / 2.0f, size);
		}

		public	static	Texture2D	CreateTexture (int width, int height, Color color) {

			Color[] pixels = new Color[width * height];

			for (int i = 0; i < pixels.Length; i++)
				pixels [i] = color;

			Texture2D texture = new Texture2D (width, height);
			texture.SetPixels (pixels);
			texture.Apply ();

			return texture;
		}


		public static Ray GetRayFromCenter (this Camera cam)
		{
			Vector3 viewportPos = new Vector3 (0.5f, 0.5f, 0f);
			return cam.ViewportPointToRay (viewportPos);
		}


		public static void GizmosDrayLineFromCamera ()
		{
			if (null == Camera.main)
				return;

			Ray ray = Camera.main.GetRayFromCenter ();

			Gizmos.DrawLine (ray.origin, ray.origin + ray.direction * Camera.main.farClipPlane);
		}

    }
}