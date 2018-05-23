using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class ArgNonLazyBinder : NonLazyBinder
    {
        public ArgNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        // We use generics instead of params object[] so that we preserve type info
        // So that you can for example pass in a variable that is null and the type info will
        // still be used to map null on to the correct field
        public ArgNonLazyBinder WithArguments<T>(T param)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param);
            return this;
        }

        public ArgNonLazyBinder WithArguments<TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param1, param2);
            return this;
        }

        public ArgNonLazyBinder WithArguments<TParam1, TParam2, TParam3>(
            TParam1 param1, TParam2 param2, TParam3 param3)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param1, param2, param3);
            return this;
        }

        public ArgNonLazyBinder WithArguments<TParam1, TParam2, TParam3, TParam4>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param1, param2, param3, param4);
            return this;
        }

        public ArgNonLazyBinder WithArguments<TParam1, TParam2, TParam3, TParam4, TParam5>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param1, param2, param3, param4, param5);
            return this;
        }

        public ArgNonLazyBinder WithArguments<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            BindInfo.Arguments = InjectUtil.CreateArgListExplicit(param1, param2, param3, param4, param5, param6);
            return this;
        }

        public ArgNonLazyBinder WithArguments(object[] args)
        {
            BindInfo.Arguments = InjectUtil.CreateArgList(args);
            return this;
        }

        public ArgNonLazyBinder WithArgumentsExplicit(IEnumerable<TypeValuePair> extraArgs)
        {
            BindInfo.Arguments = extraArgs.ToList();
            return this;
        }
    }
}