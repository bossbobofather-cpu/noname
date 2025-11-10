using System.Collections;
using UnityEngine;

namespace Noname.Presentation.Views
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private float impactEffectLifetime = 1f;

        private Vector3 _targetPosition;
        private float _moveSpeed = 10f;
        private bool _isActive;

        public void Launch(Vector3 origin, Vector3 target, float speed)
        {
            transform.position = origin;
            _targetPosition = target;
            _moveSpeed = Mathf.Max(0.1f, speed);
            _isActive = true;
        }

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
