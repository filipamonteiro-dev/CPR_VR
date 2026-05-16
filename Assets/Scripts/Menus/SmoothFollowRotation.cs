using UnityEngine;

    public class SmoothFollowRotation : MonoBehaviour
    {
        [Header("References")]
        [Tooltip(" The transform of the player's camera/head.")]
        [SerializeField] private Transform playerHead;

        [Header("Settings")]
        [Tooltip("How fast the object moves to center.")]
        [SerializeField] private float smoothTime = 0.3f;

        [Header("Axes Configuration")]
        [Tooltip("If true, object rotates up and down.")]
        [SerializeField] private bool rotateVertical = true; // X Axis
        [Tooltip("If true, object rotates left and right.")]
        [SerializeField] private bool rotateHorizontal = true; // Y Axis

        [Header("Dead Zones (Degrees)")]
        [Tooltip("How far the player can look Up/Down before the UI moves.")]
        [SerializeField] private float verticalDeadZone = 10f;

        [Tooltip("How far the player can look Left/Right before the UI moves.")]
        [SerializeField] private float horizontalDeadZone = 15f;

        
        [Tooltip("Special behaviour for tutorial purposes.")]
        [SerializeField] public bool Tutorial = false;


        // Internal state for SmoothDamp
        private float _currentVelocityX;
        private float _currentVelocityY;

        // Internal state to track if we are currently correcting position
        private bool _isMovingX;
        private bool _isMovingY;

        // Threshold to consider the movement 'finished' so it stops jittering
        private const float StopThreshold = 0.5f;


        
        void Start()
        {
             if (Tutorial == true)
            {
                playerHead = Camera.main.transform;
            }
        }

        public void ForceFrontPosition()
        {
            transform.rotation = Quaternion.Euler(
                rotateVertical ? playerHead.eulerAngles.x : 0,
                rotateHorizontal ? playerHead.eulerAngles.y : 0F,
                0f);

        }

        
        private void LateUpdate()
        {
            if (playerHead == null) return;

            // 1. Rotation Logic
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler = playerHead.eulerAngles;

            // Calculate the ideal rotation based on configuration
            float nextX = currentEuler.x;
            float nextY = currentEuler.y;

            // --- Vertical Logic (X Axis) ---
            if (rotateVertical)
            {
                // Calculate difference between where we are and where the head is
                float diffX = Mathf.DeltaAngle(currentEuler.x, targetEuler.x);

                // If we aren't moving, check if we exceeded the dead zone
                if (!_isMovingX && Mathf.Abs(diffX) > verticalDeadZone)
                {
                    _isMovingX = true;
                }

                // If we are moving, smooth damp towards the target
                if (_isMovingX)
                {
                    nextX = Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref _currentVelocityX, smoothTime);

                    // Stop moving if we are very close to the center
                    if (Mathf.Abs(Mathf.DeltaAngle(nextX, targetEuler.x)) < StopThreshold)
                    {
                        _isMovingX = false;
                    }
                }
            }
            else
            {
                // If vertical rotation is disabled, force horizon (0) or keep current? 
                // Usually for HUDs, we want it flat on the horizon (0).
                // Change to 'nextX = currentEuler.x' if you want it to lock to world angle instead.
                nextX = Mathf.SmoothDampAngle(currentEuler.x, 0f, ref _currentVelocityX, smoothTime);
            }

            // --- Horizontal Logic (Y Axis) ---
            if (rotateHorizontal)
            {
                float diffY = Mathf.DeltaAngle(currentEuler.y, targetEuler.y);

                if (!_isMovingY && Mathf.Abs(diffY) > horizontalDeadZone)
                {
                    _isMovingY = true;
                }

                if (_isMovingY)
                {
                    nextY = Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref _currentVelocityY, smoothTime);

                    if (Mathf.Abs(Mathf.DeltaAngle(nextY, targetEuler.y)) < StopThreshold)
                    {
                        _isMovingY = false;
                    }
                }
            }

            // 2. Apply Rotation
            // We explicitly set Z to 0 to prevent tilting
            transform.rotation = Quaternion.Euler(nextX, nextY, 0f);
        }
    }
