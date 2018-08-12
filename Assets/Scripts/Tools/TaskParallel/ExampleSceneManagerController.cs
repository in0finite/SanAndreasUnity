using UnityEngine;
using CI.TaskParallel;
using UnityEngine.UI;

public class ExampleSceneManagerController : MonoBehaviour
{
    public Text ResultText;

    public void Start()
    {
        UnityTask.InitialiseDispatcher();
    }

    public void CreateAndRun()
    {
        UnityTask.Run(() =>
        {
            var result = FindPrimeNumber(1000);
        });
    }

    public void WaitAndReturnAValue()
    {
        UnityTask<long> unityTask = UnityTask.Run(() =>
        {
            return FindPrimeNumber(10000);
        });

        UnityTask.WaitAll(unityTask);

        long result = unityTask.Result;
    }

    public void ReturnAValueToTheUIThread()
    {
        UnityTask.Run(() =>
        {
            return FindPrimeNumber(10000);
        }).ContinueOnUIThread((r) =>
        {
            ResultText.text = "The result is: " + r.Result.ToString();
        });
    }

    public void CreateContinuation()
    {
        UnityTask<long> unityTask = UnityTask.Run(() =>
        {
            return FindPrimeNumber(1000);
        }).ContinueWith((r) =>
        {
            return FindPrimeNumber((int)r.Result);
        });
    }

    public long FindPrimeNumber(int n)
    {
        int count = 0;
        long a = 2;
        while (count < n)
        {
            long b = 2;
            int prime = 1;
            while (b * b <= a)
            {
                if (a % b == 0)
                {
                    prime = 0;
                    break;
                }
                b++;
            }
            if (prime > 0)
            {
                count++;
            }
            a++;
        }
        return (--a);
    }
}