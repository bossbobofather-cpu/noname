using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// 2D FX 연출을 중앙에서 트리거하는 매니저. 현재는 로그만 남긴다.
    /// </summary>
    public sealed class FXManager : MonoBehaviour
    {
        public void PlayFx(string fxName)
        {
            LogFx(fxName, "no position", "no parent");
        }

        public void PlayFx(string fxName, Vector2 worldPosition)
        {
            PlayFx(fxName, new Vector3(worldPosition.x, worldPosition.y, 0f));
        }

        public void PlayFx(string fxName, Vector3 worldPosition)
        {
            LogFx(fxName, $"world:{worldPosition}", "no parent");
        }

        public void PlayFx(string fxName, Transform parent)
        {
            var parentName = parent != null ? parent.name : "<null>";
            LogFx(fxName, "inherit parent position", parentName);
        }

        public void PlayFx(string fxName, Vector3 worldPosition, Transform parent)
        {
            var parentName = parent != null ? parent.name : "<null>";
            LogFx(fxName, $"world:{worldPosition}", parentName);
        }

        private static void LogFx(string fxName, string position, string parent)
        {
            Debug.Log($"[FX] Play '{fxName}' ({position}) parent:{parent}");
        }
    }
}
