using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Ferramentas que aparecem no menu "Toddynho", lá em cima na Unity.
public static class FerramentasToddynho
{
    // ---------- 1) URSOS, SORVETES E PIRULITOS CAINDO ----------

    static readonly string[] palavrasQueCaem =
    {
        "bear", "urso", "ice cream", "ice_cream", "icecream", "popsicle", "sorvete", "lollipop"
    };

    const float alturaMinimaParaCair = 3f; // enfeites pequenininhos não caem

    [MenuItem("Toddynho/1 - Fazer ursos e sorvetes cairem")]
    static void FazerCair()
    {
        List<string> nomes = new List<string>();

        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            bool jaCai = t.GetComponent<PirulitoCaindo>() != null;
            if (!jaCai)
            {
                if (!Combina(t.name)) continue;
                if (PaiCombina(t.parent)) continue;               // pega só o objeto "de fora"
                Bounds? caixa = Medir(t.gameObject);
                if (caixa == null || caixa.Value.size.y < alturaMinimaParaCair) continue;
                Undo.AddComponent<PirulitoCaindo>(t.gameObject);
            }

            if (t.GetComponent<Obstaculo>() == null) Undo.AddComponent<Obstaculo>(t.gameObject);
            ColocarColisores(t.gameObject);
            TirarStatic(t.gameObject);
            nomes.Add(t.name);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Objetos que caem (" + nomes.Count + "):\n" + string.Join("\n", nomes));
        EditorUtility.DisplayDialog("Pronto!", nomes.Count + " objetos agora caem e viram obstáculo.\n\n" +
            string.Join("\n", nomes.GetRange(0, Mathf.Min(25, nomes.Count))), "OK");
    }

    static bool Combina(string nome)
    {
        string n = nome.ToLower();
        foreach (string p in palavrasQueCaem) if (n.Contains(p)) return true;
        return false;
    }

    static bool PaiCombina(Transform pai)
    {
        while (pai != null)
        {
            if (Combina(pai.name) || pai.GetComponent<PirulitoCaindo>() != null) return true;
            pai = pai.parent;
        }
        return false;
    }

    static void ColocarColisores(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>() != null) return; // já tem
        foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            MeshCollider mc = Undo.AddComponent<MeshCollider>(mf.gameObject);
            mc.sharedMesh = mf.sharedMesh;
        }
    }

    static void TirarStatic(GameObject go)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
        {
            if (GameObjectUtility.GetStaticEditorFlags(t.gameObject) == 0) continue;
            Undo.RecordObject(t.gameObject, "Tirar Static");
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
        }
    }

    // ---------- 2) OBSTÁCULOS NA PISTA ----------

    static readonly string[] prefabsDeObstaculo =
    {
        "Assets/Styloo/3D Food Asset Pack/Prefabs/cakechocolatewithtop.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/cakewhite.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/cakechocolate.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/icecreamChocowithtop.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/icecreamStrawberrywithtop.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/icecreamvanillawithtop.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/macaron.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/pancakeall.prefab",
        "Assets/Styloo/3D Food Asset Pack/Prefabs/candy.prefab",
        "Assets/Alexproxy/Sweet Gummy Candies/Prefabs/Gummy Large.prefab",
        "Assets/Alexproxy/Sweet Gummy Candies/Prefabs/Gummy Cube (1).prefab",
        "Assets/Alexproxy/Sweet Gummy Candies/Prefabs/Gummy Cube (3).prefab",
    };

    const float alturaDoObstaculo = 2f;    // dá pra pular por cima
    const float larguraMaxima = 3f;        // cabe dentro de uma pista
    const float espacoMinimo = 22f;        // distância entre uma fileira e outra
    const float espacoMaximo = 32f;
    const float distanciaDoInicio = 50f;   // começa um pouco depois da largada

    [MenuItem("Toddynho/2 - Criar obstaculos na pista")]
    static void CriarObstaculos()
    {
        RunnerController corredor = Object.FindFirstObjectByType<RunnerController>();
        if (corredor == null)
        {
            EditorUtility.DisplayDialog("Ops", "Não achei o Toddynho (script RunnerController) na cena.", "OK");
            return;
        }

        // carrega os modelos
        List<GameObject> modelos = new List<GameObject>();
        foreach (string caminho in prefabsDeObstaculo)
        {
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (p != null) modelos.Add(p);
        }
        if (modelos.Count == 0)
        {
            EditorUtility.DisplayDialog("Ops", "Não achei nenhum modelo de doce para usar.", "OK");
            return;
        }

        // apaga os obstáculos antigos (se já tiver rodado antes)
        GameObject antigo = GameObject.Find("OBSTACULOS");
        if (antigo != null) Undo.DestroyObjectImmediate(antigo);

        GameObject grupo = new GameObject("OBSTACULOS");
        Undo.RegisterCreatedObjectUndo(grupo, "Criar obstáculos");

        float centroX = corredor.transform.position.x;
        float pista = corredor.laneDistance;
        float zInicio = corredor.transform.position.z + distanciaDoInicio;
        float zFim = FimDoTerreno(centroX) - 30f;

        int total = 0;
        float z = zInicio;
        while (z < zFim)
        {
            // às vezes bloqueia 1 pista, às vezes 2 (nunca as 3)
            List<int> pistas = new List<int> { 0, 1, 2 };
            Embaralhar(pistas);
            int quantas = Random.value < 0.3f ? 2 : 1;

            for (int i = 0; i < quantas; i++)
            {
                float x = centroX + (pistas[i] - 1) * pista;
                if (CriarUm(modelos[Random.Range(0, modelos.Count)], grupo.transform, new Vector3(x, 0, z)))
                    total++;
            }
            z += Random.Range(espacoMinimo, espacoMaximo);
        }

        Selection.activeGameObject = grupo;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Pronto!", total + " obstáculos criados na pista.\n\n" +
            "Eles estão dentro do objeto OBSTACULOS.\nRodar de novo troca por outros.", "OK");
    }

    static bool CriarUm(GameObject modelo, Transform pai, Vector3 lugar)
    {
        float chao;
        if (!AlturaDoChao(lugar, out chao)) return false;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(modelo, pai.gameObject.scene);
        Undo.RegisterCreatedObjectUndo(obj, "Criar obstáculo");
        obj.transform.SetParent(pai, true);
        obj.transform.position = new Vector3(lugar.x, chao, lugar.z);
        obj.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0) * obj.transform.rotation;

        // deixa no tamanho certo
        Bounds? medida = Medir(obj);
        if (medida == null) { Object.DestroyImmediate(obj); return false; }
        Bounds b = medida.Value;
        float fator = Mathf.Min(alturaDoObstaculo / Mathf.Max(b.size.y, 0.01f),
                                larguraMaxima / Mathf.Max(b.size.x, b.size.z, 0.01f));
        obj.transform.localScale *= fator;

        // encosta no chão e centraliza na pista
        b = Medir(obj).Value;
        obj.transform.position += new Vector3(lugar.x - b.center.x, chao - b.min.y, lugar.z - b.center.z);
        b = Medir(obj).Value;

        // tira colisores antigos dos filhos e coloca uma caixa só, do tamanho do doce
        foreach (Collider c in obj.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        BoxCollider caixa = obj.AddComponent<BoxCollider>();
        caixa.center = obj.transform.InverseTransformPoint(b.center);
        Vector3 tamanho = obj.transform.InverseTransformVector(b.size);
        caixa.size = new Vector3(Mathf.Abs(tamanho.x), Mathf.Abs(tamanho.y), Mathf.Abs(tamanho.z));

        obj.AddComponent<Obstaculo>();
        TirarStatic(obj);
        return true;
    }

    // ---------- ajudantes ----------

    static Bounds? Medir(GameObject go)
    {
        Renderer[] partes = go.GetComponentsInChildren<Renderer>();
        if (partes.Length == 0) return null;
        Bounds caixa = partes[0].bounds;
        foreach (Renderer r in partes) caixa.Encapsulate(r.bounds);
        return caixa;
    }

    // acha o chão de verdade: o ponto mais alto de terreno embaixo do lugar
    static bool AlturaDoChao(Vector3 ponto, out float altura)
    {
        bool achou = false;
        altura = float.MinValue;

        Vector3 origem = new Vector3(ponto.x, 1000f, ponto.z);
        foreach (RaycastHit hit in Physics.RaycastAll(origem, Vector3.down, 3000f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider is TerrainCollider && hit.point.y > altura)
            {
                altura = hit.point.y;
                achou = true;
            }
        }
        if (achou) return true;

        foreach (Terrain t in Terrain.activeTerrains)
        {
            Vector3 inicio = t.transform.position;
            Vector3 tam = t.terrainData.size;
            if (ponto.x >= inicio.x && ponto.x <= inicio.x + tam.x &&
                ponto.z >= inicio.z && ponto.z <= inicio.z + tam.z)
            {
                altura = Mathf.Max(altura, t.SampleHeight(ponto) + inicio.y);
                achou = true;
            }
        }
        return achou;
    }

    static float FimDoTerreno(float x)
    {
        float fim = 0;
        foreach (Terrain t in Terrain.activeTerrains)
        {
            Vector3 inicio = t.transform.position;
            Vector3 tam = t.terrainData.size;
            if (x >= inicio.x && x <= inicio.x + tam.x)
                fim = Mathf.Max(fim, inicio.z + tam.z);
        }
        return fim;
    }

    static void Embaralhar(List<int> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = lista[i]; lista[i] = lista[j]; lista[j] = tmp;
        }
    }
}
