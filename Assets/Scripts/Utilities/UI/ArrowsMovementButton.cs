using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SanAndreasUnity.Utilities
{

	public class ArrowsMovementButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		
		public RawImage leftArrow, rightArrow, upArrow, downArrow;

		bool m_isPointerDown = false;
		public bool IsPointerDown => m_isPointerDown;

		public bool IsPointerInside { get; private set; } = false;



		void Awake()
		{
			// setup references

			// leftArrow = this.transform.FindChild("LeftArrow").GetComponent<RawImage>();
			// rightArrow = this.transform.FindChild("RightArrow").GetComponent<RawImage>();
			// upArrow = this.transform.FindChild("UpArrow").GetComponent<RawImage>();
			// downArrow = this.transform.FindChild("DownArrow").GetComponent<RawImage>();

		}

		void OnDisable()
		{
			m_isPointerDown = false;
			this.IsPointerInside = false;
		}

		public void OnPointerDown(PointerEventData pointerEventData)
	    {
	        m_isPointerDown = true;
	    }

	    public void OnPointerUp(PointerEventData pointerEventData)
	    {
	        m_isPointerDown = false;
	    }

	    public void OnPointerEnter(PointerEventData pointerEventData)
	    {
	    	this.IsPointerInside = true;
	    }

	    public void OnPointerExit(PointerEventData pointerEventData)
	    {
	    	this.IsPointerInside = false;
	    }

	    public Vector2 GetMovement()
	    {
	    	if (!m_isPointerDown || !this.IsPointerInside)
	    		return Vector2.zero;
	    	Vector2 mousePos = Input.mousePosition;
	    	Vector2 localPoint = Vector2.zero;
	    	if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, mousePos, null, out localPoint))
	    		return Vector2.zero;
	    	Vector2 diff = localPoint;
	    	if (diff.sqrMagnitude < float.Epsilon)
	    		return Vector2.zero;
	    	return diff.normalized;
	    }


	}

}
