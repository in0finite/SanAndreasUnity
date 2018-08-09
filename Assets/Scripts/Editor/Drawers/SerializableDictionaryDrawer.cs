using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public abstract class SerializableKeyValueTemplate<K, V> : ScriptableObject
{
    public K key;
    public V value;
}

public abstract class SerializableDictionaryDrawer<K, V> : PropertyDrawer
{

    protected abstract SerializableKeyValueTemplate<K, V> GetTemplate();
    protected T GetGenericTemplate<T>() where T : SerializableKeyValueTemplate<K, V>
    {
        return ScriptableObject.CreateInstance<T>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);

        var firstLine = position;
        firstLine.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(firstLine, property);

        if (property.isExpanded)
        {
            var secondLine = firstLine;
            secondLine.y += EditorGUIUtility.singleLineHeight;

            EditorGUIUtility.labelWidth = 50f;

            secondLine.x += 15f; // indentation
            secondLine.width -= 15f;

            var secondLine_key = secondLine;

            var buttonWidth = 60f;
            secondLine_key.width -= buttonWidth; // assign button
            secondLine_key.width /= 2f;

            var secondLine_value = secondLine_key;
            secondLine_value.x += secondLine_value.width;
            if (GetTemplateValueProp(property).hasVisibleChildren)
            { // if the value has children, indent to make room for fold arrow
                secondLine_value.x += 15;
                secondLine_value.width -= 15;
            }

            var secondLine_button = secondLine_value;
            secondLine_button.x += secondLine_value.width;
            secondLine_button.width = buttonWidth;

            var kHeight = EditorGUI.GetPropertyHeight(GetTemplateKeyProp(property));
            var vHeight = EditorGUI.GetPropertyHeight(GetTemplateValueProp(property));
            var extraHeight = Mathf.Max(kHeight, vHeight);

            secondLine_key.height = kHeight;
            secondLine_value.height = vHeight;

            EditorGUI.PropertyField(secondLine_key, GetTemplateKeyProp(property), true);
            EditorGUI.PropertyField(secondLine_value, GetTemplateValueProp(property), true);

            var keysProp = GetKeysProp(property);
            var valuesProp = GetValuesProp(property);

            var numLines = keysProp.arraySize;

            if (GUI.Button(secondLine_button, "Assign"))
            {
                bool assignment = false;
                for (int i = 0; i < numLines; i++)
                { // Try to replace existing value
                    if (SerializedPropertyExtension.EqualBasics(GetIndexedItemProp(keysProp, i), GetTemplateKeyProp(property)))
                    {
                        SerializedPropertyExtension.CopyBasics(GetTemplateValueProp(property), GetIndexedItemProp(valuesProp, i));
                        assignment = true;
                        break;
                    }
                }
                if (!assignment)
                { // Create a new value
                    keysProp.arraySize += 1;
                    valuesProp.arraySize += 1;
                    SerializedPropertyExtension.CopyBasics(GetTemplateKeyProp(property), GetIndexedItemProp(keysProp, numLines));
                    SerializedPropertyExtension.CopyBasics(GetTemplateValueProp(property), GetIndexedItemProp(valuesProp, numLines));
                }
            }

            for (int i = 0; i < numLines; i++)
            {
                secondLine_key.y += extraHeight;
                secondLine_value.y += extraHeight;
                secondLine_button.y += extraHeight;

                kHeight = EditorGUI.GetPropertyHeight(GetIndexedItemProp(keysProp, i));
                vHeight = EditorGUI.GetPropertyHeight(GetIndexedItemProp(valuesProp, i));
                extraHeight = Mathf.Max(kHeight, vHeight);

                secondLine_key.height = kHeight;
                secondLine_value.height = vHeight;

                EditorGUI.PropertyField(secondLine_key, GetIndexedItemProp(keysProp, i), true);
                EditorGUI.PropertyField(secondLine_value, GetIndexedItemProp(valuesProp, i), true);

                if (GUI.Button(secondLine_button, "Remove"))
                {
                    keysProp.DeleteArrayElementAtIndex(i);
                    valuesProp.DeleteArrayElementAtIndex(i);
                }
            }
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        var total = EditorGUIUtility.singleLineHeight;

        var kHeight = EditorGUI.GetPropertyHeight(GetTemplateKeyProp(property));
        var vHeight = EditorGUI.GetPropertyHeight(GetTemplateValueProp(property));
        total += Mathf.Max(kHeight, vHeight);

        var keysProp = GetKeysProp(property);
        var valuesProp = GetValuesProp(property);
        int numLines = keysProp.arraySize;
        for (int i = 0; i < numLines; i++)
        {
            kHeight = EditorGUI.GetPropertyHeight(GetIndexedItemProp(keysProp, i));
            vHeight = EditorGUI.GetPropertyHeight(GetIndexedItemProp(valuesProp, i));
            total += Mathf.Max(kHeight, vHeight);
        }
        return total;
    }

    private SerializedProperty GetTemplateKeyProp(SerializedProperty mainProp)
    {
        return GetTemplateProp(templateKeyProp, mainProp);
    }
    private SerializedProperty GetTemplateValueProp(SerializedProperty mainProp)
    {
        return GetTemplateProp(templateValueProp, mainProp);
    }

    private SerializedProperty GetTemplateProp(Dictionary<int, SerializedProperty> source, SerializedProperty mainProp)
    {
        SerializedProperty p;
        if (!source.TryGetValue(mainProp.GetObjectCode(), out p))
        {
            var templateObject = GetTemplate();
            var templateSerializedObject = new SerializedObject(templateObject);
            var kProp = templateSerializedObject.FindProperty("key");
            var vProp = templateSerializedObject.FindProperty("value");
            templateKeyProp[mainProp.GetObjectCode()] = kProp;
            templateValueProp[mainProp.GetObjectCode()] = vProp;
            p = source == templateKeyProp ? kProp : vProp;
        }
        return p;
    }
    private Dictionary<int, SerializedProperty> templateKeyProp = new Dictionary<int, SerializedProperty>();
    private Dictionary<int, SerializedProperty> templateValueProp = new Dictionary<int, SerializedProperty>();

    private SerializedProperty GetKeysProp(SerializedProperty mainProp)
    {
        return GetCachedProp(mainProp, "keys", keysProps);
    }
    private SerializedProperty GetValuesProp(SerializedProperty mainProp)
    {
        return GetCachedProp(mainProp, "values", valuesProps);
    }

    private SerializedProperty GetCachedProp(SerializedProperty mainProp, string relativePropertyName, Dictionary<int, SerializedProperty> source)
    {
        SerializedProperty p;
        int objectCode = mainProp.GetObjectCode();
        if (!source.TryGetValue(objectCode, out p))
            source[objectCode] = p = mainProp.FindPropertyRelative(relativePropertyName);
        return p;
    }

    private Dictionary<int, SerializedProperty> keysProps = new Dictionary<int, SerializedProperty>();
    private Dictionary<int, SerializedProperty> valuesProps = new Dictionary<int, SerializedProperty>();

    private Dictionary<int, Dictionary<int, SerializedProperty>> indexedPropertyDicts = new Dictionary<int, Dictionary<int, SerializedProperty>>();

    private SerializedProperty GetIndexedItemProp(SerializedProperty arrayProp, int index)
    {
        Dictionary<int, SerializedProperty> d;
        if (!indexedPropertyDicts.TryGetValue(arrayProp.GetObjectCode(), out d))
            indexedPropertyDicts[arrayProp.GetObjectCode()] = d = new Dictionary<int, SerializedProperty>();
        SerializedProperty result;
        if (!d.TryGetValue(index, out result))
            d[index] = result = arrayProp.FindPropertyRelative(string.Format("Array.data[{0}]", index));
        return result;
    }

}