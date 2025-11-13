using System.Collections.Generic;
using UnityEngine;

namespace Noname.Presentation.Utilities
{
    /// <summary>
    /// 단일 프리팹을 미리 생성해두고 재사용하기 위한 간단한 컴포넌트 풀입니다.
    /// </summary>
    /// <typeparam name="T">Pooling 대상 컴포넌트 타입</typeparam>
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _cache = new Stack<T>();

        public ComponentPool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        /// <summary>
        /// 비활성화된 인스턴스를 가져오거나 없으면 새로 생성합니다.
        /// </summary>
        public T Get()
        {
            T instance = _cache.Count > 0 ? _cache.Pop() : Object.Instantiate(_prefab, _parent);
            if (instance == null)
            {
                return null;
            }

            instance.transform.SetParent(_parent, false);
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// 사용을 마친 인스턴스를 풀에 반환합니다.
        /// </summary>
        public void Release(T instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_parent, false);
            _cache.Push(instance);
        }

        public void Clear()
        {
            while (_cache.Count > 0)
            {
                var item = _cache.Pop();
                if (item != null)
                {
                    Object.Destroy(item.gameObject);
                }
            }
        }
    }
}
