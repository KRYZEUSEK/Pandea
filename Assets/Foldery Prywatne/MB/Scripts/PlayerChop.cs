
using UnityEngine;

public class PlayerChop : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private HotbarSelector hotbarSelector;

    [Header("Interakcja")]
    [SerializeField] private KeyCode chopKey = KeyCode.R;
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask treeLayerMask; 

    [Header("Raycast")]
    [SerializeField] private float rayRadius = 0.2f; 

    [Header("Cooldown")]
    [SerializeField] private float chopCooldown = 0.35f;
    private float lastChopTime = -999f;

    private void Reset()
    {
        hotbarSelector = GetComponent<HotbarSelector>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(chopKey))
        {
            TryChop();
        }
    }

    private void TryChop()
    {
        if (Time.time - lastChopTime < chopCooldown) return;
        lastChopTime = Time.time;

        if (hotbarSelector == null)
        {
            Debug.LogWarning("[PlayerChop] Brak referencji do HotbarSelector.");
            return;
        }

        if (!hotbarSelector.IsAxeEquipped())
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f; 
        Vector3 dir = transform.forward;

        if (Physics.SphereCast(origin, rayRadius, dir, out RaycastHit hit, interactDistance, treeLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Tree") || hit.collider.GetComponent<Tree>() != null)
            {
                Tree tree = hit.collider.GetComponent<Tree>();
                if (tree != null)
                {
                    tree.Hit();
                    return;
                }
            }
        }
    }

    
}
