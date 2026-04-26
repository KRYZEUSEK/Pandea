using UnityEngine;

public class PlayerChop : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private HotbarSelector hotbarSelector;

    [Header("Interakcja")]
    [SerializeField] private KeyCode chopKey = KeyCode.R;
    [SerializeField] private LayerMask treeLayerMask;

    [Header("Hitbox œcinania")]
    [Tooltip("Jak daleko przed graczem powstaje strefa uderzenia")]
    [SerializeField] private float hitOffset = 0.5f;
    [Tooltip("Wielkoœæ strefy uderzenia (promieñ kuli)")]
    [SerializeField] private float hitRadius = 1.0f;
    [Tooltip("Wysokoœæ, na której znajduje siê œrodek kuli (Daj blisko 0 dla ma³ych obiektów!)")]
    [SerializeField] private float hitHeight = 0.2f;

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
        // 1. Sprawdzenie cooldownu
        if (Time.time - lastChopTime < chopCooldown) return;
        lastChopTime = Time.time;

        // 2. Sprawdzenie narzêdzia
        if (hotbarSelector == null || !hotbarSelector.IsAxeEquipped()) return;

        // 3. Wyliczenie œrodka kuli z uwzglêdnieniem nowej wysokoœci (hitHeight)
        Vector3 hitCenter = transform.position + transform.forward * hitOffset + Vector3.up * hitHeight;

        // 4. Zebranie obiektów
        Collider[] hitColliders = Physics.OverlapSphere(hitCenter, hitRadius, treeLayerMask);

        // 5. Sprawdzenie trafieñ
        foreach (Collider col in hitColliders)
        {
            Tree tree = col.GetComponentInParent<Tree>();

            if (tree != null)
            {
                tree.Hit();
                break; // Koñczymy po pierwszym trafieniu
            }
        }
    }

    // Rysowanie strefy uderzenia w edytorze
    private void OnDrawGizmosSelected()
    {
        Vector3 hitCenter = transform.position + transform.forward * hitOffset + Vector3.up * hitHeight;
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(hitCenter, hitRadius);
    }
}