using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Presentation.Managers
{
    public sealed class FXManager : MonoBehaviour
    {
        public void PlayResourcePickupFx(Vector3 worldPosition, ResourceDropType type)
        {
            Debug.Log($"[FX] Resource pickup FX ({type}) at {worldPosition}");
        }

        public void PlayProjectileImpactFx(Vector3 worldPosition, ProjectileFaction faction)
        {
            Debug.Log($"[FX] Projectile impact FX ({faction}) at {worldPosition}");
        }

        public void PlayAbilitySelectionFx(bool opening)
        {
            Debug.Log($"[FX] Ability selection {(opening ? "open" : "close")} FX");
        }
    }
}
