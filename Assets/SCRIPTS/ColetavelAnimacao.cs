
using UnityEngine;

public class ColetavelAnimacao : MonoBehaviour
{
    [Header("Rotação")]
    public float velocidadeRotacao = 90f;

    [Header("Flutuação")]
    public float alturaFlutuacao = 0.25f;
    public float velocidadeFlutuacao = 2f;

    private Vector3 posicaoInicial;

    void Start()
    {
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // Gira o coletável no próprio eixo
        transform.Rotate(0f, velocidadeRotacao * Time.deltaTime, 0f);

        // Faz o coletável flutuar suavemente
        float movimento = Mathf.Sin(Time.time * velocidadeFlutuacao)
                          * alturaFlutuacao;

        transform.position = posicaoInicial
                             + Vector3.up * movimento;
    }
}
