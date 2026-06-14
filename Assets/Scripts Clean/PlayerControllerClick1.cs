using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerControllerClick1 : MonoBehaviour
{
    const string JUMP_START_STATE = "Jump_start";

    CustomActions input;
    NavMeshAgent agent;
    Animator animator;
    Camera mainCamera;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] float lookRotationSpeed = 8f;

    [Header("Speed Modification")]
    [SerializeField] float maxSpeed = 8.0f;
    [SerializeField] float minSpeed = 0.5f;

    [System.Serializable]
    public class SpeedModifier
    {
        public string id;
        public float amount;
        public float duration;
        public float timer;

        public SpeedModifier(string id, float amount, float duration)
        {
            this.id = id;
            this.amount = amount;
            this.duration = duration;
            this.timer = duration;
        }
    }

    private System.Collections.Generic.List<SpeedModifier> speedModifiers = new System.Collections.Generic.List<SpeedModifier>();
    private float baseSpeed = -1f;

    [Header("Interaction")]
    [Tooltip("Warstwa obiektów, w które można klikać (np. statki)")]
    [SerializeField] LayerMask interactableLayers;

    [Header("Jumping & Pivot Fix (Head Pivot)")]
    [SerializeField] float heightFromPivotToFeet = 1.2f;
    [SerializeField] float jumpHeight = 2.5f;
    [SerializeField] float jumpDuration = 0.6f;
    [SerializeField] float groundCheckDistance = 0.3f;

    private bool isJumpingInternal = false;
    private bool isGrounded;
    private bool isHoldingMove = false;
    private float nextMoveTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (agent != null)
        {
            baseSpeed = agent.speed;
        }

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

        // Tick speed modifiers
        bool speedChanged = false;
        for (int i = speedModifiers.Count - 1; i >= 0; i--)
        {
            speedModifiers[i].timer -= Time.deltaTime;
            if (speedModifiers[i].timer <= 0)
            {
                speedModifiers.RemoveAt(i);
                speedChanged = true;
            }
        }
        if (speedChanged)
        {
            RecalculateSpeed();
        }

        if (isJumpingInternal) return;

        if (isHoldingMove)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Time.time >= nextMoveTime)
            {
                MoveToCursor(false);
                nextMoveTime = Time.time + 0.1f;
            }
        }

        FaceTarget();
        SetAnimations();
    }

    void GroundCheck()
    {
        Vector3 feetPosition = transform.position - (Vector3.up * heightFromPivotToFeet);
        Vector3 rayStart = feetPosition + (Vector3.up * 0.1f);
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, Color.red);
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, clickableLayers);
    }

    void MoveToCursor(bool isInitialClick)
    {
        if (isJumpingInternal) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // 1. Sprawdzanie Interakcji (Tylko przy pojedynczym kliknięciu)
        if (isInitialClick && Physics.Raycast(ray, out hit, 100, interactableLayers))
        {
            Interactable interactableObject = hit.collider.GetComponent<Interactable>();

            if (interactableObject != null)
            {
                interactableObject.TriggerInteraction();
                StopMovement();
                return;
            }
        }

        // 2. Normalne poruszanie się
        if (Physics.Raycast(ray, out hit, 100, clickableLayers))
        {
            agent.SetDestination(hit.point);
            if (isInitialClick && clickEffect != null)
            {
                Instantiate(clickEffect, hit.point + Vector3.up * 0.1f, clickEffect.transform.rotation);
            }
        }
    }

    void TryJump()
    {
        if (isGrounded && !isJumpingInternal)
        {
            StartCoroutine(JumpArc());
        }
    }

    void StopMovement()
    {
        if (isJumpingInternal || !agent.enabled) return;
        isHoldingMove = false;

        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private IEnumerator JumpArc()
    {
        isJumpingInternal = true;

        animator.applyRootMotion = false;
        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", true);
        animator.Play(JUMP_START_STATE);

        Vector3 startPosition = transform.position; // Zapamiętujemy punkt startu
        Vector3 savedDestination = agent.destination;
        Vector3 horizontalVelocity = agent.velocity;

        if (horizontalVelocity.magnitude < 0.2f)
            horizontalVelocity = transform.forward * 2.0f;

        agent.enabled = false;

        float timeToPeak = jumpDuration / 2.0f;
        float gravity = (-2 * jumpHeight) / Mathf.Pow(timeToPeak, 2);
        float verticalVelocity = (2 * jumpHeight) / timeToPeak;

        yield return null;

        float elapsed = 0;

        // Zabezpieczenie czasowe, żeby pętla nie trwała w nieskończoność
        while (elapsed < jumpDuration * 2.5f)
        {
            elapsed += Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 moveVector = (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
            transform.position += moveVector;

            // Sprawdzanie lądowania tylko podczas opadania
            if (verticalVelocity < 0)
            {
                Vector3 feetPosition = transform.position - (Vector3.up * heightFromPivotToFeet);

                // SphereCast zamiast Raycasta - działa jak fizyczna stopa o promieniu 0.2f, patrzy 0.8f w dół
                if (Physics.SphereCast(feetPosition + Vector3.up * 0.5f, 0.2f, Vector3.down, out RaycastHit hitGround, 0.8f, clickableLayers))
                {
                    transform.position = hitGround.point + (Vector3.up * heightFromPivotToFeet);
                    break;
                }
            }

            // MECHANIZM RATUNKOWY: Jeśli postać spadła za bardzo (poza mapę)
            if (transform.position.y < startPosition.y - 10f)
            {
                Debug.LogWarning("Gracz wypadł poza mapę. Cofa na pozycję startową.");
                transform.position = startPosition;
                break;
            }

            yield return null;
        }

        animator.SetBool("isJumping", false);

        // PRZYCIĄGANIE DO NAVMESHA PRZED WŁĄCZENIEM AGENTA
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5.0f, NavMesh.AllAreas))
        {
            // Znaleziono NavMesh w promieniu 5 metrów, przyciągamy gracza
            transform.position = navHit.position;
            agent.enabled = true;
        }
        else
        {
            // Nie znaleziono podłogi (np. wylądował na dachu, który nie ma NavMesha). Cofa na start.
            transform.position = startPosition;
            agent.enabled = true;
        }

        // Dopiero gdy agent jest bezpiecznie włączony i na NavMeshu, możemy resetować trasę
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            if (savedDestination != Vector3.zero)
            {
                agent.SetDestination(savedDestination);
            }
        }

        isJumpingInternal = false;
    }

    void FaceTarget()
    {
        if (!agent.enabled || agent.velocity.sqrMagnitude < 0.1f) return;

        Vector3 direction = (agent.steeringTarget - transform.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    void SetAnimations()
    {
        if (isJumpingInternal) return;

        if (agent.enabled)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isWalking);
        }
    }

    public void AddSpeedModifier(string id, float amount, float duration)
    {
        if (agent == null) return;
        if (baseSpeed < 0f && agent != null) baseSpeed = agent.speed;

        SpeedModifier existing = speedModifiers.Find(m => m.id == id);
        if (existing != null)
        {
            existing.timer = duration;
            existing.amount = amount;
        }
        else
        {
            speedModifiers.Add(new SpeedModifier(id, amount, duration));
        }
        RecalculateSpeed();
    }

    public void RemoveSpeedModifier(string id)
    {
        int removed = speedModifiers.RemoveAll(m => m.id == id);
        if (removed > 0)
        {
            RecalculateSpeed();
        }
    }

    private void RecalculateSpeed()
    {
        if (agent == null) return;
        if (baseSpeed < 0f) baseSpeed = agent.speed;

        float totalMod = 0f;
        foreach (var mod in speedModifiers)
        {
            totalMod += mod.amount;
        }

        agent.speed = Mathf.Clamp(baseSpeed + totalMod, minSpeed, maxSpeed);
    }
}