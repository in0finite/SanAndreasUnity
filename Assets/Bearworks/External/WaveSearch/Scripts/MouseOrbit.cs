using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace FastOcean
{
	public class MouseOrbit : MonoBehaviour
	{
	    public Transform target = null;
	    public float distance = 10.0f;
        
	    public float xSpeed = 250.0f;
	    public float ySpeed = 200.0f;
	    public float zSpeed = 1000.0f;
	    public float maxDis = 20.0f;

        private Quaternion rot = Quaternion.identity;
	    void Start () {
            UparamAngles();
	    }


        void UparamAngles()
        {
            if (target != null && target.transform.position != transform.position)
                transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);

            rot = transform.rotation;

            Uparams(rot);
        }

        void Uparams(Quaternion rotation)
	    {
	        Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position;

	        transform.rotation = rotation;
	        transform.position = position;
	    }
        
	    void Update()
        {
            float dx, dy = 0.0f;

	        if (target && Input.GetMouseButton(0)) 
            {
                dx = Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                dy = -Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

                transform.RotateAround(target.position, Vector3.up, dx);
                transform.RotateAround(target.position, transform.right, dy);

                if (target != null && target.transform.position != transform.position)
                    transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);

                if ((!Mathf.Approximately(dx, 0f) || !Mathf.Approximately(dy, 0f)) &&
                    (transform.forward.y > -0.9f) && (transform.forward.y < 0.9f))
                   rot = transform.rotation;
	        }
	        else if (!Mathf.Approximately(Input.GetAxisRaw("Mouse ScrollWheel"), 0))
	        {
	            distance -= Input.GetAxisRaw("Mouse ScrollWheel") * zSpeed * Time.deltaTime;
	            distance = Mathf.Clamp(distance, 2, maxDis);
	        }

            Uparams(rot);

	    }

	}
}