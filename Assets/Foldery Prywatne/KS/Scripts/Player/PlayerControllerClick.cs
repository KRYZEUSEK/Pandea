using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerControllerClick : MonoBehaviour
{
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string JUMP_TRIGGER = "Jump";

    CustomActions input;
    NavMeshAgent agent;
    Animator animator;
    Camera mainCamera; // FIX: Cache kamery

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers; // Warstwa pod³ogi
    [SerializeField] LayerMask obstacleLayers;  // FIX: Nowa warstwa dla œcian (do skoku)
    [SerializeField] float lookRotationSpeed = 8f;

    [Header("Jumping")]
    [SerializeField] float jumpHeight = 2.0f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] float bodyRadius = 0.5f; // FIX: Promieñ kolizji gracza do skoku

    private bool isJumping = false;
    private bool isGrounded;
    private bool isHoldingMove = false;

    // FIX: Optymalizacja SetDestination
    private Vector3 lastMousePos;
    private float nextMoveTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main; // FIX: Przypisanie kamery raz

        input = new CustomActions();
        AssignInputs();
    }

    void AssignInputs()
    {
        input.Main.Move.performed += ctx => {
            isHoldingMove = true;
            MoveToCursor(true);
        };
        input.Main.Move.canceled += ctx => isHoldingMove = false;
        input.Main.Jump.performed += ctx => TryJump();
        input.Main.Stop.performed += ctx => StopMovement();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    private void Update()
    {
        GroundCheck();

        if (isJumping) return;

        if (isHoldingMove)
        {
            // FIX: Ograniczenie spamowania SetDestination (np. co 0.1s)
            if (Time.time >= nextMoveTime)
            {
                MoveToCursor(false);
                nextMoveTime = Time.time + 0.1f;
            }
        }

        FaceTarget();
        SetAnimations();
    }

    void MoveToCursor(bool spawnEffect)
    {
        if (isJumping) return;

        RaycastHit hit;
        // FIX: U¿ycie cache'owanej kamery
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100, clickableLayers))
        {
            agent.SetDestination(hit.point);

            if (spawnEffect && clickEffect != null)
            {
                ParticleSystem newEffect = Instantiate(clickEffect, hit.point + Vector3.up * 0.1f, clickEffect.transform.rotation);
                Destroy(newEffect.gameObject, 2.0f);
            }
        }
    }

    void TryJump()
    {
        if (isGrounded && !isJumping)
        {
            // isHoldingMove = false; // Opcjonalnie reset trzymania
            StartCoroutine(JumpArc());
        }
    }

    void StopMovement()
    {
        if (isJumping || !agent.enabled) return;
        isHoldingMove = false;
        agent.ResetPath(); // FIX: ResetPath jest czystsze ni¿ SetDestination(transform.position)
    }

    private IEnumerator JumpArc()
    {
        isJumping = true;
        animator.Play("Jump_start");

        Vector3 savedDestination = agent.destination;
        Vector3 horizontalVelocity = agent.velocity;

        // Zabezpieczenie: Jeœli skaczemy z miejsca (velocity ~ 0), 
        // przyjmijmy, ¿e ruchem jest to, gdzie postaæ patrzy.
        if (horizontalVelocity.magnitude < 0.1f)
        {
            horizontalVelocity = transform.forward * 0.1f; // Minimalny ruch w przód
        }

        agent.enabled = false;

        float timeToPeak = jumpDuration / 2.0f;
        float gravity = (-2 * jumpHeight) / Mathf.Pow(timeToPeak, 2);
        float verticalVelocity = (2 * jumpHeight) / timeToPeak;

        yield return null;

        while (true)
        {
            float deltaTime = Time.deltaTime;
            verticalVelocity += gravity * deltaTime;

            // --- NOWE: Rêczne obracanie postaci w locie ---
            // Sprawdzamy, czy poruszamy siê w poziomie, ¿eby nie obracaæ siê do (0,0,0)
            if (horizontalVelocity.sqrMagnitude > 0.05f)
            {
                // Obliczamy rotacjê w stronê, w któr¹ faktycznie lecimy
                Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);

                // P³ynnie obracamy postaæ (u¿ywaj¹c tej samej zmiennej lookRotationSpeed co przy chodzeniu)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * lookRotationSpeed);
            }
            // ----------------------------------------------

            // Przesuniêcie
            Vector3 moveVector = (horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime;

            // (Tu powinna byæ Twoja logika kolizji ze œcianami z poprzedniej rozmowy, jeœli j¹ doda³eœ)
            // ...

            transform.position += moveVector;

            // Logika l¹dowania
            if (verticalVelocity < 0)
            {
                Vector3 rayStart = transform.position + Vector3.up * 0.5f;
                // Zwiêkszy³em lekko dystans raycasta, ¿eby l¹dowanie by³o pewniejsze
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hitGround, 0.6f + groundCheckDistance, clickableLayers))
                {
                    transform.position = hitGround.point;
                    break;
                }
            }

            yield return null;
        }

        agent.enabled = true;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }

        if (savedDestination != Vector3.zero)
            agent.SetDestination(savedDestination);

        isJumping = false;
    }

    void GroundCheck()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, clickableLayers);
    }

    void FaceTarget()
    {
        // FIX: Sprawdzenie pozosta³ego dystansu mo¿e byæ myl¹ce przy "Hold to move", 
        // lepiej sprawdzaæ czy agent ma œcie¿kê (agent.hasPath) lub prêdkoœæ.
        if (agent.velocity.sqrMagnitude < 0.1f) return;

        // FIX: U¿ycie steeringTarget zamiast destination
        Vector3 targetPosition = agent.steeringTarget;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // FIX: Zabezpieczenie przed b³êdem Zero Vector
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    void SetAnimations()
    {
        // U¿ywamy velocity agenta, to najdok³adniejszy wskaŸnik ruchu
        bool isWalking = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isWalking);
    }
}