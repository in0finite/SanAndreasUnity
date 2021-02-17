using SanAndreasUnity.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace SanAndreasUnity.AR
{
    public class ARManager : MonoBehaviour
    {
        private Text _arStatusText;

        private void Awake()
        {
            _arStatusText = GameObject.Find("AR status text").GetComponentOrThrow<Text>();
        }

        void Update()
        {
            _arStatusText.text = "AR status: " + ARSession.state;
        }

    }
}
