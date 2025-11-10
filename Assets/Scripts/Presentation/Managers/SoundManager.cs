using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// 게임 내 사운드 큐를 한곳에서 트리거하는 매니저입니다.
    /// 현재는 로그만 출력하지만 실제 오디오 시스템으로 쉽게 교체할 수 있습니다.
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        /// <summary>
        /// 자원 드롭 수집 음향을 재생합니다.
        /// </summary>
        public void PlayResourcePickupSound(ResourceDropType type)
        {
            Debug.Log($"[SFX] Resource pickup sound ({type})");
        }

        /// <summary>
        /// 어빌리티 선택 UI가 열리거나 닫힐 때 음향을 재생합니다.
        /// </summary>
        public void PlayAbilitySelectionSound(bool opening)
        {
            Debug.Log($"[SFX] Ability selection {(opening ? "open" : "close")} sound");
        }
    }
}
