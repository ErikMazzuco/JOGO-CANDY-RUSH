
using UnityEngine;
using TMPro;

public class Pontuacao : MonoBehaviour
{
    public int pontos = 0;
    public TMP_Text textoPontuacao;

    void Start()
    {
        AtualizarTexto();
    }

    public void AdicionarPontos(int quantidade)
    {
        pontos += quantidade;
        AtualizarTexto();
    }

    void AtualizarTexto()
    {
        if (textoPontuacao != null)
        {
            textoPontuacao.text = "SUSHIS: " + pontos;
        }
    }
}
