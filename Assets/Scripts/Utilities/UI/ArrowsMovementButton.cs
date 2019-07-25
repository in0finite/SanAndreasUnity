using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SanAndreasUnity.Utilities
{

	public class ArrowsMovementButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
	{
		
		public RawImage leftArrow, rightArrow, upArrow, downArrow;

		bool m_isPointerDown = false;
		public bool IsPointerDown => m_isPointerDown;

		public bool IsPointerInside { get; private set; } = false;

		public Vector2 LastPointerPos { get; private set; } = Vector2.zero;



		void OnDisable()
		{
			m_isPointerDown = false;
			this.IsPointerInside = false;
		}

		public void OnPointerDown(PointerEventData pointerEventData)
	    {
	        m_isPointerDown = true;
	        this.LastPointerPos = pointerEventData.position;
	    }

	    public void OnPointerUp(PointerEventData pointerEventData)
	    {
	        m_isPointerDown = false;
	    }

	    public void OnPointerEnter(PointerEventData pointerEventData)
	    {
	    	this.IsPointerInside = true;
	    	this.LastPointerPos = pointerEventData.position;
	    }

	    public void OnPointerExit(PointerEventData pointerEventData)
	    {
	    	this.IsPointerInside = false;
	    }

	    public void OnDrag (PointerEventData pointerEventData)
	    {
	    	this.LastPointerPos = pointerEventData.position;
	    }


	    public Vector2 GetMovementNonNormalized()
	    {
	    	if (!m_isPointerDown || !this.IsPointerInside)
	    		return Vector2.zero;
	    	Vector2 pointerPos = this.LastPointerPos;
	    	Vector2 localPoint = Vector2.zero;
	    	if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, pointerPos, null, out localPoint))
	    		return Vector2.zero;
	    	Vector2 diff = localPoint;
	    	return diff;
	    }

	    public Vector2 GetMovement()
	    {
	    	return this.GetMovementNonNormalized().normalized;
	    }

	    public Vector2 GetMovementPercentage()
	    {
	    	Rect rect = (this.transform as RectTransform).rect;
	    	float width = rect.width;
	    	float height = rect.height;
	    	if (width < float.Epsilon || height < float.Epsilon)
	    		return Vector2.zero;

	    	Vector2 diff = this.GetMovementNonNormalized();
	    	float xPerc = diff.x / (width * 0.5f);
	    	float yPerc = diff.y / (height * 0.5f);
	    	xPerc = Mathf.Clamp(xPerc, -1f, 1f);
	    	yPerc = Mathf.Clamp(yPerc, -1f, 1f);
	    	return new Vector2(xPerc, yPerc);
	    }


	}

}
