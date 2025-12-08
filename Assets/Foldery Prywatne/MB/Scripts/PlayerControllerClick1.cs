using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem; // Upewnij się, że masz tę linię
using UnityEngine.EventSystems;

public class PlayerControllerClick1 : MonoBehaviour
{
    // Animacje
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string JUMP_TRIGGER = "Jump"; // Trigger dla animacji skoku

    CustomActions input;

    NavMeshAgent agent;
    Animator animator;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] float lookRotationSpeed = 8f;

    [Header("Jumping")]
    [SerializeField] float jumpHeight = 2.0f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float groundCheckDistance = 0.3f;

    private bool isJumping = false;
    private bool isGrounded;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        input = new CustomActions();
        AssignInputs();
    }

    void AssignInputs()
    {
        // Kliknięcie myszką, aby się poruszyć
        input.Main.Move.performed += ctx => ClickToMove();

        // Naciśnięcie przycisku "Jump"
        input.Main.Jump.performed += ctx => TryJump();

        // --- NOWA LINIA ---
        // Naciśnięcie przycisku "Stop" (np. 'S')
        input.Main.Stop.performed += ctx => StopMovement();
    }

    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }

    // --- LOGIKA RUCHU I SKOKU ---

    private void Update()
    {
        // 1. Zawsze sprawdzaj, czy jesteśmy na ziemi
        GroundCheck();

        // 2. Jeśli skaczemy, korutyna JumpArc() przejmuje kontrolę.
        if (isJumping)
            return;

        // 3. Jeśli nie skaczemy, wykonuj normalną logikę
        FaceTarget();
        SetAnimations();
    }


    void ClickToMove()
    {
        // Jeśli klik jest na UI → ignoruj
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (isJumping)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100, clickableLayers))
        {
            agent.SetDestination(hit.point);
            if (clickEffect != null)
            {
                ParticleSystem newEffect = Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                Destroy(newEffect.gameObject, 2.0f);
            }
        }
    }


    void TryJump()
    {
        // Możemy skoczyć tylko jeśli jesteśmy na ziemi i aktualnie nie skaczemy
        if (isGrounded && !isJumping)
        {
            StartCoroutine(JumpArc());
        }
    }

    // --- NOWA FUNKCJA ---
    /// <summary>
    /// Zatrzymuje agenta NavMesh, anulując jego bieżącą ścieżkę.
    /// </summary>
    void StopMovement()
    {
        // Możemy zatrzymać agenta tylko wtedy, gdy jest na ziemi (i włączony)
        if (isJumping || !agent.enabled)
            return;

        // Ustawienie celu na bieżącą pozycję agenta jest
        // najlepszym sposobem na anulowanie jego ścieżki.
        agent.SetDestination(transform.position);
    }

    private IEnumerator JumpArc()
    {
        isJumping = true;
        animator.Play("Jump_start"); // Rozważ użycie SetTrigger("Jump_start")
                                     //animator.SetBool("isJumping", true);

        // Zapisz aktualny cel (jeśli 'S' zostało wciśnięte przed skokiem,
        // savedDestination będzie pozycją gracza, co jest poprawne)
        Vector3 savedDestination = agent.destination;

        Vector3 horizontalVelocity = agent.velocity;

        // 1. Wyłącz agenta
        agent.enabled = false;

        // --- Symulacja fizyki skoku ---
        float timeToPeak = jumpDuration / 2.0f;
        float gravity = (-2 * jumpHeight) / Mathf.Pow(timeToPeak, 2);
        float verticalVelocity = (2 * jumpHeight) / timeToPeak;

        Vector3 movement = (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        transform.position += movement;
        yield return null;


        while (true)
        {
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            bool hasLanded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, clickableLayers);

            if (hasLanded && verticalVelocity < 0)
            {
                break;
            }

            verticalVelocity += gravity * Time.deltaTime;
            movement = (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
            transform.position += movement;

            yield return null;
        }

        // --- Lądowanie ---
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 1.0f, clickableLayers))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }

        // 2. Włącz agenta z powrotem
        agent.enabled = true;

        // 3. Zsynchronizuj pozycję
        agent.Warp(transform.position);

        // 4. Przywróć zapisany cel
        agent.SetDestination(savedDestination);

        isJumping = false;
        //animator.SetBool("isJumping", false);
        //animator.SetBool("isWalking", true);
    }

    // --- FUNKCJE POMOCNICZE ---

    void GroundCheck()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, clickableLayers);
    }

    void FaceTarget()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
            return;

        Vector3 direction = (agent.destination - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
    }

    void SetAnimations()
    {
        bool isWalking = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isWalking);
    }
}