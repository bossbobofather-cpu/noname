using System.Collections.Generic;
using UnityEngine;

namespace Noname.Presentation.Utilities
{
    /// <summary>
    /// 프리팹 이름 기반으로 각 풀을 생성/관리하고, 인스턴스-풀 매핑까지 추적하는 레지스트리입니다.
    /// </summary>
    public sealed class ComponentPoolRegistry<T> where T : Component
    {
        private readonly Dictionary<string, ComponentPool<T>> _pools = new Dictionary<string, ComponentPool<T>>();
        private readonly Dictionary<T, string> _instanceLookup = new Dictionary<T, string>();

        /// <summary>
        /// 프리팹과 키(없으면 프리팹 이름)를 기반으로 인스턴스를 가져옵니다.
        /// </summary>
        public T GetInstance(T prefab, Transform parent, string keyOverride = null)
        {
            var key = ResolveKey(prefab, keyOverride);
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var pool = GetOrCreatePool(key, prefab, parent);
            if (pool == null)
            {
                return null;
            }

            var instance = pool.Get();
            if (instance != null)
            {
                _instanceLookup[instance] = key;
            }

            return instance;
        }

        /// <summary>
        /// 사용이 끝난 인스턴스를 원래 풀에 반환합니다.
        /// </summary>
        public void ReleaseInstance(T instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_instanceLookup.TryGetValue(instance, out var key) && _pools.TryGetValue(key, out var pool))
            {
                pool.Release(instance);
            }
            else
            {
                Object.Destroy(instance.gameObject);
            }

            _instanceLookup.Remove(instance);
        }

        /// <summary>
        /// 모든 풀을 제거하고 캐시를 비웁니다.
        /// </summary>
        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _instanceLookup.Clear();
        }

        private ComponentPool<T> GetOrCreatePool(string key, T prefab, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = new ComponentPool<T>(prefab, parent);
                _pools.Add(key, pool);
            }

            return pool;
        }

        private static string ResolveKey(T prefab, string keyOverride)
        {
            if (!string.IsNullOrEmpty(keyOverride))
            {
                return keyOverride;
            }

            if (prefab == null)
            {
                return null;
            }

            var name = prefab.gameObject != null ? prefab.gameObject.name : prefab.name;
            if (string.IsNullOrEmpty(name))
            {
                name = prefab.GetInstanceID().ToString();
            }

            return name;
        }
    }
}
