using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if !NOT_UNITY3D

using UnityEngine;

#endif

namespace ModestTree.Util
{
    public static class ReflectionUtil
    {
        public static bool IsGenericList(Type type)
        {
            return type.IsGenericType()
                && (type.GetGenericTypeDefinition() == typeof(List<>)
                        || type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public static bool IsGenericList(Type type, out Type contentsType)
        {
            if (IsGenericList(type))
            {
                contentsType = type.GenericArguments().Single();
                return true;
            }

            contentsType = null;
            return false;
        }

        public static IList CreateGenericList(Type elementType, object[] contentsAsObj)
        {
            var genericType = typeof(List<>).MakeGenericType(elementType);

            var list = (IList)Activator.CreateInstance(genericType);

            foreach (var obj in contentsAsObj)
            {
                if (obj != null)
                {
                    Assert.That(obj.GetType().DerivesFromOrEqual(elementType),
                        "Wrong type when creating generic list, expected something assignable from '" + elementType + "', but found '" + obj.GetType() + "'");
                }

                list.Add(obj);
            }

            return list;
        }

        public static IDictionary CreateGenericDictionary(
            Type keyType, Type valueType, object[] keysAsObj, object[] valuesAsObj)
        {
            Assert.That(keysAsObj.Length == valuesAsObj.Length);

            var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            var dictionary = (IDictionary)Activator.CreateInstance(genericType);

            for (int i = 0; i < keysAsObj.Length; i++)
            {
                dictionary.Add(keysAsObj[i], valuesAsObj[i]);
            }

            return dictionary;
        }

        public static object DowncastList<TFrom, TTo>(IEnumerable<TFrom> fromList) where TTo : class, TFrom
        {
            var toList = new List<TTo>();

            foreach (var obj in fromList)
            {
                toList.Add(obj as TTo);
            }

            return toList;
        }

        public static IEnumerable<IMemberInfo> GetFieldsAndProperties<T>(BindingFlags flags)
        {
            return GetFieldsAndProperties(typeof(T), flags);
        }

        public static IEnumerable<IMemberInfo> GetFieldsAndProperties(Type type, BindingFlags flags)
        {
            foreach (var propInfo in type.GetProperties(flags))
            {
                yield return new PropertyMemberInfo(propInfo);
            }

            foreach (var fieldInfo in type.GetFields(flags))
            {
                yield return new FieldMemberInfo(fieldInfo);
            }
        }

        public static string ToDebugString(this MethodInfo method)
        {
            return "{0}.{1}".Fmt(method.DeclaringType.Name(), method.Name);
        }

        public static string ToDebugString(this Action action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1>(this Action<TParam1> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2>(this Action<TParam1, TParam2> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3>(this Action<TParam1, TParam2, TParam3> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3, TParam4>(this Action<TParam1, TParam2, TParam3, TParam4> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3, TParam4, TParam5>(this ModestTree.Util.Action<TParam1, TParam2, TParam3, TParam4, TParam5> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this ModestTree.Util.Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> action)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return action.ToString();
#else
            return action.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1>(this Func<TParam1> func)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return func.ToString();
#else
            return func.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2>(this Func<TParam1, TParam2> func)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return func.ToString();
#else
            return func.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3>(this Func<TParam1, TParam2, TParam3> func)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return func.ToString();
#else
            return func.Method.ToDebugString();
#endif
        }

        public static string ToDebugString<TParam1, TParam2, TParam3, TParam4>(this Func<TParam1, TParam2, TParam3, TParam4> func)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return func.ToString();
#else
            return func.Method.ToDebugString();
#endif
        }

        public interface IMemberInfo
        {
            Type MemberType
            {
                get;
            }

            string MemberName
            {
                get;
            }

            object GetValue(object instance);

            void SetValue(object instance, object value);
        }

        public class PropertyMemberInfo : IMemberInfo
        {
            private PropertyInfo _propInfo;

            public PropertyMemberInfo(PropertyInfo propInfo)
            {
                _propInfo = propInfo;
            }

            public Type MemberType
            {
                get
                {
                    return _propInfo.PropertyType;
                }
            }

            public string MemberName
            {
                get
                {
                    return _propInfo.Name;
                }
            }

            public object GetValue(object instance)
            {
                try
                {
#if NOT_UNITY3D
                    return _propInfo.GetValue(instance, null);
#else
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // GetValue() doesn't work on webgl for some reason
                        // This is a bit slower though so only do this on webgl
                        return _propInfo.GetGetMethod().Invoke(instance, null);
                    }
                    else
                    {
                        return _propInfo.GetValue(instance, null);
                    }
#endif
                }
                catch (Exception e)
                {
                    throw new Exception("Error occurred while accessing property '{0}'".Fmt(_propInfo.Name), e);
                }
            }

            public void SetValue(object instance, object value)
            {
                _propInfo.SetValue(instance, value, null);
            }
        }

        public class FieldMemberInfo : IMemberInfo
        {
            private FieldInfo _fieldInfo;

            public FieldMemberInfo(FieldInfo fieldInfo)
            {
                _fieldInfo = fieldInfo;
            }

            public Type MemberType
            {
                get
                {
                    return _fieldInfo.FieldType;
                }
            }

            public string MemberName
            {
                get
                {
                    return _fieldInfo.Name;
                }
            }

            public object GetValue(object instance)
            {
                return _fieldInfo.GetValue(instance);
            }

            public void SetValue(object instance, object value)
            {
                _fieldInfo.SetValue(instance, value);
            }
        }
    }
}