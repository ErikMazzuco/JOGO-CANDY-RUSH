using UnityEngine;

public class PirulitoCaindo : MonoBehaviour
{
    public Transform toddynho;              // o Toddynho (se ficar vazio, procura pela tag Player)
    public float distanciaParaCair = 30f;   // quão perto o Toddynho precisa chegar
    public float anguloFinal = 90f;         // ângulo máximo, caso não encoste no chão antes
    public float aceleracao = 200f;         // quanto maior, mais rápido cai
    public float centroDaPistaX = 45.3f;    // o meio da pista, pra ele cair pra dentro dela

    bool caindo = false;
    bool terminou = false;
    float anguloAtual = 0f;
    float velocidade = 0f;

    Vector3 pontoDaBase;   // o "pé" do pirulito
    Vector3 direcao;       // pra onde o palito está apontando agora
    Vector3 eixo;          // em volta de qual eixo ele gira
    float altura;          // altura total do pirulito
    float raioCabeca;      // tamanho da "bola" do pirulito

    void Start()
    {
        if (toddynho == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) toddynho = p.transform;
        }

        // mede o pirulito: onde fica o pé, a altura e o tamanho da cabeça
        Renderer[] partes = GetComponentsInChildren<Renderer>();
        if (partes.Length > 0)
        {
            Bounds caixa = partes[0].bounds;
            foreach (Renderer r in partes) caixa.Encapsulate(r.bounds);
            pontoDaBase = new Vector3(caixa.center.x, caixa.min.y, caixa.center.z);
            altura = caixa.size.y;
            raioCabeca = Mathf.Max(caixa.size.x, caixa.size.z) / 2f;
        }
        else
        {
            pontoDaBase = transform.position;
            altura = 5f;
            raioCabeca = 1f;
        }

        direcao = Vector3.up;

        // se está à esquerda da pista, cai pra direita, e vice-versa
        eixo = transform.position.x < centroDaPistaX ? Vector3.back : Vector3.forward;
    }

    void Update()
    {
        if (toddynho == null || terminou) return;

        if (!caindo)
        {
            float distancia = transform.position.z - toddynho.position.z;
            if (distancia > 0 && distancia < distanciaParaCair) caindo = true;
            return;
        }

        velocidade += aceleracao * Time.deltaTime;
        float passo = Mathf.Min(velocidade * Time.deltaTime, anguloFinal - anguloAtual);

        transform.RotateAround(pontoDaBase, eixo, passo);
        direcao = Quaternion.AngleAxis(passo, eixo) * direcao;
        anguloAtual += passo;

        if (anguloAtual >= anguloFinal || EncostouNoChao())
            terminou = true;
    }

    // confere se alguma parte do pirulito já chegou no chão
    bool EncostouNoChao()
    {
        if (anguloAtual < 10f) return false; // no comecinho ainda não precisa checar

        // alguns pontos ao longo do palito
        float[] pedacos = { 0.35f, 0.6f, 0.85f };
        foreach (float f in pedacos)
        {
            Vector3 ponto = pontoDaBase + direcao * (altura * f);
            if (ponto.y <= AlturaDoChao(ponto) + 0.2f) return true;
        }

        // a parte de baixo da cabeça
        Vector3 centroCabeca = pontoDaBase + direcao * (altura - raioCabeca);
        if (centroCabeca.y - raioCabeca <= AlturaDoChao(centroCabeca)) return true;

        return false;
    }

    // descobre a altura do chão (terreno) embaixo de um ponto
    float AlturaDoChao(Vector3 ponto)
    {
        float altura = float.MinValue;

        Vector3 origem = new Vector3(ponto.x, 1000f, ponto.z);
        foreach (RaycastHit hit in Physics.RaycastAll(origem, Vector3.down, 3000f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider is TerrainCollider && hit.point.y > altura)
                altura = hit.point.y;
        }
        if (altura > float.MinValue) return altura;

        foreach (Terrain t in Terrain.activeTerrains)
        {
            Vector3 inicio = t.transform.position;
            Vector3 tamanho = t.terrainData.size;
            if (ponto.x >= inicio.x && ponto.x <= inicio.x + tamanho.x &&
                ponto.z >= inicio.z && ponto.z <= inicio.z + tamanho.z)
            {
                altura = Mathf.Max(altura, t.SampleHeight(ponto) + inicio.y);
            }
        }
        if (altura > float.MinValue) return altura;
        return pontoDaBase.y; // se não achar terreno, usa a altura do pé
    }
}
