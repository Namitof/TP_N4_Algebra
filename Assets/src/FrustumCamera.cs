using UnityEngine;

// https://learnopengl.com/Guest-Articles/2021/Scene/Frustum-Culling
// Datos de entrada del frustum (FOV, aspect ratio y distancia de renderizado) 
// Chequea si un punto está adentro o no.
[RequireComponent(typeof(Camera))] //Requiere una camara
public class FrustumCamera : MonoBehaviour
{
    [Header("Datos del frustum")]
    [Tooltip("Campo de visión VERTICAL en grados")]
    [Range(10f, 170f)]
    [SerializeField] private float fieldOfViewDegrees = 60f; //Fov

    [Tooltip("Relación de aspecto")]
    [SerializeField] private float aspectRatio = 16f / 9f; //Relacion de aspecto

    [Tooltip("Distancia del plano cercano (near plane)")]
    [SerializeField] private float nearPlaneDistance = 0.3f; //Distancia plano cercano

    [Tooltip("Distancia de renderizado (far plane)")]
    [SerializeField] private float renderDistance = 50f; //Distancia plano lejano

    [Header("Gizmos")]
    [SerializeField] private Color frustumGizmoColor = Color.yellow;

    private Camera unityCamera;

    //Planos del frustrum
    private struct Plane
    {
        public Vector3 normal;
        public float d;
    }

    const int PLANES_QUANTITY = 6;
    private Plane[] frustumPlanes = new Plane[PLANES_QUANTITY];

    private void Start()
    {
        unityCamera = GetComponent<Camera>();
        ApplySettingsToUnityCamera();
        CalculateFrustumPlanes();
    }

    private void Update()
    {
        CalculateFrustumPlanes();
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

    // Devuelve true si el punto (en coordenadas de mundo) está dentro del frustum.
    public bool IsPointInsideFrustum(Vector3 worldPoint)
    {
        //Resultado Mayor que 0 -> Lado hacia donde apunta la normal.
        //Resultado Menor que 0 -> Lado contrario a la normal.
        //Resultado Igual a 0 -> El punto está sobre el plano.
        foreach (Plane plane in frustumPlanes)
        {
            float distance = Vector3.Dot(plane.normal, worldPoint) + plane.d;

            if (distance < 0f)
            {
                return false;
            }
        }

        return true;
    }

    // Calcula las 4 esquinas del rectángulo que forma el frustum a cierta distancia de la cámara.
    // Orden: arriba-izquierda, arriba-derecha, abajo-derecha, abajo-izquierda.
    private Vector3[] GetFrustumCornersAtDistance(float distanceFromCamera)
    {
        //Pasar de grados a radianes X Grados * PI/180
        float halfFieldOfViewRadians = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad; 

        //Mitad del alto del rectángulo: tan(FOV/2) = mitadAlto / distancia //Trigonometria -> SOHCAHTOA
        float halfHeight = Mathf.Tan(halfFieldOfViewRadians) * distanceFromCamera;

        //Mitad del ancho: el alto multiplicado por la relación de aspecto
        float halfWidth = halfHeight * aspectRatio;

        //transform forward es un vector unitario que representa la direccion donde mira la camara (Eje Z positivo local del objeto)
        Vector3 rectangleCenter = transform.position + transform.forward * distanceFromCamera;
        Vector3 upOffset = transform.up * halfHeight;
        Vector3 rightOffset = transform.right * halfWidth;

        //transform up es el vector que apunta hacia arriba respecto a la rotación actual de la cámara (su eje Y local) //Direccion

        //transform right vector normalizado que apunta hacia la derecha desde la perspectiva actual de la cámara (eje local X) //Direccion

        return new Vector3[]
        {
                rectangleCenter + upOffset - rightOffset, //Arriba izquierda
                rectangleCenter + upOffset + rightOffset, //Arriba derecha
                rectangleCenter - upOffset + rightOffset, //Abajo derecha
                rectangleCenter - upOffset - rightOffset //Abajo izquierda
        };
    }

    private void OnDrawGizmos()
    {
        Vector3[] nearCorners = GetFrustumCornersAtDistance(nearPlaneDistance);
        Vector3[] farCorners = GetFrustumCornersAtDistance(renderDistance);

        Gizmos.color = frustumGizmoColor;

        // Plano cercano
        Gizmos.DrawLine(nearCorners[0], nearCorners[1]);
        Gizmos.DrawLine(nearCorners[1], nearCorners[2]);
        Gizmos.DrawLine(nearCorners[2], nearCorners[3]);
        Gizmos.DrawLine(nearCorners[3], nearCorners[0]);

        // Plano lejano
        Gizmos.DrawLine(farCorners[0], farCorners[1]);
        Gizmos.DrawLine(farCorners[1], farCorners[2]);
        Gizmos.DrawLine(farCorners[2], farCorners[3]);
        Gizmos.DrawLine(farCorners[3], farCorners[0]);

        // Conectar ambos planos
        Gizmos.DrawLine(nearCorners[0], farCorners[0]);
        Gizmos.DrawLine(nearCorners[1], farCorners[1]);
        Gizmos.DrawLine(nearCorners[2], farCorners[2]);
        Gizmos.DrawLine(nearCorners[3], farCorners[3]);
    }

    private void CalculateFrustumPlanes()
    {
        Vector3[] nearCorners = GetFrustumCornersAtDistance(nearPlaneDistance);
        Vector3[] farCorners = GetFrustumCornersAtDistance(renderDistance);

        //Sacamos un punto que sabemos que está dentro del frustum -> Ubicado en el eje central de la cámara, a mitad de distancia entre el plano cercano y el lejano del frustum.
        Vector3 insidePoint = transform.position + transform.forward * ((nearPlaneDistance + renderDistance) * 0.5f);

        //Construir los seis planos
        frustumPlanes[0] = CreatePlane(nearCorners[0], nearCorners[1], nearCorners[2], insidePoint); //Near

        frustumPlanes[1] = CreatePlane(farCorners[0], farCorners[1], farCorners[2], insidePoint); //Far

        frustumPlanes[2] = CreatePlane(nearCorners[0], nearCorners[3], farCorners[3], insidePoint); //Left

        frustumPlanes[3] = CreatePlane(nearCorners[1], nearCorners[2], farCorners[2], insidePoint); //Right

        frustumPlanes[4] = CreatePlane(nearCorners[0], nearCorners[1], farCorners[1], insidePoint); //Top

        frustumPlanes[5] = CreatePlane(nearCorners[3], nearCorners[2], farCorners[2], insidePoint); // Bottom
    }

    private Plane CreatePlane(Vector3 a, Vector3 b, Vector3 c, Vector3 insidePoint)
    {
        //Calcular dos vectores del plano 
        //Extremo - Cola
        //(Punto final - punto de origen) -> Asi el vector apunta al punto final
        Vector3 vectorAB = b - a;
        Vector3 vectorAC = c - a;

        //Calcular la normal con producto vectorial (producto cruz)
        Vector3 normal = Vector3.Cross(vectorAB, vectorAC).normalized;

        //Calcular el término independiente D

        //Ecuación Normal del Plano -> N . (P - P0) = 0
        // N -> Vector normal
        // P -> Punto a evaluar
        // P0 -> Punto perteneciente al plano

        //Hago distributiva con N
        // (N . P) - (N . P0) = 0
        // -N . P0 da siempre un numero fijo -> D

        // N . P + D = 0

        //D = -(n . P0)

        float d = -Vector3.Dot(normal, a);

        //Comprobar que la normal apunte hacia adentro -> N . P + D = 0
        if (Vector3.Dot(normal, insidePoint) + d < 0f)
        {
            normal = -normal;
            d = -d;
        }

        return new Plane
        {
            normal = normal,
            d = d
        };
    }
}