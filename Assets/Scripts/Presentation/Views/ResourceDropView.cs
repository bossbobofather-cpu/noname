using System;
using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 자원 드롭의 시각적 상태(대기, 플레이어에게 이동)를 표현합니다.
    /// </summary>
    public sealed class ResourceDropView : MonoBehaviour
    {
        [Header("Idle Motion")]
        [SerializeField] private float bobAmplitude = 0.1f;
        [SerializeField] private float bobFrequency = 2f;

        [Header("Pickup Motion")]
        [SerializeField] private float pickupDuration = 0.6f;
        [SerializeField] private float pickupArcHeight = 0.5f;
        [SerializeField] private AnimationCurve pickupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private int _dropId;
        private bool _isTravelling;
        private Vector3 _idleOrigin;
        private float _idleTimer;
        private Transform _travelTarget;
        private Vector3 _travelStart;
        private Vector3 _travelControl;
        private float _travelTimer;
        private Action<int> _onTravelCompleted;

        /// <summary>
        /// 드롭 ID를 설정하고 초기 위치를 기록합니다.
        /// </summary>
        public void Initialize(int dropId, ResourceDropType type)
        {
            _dropId = dropId;
            _idleOrigin = transform.position;
        }

        private void Update()
        {
            if (_isTravelling)
            {
                UpdateTravel();
            }
            else
            {
                _idleTimer += Time.deltaTime;
                var offset = Mathf.Sin(_idleTimer * bobFrequency) * bobAmplitude;
                transform.position = _idleOrigin + new Vector3(0f, offset, 0f);
            }
        }

        /// <summary>
        /// 플레이어 앵커를 향해 이동을 시작하고 완료 시 콜백을 호출합니다.
        /// </summary>
        public void BeginPickupTravel(Transform target, Action<int> onCompleted)
        {
            if (target == null)
            {
                onCompleted?.Invoke(_dropId);
                return;
            }

            _travelTarget = target;
            _onTravelCompleted = onCompleted;
            _travelStart = transform.position;
            _travelTimer = 0f;
            _idleOrigin = transform.position;

            var midPoint = Vector3.Lerp(_travelStart, target.position, 0.5f);
            _travelControl = midPoint + Vector3.up * pickupArcHeight;
            _isTravelling = true;
        }

        private void UpdateTravel()
        {
            if (_travelTarget == null)
            {
                CompleteTravel();
                return;
            }

            _travelTimer += Time.deltaTime;
            var t = Mathf.Clamp01(_travelTimer / Mathf.Max(0.01f, pickupDuration));
            var curveT = pickupCurve != null ? pickupCurve.Evaluate(t) : t;
            var endPoint = _travelTarget.position;
            var a = Vector3.Lerp(_travelStart, _travelControl, curveT);
            var b = Vector3.Lerp(_travelControl, endPoint, curveT);
            transform.position = Vector3.Lerp(a, b, curveT);

            if (t >= 1f)
            {
                CompleteTravel();
            }
        }

        private void CompleteTravel()
        {
            _isTravelling = false;
            _onTravelCompleted?.Invoke(_dropId);
        }

    }
}
