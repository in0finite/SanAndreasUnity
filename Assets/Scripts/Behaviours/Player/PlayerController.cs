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

        private Player _player;

        private float _pitch;
        private float _yaw;

		private Transform[] _spawns;

		private static int fpsTextureWidth = 75;
		private static int fpsTextureHeight = 25;
		private static float fpsMaximum = 60.0f;
		private static float fpsGreen = 50.0f;
		private static float fpsRed = 23.0f;
		private float fpsDeltaTime = 0.0f;
		private Texture2D fpsTexture = null;
		private float[] fpsHistory = new float[fpsTextureWidth];
		private int fpsIndex = 0;

        #endregion

        #region Inspector Fields

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 3.0f;

        public Vector2 PitchClamp = new Vector2(-89f, 89f);

        public float EnterVehicleRadius = 5.0f;

		public	float	animationBlendWeight = 0.4f ;

		public bool CursorLocked;

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

			CursorLocked = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			_spawns = GameObject.Find ("Player Spawns").GetComponentsInChildren<Transform> ();
			fpsTexture = new Texture2D (fpsTextureWidth, fpsTextureHeight, TextureFormat.RGBA32, false, true);

			teleportWindowRect = new Rect (Screen.width - 260, 10, 250, 10 + (25 * _spawns.Count()));
        }

		private static Rect teleportWindowRect;
		private const int teleportWindowID = 1;

		void teleportWindow(int windowID) {
			for (int i = 1; i < _spawns.Count (); i++) {
				if (GUILayout.Button (_spawns [i].name)) {
					_player.transform.position = _spawns [i].position;
					_player.transform.rotation = _spawns [i].rotation;
				}
			}

			GUI.DragWindow();
		}

		void OnGUI() {
			// Show buttons for teleport to player spawn locations
			if ((!CursorLocked) && (!_player.IsInVehicle) && (_spawns.Count () > 1)) {
				teleportWindowRect = GUILayout.Window (teleportWindowID, teleportWindowRect, teleportWindow, "Teleport to a location:");
			}

			// Shohw flying / noclip states
			if (_player.enableFlying || _player.enableNoclip) {
				int height = (_player.enableFlying && _player.enableNoclip) ? 50 : 25;
				GUILayout.BeginArea (new Rect (Screen.width - 140, Screen.height - height, 140, height));
				if (_player.enableFlying) {
					GUILayout.Label ("Flying-mode enabled!");
				}
				if (_player.enableNoclip) {
					GUILayout.Label ("Noclip-mode enabled!");
				}
				GUILayout.EndArea ();
			}

			// Show FPS counter
			float msec = fpsDeltaTime * 1000.0f;
			float fps = 1.0f / fpsDeltaTime;
			GUILayout.BeginArea (new Rect (15 + fpsTexture.width, Screen.height - 25, 100, 25));
			GUILayout.Label (string.Format("{0:0.}fps ({1:0.0}ms)", fps, msec));
			GUILayout.EndArea ();

			if (fpsTexture == null) return;

			// Show FPS history
			Color[] colors = new Color[fpsTexture.width * fpsTexture.height];
			Color cRed = new Color (1.0f, 0.0f, 0.0f, 1.0f);
			Color cYellow = new Color (1.0f, 1.0f, 0.0f, 1.0f);
			Color cGreen = new Color (0.0f, 1.0f, 0.0f, 1.0f);
			for (int i = 0; i < (fpsTexture.width * fpsTexture.height); i++) {
				colors [i] = new Color (0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph
			}
			fpsTexture.SetPixels (colors);
			// Append to history storage
			fpsHistory [fpsIndex] = fps;
			int f = fpsIndex;
			// Draw graph into texture
			for (int i = fpsTexture.width - 1; i >= 0; i--) {
				float graphVal = (fpsHistory [f] > fpsMaximum) ? fpsMaximum : fpsHistory [f];
				int height = (int)(graphVal * fpsTexture.height / (fpsMaximum + 0.1f));
				Color c = (fpsHistory[f] >= fpsGreen) ? cGreen : ((fpsHistory[f] <= fpsRed) ? cRed : cYellow);
				fpsTexture.SetPixel(i, height, c);
				f--;
				if (f < 0) {
					f = fpsHistory.Length - 1;
				}
			}
			// Next entry in rolling history buffer
			fpsIndex++;
			if (fpsIndex >= fpsHistory.Length) {
				fpsIndex = 0;
			}
			// Draw texture on GUI
			fpsTexture.Apply (false, false);
			GUI.DrawTexture (new Rect(5, Screen.height - fpsTexture.height - 5, fpsTexture.width, fpsTexture.height), fpsTexture);
		}

        private void Update()
        {
			// FPS counting
			fpsDeltaTime += (Time.deltaTime - fpsDeltaTime) * 0.1f;

			if (!Loader.HasLoaded)
				return;

			if (!_player.enableFlying && !_player.IsInVehicle && Input.GetKeyDown (KeyCode.T)) {
				_player.enableFlying = true;
				_player.Movement = new Vector3 (0f, 0f, 0f); // disable current movement
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
			} else if (_player.enableFlying && Input.GetKeyDown (KeyCode.T)) {
				_player.enableFlying = false;
			}

			if (!_player.IsInVehicle && Input.GetKeyDown (KeyCode.R)) {
				_player.enableNoclip = !_player.enableNoclip;
				_player.characterController.detectCollisions = !_player.enableNoclip;
				if (_player.enableNoclip && !_player.enableFlying) {
					_player.Movement = new Vector3 (0f, 0f, 0f); // disable current movement
					PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
				}
			}

			// Fix cursor state if it has been 'broken', happens eg. with zoom gestures in the editor in macOS
			if (CursorLocked && ((Cursor.lockState != CursorLockMode.Locked) || (Cursor.visible))) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			
            if (!CursorLocked && Input.GetKeyDown(KeyCode.Q)) {
                CursorLocked = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (CursorLocked && Input.GetKeyDown(KeyCode.Q)) {
                CursorLocked = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (CursorLocked)
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

			if (!CursorLocked) return;

            if (Input.GetButtonDown("Use") && _player.IsInVehicle)
            {
                _player.ExitVehicle();

                return;
            }

            if (_player.IsInVehicle) return;
            
			if (_player.enableFlying || _player.enableNoclip) {
				var up_down = 0.0f;
				if (Input.GetKey (KeyCode.Backspace)) {
					up_down = 1.0f;
				} else if (Input.GetKey (KeyCode.Delete)) {
					up_down = -1.0f;
				}
				var inputMove = new Vector3 (Input.GetAxis ("Horizontal"), up_down, Input.GetAxis ("Vertical"));
				_player.Movement = Vector3.Scale (Camera.transform.TransformVector (inputMove),
					new Vector3 (1f, 1f, 1f)).normalized;
				_player.Movement *= 10.0f;
				if (Input.GetKey (KeyCode.LeftShift)) {
					_player.Movement *= 10.0f;
				} else if (Input.GetKey (KeyCode.Z)) {
					_player.Movement *= 100.0f;
				}
				return;
			}
			
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
