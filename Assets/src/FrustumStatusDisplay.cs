using UnityEngine;

namespace ReyesCuadra
{
    // Muestra en pantalla los datos del frustum y qué objetos están adentro o afuera.
    public class FrustumStatusDisplay : MonoBehaviour
    {
        private FrustumCamera frustumCamera;
        private FrustumTarget[] frustumTargets;
        private GUIStyle labelStyle;

        private void Start()
        {
            frustumCamera = FindFirstObjectByType<FrustumCamera>();
            frustumTargets = FindObjectsByType<FrustumTarget>(FindObjectsSortMode.None);
        }

        private void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    richText = true
                };
            }

            GUILayout.BeginArea(new Rect(10f, 10f, 460f, 600f), GUI.skin.box);

            if (frustumCamera != null)
            {
                GUILayout.Label($"FOV: {frustumCamera.FieldOfViewDegrees:0.0}°   (Z / X)", labelStyle);
                GUILayout.Label($"Aspect ratio: {frustumCamera.AspectRatio:0.000}", labelStyle);
                GUILayout.Label($"Distancia de renderizado: {frustumCamera.RenderDistance:0.0}   (C / V)", labelStyle);
                GUILayout.Space(8f);
            }

            foreach (FrustumTarget frustumTarget in frustumTargets)
            {
                string stateText = frustumTarget.IsInsideFrustum
                    ? "<color=lime>ADENTRO</color>"
                    : "<color=red>AFUERA</color>";

                GUILayout.Label($"{frustumTarget.name}: {stateText}", labelStyle);
            }

            GUILayout.Space(8f);
            GUILayout.Label("WASD mover · Q/E bajar/subir · Click der. mirar · Shift rápido", labelStyle);

            GUILayout.EndArea();
        }
    }
}
