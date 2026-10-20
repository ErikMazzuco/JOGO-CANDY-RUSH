using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Conta os sushis, mostra o placar e, na linha de chegada,
// diz se ganhou (pegou todos) ou perdeu (faltou algum).
public class GerenciadorDeSushis : MonoBehaviour
{
    public float zDaChegada = 600f;          // onde fica a linha de chegada
    public AudioClip somDePegar;              // som quando pega um sushi
    [Range(0f, 1f)] public float volumeDoSom = 0.3f;   // 0 = mudo, 1 = volume máximo
    public bool esconderPlacar = false;       // marque pra esconder o "SUSHIS 0 / 18" do topo da tela

    [Header("Fica mais rápido a cada sushi")]
    public float aumentoPorSushi = 0.6f;      // quanto a velocidade aumenta a cada sushi
    public float velocidadeMaxima = 16f;      // limite, pra não ficar impossível

    [Header("Textos")]
    public string textoGanhou = "VOCÊ GANHOU!";
    public string textoPerdeu = "QUASE!";

    static GerenciadorDeSushis atual;

    int total;
    int pegos;
    float horaUltimoSushi = -10f;
    bool acabou;
    bool ganhou;
    float horaDoFim;

    Transform jogador;
    List<Coletavel> sushisDoErik;   // os sushis do Erik (script Coletavel) também contam
    RunnerController corredor;
    Camera cameraDoJogo;
    float campoDeVisaoInicial;
    float velocidadeInicial;
    AudioSource caixaDeSom;
    Texture2D escurecer, painel, botaoDeNovo, botaoMenu;

    void Awake()
    {
        atual = this;
        escurecer = DoceUI.Cor(new Color(0.2f, 0.05f, 0.1f, 0.5f));
        painel = DoceUI.Imagem("painel_laranja");
        botaoDeNovo = DoceUI.Imagem("botao_jogar_de_novo");
        botaoMenu = DoceUI.Imagem("botao_menu");
    }

    void Start()
    {
        // os sushis do Erik: entram no total, e quando somem (foram pegos) contam como pegos
        sushisDoErik = new List<Coletavel>(Object.FindObjectsByType<Coletavel>(FindObjectsSortMode.None));
        total += sushisDoErik.Count;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            jogador = p.transform;
            corredor = p.GetComponent<RunnerController>();
            if (corredor != null) velocidadeInicial = corredor.forwardSpeed;
        }
        cameraDoJogo = Camera.main;
        if (cameraDoJogo != null) campoDeVisaoInicial = cameraDoJogo.fieldOfView;
        caixaDeSom = gameObject.AddComponent<AudioSource>();
        caixaDeSom.playOnAwake = false;
    }

    // cada sushi chama isto quando o jogo começa ("estou aqui!"), assim a conta do total fica certa
    public static void Registrar()
    {
        if (atual != null) atual.total++;
    }

    // os sushis chamam isto quando são pegos
    public static void Pegou()
    {
        if (atual == null || atual.acabou) return;
        atual.pegos++;
        atual.horaUltimoSushi = Time.unscaledTime;
        if (atual.somDePegar != null) atual.caixaDeSom.PlayOneShot(atual.somDePegar, atual.volumeDoSom);

        // cada sushi deixa o Toddynho um pouquinho mais rápido
        if (atual.corredor != null)
            atual.corredor.forwardSpeed = Mathf.Min(atual.corredor.forwardSpeed + atual.aumentoPorSushi, atual.velocidadeMaxima);
    }

    void Update()
    {
        // a câmera "abre" um pouco quando está mais rápido (dá sensação de velocidade)
        if (cameraDoJogo != null && corredor != null && velocidadeInicial > 0f)
        {
            float extra = (corredor.forwardSpeed - velocidadeInicial) * 1.2f;
            cameraDoJogo.fieldOfView = Mathf.Lerp(cameraDoJogo.fieldOfView, campoDeVisaoInicial + extra, Time.deltaTime * 3f);
        }

        // um sushi do Erik sumiu? então o Toddynho pegou ele
        if (sushisDoErik != null)
        {
            for (int i = sushisDoErik.Count - 1; i >= 0; i--)
            {
                if (sushisDoErik[i] == null)
                {
                    sushisDoErik.RemoveAt(i);
                    Pegou();
                }
            }
        }

        if (acabou || jogador == null) return;
        if (jogador.position.z >= zDaChegada) Terminar();
    }

    void Terminar()
    {
        acabou = true;
        ganhou = pegos >= total;
        horaDoFim = Time.unscaledTime;

        // para o Toddynho
        foreach (MonoBehaviour m in jogador.GetComponents<MonoBehaviour>()) m.enabled = false;
        Rigidbody corpo = jogador.GetComponent<Rigidbody>();
        if (corpo != null) corpo.isKinematic = true;
    }

    void Recarregar(bool voltarProMenu)
    {
        if (voltarProMenu) TelaInicial.jogoComecou = false;
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
        float W = Screen.width, H = Screen.height;

        // placar fixo no topo da tela (some só enquanto a tela inicial está aberta)
        if (!esconderPlacar && !acabou && Time.timeScale > 0f)
        {
            float pulo = Mathf.Clamp01(1f - (Time.unscaledTime - horaUltimoSushi) * 4f); // "pulinho" quando pega
            float placaW = H * 0.42f, placaH = H * 0.11f;
            Rect fundo = DoceUI.Escalar(new Rect(W / 2f - placaW / 2f, H * 0.02f, placaW, placaH), 1f + 0.12f * pulo);
            DoceUI.Texto(new Rect(fundo.x, fundo.y - fundo.height * 0.04f, fundo.width, fundo.height),
                         "SUSHIS  " + pegos + " / " + total,
                         Mathf.RoundToInt(H * 0.045f * (1f + 0.15f * pulo)), Color.white, DoceUI.Chocolate, H * 0.004f);
        }

        if (!acabou) return;
        GUI.depth = -10;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            e.Use();
            Recarregar(false);
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, W, H), escurecer);

        float entrada = Mathf.Clamp01((Time.unscaledTime - horaDoFim) / 0.25f);
        float escala = 0.85f + 0.15f * Mathf.Sin(entrada * Mathf.PI * 0.5f);

        float pw = Mathf.Min(W * 0.9f, H * 0.95f) * escala;
        float ph = H * 0.6f * escala;
        Rect r = new Rect(W / 2f - pw / 2f, H / 2f - ph / 2f, pw, ph);
        DoceUI.Painel(r, painel);

        string titulo = ganhou ? textoGanhou : textoPerdeu;
        string detalhe = ganhou
            ? "Você pegou todos os " + total + " sushis!"
            : (total - pegos == 1 ? "Faltou 1 sushi..." : "Faltaram " + (total - pegos) + " sushis...");

        DoceUI.Texto(new Rect(r.x, r.y + ph * 0.08f, pw, ph * 0.22f), titulo,
                     Mathf.RoundToInt(H * 0.085f * escala), Color.white, DoceUI.Chocolate, H * 0.006f);
        DoceUI.Texto(new Rect(r.x, r.y + ph * 0.3f, pw, ph * 0.12f), detalhe,
                     Mathf.RoundToInt(H * 0.04f * escala), DoceUI.Creme, DoceUI.Chocolate, H * 0.0035f);

        float b = H * 0.13f * escala;
        float y = r.y + ph * 0.5f;
        if (DoceUI.Botao(new Rect(W / 2f - b - H * 0.03f, y, b, b), botaoDeNovo)) Recarregar(false);
        if (DoceUI.Botao(new Rect(W / 2f + H * 0.03f, y, b, b), botaoMenu)) Recarregar(true);

        int tl = Mathf.RoundToInt(H * 0.03f * escala);
        DoceUI.Texto(new Rect(W / 2f - b - H * 0.03f - b * 0.5f, y + b, b * 2f, H * 0.05f),
                     ganhou ? "jogar de novo" : "tentar novamente", tl, DoceUI.Creme, DoceUI.Chocolate, H * 0.0025f);
        DoceUI.Texto(new Rect(W / 2f + H * 0.03f - b * 0.5f, y + b, b * 2f, H * 0.05f), "menu",
                     tl, DoceUI.Creme, DoceUI.Chocolate, H * 0.0025f);
    }
}
