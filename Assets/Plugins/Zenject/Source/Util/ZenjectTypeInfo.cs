using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zenject
{
    public class PostInjectableInfo
    {
        private readonly MethodInfo _methodInfo;
        private readonly List<InjectableInfo> _injectableInfo;

        public PostInjectableInfo(
            MethodInfo methodInfo, List<InjectableInfo> injectableInfo)
        {
            _methodInfo = methodInfo;
            _injectableInfo = injectableInfo;
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        public IEnumerable<InjectableInfo> InjectableInfo
        {
            get { return _injectableInfo; }
        }
    }

    public class ZenjectTypeInfo
    {
        private readonly List<PostInjectableInfo> _postInjectMethods;
        private readonly List<InjectableInfo> _constructorInjectables;
        private readonly List<InjectableInfo> _fieldInjectables;
        private readonly List<InjectableInfo> _propertyInjectables;
        private readonly ConstructorInfo _injectConstructor;
        private readonly Type _typeAnalyzed;

        public ZenjectTypeInfo(
            Type typeAnalyzed,
            List<PostInjectableInfo> postInjectMethods,
            ConstructorInfo injectConstructor,
            List<InjectableInfo> fieldInjectables,
            List<InjectableInfo> propertyInjectables,
            List<InjectableInfo> constructorInjectables)
        {
            _postInjectMethods = postInjectMethods;
            _fieldInjectables = fieldInjectables;
            _propertyInjectables = propertyInjectables;
            _constructorInjectables = constructorInjectables;
            _injectConstructor = injectConstructor;
            _typeAnalyzed = typeAnalyzed;
        }

        public Type Type
        {
            get { return _typeAnalyzed; }
        }

        public IEnumerable<PostInjectableInfo> PostInjectMethods
        {
            get { return _postInjectMethods; }
        }

        public IEnumerable<InjectableInfo> AllInjectables
        {
            get
            {
                return _constructorInjectables.Concat(_fieldInjectables).Concat(_propertyInjectables)
                    .Concat(_postInjectMethods.SelectMany(x => x.InjectableInfo));
            }
        }

        public IEnumerable<InjectableInfo> FieldInjectables
        {
            get { return _fieldInjectables; }
        }

        public IEnumerable<InjectableInfo> PropertyInjectables
        {
            get { return _propertyInjectables; }
        }

        public IEnumerable<InjectableInfo> ConstructorInjectables
        {
            get { return _constructorInjectables; }
        }

        // May be null
        public ConstructorInfo InjectConstructor
        {
            get { return _injectConstructor; }
        }
    }
}