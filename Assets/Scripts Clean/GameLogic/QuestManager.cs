using UnityEngine;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Kolejność Celów")]
    [Tooltip("Przeciągnij tutaj prefaby celów w kolejności, w jakiej gracz ma je rozwiązywać (np. 1. Radar, 2. Pudełko, 3. Wieża).")]
    public GameObject[] questSequencePrefabs;

    private int currentStepIndex = 0;
    private GameObject[] spawnedInstances;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ponieważ obiekty są spawnowane dynamicznie przez EndlessTerrain w metodzie Start(),
        // musimy poczekać 1 klatkę, aż zostaną poprawnie zainicjalizowane w hierarchii.
        StartCoroutine(InitializeQuestChain());
    }

    private IEnumerator InitializeQuestChain()
    {
        yield return null; // Czekamy na klatkę spawnu

        if (questSequencePrefabs == null || questSequencePrefabs.Length == 0)
        {
            Debug.LogWarning("QuestManager: Lista prefabów celów jest pusta!");
            yield break;
        }

        spawnedInstances = new GameObject[questSequencePrefabs.Length];

        // Szukamy zespawnowanych instancji w scenie na podstawie nazwy klonu
        for (int i = 0; i < questSequencePrefabs.Length; i++)
        {
            string cloneName = questSequencePrefabs[i].name + "(Clone)";
            GameObject instance = GameObject.Find(cloneName);
            
            if (instance != null)
            {
                spawnedInstances[i] = instance;
                
                // Aktywujemy tag "Objective1" tylko dla pierwszego kroku, a resztę ustawiamy na "Untagged"
                if (i == 0)
                {
                    instance.tag = "Objective1";
                    Debug.Log($"[QuestManager] Pierwszy cel aktywowany: {instance.name}");
                }
                else
                {
                    instance.tag = "Untagged";
                }
            }
            else
            {
                Debug.LogWarning($"QuestManager: Nie znaleziono zespawnowanej instancji w scenie dla prefabu: {questSequencePrefabs[i].name}");
            }
        }
    }

    /// <summary>
    /// Wywoływane, gdy gracz ukończy aktualny krok (np. naprawi radar lub rozwiąże zagadkę).
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (spawnedInstances == null || currentStepIndex >= spawnedInstances.Length) return;

        Debug.Log($"[QuestManager] Krok {currentStepIndex} ukończony!");

        // Zdejmujemy tag z ukończonego celu (na wypadek gdyby nadal istniał w scenie)
        if (spawnedInstances[currentStepIndex] != null)
        {
            spawnedInstances[currentStepIndex].tag = "Untagged";
        }

        // Przechodzimy do kolejnego kroku
        currentStepIndex++;

        // Aktywujemy kolejny cel
        if (currentStepIndex < spawnedInstances.Length)
        {
            if (spawnedInstances[currentStepIndex] != null)
            {
                spawnedInstances[currentStepIndex].tag = "Objective1";
                Debug.Log($"[QuestManager] Nowy cel aktywowany: {spawnedInstances[currentStepIndex].name} (Krok {currentStepIndex})");
            }
            else
            {
                Debug.LogWarning($"[QuestManager] Cel dla kroku {currentStepIndex} jest nullem!");
            }
        }
        else
        {
            Debug.Log("[QuestManager] Wszystkie cele misji zostały ukończone!");
        }
    }
}
