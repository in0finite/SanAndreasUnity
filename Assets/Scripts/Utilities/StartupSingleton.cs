namespace SanAndreasUnity.Utilities
{
    public class StartupSingleton<T> : SingletonComponent<T>
        where T : StartupSingleton<T>
    {
    }
}
