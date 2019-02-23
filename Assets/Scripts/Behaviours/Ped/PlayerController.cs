using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	[DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Ped))]
    public class PlayerController : MonoBehaviour
    {
        
		public	static	PlayerController	Instance { get ; private set ; }

		private Ped m_ped;

		public static bool _showVel = true;

        // Alpha speedometer
        private const float velTimer = 1 / 4f;

        private static float velCounter = velTimer;

        private static Vector3 lastPos = Vector3.zero,
                               deltaPos = Vector3.zero;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse = Vector2.zero;
        
        
        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        //public Vector2 PitchClamp = new Vector2(-89f, 89f);
        public Vector2 clampInDegrees = new Vector2(90, 60);

		public float EnterVehicleRadius { get { return m_ped.EnterVehicleRadius; } }

        public Vector2 smoothing = new Vector2(10, 10);
        public bool m_doSmooth = true;

		[SerializeField] private bool m_smoothMovement = false;


        public float CurVelocity
        {
            get
            {
                return deltaPos.magnitude * 3.6f / velTimer;
            }
        }

        public Camera Camera { get { return m_ped.Camera; } }
        public PedModel PlayerModel { get { return m_ped.PlayerModel; } }



        private void Awake()
        {
            Instance = this;
            m_ped = GetComponent<Ped>();

        }


        private void OnGUI()
        {
			if (!Loader.HasLoaded)
				return;
			
            
            // show that we are in flying state
			if (m_ped.CurrentState is Peds.States.FlyState)
            {
                int height = 25;
                GUILayout.BeginArea(new Rect(Screen.width - 140, Screen.height - height, 140, height));
				GUILayout.Label("Flying-mode enabled!");
				GUILayout.EndArea();
            }

            if (_showVel)
                GUI.Label(GUIUtils.GetCornerRect(ScreenCorner.TopLeft, 100, 25, new Vector2(5, 5)), string.Format("{0:0.0} km/h", deltaPos.magnitude * 3.6f / velTimer), new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });

			// show current ped state
			GUI.Label (GUIUtils.GetCornerRect(ScreenCorner.BottomLeft, 250, 50), string.Format("Current ped state: {0}", m_ped.CurrentState != null ? m_ped.CurrentState.GetType().Name : "none") );
            
        }

        private void FixedUpdate()
        {
            velCounter -= Time.deltaTime;
            if (velCounter <= 0)
            {
                Vector3 t = new Vector3(transform.position.x, 0, transform.position.z);

                deltaPos = t - lastPos;
                lastPos = t;

                velCounter = velTimer;
            }
        }

        private void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.F9))
                _showVel = !_showVel;

            if (!Loader.HasLoaded)
                return;


			// reset player input
			m_ped.ResetMovementInput ();
			m_ped.MouseMoveInput = Vector2.zero;
			m_ped.MouseScrollInput = Vector2.zero;
			

			this.ReadCameraInput ();


			if (!GameManager.CanPlayerReadInput()) return;


			if (Input.GetKeyDown (KeyCode.Q))
				m_ped.OnPreviousWeaponButtonPressed();
			else if (Input.GetKeyDown (KeyCode.E))
				m_ped.OnNextWeaponButtonPressed();
			
            if (Input.GetButtonDown("Use"))
            	m_ped.OnSubmitPressed ();
            
			if (Input.GetKeyDown (KeyCode.T))
				m_ped.OnFlyButtonPressed();
			
			if (Input.GetKeyDown (KeyCode.R))
				m_ped.OnFlyThroughButtonPressed();

			m_ped.MouseScrollInput = Input.mouseScrollDelta;

            
			if (m_ped.IsInVehicle) return;


			m_ped.IsAimOn = Input.GetButton ("RightClick");
			m_ped.IsFireOn = Input.GetButton ("LeftClick");

            
			m_ped.IsJumpOn = Input.GetButton ("Jump");

			Vector3 inputMove = Vector3.zero;
			if (m_smoothMovement)
				inputMove = new Vector3 (Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));
			else
				inputMove = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0f, Input.GetAxisRaw ("Vertical"));

            if (inputMove.sqrMagnitude > 0f)
            {
                inputMove.Normalize();

				if (Input.GetButton ("Walk"))
					m_ped.IsWalkOn = true;
				else if (Input.GetButton ("Sprint"))
					m_ped.IsSprintOn = true;
				else
					m_ped.IsRunOn = true;

            }
           	
            m_ped.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                new Vector3(1f, 1f, 1f)).normalized;

			// player heading should be assigned here, not in Player class
		//	if (!_player.IsAiming)
			{
				if (m_ped.Movement.sqrMagnitude > float.Epsilon) {
					m_ped.Heading = m_ped.Movement;
				}
			}


        }

		private void ReadCameraInput ()
		{

			if (GameManager.CanPlayerReadInput())
			{
				// rotate camera

				var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                var rightAnalogDelta = new Vector2(Input.GetAxisRaw("Joystick X"), Input.GetAxisRaw("Joystick Y"));

				Vector2 totalMouseDelta = mouseDelta + rightAnalogDelta;

				totalMouseDelta = Vector2.Scale (totalMouseDelta, this.CursorSensitivity);

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


				/*if (clampInDegrees.x > 0)
                    _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x, clampInDegrees.x);*/

				if (clampInDegrees.y > 0)
					_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y, clampInDegrees.y);


//				Vector3 eulers = Camera.transform.eulerAngles;
//				eulers.x += - mouseDelta.y;
//				eulers.y += mouseDelta.x;
//
//				// no rotation around z axis
//				eulers.z = 0;
//
//				// clamp rotation
//				if(eulers.x > 180)
//					eulers.x -= 360;
//				eulers.x = Mathf.Clamp (eulers.x, -clampInDegrees.x, clampInDegrees.x);
//
//				// apply new rotation
//				Camera.transform.eulerAngles = eulers;


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

                var closestSeat = vehicle.FindClosestSeat(transform.position);

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

        /*public static float ClampAngle(float currentValue, float minAngle, float maxAngle, float clampAroundAngle = 0)
        {
            float angle = currentValue - (clampAroundAngle + 180);

            while (angle < 0)
            {
                angle += 360;
            }

            angle = Mathf.Repeat(angle, 360);

            return Mathf.Clamp(
                angle - 180,
                minAngle,
                maxAngle
            ) + 360 + clampAroundAngle;
        }*/

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}