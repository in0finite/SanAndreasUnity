using UnityEngine;

namespace SanAndreasUnity.UI
{
    /// <summary>
    /// This script should have lowest possible script execution order so that the <see cref="Console"/> is subscribed to
    /// log events before any other script executes.
    /// </summary>
    public class ConsoleLogEventSubscriber : MonoBehaviour
    {
        void Awake()
        {
            Console.OnEventSubscriberAwake();
        }
    }
}
