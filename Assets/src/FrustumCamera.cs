using UnityEngine;
using UnityEngine.InputSystem;

namespace ReyesCuadra
{
    // Cámara "observadora": guarda los datos de entrada del frustum (FOV, aspect ratio
    // y distancia de renderizado) y es la que decide si un punto está adentro o no.
    // Unity solo se usa para renderizar; la cuenta del frustum la hacemos nosotros.
    [RequireComponent(typeof(Camera))]
    public class FrustumCamera : MonoBehaviour
    {
        [Header("Datos de entrada del frustum")]
        [Tooltip("Campo de visión VERTICAL en grados")]
        [Range(10f, 170f)]
        [SerializeField] private float fieldOfViewDegrees = 60f;

        [Tooltip("Relación de aspecto del monitor (asumimos 16:9, no se lee del hardware)")]
        [SerializeField] private float aspectRatio = 16f / 9f;

        [Tooltip("Distancia del plano cercano (near plane)")]
        [SerializeField] private float nearPlaneDistance = 0.3f;

        [Tooltip("Distancia de renderizado (far plane)")]
        [SerializeField] private float renderDistance = 50f;

        [Header("Ajuste en tiempo de ejecución")]
        [Tooltip("Grados por segundo al cambiar el FOV con Z / X")]
        [SerializeField] private float fieldOfViewChangeSpeed = 30f;

        [Tooltip("Unidades por segundo al cambiar la distancia con C / V")]
        [SerializeField] private float renderDistanceChangeSpeed = 20f;

        [Header("Gizmos")]
        [SerializeField] private Color frustumGizmoColor = Color.yellow;

        private Camera unityCamera;

        // Getters para que otros scripts (HUD, objetos) puedan leer los datos
        public float FieldOfViewDegrees => fieldOfViewDegrees;
        public float AspectRatio => aspectRatio;
        public float NearPlaneDistance => nearPlaneDistance;
        public float RenderDistance => renderDistance;

        private void Awake()
        {
            unityCamera = GetComponent<Camera>();
            ApplySettingsToUnityCamera();
        }

        private void OnValidate()
        {
            // Se llama cuando cambiás un valor en el Inspector: mantenemos todo sincronizado
            if (nearPlaneDistance < 0.01f)
                nearPlaneDistance = 0.01f;

            if (renderDistance < nearPlaneDistance + 0.1f)
                renderDistance = nearPlaneDistance + 0.1f;

            if (aspectRatio < 0.1f)
                aspectRatio = 0.1f;

            if (unityCamera == null)
                unityCamera = GetComponent<Camera>();

            ApplySettingsToUnityCamera();
        }

        private void Update()
        {
            HandleRuntimeAdjustments();
            ApplySettingsToUnityCamera();
        }

        // Le pasa nuestros datos a la cámara de Unity, así lo que se VE en pantalla
        // coincide con el frustum que nosotros calculamos.
        private void ApplySettingsToUnityCamera()
        {
            if (unityCamera == null)
                return;

            unityCamera.fieldOfView = fieldOfViewDegrees;
            unityCamera.aspect = aspectRatio;
            unityCamera.nearClipPlane = nearPlaneDistance;
            unityCamera.farClipPlane = renderDistance;
        }

        // Z / X cambian el FOV, C / V cambian la distancia de renderizado (útil para mostrar en el parcial)
        private void HandleRuntimeAdjustments()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.zKey.isPressed)
                fieldOfViewDegrees -= fieldOfViewChangeSpeed * Time.deltaTime;
            if (keyboard.xKey.isPressed)
                fieldOfViewDegrees += fieldOfViewChangeSpeed * Time.deltaTime;

            if (keyboard.cKey.isPressed)
                renderDistance -= renderDistanceChangeSpeed * Time.deltaTime;
            if (keyboard.vKey.isPressed)
                renderDistance += renderDistanceChangeSpeed * Time.deltaTime;

            fieldOfViewDegrees = Mathf.Clamp(fieldOfViewDegrees, 10f, 170f);
            renderDistance = Mathf.Max(renderDistance, nearPlaneDistance + 0.1f);
        }

        // ------------------------------------------------------------------
        // FRUSTUM CULLING
        // ------------------------------------------------------------------

        // Devuelve true si el punto (en coordenadas de mundo) está dentro del frustum.
        // TODO (siguiente paso): acá va la matemática del frustum culling.
        public bool IsPointInsideFrustum(Vector3 worldPoint)
        {
            return false;
        }

        // ------------------------------------------------------------------
        // DIBUJO DEL FRUSTUM (solo visual, para verlo en la Scene / Game view)
        // ------------------------------------------------------------------

        // Calcula las 4 esquinas del rectángulo que forma el frustum a cierta distancia de la cámara.
        // Orden: arriba-izquierda, arriba-derecha, abajo-derecha, abajo-izquierda.
        private Vector3[] GetFrustumCornersAtDistance(float distanceFromCamera)
        {
            float halfFieldOfViewRadians = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;

            // Mitad del alto del rectángulo: tan(FOV/2) = mitadAlto / distancia
            float halfHeight = Mathf.Tan(halfFieldOfViewRadians) * distanceFromCamera;
            // Mitad del ancho: el alto multiplicado por la relación de aspecto
            float halfWidth = halfHeight * aspectRatio;

            Vector3 rectangleCenter = transform.position + transform.forward * distanceFromCamera;
            Vector3 upOffset = transform.up * halfHeight;
            Vector3 rightOffset = transform.right * halfWidth;

            return new Vector3[]
            {
                rectangleCenter + upOffset - rightOffset,
                rectangleCenter + upOffset + rightOffset,
                rectangleCenter - upOffset + rightOffset,
                rectangleCenter - upOffset - rightOffset
            };
        }

        private void OnDrawGizmos()
        {
            Vector3[] nearCorners = GetFrustumCornersAtDistance(nearPlaneDistance);
            Vector3[] farCorners = GetFrustumCornersAtDistance(renderDistance);

            Gizmos.color = frustumGizmoColor;

            for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++)
            {
                int nextCornerIndex = (cornerIndex + 1) % 4;

                // Rectángulo del plano cercano
                Gizmos.DrawLine(nearCorners[cornerIndex], nearCorners[nextCornerIndex]);
                // Rectángulo del plano lejano
                Gizmos.DrawLine(farCorners[cornerIndex], farCorners[nextCornerIndex]);
                // Aristas que unen el plano cercano con el lejano
                Gizmos.DrawLine(nearCorners[cornerIndex], farCorners[cornerIndex]);
            }
        }
    }
}
