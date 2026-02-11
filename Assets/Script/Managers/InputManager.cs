using UnityEngine;
using System;

namespace TPSShooter.Managers
{
    /// <summary>
    /// Manages user input and provides an abstraction layer for input handling.
    /// This allows for easier key remapping and AI agent control.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Key Bindings")]
        public KeyCode reloadKey = KeyCode.R;
        public KeyCode jumpKey = KeyCode.Space;

        // AI Control Override
        private bool isAIControlled = false;
        private Vector2 aiMovementInput;
        private Vector2 aiLookInput;
        private bool aiFireInput;
        private bool aiReloadInput;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetAIControl(bool active)
        {
            isAIControlled = active;
        }

        public void SetAIInputs(Vector2 movement, Vector2 look, bool fire, bool reload)
        {
            if (!isAIControlled) return;
            aiMovementInput = movement;
            aiLookInput = look;
            aiFireInput = fire;
            aiReloadInput = reload;
        }

        public float GetHorizontalInput()
        {
            if (isAIControlled) return aiMovementInput.x;
            return Input.GetAxis("Horizontal");
        }

        public float GetVerticalInput()
        {
            if (isAIControlled) return aiMovementInput.y;
            return Input.GetAxis("Vertical");
        }

        public float GetMouseX()
        {
            if (isAIControlled) return aiLookInput.x;
            return Input.GetAxis("Mouse X");
        }

        public float GetMouseY()
        {
            if (isAIControlled) return aiLookInput.y;
            return Input.GetAxis("Mouse Y");
        }

        public bool GetFireInput()
        {
            if (isAIControlled) return aiFireInput;
            return Input.GetMouseButton(0);
        }

        public bool GetReloadInputDown()
        {
            if (isAIControlled) return aiReloadInput; // Note: This might need edge detection for "Down" in real usage
            return Input.GetKeyDown(reloadKey);
        }

        public bool GetJumpInputDown()
        {
            if (isAIControlled) return false;
            return Input.GetKeyDown(jumpKey);
        }
    }
}
