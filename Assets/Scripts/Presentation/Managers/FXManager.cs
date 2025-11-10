using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// 연출용 파티클/후광 효과를 한곳에서 관리합니다.
    /// 현재는 로그만 남기지만 실제 FX 시스템으로 교체하기 쉽도록 추상화돼 있습니다.
    /// </summary>
    public sealed class FXManager : MonoBehaviour
    {
        /// <summary>
        /// 자원 드롭 수집 위치에 FX를 재생합니다.
        /// </summary>
        public void PlayResourcePickupFx(Vector3 worldPosition, ResourceDropType type)
        {
            Debug.Log($"[FX] Resource pickup FX ({type}) at {worldPosition}");
        }

        /// <summary>
        /// 투사체 충돌 위치에 FX를 재생합니다.
        /// </summary>
        public void PlayProjectileImpactFx(Vector3 worldPosition, ProjectileFaction faction)
        {
            Debug.Log($"[FX] Projectile impact FX ({faction}) at {worldPosition}");
        }

        /// <summary>
        /// 어빌리티 선택 UI가 열리거나 닫힐 때 FX를 재생합니다.
        /// </summary>
        public void PlayAbilitySelectionFx(bool opening)
        {
            Debug.Log($"[FX] Ability selection {(opening ? "open" : "close")} FX");
        }
    }
}
