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
    [Tooltip("O ile zostanie zredukowana predkosc (np. 2.0).")]
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
                // aby efekt trwal nawet po zniszczeniu pocisku.
                // Najbezpieczniej odpalic to przez MonoBehavior gracza lub pomocnika:
                SlowEffectHelper helper = agent.GetComponent<SlowEffectHelper>();
                if (helper == null) helper = agent.gameObject.AddComponent<SlowEffectHelper>();

                helper.ApplySlow(agent, slowAmount, slowDuration);

                Debug.Log($"Pocisk trafil! Gracz spowolniony o {slowAmount} na {slowDuration}s.");
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

// Prosty skrypt pomocniczy, ktory zarzadza czasem trwania spowolnienia
public class SlowEffectHelper : MonoBehaviour
{
    private Coroutine slowCoroutine;
    private float appliedSlow = 0f;
    private PlayerControllerClick1 playerController1;
    private PlayerControllerClick playerController;

    private void Awake()
    {
        playerController1 = GetComponent<PlayerControllerClick1>();
        playerController = GetComponent<PlayerControllerClick>();
    }

    public void ApplySlow(NavMeshAgent agent, float amount, float duration)
    {
        if (playerController1 != null)
        {
            playerController1.AddSpeedModifier("SlowEffect", -amount, duration);
            return;
        }

        if (playerController != null)
        {
            playerController.AddSpeedModifier("SlowEffect", -amount, duration);
            return;
        }

        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            // Restore previous slow before calculating the new one to avoid compounding slows
            if (agent != null)
            {
                agent.speed += appliedSlow;
            }
            appliedSlow = 0f;
        }
        slowCoroutine = StartCoroutine(SlowRoutine(agent, amount, duration));
    }

    private IEnumerator SlowRoutine(NavMeshAgent agent, float amount, float duration)
    {
        // Safe slow amount: do not reduce speed below 0.5
        appliedSlow = Mathf.Min(amount, agent.speed - 0.5f);
        agent.speed -= appliedSlow;

        yield return new WaitForSeconds(duration);

        if (agent != null)
        {
            agent.speed += appliedSlow;
        }
        appliedSlow = 0f;
        slowCoroutine = null;
    }
}
