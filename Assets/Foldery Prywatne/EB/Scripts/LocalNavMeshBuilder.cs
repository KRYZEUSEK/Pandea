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
        m_NavMeshData = new NavMeshData();
        m_Instance = NavMesh.AddNavMeshData(m_NavMeshData);

        UpdateNavMesh(false);
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

    IEnumerator UpdateNavMeshCoroutine()
    {
        while (true)
        {
            UpdateNavMesh(true);
            // ZMIANA: Zamiast odświeżać NavMesh w każdej klatce (co obciąża procesor),
            // system będzie go aktualizował 2 razy na sekundę. To w zupełności wystarczy dla AI.
            yield return new WaitForSeconds(0.5f);
        }
    }

    void UpdateNavMesh(bool asyncUpdate = false)
    {
        if (m_IsBaking && m_Operation != null && !m_Operation.isDone) return;

        Vector3 center = trackedTransform ? trackedTransform.position : transform.position;
        center = Quantize(center, 0.5f * size);

        Bounds bounds = new Bounds(center, size);

        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);

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