using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace Believe.Games.Studios
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : Player
    {
        [Header("Components")]
        [Tooltip("To use your own animators, simply use an animator override controller to override the armature animator controller provided in the animator component")]
        [SerializeField] AnimatorOverrideController customAnimator;
        Animator animator;
        CinemachineCamera cCam;
        Transform cameraRoot;
        CharacterController characterController;

        [Header("Movement Parameters")]
        [Range(0.05f, 0.4f)]
        [SerializeField] float smoothTurnTime = 0.2f;
        [SerializeField] float walkSpeed = 2.33f;
        [SerializeField] float runSpeed = 5.33f;
        [Range(0.1f, 3f), Tooltip("This is how fast we accelerate to the target speed instead of jumping into the speed")]
        [SerializeField] float accelerationTime = 0.33f;
        [SerializeField] float jumpTimeout = 1;
        [SerializeField] float gravity = -15f;
        [SerializeField] float jumpHeight = 1.5f;
        public float currentSpeed;
        float jumpCooldown;
        float targetRotation;
        float currentVelocity;
        float terminalVelocity = 53;
        Vector3 verticalVelocity;
        bool isGrounded;

        [Header("Crouched Variables")]
        [SerializeField] float crouchedSpeed = 1.33f;
        [SerializeField] float crouchedHeight;
        private float defaultHeight;
        private float crouchedFov;
        private float defaultFov;
        [SerializeField]Vector3 crouchedControllerCenter=new Vector3(0,0.58f,0);
        Vector3 defaultControllerCenter;

        [Header("State Machine")]
        bool isCrouched = false;
        bool isStrafe = false;

        [Header("Footstep")]
        [SerializeField] AudioSource footstepSource;
        [SerializeField] AudioClip[] footstepClips;
        [SerializeField] AudioClip jumpClip;
        [SerializeField] AudioClip landClip;
        int clipIndex=0;

        [Header("Animation Rigging")]
        [SerializeField] Transform headTarget;
        [SerializeField] Rig headRig;
        [Range(120,360),Tooltip("This is used to determine how far the character can rotate to look at the direction the camera is facing. The value of each side is look value/2")]
        [SerializeField] float lookAngle = 210;
        private void Start()
        {
            AssignComponents();
            AssignVariables();
        }
        void AssignComponents()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            if(customAnimator!=null)
            {
                animator.runtimeAnimatorController = customAnimator;
            }
            cCam = GetComponentInChildren<CinemachineCamera>();
            cameraRoot = Camera.main.transform;
            defaultFov = cCam.Lens.FieldOfView;
            crouchedFov = cCam.Lens.FieldOfView-5;
        }
        void AssignVariables()
        {
            defaultHeight = characterController.height;
            defaultControllerCenter = characterController.center;
        }
        private void Update()
        {
            CheckGround();
            JumpAndGravity();
            PlayAnimation();
            ConstraintHead();
        }
        public override void Move(Vector2 moveInput, Vector2 lookInput, InputAction isRun, InputAction jumpInput, InputAction crouchInput)
        {
            PlayerLocomotion(moveInput, isRun);
            jumpInput.performed += JumpAndGravity;
            crouchInput.performed+=SwitchCrouch;
        }
        void PlayerLocomotion(Vector2 mInput, InputAction runAction)
        {
            Vector3 inputVector = new Vector3(mInput.x, 0, mInput.y);
            if (mInput.magnitude != 0 && isCrouched == false)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, runAction.IsPressed() ? runSpeed : walkSpeed, accelerationTime);
            }
            else if(mInput.magnitude != 0 && isCrouched)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, crouchedSpeed, accelerationTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, accelerationTime);
            }

            if (inputVector.magnitude != 0)
            {
                targetRotation = Mathf.Atan2(mInput.x, mInput.y) * Mathf.Rad2Deg + cameraRoot.eulerAngles.y;
                float smoothTurn = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref currentVelocity, smoothTurnTime);
                transform.rotation = Quaternion.Euler(0, smoothTurn, 0);
            }
            Vector3 targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;
            characterController.Move((inputVector.magnitude * targetDirection * currentSpeed * Time.deltaTime) + (verticalVelocity * Time.deltaTime));
        }
        void JumpAndGravity(InputAction.CallbackContext context = new InputAction.CallbackContext())
        {
            if (isGrounded)
            {
                if (jumpCooldown <= 0)
                {
                    jumpCooldown = 0;
                }
                else
                {
                    jumpCooldown -= Time.deltaTime;

                }
                if (context.performed)
                {
                    if (jumpCooldown <= 0)
                    {
                        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
                        footstepSource.clip = jumpClip;
                        footstepSource.Play();
                        isCrouched = false;
                        SwitchCrouch(new InputAction.CallbackContext());
                    }
                }

                if (verticalVelocity.y <= 0)
                {
                    verticalVelocity.y = -2f;
                }
            }
            else
            {
                jumpCooldown = jumpTimeout;
                if (verticalVelocity.y < terminalVelocity)
                {
                    verticalVelocity.y += gravity * Time.deltaTime;
                }
            }
        }
        void SwitchCrouch(InputAction.CallbackContext ctx)
        {
            if(ctx.performed && isGrounded)
            {
                isCrouched = !isCrouched;
            }

            if(isCrouched)
            {
                characterController.center = crouchedControllerCenter;
                characterController.height = crouchedHeight;
                cCam.Lens.FieldOfView = crouchedFov;
            }
            else
            {
                characterController.center = defaultControllerCenter;
                characterController.height = defaultHeight;
               cCam.Lens.FieldOfView =defaultFov;
            }
        }
        void ConstraintHead()
        {
            if (!isGrounded) return;
            Vector3 directionOfTarget = (headTarget.position - transform.position);
            if(Vector3.Angle(transform.forward,directionOfTarget)<=lookAngle/2)
            {
                headRig.weight = Mathf.Lerp(headRig.weight, 1, 0.4f);
            }
            else
            {
                print("Behind head");
                headRig.weight = Mathf.Lerp(headRig.weight, 0, 0.4f);
            }
        }
        void CheckGround()
        {
            isGrounded = characterController.isGrounded;
        }
        void PlayAnimation()
        {
            if (isGrounded == false)
            {
                animator.SetBool("InAir", true);
                return;
            }
            else if (isCrouched == false && isStrafe == false && isGrounded)
            {
                animator.SetFloat("Stance", Mathf.Lerp(animator.GetFloat("Stance"), 0, accelerationTime));
                animator.SetFloat("Base", currentSpeed / runSpeed);
                animator.SetBool("InAir", false);
            }
            else if (isCrouched && isStrafe == false && isGrounded)
            {
                animator.SetFloat("Stance", Mathf.Lerp(animator.GetFloat("Stance"), 1, accelerationTime));
                animator.SetFloat("Crouch", currentSpeed / crouchedSpeed);
                animator.SetBool("InAir", false);
            }
        }
        public void PlayFootstep()
        {
            if (currentSpeed < 0.1f || !isGrounded) return;

            footstepSource.clip = footstepClips[clipIndex];
            footstepSource.Play();
            if(clipIndex>=footstepClips.Length-1)
            {
                clipIndex = 0;
            }
            else
            {
                clipIndex++;
            }
        }
        public void PlayLandClip()
        {
            footstepSource.clip = landClip;
            footstepSource.Play();
        }
    }
}
