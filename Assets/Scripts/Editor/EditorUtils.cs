using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public static class EditorUtils
    {

        public static void DrawAllFieldsInInspector(object objectToDraw)
        {
            var fieldInfos = objectToDraw
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var fieldInfo in fieldInfos)
            {
                var type = fieldInfo.FieldType;
                var value = fieldInfo.GetValue(objectToDraw);
                string labelText = $"{fieldInfo.Name}: ";

                if (type.IsAssignableFrom(typeof(Component)))
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
}
