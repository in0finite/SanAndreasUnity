using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class AddFocusPointWhenLoaderFinishes : MonoBehaviour
    {
        public FocusPointParameters parameters = FocusPointParameters.Default;


        private void OnLoaderFinished()
        {
            FocusPoint.Create(this.gameObject, this.parameters);
        }
    }
}
