using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// 사운드 재생을 중앙에서 관리한다. 현재는 호출 로그만 남긴다.
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        public void PlaySound(string soundId)
        {
            Debug.Log($"[SFX] Play '{soundId}'");
        }

        public void PlaySound(string soundId, Vector3 worldPosition)
        {
            Debug.Log($"[SFX] Play '{soundId}' at {worldPosition}");
        }

        public void StopSound(string soundId)
        {
            Debug.Log($"[SFX] Stop '{soundId}'");
        }

        public void StopAll()
        {
            Debug.Log("[SFX] Stop all");
        }
    }
}
