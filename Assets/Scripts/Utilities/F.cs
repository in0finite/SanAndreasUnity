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

        private static Vector3[] m_fourCornersArray = new Vector3[4];



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

        public static Vector3 ClosestPointOrBoundsCenter(this Collider collider, Vector3 position)
        {
            if (collider is MeshCollider && !((MeshCollider)collider).convex)
            {
                // ClosestPoint() function is not supported on non-convex mesh colliders
                return collider.bounds.center;
            }

            return collider.ClosestPoint(position);
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

		public static float SqrtOrZero(this float f)
		{
			if (float.IsNaN(f))
				return 0f;
			if (f <= 0f)
				return 0f;
			return Mathf.Sqrt(f);
		}

        public static double DateTimeToUnixTimestamp(this DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                     new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static string CurrentDateForLogging
        {
            get
            {
                try {
                    return System.DateTime.Now.ToString("dd MMM HH:mm:ss");
                } catch {
                    return "";
                }
            }
        }

        /// <summary>
        /// Formats the elapsed time (in seconds) in format hh:mm:ss, but removes the unnecessary prefix values which are zero.
        /// For example, 40 seconds will just return 40, 70 will return 01:10, 3700 will return 01:01:40.
        /// </summary>
        public	static	string	FormatElapsedTime( float elapsedTime ) {

	        int elapsedTimeInteger = Mathf.CeilToInt (elapsedTime);

	        int hours = elapsedTimeInteger / 3600;
	        int minutes = elapsedTimeInteger % 3600 / 60;
	        int seconds = elapsedTimeInteger % 3600 % 60 ;

	        var sb = new System.Text.StringBuilder (10);

	        if (hours > 0) {
		        if (hours < 10)
			        sb.Append ("0");
		        sb.Append (hours);
		        sb.Append (":");
	        }

	        if (hours > 0 || minutes > 0) {
		        if (minutes < 10)
			        sb.Append ("0");
		        sb.Append (minutes);
		        sb.Append (":");
	        }

	        if (seconds < 10)
		        sb.Append ("0");
	        sb.Append (seconds);

	        return sb.ToString ();
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

        public static void DestroyComponent<T> (this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T> ();
			if (comp != null)
                UnityEngine.Object.Destroy(comp);
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

        public static IEnumerable<Transform> GetAllParents(this Transform tr)
        {
            Transform currentParent = tr.parent;
            while (currentParent != null)
            {
                yield return currentParent;
                currentParent = currentParent.parent;
            }
        }

        public static bool IsParentOf(this Transform tr, Transform child)
        {
            return child.GetAllParents().Any(p => p == tr);
        }

        public static IEnumerable<Transform> GetFirstLevelChildren(this Transform tr)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                yield return tr.GetChild(i);
            }
        }

        public static IEnumerable<T> GetFirstLevelChildrenComponents<T>(this GameObject go) where T : Component
        {
            return go.transform.GetFirstLevelChildren().SelectMany(c => c.GetComponents<T>());
        }

        public static void SetY(this Transform t, float yPos) {
			Vector3 pos = t.position;
			pos.y = yPos;
			t.position = pos;
		}

        public static void SetLocalY(this Transform t, float yPos)
        {
	        Vector3 pos = t.localPosition;
	        pos.y = yPos;
	        t.localPosition = pos;
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


        public static void EnableRigidBody(Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        public static void DisableRigidBody(Rigidbody rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
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

        public static string FirstCharToUpper(this string str)
        {
	        if (string.Empty == str)
		        return str;
	        
	        return str[0].ToString().ToUpperInvariant() + str.Substring(1);
        }

        public static string ToLowerIfNotLower(this string str)
        {
	        for (int i = 0; i < str.Length; i++)
	        {
		        if (!char.IsLower(str[i]))
			        return str.ToLower();
	        }
	        return str;
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


		public static bool RunExceptionSafe (System.Action function, string errorMessagePrefix)
		{
			try {
				function();
				return true;
			} catch(System.Exception ex) {
				try {
					Debug.LogError (errorMessagePrefix + ex);
				} catch {}
			}

			return false;
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

		public	static	void	Invoke( this System.Object obj, string methodName, params object[] args ) {

			var method = obj.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			if(method != null) {
				method.Invoke( obj, args );
			}

		}

		public	static	void	InvokeExceptionSafe( this System.Object obj, string methodName, params object[] args ) {

			try {
				obj.Invoke( methodName, args );
			} catch (System.Exception ex) {
				Debug.LogException (ex);
			}

		}

        /// <summary>
		/// Invokes all subscribed delegates, and makes sure any exception is caught and logged, so that all
		/// subscribers will get notified.
		/// </summary>
		public static void InvokeEventExceptionSafe( MulticastDelegate eventDelegate, params object[] parameters )
        {
            RunExceptionSafe( () => {
                var delegates = eventDelegate.GetInvocationList ();

                foreach (var del in delegates) {
                    if (del.Method != null) {
                        try {
                            del.Method.Invoke (del.Target, parameters);
                        } catch(System.Exception ex) {
                            UnityEngine.Debug.LogException (ex);
                        }
                    }
                }
            });
        }

		public static void SendMessageToObjectsOfType<T> (string msg, params object[] args) where T : UnityEngine.Object
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

        public static TNew[] ConvertArray<TNew, TOld>(TOld[] array, Func<TOld, TNew> selector)
        {
            int length = array.Length;
            TNew[] newArray = new TNew[length];
            for (int i = 0; i < length; i++)
            {
                newArray[i] = selector(array[i]);
            }
            return newArray;
        }

		public static IEnumerable<T> DistinctBy<T,T2>(this IEnumerable<T> enumerable, System.Func<T,T2> selector)
		{
			var hashSet = new HashSet<T2>();

			foreach (T elem in enumerable)
			{
				T2 value = selector (elem);
				if (hashSet.Add(value))
					yield return elem;
			}
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

		public static bool AnyInList<T>(this IList<T> list, System.Predicate<T> predicate)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
					return true;
			}
			return false;
		}

		public static bool AddIfNotPresent<T> (this List<T> list, T item)
		{
			if (!list.Contains (item)) {
				list.Add (item);
				return true;
			}
			return false;
		}

		public static void EnsureCount<T>(this List<T> list, int count)
		{
			if (list.Count < count)
			{
				int diff = count - list.Count;
				for (int i = 0; i < diff; i++)
				{
					list.Add(default);
				}
			}
		}

        public static T RandomElement<T> (this IList<T> list)
        {
            if (list.Count < 1)
                throw new System.InvalidOperationException("List has no elements");
            return list [UnityEngine.Random.Range(0, list.Count)];
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T> action)
        {
	        foreach (var element in enumerable)
	        {
		        action(element);
	        }
        }

        public static IEnumerable<T> WhereAlive<T> (this IEnumerable<T> enumerable)
	        where T : UnityEngine.Object
        {
	        return enumerable.Where(obj => obj != null);
        }

        public static int RemoveDeadObjects<T> (this List<T> list) where T : UnityEngine.Object
        {
            return list.RemoveAll(item => null == item);
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

		public static Vector2 GetSize (this Texture2D tex)
		{
			return new Vector2 (tex.width, tex.height);
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

        public static Vector3 WithY( this Vector3 vec3, float yValue ) {
			return new Vector3 (vec3.x, yValue, vec3.z);
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

        public static Rect GetRect (this RectTransform rectTransform)
        {

            Vector3[] localCorners = m_fourCornersArray;
            rectTransform.GetLocalCorners (localCorners);

            float xMin = float.PositiveInfinity, yMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity, yMax = float.NegativeInfinity;

            for (int i = 0; i < localCorners.Length; i++) {
                Vector3 corner = localCorners [i];

                if (corner.x < xMin)
                    xMin = corner.x;
                else if (corner.x > xMax)
                    xMax = corner.x;

                if (corner.y < yMin)
                    yMin = corner.y;
                else if (corner.y > yMax)
                    yMax = corner.y;
            }

            return new Rect (xMin, yMin, xMax - xMin, yMax - yMin);
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

        public static Camera FindMainCameraEvenIfDisabled()
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (camObj != null)
                return camObj.GetComponentInChildren<Camera>(true);
            return null;
        }


		public static void GizmosDrawLineFromCamera ()
		{
			if (null == Camera.main)
				return;

			Ray ray = Camera.main.GetRayFromCenter ();

			Gizmos.DrawLine (ray.origin, ray.origin + ray.direction * Camera.main.farClipPlane);
		}

        public static void HandlesDrawText(Vector3 pos, string text, Color color)
        {
            #if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(pos, text);
            #endif
        }


        private static bool? _isInHeadlessModeCached;
	    public static bool IsInHeadlessMode
	    {
		    get
		    {
			    if (_isInHeadlessModeCached.HasValue)
				    return _isInHeadlessModeCached.Value;
			    _isInHeadlessModeCached = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
			    return _isInHeadlessModeCached.Value;
		    }
	    }

        public static bool ScreenHasHighDensity => Application.isMobilePlatform;


        public static int GetAudioClipSizeInBytes(AudioClip clip)
        {
            return clip.samples * sizeof(float);
        }

		/// <summary>
		///		Returns the ground position using X and Z position
		/// </summary>
		/// <param name="x">The X position</param>
		/// <param name="z">The Z position</param>
		/// <returns>The ground level</returns>
		public static float FindYCoordWithXZ(float x, float z)
		{
			if (Physics.Raycast(new Vector3(x, 3000, z), Vector3.down, out RaycastHit hit))
			{
				return hit.point.y;
			}
			return -1;
		}

    }
}