using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
	
	public class Bar : MonoBehaviour {

		public Transform Border { get; private set; }
		public Transform Background { get; private set; }
		public Transform Fill { get; private set; }

		public Renderer BorderRenderer { get; private set; }
		public Renderer BackgroundRenderer { get; private set; }
		public Renderer FillRenderer { get; private set; }

		public Color BorderColor { get { return this.BorderRenderer.material.color; } set { this.BorderRenderer.material.color = value; } }
		public Color BackgroundColor { get { return this.BackgroundRenderer.material.color; } set { this.BackgroundRenderer.material.color = value; } }
		public Color FillColor { get { return this.FillRenderer.material.color; } set { this.FillRenderer.material.color = value; } }

		public Vector3 BarSize { get { return this.transform.lossyScale; } set { this.transform.SetGlobalScale( value ); } }

		public bool faceTowardsCamera = true;



		void Awake ()
		{
			this.Border = this.transform.Find ("Border");
			this.Background = this.transform.Find ("Background");
			this.Fill = this.transform.Find ("Fill");

			this.BorderRenderer = this.Border.GetComponent<Renderer> ();
			this.BackgroundRenderer = this.Background.GetComponent<Renderer> ();
			this.FillRenderer = this.Fill.GetComponent<Renderer> ();
		}

		void Update ()
		{

			if (this.faceTowardsCamera && Camera.main != null) {
				this.transform.rotation = Camera.main.transform.rotation;
			}

		}

		public void SetFillPerc (float fillPerc)
		{
			fillPerc = Mathf.Clamp01 (fillPerc);

			Vector3 scale = this.Fill.localScale;
			scale.x = fillPerc;
			this.Fill.localScale = scale;

			// reposition it
			Vector3 pos = this.Fill.localPosition;
			pos.x = - (1.0f - fillPerc) / 2.0f;
			this.Fill.localPosition = pos;
		}

		/*
		public void SetBorderWidth (float borderWidth)
		{
		//	borderWidthPerc = Mathf.Clamp (borderWidthPerc, 0f, 0.5f);

			// stretch border to parent
			this.Border.localScale = Vector3.one;

			// reduce width and height of background and fill objects

			Vector3 size = this.BarSize;
			size.x -= borderWidth * 2;
			size.y -= borderWidth * 2;

			this.Background.SetGlobalScale( size );
			this.Fill.SetGlobalScale( size );
		}
		*/

	}

}
