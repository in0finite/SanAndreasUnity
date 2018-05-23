#if !NOT_UNITY3D

namespace Zenject
{
    public class NameTransformScopeArgNonLazyBinder : TransformScopeArgNonLazyBinder
    {
        public NameTransformScopeArgNonLazyBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public TransformScopeArgNonLazyBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif