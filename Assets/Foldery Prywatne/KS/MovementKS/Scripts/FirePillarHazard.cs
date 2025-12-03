using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FirePillarHazard : MonoBehaviour
{
    [Header("Ustawienia")]
    public float lifetime = 5.0f;       // Ile sekund ogieñ p³onie na ziemi
    public float timeDamage = -10f;     // Ile zabiera czasu

    private bool hasDealtDamage = false;

    void Start()
    {
        // S³up ognia znika sam po okreœlonym czasie
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return;

        if (other.CompareTag("Player"))
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ModifyTime(timeDamage);
                Debug.Log("Gracz wszed³ w œcianê ognia!");
                hasDealtDamage = true; // Zadaj obra¿enia tylko raz na wejœcie
            }
        }
    }
}
