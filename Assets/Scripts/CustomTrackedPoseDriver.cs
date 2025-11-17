using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.InputSystem.XR
{
    // Inherit so we integrate with the Input System TrackedPoseDriver lifecycle.
    public class CustomTrackedPoseDriver : TrackedPoseDriver
    {
        [Header("Custom Rotation Options")]
        [Tooltip("If true, only apply the HMD yaw (ignore pitch/roll).")]
        public bool yawOnly = true;

        [Tooltip("Apply this yaw (degrees) as an additional offset.")]
        public float yawOffsetDegrees = 0f;

        [Header("Controller Look (Joystick)")]
        [Tooltip("Input action (Vector2) used for looking around (joystick). Leave empty to not use joystick look.")]
        public InputActionProperty lookInput;

        [Tooltip("Enable controller-based looking.")]
        public bool enableLook = true;

        [Tooltip("Degrees per second applied when joystick is at full deflection.")]
        public float lookSensitivity = 90f;

        [Tooltip("Smoothing time for look movement (seconds). Lower = snappier.")]
        [Range(0.001f, 1f)]
        public float lookSmoothTime = 0.12f;

        [Tooltip("Ignore small joystick noise under this magnitude.")]
        [Range(0f, 0.5f)]
        public float lookDeadzone = 0.12f;

        [Tooltip("Invert vertical axis for look input.")]
        public bool invertY = true;

        [Tooltip("Allow pitch changes from joystick look.")]
        public bool allowPitch = false;

        [Tooltip("Minimum pitch allowed (degrees).")]
        public float minPitch = -80f;

        [Tooltip("Maximum pitch allowed (degrees).")]
        public float maxPitch = 80f;

        // internal state for smoothing and accumulation
        float m_LookOffsetYaw = 0f;
        float m_LookOffsetPitch = 0f;

        float m_AppliedYaw = 0f;
        float m_AppliedPitch = 0f;

        float m_YawVelocity = 0f;
        float m_PitchVelocity = 0f;

        bool m_FirstUpdate = true;
        bool m_LookBound = false;

        // Unity calls both base and derived OnEnable/OnDisable; hide base to add look binding.
        protected new void OnEnable()
        {
            base.OnEnable();
            BindLook();
            m_FirstUpdate = true;
        }

        protected new void OnDisable()
        {
            UnbindLook();
            base.OnDisable();
        }

        void BindLook()
        {
            var action = lookInput.action;
            if (action == null)
                return;
            // If this is a locally-created action (no external reference), ensure enabled like TrackedPoseDriver does.
            if (lookInput.reference == null)
            {
#if UNITY_EDITOR
                // keep naming behavior simple (no editor analytic)
                action.Rename($"{gameObject.name} - TPD - Look");
#endif
                action.Enable();
            }
            m_LookBound = true;
        }

        void UnbindLook()
        {
            if (!m_LookBound)
                return;

            var action = lookInput.action;
            if (action == null)
                return;

            if (lookInput.reference == null)
                action.Disable();

            m_LookBound = false;
        }

        protected override void PerformUpdate()
        {
            // Read current raw pose values from bound actions (fall back to transform if not available).
            Vector3 newPos = transform.localPosition;
            Quaternion newRot = transform.localRotation;

            var posAction = positionInput.action;
            if (posAction != null && posAction.enabled)
                newPos = posAction.ReadValue<Vector3>();

            var rotAction = rotationInput.action;
            if (rotAction != null && rotAction.enabled)
                newRot = rotAction.ReadValue<Quaternion>();

            // base orientation angles (in degrees)
            var baseEuler = newRot.eulerAngles;
            float baseYaw = Mathf.DeltaAngle(0f, baseEuler.y) + yawOffsetDegrees;
            float basePitch = yawOnly ? 0f : Mathf.DeltaAngle(0f, baseEuler.x);
            float baseRoll = yawOnly ? 0f : baseEuler.z;

            // Handle joystick look input if enabled
            float dt = Time.deltaTime;
            if (enableLook && lookInput.action != null && lookInput.action.enabled)
            {
                Vector2 raw = lookInput.action.ReadValue<Vector2>();

                // Apply deadzone
                if (raw.magnitude < lookDeadzone)
                    raw = Vector2.zero;

                // Axis mapping: x -> yaw (horizontal), y -> pitch (vertical)
                float inputX = raw.x;
                float inputY = raw.y * (invertY ? -1f : 1f);

                // Integrate offsets based on sensitivity (degrees per second)
                m_LookOffsetYaw += inputX * lookSensitivity * dt;
                if (allowPitch)
                    m_LookOffsetPitch += inputY * lookSensitivity * dt;

                // Keep pitch in sensible range relative to 0
                m_LookOffsetPitch = Mathf.Clamp(m_LookOffsetPitch, minPitch - basePitch, maxPitch - basePitch);
            }

            // Compute targets (base orientation + look offsets)
            float targetYaw = baseYaw + m_LookOffsetYaw;
            float targetPitch = basePitch + m_LookOffsetPitch;

            // Initialize applied angles on first update to match current base orientation
            if (m_FirstUpdate)
            {
                m_AppliedYaw = targetYaw;
                m_AppliedPitch = targetPitch;
                m_FirstUpdate = false;
            }

            // Smoothly approach target angles
            m_AppliedYaw = Mathf.SmoothDampAngle(m_AppliedYaw, targetYaw, ref m_YawVelocity, Mathf.Max(0.0001f, lookSmoothTime), Mathf.Infinity, dt);
            m_AppliedPitch = Mathf.SmoothDampAngle(m_AppliedPitch, targetPitch, ref m_PitchVelocity, Mathf.Max(0.0001f, lookSmoothTime), Mathf.Infinity, dt);

            // Construct final rotation by replacing pitch/yaw (and optionally roll)
            Quaternion finalRot;
            if (yawOnly)
            {
                finalRot = Quaternion.Euler(0f, m_AppliedYaw, 0f);
            }
            else
            {
                finalRot = Quaternion.Euler(m_AppliedPitch, m_AppliedYaw, baseRoll);
            }

            // Call base helper which uses the component's trackingType and tracking state logic
            SetLocalTransform(newPos, finalRot);
        }
    }
}