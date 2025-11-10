using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Presentation.Managers
{
    public sealed class SoundManager : MonoBehaviour
    {
        public void PlayResourcePickupSound(ResourceDropType type)
        {
            Debug.Log($"[SFX] Resource pickup sound ({type})");
        }

        public void PlayAbilitySelectionSound(bool opening)
        {
            Debug.Log($"[SFX] Ability selection {(opening ? "open" : "close")} sound");
        }
    }
}
