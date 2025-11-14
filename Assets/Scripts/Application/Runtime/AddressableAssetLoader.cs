using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Noname.Application.Runtime
{
    /// <summary>
    /// Addressables 자산 로드를 위한 공용 헬퍼.
    /// </summary>
    public static class AddressableAssetLoader
    {
        public static T LoadAssetSync<T>(AssetReference reference) where T : Object
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                Debug.LogError("AddressableAssetLoader: invalid asset reference.");
                return null;
            }

            var handle = reference.LoadAssetAsync<T>();
            if (!handle.IsDone)
            {
                handle.WaitForCompletion();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($"AddressableAssetLoader: failed to load asset '{reference.RuntimeKey}'. Status={handle.Status}");
            return null;
        }

        public static AsyncOperationHandle<T> LoadAssetHandleSync<T>(string address) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            if (!handle.IsDone)
            {
                handle.WaitForCompletion();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"AddressableAssetLoader: failed to load asset '{address}'. Status={handle.Status}");
            }

            return handle;
        }

        public static void ReleaseAsset(AssetReference reference)
        {
            if (reference != null && reference.RuntimeKeyIsValid())
            {
                reference.ReleaseAsset();
            }
        }

        public static void ReleaseHandle<T>(ref AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
                handle = default;
            }
        }
    }
}
