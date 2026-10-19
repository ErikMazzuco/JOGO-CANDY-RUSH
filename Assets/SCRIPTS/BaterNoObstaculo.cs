using UnityEngine;
using UnityEngine.SceneManagement;

// Vai no Toddynho. Quando ele bate de frente num obstáculo, para tudo e reinicia a fase.
public class BaterNoObstaculo : MonoBehaviour
{
    public float tempoParaReiniciar = 2f;   // segundos até recomeçar
    public string mensagem = "Ops! Você bateu!";

    bool bateu = false;

    // a Unity chama isto sempre que o Character Controller encosta em algo
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (bateu) return;
        if (hit.collider.GetComponentInParent<Obstaculo>() == null) return; // não é obstáculo
        if (hit.normal.y > 0.7f) return; // pisou em cima dele (pulou): tudo bem

        bateu = true;
        Debug.Log("Toddynho bateu em: " + hit.collider.GetComponentInParent<Obstaculo>().name);

        // desliga os outros scripts do Toddynho (ele para de correr)
        foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
            if (m != this) m.enabled = false;

        // trava o corpo para ele não cair pelo chão
        Rigidbody corpo = GetComponent<Rigidbody>();
        if (corpo != null) corpo.isKinematic = true;

        Invoke(nameof(Reiniciar), tempoParaReiniciar);
    }

    void Reiniciar()
    {
        Scene cena = SceneManager.GetActiveScene();
        if (cena.buildIndex >= 0)
        {
            SceneManager.LoadScene(cena.buildIndex);
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                cena.path, new LoadSceneParameters(LoadSceneMode.Single));
#endif
        }
    }

    void OnGUI()
    {
        if (!bateu) return;
        GUIStyle estilo = new GUIStyle(GUI.skin.label);
        estilo.fontSize = Mathf.Max(20, Screen.height / 9);
        estilo.fontStyle = FontStyle.Bold;
        estilo.alignment = TextAnchor.MiddleCenter;

        estilo.normal.textColor = new Color(0.35f, 0.15f, 0.05f);
        GUI.Label(new Rect(4, 4, Screen.width, Screen.height), mensagem, estilo); // sombra
        estilo.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), mensagem, estilo);
    }
}
