using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class Radio : MonoBehaviour
    {
        private Vehicle vehicle;

        public void StartRadio(Vehicle vehicle)
        {
            this.vehicle = vehicle;
            InvokeRepeating("RadioUpdate", 0f, 1f);
        }

        public void StopRadio()
        {
            CancelInvoke();
            vehicle.StopRadio();
        }

        private void RadioUpdate()
        {
            vehicle.RadioUpdate();
        }
    }
}
