using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zenject
{
    public static class TypeAnalyzer
    {
        private static Dictionary<Type, ZenjectTypeInfo> _typeInfo = new Dictionary<Type, ZenjectTypeInfo>();

        public static ZenjectTypeInfo GetInfo<T>()
        {
            return GetInfo(typeof(T));
        }

        public static ZenjectTypeInfo GetInfo(Type type)
        {
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start("Zenject Reflection"))
#endif
            {
                Assert.That(!type.IsAbstract(),
                    "Tried to analyze abstract type '{0}'.  This is not currently allowed.", type);

                ZenjectTypeInfo info;

#if ZEN_MULTITHREADING
                lock (_typeInfo)
#endif
                {
                    if (!_typeInfo.TryGetValue(type, out info))
                    {
                        info = CreateTypeInfo(type);
                        _typeInfo.Add(type, info);
                    }
                }

                return info;
            }
        }

        private static ZenjectTypeInfo CreateTypeInfo(Type type)
        {
            var constructor = GetInjectConstructor(type);

            return new ZenjectTypeInfo(
                type,
                GetPostInjectMethods(type),
                constructor,
                GetFieldInjectables(type).ToList(),
                GetPropertyInjectables(type).ToList(),
                GetConstructorInjectables(type, constructor).ToList());
        }

        private static IEnumerable<InjectableInfo> GetConstructorInjectables(Type parentType, ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                return Enumerable.Empty<InjectableInfo>();
            }

            return constructorInfo.GetParameters().Select(
                paramInfo => CreateInjectableInfoForParam(parentType, paramInfo));
        }

        private static InjectableInfo CreateInjectableInfoForParam(
            Type parentType, ParameterInfo paramInfo)
        {
            var injectAttributes = paramInfo.AllAttributes<InjectAttributeBase>().ToList();

            Assert.That(injectAttributes.Count <= 1,
                "Found multiple 'Inject' attributes on type parameter '{0}' of type '{1}'.  Parameter should only have one", paramInfo.Name, parentType);

            var injectAttr = injectAttributes.SingleOrDefault();

            object identifier = null;
            bool isOptional = false;
            InjectSources sourceType = InjectSources.Any;

            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
                isOptional = injectAttr.Optional;
                sourceType = injectAttr.Source;
            }

            bool isOptionalWithADefaultValue = (paramInfo.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;

            return new InjectableInfo(
                isOptionalWithADefaultValue || isOptional,
                identifier,
                paramInfo.Name,
                paramInfo.ParameterType,
                parentType,
                null,
                isOptionalWithADefaultValue ? paramInfo.DefaultValue : null,
                sourceType);
        }

        private static List<PostInjectableInfo> GetPostInjectMethods(Type type)
        {
            // Note that unlike with fields and properties we use GetCustomAttributes
            // This is so that we can ignore inherited attributes, which is necessary
            // otherwise a base class method marked with [Inject] would cause all overridden
            // derived methods to be added as well
            var methods = type.GetAllInstanceMethods()
                .Where(x => x.GetCustomAttributes(typeof(InjectAttribute), false).Any()).ToList();

            var heirarchyList = type.Yield().Concat(type.GetParentTypes()).Reverse().ToList();

            // Order by base classes first
            // This is how constructors work so it makes more sense
            var values = methods.OrderBy(x => heirarchyList.IndexOf(x.DeclaringType));

            var postInjectInfos = new List<PostInjectableInfo>();

            foreach (var methodInfo in values)
            {
                var paramsInfo = methodInfo.GetParameters();

                var injectAttr = methodInfo.AllAttributes<InjectAttribute>().Single();
                Assert.That(!injectAttr.Optional && injectAttr.Id == null && injectAttr.Source == InjectSources.Any,
                    "Parameters of InjectAttribute do not apply to constructors and methods");

                postInjectInfos.Add(
                    new PostInjectableInfo(
                        methodInfo,
                        paramsInfo.Select(paramInfo =>
                            CreateInjectableInfoForParam(type, paramInfo)).ToList()));
            }

            return postInjectInfos;
        }

        private static IEnumerable<InjectableInfo> GetPropertyInjectables(Type type)
        {
            var propInfos = type.GetAllInstanceProperties()
                .Where(x => x.HasAttribute(typeof(InjectAttributeBase)));

            foreach (var propInfo in propInfos)
            {
                yield return CreateForMember(propInfo, type);
            }
        }

        private static IEnumerable<InjectableInfo> GetFieldInjectables(Type type)
        {
            var fieldInfos = type.GetAllInstanceFields()
                .Where(x => x.HasAttribute(typeof(InjectAttributeBase)));

            foreach (var fieldInfo in fieldInfos)
            {
                yield return CreateForMember(fieldInfo, type);
            }
        }

#if !(UNITY_WSA && ENABLE_DOTNET) || UNITY_EDITOR

        private static IEnumerable<FieldInfo> GetAllFields(Type t, BindingFlags flags)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            return t.GetFields(flags).Concat(GetAllFields(t.BaseType, flags)).Distinct();
        }

        private static Action<object, object> GetOnlyPropertySetter(
            Type parentType,
            string propertyName)
        {
            Assert.That(parentType != null);
            Assert.That(!string.IsNullOrEmpty(propertyName));

            var allFields = GetAllFields(
                parentType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();

            var writeableFields = allFields.Where(f => f.Name == string.Format("<{0}>k__BackingField", propertyName)).ToList();

            if (!writeableFields.Any())
            {
                throw new ZenjectException(string.Format(
                    "Can't find backing field for get only property {0} on {1}.\r\n{2}",
                    propertyName, parentType.FullName, string.Join(";", allFields.Select(f => f.Name).ToArray())));
            }

            return (injectable, value) => writeableFields.ForEach(f => f.SetValue(injectable, value));
        }

#endif

        private static InjectableInfo CreateForMember(MemberInfo memInfo, Type parentType)
        {
            var injectAttributes = memInfo.AllAttributes<InjectAttributeBase>().ToList();

            Assert.That(injectAttributes.Count <= 1,
                "Found multiple 'Inject' attributes on type field '{0}' of type '{1}'.  Field should only container one Inject attribute", memInfo.Name, parentType);

            var injectAttr = injectAttributes.SingleOrDefault();

            object identifier = null;
            bool isOptional = false;
            InjectSources sourceType = InjectSources.Any;

            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
                isOptional = injectAttr.Optional;
                sourceType = injectAttr.Source;
            }

            Type memberType;
            Action<object, object> setter;

            if (memInfo is FieldInfo)
            {
                var fieldInfo = (FieldInfo)memInfo;
                setter = ((object injectable, object value) => fieldInfo.SetValue(injectable, value));
                memberType = fieldInfo.FieldType;
            }
            else
            {
                Assert.That(memInfo is PropertyInfo);
                var propInfo = (PropertyInfo)memInfo;
                memberType = propInfo.PropertyType;

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
                setter = ((object injectable, object value) => propInfo.SetValue(injectable, value, null));
#else
                if (propInfo.CanWrite)
                {
                    setter = ((object injectable, object value) => propInfo.SetValue(injectable, value, null));
                }
                else
                {
                    setter = GetOnlyPropertySetter(parentType, propInfo.Name);
                }
#endif
            }

            return new InjectableInfo(
                isOptional,
                identifier,
                memInfo.Name,
                memberType,
                parentType,
                setter,
                null,
                sourceType);
        }

        private static ConstructorInfo GetInjectConstructor(Type parentType)
        {
            var constructors = parentType.Constructors();

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            // WP8 generates a dummy constructor with signature (internal Classname(UIntPtr dummy))
            // So just ignore that
            constructors = constructors.Where(c => !IsWp8GeneratedConstructor(c)).ToArray();
#endif

            if (constructors.IsEmpty())
            {
                return null;
            }

            if (constructors.HasMoreThan(1))
            {
                var explicitConstructor = (from c in constructors where c.HasAttribute<InjectAttribute>() select c).SingleOrDefault();

                if (explicitConstructor != null)
                {
                    return explicitConstructor;
                }

                // If there is only one public constructor then use that
                // This makes decent sense but is also necessary on WSA sometimes since the WSA generated
                // constructor can sometimes be private with zero parameters
                var singlePublicConstructor = constructors.Where(x => !x.IsPrivate).OnlyOrDefault();

                if (singlePublicConstructor != null)
                {
                    return singlePublicConstructor;
                }

                return null;
            }

            return constructors[0];
        }

#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
        static bool IsWp8GeneratedConstructor(ConstructorInfo c)
        {
            ParameterInfo[] args = c.GetParameters();

            if (args.Length == 1)
            {
                return args[0].ParameterType == typeof(UIntPtr)
                    && (string.IsNullOrEmpty(args[0].Name) || args[0].Name == "dummy");
            }

            if (args.Length == 2)
            {
                return args[0].ParameterType == typeof(UIntPtr)
                    && args[1].ParameterType == typeof(Int64*)
                    && (string.IsNullOrEmpty(args[0].Name) || args[0].Name == "dummy")
                    && (string.IsNullOrEmpty(args[1].Name) || args[1].Name == "dummy");
            }

            return false;
        }
#endif
    }
}