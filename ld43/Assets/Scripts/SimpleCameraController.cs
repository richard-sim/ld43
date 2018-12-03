using UnityEngine;

namespace UnityTemplateProjects
{
    public class SimpleCameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 lsTranslation, Vector3 wsTranslation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * lsTranslation;

                x += rotatedTranslation.x + wsTranslation.x;
                y += rotatedTranslation.y + wsTranslation.y;
                z += rotatedTranslation.z + wsTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                
                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        public Camera TargetCamera;
        public PlayerController Player;
        public float CameraDistace = 35.0f;

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 7.0f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;

        private bool isTranslating = false;
        private bool isRotating = false;
        private Vector3 initialCursorPosition;

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);

            GameObject commander = Player.GetCommander().gameObject;
            Vector3 viewTarget = commander.transform.position + commander.transform.forward * 2.0f;
            
            // Rotation happens on the Camera
//            TargetCamera.transform.LookAt(viewTarget);
            // Translation happens on this GameObject
            transform.position = viewTarget - TargetCamera.transform.forward * CameraDistace;
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (Input.GetKey(KeyCode.W))
            {
                direction += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += Vector3.right;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                direction += Vector3.down;
            }
            if (Input.GetKey(KeyCode.E))
            {
                direction += Vector3.up;
            }
            return direction;
        }
        
        void Update()
        {
            // Exit Sample  
//            if (Input.GetKey(KeyCode.Escape))
//            {
//                Application.Quit();
//				#if UNITY_EDITOR
//				UnityEditor.EditorApplication.isPlaying = false; 
//				#endif
//            }

            // Mouse Rotation
            
            // Hide and lock cursor when right mouse button pressed
            if (!isTranslating && (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2")))
            {
                if (Input.GetMouseButton(1) && Input.GetButton("Fire2")) {
                    isRotating = true;
                    initialCursorPosition = Input.mousePosition;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            // Unlock and show cursor when right mouse button released
            if (isRotating && (Input.GetMouseButtonUp(1) || Input.GetButtonUp("Fire2")))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                isRotating = false;
            }
            
            // Mouse Translation

            Vector3 mouseTranslation = Vector3.zero;
            // Hide and lock cursor when right mouse button pressed
            if (!isRotating && Input.GetMouseButtonDown(1))
            {
                GameObject commander = Player.GetCommander().gameObject;
                
                Ray mouseRay = TargetCamera.ScreenPointToRay(Input.mousePosition);
            
                int layerMask = LayerMask.GetMask("Ground");

                RaycastHit targetHit;
                if (Physics.Raycast(mouseRay, out targetHit, Mathf.Infinity, layerMask)) {
                    Vector3 newPosition = targetHit.point - TargetCamera.transform.forward * CameraDistace;
                    mouseTranslation = newPosition - transform.position;
                }

//                isTranslating = true;
//                initialCursorPosition = Input.mousePosition;
//                Cursor.lockState = CursorLockMode.Locked;
            }
            // Unlock and show cursor when right mouse button released
            if (isTranslating && Input.GetMouseButtonUp(1))
            {
//                Cursor.visible = true;
//                Cursor.lockState = CursorLockMode.None;
//                isTranslating = false;
            }
            
            // Movement

            if (isRotating) {
                // Rotation
                var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));
                
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            } else if (isTranslating) {
                // Translation
//                Vector3 mouseStart = initialCursorPosition;
//                mouseStart.z = TargetCamera.nearClipPlane;
//                Vector3 mouseCurr = Input.mousePosition;
//                mouseCurr.z = TargetCamera.nearClipPlane;
//                Vector3 mouseMovement = TargetCamera.ScreenToViewportPoint((mouseCurr) - TargetCamera.ScreenToViewportPoint(mouseStart));
//                
//                mouseTranslation = mouseMovement.x * Vector3.right + mouseMovement.y * Vector3.forward;
            }

            Vector3 keyboardTranslation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (Input.GetKey(KeyCode.LeftShift)) {
                keyboardTranslation *= 10.0f;
            }

            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            boost += Input.mouseScrollDelta.y * 0.2f;
            keyboardTranslation *= Mathf.Pow(2.0f, boost);
                
            m_TargetCameraState.Translate(keyboardTranslation, mouseTranslation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }

}