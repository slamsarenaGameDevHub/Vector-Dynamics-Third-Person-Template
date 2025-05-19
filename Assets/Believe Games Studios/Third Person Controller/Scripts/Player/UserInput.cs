using UnityEngine;
using UnityEngine.InputSystem;

namespace Believe.Games.Studios
{
    public class UserInput : MonoBehaviour
    {
        InputSystem_Actions inputHandler;
        Player player;
        private void OnEnable()
        {
            inputHandler = new InputSystem_Actions();
            inputHandler.Player.Enable();
            player = GetComponent<PlayerMovement>();
        }
        private void OnDisable()
        {
            inputHandler.Player.Disable();
        }
        private void Update()
        {
            Vector2 moveInput = inputHandler.Player.Move.ReadValue<Vector2>();
            Vector2 lookInput = inputHandler.Player.Look.ReadValue<Vector2>();
            InputAction runInput = inputHandler.Player.Sprint;
            InputAction jumpInput = inputHandler.Player.Jump;
            InputAction crouchInput = inputHandler.Player.Crouch;
            player.Move(moveInput, lookInput, runInput, jumpInput, crouchInput);
        }
    }
}
