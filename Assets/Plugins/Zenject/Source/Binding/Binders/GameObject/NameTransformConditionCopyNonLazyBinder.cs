#if !NOT_UNITY3D

namespace Zenject
{
    public class NameTransformConditionCopyNonLazyBinder : TransformConditionCopyNonLazyBinder
    {
        public NameTransformConditionCopyNonLazyBinder(
            BindInfo bindInfo, GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public TransformConditionCopyNonLazyBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif