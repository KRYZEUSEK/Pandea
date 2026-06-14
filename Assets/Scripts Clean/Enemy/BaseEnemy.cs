using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Statystyki Bazowe")]
    public float baseMoveSpeed = 3.5f;

    // Odniesienia
    protected Transform player;
    protected NavMeshAgent agent;

    // Metoda 'virtual' mo¿e byæ nadpisana, ale nie musi.
    // U¿ywamy Awake() do pobrania komponentów.
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = baseMoveSpeed;

        // ZnajdŸ gracza po tagu "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu gracza! Upewnij siê, ¿e gracz ma tag 'Player'.");
        }
    }

    // Update jest zapieczêtowany - klasy dziedzicz¹ce nie mog¹ go u¿yæ.
    // Zamiast tego, musz¹ u¿ywaæ naszej abstrakcyjnej metody.
    private void Update()
    {
        // Jeœli nie mamy gracza, nic nie rób
        if (player == null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        // Wywo³aj logikê specyficzn¹ dla danego typu przeciwnika
        UpdateEnemyBehavior();
    }

    // Metoda 'abstract' MUSI byæ zaimplementowana (nadpisana)
    // przez ka¿d¹ klasê, która dziedziczy z BaseEnemy.
    // To jest serce naszej rozszerzalnej logiki.
    protected abstract void UpdateEnemyBehavior();
}