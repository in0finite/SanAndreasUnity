using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SanAndreasUnity.Utilities
{

    public class CustomEventSystemForMixingIMGUIAndNewUI : EventSystem
    {
        bool m_wasAnyElementActiveLastFrame = false;


        protected override void Start()
        {
            base.Start();

            StartCoroutine(this.Coroutine());
        }

        IEnumerator Coroutine()
        {
            while (true)
            {
                yield return null;
                m_wasAnyElementActiveLastFrame = GUIUtility.hotControl != 0;
            }
        }

        protected override void Update()
        {
            if (!m_wasAnyElementActiveLastFrame)
                base.Update();
        }

    }

}
