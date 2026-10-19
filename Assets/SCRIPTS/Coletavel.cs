
using UnityEngine;

public class Coletavel : MonoBehaviour
{
    public int valor = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pontuacao pontuacao =
                FindObjectOfType<Pontuacao>();

            if (pontuacao != null)
            {
                pontuacao.AdicionarPontos(valor);
                Destroy(gameObject);
            }
        }
    }
}
