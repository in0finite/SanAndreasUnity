using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using UGameCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
	[RequireComponent(typeof(Ped))]
    public class PlayerController : MonoBehaviour
    {
        
		public static PlayerController Instance { get { return Ped.Instance != null ? Ped.Instance.GetComponent<PlayerController>() : null; } }

		private Ped m_ped;

        // Alpha speedometer
        private const float velTimer = 1 / 4f;

		private float m_velCounter = velTimer;

		private Vector3 m_lastPos = Vector3.zero,
			m_deltaPos = Vector3.zero;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse = Vector2.zero;
        

        public Vector2 clampInDegrees = new Vector2(90, 60);

		public float EnterVehicleRadius { get { return m_ped.EnterVehicleRadius; } }

        public Vector2 smoothing = new Vector2(10, 10);
		[SerializeField] private bool m_doSmooth = true;

		[SerializeField] private bool m_smoothMovement = false;

        public float CurVelocity { get { return m_deltaPos.magnitude * 3.6f / velTimer; } }



        private void Awake()
        {
            m_ped = GetComponent<Ped>();

        }

        private void FixedUpdate()
        {
            m_velCounter -= Time.deltaTime;
            if (m_velCounter <= 0)
            {
                Vector3 t = new Vector3(transform.position.x, 0, transform.position.z);

                m_deltaPos = t - m_lastPos;
                m_lastPos = t;

                m_velCounter = velTimer;
            }
        }

        private void Update()
        {
			if (!m_ped.IsControlledByLocalPlayer)
				return;

            if (!Loader.HasLoaded)
                return;


			// reset ped input
			m_ped.ResetMovementInput ();
			m_ped.MouseMoveInput = Vector2.zero;
			m_ped.MouseScrollInput = Vector2.zero;
			

			if (!GameManager.CanPlayerReadInput()) return;


			// states must be read before events, otherwise callback functions for events will not have access
			// to states (they will always be unpressed/reset, because we did a reset above)
			this.ReadStates ();
			this.ReadEvents ();


        }

		void ReadStates()
		{

			CustomInput customInput = CustomInput.Instance;

			this.ReadCameraInput ();

			m_ped.MouseScrollInput = Input.mouseScrollDelta;


			m_ped.IsAimOn = customInput.GetButton ("RightClick");
			m_ped.IsFireOn = customInput.GetButton ("LeftClick");

			m_ped.IsJumpOn = customInput.GetButton ("Jump");


			Vector3 inputMove = Vector3.zero;
			if (m_smoothMovement)
				inputMove = new Vector3 (customInput.GetAxis ("Horizontal"), 0f, customInput.GetAxis ("Vertical"));
			else
				inputMove = new Vector3 (customInput.GetAxisRaw ("Horizontal"), 0f, customInput.GetAxisRaw ("Vertical"));

			if (inputMove.sqrMagnitude > 0f)
			{
				inputMove.Normalize();

				if (customInput.GetButton ("Walk"))
					m_ped.IsWalkOn = true;
				else if (customInput.GetButton ("Sprint"))
					m_ped.IsSprintOn = true;
				else
					m_ped.IsRunOn = true;

			}

			if (m_ped.Camera != null)
				m_ped.Movement = m_ped.Camera.transform.TransformVector (inputMove).normalized;
			else
				m_ped.Movement = inputMove.normalized;

			if (m_ped.Movement.sqrMagnitude > float.Epsilon) {
				// only assign heading if there is any movement - we don't want the heading to be zero vector
				m_ped.Heading = m_ped.Movement;
			}

		}

		void ReadEvents()
		{

			CustomInput customInput = CustomInput.Instance;

			if (customInput.GetButtonDown ("LeftClick"))
				m_ped.OnFireButtonPressed ();

			if (customInput.GetButtonDown ("RightClick"))
				m_ped.OnAimButtonPressed ();

			if (customInput.GetKeyDown (KeyCode.Q))
				m_ped.OnPreviousWeaponButtonPressed();
			else if (customInput.GetKeyDown (KeyCode.E))
				m_ped.OnNextWeaponButtonPressed();

			if (customInput.GetButtonDown("Use"))
				m_ped.OnSubmitPressed ();

			if (customInput.GetButtonDown("Jump"))
				m_ped.OnJumpButtonPressed ();

			if (customInput.GetKeyDown(KeyCode.C))
				m_ped.OnCrouchButtonPressed ();

			if (customInput.GetKeyDown(KeyCode.G))
				m_ped.OnButtonPressed ("G");

			if (customInput.GetKeyDown(KeyCode.H))
				m_ped.OnButtonPressed ("H");

			if (customInput.GetKeyDown (KeyCode.T))
				m_ped.OnFlyButtonPressed();

			if (customInput.GetKeyDown (KeyCode.R))
				m_ped.OnFlyThroughButtonPressed();

		}

		private void ReadCameraInput ()
		{

			CustomInput customInput = CustomInput.Instance;

			if (GameManager.CanPlayerReadInput())
			{
				// rotate camera

				// FIXME: Camera rotation should be done by ped's current state. We should only assign mouse move input.

				var mouseDelta = new Vector2(customInput.GetAxisRaw("Mouse X"), customInput.GetAxisRaw("Mouse Y"));
                var rightAnalogDelta = new Vector2(customInput.GetAxisRaw("Joystick X"), customInput.GetAxisRaw("Joystick Y"));

				Vector2 totalMouseDelta = mouseDelta + rightAnalogDelta;

				totalMouseDelta = Vector2.Scale (totalMouseDelta, GameManager.Instance.cursorSensitivity);

				m_ped.MouseMoveInput = totalMouseDelta;


				if (m_doSmooth)
				{
					
					_smoothMouse.x = Mathf.Lerp (_smoothMouse.x, totalMouseDelta.x, 1f / smoothing.x);
					_smoothMouse.y = Mathf.Lerp (_smoothMouse.y, totalMouseDelta.y, 1f / smoothing.y);

					_mouseAbsolute += _smoothMouse;
				}
				else
				{
					_mouseAbsolute += totalMouseDelta;
				}


				if (clampInDegrees.y > 0)
					_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y, clampInDegrees.y);
				
			}


		}


        private void OnDrawGizmosSelected()
        {
			if (null == m_ped)
				return;

            Gizmos.color = Color.white;

			// draw enter vehicle radius
            Gizmos.DrawWireSphere(transform.position, EnterVehicleRadius);

			// find closest vehicle in entering range

            var vehicles = FindObjectsOfType<Vehicle>()
                .Where(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position) < EnterVehicleRadius)
                .OrderBy(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position)).ToArray();

            foreach (var vehicle in vehicles)
            {
				// draw all seats
                foreach (var seat in vehicle.Seats)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(seat.Parent.position, 0.1f);
                }

				// draw closest seat

                var closestSeat = vehicle.GetSeatAlignmentOfClosestSeat(transform.position);

                if (closestSeat != Vehicle.SeatAlignment.None)
                {
                    var closestSeatTransform = vehicle.GetSeatTransform(closestSeat);

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(closestSeatTransform.position, 0.1f);
                    Gizmos.DrawLine(transform.position, closestSeatTransform.position);
                }

                break;
            }
        }

    }
}