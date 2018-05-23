#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    public class TransformConditionCopyNonLazyBinder : ConditionCopyNonLazyBinder
    {
        public TransformConditionCopyNonLazyBinder(BindInfo bindInfo, GameObjectCreationParameters gameObjInfo)
            : base(bindInfo)
        {
            GameObjectInfo = gameObjInfo;
        }

        protected GameObjectCreationParameters GameObjectInfo
        {
            get;
            private set;
        }

        public ConditionCopyNonLazyBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ConditionCopyNonLazyBinder UnderTransform(Func<InjectContext, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ConditionCopyNonLazyBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif