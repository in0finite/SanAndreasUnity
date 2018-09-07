using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(Ped))]
    public class PlayerController : MonoBehaviour
    {
        
		public	static	PlayerController	Instance { get ; private set ; }

		private Ped m_ped;

        private float _pitch;
        private float _yaw;

		public static bool _showVel = true;

        // Alpha speedometer
        private const float velTimer = 1 / 4f;

        private static float velCounter = velTimer;

        private static Vector3 lastPos = Vector3.zero,
                               deltaPos = Vector3.zero;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse = Vector2.zero;
        private Vector3 targetDirection = Vector3.forward;

        
        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 3.0f;

        //public Vector2 PitchClamp = new Vector2(-89f, 89f);
        public Vector2 clampInDegrees = new Vector2(90, 60);

		public float EnterVehicleRadius { get { return m_ped.EnterVehicleRadius; } }

        public Vector2 smoothing = new Vector2(10, 10);
        public bool m_doSmooth = true;

		[SerializeField] private bool m_smoothMovement = false;

		[SerializeField] private KeyCode m_walkKey = KeyCode.LeftAlt;
		[SerializeField] private KeyCode m_sprintKey = KeyCode.Space;
		[SerializeField] private KeyCode m_jumpKey = KeyCode.LeftShift;


        public float CurVelocity
        {
            get
            {
                return deltaPos.magnitude * 3.6f / velTimer;
            }
        }

		public Vector3 CameraFocusPos { get { return m_ped.transform.position + Vector3.up * 0.5f; } }
		public Vector3 CameraFocusPosVehicle { get { return m_ped.CurrentVehicle.transform.position; } }

        public Camera Camera { get { return m_ped.Camera; } }
        public PedModel PlayerModel { get { return m_ped.PlayerModel; } }



        private void Awake()
        {
            Instance = this;
            m_ped = GetComponent<Ped>();

        }


        private void OnGUI()
        {
            Event e = Event.current;

            
            // Shohw flying / noclip states
            if (m_ped.enableFlying || m_ped.enableNoclip)
            {
                int height = (m_ped.enableFlying && m_ped.enableNoclip) ? 50 : 25;
                GUILayout.BeginArea(new Rect(Screen.width - 140, Screen.height - height, 140, height));

                if (m_ped.enableFlying)
                    GUILayout.Label("Flying-mode enabled!");

                if (m_ped.enableNoclip)
                    GUILayout.Label("Noclip-mode enabled!");

                GUILayout.EndArea();
            }

            if (_showVel && Loader.HasLoaded)
                GUI.Label(GUIUtils.GetCornerRect(ScreenCorner.TopLeft, 100, 25, new Vector2(5, 5)), string.Format("{0:0.0} km/h", deltaPos.magnitude * 3.6f / velTimer), new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });

            
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
			

            if (!m_ped.enableFlying && !m_ped.IsInVehicle && Input.GetKeyDown(KeyCode.T))
            {
                m_ped.enableFlying = true;
                m_ped.Movement = new Vector3(0f, 0f, 0f); // disable current movement
                PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
            }
            else if (m_ped.enableFlying && Input.GetKeyDown(KeyCode.T))
            {
                m_ped.enableFlying = false;
            }

            if (!m_ped.IsInVehicle && Input.GetKeyDown(KeyCode.R))
            {
                m_ped.enableNoclip = !m_ped.enableNoclip;
                m_ped.characterController.detectCollisions = !m_ped.enableNoclip;
                if (m_ped.enableNoclip && !m_ped.enableFlying)
                {
                    m_ped.Movement = new Vector3(0f, 0f, 0f); // disable current movement
                    PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
                }
            }


			this.UpdateCamera ();


			if (!GameManager.CanPlayerReadInput()) return;


            if (Input.GetButtonDown("Use") && m_ped.IsInVehicle)
            {
                m_ped.ExitVehicle();

                return;
            }

            
			if (m_ped.IsInVehicle) return;


            if (m_ped.enableFlying || m_ped.enableNoclip)
            {
                var up_down = 0.0f;

                if (Input.GetKey(KeyCode.Backspace))
                {
                    up_down = 1.0f;
                }
                else if (Input.GetKey(KeyCode.Delete))
                {
                    up_down = -1.0f;
                }

                var inputMove = new Vector3(Input.GetAxis("Horizontal"), up_down, Input.GetAxis("Vertical"));

                m_ped.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                    new Vector3(1f, 1f, 1f)).normalized;

                m_ped.Movement *= 10.0f;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    m_ped.Movement *= 10.0f;
                }
                else if (Input.GetKey(KeyCode.Z))
                {
                    m_ped.Movement *= 100.0f;
                }

                return;
            }


			m_ped.WeaponHolder.IsAimOn = m_ped.WeaponHolder.IsHoldingWeapon && Input.GetMouseButton (1);
			m_ped.WeaponHolder.IsFireOn = m_ped.WeaponHolder.IsHoldingWeapon && Input.GetMouseButton (0);

			//if (!_player.WeaponHolder.IsAimOn)
            {
				// give input to player

				m_ped.IsJumpOn = Input.GetKey (m_jumpKey);

				Vector3 inputMove = Vector3.zero;
				if (m_smoothMovement)
					inputMove = new Vector3 (Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));
				else
					inputMove = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0f, Input.GetAxisRaw ("Vertical"));

                if (inputMove.sqrMagnitude > 0f)
                {
                    inputMove.Normalize();

					if (Input.GetKey (m_walkKey))
						m_ped.IsWalking = true;
					else if (Input.GetKey (m_sprintKey))
						m_ped.IsSprinting = true;
					else
						m_ped.IsRunning = true;

                }
               	
                m_ped.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                    new Vector3(1f, 0f, 1f)).normalized;

				// player heading should be assigned here, not in Player class
			//	if (!_player.IsAiming)
				{
					if (m_ped.Movement.sqrMagnitude > float.Epsilon) {
						m_ped.Heading = m_ped.Movement;
					}
				}

            }


            if (!Input.GetButtonDown("Use")) return;

            // find any vehicles that have a seat inside the checking radius and sort by closest seat
            var vehicles = FindObjectsOfType<Vehicle>()
                .Where(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position) < EnterVehicleRadius)
                .OrderBy(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position)).ToArray();

            foreach (var vehicle in vehicles)
            {
                var seat = vehicle.FindClosestSeat(transform.position);

                m_ped.EnterVehicle(vehicle, seat);

                break;
            }

        }

		private void UpdateCamera ()
		{

			if (GameManager.CanPlayerReadInput())
			{
				// rotate camera

				var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

				mouseDelta = Vector2.Scale(mouseDelta, CursorSensitivity);


				if (m_doSmooth)
				{
					_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
					_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

					_mouseAbsolute += _smoothMouse;
				}
				else
					_mouseAbsolute += mouseDelta;

				// Waiting for an answer: https://stackoverflow.com/questions/50837685/camera-global-rotation-clamping-issue-unity3d

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

			Camera.transform.rotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up)
				* Quaternion.AngleAxis(-_mouseAbsolute.y, Vector3.right);


			// this must be called from here (right after the camera transform is changed), otherwise camera will shake
			m_ped.WeaponHolder.RotatePlayerInDirectionOfAiming ();


			// cast a ray from player to camera to see if it hits anything
			// if so, then move the camera to hit point

			float distance;
			Vector3 castFrom;
			Vector3 castDir = -Camera.transform.forward;

			float scrollValue = Input.mouseScrollDelta.y;
			if (!GameManager.CanPlayerReadInput ())
				scrollValue = 0;

			if (m_ped.IsInVehicle)
			{
				CarCameraDistance = Mathf.Clamp (CarCameraDistance - scrollValue, 2.0f, 32.0f);
				distance = CarCameraDistance;
				castFrom = this.CameraFocusPosVehicle;
				// cast towards current camera position
			//	castDir = (Camera.transform.position - castFrom).normalized;
			}
			else if (m_ped.IsAiming)
			{
				castFrom = this.CameraFocusPos;

				// use distance from gun aiming offset ?
				if (m_ped.CurrentWeapon.GunAimingOffset != null) {
				//	Vector3 desiredCameraPos = this.transform.TransformPoint (- _player.CurrentWeapon.GunAimingOffset.Aim) + Vector3.up * .5f;
				//	Vector3 desiredCameraPos = this.transform.TransformPoint( new Vector3(0.8f, 1.0f, -1) );
					Vector3 desiredCameraPos = this.CameraFocusPos + Camera.transform.TransformVector( m_ped.WeaponHolder.cameraAimOffset );
					Vector3 diff = desiredCameraPos - castFrom;
					distance = diff.magnitude;
					castDir = diff.normalized;
				}
				else
					distance = PlayerCameraDistance;
			}
			else
			{
				PlayerCameraDistance = Mathf.Clamp(PlayerCameraDistance - scrollValue, 2.0f, 32.0f);
				distance = PlayerCameraDistance;
				castFrom = this.CameraFocusPos;
			}

			var castRay = new Ray(castFrom, castDir);

			RaycastHit hitInfo;

			if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance,
				-1 ^ (1 << MapObject.BreakableLayer) ^ (1 << Vehicle.Layer)))
			{
				distance = hitInfo.distance;
			}

			Camera.transform.position = castRay.GetPoint(distance);


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