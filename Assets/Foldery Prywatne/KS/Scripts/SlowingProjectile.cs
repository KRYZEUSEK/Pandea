using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlowingProjectile : MonoBehaviour
{
    [Header("Ruch Pocisku")]
    public float speed = 8.0f;
    public float maxLifetime = 4.0f;

    [Header("Efekt Spowolnienia")]
    [Tooltip("O ile zostanie zredukowana prêdkoœæ (np. 2.0).")]
    public float slowAmount = 2.0f;
    [Tooltip("Na ile sekund gracz zostanie spowolniony.")]
    public float slowDuration = 3.0f;

    void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                // Uruchamiamy Coroutine na obiekcie gracza (lub przez managera), 
                // aby efekt trwa³ nawet po zniszczeniu pocisku.
                // Najbezpieczniej odpaliæ to przez MonoBehavior gracza lub pomocnika:
                SlowEffectHelper helper = agent.GetComponent<SlowEffectHelper>();
                if (helper == null) helper = agent.gameObject.AddComponent<SlowEffectHelper>();

                helper.ApplySlow(agent, slowAmount, slowDuration);

                Debug.Log($"Pocisk trafi³! Gracz spowolniony o {slowAmount} na {slowDuration}s.");
            }
            else
            {
                Debug.LogWarning("Gracz nie ma komponentu NavMeshAgent!");
            }

            Destroy(gameObject);
        }
        else if (!other.isTrigger && !other.GetComponent<BasePlant>())
        {
            Destroy(gameObject);
        }
    }
}

// Prosty skrypt pomocniczy, który zarz¹dza czasem trwania spowolnienia
public class SlowEffectHelper : MonoBehaviour
{
    private Coroutine slowCoroutine;
    private float originalSpeed;
    private bool isSlowed = false;

    public void ApplySlow(NavMeshAgent agent, float amount, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(agent, amount, duration));
    }

    private IEnumerator SlowRoutine(NavMeshAgent agent, float amount, float duration)
    {
        if (!isSlowed)
        {
            originalSpeed = agent.speed;
            isSlowed = true;
        }

        agent.speed = Mathf.Max(0.5f, originalSpeed - amount); // Zwalniamy, ale nie do zera

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
        isSlowed = false;
    }
}

