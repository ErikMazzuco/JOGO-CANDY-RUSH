using UnityEngine;
using UnityEngine.Rendering;

// Deixa o jogo de noite e assustador quando você aperta Play.
// Pra voltar pro dia, é só desmarcar a caixinha deste script no Inspector.
// (Ele só muda as coisas enquanto o jogo roda: a cena salva continua de dia.)
public class ModoNoite : MonoBehaviour
{
    [Header("Lua")]
    public Color corDaLua = new Color(0.55f, 0.6f, 1f);
    public float forcaDaLua = 0.35f;
    public float alturaDaLua = 20f;           // em graus, acima do horizonte

    [Header("Escuridão")]
    public Color luzAmbiente = new Color(0.10f, 0.06f, 0.16f);

    [Header("Neblina")]
    public bool neblina = true;
    public Color corDaNeblina = new Color(0.10f, 0.03f, 0.11f);
    public float densidadeDaNeblina = 0.018f;

    [Header("Lanterna do Toddynho")]
    public bool lanterna = true;
    public Color corDaLanterna = new Color(1f, 0.75f, 0.55f);
    public float forcaDaLanterna = 6f;
    public float alcanceDaLanterna = 20f;
    public float aberturaDaLanterna = 70f;     // em graus: quanto maior, mais larga a luz
    public float alturaDaLanterna = 2f;        // altura da "mão" que segura a lanterna
    public float inclinacaoDaLanterna = 20f;   // em graus: quanto ela aponta pra baixo, pro chão

    [Header("Luzes piscando")]
    public bool luzesPiscando = true;

    [Header("Relâmpagos")]
    public bool relampagos = true;
    public float tempoMinimoEntreRelampagos = 7f;
    public float tempoMaximoEntreRelampagos = 15f;

    Light lua;
    Light[] luzesDoMapa;
    float[] forcaOriginal;
    float[] sorteio;
    float proximoRelampago;
    float inicioDoRelampago = -10f;

    void Start()
    {
        Vector3 direcaoDaLua = Quaternion.Euler(-alturaDaLua, 0f, 0f) * Vector3.forward; // na frente do Toddynho

        // 1) céu de noite (com lua e estrelas)
        Material modelo = Resources.Load<Material>("Noite/CeuNoite");
        if (modelo != null)
        {
            Material ceu = new Material(modelo); // cópia, pra não estragar o arquivo original
            Matrix4x4 posicaoDaLua = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(-direcaoDaLua), Vector3.one).inverse;
            ceu.SetMatrix("moonMatrix", posicaoDaLua);
            RenderSettings.skybox = ceu;
        }

        // 2) tudo mais escuro
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = luzAmbiente;

        // 3) neblina roxa
        RenderSettings.fog = neblina;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = corDaNeblina;
        RenderSettings.fogDensity = densidadeDaNeblina;

        // 4) o "sol" vira lua; as outras luzes do mapa vão piscar
        System.Collections.Generic.List<Light> pontos = new System.Collections.Generic.List<Light>();
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional && lua == null) lua = l;
            else if (l.type == LightType.Point || l.type == LightType.Spot) pontos.Add(l);
        }
        if (lua != null)
        {
            lua.color = corDaLua;
            lua.intensity = forcaDaLua;
            lua.transform.rotation = Quaternion.LookRotation(-direcaoDaLua);
            RenderSettings.sun = lua;
        }

        luzesDoMapa = pontos.ToArray();
        forcaOriginal = new float[luzesDoMapa.Length];
        sorteio = new float[luzesDoMapa.Length];
        for (int i = 0; i < luzesDoMapa.Length; i++)
        {
            forcaOriginal[i] = luzesDoMapa[i].intensity;
            sorteio[i] = Random.Range(0f, 100f); // cada luz pisca de um jeito
        }

        // 5) lanterna que acompanha o Toddynho, pra dar pra ver a pista
        if (lanterna) CriarLanterna();

        DynamicGI.UpdateEnvironment();
        proximoRelampago = Time.unscaledTime + Random.Range(tempoMinimoEntreRelampagos, tempoMaximoEntreRelampagos);
    }

    void CriarLanterna()
    {
        GameObject jogador = GameObject.FindGameObjectWithTag("Player");
        if (jogador == null) return;

        // a lanterna fica na frente do Toddynho, na altura da "mão",
        // apontando pra frente e um pouco pra baixo (igual farol de carro)
        GameObject go = new GameObject("Lanterna");
        go.transform.position = jogador.transform.position + Vector3.up * alturaDaLanterna + Vector3.forward * 0.5f;
        go.transform.rotation = Quaternion.Euler(inclinacaoDaLanterna, 0f, 0f); // olhando pra frente (+Z)
        go.transform.SetParent(jogador.transform, true);

        Light luz = go.AddComponent<Light>();
        luz.type = LightType.Spot;            // spot = luz em formato de cone, como lanterna
        luz.color = corDaLanterna;
        luz.intensity = forcaDaLanterna;
        luz.range = alcanceDaLanterna;
        luz.spotAngle = aberturaDaLanterna;
        luz.innerSpotAngle = aberturaDaLanterna * 0.5f; // borda suave
        luz.shadows = LightShadows.None;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        // luzes do mapa tremendo, como vela
        if (luzesPiscando && luzesDoMapa != null)
        {
            for (int i = 0; i < luzesDoMapa.Length; i++)
            {
                if (luzesDoMapa[i] == null) continue;
                float tremida = Mathf.PerlinNoise(t * 4f + sorteio[i], sorteio[i]);
                float apagao = Mathf.PerlinNoise(t * 0.7f + sorteio[i], 7f) < 0.25f ? 0.15f : 1f; // às vezes quase apaga
                luzesDoMapa[i].intensity = forcaOriginal[i] * (0.55f + 0.45f * tremida) * apagao;
            }
        }

        // relâmpago: dois clarões rápidos
        if (relampagos)
        {
            if (t >= proximoRelampago)
            {
                inicioDoRelampago = t;
                proximoRelampago = t + Random.Range(tempoMinimoEntreRelampagos, tempoMaximoEntreRelampagos);
            }

            float d = t - inicioDoRelampago;
            float clarao = (d < 0.08f) ? 1f : (d > 0.16f && d < 0.24f) ? 0.7f : 0f;

            RenderSettings.ambientLight = Color.Lerp(luzAmbiente, new Color(0.75f, 0.7f, 1f), clarao);
            if (lua != null) lua.intensity = Mathf.Lerp(forcaDaLua, 2.5f, clarao);
        }
    }
}
