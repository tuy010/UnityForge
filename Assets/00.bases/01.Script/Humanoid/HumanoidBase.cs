using System;
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
            Walk,
            Jump,
            Falling
        }
        #endregion

        #region Serialize Field
        [Header("Component")]
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController controller;
        [SerializeField] private Collider bottomCollider;

        [Header("Unit Info")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float gravityMultiplier;
        #endregion

        #region Protected Field
        protected Vector2 moveingDir;

        protected State nowState;
        protected State nextState { 
            get { return _nextState; }
            set { UpdateState(value); }
        }
        #endregion

        #region Private Field
        private State _nextState;
        private int groundLayer;

        private float airTimer = 0f;
        private bool isJump = false;
        private Vector3 playerVelocity = Vector3.zero;
        private const float gravity = -9.81f;
        #endregion

        #region Unity

        #endregion

        #region Public Methods
        public void JumpFunc()
        {
            if(nowState != State.Falling) isJump = true;
        }
        #endregion

        #region virtual Methods
        protected virtual void Init()
        {
            animator ??= GetComponent<Animator>();
            controller ??= GetComponent<CharacterController>();

            groundLayer = LayerMask.GetMask("Ground");
        }
        #endregion

        #region Protected Methods
        protected void UpdatePosition()
        {
            playerVelocity.x = 0f;
            playerVelocity.z = 0f;

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

            if(isJump)
            {
                nextState = State.Jump;
                playerVelocity.y = Mathf.Sqrt(jumpHeight*-2.0f*gravity*gravityMultiplier); 
                isJump = false;
            }


            if (moveingDir != Vector2.zero)
            {
                playerVelocity.x = moveingDir.x * speed;
                playerVelocity.z = moveingDir.y * speed;
                if (nowState == State.Idle) nextState = State.Walk;
                UpdateWalkingAnim();
            }
            else
            {
                if (nowState == State.Walk) nextState = State.Idle;
            }

            controller.Move(playerVelocity * Time.deltaTime);
        }
        protected void UpdateWalkingAnim()
        {
            animator.SetFloat("XDir", moveingDir.x);
            animator.SetFloat("YDir", moveingDir.y);
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
                    case State.Walk:
                        animator.SetBool("Walking", true);
                        _nextState = state;
                        break;
                    case State.Jump:
                        animator.SetTrigger("Jump");
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool("Falling", true);
                        _nextState = state;
                        break;
                    default:
                        return;
                }
            }
            else if(nowState == State.Walk)
            {
                switch (state)
                {
                    case State.Idle:
                        animator.SetBool("Walking", false);
                        _nextState = state;
                        break;
                    case State.Jump:
                        animator.SetTrigger("Jumping");
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool("Falling", true);
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
                        animator.SetBool("Walking", false);
                        _nextState = state;
                        break;
                    case State.Falling:
                        animator.SetBool("Falling", true);
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
                        animator.SetBool("Walking", false);
                        animator.SetBool("Falling", false);
                        _nextState = state;
                        break;
                }
            }

            nowState = _nextState;
        }

        private void OnAnimatorIK(int _layerIndex)
        {
            Vector3? leftGoal =  ApplyFootIK(AvatarIKGoal.LeftFoot);
            Vector3? rightRoal = ApplyFootIK(AvatarIKGoal.RightFoot);
        }
        private Vector3? ApplyFootIK(AvatarIKGoal foot)
        {
            Vector3 footPosition = animator.GetIKPosition(foot);
            RaycastHit hit;

            // 발 아래로 Ray를 쏴서 지면을 감지
            if (Physics.Raycast(footPosition+Vector3.up, Vector3.down, out hit, 1.5f, groundLayer))
            {
                Vector3 targetFootPosition = hit.point + new Vector3(0, 0.1f, 0);;
                Quaternion targetFootRotation = Quaternion.LookRotation(transform.forward, hit.normal);

                
                animator.SetIKPositionWeight(foot, 1);
                animator.SetIKRotationWeight(foot, 1);

                
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
