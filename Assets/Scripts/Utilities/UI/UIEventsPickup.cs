using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SanAndreasUnity.Utilities
{

	public class UIEventsPickup : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
		IPointerUpHandler
	{

		public	event	Action<PointerEventData>	onPointerClick = delegate {};
		public	event	Action<PointerEventData>	onPointerEnter = delegate {};
		public	event	Action<PointerEventData>	onPointerExit = delegate {};
		public	event	Action<PointerEventData>	onPointerDown = delegate {};
		public	event	Action<PointerEventData>	onPointerUp = delegate {};

		public bool IsPointerInside { get; private set; } = false;
		public bool IsPointerDown { get; private set; } = false;



		void OnDisable()
		{
			this.IsPointerInside = false;
			this.IsPointerDown = false;
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			onPointerClick (eventData);
		}

		public void OnPointerEnter (PointerEventData eventData)
		{
			this.IsPointerInside = true;
			onPointerEnter (eventData);
		}

		public void OnPointerExit (PointerEventData eventData)
		{
			this.IsPointerInside = false;
			onPointerExit (eventData);
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			this.IsPointerDown = true;
			onPointerDown (eventData);
		}

		public void OnPointerUp (PointerEventData eventData)
		{
			this.IsPointerDown = false;
			onPointerUp (eventData);
		}


	}

}
