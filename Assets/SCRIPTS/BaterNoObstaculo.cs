using UnityEngine;
using UnityEngine.SceneManagement;

// Vai no Toddynho. Quando ele bate de frente num obstáculo, ele para
// e aparece a telinha "Ops! Você bateu!" com os botões de jogar de novo e menu.
public class BaterNoObstaculo : MonoBehaviour
{
    public string mensagem = "Ops! Você bateu!";
    public float esperaParaMostrar = 0.6f;   // um tempinho antes da telinha aparecer

    bool bateu = false;
    float horaDaBatida;
    Texture2D escurecer, painel, botaoDeNovo, botaoMenu;

    void Awake()
    {
        escurecer = DoceUI.Cor(new Color(0.2f, 0.05f, 0.1f, 0.5f));
        painel = DoceUI.Imagem("painel_laranja");
        botaoDeNovo = DoceUI.Imagem("botao_jogar_de_novo");
        botaoMenu = DoceUI.Imagem("botao_menu");
    }

    // a Unity chama isto sempre que o Character Controller encosta em algo
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (bateu) return;
        Obstaculo obstaculo = hit.collider.GetComponentInParent<Obstaculo>();
        if (obstaculo == null) return;          // não é obstáculo
        if (hit.normal.y > 0.7f) return;        // pisou em cima dele (pulou): tudo bem

        bateu = true;
        horaDaBatida = Time.unscaledTime;
        Debug.Log("Toddynho bateu em: " + obstaculo.name);

        // desliga os outros scripts do Toddynho (ele para de correr)
        foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
            if (m != this) m.enabled = false;

        // trava o corpo para ele não cair pelo chão
        Rigidbody corpo = GetComponent<Rigidbody>();
        if (corpo != null) corpo.isKinematic = true;
    }

    void JogarDeNovo()
    {
        Recarregar();
    }

    void VoltarProMenu()
    {
        TelaInicial.jogoComecou = false;
        Recarregar();
    }

    void Recarregar()
    {
        Time.timeScale = 1f;
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
        GUI.depth = -10;

        float W = Screen.width, H = Screen.height;
        float tempo = Time.unscaledTime - horaDaBatida;

        // primeiro só o texto, depois a telinha com os botões
        if (tempo < esperaParaMostrar)
        {
            DoceUI.Texto(new Rect(0, H * 0.35f, W, H * 0.15f), mensagem,
                         Mathf.RoundToInt(H * 0.09f), Color.white, DoceUI.Chocolate, H * 0.006f);
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            e.Use();
            JogarDeNovo();
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, W, H), escurecer);

        // a telinha "pula" um pouco quando aparece
        float entrada = Mathf.Clamp01((tempo - esperaParaMostrar) / 0.25f);
        float escala = 0.85f + 0.15f * Mathf.Sin(entrada * Mathf.PI * 0.5f);

        float pw = Mathf.Min(W * 0.9f, H * 0.9f) * escala;
        float ph = H * 0.5f * escala;
        Rect r = new Rect(W / 2f - pw / 2f, H / 2f - ph / 2f, pw, ph);
        DoceUI.Painel(r, painel);

        DoceUI.Texto(new Rect(r.x, r.y + ph * 0.1f, pw, ph * 0.25f), mensagem,
                     Mathf.RoundToInt(H * 0.075f * escala), Color.white, DoceUI.Chocolate, H * 0.006f);

        float b = H * 0.14f * escala;
        float y = r.y + ph * 0.48f;
        if (DoceUI.Botao(new Rect(W / 2f - b - H * 0.03f, y, b, b), botaoDeNovo)) JogarDeNovo();
        if (DoceUI.Botao(new Rect(W / 2f + H * 0.03f, y, b, b), botaoMenu)) VoltarProMenu();

        int tl = Mathf.RoundToInt(H * 0.03f * escala);
        DoceUI.Texto(new Rect(W / 2f - b - H * 0.03f - b * 0.5f, y + b, b * 2f, H * 0.05f), "tentar novamente",
                     tl, DoceUI.Creme, DoceUI.Chocolate, H * 0.0025f);
        DoceUI.Texto(new Rect(W / 2f + H * 0.03f - b * 0.5f, y + b, b * 2f, H * 0.05f), "menu",
                     tl, DoceUI.Creme, DoceUI.Chocolate, H * 0.0025f);
    }
}
