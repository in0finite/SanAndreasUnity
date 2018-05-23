using System;

namespace Zenject
{
    public interface IMemoryPool
    {
        int NumTotal { get; }
        int NumActive { get; }
        int NumInactive { get; }

        Type ItemType
        {
            get;
        }
    }

    public interface IMemoryPool<TValue> : IMemoryPool
    {
        TValue Spawn();

        void Despawn(TValue item);
    }

    public interface IMemoryPool<in TParam1, TValue> : IMemoryPool
    {
        TValue Spawn(TParam1 param);

        void Despawn(TValue item);
    }

    public interface IMemoryPool<in TParam1, in TParam2, TValue> : IMemoryPool
    {
        TValue Spawn(TParam1 param1, TParam2 param2);

        void Despawn(TValue item);
    }

    public interface IMemoryPool<in TParam1, in TParam2, in TParam3, TValue> : IMemoryPool
    {
        TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3);

        void Despawn(TValue item);
    }

    public interface IMemoryPool<in TParam1, in TParam2, in TParam3, in TParam4, TValue> : IMemoryPool
    {
        TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);

        void Despawn(TValue item);
    }

    public interface IMemoryPool<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, TValue> : IMemoryPool
    {
        TValue Spawn(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);

        void Despawn(TValue item);
    }
}