using UnityEngine;

namespace Noname.Application.Runtime
{
    /// <summary>
    /// CoreRuntime를 포함해 향후 확장될 초기화 루틴을 묶어두는 엔트리 포인트.
    /// </summary>
    internal static class RuntimeInitialization
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 는 여기서만 정의한다. 순서 보장이 되지 않기때문에
        //씬에 관계 없이 필요한 초기화 로직을 여기에 추가한다.
        private static void Initialize()
        {
            CoreRuntime.Initialize();
        }
    }
}
