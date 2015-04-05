using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle : MonoBehaviour
    {
        private void Update()
        {
            foreach (var wheel in _wheels) {
                wheel.transform.Rotate(Vector3.left, Time.deltaTime * 500.0f);
            }
        }
    }
}
