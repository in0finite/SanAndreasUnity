using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class ConcurrentBagExtensions
{
    static object locker = new object();

    public static void Clear<T>(this ConcurrentBag<T> bag)
    {
        bag = new ConcurrentBag<T>();
    }


    public static void Remove<T>(this ConcurrentBag<T> bag, List<T> itemlist)
    {
        try
        {
            lock (locker)
            {
                List<T> removelist = bag.ToList();

                Parallel.ForEach(itemlist, currentitem => {
                    removelist.Remove(currentitem);
                });

                bag = new ConcurrentBag<T>();


                Parallel.ForEach(removelist, currentitem =>
                {
                    bag.Add(currentitem);
                });
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public static void Remove<T>(this ConcurrentBag<T> bag, T removeitem)
    {
        try
        {
            lock (locker)
            {
                List<T> removelist = bag.ToList();
                removelist.Remove(removeitem);

                bag = new ConcurrentBag<T>();

                Parallel.ForEach(removelist, currentitem =>
                {
                    bag.Add(currentitem);
                });
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}