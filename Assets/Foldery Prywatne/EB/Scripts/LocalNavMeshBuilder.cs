using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder; 

public class LocalNavMeshBuilder : MonoBehaviour
{
    [Header("Konfiguracja")]
    [Tooltip("Obiekt, za którym podąża NavMesh (Gracz)")]
    public Transform trackedTransform; 

    [Tooltip("Rozmiar obszaru, na którym działa NavMesh")]
    public Vector3 size = new Vector3(200, 60, 200); 

    [Tooltip("Wybierz tu warstwę 'Terrain'. Tylko obiekty z tej warstwy będą brane pod uwagę.")]
    public LayerMask layerMask; 

    NavMeshData m_NavMeshData;
    NavMeshDataInstance m_Instance;
    List<NavMeshBuildSource> m_Sources = new List<NavMeshBuildSource>();
    AsyncOperation m_Operation;
    bool m_IsBaking = false;

    void Start()
    {
        // Tworzymy pusty kontener na dane NavMesh
        m_NavMeshData = new NavMeshData();
        m_Instance = NavMesh.AddNavMeshData(m_NavMeshData);
        
        // Pierwsze, szybkie wygenerowanie (synchroniczne) na start
        UpdateNavMesh(false);
        
        // Uruchamiamy pętlę aktualizacji w tle
        StartCoroutine(UpdateNavMeshCoroutine());
    }

    void OnEnable()
    {
        m_NavMeshData = new NavMeshData();
        m_Instance = NavMesh.AddNavMeshData(m_NavMeshData);
    }

    void OnDisable()
    {
        m_Instance.Remove();
    }

    // Pętla sprawdzająca i aktualizująca NavMesh
    IEnumerator UpdateNavMeshCoroutine()
    {
        while (true)
        {
            UpdateNavMesh(true);
            yield return null; // Czekamy klatkę
        }
    }

    void UpdateNavMesh(bool asyncUpdate = false)
    {
        // Jeśli poprzednie pieczenie jeszcze trwa, czekamy
        if (m_IsBaking && m_Operation != null && !m_Operation.isDone) return;

        // Ustalenie środka NavMesha (pozycja gracza)
        Vector3 center = trackedTransform ? trackedTransform.position : transform.position;
        // Kwantyzacja (zaokrąglenie) pozycji, aby NavMesh nie przeliczał się przy minimalnym ruchu
        center = Quantize(center, 0.5f * size); 

        Bounds bounds = new Bounds(center, size);

        // Pobranie ustawień agenta (0 = Humanoid)
        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);

        // Zbieranie źródeł (Meshy) tylko z wybranej warstwy (Terrain)
        // To jest kluczowe - pobiera collidery ze wszystkich chunków w zasięgu i traktuje je jako całość
        m_Sources.Clear();
        NavMeshBuilder.CollectSources(
            bounds, 
            layerMask, 
            NavMeshCollectGeometry.PhysicsColliders, 
            0, 
            new List<NavMeshBuildMarkup>(), 
            m_Sources
        );

        if (asyncUpdate)
        {
            m_IsBaking = true;
            m_Operation = NavMeshBuilder.UpdateNavMeshDataAsync(m_NavMeshData, settings, m_Sources, bounds);
            StartCoroutine(WaitForBake());
        }
        else
        {
            // Opcja synchroniczna (startowa)
            NavMeshBuilder.UpdateNavMeshData(m_NavMeshData, settings, m_Sources, bounds);
        }
    }

    IEnumerator WaitForBake()
    {
        yield return m_Operation;
        m_IsBaking = false;
    }

    static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }

    // Rysowanie obszaru NavMesha w edytorze (żółta klatka)
    void OnDrawGizmosSelected()
    {
        if (m_NavMeshData)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawWireCube(m_NavMeshData.sourceBounds.center, m_NavMeshData.sourceBounds.size);
        }
        
        Gizmos.color = Color.yellow;
        Vector3 center = trackedTransform ? trackedTransform.position : transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}