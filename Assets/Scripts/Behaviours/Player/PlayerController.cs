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

        private static PlayerController me;

        private Player _player;

        private float _pitch;
        private float _yaw;

        private Transform[] _spawns;

        private static int fpsTextureWidth = 75;
        private static int fpsTextureHeight = 25;
        private static float fpsMaximum = 60.0f;
        /*private static float fpsGreen = 50.0f;
        private static float fpsRed = 23.0f;*/
        private float fpsDeltaTime = 0.0f;
        private Texture2D fpsTexture = null;
        private float[] fpsHistory = new float[fpsTextureWidth];
        private int fpsIndex = 0;

        private static Rect teleportWindowRect;
        private const int teleportWindowID = 1;

        private static bool _showFPS = true,
                            _showVel = true;

        private static bool __menu;

        public static bool _showMenu
        {
            get
            {
                return __menu;
            }
            set
            {
                __menu = value;

                // Fix: This is weird
                if (me.CursorLocked)
                    me.ChangeCursorState(false);
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
        public Vector2 clampInDegrees = new Vector2(90, 90);

        public float EnterVehicleRadius = 5.0f;

        public float animationBlendWeight = 0.4f;

        public Vector2 smoothing = new Vector2(10, 10);
        public bool m_doSmooth = true;

        public bool CursorLocked;

        #endregion Inspector Fields

        #region Properties

        public Camera Camera { get { return _player.Camera; } }
        public Pedestrian PlayerModel { get { return _player.PlayerModel; } }

        /*public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = Mathf.Clamp(value, clampInDegrees.x, -clampInDegrees.x);

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
        }*/

        #endregion Properties

        private void Awake()
        {
            me = this;
            _player = GetComponent<Player>();

            CursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _spawns = GameObject.Find("Player Spawns").GetComponentsInChildren<Transform>();
            fpsTexture = new Texture2D(fpsTextureWidth, fpsTextureHeight, TextureFormat.RGBA32, false, true);

            teleportWindowRect = new Rect(Screen.width - 260, 10, 250, 10 + (25 * _spawns.Count()));
        }

        private void teleportWindow(int windowID)
        {
            for (int i = 1; i < _spawns.Count(); i++)
            {
                if (GUILayout.Button(_spawns[i].name))
                {
                    _player.transform.position = _spawns[i].position;
                    _player.transform.rotation = _spawns[i].rotation;
                }
            }

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            Event e = Event.current;

            // Show buttons for teleport to player spawn locations
            if (_showMenu && (_spawns.Count() > 1))
                teleportWindowRect = GUILayout.Window(teleportWindowID, teleportWindowRect, teleportWindow, "Teleport to a location:");

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

            if (_showFPS)
            {
                float msec = fpsDeltaTime * 1000.0f;
                float fps = 1.0f / fpsDeltaTime;

                // Show FPS counter
                GUILayout.BeginArea(GUIUtils.GetCornerRect(ScreenCorner.BottomRight, 100, 25, new Vector2(15 + fpsTexture.width, 10)));
                GUILayout.Label(string.Format("{0:0.}fps ({1:0.0}ms)", fps, msec), new GUIStyle("label") { alignment = TextAnchor.MiddleLeft });
                GUILayout.EndArea();

                if (fpsTexture == null) return;

                // Show FPS history
                Color[] colors = new Color[fpsTexture.width * fpsTexture.height];
                /*Color cRed = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                Color cYellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                Color cGreen = new Color(0.0f, 1.0f, 0.0f, 1.0f);*/

                for (int i = 0; i < (fpsTexture.width * fpsTexture.height); i++)
                    colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph

                fpsTexture.SetPixels(colors);

                // Append to history storage
                fpsHistory[fpsIndex] = fps;

                int f = fpsIndex;

                if (fps > fpsHistory.Average())
                    fpsMaximum = fps;

                // Draw graph into texture
                for (int i = fpsTexture.width - 1; i >= 0; i--)
                {
                    float graphVal = (fpsHistory[f] > fpsMaximum) ? fpsMaximum : fpsHistory[f]; //Clamps
                    int height = (int)(graphVal * fpsTexture.height / (fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

                    float p = fpsHistory[f] / fpsMaximum,
                          r = Mathf.Lerp(1, 1 - p, p),
                          g = Mathf.Lerp(p * 2, p, p);

                    fpsTexture.SetPixel(i, height, new Color(r, g, 0));
                    f--;

                    if (f < 0)
                        f = fpsHistory.Length - 1;
                }

                // Next entry in rolling history buffer
                fpsIndex++;
                if (fpsIndex >= fpsHistory.Length)
                    fpsIndex = 0;

                // Draw texture on GUI
                fpsTexture.Apply(false, false);
                GUI.DrawTexture(GUIUtils.GetCornerRect(ScreenCorner.BottomRight, fpsTexture.width, fpsTexture.height, new Vector2(5, fpsTexture.height - 15)), fpsTexture);
            }
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
            // FPS counting
            fpsDeltaTime += (Time.deltaTime - fpsDeltaTime) * 0.1f;

            if (Input.GetKeyDown(KeyCode.F10))
                _showFPS = !_showFPS;

            if (Input.GetKeyDown(KeyCode.F9))
                _showVel = !_showVel;

            if (!Loader.HasLoaded)
                return;

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

            // Fix cursor state if it has been 'broken', happens eg. with zoom gestures in the editor in macOS
            if (CursorLocked && ((Cursor.lockState != CursorLockMode.Locked) || (Cursor.visible)))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.F1))
                _showMenu = !_showMenu;

            bool isConsoleStateChanged = Console.Instance.m_openKey != Console.Instance.m_closeKey ?
                Input.GetKeyDown(Console.Instance.m_openKey) || Input.GetKeyDown(Console.Instance.m_closeKey) :
                Input.GetKeyDown(Console.Instance.m_openKey);

            if (!_showMenu && (Input.GetKeyDown(KeyCode.Escape) || isConsoleStateChanged || Input.GetKeyDown(KeyCode.F1)))
                ChangeCursorState(!CursorLocked);

            if (CursorLocked)
            { // While cursor is locked and don't show on screen we can move player's camera.
                var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                mouseDelta = Vector2.Scale(mouseDelta, CursorSensitivity); //new Vector2(CursorSensitivity.x * smoothing.x, CursorSensitivity.y * smoothing.y));

                if (m_doSmooth)
                {
                    _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
                    _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

                    // Find the absolute mouse movement value from point zero.
                    _mouseAbsolute += _smoothMouse;
                }
                else
                    _mouseAbsolute += mouseDelta;

                // Waiting for an answer: https://stackoverflow.com/questions/50837685/camera-global-rotation-clamping-issue-unity3d

                /*if (clampInDegrees.x > 0)
                    _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x, clampInDegrees.x);

                if (clampInDegrees.y > 0)
                    _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y, clampInDegrees.y);*/

                Camera.transform.rotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up) //transform.InverseTransformDirection(Vector3.up))
                    * Quaternion.AngleAxis(-_mouseAbsolute.y, Vector3.right);

                //Quaternion targetOrientation = Quaternion.Euler(_mouseAbsolute);

                //Camera.transform.rotation *= targetOrientation;

                /*Vector3 euler = Camera.transform.eulerAngles;

                if (clampInDegrees.x > 0)
                    euler.x = ClampAngle(euler.x, -clampInDegrees.x, clampInDegrees.x);

                if (clampInDegrees.x > 0)
                    euler.y = ClampAngle(euler.y, -clampInDegrees.y, clampInDegrees.y);

                Camera.transform.eulerAngles = euler;*/
            }

            float distance;
            Vector3 castFrom;

            float scrollValue = Input.mouseScrollDelta.y;

            if (Console.Instance.IsOpened)
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

            if (!CursorLocked) return;

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

            if (_player.currentWeaponSlot > 0 && Input.GetMouseButton(1))
            {
                // right click is on
                // aim with weapon
                //	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
                //		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);
                PlayerModel.PlayAnim(AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND, PlayMode.StopAll);
            }
            else
            {
                var inputMove = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

                if (inputMove.sqrMagnitude > 0f)
                {
                    inputMove.Normalize();

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (_player.currentWeaponSlot > 0)
                        {
                            // player is holding a weapon

                            Play2Animations(new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
                                AnimGroup.MyWalkCycle, AnimIndex.Run, AnimIndex.IdleArmed);
                        }
                        else
                        {
                            // player is not holding a weapon
                            PlayerModel.PlayAnim(AnimGroup.WalkCycle,
                                AnimIndex.Run, PlayMode.StopAll);
                        }
                        //    PlayerModel.Running = true;
                    }
                    else
                    {
                        // player is walking
                        if (_player.currentWeaponSlot > 0)
                        {
                            Play2Animations(new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
                                AnimGroup.MyWalkCycle, AnimIndex.Walk, AnimIndex.IdleArmed);
                        }
                        else
                        {
                            PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.Walk, PlayMode.StopAll);
                        }
                        //    PlayerModel.Walking = true;
                    }
                }
                else
                {
                    // player is standing
                    if (_player.currentWeaponSlot > 0)
                    {
                        Play2Animations(new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.MyWalkCycle,
                            AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.IdleArmed);
                        //	PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, PlayMode.StopAll);
                    }
                    else
                    {
                        PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);
                    }
                    //    PlayerModel.Walking = false;
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

        private void ChangeCursorState(bool locked)
        {
            CursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
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