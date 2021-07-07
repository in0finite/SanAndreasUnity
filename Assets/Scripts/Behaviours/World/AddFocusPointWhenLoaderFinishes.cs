using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class AddFocusPointWhenLoaderFinishes : MonoBehaviour
    {
        public bool hasRevealRadius = true;
        public float revealRadius = 50f;


        private void OnLoaderFinished()
        {
            var focusPoint = this.gameObject.AddComponent<FocusPoint>();
            focusPoint.hasRevealRadius = this.hasRevealRadius;
            focusPoint.revealRadius = this.revealRadius;
        }
    }
}
