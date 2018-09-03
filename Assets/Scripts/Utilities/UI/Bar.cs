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

		[SerializeField] private Vector3 m_barSize = new Vector3 (1f, 0.2f, 1f);
		public Vector3 BarSize { get { return m_barSize; } set { m_barSize = value; } }

		[SerializeField] private float m_maxHeightOnScreen = 10f;
		public float MaxHeightOnScreen { get { return m_maxHeightOnScreen; } set { m_maxHeightOnScreen = value; } }

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
			Camera cam = Camera.main;

			// update size
			this.transform.SetGlobalScale (this.BarSize);

			if (cam)
			{
				if (this.faceTowardsCamera)
				{
					// make rotation same as camera's rotation
					this.transform.rotation = cam.transform.rotation;

					if (this.MaxHeightOnScreen > 0)
					{
						// limit height on screen

						// get current height on screen

						Vector3 top = this.transform.position + this.transform.up * this.transform.lossyScale.y * 0.5f;
						Vector3 bottom = this.transform.position - this.transform.up * this.transform.lossyScale.y * 0.5f;

						Vector3 screenTop = cam.WorldToScreenPoint( top );
						Vector3 screenBottom = cam.WorldToScreenPoint( bottom );

						if (screenTop.z >= 0 && screenBottom.z >= 0)
						{
							float heightOnScreen = Mathf.Abs( screenTop.y - screenBottom.y );
							if (heightOnScreen > this.MaxHeightOnScreen)
							{
								// reduce height of bar

								float ratio = this.MaxHeightOnScreen / heightOnScreen;

								Vector3 newSize = this.transform.lossyScale;
								newSize.y *= ratio;
								this.transform.SetGlobalScale( newSize );

							}
						}

					}
				}

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
