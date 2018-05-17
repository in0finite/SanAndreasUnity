using UnityEngine;

namespace System.Collections.Generic
{
    public static class ListEx
    {
        public static void RemoveRangeWrapped<T>(this List<T> list, int index, int count)
        {
            int itemCount = list.Count;

            if (count == 0 || itemCount == 0) return;

            if (count >= itemCount)
            {
                list.Clear();

                return;
            }

            int removeIndex = index;

            if (count < 0)
            {
                removeIndex += count;
                count = -count;
            }

            removeIndex = removeIndex.Mod(itemCount);
            int backCount = count - (itemCount - removeIndex);

            if (backCount <= 0)
            {
                list.RemoveRange(removeIndex, count);
            }
            else
            {
                list.RemoveRange(removeIndex, count - backCount);
                list.RemoveRange(0, backCount);
            }
        }

        public static int GetOffsetIndex<T>(this List<T> list, int index, int offset)
        {
            // wrap around the offsetted index
            return (index + offset).Mod(list.Count);
        }
    }
}