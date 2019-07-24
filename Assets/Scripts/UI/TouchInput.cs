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
		UIEventsPickup jumpButtonEventsPickup, fireButtonEventsPickup;
		Text walkButtonText, sprintButtonText, aimButtonText, jumpButtonText, fireButtonText;
		ArrowsMovementButton movementButton;

		bool m_walkPressed, m_sprintPressed, m_aimPressed, m_crouchPressed, m_enterPressed, m_flyPressed;

		public Color activeButtonColor = Color.blue;
		public Color inactiveButtonColor = Color.black;



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

			// repeat buttons: jump, fire
			jumpButtonEventsPickup = jumpButton.gameObject.GetOrAddComponent<UIEventsPickup>();
			fireButtonEventsPickup = fireButton.gameObject.GetOrAddComponent<UIEventsPickup>();

			// text components
			walkButtonText = walkButton.GetComponentInChildren<Text>();
			sprintButtonText = sprintButton.GetComponentInChildren<Text>();
			aimButtonText = aimButton.GetComponentInChildren<Text>();
			jumpButtonText = jumpButton.GetComponentInChildren<Text>();
			fireButtonText = fireButton.GetComponentInChildren<Text>();

			// setup button event handlers
			// note: for this to work properly, EventSystem.Update() must run before our Update()

			// toggle buttons
			walkButton.onClick.AddListener( () => m_walkPressed = true );
			sprintButton.onClick.AddListener( () => m_sprintPressed = true );
			aimButton.onClick.AddListener( () => m_aimPressed = true );

			// click buttons: crouch, enter, fly
			crouchButton.onClick.AddListener( () => m_crouchPressed = true );
			enterButton.onClick.AddListener( () => m_enterPressed = true );
			flyButton.onClick.AddListener( () => m_flyPressed = true );

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
			this.UpdateActionsInput();


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

			// preserve input for toggle buttons: walk, sprint, aim

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

		void UpdateActionsInput()
		{

			var customInput = CustomInput.Instance;
			// Ped ped = Ped.Instance;
			// bool pedExists = ped != null;


			// get status of jump & fire repeat butons

			bool isJumpOn = jumpButtonEventsPickup.IsPointerInside && jumpButtonEventsPickup.IsPointerDown;
			bool isFireOn = fireButtonEventsPickup.IsPointerInside && fireButtonEventsPickup.IsPointerDown;

			customInput.SetButton("Jump", isJumpOn);
			customInput.SetButton("LeftClick", isFireOn);

			// process click events

			if (m_walkPressed)
				customInput.SetButton("Walk", ! customInput.GetButton("Walk"));
			if (m_sprintPressed)
				customInput.SetButton("Sprint", ! customInput.GetButton("Sprint"));
			if (m_aimPressed)
				customInput.SetButton("RightClick", ! customInput.GetButton("RightClick"));
			if (m_crouchPressed)
				customInput.SetKeyDown(KeyCode.C, true);
			if (m_enterPressed)
				customInput.SetButtonDown("Use", true);
			if (m_flyPressed)
				customInput.SetKeyDown(KeyCode.T, true);

			m_walkPressed = m_sprintPressed = m_aimPressed = m_crouchPressed = m_enterPressed = m_flyPressed = false;

			// set color of toggle & repeat buttons

			jumpButtonText.color = isJumpOn ? this.activeButtonColor : this.inactiveButtonColor;
			fireButtonText.color = isFireOn ? this.activeButtonColor : this.inactiveButtonColor;

			walkButtonText.color = customInput.GetButton("Walk") ? this.activeButtonColor : this.inactiveButtonColor;
			sprintButtonText.color = customInput.GetButton("Sprint") ? this.activeButtonColor : this.inactiveButtonColor;
			aimButtonText.color = customInput.GetButton("RightClick") ? this.activeButtonColor : this.inactiveButtonColor;

		}

	}

}
