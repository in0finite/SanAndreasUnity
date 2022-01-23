namespace SanAndreasUnity.Utilities
{
    public class Ref<T>
    {
        public T value;

        public Ref()
        {
        }

        public Ref(T value)
        {
            this.value = value;
        }
    }
}
