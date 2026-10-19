
using UnityEngine;

public class NuvemFlutuante : MonoBehaviour
{
    public float altura = 0.5f;
    public float velocidade = 1f;

    private Vector3 posicaoInicial;

    void Start()
    {
        posicaoInicial = transform.position;
    }

    void Update()
    {
        float movimento = Mathf.Sin(Time.time * velocidade) * altura;

        transform.position = posicaoInicial
            + Vector3.up * movimento;
    }
}
