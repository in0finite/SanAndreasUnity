namespace Zenject
{
    public enum PoolExpandMethods
    {
        OneAtATime,
        Double,
        Fixed,
    }

    public class MemoryPoolBindInfo
    {
        public MemoryPoolBindInfo()
        {
        }

        public PoolExpandMethods ExpandMethod
        {
            get; set;
        }

        public int InitialSize
        {
            get; set;
        }
    }
}