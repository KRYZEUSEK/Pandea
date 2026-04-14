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

    // --- NOWE: Konfiguracja warstwy interakcji ---
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

    // ZMIANA: Zmieniłem nazwę 'spawnEffect' na 'isInitialClick' żeby było bardziej zrozumiale
    void MoveToCursor(bool isInitialClick)
    {
        if (isJumpingInternal) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // --- NOWE: 1. Sprawdzanie Interakcji (Tylko przy pojedynczym kliknięciu myszą) ---
        if (isInitialClick && Physics.Raycast(ray, out hit, 100, interactableLayers))
        {
            // Szukamy skryptu Interactable na trafionym obiekcie
            Interactable interactableObject = hit.collider.GetComponent<Interactable>();

            if (interactableObject != null)
            {
                // Odpalamy funkcję na statku i zatrzymujemy chodzenie!
                interactableObject.TriggerInteraction();
                StopMovement();
                return; // Przerywamy kod, Panda nie idzie w stronę statku
            }
        }

        // 2. Normalne poruszanie się po podłodze
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
        agent.ResetPath();
    }

    private IEnumerator JumpArc()
    {
        isJumpingInternal = true;

        animator.applyRootMotion = false;
        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", true);
        animator.Play(JUMP_START_STATE);

        Vector3 savedDestination = agent.destination;
        Vector3 horizontalVelocity = agent.velocity;

        if (horizontalVelocity.magnitude < 0.2f)
            horizontalVelocity = transform.forward * 2.0f;

        agent.enabled = false;

        float timeToPeak = jumpDuration / 2.0f;
        float gravity = (-2 * jumpHeight) / Mathf.Pow(timeToPeak, 2);
        float verticalVelocity = (2 * jumpHeight) / timeToPeak;

        yield return null;

        while (true)
        {
            float deltaTime = Time.deltaTime;
            verticalVelocity += gravity * deltaTime;

            Vector3 moveVector = (horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime;
            transform.position += moveVector;

            if (verticalVelocity < 0)
            {
                Vector3 feetPosition = transform.position - (Vector3.up * heightFromPivotToFeet);
                if (Physics.Raycast(feetPosition + Vector3.up * 0.2f, Vector3.down, out RaycastHit hitGround, 0.4f, clickableLayers))
                {
                    transform.position = hitGround.point + (Vector3.up * heightFromPivotToFeet);
                    break;
                }
            }

            yield return null;
        }

        agent.enabled = true;
        animator.SetBool("isJumping", false);

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 3.0f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }

        agent.ResetPath();

        if (savedDestination != Vector3.zero)
            agent.SetDestination(savedDestination);

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
}