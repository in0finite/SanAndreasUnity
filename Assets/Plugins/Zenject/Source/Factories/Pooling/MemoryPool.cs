namespace Zenject
{
    // Zero parameters
    public class MemoryPool<TValue> : MemoryPoolBase<TValue>, IMemoryPool<TValue>
    {
        public TValue Spawn()
        {
            var item = GetInternal();
            Reinitialize(item);
            return item;
        }

        protected virtual void Reinitialize(TValue item)
        {
            // Optional
        }
    }

    // One parameter
    public class MemoryPool<TParam1, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TValue>
    {
        public TValue Spawn(TParam1 param)
        {
            var item = GetInternal();
            Reinitialize(param, item);
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TValue item)
        {
            // Optional
        }
    }

    // Two parameters
    public class MemoryPool<TParam1, TParam2, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2)
        {
            var item = GetInternal();
            Reinitialize(param1, param2, item);
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TValue item)
        {
            // Optional
        }
    }

    // Three parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            var item = GetInternal();
            Reinitialize(param1, param2, param3, item);
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TValue item)
        {
            // Optional
        }
    }

    // Four parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        public TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            var item = GetInternal();
            Reinitialize(param1, param2, param3, param4, item);
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TValue item)
        {
            // Optional
        }
    }

    // Five parameters
    public class MemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
        : MemoryPoolBase<TValue>, IMemoryPool<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        public TValue Spawn(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            var item = GetInternal();
            Reinitialize(param1, param2, param3, param4, param5, item);
            return item;
        }

        protected virtual void Reinitialize(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4, TParam5 p5, TValue item)
        {
            // Optional
        }
    }
}