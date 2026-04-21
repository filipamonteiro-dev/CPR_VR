using UnityEngine;


    public class SmoothFollowPosition : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The transform to follow (e.g. player head or camera).")]
        [SerializeField] private Transform target;

        [Header("Settings")]
        [Tooltip("How fast the object moves to the target position.")]
        [SerializeField] private float smoothTime = 0.3f;

        [Header("Axes Configuration")]
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;
        [SerializeField] private bool followZ = true;

        [Tooltip("Special behaviour for tutorial purposes.")]
        [SerializeField] private bool Tutorial = false;

        [Tooltip("Extra Z offset to apply when Tutorial is enabled.")]
        [SerializeField] private float tutorialZOffset = 0.5f;

      


        [Header("Dead Zones (Meters)")]
        [Tooltip("How far the target can move on X before this object follows.")]
        [SerializeField] private float deadZoneX = 0.05f;

        [Tooltip("How far the target can move on Y before this object follows.")]
        [SerializeField] private float deadZoneY = 0.05f;

        [Tooltip("How far the target can move on Z before this object follows.")]
        [SerializeField] private float deadZoneZ = 0.05f;

        // SmoothDamp velocities
        private float _velocityX;
        private float _velocityY;
        private float _velocityZ;

        // Movement state
        private bool _isMovingX;
        private bool _isMovingY;
        private bool _isMovingZ;


        // Stop threshold to avoid jitter
        private const float StopThreshold = 0.01f;

        void Start()
        {
             if (Tutorial == true)
            {
                target = Camera.main.transform;
            }
        }
        public void ForceTargetPosition()
        {
           
            if (target == null) return;

            transform.position = target.position;
            _isMovingX = _isMovingY = _isMovingZ = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 current = transform.position;
            Vector3 targetPos = target.position;

            if (Tutorial)
            {
                targetPos.z += tutorialZOffset;
            }

            float nextX = current.x;
            float nextY = current.y;
            float nextZ = current.z;

            // --- X Axis ---
            if (followX)
            {
                float diff = targetPos.x - current.x;

                if (!_isMovingX && Mathf.Abs(diff) > deadZoneX)
                    _isMovingX = true;

                if (_isMovingX)
                {
                    nextX = Mathf.SmoothDamp(current.x, targetPos.x, ref _velocityX, smoothTime);

                    if (Mathf.Abs(nextX - targetPos.x) < StopThreshold)
                        _isMovingX = false;
                }
            }

            // --- Y Axis ---
            if (followY)
            {
                float diff = targetPos.y - current.y;

                if (!_isMovingY && Mathf.Abs(diff) > deadZoneY)
                    _isMovingY = true;

                if (_isMovingY)
                {
                    nextY = Mathf.SmoothDamp(current.y, targetPos.y, ref _velocityY, smoothTime);

                    if (Mathf.Abs(nextY - targetPos.y) < StopThreshold)
                        _isMovingY = false;
                }
            }

            // --- Z Axis ---
            if (followZ)
            {
                float diff = targetPos.z - current.z;

                if (!_isMovingZ && Mathf.Abs(diff) > deadZoneZ)
                    _isMovingZ = true;

                if (_isMovingZ)
                {
                    nextZ = Mathf.SmoothDamp(current.z, targetPos.z, ref _velocityZ, smoothTime);

                    if (Mathf.Abs(nextZ - targetPos.z) < StopThreshold)
                        _isMovingZ = false;
                }
            }

            transform.position = new Vector3(nextX, nextY, nextZ);
        }
    }

