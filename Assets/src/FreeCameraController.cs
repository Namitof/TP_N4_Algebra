using UnityEngine;
using UnityEngine.InputSystem;

namespace ReyesCuadra
{
    // Cámara libre para moverse por la escena y probar el frustum.
    // WASD: moverse · Q/E: bajar/subir · Click derecho + mouse: mirar · Shift: más rápido
    public class FreeCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float fastMoveMultiplier = 3f;
        [SerializeField] private float lookSensitivity = 0.15f;

        private float yawDegrees;   // rotación horizontal (eje Y)
        private float pitchDegrees; // rotación vertical (eje X)

        private void Start()
        {
            Vector3 startRotation = transform.eulerAngles;
            yawDegrees = startRotation.y;
            pitchDegrees = startRotation.x > 180f ? startRotation.x - 360f : startRotation.x;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.isPressed)
                return;

            Vector2 mouseDelta = mouse.delta.ReadValue();

            yawDegrees += mouseDelta.x * lookSensitivity;
            pitchDegrees -= mouseDelta.y * lookSensitivity;
            pitchDegrees = Mathf.Clamp(pitchDegrees, -89f, 89f); // evita dar la vuelta

            transform.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
        }

        private void HandleMovement()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Vector3 moveDirection = Vector3.zero;

            if (keyboard.wKey.isPressed) moveDirection += transform.forward;
            if (keyboard.sKey.isPressed) moveDirection -= transform.forward;
            if (keyboard.dKey.isPressed) moveDirection += transform.right;
            if (keyboard.aKey.isPressed) moveDirection -= transform.right;
            if (keyboard.eKey.isPressed) moveDirection += Vector3.up;
            if (keyboard.qKey.isPressed) moveDirection -= Vector3.up;

            float currentSpeed = moveSpeed;
            if (keyboard.leftShiftKey.isPressed)
                currentSpeed *= fastMoveMultiplier;

            transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;
        }
    }
}
