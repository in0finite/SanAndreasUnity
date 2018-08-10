using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            ExceptionDispatchInfo.Capture(task.Exception).Throw();
        }
    }

    public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
    {
        while (!task.IsCompleted)
        {
            yield return default(T);
        }

        if (task.IsFaulted)
        {
            ExceptionDispatchInfo.Capture(task.Exception).Throw();
        }

        yield return task.Result;
    }
}
