using System;
using UnityEngine;
using System.Collections;
using CielaSpike;
using System.Threading;

public class ExampleScript : MonoBehaviour {

    void Start()
    {
        StartCoroutine(StartExamples());
    }

    void Update()
    {
        // rotate cube to see if main thread has been blocked;
        transform.Rotate(Vector3.up, Time.deltaTime * 180);
    }

    IEnumerator StartExamples()
    {
        Task task;
        LogExample("Blocking Thread");
        this.StartCoroutineAsync(Blocking(), out task);
        yield return StartCoroutine(task.Wait());
        LogState(task);

        LogExample("Cancellation");
        this.StartCoroutineAsync(Cancellation(), out task);
        yield return new WaitForSeconds(2.0f);
        task.Cancel();
        LogState(task);

        LogExample("Error Handling");
        yield return this.StartCoroutineAsync(ErrorHandling(), out task);
        LogState(task);
    }

    IEnumerator Blocking()
    {
        LogAsync("Thread.Sleep(5000); -> See if cube rotates.");
        Thread.Sleep(5000);
        LogAsync("Jump to main thread.");
        yield return Ninja.JumpToUnity;
        LogSync("Thread.Sleep(5000); -> See if cube rotates.");
        yield return new WaitForSeconds(0.1f);
        Thread.Sleep(5000);
        LogSync("Jump to background.");
        yield return Ninja.JumpBack;
        LogAsync("Yield WaitForSeconds on background.");
        yield return new WaitForSeconds(3.0f);
    }

    IEnumerator Cancellation()
    {
        LogAsync("Running heavy task...");
        for (int i = 0; i < int.MaxValue; i++)
        {
            // do some heavy ops;
            // ...
        }

        yield break;
    }

    IEnumerator ErrorHandling()
    {
        LogAsync("Running heavy task...");
        for (int i = 0; i < int.MaxValue; i++)
        {
            if (i > int.MaxValue / 2)
                throw new Exception("Some error from background thread...");
        }

        yield break;
    }

    private void LogAsync(string msg)
    {
        Debug.Log("[Async]" + msg);
    }

    private void LogState(Task task)
    {
        Debug.Log("[State]" + task.State);
    }

    private void LogSync(string msg)
    {
        Debug.Log("[Sync]" + msg);
    }

    private void LogExample(string msg)
    {
        Debug.Log("[Example]" + msg);
    }
}
