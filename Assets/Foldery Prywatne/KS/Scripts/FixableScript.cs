using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FixableScript : MonoBehaviour
{
    [Header("Objects to swap after interaction")]
    public GameObject brokenObject;
    public GameObject fixedObject;

    [Header("Item required to fix the object")]
    public GameObject hand; // Ten slot jest teraz NIEPOTRZEBNY dla logiki, ale zostawiam
    public string requiredItemName;

    [Header("UI")]
    public GameObject fixPromptUI;

    // Przechowuje referencjê do narzêdzia, gdy jest w zasiêgu
    private GameObject toolInRange = null;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (fixPromptUI != null)
            fixPromptUI.SetActive(false);
    }

    // ##### POPRAWIONY UPDATE #####
    // Przenios³em Fix() na zewn¹trz i uproœci³em logikê
    void Update()
    {
        // OnTriggerEnter wykona³ ju¿ ca³¹ pracê.
        // Wystarczy sprawdziæ, czy narzêdzie jest w zasiêgu i czy naciœniêto F.
        if (toolInRange != null && Input.GetKeyDown(KeyCode.F))
        {
            Fix();
        }
    }

    // ##### FUNKCJA PRZENIESIONA TUTAJ #####
    void Fix()
    {
        // Ta linia nie jest ju¿ potrzebna, bo Update to sprawdzi³, ale jest bezpieczna
        if (toolInRange == null) return;
        

        // 1. Zamieñ obiekty
        if (brokenObject != null) brokenObject.SetActive(false);
        if (fixedObject != null) fixedObject.SetActive(true);

        // 2. "Zu¿yj" narzêdzie - zniszcz obiekt narzêdzia
        // Odkomentowaæ poni¿sz¹ liniê, jeœli narzêdzie ma zostaæ zniszczone
        //Destroy(toolInRange.gameObject);

        // 3. Wy³¹cz ten skrypt i UI, aby nie mo¿na by³o go u¿yæ ponownie
        if (fixPromptUI != null)
            fixPromptUI.SetActive(false);

        Destroy(this); // Niszczy ten komponent FixableScript
    }

    // ##### OnTriggerEnter i OnTriggerExit zostaj¹ bez zmian #####

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            if (other.gameObject.name.StartsWith(requiredItemName))
            {
                Rigidbody toolRb = other.GetComponent<Rigidbody>();

                // SprawdŸ, czy narzêdzie jest trzymane (isKinematic)
                if (toolRb != null && toolRb.isKinematic)
                {
                    Debug.Log("W³aœciwe narzêdzie (" + other.name + ") jest w zasiêgu!");
                    toolInRange = other.gameObject; // Zapisz referencjê

                    if (fixPromptUI != null)
                        fixPromptUI.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == toolInRange)
        {
            Debug.Log("Narzêdzie opuœci³o zasiêg.");
            toolInRange = null; // Wyczyœæ referencjê

            if (fixPromptUI != null)
                fixPromptUI.SetActive(false);
        }
    }
}