using UnityEngine;

public class OutOfRangeDestroyer : MonoBehaviour
{
    public float timeUntilDestroyed = 5;
    public float range = 250;
    public Transform targetObject = null;

    private float timeSinceOutOfRange = 0;

    // Use this for initialization
    private void Start()
    {
        if (null == this.targetObject)
        {
            if (Camera.current != null)
                this.targetObject = Camera.current.transform;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        timeSinceOutOfRange += Time.deltaTime;

        if (null == this.targetObject)
        {
            if (Camera.current != null)
                this.targetObject = Camera.current.transform;
        }

        if (null != this.targetObject)
        {
            float distanceSq = (transform.position - targetObject.position).sqrMagnitude;
            if (distanceSq <= range * range)
            {
                timeSinceOutOfRange = 0;
            }
        }

        if (timeSinceOutOfRange >= timeUntilDestroyed)
        {
            Destroy(this.gameObject);
        }
    }
}