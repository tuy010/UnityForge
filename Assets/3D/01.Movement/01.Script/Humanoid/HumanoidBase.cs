using System;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tuy.UnityForge.Base
{
    public class HumanoidBase : MonoBehaviour
    {
        #region Enum
        protected enum State
        {
            Idle,
            Move,
            Jump,
            Falling
        }
        protected enum animationParam
        {
            XDir,
            YDir,
            Jump,
            Fall,
            Move,
            Crouch,
            Run
        }
        #endregion

        #region Serialize Field
        [Header("Component")]
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController controller;
        [SerializeField] private Collider bottomCollider;

        [Header("IK")]
        [SerializeField] private float footIKHeight;
        [SerializeField] private Transform headPivot;
        [SerializeField] private Transform baseAimPoint;


        [Header("Unit Info")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float controllerCrouchHeight;
        #endregion

        #region Protected Field
        protected Vector2 moveingDir;
        protected bool isCrouch;
        protected bool isRunning;

        protected Transform aimPoint;

        protected State nowState;
        protected State nextState { 
            get { return _nextState; }
            set { UpdateState(value); }
        }

        readonly Dictionary<animationParam, string> animationParams = new Dictionary<animationParam, string>{
            {animationParam.XDir, "XDir"},
            {animationParam.YDir, "YDir"},
            {animationParam.Jump, "Jump"},
            {animationParam.Fall, "Fall"},
            {animationParam.Move, "Move"},
            {animationParam.Run, "Run"}
        };
        
        #endregion

        #region Private Field
        private State _nextState;
        private int groundLayer;

        private float airTimer = 0f;
        private bool isJump = false;
        private Vector3 playerVelocity = Vector3.zero;
        private const float gravity = -9.81f;

        private Vector3 controllerCenter => new Vector3 (0, ( isCrouch ? controllerCrouchHeight : controllerHeight)/ 2, 0);
        private float controllerHeight;

        #endregion

        #region Unity

        #endregion

        #region Public Methods
        /// <summary>
        /// Call To Start Jump
        /// </summary>
        public void JumpFunc()
        {
            if (nowState == State.Falling || isCrouch) return;

            isJump = true;
            return;
        }

        /// <summary>
        /// Call To Start/Stop Crouch
        /// </summary>
        /// <param name="value">Use Null to Toggle action</param>
        public void CrouchFunc(bool? value = null)
        {
            if(nowState == State.Falling) return;

            isCrouch = value != null? value.Value : !isCrouch;
            animator.SetBool(animationParams[animationParam.Crouch], isCrouch);

            if(isCrouch&&isRunning)
            {
                isRunning = false;
                animator.SetBool(animationParams[animationParam.Run], false);
            }
            return;
        }

        /// <summary>
        /// Call To Start/Stop Run
        /// </summary>
        /// <param name="value">Use Null to Toggle action</param>
        public void RunFunc(bool? value = null) 
        {
            if (nowState == State.Falling) return;          
            isRunning = value != null ? value.Value : !isRunning;
            animator.SetBool(animationParams[animationParam.Run], isRunning);

            if(isRunning && isCrouch)
            {
                isCrouch = false;
                animator.SetBool(animationParams[animationParam.Crouch], false);
            }
            return;
        }
        #endregion

        #region virtual Methods
        protected virtual void Init()
        {
            animator ??= GetComponent<Animator>();
            controller ??= GetComponent<CharacterController>();

            groundLayer = LayerMask.GetMask("Ground");

            controllerHeight = controller.height;
        }
        #endregion

        #region Protected Methods
        protected void UpdateInfo()
        {
            playerVelocity.x = 0f;
            playerVelocity.z = 0f;

            //Ground Check
            bool isGrounded = controller.isGrounded;
            if (!isGrounded)
            {
                //if(playerVelocity.y == -0.1f) playerVelocity.y = 0f;
                playerVelocity.y += gravity * Time.deltaTime * gravityMultiplier;
                if (nowState != State.Falling) airTimer += Time.deltaTime;
                if (airTimer > 0.5f) nextState = State.Falling;
            }
            else if (isGrounded && playerVelocity.y < 0.0f)
            {
                playerVelocity.y = -1f;
                airTimer = 0;
                nextState = State.Idle;
            }

            //Jump Check
            if(isJump)
            {
                nextState = State.Jump;
                playerVelocity.y = Mathf.Sqrt(jumpHeight*-2.0f*gravity*gravityMultiplier); 
                isJump = false;
                nextState = State.Falling;
            }

            //moving Check
            if (moveingDir != Vector2.zero)
            {
                float moveSpeed = new System.Func<float>(() => {
                    if (isCrouch) return crouchSpeed;
                    if (isRunning) return runSpeed;
                    return walkSpeed;
                })();

                playerVelocity.x = moveingDir.x * moveSpeed;
                playerVelocity.z = moveingDir.y * moveSpeed;
                if (nowState == State.Idle) nextState = State.Move;
                UpdateWalkingAnim();
            }
            else
            {
                if (nowState == State.Move) nextState = State.Idle;
            }
            controller.Move(playerVelocity * Time.deltaTime);

        }
        protected void UpdateWalkingAnim()
        {
            animator.SetFloat(animationParams[animationParam.XDir], moveingDir.x);
            animator.SetFloat(animationParams[animationParam.YDir], moveingDir.y);
        }
        #endregion

        #region Private Methods
        private void UpdateState(State state)
        {
            if (nowState == state) return;

            if(nowState == State.Idle)
            {
                switch (state)
                {
                    case State.Move:
                        animator.SetBool(animationParams[animationParam.Move], true);
                        _nextState = state;
                        break;
                    case State.Jump:
                        animator.SetTrigger(animationParams[animationParam.Jump]);
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool(animationParams[animationParam.Fall], true);
                        _nextState = state;
                        break;
                    default:
                        return;
                }
            }
            else if(nowState == State.Move)
            {
                switch (state)
                {
                    case State.Idle:
                        animator.SetBool(animationParams[animationParam.Move], false);
                        _nextState = state;
                        break;
                    case State.Jump:
                        animator.SetTrigger(animationParams[animationParam.Jump]);
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool(animationParams[animationParam.Fall], true);
                        _nextState = state;
                        break;
                    default:
                        return;
                }
            }
            else if(nowState == State.Jump)
            {
                switch (state)
                {
                    case State.Idle:
                        animator.SetBool(animationParams[animationParam.Move], false);
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool(animationParams[animationParam.Fall], true);
                        _nextState = state;
                        break;
                    default:
                        return;
                }
            }
            else if(nowState == State.Falling)
            {
                switch (state)
                {
                    case State.Idle:
                        animator.SetBool(animationParams[animationParam.Move], false);
                        animator.SetBool(animationParams[animationParam.Fall], false);
                        _nextState = state;
                        break;
                }
            }

            nowState = _nextState;
        }

        private void OnAnimatorIK(int _layerIndex)
        {
            float footWeightValue = (playerVelocity.x == 0 && playerVelocity.z == 0) ? 1 : 0.1f;
            Vector3? leftGoal =  ApplyFootIK(AvatarIKGoal.LeftFoot, footWeightValue);
            Vector3? rightGoal = ApplyFootIK(AvatarIKGoal.RightFoot, footWeightValue);
            if ( leftGoal != null && rightGoal != null && !isCrouch)
            {
                float HeightGap = Mathf.Abs(leftGoal.Value.y - rightGoal.Value.y);
                controller.height = Math.Clamp(controllerHeight - HeightGap, controllerCrouchHeight, controllerHeight);
                controller.center = controllerCenter + new Vector3 (0, HeightGap/2, 0);
            }
            else
            {
                controller.height = isCrouch ? controllerCrouchHeight : controllerHeight;
                controller.center = controllerCenter;
            }
            
            if(aimPoint != null)
            {
                animator.SetLookAtPosition(aimPoint.position);
                animator.SetLookAtWeight(1);
            }
            
        }
        private Vector3? ApplyFootIK(AvatarIKGoal foot, float weightValue)
        {
            Vector3 footPosition = animator.GetIKPosition(foot);
            RaycastHit hit;

            if (Physics.Raycast(footPosition+new Vector3(0, footIKHeight, 0), Vector3.down, out hit, footIKHeight*2, groundLayer))
            {
                Vector3 targetFootPosition = hit.point + new Vector3(0, 0.1f, 0);;
                Quaternion targetFootRotation = Quaternion.LookRotation(transform.forward, hit.normal);

                
                animator.SetIKPositionWeight(foot, weightValue);
                animator.SetIKRotationWeight(foot, weightValue);

                
                animator.SetIKPosition(foot, targetFootPosition);
                animator.SetIKRotation(foot, targetFootRotation);

                return targetFootPosition;
            }
            else
            {
                animator.SetIKPositionWeight(foot, 0);
                animator.SetIKRotationWeight(foot, 0);
                return null;
            }
        }
        #endregion
    }
}
