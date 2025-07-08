using UnityEngine;
using UnityEngine.InputSystem;

namespace Tuy.UnityForge.Base
{
    public class HumanoidControl : HumanoidBase
    {
        #region Serialize Field
        [Header("Control Option")]
        [SerializeField] bool holdCrouch;
        [SerializeField] bool holdRun;
        #endregion
        #region Unity
        void Start()
        {
            Init();
        }

        void Update()
        {
            UpdateInfo();
        }
        #endregion

        #region public Methods
        public void InputAction_GetMovingDirInput(InputAction.CallbackContext value)
        {
            moveingDir = value.ReadValue<Vector2>();
        }
        public void InputAction_Jump(InputAction.CallbackContext value)
        {
            if (!value.started) return;
            JumpFunc();
        }
        public void InputAction_Crouch(InputAction.CallbackContext value)
        {
            if (!holdCrouch)
            {
                if (!value.started) return;
                CrouchFunc();
            }
            else
            {
                if (value.started) CrouchFunc(true);
                else if (value.canceled) CrouchFunc(false);
                else return;
            }
        }
        public void InputAction_Run(InputAction.CallbackContext value)
        {
            if(!holdRun)
            {
                if (!value.started) return;
                RunFunc();
            }
            else
            {
                if(value.started) RunFunc(true);
                else if (value.canceled) RunFunc(false);
                else return;
            }
        }
        #endregion
    }
}

