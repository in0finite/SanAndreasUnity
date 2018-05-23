#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    public class TransformScopeConditionCopyNonLazyBinder : ScopeConditionCopyNonLazyBinder
    {
        public TransformScopeConditionCopyNonLazyBinder(
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

        public ScopeConditionCopyNonLazyBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ScopeConditionCopyNonLazyBinder UnderTransform(Func<InjectContext, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ScopeConditionCopyNonLazyBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif