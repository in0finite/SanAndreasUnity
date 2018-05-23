#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    public class TransformScopeArgNonLazyBinder : ScopeArgNonLazyBinder
    {
        public TransformScopeArgNonLazyBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo)
        {
            GameObjectInfo = gameObjectInfo;
        }

        protected GameObjectCreationParameters GameObjectInfo
        {
            get;
            private set;
        }

        public ScopeArgNonLazyBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ScopeArgNonLazyBinder UnderTransform(Func<InjectContext, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ScopeArgNonLazyBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif