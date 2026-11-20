using UnityEngine;

// Objeto 3D que se chequea con el frustum.
// Si AL MENOS UNO de sus vértices está adentro, se pinta de verde y avisa por consola.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] //Requiere mesh
public class FrustumTarget : MonoBehaviour
{
    [SerializeField] private FrustumCamera frustumCamera;
    [SerializeField] private Color insideFrustumColor = Color.green;
    [SerializeField] private Color outsideFrustumColor = Color.red;

    private Vector3[] localVertices;
    private MeshRenderer meshRenderer;
    private bool hasBeenCheckedOnce = false;
    private bool IsInsideFrustum;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        //Los vértices del mesh vienen en espacio LOCAL del objeto
        localVertices = GetComponent<MeshFilter>().sharedMesh.vertices;

        if (frustumCamera == null)
            Debug.Log("No hay ninguna FrustumCamera");
    }

    private void Update()
    {
        if (frustumCamera == null)
            return;

        bool isAnyVertexInside = false;

        foreach (Vector3 localVertex in localVertices)
        {
            //Pasamos el vértice de espacio local a espacio de mundo
            //(aplica la posición, rotación y escala del objeto)
            Vector3 worldVertex = transform.TransformPoint(localVertex);

            if (frustumCamera.IsPointInsideFrustum(worldVertex))
            {
                isAnyVertexInside = true;
                break; //con uno solo alcanza, no hace falta seguir revisando
            }
        }

        //Actualizamos color y avisamos cuando el estado cambia
        if (isAnyVertexInside != IsInsideFrustum || !hasBeenCheckedOnce)
        {
            IsInsideFrustum = isAnyVertexInside;
            hasBeenCheckedOnce = true;
            NotifyStateChange();
        }
    }

    private void NotifyStateChange()
    {
        //MeshRenderer
        //Se encarga de renderizar (dibujar) el modelo en pantalla.
        //Controla aspectos como:
        //  - Materiales.
        //  - Sombras.
        //  - Iluminación.
        //  - Visibilidad del renderizado.

        //Cambia el color del objeto dependiendo de si está dentro o fuera del frustum.
        meshRenderer.material.color = IsInsideFrustum ? insideFrustumColor : outsideFrustumColor;

        // OCULTAR EL MODELO
        //if (!IsInsideFrustum)
        //    meshRenderer.enabled = false;
        //else
        //    meshRenderer.enabled = true;

        if (IsInsideFrustum)
        {
            Debug.Log($"[ADENTRO] {gameObject.name}");
        }
        else
        {
            Debug.Log($"[AFUERA] {gameObject.name}");
        }
    }
}

