#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    public class GameObjectCreationParameters
    {
        public string Name
        {
            get;
            set;
        }

        public string GroupName
        {
            get;
            set;
        }

        public Transform ParentTransform
        {
            get;
            set;
        }

        public Func<InjectContext, Transform> ParentTransformGetter
        {
            get;
            set;
        }

        public Vector3? Position
        {
            get;
            set;
        }

        public Quaternion? Rotation
        {
            get;
            set;
        }

        public static readonly GameObjectCreationParameters Default = new GameObjectCreationParameters();

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + (this.Name == null ? 0 : this.Name.GetHashCode());
                hash = hash * 29 + (this.GroupName == null ? 0 : this.GroupName.GetHashCode());
                hash = hash * 29 + (this.ParentTransform == null ? 0 : this.ParentTransform.GetHashCode());
                hash = hash * 29 + (this.ParentTransformGetter == null ? 0 : this.ParentTransformGetter.GetHashCode());
                hash = hash * 29 + (!this.Position.HasValue ? 0 : this.Position.Value.GetHashCode());
                hash = hash * 29 + (!this.Rotation.HasValue ? 0 : this.Rotation.Value.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object other)
        {
            if (other is GameObjectCreationParameters)
            {
                GameObjectCreationParameters otherId = (GameObjectCreationParameters)other;
                return otherId == this;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(GameObjectCreationParameters that)
        {
            return this == that;
        }

        public static bool operator ==(GameObjectCreationParameters left, GameObjectCreationParameters right)
        {
            return object.Equals(left.Name, right.Name)
                && object.Equals(left.GroupName, right.GroupName);
        }

        public static bool operator !=(GameObjectCreationParameters left, GameObjectCreationParameters right)
        {
            return !left.Equals(right);
        }
    }
}

#endif