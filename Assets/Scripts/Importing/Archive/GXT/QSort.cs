using System;
using System.Collections.Generic;

internal class QSort<T> where T : IComparable
{
    public IList<T> A;

    public QSort(IList<T> A)
    {
        this.A = A;
    }

    public int Partition(int L, int U)
    {
        int s = U;
        int p = L;
        while (s != p)
        {
            if (A[p].CompareTo(A[s]) <= 0)
            {
                p++;
            }
            else
            {
                Swap(p, s);
                Swap(p, s - 1);
                s--;
            }
        }
        return p;
    }

    private void Swap(int p, int s)
    {
        T tmp = A[p];
        A[p] = A[s];
        A[s] = tmp;
    }

    public void Sort(int L, int U)
    {
        if (L >= U) return;
        int p = Partition(L, U);
        Sort(L, p - 1);
        Sort(p + 1, U);
    }

    public void Sort()
    {
        Sort(0, A.Count - 1);
    }
}