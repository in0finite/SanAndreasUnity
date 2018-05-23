using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModestTree
{
    public static class TypeExtensions
    {
        public static bool DerivesFrom<T>(this Type a)
        {
            return DerivesFrom(a, typeof(T));
        }

        // This seems easier to think about than IsAssignableFrom
        public static bool DerivesFrom(this Type a, Type b)
        {
            return b != a && a.DerivesFromOrEqual(b);
        }

        public static bool DerivesFromOrEqual<T>(this Type a)
        {
            return DerivesFromOrEqual(a, typeof(T));
        }

        public static bool DerivesFromOrEqual(this Type a, Type b)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return b == a || b.GetTypeInfo().IsAssignableFrom(a.GetTypeInfo());
#else
            return b == a || b.IsAssignableFrom(a);
#endif
        }

#if !(UNITY_WSA && ENABLE_DOTNET)

        // TODO: Is it possible to do this on WSA?
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            Type baseType = givenType.BaseType;

            if (baseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(baseType, genericType);
        }

#endif

        public static bool IsEnum(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static MethodInfo[] DeclaredInstanceMethods(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetRuntimeMethods()
                .Where(x => x.DeclaringType == type).ToArray();
#else
            return type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif
        }

        public static PropertyInfo[] DeclaredInstanceProperties(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            // There doesn't appear to be an IsStatic member on PropertyInfo
            return type.GetRuntimeProperties()
                .Where(x => x.DeclaringType == type).ToArray();
#else
            return type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif
        }

        public static FieldInfo[] DeclaredInstanceFields(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetRuntimeFields()
                .Where(x => x.DeclaringType == type && !x.IsStatic).ToArray();
#else
            return type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif
        }

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
        public static bool IsAssignableFrom(this Type a, Type b)
        {
            return a.GetTypeInfo().IsAssignableFrom(b.GetTypeInfo());
        }
#endif

        public static Type BaseType(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static MethodInfo Method(this Delegate del)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return del.GetMethodInfo();
#else
            return del.Method;
#endif
        }

        public static Type[] GenericArguments(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static Type[] Interfaces(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
#else
            return type.GetInterfaces();
#endif
        }

        public static ConstructorInfo[] Constructors(this Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
#else
            return type.GetConstructors(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType())
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        // Returns name without generic arguments
        public static string GetSimpleName(this Type type)
        {
            var name = type.Name;

            var quoteIndex = name.IndexOf("`");

            if (quoteIndex == -1)
            {
                return name;
            }

            // Remove the backtick
            return name.Substring(0, quoteIndex);
        }

        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            if (type == null || type.BaseType() == null || type == typeof(object) || type.BaseType() == typeof(object))
            {
                yield break;
            }

            yield return type.BaseType();

            foreach (var ancestor in type.BaseType().GetParentTypes())
            {
                yield return ancestor;
            }
        }

        public static bool IsClosedGenericType(this Type type)
        {
            return type.IsGenericType() && type != type.GetGenericTypeDefinition();
        }

        public static bool IsOpenGenericType(this Type type)
        {
            return type.IsGenericType() && type == type.GetGenericTypeDefinition();
        }

        // Returns all instance fields, including private and public and also those in base classes
        public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type)
        {
            foreach (var fieldInfo in type.DeclaredInstanceFields())
            {
                yield return fieldInfo;
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                foreach (var fieldInfo in type.BaseType().GetAllInstanceFields())
                {
                    yield return fieldInfo;
                }
            }
        }

        // Returns all instance properties, including private and public and also those in base classes
        public static IEnumerable<PropertyInfo> GetAllInstanceProperties(this Type type)
        {
            foreach (var propInfo in type.DeclaredInstanceProperties())
            {
                yield return propInfo;
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                foreach (var propInfo in type.BaseType().GetAllInstanceProperties())
                {
                    yield return propInfo;
                }
            }
        }

        // Returns all instance methods, including private and public and also those in base classes
        public static IEnumerable<MethodInfo> GetAllInstanceMethods(this Type type)
        {
            foreach (var methodInfo in type.DeclaredInstanceMethods())
            {
                yield return methodInfo;
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                foreach (var methodInfo in type.BaseType().GetAllInstanceMethods())
                {
                    yield return methodInfo;
                }
            }
        }

        public static string Name(this Type type)
        {
            if (type.IsArray)
            {
                return string.Format("{0}[]", type.GetElementType().Name());
            }

            return (type.DeclaringType == null ? "" : type.DeclaringType.Name() + ".") + GetCSharpTypeName(type.Name);
        }

        private static string GetCSharpTypeName(string typeName)
        {
            switch (typeName)
            {
                case "String":
                case "Object":
                case "Void":
                case "Byte":
                case "Double":
                case "Decimal":
                    return typeName.ToLower();

                case "Int16":
                    return "short";

                case "Int32":
                    return "int";

                case "Int64":
                    return "long";

                case "Single":
                    return "float";

                case "Boolean":
                    return "bool";

                default:
                    return typeName;
            }
        }

        public static T GetAttribute<T>(this MemberInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes<T>().Single();
        }

        public static T TryGetAttribute<T>(this MemberInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes<T>().OnlyOrDefault();
        }

        public static bool HasAttribute(
            this MemberInfo provider, params Type[] attributeTypes)
        {
            return provider.AllAttributes(attributeTypes).Any();
        }

        public static bool HasAttribute<T>(this MemberInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Any();
        }

        public static IEnumerable<T> AllAttributes<T>(
            this MemberInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Cast<T>();
        }

        public static IEnumerable<Attribute> AllAttributes(
            this MemberInfo provider, params Type[] attributeTypes)
        {
            var allAttributes = System.Attribute.GetCustomAttributes(provider, typeof(Attribute), true);

            if (attributeTypes.Length == 0)
            {
                return allAttributes;
            }

            return allAttributes.Where(a => attributeTypes.Any(x => a.GetType().DerivesFromOrEqual(x)));
        }

        // We could avoid this duplication here by using ICustomAttributeProvider but this class
        // does not exist on the WP8 platform
        public static bool HasAttribute(
            this ParameterInfo provider, params Type[] attributeTypes)
        {
            return provider.AllAttributes(attributeTypes).Any();
        }

        public static bool HasAttribute<T>(this ParameterInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Any();
        }

        public static IEnumerable<T> AllAttributes<T>(
            this ParameterInfo provider)
            where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Cast<T>();
        }

        public static IEnumerable<Attribute> AllAttributes(
            this ParameterInfo provider, params Type[] attributeTypes)
        {
            var allAttributes = System.Attribute.GetCustomAttributes(provider, typeof(Attribute), true);

            if (attributeTypes.Length == 0)
            {
                return allAttributes;
            }

            return allAttributes.Where(a => attributeTypes.Any(x => a.GetType().DerivesFromOrEqual(x)));
        }
    }
}