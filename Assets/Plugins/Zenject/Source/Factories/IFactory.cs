namespace Zenject
{
    public interface IFactory
    {
    }

    public interface IFactory<out TValue> : IFactory
    {
        TValue Create();
    }

    public interface IFactory<in TParam1, out TValue> : IFactory
    {
        TValue Create(TParam1 param);
    }

    public interface IFactory<in TParam1, in TParam2, out TValue> : IFactory
    {
        TValue Create(TParam1 param1, TParam2 param2);
    }

    public interface IFactory<in TParam1, in TParam2, in TParam3, out TValue> : IFactory
    {
        TValue Create(TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public interface IFactory<in TParam1, in TParam2, in TParam3, in TParam4, out TValue> : IFactory
    {
        TValue Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }

    public interface IFactory<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, out TValue> : IFactory
    {
        TValue Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
    }
}