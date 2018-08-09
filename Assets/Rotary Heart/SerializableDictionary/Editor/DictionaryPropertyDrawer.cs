using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RotaryHeart.Lib.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(DrawableDictionary), true)]
    public class DictionaryPropertyDrawer : PropertyDrawer
    {
        #region Fields
        float lineHeight = 18;

        SerializedProperty property;
        SerializedProperty KeysValues;
        SerializedProperty KeysProp;
        SerializedProperty ValuesProp;

        GUIContent idContent = new GUIContent("Id");
        GUIContent valueContent = new GUIContent("Value");

        ReorderableList list;

        string title;

        System.Type[] typesNative =
        {
                typeof(bool),
                typeof(byte),
                typeof(float),
                typeof(int),
                typeof(string),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Matrix4x4),
                typeof(Color),
                typeof(Rect),
                typeof(LayerMask)
        };
        #endregion

        //Used to draw rects with color
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
        readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

        /// <summary>
        /// Draws a rect with a solid color
        /// </summary>
        /// <param name="position">Position to draw the rect</param>
        /// <param name="color">Color to draw the rect</param>
        /// <param name="content">Content, if any</param>
        private void DrawRect(Rect position, Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, textureStyle);
            GUI.backgroundColor = backgroundColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keys = property.FindPropertyRelative("_keys");
            var valuesProp = property.FindPropertyRelative("_values");

            // Default height 20, fixes excessive expansion. 
            float height = 20;

#if UNITY_2018_2_OR_NEWER
            height = 32;
#endif

            string keyType = keys.arrayElementType;

            if (keyType.Contains("$")) height += 20; // + height instead of absolute value.

            if (property.isExpanded)
            {
                // Default height for the add & remove buttons at the bottom of the list.
                height += 20;

                if (keys.arraySize > 0)
                {
                    for (int keyIndex = 0; keyIndex < keys.arraySize; keyIndex++)
                    {
                        var keyProp = keys.GetArrayElementAtIndex(keyIndex);

                        if (keyProp.isExpanded)
                        {
                            // The + 8 for every element gets added by the Reorderable list.
                            if (keyIndex < valuesProp.arraySize)
                            {
                                height += PropertyChildHeight(valuesProp.GetArrayElementAtIndex(keyIndex));

                                var keyElement = keys.GetArrayElementAtIndex(keyIndex);

                                if (keyElement.isExpanded)
                                {
                                    height += PropertyChildHeight(keyElement);
                                }
                            }
                        }
                        else
                        {
                            height += 22;
                        }
                    }
                }
                else height += 22; // Default height for empty list. 
            }

            return height;
        }

        /// <summary>
        /// Calculates the height for all the children properties
        /// </summary>
        /// <param name="prop">Parent proeprty</param>
        /// <returns>Height of all the children properties</returns>
        private float PropertyChildHeight(SerializedProperty prop)
        {
            float height = 0;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    height = EditorGUI.GetPropertyHeight(prop, GUIContent.none, true) + 2;
                    break;
                default:
                    height = 22;
#if UNITY_2018_2_OR_NEWER
                    height = 20;
#endif
                    break;
            }

            return height;
        }

        #region Helpers

        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private void SetTargetObjectOfProperty(SerializedProperty prop, object value, bool custom = false)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            if (Object.ReferenceEquals(obj, null)) return;

            try
            {
                var element = elements.Last();
                var tp = obj.GetType();

                if (custom)
                    tp = tp.BaseType;

                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var arr = field.GetValue(obj) as System.Collections.IList;
                    arr[index] = value;
                }
                else
                {
                    var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                    }
                }

            }
            catch
            {
                return;
            }
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private bool IsUnitySerialized(FieldInfo fieldInfo)
        {
            object[] customAttributes = fieldInfo.GetCustomAttributes(true);
            if (customAttributes.Any(x => x is System.NonSerializedAttribute))
            {
                return false;
            }
            if (fieldInfo.IsPrivate && !customAttributes.Any(x => x is SerializeField))
            {
                return false;
            }
            return IsUnitySerialized(fieldInfo.FieldType);
        }

        private bool IsUnitySerialized(System.Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return IsUnitySerialized(type.GetGenericArguments()[0]);
                }
                return false;
            }
            if (type.IsEnum)
            {
                return true;
            }
            if (type.IsValueType)
            {
                return true;
            }
            if (type.IsAssignableFrom(typeof(Object)))
            {
                return true;
            }
            if (typesNative.Contains(type) || (type.IsArray && typesNative.Contains(type.GetElementType())))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a Vector4 to Quaternion
        /// </summary>
        /// <param name="v4">Vector to convert</param>
        private Quaternion ConvertToQuaternion(Vector4 v4)
        {
            return new Quaternion(v4.x, v4.y, v4.z, v4.w);
        }

        /// <summary>
        /// Converts a Quaternion to Vector4
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        private Vector4 QuaternionToVector4(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            title = label.text;
            this.property = property;

            SerializedProperty listProp = property.FindPropertyRelative("reorderableList");

            list = GetTargetObjectOfProperty(listProp) as ReorderableList;

            KeysValues = property.FindPropertyRelative("_keyValues");
            KeysProp = property.FindPropertyRelative("_keys");
            ValuesProp = property.FindPropertyRelative("_values");

            Rect nextRect;

            string keyType = KeysProp.arrayElementType;
            int offset = 0;

            //Only draw the required references field for keys that requires a default value
            if (keyType.Contains("$"))
            {
                nextRect = GetNextRect(ref position);
                EditorGUI.PropertyField(nextRect, property.FindPropertyRelative("reqReferences"));
                offset = 20;
            }

            nextRect = GetNextRect(ref position);

            //Fix values size based on the keys size
            if (ValuesProp.arraySize != KeysProp.arraySize)
                ValuesProp.arraySize = KeysProp.arraySize;
            if (KeysValues.arraySize != KeysProp.arraySize)
                KeysValues.arraySize = KeysProp.arraySize;

            if (list != null)
            {
                if (!list.HasList)
                {
                    list = new ReorderableList(KeysProp, true, true, true);

                    //Required callbacks
                    list.onRemoveCallback += List_onRemoveCallback;
                    list.onAddCallback += List_onAddCallback;
                    list.drawElementCallback += List_drawElementCallback;
                    list.drawHeaderCallback += List_drawHeaderCallback;
                    list.getElementHeightCallback += List_getElementHeightCallback;
                    list.onElementsReorder += List_onElementsReorder;
                    list.headerExpand += List_headerExpand;

                    SetTargetObjectOfProperty(listProp, list);
                }

                list.DoList(new Rect(nextRect.x, nextRect.y, nextRect.width, GetPropertyHeight(property, label) - offset), label);
            }
        }

        private void List_headerExpand(bool expand)
        {
            for (int i = 0; i < KeysValues.arraySize; i++)
            {
                KeysProp.GetArrayElementAtIndex(i).isExpanded = expand;
                ValuesProp.GetArrayElementAtIndex(i).isExpanded = expand;
            }
        }

        private void List_onElementsReorder(int startIndex, int newIndex)
        {
            KeysValues.MoveArrayElement(startIndex, newIndex);
            //KeysProp.MoveArrayElement(startIndex, newIndex);
            ValuesProp.MoveArrayElement(startIndex, newIndex);
        }

        private float List_getElementHeightCallback(SerializedProperty element, int index)
        {
            float height;

            //Key height
            switch (element.propertyType)
            {
                case SerializedPropertyType.Generic:
                    height = EditorGUI.GetPropertyHeight(element, GUIContent.none, true);
                    break;

                default:
                    height = 20;
                    break;
            }

            //Value height
            if (element.isExpanded)
            {
                var valueProp = ValuesProp.GetArrayElementAtIndex(index);

                switch (valueProp.propertyType)
                {
                    case SerializedPropertyType.Generic:
                        height += EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true);
                        break;
                    default:
                        height += 20;
                        break;
                }
            }

            return height;
        }

        private void List_drawHeaderCallback(Rect rect, GUIContent label)
        {
            rect.x += 6;

            list.List.isExpanded = property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "", true);
            EditorGUI.LabelField(rect, title);
        }

        private void List_drawElementCallback(Rect rect, SerializedProperty element, GUIContent label, int index, bool selected, bool focused)
        {
            var keyValueProp = KeysValues.GetArrayElementAtIndex(index);
            var keyProp = KeysProp.GetArrayElementAtIndex(index);
            var valueProp = ValuesProp.GetArrayElementAtIndex(index);

            SerializedProperty keyToUse = keyProp.propertyType == SerializedPropertyType.Generic ? keyProp : keyValueProp;

            //Only draw the color if this entry is selected
            if (!selected)
            {
                Color color = Color.white * 0.4f;
                color.a = 1;

                DrawRect(rect, color);
            }

            rect.height = lineHeight;

            Rect keyRect = new Rect(rect.x + 50, rect.y, rect.width - 50, rect.height);
            Rect valueRect = new Rect(keyRect);

            #region Key Field

            //Check if it contains the new attribute
            bool containsAttribute = fieldInfo.GetCustomAttributes(typeof(DrawKeyAsPropertyAttribute), true).Any();

            string propName = "";
            if (containsAttribute)
            {
                foreach (var fi in fieldInfo.FieldType.BaseType.GetField("_keys", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FieldType.GetElementType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (IsUnitySerialized(fi))
                    {
                        propName = fi.Name;
                        break;
                    }
                }
            }

            //Draw only if its not a generic type or can be draw as property
            if ((containsAttribute && !string.IsNullOrEmpty(propName)) || keyToUse.propertyType != SerializedPropertyType.Generic)
            {
                keyProp.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 15, rect.y, 20, rect.height), keyProp.isExpanded, idContent, true);
            }

            GUI.SetNextControlName("CheckGenericFocus" + index);

            EditorGUI.BeginProperty(rect, GUIContent.none, keyToUse);

            switch (keyToUse.propertyType)
            {
                case SerializedPropertyType.Quaternion:
                    EditorGUI.BeginChangeCheck();
                    var newV4 = EditorGUI.Vector4Field(keyRect, GUIContent.none, QuaternionToVector4(keyToUse.quaternionValue));

                    if (EditorGUI.EndChangeCheck())
                    {
                        keyToUse.quaternionValue = ConvertToQuaternion(newV4);
                    }
                    break;

                case SerializedPropertyType.Enum:
                    string[] names = keyToUse.enumDisplayNames;
                    var selectedVal = names[keyToUse.enumValueIndex];

                    //Draw button with dropdown style
                    if (GUI.Button(keyRect, selectedVal, EditorStyles.layerMaskField))
                    {
                        List<string> usedNames = new List<string>();
                        GenericMenu menu = new GenericMenu();

                        //Add all the used values
                        for (int i = 0; i < KeysValues.arraySize; i++)
                        {
                            usedNames.Add(names[KeysValues.GetArrayElementAtIndex(i).enumValueIndex]);
                        }

                        //Add all the menu items
                        for (int i = 0; i < names.Length; i++)
                        {
                            int nameIndex = i;

                            //If the value is being used, show it disabled
                            if (usedNames.Contains(names[nameIndex]) && !names[nameIndex].Equals(selectedVal))
                            {
                                menu.AddDisabledItem(new GUIContent(names[nameIndex]));
                            }
                            else
                            {
                                menu.AddItem(new GUIContent(names[nameIndex]), selectedVal == names[nameIndex], () =>
                                {
                                    keyToUse.enumValueIndex = nameIndex;
                                    keyToUse.serializedObject.ApplyModifiedProperties();
                                });
                            }
                        }

                        //Show menu under mouse position
                        menu.ShowAsContext();

                        Event.current.Use();
                    }
                    break;

                case SerializedPropertyType.Generic:
                    //Only draw as property if values are correct
                    if (containsAttribute && !string.IsNullOrEmpty(propName))
                    {
                        EditorGUI.PropertyField(keyRect, keyToUse.FindPropertyRelative(propName), GUIContent.none, false);
                    }
                    else
                    {
                        keyRect.height = EditorGUI.GetPropertyHeight(keyToUse, idContent);
                        EditorGUI.PropertyField(new Rect(rect.x + 15, rect.y, keyRect.width + 35, keyRect.height), keyToUse, idContent, true);
                    }
                    break;

                default:
                    EditorGUI.PropertyField(keyRect, keyToUse, GUIContent.none, false);
                    break;
            }
            EditorGUI.EndProperty();

            //Not used for generic type
            if (keyToUse.propertyType != SerializedPropertyType.Generic)
            {
                //Old key value
                var oldId = GetKeyValue(keyProp);
                //New key value
                var newId = GetKeyValue(keyValueProp);

                //Notify if the key is empty or null
                if ((keyToUse.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(newId.ToString())) || newId == null)
                {
                    GUIContent content = EditorGUIUtility.IconContent("console.warnicon.sml");
                    content.tooltip = "ID cannot be left empty";

                    GUI.Button(new Rect(keyRect.x - 15, keyRect.y, 30, 30), content, GUIStyle.none);
                }
                //Check if the key value has been changed
                else if (!oldId.Equals(newId))
                {
                    //Be sure that the dictionary doesn't contain an element with this key
                    if (ContainsId(newId, index))
                    {
                        //Check if this key is still focused
                        if (GUI.GetNameOfFocusedControl().Equals("CheckGenericFocus" + index))
                        {
                            //Notify the user that this key already exists
                            GUIContent content = EditorGUIUtility.IconContent("console.erroricon.sml");
                            content.tooltip = "Dictionary already has this id, this id cannot be used";

                            GUI.Button(new Rect(keyRect.x - 15, keyRect.y, 30, 30), content, GUIStyle.none);
                        }
                        else
                        {
                            //If it's not, set the correct key back. This is to avoid having multiple errors with ids
                            SetValue(keyValueProp, oldId);
                        }
                    }
                    else
                    {
                        //Set the value
                        SetGenericValue(keyProp, valueProp, newId);
                    }
                }
            }
            #endregion Fey Field

            valueRect.y = keyRect.yMax + 3;
            valueRect.x -= 20;
            valueRect.width += 20;

            #region Value Field

            //Special case for generic types
            if (valueProp.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUI.BeginChangeCheck();
                rect.y += EditorGUIUtility.singleLineHeight;
                //Value field
                if (keyProp.isExpanded)
                {
                    EditorGUI.BeginProperty(valueRect, GUIContent.none, valueProp);

                    if (valueProp.propertyType == SerializedPropertyType.Quaternion)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newV4 = EditorGUI.Vector4Field(valueRect, GUIContent.none, QuaternionToVector4(valueProp.quaternionValue));

                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.quaternionValue = ConvertToQuaternion(newV4);
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(valueRect, valueProp, valueContent, true);
                    }
                    EditorGUI.EndProperty();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    //This is used to apply the modified changes
                    ValuesProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                //Value field
                if (keyProp.isExpanded)
                {
                    valueRect.x -= 10;
                    valueRect.width += 10;

                    EditorGUI.BeginProperty(valueRect, GUIContent.none, valueProp);

                    EditorGUI.PrefixLabel(valueRect, valueContent);
                    if (valueProp.propertyType == SerializedPropertyType.Quaternion)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newV4 = EditorGUI.Vector4Field(new Rect(valueRect.x + 45, valueRect.y, valueRect.width - 45, valueRect.height), GUIContent.none, QuaternionToVector4(valueProp.quaternionValue));

                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.quaternionValue = ConvertToQuaternion(newV4);
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(new Rect(valueRect.x + 45, valueRect.y, valueRect.width - 45, valueRect.height), valueProp, GUIContent.none, true);
                    }
                    EditorGUI.EndProperty();
                }
            }

            #endregion Value Field
        }

        private void List_onAddCallback(ReorderableList list)
        {
            KeysValues.arraySize = ValuesProp.arraySize = ++KeysProp.arraySize;

            KeysValues.serializedObject.ApplyModifiedProperties();
            ValuesProp.serializedObject.ApplyModifiedProperties();
            KeysProp.serializedObject.ApplyModifiedProperties();

            SetPropertyDefault(KeysValues.GetArrayElementAtIndex(KeysValues.arraySize - 1), KeysValues);
            SetPropertyDefault(KeysProp.GetArrayElementAtIndex(KeysProp.arraySize - 1), KeysProp);
            SetPropertyDefault(ValuesProp.GetArrayElementAtIndex(ValuesProp.arraySize - 1), null);
        }

        private void List_onRemoveCallback(ReorderableList list)
        {
            for (int i = list.Selected.Length - 1; i >= 0; i--)
            {
                int index = list.Selected[i];

                int last = KeysProp.arraySize - 1;

                KeysValues.MoveArrayElement(index, last);
                KeysProp.MoveArrayElement(index, last);
                ValuesProp.MoveArrayElement(index, last);

                KeysValues.arraySize--;
                KeysProp.arraySize--;
                ValuesProp.arraySize--;
            }

            ValuesProp.serializedObject.ApplyModifiedProperties();
            ValuesProp.serializedObject.Update();
        }

        /// <summary>
        /// Checks if the <paramref name="KeysProp"/> contains the id
        /// </summary>
        /// <param name="obj">Id to check</param>
        /// <param name="index">Property index on the array</param>
        /// <returns>True if an element is already using the id; otherwise, false</returns>
        private bool ContainsId(object obj, int index)
        {
            for (int i = 0; i < KeysProp.arraySize; i++)
            {
                if (index == i)
                {
                    continue;
                }

                object val = GetKeyValue(KeysProp.GetArrayElementAtIndex(i));

                if (val.Equals(obj))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the current property value
        /// </summary>
        /// <param name="prop">Property to check</param>
        /// <returns>object representation of the property value</returns>
        private object GetKeyValue(SerializedProperty prop)
        {
            object obj = null;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    obj = prop.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    obj = prop.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    obj = prop.floatValue;
                    break;
                case SerializedPropertyType.String:
                    obj = prop.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    obj = prop.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    obj = prop.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    obj = prop.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    obj = prop.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    obj = prop.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    obj = prop.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    obj = prop.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    obj = prop.arraySize;
                    break;
                case SerializedPropertyType.Character:
                    obj = (char)prop.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    obj = prop.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    obj = prop.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    obj = GetGradientValue(prop);
                    break;
                case SerializedPropertyType.Quaternion:
                    obj = prop.quaternionValue;
                    break;
                case SerializedPropertyType.Generic:
                    obj = GetTargetObjectOfProperty(prop);
                    break;
                default:
                    Debug.LogError("Key Type not implemented: " + prop.propertyType);
                    break;
            }

            return obj;
        }

        /// <summary>
        /// Sets the property value
        /// </summary>
        /// <param name="prop">Property to modify</param>
        /// <param name="obj">Value</param>
        private void SetValue(SerializedProperty prop, object obj)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (int)obj;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)obj;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)obj;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = (string)obj;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)obj;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = (Object)obj;
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = (int)obj;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)obj;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)obj;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = (Vector4)obj;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)obj;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int)obj;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (char)obj;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = (AnimationCurve)obj;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)obj;
                    break;
                case SerializedPropertyType.Gradient:
                    SetGradientValue(prop, (Gradient)obj);
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = (Quaternion)obj;
                    break;
                case SerializedPropertyType.Generic:
                    SetTargetObjectOfProperty(prop, null);
                    break;
                default:
                    Debug.Log("Type not implemented: " + prop.propertyType);
                    break;
            }
        }

        /// <summary>
        /// Tries to get the Gradient value of the <paramref name="prop"/> using reflection, if it fails returns null
        /// </summary>
        /// <param name="prop">SerializedProperty to get the value from</param>
        /// <returns>Gradient value, or null if it fails</returns>
        private Gradient GetGradientValue(SerializedProperty prop)
        {
            PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(prop, null) as Gradient;
        }

        /// <summary>
        /// Tries to set the Gradient value of the <paramref name="prop"/> using reflection, if it fails nothing is saved
        /// </summary>
        /// <param name="prop">SerializedProperty to get the value from</param>
        /// <param name="gradient">Gradient value to save</param>
        private void SetGradientValue(SerializedProperty prop, Gradient gradient)
        {
            PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                return;

            propertyInfo.SetValue(prop, gradient, null);
        }

        /// <summary>
        /// Special check for a dictionary with a generic value type. This tries to set the key to its id field
        /// </summary>
        /// <param name="keyProp">Key proeprty</param>
        /// <param name="valueProp">Value property</param>
        /// <param name="obj">Key value to set</param>
        private void SetGenericValue(SerializedProperty keyProp, SerializedProperty valueProp, object obj)
        {
            SetValue(keyProp, obj);

            IDAttribute attribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(IDAttribute)) as IDAttribute;

            if (attribute == null)
            {
                //This generic dictionary doesn't contain an id attribute
                return;
            }

            SerializedProperty id = valueProp.FindPropertyRelative(attribute.Id);

            if (id == null)
            {
                Debug.LogError("Couldn't find any id field with name '" + attribute.Id + "' on field: " + fieldInfo.Name);
                return;
            }

            SetValue(id, obj);
        }

        /// <summary>
        /// Returns the next rect forcing the height to be of a single line
        /// </summary>
        /// <param name="position">Position reference</param>
        /// <returns>Next rect</returns>
        private Rect GetNextRect(ref Rect position)
        {
            var h = lineHeight + 1f;
            var r = new Rect(position.x, position.y, position.width, lineHeight);
            position = new Rect(position.x, position.y + h, position.width, h);
            return r;
        }

        /// <summary>
        /// Sets the default value based on the property
        /// </summary>
        /// <param name="prop">Property</param>
        private void SetPropertyDefault(SerializedProperty prop, SerializedProperty parentProperty)
        {
            if (prop == null)
                throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = int.MaxValue;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = Mathf.Infinity;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = Color.black;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = -1;
                    break;
                case SerializedPropertyType.Enum:
                    int index = 0;

                    if (parentProperty != null)
                    {
                        List<int> numbersUsed = new List<int>();

                        for (int i = 0; i < parentProperty.arraySize; i++)
                            numbersUsed.Add(parentProperty.GetArrayElementAtIndex(i).enumValueIndex);

                        while (true)
                        {
                            if (!numbersUsed.Contains(index))
                            {
                                break;
                            }
                            index++;
                        }

                        if (index >= prop.enumNames.Length)
                            index = 0;
                    }

                    prop.enumValueIndex = index;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = Vector4.zero;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = Rect.zero;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = 0;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = 0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = null;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = default(Bounds);
                    break;
                case SerializedPropertyType.Gradient:
                    SetGradientValue(prop, new Gradient());
                    break;
                case SerializedPropertyType.Generic:
                    //Used to initialized all the values on the generic type
                    var t = prop.GetEnumerator();
                    while (t.MoveNext())
                    {
                        var val = t.Current;
                        SetPropertyDefault((val as SerializedProperty), null);
                    }
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = Quaternion.identity;
                    break;
                default:
                    Debug.Log("Type not implemented: " + prop.propertyType);
                    break;
            }
        }
    }
}
