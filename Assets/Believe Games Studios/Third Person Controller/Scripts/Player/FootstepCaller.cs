using UnityEngine;

namespace Believe.Games.Studios
{
    public class FootstepCaller : MonoBehaviour
    {
        PlayerMovement playerMovement;
        private void OnEnable()
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
        public void OnFootstep()
        {
            playerMovement.PlayFootstep();
        }
        public void OnLand()
        {
            playerMovement.PlayLandClip();
        }
    }
}

