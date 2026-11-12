using UnityEngine;

namespace ReyesCuadra
{
    // Objeto 3D que se chequea contra el frustum.
    // Si AL MENOS UNO de sus vértices está adentro, se pinta de verde y avisa por consola.
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FrustumTarget : MonoBehaviour
    {
        [SerializeField] private Color insideFrustumColor = Color.green;
        [SerializeField] private Color outsideFrustumColor = Color.red;

        private FrustumCamera frustumCamera;
        private Vector3[] localVertices;
        private MeshRenderer meshRenderer;
        private bool hasBeenCheckedOnce;

        // Estado actual, lo lee el HUD para mostrarlo en pantalla
        public bool IsInsideFrustum { get; private set; }

        private void Start()
        {
            frustumCamera = FindFirstObjectByType<FrustumCamera>();
            meshRenderer = GetComponent<MeshRenderer>();

            // Los vértices del mesh vienen en espacio LOCAL del objeto
            localVertices = GetComponent<MeshFilter>().sharedMesh.vertices;

            if (frustumCamera == null)
                Debug.LogWarning($"[{name}] No hay ninguna FrustumCamera en la escena.");
        }

        private void Update()
        {
            if (frustumCamera == null)
                return;

            bool isAnyVertexInside = false;

            foreach (Vector3 localVertex in localVertices)
            {
                // Pasamos el vértice de espacio local a espacio de mundo
                // (aplica la posición, rotación y escala del objeto)
                Vector3 worldVertex = transform.TransformPoint(localVertex);

                if (frustumCamera.IsPointInsideFrustum(worldVertex))
                {
                    isAnyVertexInside = true;
                    break; // con uno solo alcanza, no hace falta seguir revisando
                }
            }

            // Solo actualizamos color y avisamos cuando el estado cambia
            if (isAnyVertexInside != IsInsideFrustum || !hasBeenCheckedOnce)
            {
                IsInsideFrustum = isAnyVertexInside;
                hasBeenCheckedOnce = true;
                NotifyStateChange();
            }
        }

        private void NotifyStateChange()
        {
            // renderer.material crea una copia del material solo para este objeto
            meshRenderer.material.color = IsInsideFrustum ? insideFrustumColor : outsideFrustumColor;

            string stateText = IsInsideFrustum ? "ENTRÓ al frustum" : "SALIÓ del frustum";
            Debug.Log($"[{name}] {stateText}");
        }
    }
}
