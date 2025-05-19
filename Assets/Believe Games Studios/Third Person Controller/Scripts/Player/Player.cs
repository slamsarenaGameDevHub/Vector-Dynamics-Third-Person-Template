using UnityEngine;
using UnityEngine.InputSystem;

namespace Believe.Games.Studios
{
    public class Player : MonoBehaviour
    {
        public virtual void Move(Vector2 moveInput, Vector2 lookInput, InputAction isRun, InputAction jumpAction, InputAction crouchAction)
        {

        }
    }
}
