using UnityEditor;

public static class SerializedPropertyExtension
{

    public static int GetObjectCode(this SerializedProperty p)
    { // Unique code per serialized object and property path
        return p.propertyPath.GetHashCode() ^ p.serializedObject.GetHashCode();
    }

    public static bool EqualBasics(SerializedProperty left, SerializedProperty right)
    {
        if (left.propertyType != right.propertyType)
            return false;
        if (left.propertyType == SerializedPropertyType.Integer)
        {
            if (left.type == right.type)
            {
                if (left.type == "int")
                    return left.intValue == right.intValue;
                else
                    return left.longValue == right.longValue;
            }
            else
            {
                return false;
            }
        }
        else if (left.propertyType == SerializedPropertyType.String)
        {
            return left.stringValue == right.stringValue;
        }
        else if (left.propertyType == SerializedPropertyType.ObjectReference)
        {
            return left.objectReferenceValue == right.objectReferenceValue;
        }
        else if (left.propertyType == SerializedPropertyType.Enum)
        {
            return left.enumValueIndex == right.enumValueIndex;
        }
        else if (left.propertyType == SerializedPropertyType.Boolean)
        {
            return left.boolValue == right.boolValue;
        }
        else if (left.propertyType == SerializedPropertyType.Float)
        {
            if (left.type == right.type)
            {
                if (left.type == "float")
                    return left.floatValue == right.floatValue;
                else
                    return left.doubleValue == right.doubleValue;
            }
            else
            {
                return false;
            }
        }
        else if (left.propertyType == SerializedPropertyType.Color)
        {
            return left.colorValue == right.colorValue;
        }
        else if (left.propertyType == SerializedPropertyType.LayerMask)
        {
            return left.intValue == right.intValue;
        }
        else if (left.propertyType == SerializedPropertyType.Vector2)
        {
            return left.vector2Value == right.vector2Value;
        }
        else if (left.propertyType == SerializedPropertyType.Vector3)
        {
            return left.vector3Value == right.vector3Value;
        }
        else if (left.propertyType == SerializedPropertyType.Vector4)
        {
            return left.vector4Value == right.vector4Value;
        }
        else if (left.propertyType == SerializedPropertyType.Rect)
        {
            return left.rectValue == right.rectValue;
        }
        else if (left.propertyType == SerializedPropertyType.ArraySize)
        {
            return left.arraySize == right.arraySize;
        }
        else if (left.propertyType == SerializedPropertyType.Character)
        {
            return left.intValue == right.intValue;
        }
        else if (left.propertyType == SerializedPropertyType.AnimationCurve)
        {
            return false;
        }
        else if (left.propertyType == SerializedPropertyType.Bounds)
        {
            return left.boundsValue == right.boundsValue;
        }
        else if (left.propertyType == SerializedPropertyType.Gradient)
        {
            return false;
        }
        else if (left.propertyType == SerializedPropertyType.Quaternion)
        {
            return left.quaternionValue == right.quaternionValue;
        }
        else
        {
            return false;
        }
    }

    public static void CopyBasics(SerializedProperty source, SerializedProperty target)
    {
        if (source.propertyType != target.propertyType)
            return;
        if (source.propertyType == SerializedPropertyType.Integer)
        {
            if (source.type == target.type)
            {
                if (source.type == "int")
                    target.intValue = source.intValue;
                else
                    target.longValue = source.longValue;
            }
        }
        else if (source.propertyType == SerializedPropertyType.String)
        {
            target.stringValue = source.stringValue;
        }
        else if (source.propertyType == SerializedPropertyType.ObjectReference)
        {
            target.objectReferenceValue = source.objectReferenceValue;
        }
        else if (source.propertyType == SerializedPropertyType.Enum)
        {
            target.enumValueIndex = source.enumValueIndex;
        }
        else if (source.propertyType == SerializedPropertyType.Boolean)
        {
            target.boolValue = source.boolValue;
        }
        else if (source.propertyType == SerializedPropertyType.Float)
        {
            if (source.type == target.type)
            {
                if (source.type == "float")
                    target.floatValue = source.floatValue;
                else
                    target.doubleValue = source.doubleValue;
            }
        }
        else if (source.propertyType == SerializedPropertyType.Color)
        {
            target.colorValue = source.colorValue;
        }
        else if (source.propertyType == SerializedPropertyType.LayerMask)
        {
            target.intValue = source.intValue;
        }
        else if (source.propertyType == SerializedPropertyType.Vector2)
        {
            target.vector2Value = source.vector2Value;
        }
        else if (source.propertyType == SerializedPropertyType.Vector3)
        {
            target.vector3Value = source.vector3Value;
        }
        else if (source.propertyType == SerializedPropertyType.Vector4)
        {
            target.vector4Value = source.vector4Value;
        }
        else if (source.propertyType == SerializedPropertyType.Rect)
        {
            target.rectValue = source.rectValue;
        }
        else if (source.propertyType == SerializedPropertyType.ArraySize)
        {
            target.arraySize = source.arraySize;
        }
        else if (source.propertyType == SerializedPropertyType.Character)
        {
            target.intValue = source.intValue;
        }
        else if (source.propertyType == SerializedPropertyType.AnimationCurve)
        {
            target.animationCurveValue = source.animationCurveValue;
        }
        else if (source.propertyType == SerializedPropertyType.Bounds)
        {
            target.boundsValue = source.boundsValue;
        }
        else if (source.propertyType == SerializedPropertyType.Gradient)
        {
            // TODO?
        }
        else if (source.propertyType == SerializedPropertyType.Quaternion)
        {
            target.quaternionValue = source.quaternionValue;
        }
        else
        {
            if (source.hasChildren && target.hasChildren)
            {
                var sourceIterator = source.Copy();
                var targetIterator = target.Copy();
                while (true)
                {
                    if (sourceIterator.propertyType == SerializedPropertyType.Generic)
                    {
                        if (!sourceIterator.Next(true) || !targetIterator.Next(true))
                            break;
                    }
                    else if (!sourceIterator.Next(false) || !targetIterator.Next(false))
                    {
                        break;
                    }
                    SerializedPropertyExtension.CopyBasics(sourceIterator, targetIterator);
                }
            }
        }
    }
}