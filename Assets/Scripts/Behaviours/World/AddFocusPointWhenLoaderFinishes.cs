using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class AddFocusPointWhenLoaderFinishes : MonoBehaviour
    {
        public bool hasRevealRadius = true;
        public float revealRadius = 50f;


        private void OnLoaderFinished()
        {
            FocusPoint.Create(this.gameObject, this.hasRevealRadius, this.revealRadius);
        }
    }
}
