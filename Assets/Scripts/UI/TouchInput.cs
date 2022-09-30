using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;
using UnityEngine.UI;

namespace SanAndreasUnity.UI
{
	
	public class TouchInput : MonoBehaviour
	{

		public static TouchInput Instance { get; private set; }

		public Canvas canvas;
		public GameObject panel;
		public GameObject pedMovementInputGo, vehicleInputGo;
		Button walkButton, sprintButton, jumpButton, crouchButton, enterButton, aimButton, fireButton, flyButton, 
			exitVehicleButton, nextWeaponButton, previousWeaponButton, nextRadioStationButton, previousRadioStationButton;
		UIEventsPickup jumpButtonEventsPickup, fireButtonEventsPickup, handbrakePickup, backwardVehiclePickup, forwardVehiclePickup, panelPickup;
		Text walkButtonText, sprintButtonText, aimButtonText, jumpButtonText, fireButtonText;
		ArrowsMovementButton movementButton, turnVehicleButton;

		bool m_walkPressed, m_sprintPressed, m_aimPressed, m_crouchPressed, m_enterPressed, m_flyPressed, m_exitVehiclePressed,
			m_nextWeaponPressed, m_previousWeaponPressed;

		public Color activeButtonColor = Color.blue;
		public Color inactiveButtonColor = Color.black;

		public float vehicleTurnMultiplier = 1.5f;

		//List<Vector2> m_panelDeltas = new List<Vector2>();
		Vector2 m_panelDeltasSum = Vector2.zero;
		public float touchPointerSensitivity = 1f;



		void Awake ()
		{
			Instance = this;

			// setup references

			Transform parent = pedMovementInputGo.transform;

			walkButton = parent.Find("WalkButton").GetComponent<Button>();
			sprintButton = parent.Find("SprintButton").GetComponent<Button>();
			jumpButton = parent.Find("JumpButton").GetComponent<Button>();
			crouchButton = parent.Find("CrouchButton").GetComponent<Button>();
			enterButton = parent.Find("EnterButton").GetComponent<Button>();
			aimButton = parent.Find("AimButton").GetComponent<Button>();
			fireButton = parent.Find("FireButton").GetComponent<Button>();
			flyButton = parent.Find("FlyButton").GetComponent<Button>();
			nextWeaponButton = parent.Find("NextWeaponButton").GetComponent<Button>();
			previousWeaponButton = parent.Find("PreviousWeaponButton").GetComponent<Button>();
			movementButton = parent.Find("MovementButton").GetComponent<ArrowsMovementButton>();

			parent = vehicleInputGo.transform;
			exitVehicleButton = parent.Find("ExitButton").GetComponent<Button>();
			turnVehicleButton = parent.Find("TurnButton").GetComponent<ArrowsMovementButton>();
            nextRadioStationButton = parent.Find("NextRadioStationButton").GetComponent<Button>();
            previousRadioStationButton = parent.Find("PreviousRadioStationButton").GetComponent<Button>();
            // repeat buttons: handbrake, backward, forward
            handbrakePickup = parent.Find("HandbrakeButton").gameObject.GetOrAddComponent<UIEventsPickup>();
			backwardVehiclePickup = parent.Find("BackwardButton").gameObject.GetOrAddComponent<UIEventsPickup>();
			forwardVehiclePickup = parent.Find("ForwardButton").gameObject.GetOrAddComponent<UIEventsPickup>();

            // repeat buttons: jump, fire
            jumpButtonEventsPickup = jumpButton.gameObject.GetOrAddComponent<UIEventsPickup>();
			fireButtonEventsPickup = fireButton.gameObject.GetOrAddComponent<UIEventsPickup>();

			// panel
			panelPickup = this.panel.GetOrAddComponent<UIEventsPickup>();

			// text components
			walkButtonText = walkButton.GetComponentInChildren<Text>();
			sprintButtonText = sprintButton.GetComponentInChildren<Text>();
			aimButtonText = aimButton.GetComponentInChildren<Text>();
			jumpButtonText = jumpButton.GetComponentInChildren<Text>();
			fireButtonText = fireButton.GetComponentInChildren<Text>();

			// setup event handlers
			// note: for this to work properly, EventSystem.Update() must run before our Update()

			// toggle buttons
			walkButton.onClick.AddListener( () => m_walkPressed = true );
			sprintButton.onClick.AddListener( () => m_sprintPressed = true );
			aimButton.onClick.AddListener( () => m_aimPressed = true );

			// click buttons: crouch, enter, fly, exit vehicle, next weapon, previous weapon
			crouchButton.onClick.AddListener( () => m_crouchPressed = true );
			enterButton.onClick.AddListener( () => m_enterPressed = true );
			flyButton.onClick.AddListener( () => m_flyPressed = true );
			exitVehicleButton.onClick.AddListener( () => m_exitVehiclePressed = true );
			nextWeaponButton.onClick.AddListener( () => m_nextWeaponPressed = true );
			previousWeaponButton.onClick.AddListener( () => m_previousWeaponPressed = true );
            nextRadioStationButton.onClick.AddListener(() => m_nextWeaponPressed = true);
            previousRadioStationButton.onClick.AddListener(() => m_previousWeaponPressed = true);

            // panel
            panelPickup.onDrag += (eventData) => this.OnPanelDrag(eventData);

		}

		void OnLoaderFinished()
		{
			if (F.IsAppInEditMode)
				return;

			// assign textures to movement buttons' arrows

			movementButton.leftArrow.texture = HUD.LeftArrowTexture;
			movementButton.rightArrow.texture = HUD.RightArrowTexture;
			movementButton.upArrow.texture = HUD.DownArrowTexture;
			movementButton.downArrow.texture = HUD.UpArrowTexture;

			turnVehicleButton.leftArrow.texture = HUD.LeftArrowTexture;
			turnVehicleButton.rightArrow.texture = HUD.RightArrowTexture;

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
			this.canvas.enabled = true;

			var customInput = CustomInput.Instance;

			// ignore mouse buttons when touch is enabled
			customInput.SetButton("LeftClick", false);
			if (!customInput.HasButton("RightClick"))
				customInput.SetButton("RightClick", false);
			customInput.SetButtonDown("LeftClick", false);
			customInput.SetButtonDown("RightClick", false);

			Ped ped = Ped.Instance;

			if (ped != null)
			{
				if (ped.IsDrivingVehicle)
				{
					pedMovementInputGo.SetActive(false);
					vehicleInputGo.SetActive(true);

					this.UpdateVehicleMovementInput();
				}
				else
				{
					pedMovementInputGo.SetActive(true);
					vehicleInputGo.SetActive(false);

					this.UpdatePedMovementInput();
				}

				this.UpdateActionsInput();
			}
			else
			{
				pedMovementInputGo.SetActive(false);
				vehicleInputGo.SetActive(false);
			}

			// set mouse move input based on panel drag events

			this.ResetMouseMoveInput();
			customInput.SetAxis("Mouse X", m_panelDeltasSum.x / Screen.width * this.touchPointerSensitivity * GameManager.Instance.cursorSensitivity.x);
			customInput.SetAxis("Mouse Y", m_panelDeltasSum.y / Screen.height * this.touchPointerSensitivity * GameManager.Instance.cursorSensitivity.y);
			// reset deltas sum
			m_panelDeltasSum = Vector2.zero;

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

		void UpdatePedMovementInput()
		{
			// obtain input from arrow button
			this.UpdateMovementInput(movementButton, movementButton.GetMovement());
		}

		void UpdateVehicleMovementInput()
		{
			var customInput = CustomInput.Instance;

			// obtain input from arrow button
			Vector2 input = turnVehicleButton.GetMovementPercentage();
			// ignore y axis
			input.y = 0;
			// now input has only x value between -1 and 1
			// apply multiplier
			input.x *= this.vehicleTurnMultiplier;
			// clamp between -1 and 1
			input.x = Mathf.Clamp(input.x, -1f, 1f);

			this.UpdateMovementInput(turnVehicleButton, input);

			// get status of backward and forward buttons
			bool isBackwardOn = backwardVehiclePickup.IsPointerInside && backwardVehiclePickup.IsPointerDown;
			bool isForwardOn = forwardVehiclePickup.IsPointerInside && forwardVehiclePickup.IsPointerDown;
			if (isBackwardOn || isForwardOn)
				customInput.SetAxis("Vertical", isForwardOn ? 1.0f : -1.0f);

		}

		void UpdateMovementInput(ArrowsMovementButton arrowButton, Vector2 input)
		{
			
			if (arrowButton.IsPointerDown && arrowButton.IsPointerInside)
			{
				// ignore mouse move input while arrow button is pressed
				this.ResetMouseMoveInput();
			}

			this.SetMovementAxesInput(input);

		}

		void ResetMouseMoveInput()
		{
			var customInput = CustomInput.Instance;
			customInput.SetAxis("Mouse X", 0);
			customInput.SetAxis("Mouse Y", 0);
			customInput.SetAxis("Joystick X", 0);
			customInput.SetAxis("Joystick Y", 0);
		}

		void SetMovementAxesInput(Vector2 input)
		{
			var customInput = CustomInput.Instance;
			customInput.SetAxis("Vertical", input.y);
			customInput.SetAxis("Horizontal", input.x);
		}

		void UpdateActionsInput()
		{

			var customInput = CustomInput.Instance;


			// get status of jump & fire repeat butons

			bool isJumpOn = jumpButtonEventsPickup.IsPointerInside && jumpButtonEventsPickup.IsPointerDown;
			bool isFireOn = fireButtonEventsPickup.IsPointerInside && fireButtonEventsPickup.IsPointerDown;

			customInput.SetButton("Jump", isJumpOn);
			customInput.SetButton("LeftClick", isFireOn);

			// get status of handbrake

			bool isHandbrakeOn = handbrakePickup.IsPointerInside && handbrakePickup.IsPointerDown;
			customInput.SetButton("Brake", isHandbrakeOn);

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
			if (m_exitVehiclePressed)
				customInput.SetButtonDown("Use", true);
			if (m_nextWeaponPressed)
				customInput.SetKeyDown (KeyCode.E, true);
			if (m_previousWeaponPressed)
				customInput.SetKeyDown (KeyCode.Q, true);

			m_walkPressed = m_sprintPressed = m_aimPressed = m_crouchPressed = m_enterPressed = m_flyPressed = 
				m_exitVehiclePressed = m_nextWeaponPressed = m_previousWeaponPressed = false;

			// set color of toggle & repeat buttons

			jumpButtonText.color = isJumpOn ? this.activeButtonColor : this.inactiveButtonColor;
			fireButtonText.color = isFireOn ? this.activeButtonColor : this.inactiveButtonColor;

			walkButtonText.color = customInput.GetButton("Walk") ? this.activeButtonColor : this.inactiveButtonColor;
			sprintButtonText.color = customInput.GetButton("Sprint") ? this.activeButtonColor : this.inactiveButtonColor;
			aimButtonText.color = customInput.GetButton("RightClick") ? this.activeButtonColor : this.inactiveButtonColor;

		}

		void OnPanelDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			m_panelDeltasSum += eventData.delta;
		}

	}

}
