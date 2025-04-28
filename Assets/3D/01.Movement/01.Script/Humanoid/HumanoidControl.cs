using UnityEngine;
using UnityEngine.InputSystem;

namespace Tuy.UnityForge.Base
{
    public class HumanoidControl : HumanoidBase
    {
        #region Unity
        void Start()
        {
            Init();
        }

        void Update()
        {
            UpdatePosition();
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
        #endregion
    }
}

