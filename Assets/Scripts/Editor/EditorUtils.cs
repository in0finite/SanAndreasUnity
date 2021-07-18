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

        public static void DrawFieldsAndPropertiesInInspector(object objectToDraw, int inheritanceLevel)
        {
            DrawFieldsInInspector(objectToDraw, inheritanceLevel);
            DrawPropertiesInInspector(objectToDraw, inheritanceLevel);
        }

        public static void DrawFieldsInInspector(object objectToDraw, int inheritanceLevel)
        {
            var fieldInfos = GetWithBaseTypes(objectToDraw.GetType(), inheritanceLevel)
                .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));

            foreach (var fieldInfo in fieldInfos)
            {
                DrawObjectInInspector(fieldInfo.FieldType, fieldInfo.GetValue(objectToDraw), fieldInfo.Name);
            }
        }

        public static void DrawPropertiesInInspector(object objectToDraw, int inheritanceLevel)
        {
            var properties = GetWithBaseTypes(objectToDraw.GetType(), inheritanceLevel)
                .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(p => p.CanRead);

            foreach (var propertyInfo in properties)
            {
                DrawObjectInInspector(propertyInfo.PropertyType, propertyInfo.GetValue(objectToDraw), propertyInfo.Name);
            }
        }

        public static void DrawObjectInInspector(
            Type type,
            object value,
            string name)
        {
            string labelText = $"{name}: ";

            if (typeof(Component).IsAssignableFrom(type))
            {
                EditorGUILayout.ObjectField(labelText, value as Component, type, true);
            }
            else if (type.IsEnum)
            {
                if (type.GetCustomAttribute<System.FlagsAttribute>() != null)
                    EditorGUILayout.EnumFlagsField(labelText, value as System.Enum);
                else
                    EditorGUILayout.EnumPopup(labelText, value as System.Enum);
            }
            else if (type == typeof(Color))
            {
                EditorGUILayout.ColorField(labelText, (Color) value);
            }
            else if (type == typeof(Color32))
            {
                EditorGUILayout.ColorField(labelText, (Color32) value);
            }
            else
            {
                EditorGUILayout.LabelField($"{labelText} {value}");
            }
        }
    }
}
