using UnityEngine;

// Ajudantes para desenhar textos e botões com cara de doce.
// Usado pela tela inicial e pela tela de "Ops! Você bateu!".
public static class DoceUI
{
    public static readonly Color Chocolate = new Color(0.36f, 0.16f, 0.07f);
    public static readonly Color RosaEscuro = new Color(0.55f, 0.05f, 0.35f);
    public static readonly Color Creme = new Color(1f, 0.94f, 0.62f);

    static Font fonte;
    static bool fonteCarregada;

    public static Font Fonte
    {
        get
        {
            if (!fonteCarregada)
            {
                fonte = Resources.Load<Font>("CandyUI/LilitaOne-Regular");
                fonteCarregada = true;
            }
            return fonte;
        }
    }

    // carrega uma imagem da pasta UI/Resources/CandyUI
    public static Texture2D Imagem(string nome)
    {
        return Resources.Load<Texture2D>("CandyUI/" + nome);
    }

    public static GUIStyle Estilo(int tamanho, TextAnchor alinhamento)
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        if (Fonte != null) s.font = Fonte;
        s.fontSize = Mathf.Max(1, tamanho);
        s.alignment = alinhamento;
        s.wordWrap = false;
        s.clipping = TextClipping.Overflow;
        return s;
    }

    // escreve um texto com contorno e sombrinha (fica bem "de jogo")
    public static void Texto(Rect r, string texto, int tamanho, Color cor, Color contorno,
                             float espessura, TextAnchor alinhamento = TextAnchor.MiddleCenter)
    {
        GUIStyle s = Estilo(tamanho, alinhamento);

        Pintar(s, new Color(0f, 0f, 0f, 0.3f * cor.a));   // sombra
        GUI.Label(new Rect(r.x, r.y + espessura * 2.5f, r.width, r.height), texto, s);

        Pintar(s, new Color(contorno.r, contorno.g, contorno.b, contorno.a * cor.a));   // contorno
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI * 2f / 16f;
            GUI.Label(new Rect(r.x + Mathf.Cos(a) * espessura, r.y + Mathf.Sin(a) * espessura,
                               r.width, r.height), texto, s);
        }

        Pintar(s, cor);   // texto
        GUI.Label(r, texto, s);
    }

    // O texto da Unity tem uma cor diferente para quando o mouse está em cima ("hover").
    // Aqui a gente pinta todos os estados com a mesma cor, para não mudar com o mouse.
    static void Pintar(GUIStyle s, Color c)
    {
        s.normal.textColor = c;
        s.hover.textColor = c;
        s.active.textColor = c;
        s.focused.textColor = c;
        s.onNormal.textColor = c;
        s.onHover.textColor = c;
        s.onActive.textColor = c;
        s.onFocused.textColor = c;
    }

    // botão de imagem que cresce um pouquinho quando o mouse passa por cima
    public static bool Botao(Rect r, Texture2D imagem, string texto = "", int tamanhoTexto = 0, float pulso = 0f)
    {
        bool emCima = r.Contains(Event.current.mousePosition);
        float escala = (emCima ? 1.08f : 1f) + pulso;
        Rect d = Escalar(r, escala);

        if (imagem != null) GUI.DrawTexture(d, imagem, ScaleMode.StretchToFill, true);

        if (!string.IsNullOrEmpty(texto))
        {
            int tam = Mathf.RoundToInt(tamanhoTexto * escala);
            Texto(new Rect(d.x, d.y - d.height * 0.05f, d.width, d.height), texto, tam,
                  Color.white, RosaEscuro, Mathf.Max(1.5f, tam * 0.07f));
        }

        return GUI.Button(d, GUIContent.none, GUIStyle.none);
    }

    public static Rect Escalar(Rect r, float e)
    {
        float w = r.width * e, h = r.height * e;
        return new Rect(r.center.x - w / 2f, r.center.y - h / 2f, w, h);
    }

    public static Texture2D Gradiente(Color cima, Color baixo)
    {
        Texture2D t = new Texture2D(1, 64);
        t.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 64; y++) t.SetPixel(0, y, Color.Lerp(baixo, cima, y / 63f));
        t.Apply();
        return t;
    }

    public static Texture2D Cor(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    // painel com bordas arredondadas que estica sem deformar os cantos
    public static void Painel(Rect r, Texture2D imagem)
    {
        if (imagem == null) return;
        GUIStyle s = new GUIStyle();
        s.normal.background = imagem;
        s.border = new RectOffset(60, 60, 60, 60);
        GUI.Box(r, GUIContent.none, s);
    }
}