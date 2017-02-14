using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Linq;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #region Private fields

        private bool _lockedCursor;

        private Player _player;

        private float _pitch;
        private float _yaw;

        #endregion

        #region Inspector Fields

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 3.0f;

        public Vector2 PitchClamp = new Vector2(-89f, 89f);

        public float EnterVehicleRadius = 5.0f;

		public	float	animationBlendWeight = 0.4f ;

        #endregion

        #region Properties

        public Camera Camera { get { return _player.Camera; } }
        public Pedestrian PlayerModel { get { return _player.PlayerModel; } }

        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = Mathf.Clamp(value, PitchClamp.x, PitchClamp.y);

                var angles = Camera.transform.localEulerAngles;
                angles.x = _pitch;
                Camera.transform.localEulerAngles = angles;
            }
        }

        public float Yaw
        {
            get { return _yaw; }
            set
            {
                _yaw = value.NormalizeAngle();

                var trans = Camera.transform;
                var angles = trans.localEulerAngles;
                angles.y = _yaw;
                trans.localEulerAngles = angles;
            }
        }

        #endregion

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        private void Update()
        {
            if (!_lockedCursor && Input.GetMouseButtonDown(0)) {
                _lockedCursor = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (_lockedCursor && Input.GetKeyDown(KeyCode.Escape)) {
                _lockedCursor = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_lockedCursor)
            {
                var cursorDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                Yaw += cursorDelta.x * CursorSensitivity.x;
                Pitch -= cursorDelta.y * CursorSensitivity.y;
            }

            Camera.transform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up)
                * Quaternion.AngleAxis(Pitch, Vector3.right);

            float distance;
            Vector3 castFrom;

            if (_player.IsInVehicle) {
                CarCameraDistance = Mathf.Clamp(CarCameraDistance - Input.mouseScrollDelta.y, 2.0f, 32.0f);
                distance = CarCameraDistance;
                castFrom = _player.CurrentVehicle.transform.position;
            } else {
                PlayerCameraDistance = Mathf.Clamp(PlayerCameraDistance - Input.mouseScrollDelta.y, 2.0f, 32.0f);
                distance = PlayerCameraDistance;
                castFrom = transform.position + Vector3.up * .5f;
            }

            var castRay = new Ray(castFrom, -Camera.transform.forward);

            RaycastHit hitInfo;

            if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance, 
                -1 ^ (1 << MapObject.BreakableLayer) ^ (1 << Vehicle.Layer))) {
                distance = hitInfo.distance;
            }

            Camera.transform.position = castRay.GetPoint(distance);

            if (Input.GetButtonDown("Use") && _player.IsInVehicle)
            {
                _player.ExitVehicle();

                return;
            }

            if (_player.IsInVehicle) return;
            if (!_lockedCursor) return;

			
			if (_player.currentWeaponSlot > 0 && Input.GetMouseButton (1)) {
				// right click is on
				// aim with weapon
			//	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
			//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);
				PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND, PlayMode.StopAll);
			} else {

				var inputMove = new Vector3 (Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));

				if (inputMove.sqrMagnitude > 0f) {
					inputMove.Normalize ();

					if (Input.GetKey (KeyCode.LeftShift)) {
						if (_player.currentWeaponSlot > 0) {
							// player is holding a weapon

							this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.WalkCycle,
								AnimGroup.MyWalkCycle, AnimIndex.Run, AnimIndex.IdleArmed);
						
						} else {
							// player is not holding a weapon
							PlayerModel.PlayAnim (AnimGroup.WalkCycle,
								AnimIndex.Run, PlayMode.StopAll);
						}
						//    PlayerModel.Running = true;
					} else {
						// player is walking
						if (_player.currentWeaponSlot > 0) {
							this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.WalkCycle,
								AnimGroup.MyWalkCycle, AnimIndex.Walk, AnimIndex.IdleArmed);
						} else {
							PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Walk, PlayMode.StopAll);
						}
						//    PlayerModel.Walking = true;
					}
				} else {
					// player is standing
					if (_player.currentWeaponSlot > 0) {
						this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
							AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.IdleArmed);
						//	PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, PlayMode.StopAll);
					} else {
						PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);
					}
					//    PlayerModel.Walking = false;
				}

				_player.Movement = Vector3.Scale (Camera.transform.TransformVector (inputMove),
					new Vector3 (1f, 0f, 1f)).normalized;
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

        void OnDrawGizmosSelected()
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

		void	Play2Animations( int[] boneIds1, int[] boneIds2,
			AnimGroup group1, AnimGroup group2, AnimIndex animIndex1, AnimIndex animIndex2 ) {

			PlayerModel._anim [ PlayerModel.GetAnimName( group1, animIndex1 ) ].layer = 0;

			AnimationState state = PlayerModel.PlayAnim (group1, animIndex1, PlayMode.StopSameLayer);
			
			foreach( int boneId in boneIds1 ) {
				Frame f = PlayerModel.Frames.GetByBoneId (boneId);
				state.AddMixingTransform (f.transform, true);
				//	runState.wrapMode = WrapMode.Loop;
			}
			
			PlayerModel._anim [ PlayerModel.GetAnimName( group2, animIndex2 ) ].layer = 1;

			state = PlayerModel.PlayAnim (group2, animIndex2, PlayMode.StopSameLayer);
			
			foreach( int boneId in boneIds2 ) {
				Frame f = PlayerModel.Frames.GetByBoneId (boneId);
				//	state.RemoveMixingTransform(f.transform);
				state.AddMixingTransform (f.transform, true);
				//	state.wrapMode = WrapMode.Loop;
			}
			state.weight = this.animationBlendWeight;

			//	PlayerModel._anim.Blend( );

		}

    }
}
