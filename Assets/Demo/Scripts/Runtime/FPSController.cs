// Designed by KINEMATION, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;

using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Demo.Scripts.Runtime
{
    public enum FPSActionState
    {
        None,
        Ready,
        Aiming,
        PointAiming,
    }

    // An example-controller class
    public class FPSController : FPSAnimController
    {
        [Header("General")] [SerializeField]
        private Animator animator;

        [SerializeField] private float turnInPlaceAngle;
        [SerializeField] private AnimationCurve turnCurve = new AnimationCurve(new Keyframe(0f, 0f));
        [SerializeField] private float turnSpeed = 1f;
        
        [Header("Dynamic Motions")]
        [SerializeField] private IKAnimation aimMotionAsset;
        [SerializeField] private IKAnimation leanMotionAsset;
        [SerializeField] private IKAnimation crouchMotionAsset;
        [SerializeField] private IKAnimation unCrouchMotionAsset;
        [SerializeField] private IKAnimation onJumpMotionAsset;
        [SerializeField] private IKAnimation onLandedMotionAsset;

        // Animation Layers
        [SerializeField] [HideInInspector] private LookLayer lookLayer;
        [SerializeField] [HideInInspector] private AdsLayer adsLayer;
        [SerializeField] [HideInInspector] private SwayLayer swayLayer;
        [SerializeField] [HideInInspector] private LocomotionLayer locoLayer;
        [SerializeField] [HideInInspector] private SlotLayer slotLayer;
        [SerializeField] [HideInInspector] private WeaponCollision collisionLayer;
        // Animation Layers
        
        [Header("General")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private float equipDelay = 0f;

        [Header("Camera")]
        [SerializeField] private Transform mainCamera;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform firstPersonCamera;
        [SerializeField] private float sensitivity;
        [SerializeField] private Vector2 freeLookAngle;

        [Header("Movement")] 
        [SerializeField] private FPSMovement movementComponent;
        
        [SerializeField] private List<Weapon> weapons;
        [SerializeField] private FPSCameraShake shake;
        public Transform weaponBone;
        
        private Vector2 _playerInput;

        // Used for free-look
        private Vector2 _freeLookInput;

        private int _index;
        private int _lastIndex;

        private float _fireTimer = -1f;
        private int _bursts;
        private bool _aiming;
        private bool _freeLook;
        private bool _hasActiveAction;
        
        private FPSActionState actionState;

        private float smoothCurveAlpha = 0f;

        private bool _equipTimerActive = false;
        private float _equipTimer = 0f;
        
        private static readonly int Crouching = Animator.StringToHash("Crouching");
        private static readonly int OverlayType = Animator.StringToHash("OverlayType");
        private static readonly int TurnRight = Animator.StringToHash("TurnRight");
        private static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
        private static readonly int Equip = Animator.StringToHash("Equip");
        private static readonly int UnEquip = Animator.StringToHash("Unequip");

        private void InitLayers()
        {
            InitAnimController();
            
            animator = GetComponentInChildren<Animator>();
            lookLayer = GetComponentInChildren<LookLayer>();
            adsLayer = GetComponentInChildren<AdsLayer>();
            locoLayer = GetComponentInChildren<LocomotionLayer>();
            swayLayer = GetComponentInChildren<SwayLayer>();
            slotLayer = GetComponentInChildren<SlotLayer>();
            collisionLayer = GetComponentInChildren<WeaponCollision>();
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            moveRotation = transform.rotation;

            movementComponent = GetComponent<FPSMovement>();
            
            movementComponent.onCrouch.AddListener(OnCrouch);
            movementComponent.onUncrouch.AddListener(OnUncrouch);
            
            movementComponent.onJump.AddListener(OnJump);
            movementComponent.onLanded.AddListener(OnLand);
            
            movementComponent.onSprintStarted.AddListener(OnSprintStarted);
            movementComponent.onSprintEnded.AddListener(OnSprintEnded);
            
            movementComponent.onSlideStarted.AddListener(OnSlideStarted);
            movementComponent.onSlideEnded.AddListener(OnSlideEnded);
            
            movementComponent.onProneStarted.AddListener(() => collisionLayer.SetLayerAlpha(0f));
            movementComponent.onProneEnded.AddListener(() => collisionLayer.SetLayerAlpha(1f));

            InitLayers();
            EquipWeapon();
        }
        
        private void StartWeaponChange()
        {
            DisableAim();
            
            _hasActiveAction = true;
            animator.CrossFade(UnEquip, 0.1f);
            _equipTimerActive = true;
            _equipTimer = 0f;
        }

        public void SetActionActive(int isActive)
        {
            _hasActiveAction = isActive != 0;
        }

        public void RefreshStagedState()
        {
            GetGun().stagedReloadSegment++;
        }
        
        public void ResetStagedState()
        {
            GetGun().stagedReloadSegment = 0;
        }

        public void EquipWeapon()
        {
            if (weapons.Count == 0) return;

            weapons[_lastIndex].gameObject.SetActive(false);
            var gun = weapons[_index];

            _bursts = gun.burstAmount;
            
            StopAnimation(0.1f);
            InitWeapon(gun);
            gun.gameObject.SetActive(true);

            animator.SetFloat(OverlayType, (float) gun.overlayType);
        }
        
        private void ChangeWeapon_Internal()
        {
            if (movementComponent.PoseState == FPSPoseState.Prone 
                || movementComponent.MovementState == FPSMovementState.Sprinting) return;
            
            if (_hasActiveAction) return;
            
            OnFireReleased();
            
            int newIndex = _index;
            newIndex++;
            if (newIndex > weapons.Count - 1)
            {
                newIndex = 0;
            }

            _lastIndex = _index;
            _index = newIndex;

            StartWeaponChange();
        }

        private void DisableAim()
        {
            if (!GetGun().canAim) return;
            
            _aiming = false;
            OnInputAim(_aiming);
            
            actionState = FPSActionState.None;
            adsLayer.SetAds(false);
            adsLayer.SetPointAim(false);
            swayLayer.SetFreeAimEnable(true);
            swayLayer.SetLayerAlpha(1f);
            slotLayer.PlayMotion(aimMotionAsset);
        }

        public void ToggleAim()
        {
            if (!GetGun().canAim) return;
            
            _aiming = !_aiming;

            if (_aiming)
            {
                actionState = FPSActionState.Aiming;
                adsLayer.SetAds(true);
                swayLayer.SetFreeAimEnable(false);
                swayLayer.SetLayerAlpha(0.3f);
                slotLayer.PlayMotion(aimMotionAsset);
                OnInputAim(_aiming);
            }
            else
            {
                DisableAim();
            }

            recoilComponent.isAiming = _aiming;
        }

        public void ChangeScope()
        {
            InitAimPoint(GetGun());
        }

        private void Fire()
        {
            if (_hasActiveAction) return;
            
            GetGun().OnFire();
            PlayAnimation(GetGun().fireClip);
            
            if (recoilComponent != null)
            {
                recoilComponent.Play();
            }
            
            PlayCameraShake(shake);
        }

        private void OnFirePressed()
        {
            if (weapons.Count == 0) return;
            if (_hasActiveAction) return;

            Fire();
            _bursts = GetGun().burstAmount - 1;
            _fireTimer = 0f;
        }

        private Weapon GetGun()
        {
            if (weapons.Count == 0) return null;
            
            return weapons[_index];
        }

        private void OnFireReleased()
        {
            if (weapons.Count == 0) return;

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }
            
            _fireTimer = -1f;
        }

        private void OnSlideStarted()
        {
            lookLayer.SetLayerAlpha(0.3f);
        }

        private void OnSlideEnded()
        {
            lookLayer.SetLayerAlpha(1f);
        }

        private void OnSprintStarted()
        {
            OnFireReleased();
            lookLayer.SetLayerAlpha(0.5f);
            adsLayer.SetLayerAlpha(0f);
            locoLayer.SetReadyWeight(0f);
            
            actionState = FPSActionState.None;

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }
        }

        private void OnSprintEnded()
        {
            lookLayer.SetLayerAlpha(1f);
            adsLayer.SetLayerAlpha(1f);
        }

        private void OnCrouch()
        {
            lookLayer.SetPelvisWeight(0f);
            animator.SetBool(Crouching, true);
            slotLayer.PlayMotion(crouchMotionAsset);
        }

        private void OnUncrouch()
        {
            lookLayer.SetPelvisWeight(1f);
            animator.SetBool(Crouching, false);
            slotLayer.PlayMotion(unCrouchMotionAsset);
        }

        private void OnJump()
        {
            slotLayer.PlayMotion(onJumpMotionAsset);
        }

        private void OnLand()
        {
            slotLayer.PlayMotion(onLandedMotionAsset);
        }

        private void TryReload()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting || _hasActiveAction) return;

            var reloadClip = GetGun().reloadClip;

            if (reloadClip == null) return;
            
            OnFireReleased();
            //DisableAim();
            
            PlayAnimation(reloadClip);
            GetGun().Reload();
        }

        private void TryGrenadeThrow()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting || _hasActiveAction) return;

            if (GetGun().grenadeClip == null) return;
            
            OnFireReleased();
            DisableAim();
            PlayAnimation(GetGun().grenadeClip);
        }

        private void UpdateActionInput()
        {
            smoothCurveAlpha = Mathf.Lerp(smoothCurveAlpha, _aiming ? 0.4f : 1f, 
                FPSAnimLib.ExpDecayAlpha(10f, Time.deltaTime));
            
            animator.SetLayerWeight(3, smoothCurveAlpha);
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryGrenadeThrow();
            }

            charAnimData.SetLeanInput(0f);
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeWeapon_Internal();
            }

            if (movementComponent.MovementState == FPSMovementState.Sprinting)
            {
                return;
            }
            
            if (actionState != FPSActionState.Ready)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyUp(KeyCode.Q)
                                                || Input.GetKeyDown(KeyCode.E) || Input.GetKeyUp(KeyCode.E))
                {
                    slotLayer.PlayMotion(leanMotionAsset);
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    charAnimData.SetLeanInput(1f);
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    charAnimData.SetLeanInput(-1f);
                }

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    OnFirePressed();
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    OnFireReleased();
                }

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    ToggleAim();
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    ChangeScope();
                }

                if (Input.GetKeyDown(KeyCode.B) && _aiming)
                {
                    if (actionState == FPSActionState.PointAiming)
                    {
                        adsLayer.SetPointAim(false);
                        actionState = FPSActionState.Aiming;
                    }
                    else
                    {
                        adsLayer.SetPointAim(true);
                        actionState = FPSActionState.PointAiming;
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (actionState == FPSActionState.Ready)
                {
                    actionState = FPSActionState.None;
                    locoLayer.SetReadyWeight(0f);
                    lookLayer.SetLayerAlpha(1f);
                }
                else
                {
                    actionState = FPSActionState.Ready;
                    locoLayer.SetReadyWeight(1f);
                    lookLayer.SetLayerAlpha(.5f);
                    OnFireReleased();
                }
            }
        }

        private Quaternion desiredRotation;
        private Quaternion moveRotation;
        private float turnProgress = 1f;
        private bool isTurning = false;

        private void TurnInPlace()
        {
            float turnInput = _playerInput.x;
            _playerInput.x = Mathf.Clamp(_playerInput.x, -90f, 90f);
            turnInput -= _playerInput.x;

            float sign = Mathf.Sign(_playerInput.x);
            if (Mathf.Abs(_playerInput.x) > turnInPlaceAngle)
            {
                if (!isTurning)
                {
                    turnProgress = 0f;
                    
                    animator.ResetTrigger(TurnRight);
                    animator.ResetTrigger(TurnLeft);
                    
                    animator.SetTrigger(sign > 0f ? TurnRight : TurnLeft);
                }
                
                isTurning = true;
            }

            transform.rotation *= Quaternion.Euler(0f, turnInput, 0f);
            
            float lastProgress = turnCurve.Evaluate(turnProgress);
            turnProgress += Time.deltaTime * turnSpeed;
            turnProgress = Mathf.Min(turnProgress, 1f);
            
            float deltaProgress = turnCurve.Evaluate(turnProgress) - lastProgress;

            _playerInput.x -= sign * turnInPlaceAngle * deltaProgress;
            
            transform.rotation *= Quaternion.Slerp(Quaternion.identity,
                Quaternion.Euler(0f, sign * turnInPlaceAngle, 0f), deltaProgress);
            
            if (Mathf.Approximately(turnProgress, 1f) && isTurning)
            {
                isTurning = false;
            }
        }

        private float _jumpState = 0f;

        private void UpdateLookInput()
        {
            _freeLook = Input.GetKey(KeyCode.X);

            float deltaMouseX = Input.GetAxis("Mouse X") * sensitivity;
            float deltaMouseY = -Input.GetAxis("Mouse Y") * sensitivity;
            
            if (_freeLook)
            {
                // No input for both controller and animation component. We only want to rotate the camera

                _freeLookInput.x += deltaMouseX;
                _freeLookInput.y += deltaMouseY;

                _freeLookInput.x = Mathf.Clamp(_freeLookInput.x, -freeLookAngle.x, freeLookAngle.x);
                _freeLookInput.y = Mathf.Clamp(_freeLookInput.y, -freeLookAngle.y, freeLookAngle.y);

                return;
            }

            _freeLookInput = Vector2.Lerp(_freeLookInput, Vector2.zero, 
                FPSAnimLib.ExpDecayAlpha(15f, Time.deltaTime));
            
            _playerInput.x += deltaMouseX;
            _playerInput.y += deltaMouseY;


            float proneWeight = animator.GetFloat("ProneWeight");
            Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);
            
            _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
            moveRotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
            TurnInPlace();

            _jumpState = Mathf.Lerp(_jumpState, movementComponent.IsInAir() ? 1f : 0f,
                FPSAnimLib.ExpDecayAlpha(10f, Time.deltaTime));

            float moveWeight = Mathf.Clamp01(movementComponent.AnimatorVelocity.magnitude);
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, moveWeight);
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, _jumpState);
            _playerInput.x *= 1f - moveWeight;
            _playerInput.x *= 1f - _jumpState;

            charAnimData.SetAimInput(_playerInput);
            charAnimData.AddDeltaInput(new Vector2(deltaMouseX, charAnimData.deltaAimInput.y));
        }

        private void UpdateFiring()
        {
            if (recoilComponent == null) return;
            
            if (recoilComponent.fireMode != FireMode.Semi && _fireTimer >= 60f / GetGun().fireRate)
            {
                Fire();

                if (recoilComponent.fireMode == FireMode.Burst)
                {
                    _bursts--;

                    if (_bursts == 0)
                    {
                        _fireTimer = -1f;
                        OnFireReleased();
                    }
                    else
                    {
                        _fireTimer = 0f;
                    }
                }
                else
                {
                    _fireTimer = 0f;
                }
            }

            if (_fireTimer >= 0f)
            {
                _fireTimer += Time.deltaTime;
            }
        }

        private Quaternion lastRotation;
        
        private void UpdateTimer()
        {
            if (!_equipTimerActive) return;

            if (_equipTimer > equipDelay)
            {
                EquipWeapon();
                
                _equipTimerActive = false;
                _equipTimer = 0f;
                return;
            }

            _equipTimer += Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            if (weaponBone != null)
            {
                Gizmos.DrawWireSphere(weaponBone.position, 0.03f);
            }
        }

        private void Update()
        {
            Time.timeScale = timeScale;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(0);
            }
            
            UpdateActionInput();
            UpdateLookInput();
            UpdateFiring();

            charAnimData.moveInput = movementComponent.AnimatorVelocity;
            
            UpdateTimer();
            UpdateAnimController();
        }

        private Quaternion _smoothBodyCam;

        public void UpdateCameraRotation()
        {
            Vector2 finalInput = new Vector2(_playerInput.x, _playerInput.y);
            (Quaternion, Vector3) cameraTransform =
                (transform.rotation * Quaternion.Euler(finalInput.y, finalInput.x, 0f),
                    firstPersonCamera.position);

            cameraHolder.rotation = cameraTransform.Item1;
            cameraHolder.position = cameraTransform.Item2;

            mainCamera.rotation = cameraHolder.rotation * Quaternion.Euler(_freeLookInput.y, _freeLookInput.x, 0f);
        }
    }
}