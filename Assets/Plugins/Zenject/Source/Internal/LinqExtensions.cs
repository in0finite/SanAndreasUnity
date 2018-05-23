using ModestTree.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModestTree
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> first, Action<T> action)
        {
            foreach (T t in first)
            {
                action(t);
            }
        }

        // Inclusive because it includes the item that meets the predicate
        public static IEnumerable<TSource> TakeUntilInclusive<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (var item in source)
            {
                yield return item;
                if (predicate(item))
                {
                    yield break;
                }
            }
        }

        // Return the first item when the list is of length one and otherwise returns default
        public static TSource OnlyOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var results = source.Take(2).ToArray();
            return results.Length == 1 ? results[0] : default(TSource);
        }

        // Another name for IEnumerable.Reverse()
        // This is useful to distinguish betweeh List.Reverse() when dealing with a list
        public static IEnumerable<T> Reversed<T>(this IEnumerable<T> list)
        {
            return list.Reverse();
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (T t in second)
            {
                yield return t;
            }

            foreach (T t in first)
            {
                yield return t;
            }
        }

        // These are more efficient than Count() in cases where the size of the collection is not known
        public static bool HasAtLeast<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount).Count() == amount;
        }

        public static bool HasMoreThan<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.HasAtLeast(amount + 1);
        }

        public static bool HasLessThan<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.HasAtMost(amount - 1);
        }

        public static bool HasAtMost<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount + 1).Count() <= amount;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> list)
        {
            return list.GroupBy(x => x).Where(x => x.Skip(1).Any()).Select(x => x.Key);
        }

        public static IEnumerable<T> ReplaceOrAppend<T>(
            this IEnumerable<T> enumerable, Predicate<T> match, T replacement)
        {
            bool replaced = false;

            foreach (T t in enumerable)
            {
                if (match(t))
                {
                    replaced = true;
                    yield return replacement;
                }
                else
                {
                    yield return t;
                }
            }

            if (!replaced)
            {
                yield return replacement;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (T)enumerator.Current;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        // This is more efficient than just Count() < x because it will end early
        // rather than iterating over the entire collection
        public static bool IsLength<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount + 1).Count() == amount;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T item)
        {
            return list.Except(item.Yield());
        }

        public static T GetSingle<T>(this object[] objectArray, bool required)
        {
            if (required)
            {
                return objectArray.Where(x => x is T).Cast<T>().Single();
            }
            else
            {
                return objectArray.Where(x => x is T).Cast<T>().SingleOrDefault();
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            return DistinctByImpl(source, keySelector, comparer);
        }

        private static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static T Second<T>(this IEnumerable<T> list)
        {
            return list.Skip(1).First();
        }

        public static T SecondOrDefault<T>(this IEnumerable<T> list)
        {
            return list.Skip(1).FirstOrDefault();
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Func<T, bool> predicate)
        {
            int numRemoved = 0;

            var currentNode = list.First;
            while (currentNode != null)
            {
                if (predicate(currentNode.Value))
                {
                    var toRemove = currentNode;
                    currentNode = currentNode.Next;
                    list.Remove(toRemove);
                    numRemoved++;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }

            return numRemoved;
        }

        // LINQ already has a method called "Contains" that does the same thing as this
        // BUT it fails to work with Mono 3.5 in some cases.
        // For example the following prints False, True in Mono 3.5 instead of True, True like it should:
        //
        // IEnumerable<string> args = new string[]
        // {
        //     "",
        //     null,
        // };

        // Log.Info(args.ContainsItem(null));
        // Log.Info(args.Where(x => x == null).Any());
        public static bool ContainsItem<T>(this IEnumerable<T> list, T value)
        {
            // Use object.Equals to support null values
            return list.Where(x => object.Equals(x, value)).Any();
        }

        // We call it Zipper instead of Zip to avoid naming conflicts with .NET 4
        public static IEnumerable<T> Zipper<A, B, T>(
            this IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func)
        {
            using (var iteratorA = seqA.GetEnumerator())
            using (var iteratorB = seqB.GetEnumerator())
            {
                while (true)
                {
                    bool isDoneA = !iteratorA.MoveNext();
                    bool isDoneB = !iteratorB.MoveNext();

                    Assert.That(isDoneA == isDoneB,
                        "Given collections have different length in Zip operator");

                    if (isDoneA || isDoneB)
                    {
                        break;
                    }

                    yield return func(iteratorA.Current, iteratorB.Current);
                }
            }
        }

        public static IEnumerable<ValuePair<A, B>> Zipper<A, B>(
            this IEnumerable<A> seqA, IEnumerable<B> seqB)
        {
            return seqA.Zipper<A, B, ValuePair<A, B>>(seqB, ValuePair.New);
        }
    }
}