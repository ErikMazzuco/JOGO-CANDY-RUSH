
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public float intervalo = 0.5f;

    private float tempo;
    private int frames;
    private float fps;

    void Update()
    {
        frames++;
        tempo += Time.unscaledDeltaTime;

        if (tempo >= intervalo)
        {
            fps = frames / tempo;
            frames = 0;
            tempo = 0;
        }
    }

    void OnGUI()
    {
        GUIStyle estilo = new GUIStyle();
        estilo.fontSize = 20;
        estilo.fontStyle = FontStyle.Bold;
        estilo.normal.textColor = Color.white;

        GUI.color = Color.black;
        GUI.Label(new Rect(13, 13, 180, 45),
            "FPS: " + fps.ToString("F0"), estilo);

        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 180, 45),
            "FPS: " + fps.ToString("F0"), estilo);
    }
}
