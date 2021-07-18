using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class AddFocusPointWhenLoaderFinishes : MonoBehaviour
    {
        public FocusPoint.Parameters parameters = FocusPoint.Parameters.Default;


        private void OnLoaderFinished()
        {
            FocusPoint.Create(this.gameObject, this.parameters);
        }
    }
}
