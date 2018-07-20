using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #region Private fields

		public	static	PlayerController	Instance { get ; private set ; }
		public	static	PlayerController	FindInstance() {
			return FindObjectOfType<PlayerController> ();
		}

        private Player _player;

        private float _pitch;
        private float _yaw;

		public static bool _showVel = true;

        public static bool _showMenu
        {
            get
            {
				return UI.PauseMenu.IsOpened;
            }
            set
            {
				UI.PauseMenu.IsOpened = value;
            }
        }

        // Alpha speedometer
        private const float velTimer = 1 / 4f;

        private static float velCounter = velTimer;

        private static Vector3 lastPos = Vector3.zero,
                               deltaPos = Vector3.zero;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse = Vector2.zero;
        private Vector3 targetDirection = Vector3.forward;

        #endregion Private fields

        #region Inspector Fields

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 3.0f;

        //public Vector2 PitchClamp = new Vector2(-89f, 89f);
        public Vector2 clampInDegrees = new Vector2(90, 60);

        public float EnterVehicleRadius = 5.0f;

        public float animationBlendWeight = 0.4f;

        public Vector2 smoothing = new Vector2(10, 10);
        public bool m_doSmooth = true;


        public float CurVelocity
        {
            get
            {
                return deltaPos.magnitude * 3.6f / velTimer;
            }
        }

        #endregion Inspector Fields

        #region Properties

        public Camera Camera { get { return _player.Camera; } }
        public Pedestrian PlayerModel { get { return _player.PlayerModel; } }

        #endregion Properties



        private void Awake()
        {
            Instance = this;
            _player = GetComponent<Player>();

        }


        private void OnGUI()
        {
            Event e = Event.current;

            
            // Shohw flying / noclip states
            if (_player.enableFlying || _player.enableNoclip)
            {
                int height = (_player.enableFlying && _player.enableNoclip) ? 50 : 25;
                GUILayout.BeginArea(new Rect(Screen.width - 140, Screen.height - height, 140, height));

                if (_player.enableFlying)
                    GUILayout.Label("Flying-mode enabled!");

                if (_player.enableNoclip)
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
			_player.IsWalking = _player.IsRunning = _player.IsSprinting = false;
			

            if (!_player.enableFlying && !_player.IsInVehicle && Input.GetKeyDown(KeyCode.T))
            {
                _player.enableFlying = true;
                _player.Movement = new Vector3(0f, 0f, 0f); // disable current movement
                PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
            }
            else if (_player.enableFlying && Input.GetKeyDown(KeyCode.T))
            {
                _player.enableFlying = false;
            }

            if (!_player.IsInVehicle && Input.GetKeyDown(KeyCode.R))
            {
                _player.enableNoclip = !_player.enableNoclip;
                _player.characterController.detectCollisions = !_player.enableNoclip;
                if (_player.enableNoclip && !_player.enableFlying)
                {
                    _player.Movement = new Vector3(0f, 0f, 0f); // disable current movement
                    PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
                }
            }

			if (GameManager.CanPlayerReadInput())
            {
				// Move player's camera.
                
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
            }

            Camera.transform.rotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up)
                                      * Quaternion.AngleAxis(-_mouseAbsolute.y, Vector3.right);

            float distance;
            Vector3 castFrom;

            float scrollValue = Input.mouseScrollDelta.y;

			if (!GameManager.CanPlayerReadInput ())
                scrollValue = 0;

            if (_player.IsInVehicle)
            {
                CarCameraDistance = Mathf.Clamp(CarCameraDistance - scrollValue, 2.0f, 32.0f);
                distance = CarCameraDistance;
                castFrom = _player.CurrentVehicle.transform.position;
            }
            else
            {
                PlayerCameraDistance = Mathf.Clamp(PlayerCameraDistance - scrollValue, 2.0f, 32.0f);
                distance = PlayerCameraDistance;
                castFrom = transform.position + Vector3.up * .5f;
            }

            var castRay = new Ray(castFrom, -Camera.transform.forward);

            RaycastHit hitInfo;

            if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance,
                -1 ^ (1 << MapObject.BreakableLayer) ^ (1 << Vehicle.Layer)))
            {
                distance = hitInfo.distance;
            }

            Camera.transform.position = castRay.GetPoint(distance);


			if (!GameManager.CanPlayerReadInput()) return;


            if (Input.GetButtonDown("Use") && _player.IsInVehicle)
            {
                _player.ExitVehicle();

                return;
            }

            
			if (_player.IsInVehicle) return;


            if (_player.enableFlying || _player.enableNoclip)
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

                _player.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                    new Vector3(1f, 1f, 1f)).normalized;

                _player.Movement *= 10.0f;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _player.Movement *= 10.0f;
                }
                else if (Input.GetKey(KeyCode.Z))
                {
                    _player.Movement *= 100.0f;
                }

                return;
            }


			if (_player.WeaponHolder.IsHoldingWeapon && Input.GetMouseButton(1))
            {
                // player is holding a weapon, and right click is on => aim with weapon
				_player.WeaponHolder.IsAimOn = true;
            }
            else
            {
				// player is not aiming

				_player.WeaponHolder.IsAimOn = false;

				// give input to player

                var inputMove = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

                if (inputMove.sqrMagnitude > 0f)
                {
                    inputMove.Normalize();

					if (Input.GetKey (KeyCode.LeftAlt))
						_player.IsWalking = true;
					else if (Input.GetKey (KeyCode.Space))
						_player.IsSprinting = true;
					else
						_player.IsRunning = true;

                }
               	
                _player.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                    new Vector3(1f, 0f, 1f)).normalized;
            }


            if (!Input.GetButtonDown("Use")) return;

            // find any vehicles that have a seat inside the checking radius and sort by closest seat
            var vehicles = FindObjectsOfType<Vehicle>()
                .Where(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position) < EnterVehicleRadius)
                .OrderBy(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position)).ToArray();

            foreach (var vehicle in vehicles)
            {
                var seat = vehicle.FindClosestSeat(transform.position);

                _player.EnterVehicle(vehicle, seat);

                break;
            }

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(transform.position, EnterVehicleRadius);

            var vehicles = FindObjectsOfType<Vehicle>()
                .Where(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position) < EnterVehicleRadius)
                .OrderBy(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position)).ToArray();

            foreach (var vehicle in vehicles)
            {
                foreach (var seat in vehicle.Seats)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(seat.Parent.position, 0.1f);
                }

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


        private void Play2Animations(int[] boneIds1, int[] boneIds2,
            AnimGroup group1, AnimGroup group2, AnimIndex animIndex1, AnimIndex animIndex2)
        {
            PlayerModel._anim[PlayerModel.GetAnimName(group1, animIndex1)].layer = 0;

            AnimationState state = PlayerModel.PlayAnim(group1, animIndex1, PlayMode.StopSameLayer);

            foreach (int boneId in boneIds1)
            {
                Frame f = PlayerModel.Frames.GetByBoneId(boneId);
                state.AddMixingTransform(f.transform, true);
                //	runState.wrapMode = WrapMode.Loop;
            }

            PlayerModel._anim[PlayerModel.GetAnimName(group2, animIndex2)].layer = 1;

            state = PlayerModel.PlayAnim(group2, animIndex2, PlayMode.StopSameLayer);

            foreach (int boneId in boneIds2)
            {
                Frame f = PlayerModel.Frames.GetByBoneId(boneId);
                //	state.RemoveMixingTransform(f.transform);
                state.AddMixingTransform(f.transform, true);
                //	state.wrapMode = WrapMode.Loop;
            }
            state.weight = animationBlendWeight;

            //	PlayerModel._anim.Blend( );
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