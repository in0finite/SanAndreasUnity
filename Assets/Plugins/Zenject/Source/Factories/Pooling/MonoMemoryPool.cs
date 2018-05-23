using UnityEngine;

namespace Zenject
{
    // Zero parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TValue> : MemoryPool<TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }

    // One parameter
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TParam1, TValue> : MemoryPool<TParam1, TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }

    // Two parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TParam1, TParam2, TValue>
        : MemoryPool<TParam1, TParam2, TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }

    // Three parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TParam1, TParam2, TParam3, TValue>
        : MemoryPool<TParam1, TParam2, TParam3, TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }

    // Four parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
        : MemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }

    // Five parameters
    // NOTE: For this to work, the given component must be at the root game object of the thing
    // you want to use in a pool
    public class MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        : MemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        where TValue : Component
    {
        protected override void OnCreated(TValue item)
        {
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(TValue item)
        {
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TValue item)
        {
            item.gameObject.SetActive(false);
        }
    }
}