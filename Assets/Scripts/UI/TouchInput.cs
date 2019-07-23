using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;

namespace SanAndreasUnity.UI
{
	
	public class TouchInput : MonoBehaviour
	{

		public static TouchInput Instance { get; private set; }

		Canvas canvas;
		GameObject pedMovementInputGo;
		Button walkButton, sprintButton, jumpButton, crouchButton, enterButton, aimButton, fireButton, flyButton;
		ArrowsMovementButton movementButton;



		void Awake ()
		{
			Instance = this;

			// setup references

			canvas = this.transform.GetChild(0).GetComponent<Canvas>();
			pedMovementInputGo = canvas.transform.GetChild(0).gameObject;
			Transform parent = pedMovementInputGo.transform;

			walkButton = parent.Find("WalkButton").GetComponent<Button>();
			sprintButton = parent.Find("SprintButton").GetComponent<Button>();
			jumpButton = parent.Find("JumpButton").GetComponent<Button>();
			crouchButton = parent.Find("CrouchButton").GetComponent<Button>();
			enterButton = parent.Find("EnterButton").GetComponent<Button>();
			aimButton = parent.Find("AimButton").GetComponent<Button>();
			fireButton = parent.Find("FireButton").GetComponent<Button>();
			flyButton = parent.Find("FlyButton").GetComponent<Button>();
			movementButton = parent.Find("MovementButton").GetComponent<ArrowsMovementButton>();

		}

		void OnLoaderFinished()
		{
			// assign textures to movement button's arrows
			movementButton.leftArrow.texture = HUD.LeftArrowTexture;
			movementButton.rightArrow.texture = HUD.RightArrowTexture;
			movementButton.upArrow.texture = HUD.DownArrowTexture;
			movementButton.downArrow.texture = HUD.UpArrowTexture;
		}

		void Update()
		{

			this.ResetCustomInput();

			if (!UIManager.Instance.UseTouchInput || !GameManager.CanPlayerReadInput())
			{
				// we are not using touch input, or we should not read input right now
				this.canvas.gameObject.SetActive(false);
				return;
			}

			this.canvas.gameObject.SetActive(true);

			// ignore mouse buttons when touch is enabled
			CustomInput.Instance.SetButton("LeftClick", false);
			if (!CustomInput.Instance.HasButton("RightClick"))
				CustomInput.Instance.SetButton("RightClick", false);
			CustomInput.Instance.SetButtonDown("LeftClick", false);
			CustomInput.Instance.SetButtonDown("RightClick", false);

			this.UpdateMovementInput();


		}

		void ResetCustomInput()
		{
			var customInput = CustomInput.Instance;

			if (!UIManager.Instance.UseTouchInput)
			{
				// touch input is not used
				customInput.ResetAllInput();
				return;
			}

			// preserve input for: walk, sprint, aim

			bool isWalkOn = customInput.GetButtonNoDefaultInput("Walk");
			bool isSprintOn = customInput.GetButtonNoDefaultInput("Sprint");
			bool isAimOn = customInput.GetButtonNoDefaultInput("RightClick");

			customInput.ResetAllInput();

			customInput.SetButton("Walk", isWalkOn);
			customInput.SetButton("Sprint", isSprintOn);
			customInput.SetButton("RightClick", isAimOn);
		}

		void UpdateMovementInput()
		{

			var customInput = CustomInput.Instance;

			// obtain input from movement button

			Vector2 input = Vector2.zero;

			if (movementButton.IsPointerDown && movementButton.IsPointerInside)
			{
				input = movementButton.GetMovement();
				Debug.LogFormat("pointer is down, input: {0}", input);
			}

			// set input for vertical and horizontal axis
			customInput.SetAxis("Vertical", input.y);
			customInput.SetAxis("Horizontal", input.x);

		}

	}

}
