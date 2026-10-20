using UnityEngine;

// Um sushi que o Toddynho pode pegar. Ele sobe e desce bem devagarinho;
// quando o Toddynho passa perto, ele dá um "pulinho", some e conta no placar.
public class SushiColetavel : MonoBehaviour
{
    public float raioParaPegar = 2f;      // quão perto o Toddynho precisa passar
    public float alturaParaPegar = 5f;    // diferença de altura máxima (menor = tem que pular pra pegar)
    public float alturaDoBalanco = 0.2f;     // quanto sobe e desce
    public float velocidadeDoBalanco = 1.5f; // quão rápido sobe e desce

    Transform jogador;
    Vector3 posicaoInicial;
    Vector3 tamanhoInicial;
    float atraso;
    bool pego;
    float horaQuePegou;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;
        posicaoInicial = transform.position;
        tamanhoInicial = transform.localScale;
        atraso = Random.Range(0f, 6f); // cada sushi balança num ritmo
        GerenciadorDeSushis.Registrar();
    }

    void Update()
    {
        if (pego)
        {
            // animação de "pegou!": cresce, sobe e some
            float d = Time.time - horaQuePegou;
            transform.localScale = tamanhoInicial * (1f + d * 4f) * Mathf.Max(0f, 1f - d * 5f);
            transform.position += Vector3.up * 6f * Time.deltaTime;
            if (d > 0.2f) Destroy(gameObject);
            return;
        }

        transform.position = posicaoInicial + Vector3.up * Mathf.Sin((Time.time + atraso) * velocidadeDoBalanco) * alturaDoBalanco;

        if (jogador == null) return;
        Vector3 diferenca = jogador.position - transform.position;
        float alturaDiferente = Mathf.Abs(diferenca.y);
        diferenca.y = 0f; // olha só a distância "no chão"
        if (diferenca.magnitude < raioParaPegar && alturaDiferente < alturaParaPegar)
        {
            pego = true;
            horaQuePegou = Time.time;
            GerenciadorDeSushis.Pegou();
        }
    }
}
