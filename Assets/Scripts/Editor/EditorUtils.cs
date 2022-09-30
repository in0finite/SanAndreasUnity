using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public static class EditorUtils
    {

        private static List<Type> GetWithBaseTypes(Type type, int maxDepth)
        {
            var types = new List<Type>();
            types.Add(type);
            type = type.BaseType;

            for (int i = 0; i < maxDepth; i++)
            {
                if (type == null)
                    break;

                types.Add(type);
                type = type.BaseType;
            }

            return types;
        }

        public static object DrawFieldsAndPropertiesInInspector(
            object objectToDraw,
            int inheritanceLevel,
            bool isEditable = false)
        {
            objectToDraw = DrawFieldsInInspector(objectToDraw, inheritanceLevel, isEditable);
            objectToDraw = DrawPropertiesInInspector(objectToDraw, inheritanceLevel, isEditable);
            return objectToDraw;
        }

        public static object DrawFieldsInInspector(object objectToDraw, int inheritanceLevel, bool isEditable)
        {
            var fieldInfos = GetWithBaseTypes(objectToDraw.GetType(), inheritanceLevel)
                .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));

            foreach (var fieldInfo in fieldInfos)
            {
                object newValue = DrawObjectInInspector(
                    fieldInfo.FieldType,
                    fieldInfo.GetValue(objectToDraw),
                    fieldInfo.Name,
                    isEditable);

                if (isEditable && !fieldInfo.IsInitOnly)
                    fieldInfo.SetValue(objectToDraw, newValue);
            }

            return objectToDraw;
        }

        public static object DrawPropertiesInInspector(object objectToDraw, int inheritanceLevel, bool isEditable)
        {
            var properties = GetWithBaseTypes(objectToDraw.GetType(), inheritanceLevel)
                .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(p => p.CanRead);

            return DrawPropertiesInInspector(objectToDraw, properties, isEditable);
        }

        public static object DrawPropertiesInInspector(
            object objectToDraw, IEnumerable<PropertyInfo> properties, bool isEditable)
        {
            foreach (var propertyInfo in properties)
            {
                object newValue = DrawObjectInInspector(
                    propertyInfo.PropertyType,
                    propertyInfo.GetValue(objectToDraw),
                    propertyInfo.Name,
                    isEditable);

                if (isEditable && propertyInfo.CanWrite)
                    propertyInfo.SetValue(objectToDraw, newValue);
            }

            return objectToDraw;
        }

        public static object DrawObjectInInspector(
            Type type,
            object value,
            string name,
            bool isEditable)
        {
            string labelText = $"{name}: ";
            
            if (typeof(Component).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(labelText, (Component)value, type, true);
            }
            else if (type.IsEnum)
            {
                if (type.GetCustomAttribute<System.FlagsAttribute>() != null)
                    return EditorGUILayout.EnumFlagsField(labelText, (System.Enum)value);
                else
                    return EditorGUILayout.EnumPopup(labelText, (System.Enum)value);
            }
            else if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField(labelText, (Color) value);
            }
            else if (type == typeof(Color32))
            {
                return EditorGUILayout.ColorField(labelText, (Color32) value);
            }
            else if (type == typeof(string))
            {
                if (isEditable)
                    return EditorGUILayout.TextField(labelText, (string)value);
            }
            else if (type == typeof(int))
            {
                if (isEditable)
                    return EditorGUILayout.IntField(labelText, (int)value);
            }
            else if (type == typeof(uint))
            {
                if (isEditable)
                    return (uint)EditorGUILayout.IntField(labelText, (int)(uint)value);
            }
            else if (type == typeof(float))
            {
                if (isEditable)
                    return EditorGUILayout.FloatField(labelText, (float)value);
            }
            else if (type == typeof(bool))
            {
                if (isEditable)
                    return EditorGUILayout.Toggle(labelText, (bool)value);
            }

            EditorGUILayout.LabelField($"{labelText} {value}");
            return value;
        }

        public static bool DisplayPausableProgressBar(string title, string text, float progress, string dialogText, string ok, string cancel)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, text, progress))
            {
                EditorUtility.ClearProgressBar();
                // ok = continue
                return !EditorUtility.DisplayDialog(title, dialogText, ok, cancel);
            }

            return false;
        }

        public static bool DisplayPausableProgressBar(string title, string text, float progress)
        {
            return DisplayPausableProgressBar(title, text, progress, "Are you sure ?", "Continue", "Quit");
        }

        public static void FocusSceneViewsOnPosition(Vector3 pos)
        {
            SceneView.sceneViews.Cast<SceneView>().ForEach(s => s.LookAt(pos));
        }
    }
}
