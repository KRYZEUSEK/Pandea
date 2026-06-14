using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events; // NOWE: Wymagane do obs³ugi zdarzeñ w Inspektorze

public class PlayerControllerClick : MonoBehaviour
{
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string JUMP_TRIGGER = "Jump";

    CustomActions input;
    NavMeshAgent agent;
    Animator animator;
    Camera mainCamera;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] LayerMask obstacleLayers;
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

    [Header("Jumping")]
    [SerializeField] float jumpHeight = 2.0f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] float bodyRadius = 0.5f;

    // NOWE: Konfiguracja Interakcji / Easter Eggów
    [Header("Interaction & Easter Eggs")]
    [Tooltip("Warstwa obiektów, w które mo¿na klikaæ (np. Interactable)")]
    [SerializeField] LayerMask interactableLayers;
    [Tooltip("Tag, jaki musi posiadaæ obiekt (zostaw puste, by sprawdzaæ tylko warstwê)")]
    [SerializeField] string interactableTag = "Interactable";
    [Tooltip("Co ma siê wydarzyæ po klikniêciu? Mo¿esz tu podpi¹æ skrypty UI.")]
    public UnityEvent<GameObject> onInteractableClicked;

    private bool isJumping = false;
    private bool isGrounded;
    private bool isHoldingMove = false;

    private Vector3 lastMousePos;
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
            MoveToCursor(true); // Wartoœæ true oznacza pierwsze klikniêcie
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

        if (isJumping) return;

        if (isHoldingMove)
        {
            if (Time.time >= nextMoveTime)
            {
                MoveToCursor(false); // False oznacza przytrzymanie przycisku
                nextMoveTime = Time.time + 0.1f;
            }
        }

        FaceTarget();
        SetAnimations();
    }

    void MoveToCursor(bool isInitialClick)
    {
        if (isJumping) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // NOWE: Sprawdzamy interakcje tylko przy pierwszym klikniêciu (nie przy przytrzymaniu myszy)
        if (isInitialClick && Physics.Raycast(ray, out hit, 100, interactableLayers))
        {
            // Sprawdzamy, czy tag siê zgadza (lub czy pole tagu jest puste)
            if (string.IsNullOrEmpty(interactableTag) || hit.collider.CompareTag(interactableTag))
            {
                // Wywo³ujemy event widoczny w Unity i przekazujemy mu klikniêty obiekt
                onInteractableClicked?.Invoke(hit.collider.gameObject);

                StopMovement(); // Zatrzymujemy postaæ
                isHoldingMove = false; // Przerywamy ewentualne chodzenie
                return; // Koñczymy funkcjê - gracz nie idzie w to miejsce
            }
        }

        // Jeœli to nie by³ obiekt interaktywny, sprawdzamy pod³ogê
        if (Physics.Raycast(ray, out hit, 100, clickableLayers))
        {
            agent.SetDestination(hit.point);

            if (isInitialClick && clickEffect != null)
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
            StartCoroutine(JumpArc());
        }
    }

    void StopMovement()
    {
        if (isJumping || !agent.enabled) return;
        isHoldingMove = false;
        agent.ResetPath();
    }

    private IEnumerator JumpArc()
    {
        isJumping = true;
        animator.Play("Jump_start");

        Vector3 savedDestination = agent.destination;
        Vector3 horizontalVelocity = agent.velocity;

        if (horizontalVelocity.magnitude < 0.1f)
        {
            horizontalVelocity = transform.forward * 0.1f;
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

            if (horizontalVelocity.sqrMagnitude > 0.05f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * lookRotationSpeed);
            }

            Vector3 moveVector = (horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime;
            transform.position += moveVector;

            if (verticalVelocity < 0)
            {
                Vector3 rayStart = transform.position + Vector3.up * 0.5f;
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
        if (agent.velocity.sqrMagnitude < 0.1f) return;

        Vector3 targetPosition = agent.steeringTarget;
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    void SetAnimations()
    {
        bool isWalking = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isWalking);
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