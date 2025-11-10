using System.Collections;
using UnityEngine;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 투사체 이동과 충돌 연출을 담당하는 뷰입니다.
    /// </summary>
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private float impactEffectLifetime = 1f;

        private Vector3 _targetPosition;
        private float _moveSpeed = 10f;
        private bool _isActive;

        /// <summary>
        /// 투사체를 발사 위치/목표/속도로 초기화합니다.
        /// </summary>
        public void Launch(Vector3 origin, Vector3 target, float speed)
        {
            transform.position = origin;
            _targetPosition = target;
            _moveSpeed = Mathf.Max(0.1f, speed);
            _isActive = true;
        }

        /// <summary>
        /// 즉시 특정 위치로 이동시킵니다.
        /// </summary>
        public void SnapTo(Vector3 position)
        {
            transform.position = position;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            var step = _moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);
        }

        /// <summary>
        /// 충돌 처리 후 FX를 재생하고 뷰를 파괴합니다.
        /// </summary>
        public void Complete(float explosionRadius)
        {
            _isActive = false;
            if (impactEffectPrefab != null)
            {
                var effect = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity, transform.parent);
                if (explosionRadius > 0f)
                {
                    var scale = explosionRadius * 2f;
                    effect.transform.localScale = new Vector3(scale, scale, scale);
                }

                if (impactEffectLifetime > 0f)
                {
                    Destroy(effect, impactEffectLifetime);
                }
            }

            Destroy(gameObject);
        }
    }
}
