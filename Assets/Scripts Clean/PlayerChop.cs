using UnityEngine;

public class PlayerChop : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private HotbarSelector hotbarSelector;

    [Header("Interakcja")]
    [SerializeField] private KeyCode chopKey = KeyCode.R;
    [SerializeField] private LayerMask treeLayerMask;

    [Header("Hitbox œcinania (Prostopad³oœcian)")]
    [Tooltip("Jak daleko przed graczem powstaje strefa uderzenia")]
    [SerializeField] private float hitOffset = 1.0f;
    [Tooltip("Wymiary strefy uderzenia (X: szerokoœæ, Y: wysokoœæ, Z: g³êbokoœæ)")]
    [SerializeField] private Vector3 hitBoxSize = new Vector3(2f, 2f, 2f);
    [Tooltip("Przesuniêcie w osi Y (daj na minus, np. -0.5, jeœli obiekty s¹ wciœniête pod ziemiê)")]
    [SerializeField] private float hitHeightOffset = 0f;

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

        // 3. Wyliczenie œrodka pude³ka
        Vector3 hitCenter = transform.position + transform.forward * hitOffset + Vector3.up * hitHeightOffset;

        // 4. Zebranie obiektów w prostok¹tnym polu (z uwzglêdnieniem rotacji gracza!)
        Collider[] hitColliders = Physics.OverlapBox(hitCenter, hitBoxSize / 2f, transform.rotation, treeLayerMask);

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
        Vector3 hitCenter = transform.position + transform.forward * hitOffset + Vector3.up * hitHeightOffset;
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        // Transformacja macierzy, aby Gizmo obraca³o siê razem z graczem
        Gizmos.matrix = Matrix4x4.TRS(hitCenter, transform.rotation, hitBoxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}