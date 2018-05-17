using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    public static class ReflectionEx
    {
        public static bool Matches(this MethodInfo method, MethodInfo delegateInfo)
        {
            if (!delegateInfo.ReturnType.IsAssignableFrom(method.ReturnType)) return false;

            var mParams = method.GetParameters();
            var dParams = delegateInfo.GetParameters();

            if (mParams.Length != dParams.Length) return false;

            for (var i = 0; i < mParams.Length; ++i)
            {
                if (!dParams[i].ParameterType.IsAssignableFrom(mParams[i].ParameterType)) return false;
            }

            return true;
        }

        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider obj, bool inherit)
            where TAttribute : Attribute
        {
            return obj.GetCustomAttributes(typeof(TAttribute), inherit).Any();
        }

        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider obj, bool inherit)
            where TAttribute : Attribute
        {
            return (TAttribute)obj.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider obj, bool inherit)
            where TAttribute : Attribute
        {
            return obj.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }
    }
}