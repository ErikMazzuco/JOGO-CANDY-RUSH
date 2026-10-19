using UnityEngine;

// Tela inicial: o mapa fica parado no fundo, com o título e os botões de doce por cima.
public class TelaInicial : MonoBehaviour
{
    [Header("Textos")]
    public string titulo = "TODDYNHO";
    public string subtitulo = "Corrida na Terra dos Doces";
    public string avisoEmbaixo = "Aperte ENTER ou clique em JOGAR";

    // "static" = continua lembrando mesmo quando a fase recomeça.
    // Assim, depois de bater, o jogo volta direto pra corrida (sem mostrar o menu de novo).
    public static bool jogoComecou = false;

    bool aberta;
    bool ajudaAberta;

    Texture2D fundo, escurecer, botaoLargo, botaoAjuda, botaoSair, painel, botaoFechar;

    void Awake()
    {
        fundo = DoceUI.Gradiente(new Color(1f, 0.55f, 0.78f, 0.55f), new Color(0.45f, 0.18f, 0.08f, 0.55f));
        escurecer = DoceUI.Cor(new Color(0.2f, 0.05f, 0.1f, 0.55f));
        botaoLargo = DoceUI.Imagem("botao_largo_rosa");
        botaoAjuda = DoceUI.Imagem("botao_ajuda");
        botaoSair = DoceUI.Imagem("botao_sair");
        painel = DoceUI.Imagem("painel_laranja");
        botaoFechar = DoceUI.Imagem("botao_fechar");

        aberta = !jogoComecou;
        Time.timeScale = aberta ? 0f : 1f; // 0 = tudo congelado esperando o "Jogar"
    }

    void Comecar()
    {
        aberta = false;
        ajudaAberta = false;
        jogoComecou = true;
        Time.timeScale = 1f;
    }

    void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnGUI()
    {
        if (!aberta) return;
        GUI.depth = -10;

        float W = Screen.width;
        float H = Screen.height;
        float t = Time.unscaledTime;

        // teclado
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { Comecar(); e.Use(); return; }
            if (e.keyCode == KeyCode.Escape && ajudaAberta) { ajudaAberta = false; e.Use(); }
        }

        // fundo rosinha por cima do mapa
        GUI.DrawTexture(new Rect(0, 0, W, H), fundo);

        // título balançando
        float balanco = Mathf.Sin(t * 2f) * H * 0.012f;
        Matrix4x4 antes = GUI.matrix;
        GUIUtility.RotateAroundPivot(-4f + Mathf.Sin(t * 1.3f) * 1.2f, new Vector2(W / 2f, H * 0.25f));
        DoceUI.Texto(new Rect(0, H * 0.12f + balanco, W, H * 0.22f), titulo,
                     Mathf.RoundToInt(H * 0.17f), Color.white, DoceUI.Chocolate, H * 0.009f);
        GUI.matrix = antes;

        DoceUI.Texto(new Rect(0, H * 0.33f, W, H * 0.08f), subtitulo,
                     Mathf.RoundToInt(H * 0.055f), DoceUI.Creme, DoceUI.Chocolate, H * 0.005f);

        if (!ajudaAberta)
        {
            // botão JOGAR (pulsando)
            float bw = H * 0.5f;
            float bh = bw * 148f / 306f;
            Rect rJogar = new Rect(W / 2f - bw / 2f, H * 0.47f, bw, bh);
            if (DoceUI.Botao(rJogar, botaoLargo, "JOGAR", Mathf.RoundToInt(H * 0.08f), Mathf.Sin(t * 4f) * 0.03f))
                Comecar();

            // botões redondos: ajuda e sair
            float rb = H * 0.12f;
            float y = rJogar.yMax + H * 0.045f;
            if (DoceUI.Botao(new Rect(W / 2f - rb - H * 0.025f, y, rb, rb), botaoAjuda)) ajudaAberta = true;
            if (DoceUI.Botao(new Rect(W / 2f + H * 0.025f, y, rb, rb), botaoSair)) Sair();

            // aviso piscando
            float alfa = 0.55f + 0.45f * Mathf.Sin(t * 3f);
            DoceUI.Texto(new Rect(0, H * 0.88f, W, H * 0.06f), avisoEmbaixo,
                         Mathf.RoundToInt(H * 0.035f), new Color(1, 1, 1, alfa), DoceUI.Chocolate, H * 0.003f);
        }
        else
        {
            DesenharAjuda(W, H);
        }
    }

    void DesenharAjuda(float W, float H)
    {
        GUI.DrawTexture(new Rect(0, 0, W, H), escurecer);

        float pw = Mathf.Min(W * 0.9f, H * 1.0f);
        float ph = H * 0.52f;
        Rect r = new Rect(W / 2f - pw / 2f, H * 0.42f, pw, ph);
        DoceUI.Painel(r, painel);

        DoceUI.Texto(new Rect(r.x, r.y + ph * 0.06f, pw, ph * 0.16f), "COMO JOGAR",
                     Mathf.RoundToInt(H * 0.06f), Color.white, DoceUI.Chocolate, H * 0.005f);

        string[] linhas =
        {
            "A / D  ou  setas  -  trocar de pista",
            "ESPAÇO  -  pular",
            "Desvie dos doces na pista",
            "e dos pirulitões que caem!"
        };
        float y = r.y + ph * 0.28f;
        foreach (string l in linhas)
        {
            DoceUI.Texto(new Rect(r.x, y, pw, ph * 0.13f), l,
                         Mathf.RoundToInt(H * 0.038f), DoceUI.Creme, DoceUI.Chocolate, H * 0.0035f);
            y += ph * 0.15f;
        }

        float fb = H * 0.1f;
        if (DoceUI.Botao(new Rect(r.xMax - fb * 0.75f, r.y - fb * 0.25f, fb, fb), botaoFechar))
            ajudaAberta = false;
    }
}
