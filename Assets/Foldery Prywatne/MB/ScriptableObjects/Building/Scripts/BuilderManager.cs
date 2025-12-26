
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI; 

public class BuildingManager : MonoBehaviour
{


    [Header("UI")]
    [SerializeField]
    private GameObject buildMenuPanel;

       [Header("Katalog budowli")]
    public BuildCatalog catalog;
    public int SelectedIndex { get; private set; } = -1;

    [Header("Wejścia")]
    public HotbarSelector hotbar;
    public Camera playerCamera;
    public Transform player; // dla fallbacków

    [Header("Aktualny wybór")]
    public BuildableData selectedBuildable;

    [Header("Ekwipunek / materiały")]
    public InventoryObject inventory;


    [Header("Podgląd / sterowanie")]
    public float maxPlacementDistance = 30f;
    public float rotationStepDegrees = 15f;
    public float gridCellSize = 0f;

    [Header("Warstwy celowania (pod kursorem)")]
    [Tooltip("Warstwy, które traktujemy jako 'powierzchnie do budowy' przy wyborze XZ pod kursorem.")]
    public LayerMask placementMask; 
    [Tooltip("Warstwy, które ray spod kursora MA ignorować (gracz, click-blocker, preview itd.).")]
    public LayerMask ignoreRayLayers; 

    [Header("Walidacja miejsca")]
    public float maxSlopeDegrees = 30f;
    [Range(0f, 1f)] public float requiredValidFraction = 0.6f;
    public float liftAboveGround = 0.02f;
    public float overlapMargin = 0.02f;
    public bool alignToGroundNormal = false;

    [Header("Osadzanie (niezależnie od kamery)")]
    [Tooltip("Z jakiej wysokości strzelać pionowym rayem w dół do osadzania na ziemi.")]
    public float verticalRayHeight = 50f;

    [Header("Fallbacki, gdy ray spod kursora nic nie trafia")]
    [Tooltip("Użyj pozycji przed graczem jako XZ fallback.")]
    public bool usePlayerForwardFallback = true;
    public float aimDistanceFromPlayer = 4f;

    [Tooltip("Użyj przecięcia z poziomą płaszczyzną (na wysokości gracza) jako fallback.")]
    public bool useHorizontalPlaneFallback = true;

    [Header("NavMesh (opcjonalnie)")]
    public bool useNavMeshSnap = false;
    public float navMeshSnapMaxDistance = 2f;

    [Header("Preview feedback")]
    public Color validColor = new Color(0f, 1f, 0f, 0.35f);   // zielony, półprzezroczysty
    public Color invalidColor = new Color(1f, 0f, 0f, 0.35f);

    CustomActions input;
    // Stan
    private GameObject previewInstance;
    private bool buildMode = false;
    private float currentRotation = 0f;
    private bool warnedGroundMask = false;
    private bool lastPreviewValid = false;
    private Bounds previewBounds;
    private bool previewBoundsCached = false;

    void Awake()
    {
        input = new CustomActions();
    }

    void OnEnable()
    {
        if (hotbar != null)
            hotbar.OnSelectedIndexChanged += OnHotbarSelectedIndexChanged;

        if (inventory != null)
            inventory.OnInventoryChanged += OnInventoryChanged;
        input.Enable();
    }

    void OnDisable()
    {
        if (hotbar != null)
            hotbar.OnSelectedIndexChanged -= OnHotbarSelectedIndexChanged;

        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;
        input.Disable();
    }

    private void HandleGlobalInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (buildMode) ExitBuildMode();
            else TryEnterBuildMode(selectedBuildable);
        }

        HandleCatalogCycling();
    }
    private void OnPlacePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI()) return;
        TryPlaceFinalFromPreview();
    }
    private void HandleCatalogCycling()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) CycleBuildable(-1);
        if (Input.GetKeyDown(KeyCode.RightBracket)) CycleBuildable(+1);

        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            float s = Input.GetAxis("Mouse ScrollWheel");
            if (s > 0f) CycleBuildable(-1);
            else if (s < 0f) CycleBuildable(+1);
        }
    }
    private void HandleBuildModeInput()
    {
        if (!HasRequiredTool())
        {
            ExitBuildMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q)) currentRotation -= rotationStepDegrees;
        if (Input.GetKeyDown(KeyCode.E)) currentRotation += rotationStepDegrees;

    }
    private bool HasRequiredTool()
    {
        return hotbar != null && hotbar.IsWrenchEquipped();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
    private void UpdatePreview()
    {
        EnsureCamera();
        EnsurePreviewInstance();

        if (!TryUpdatePreviewPosition())
            return;

        ValidateAndColorPreview();
    }
    private bool TryUpdatePreviewPosition()
    {
        if (playerCamera == null || previewInstance == null || selectedBuildable == null)
            return false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        // 1) XZ spod kursora albo fallback
        if (!TryGetTargetXZFiltered(ray, placementMask, ignoreRayLayers, out Vector3 xz)
            && !TryFallbackXZ(ray, out xz))
            return false;

        // 2) Snap do siatki (tylko XZ)
        if (gridCellSize > 0f)
        {
            xz.x = Mathf.Round(xz.x / gridCellSize) * gridCellSize;
            xz.z = Mathf.Round(xz.z / gridCellSize) * gridCellSize;
        }

        // 3) Osadzenie w dół + obrót
        return TrySettlePreview(xz);
    }

    private void EnsureCamera()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    private void EnsurePreviewInstance()
    {
        if (previewInstance != null) return;
        if (selectedBuildable?.previewPrefab == null) return;

        previewInstance = Instantiate(selectedBuildable.previewPrefab);
    }
    private bool TrySettlePreview(Vector3 xz)
    {
        if (previewInstance == null || selectedBuildable == null)
            return false;

        LayerMask groundMask = SafeGroundMask(selectedBuildable.groundMask);

        if (!TryVerticalSettle(xz, groundMask, out RaycastHit hit))
            return false;

        // Oblicz wysokość preview
        Bounds b = GetObjectBounds(previewInstance);
        float halfHeight = b.extents.y;

        Vector3 pos = hit.point;
        pos.y += halfHeight + liftAboveGround;

        // Obrót
        Quaternion yaw = Quaternion.Euler(0f, currentRotation, 0f);

        if (alignToGroundNormal)
        {
            Quaternion tilt = Quaternion.FromToRotation(Vector3.up, hit.normal);
            previewInstance.transform.rotation = LimitTilt(tilt) * yaw;
        }
        else
        {
            previewInstance.transform.rotation = yaw;
        }

        previewInstance.transform.position = pos;
        return true;
    }

    private void SnapToGrid(ref Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x / gridCellSize) * gridCellSize;
        pos.z = Mathf.Round(pos.z / gridCellSize) * gridCellSize;
    }
    private void ValidateAndColorPreview()
    {
        bool valid = ValidatePlacement(
            previewInstance.transform.position,
            previewInstance.transform.rotation,
            out _, out _
        );

        if (valid != lastPreviewValid)
            SetPreviewValid(valid);
    }
    private void OnHotbarSelectedIndexChanged(int newIndex)
    {
        // Jeżeli zmienił się slot i nie mamy w ręce wrench → wyłącz tryb
        if (buildMode && (hotbar == null || !hotbar.IsWrenchEquipped()))
        {
            Debug.Log("Wyłączam tryb budowy — w ręce nie ma 'wrench'.");
            ExitBuildMode();
        }
    }

    private void OnInventoryChanged()
    {
        // Jeśli wyrzucono lub zużyto wrench i nie ma go w aktywnym slocie → wyłącz tryb
        if (buildMode && (hotbar == null || !hotbar.IsWrenchEquipped()))
        {
            Debug.Log("Wyłączam tryb budowy — utracono 'wrench' w ekwipunku.");
            ExitBuildMode();
        }
    }


    void Update()
    {
        HandleGlobalInput();

        if (!buildMode)
            return;

        HandleBuildModeInput();
        UpdatePreview();
    }





    public void SelectBuildable(BuildableData data)
    {
        selectedBuildable = data;
        Debug.Log(data != null ? $"Wybrano budowlę: {data.id}" : "Wybrano: (brak)");

        // Jeśli jesteśmy w trybie budowy, odśwież ghosta
        if (buildMode)
            RefreshPreviewInstance();
    }


    private void RefreshPreviewInstance()
    {
        // Usuń starego ducha
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }

        // Utwórz nowego ducha z aktualnego BuildableData
        if (selectedBuildable != null && selectedBuildable.previewPrefab != null)
        {
            previewInstance = Instantiate(selectedBuildable.previewPrefab);

            // (opcjonalnie) ustaw warstwę na „Preview”, żeby ignorować przy raycastach
            int previewLayer = LayerMask.NameToLayer("Preview");
            if (previewLayer >= 0) SetLayerRecursively(previewInstance, previewLayer);

            // Wyzeruj obrót Y tylko jeśli chcesz (możesz zachować currentRotation)
            // currentRotation = 0f; // jeśli wolisz reset

            // Zresetuj kolor walidacji na neutralny/valid
            SetPreviewValid(true);
        }
    }

    private void SetPreviewValid(bool valid)
    {
        lastPreviewValid = valid;
        if (previewInstance == null) return;

        var renderers = previewInstance.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // Uwaga: Renderer.materials tworzy kopie materiałów runtime (OK dla podglądu)
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].HasProperty("_Color"))
                {
                    mats[i].color = valid ? validColor : invalidColor;
                }
            }
        }
    }

    /// <summary>
    /// Aktualny stan walidacji podglądu (true = zielony/OK, false = czerwony/NIE).
    /// </summary>
    public bool IsPreviewValid() => lastPreviewValid;


    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }



    public void SelectBuildableIndex(int index)
    {
        if (catalog == null || catalog.entries == null || catalog.entries.Count == 0) return;
        if (index < 0 || index >= catalog.entries.Count) return;

        SelectedIndex = index;
        SelectBuildable(catalog.entries[SelectedIndex]);
    }

    public void CycleBuildable(int direction)
    {
        if (catalog == null || catalog.entries == null || catalog.entries.Count == 0) return;
        if (SelectedIndex < 0) SelectedIndex = 0;
        SelectedIndex = (SelectedIndex + direction + catalog.entries.Count) % catalog.entries.Count;
        SelectBuildable(catalog.entries[SelectedIndex]);
    }


    // ===== TRYB BUDOWY =====
    public void TryEnterBuildMode(BuildableData data)
    {
        if (data == null) { Debug.LogWarning("Brak BuildableData."); return; }
        if (hotbar == null) { Debug.LogWarning("Brak HotbarSelector."); return; }
        if (!hotbar.IsWrenchEquipped())
        {
            Debug.Log("Potrzebujesz 'wrench' w ręce, aby budować.");
            return;
        }
        EnterBuildMode(data);
    }

    private void EnterBuildMode(BuildableData data)
    {
        input.Main.Place.performed += OnPlacePerformed;
        input.Enable();
        selectedBuildable = data;
        buildMode = true;
        currentRotation = 0f;

        if (buildMenuPanel != null)
            buildMenuPanel.SetActive(true);

        if (previewInstance != null) Destroy(previewInstance);
        if (selectedBuildable.previewPrefab != null)
            previewInstance = Instantiate(selectedBuildable.previewPrefab);

        if (selectedBuildable.groundMask.value == 0 && !warnedGroundMask)
        {
            Debug.LogWarning("groundMask = 0. Ustaw warstwę terenu (np. 'Terrain'/'Default') w BuildableData.");
            warnedGroundMask = true;
        }

        Debug.Log($"Tryb budowy: {data.id}");
    }

    private void ExitBuildMode()
    {
        input.Main.Place.performed -= OnPlacePerformed;
        input.Disable();
        buildMode = false;

        if (buildMenuPanel != null)
            buildMenuPanel.SetActive(false);

        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        Debug.Log("Wyłączono tryb budowy.");
    }

    // ===== STAWIANIE =====
    private void TryPlaceFinalFromPreview()
    {
        if (previewInstance == null || selectedBuildable == null || selectedBuildable.finalPrefab == null)
        {
            Debug.LogWarning("Brak preview / finalPrefab.");
            return;
        }

        if (!inventory.HasItems(selectedBuildable.costs))
        {
            Debug.Log("Brakuje wymaganych przedmiotów do budowy!");
            // Tutaj możesz wyświetlić UI z listą brakujących zasobów
            return;
        }

        Vector3 proposedPos = previewInstance.transform.position;
        Quaternion proposedRot = previewInstance.transform.rotation;

        if (!ValidatePlacement(proposedPos, proposedRot, out Vector3 validPos, out Quaternion validRot))
        {
            Debug.Log("Miejsce nie spełnia zasad (slope/kolizje/maska).");
            return;
        }

        // Ostateczne settle w dół + NavMesh snap (opcjonalnie)
        LayerMask groundMask = SafeGroundMask(selectedBuildable.groundMask);
        if (TryVerticalSettle(validPos, groundMask, out RaycastHit settleHit))
        {
            Bounds pb = GetObjectBounds(previewInstance);
            float halfHeight = pb.extents.y;

            Vector3 pos = settleHit.point;
            pos.y += halfHeight + liftAboveGround;

            if (useNavMeshSnap && NavMesh.SamplePosition(pos, out NavMeshHit navHit, navMeshSnapMaxDistance, NavMesh.AllAreas))
            {
                pos = navHit.position;
                pos.y += halfHeight + liftAboveGround;
            }

            Instantiate(selectedBuildable.finalPrefab, pos, validRot);
            Debug.Log($"Postawiono: {selectedBuildable.id} @ {pos}");
        }
        else
        {
            // fallback: postaw jak ghost (ostateczność)
            Instantiate(selectedBuildable.finalPrefab, validPos, validRot);
            Debug.Log($"Postawiono (fallback): {selectedBuildable.id} @ {validPos}");
        }

        bool consumed = inventory.ConsumeItems(selectedBuildable.costs);
        if (!consumed)
        {
            // Nie powinno się zdarzyć, bo wcześniej HasItems zwróciło true,
            // ale warto zabezpieczyć:
            Debug.LogError("Zużycie przedmiotów nie powiodło się.");
            return;
        }

    }

    // ===== CELOWANIE POD KURSOREM (FILTRY) =====
    /// <summary>
    /// Używa RaycastAll, sortuje po dystansie i wybiera najbliższe trafienie,
    /// którego warstwa jest w placementMask i NIE jest w ignoreRayLayers.
    /// </summary>
    private bool TryGetTargetXZFiltered(Ray ray, LayerMask placement, LayerMask ignore, out Vector3 xz)
    {
        xz = default;

        // Zbierz wszystkie trafienia – nie filtruj tu maską, bo chcemy wiedzieć co zasłania
        var hits = Physics.RaycastAll(ray, maxPlacementDistance, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            int layer = h.collider.gameObject.layer;

            // 1) Pomijaj warstwy ignorowane
            if ((ignore.value & (1 << layer)) != 0) continue;

            // 2) Przyjmuj tylko warstwy, które są "powierzchnią do budowy"
            if ((placement.value & (1 << layer)) == 0) continue;

            xz = h.point;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Fallbacky: najpierw przecięcie z poziomą płaszczyzną (na wysokości gracza),
    /// a jeśli brak – punkt przed graczem.
    /// </summary>
    private bool TryFallbackXZ(Ray ray, out Vector3 xz)
    {
        // Pozioma płaszczyzna na wysokości gracza
        if (useHorizontalPlaneFallback && player != null)
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0f, player.position.y, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                xz = ray.origin + ray.direction * enter;
                return true;
            }
        }

        // Punkt przed graczem
        if (usePlayerForwardFallback && player != null)
        {
            xz = player.position + player.forward * aimDistanceFromPlayer;
            return true;
        }

        xz = default;
        return false;
    }

    // ===== OSADZANIE W DÓŁ =====
    private bool TryVerticalSettle(Vector3 xz, LayerMask groundMask, out RaycastHit hit)
    {
        Vector3 start = new Vector3(xz.x, xz.y + verticalRayHeight, xz.z);
        return Physics.Raycast(start, Vector3.down, out hit, verticalRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore);
    }

    // ===== WALIDACJA (łagodna) =====
    private bool ValidatePlacement(Vector3 previewPos, Quaternion previewRot, out Vector3 finalPos, out Quaternion finalRot)
    {
        finalPos = previewPos;
        finalRot = previewRot;

        Bounds b = GetObjectBounds(previewInstance);
        Vector3 center = b.center;
        Vector3 ex = b.extents;

        // 9‑punktowa siatka próbkowania
        List<Vector3> starts = new List<Vector3>();
        float startAbove = 1.5f;
        for (int ix = -1; ix <= 1; ix++)
        {
            for (int iz = -1; iz <= 1; iz++)
            {
                starts.Add(new Vector3(center.x + ex.x * ix, previewPos.y + startAbove, center.z + ex.z * iz));
            }
        }

        int validHits = 0;
        Vector3 avgNormal = Vector3.zero;
        float maxRayDown = 6f;
        LayerMask mask = SafeGroundMask(selectedBuildable.groundMask);

        foreach (var s in starts)
        {
            if (Physics.Raycast(s, Vector3.down, out RaycastHit hit, maxRayDown, mask, QueryTriggerInteraction.Ignore))
            {
                float slope = Vector3.Angle(hit.normal, Vector3.up);
                if (slope <= maxSlopeDegrees)
                {
                    validHits++;
                    avgNormal += hit.normal;
                }
            }
        }

        float fraction = (float)validHits / starts.Count;
        if (fraction < requiredValidFraction)
        {
            // fallback: środek + tolerancja
            Vector3 cStart = new Vector3(center.x, previewPos.y + startAbove, center.z);
            if (Physics.Raycast(cStart, Vector3.down, out RaycastHit cHit, maxRayDown, mask))
            {
                float cSlope = Vector3.Angle(cHit.normal, Vector3.up);
                if (cSlope <= maxSlopeDegrees + 5f) return true;
            }
            return false;
        }

        // (opcjonalnie) tilt do średniej normalnej
        if (alignToGroundNormal && avgNormal != Vector3.zero)
        {
            Vector3 n = (avgNormal / Mathf.Max(1, validHits)).normalized;
            Quaternion tilt = Quaternion.FromToRotation(Vector3.up, n);
            finalRot = LimitTilt(tilt) * Quaternion.Euler(0f, finalRot.eulerAngles.y, 0f);
        }

        // OverlapBox (łagodniej, ignoruj triggery/dekoracje)
        Vector3 half = new Vector3(
            Mathf.Max(0f, ex.x - overlapMargin),
            Mathf.Max(0f, ex.y - overlapMargin),
            Mathf.Max(0f, ex.z - overlapMargin)
        );

        Vector3 centerOffset = b.center - previewInstance.transform.position;
        Vector3 overlapCenter = finalPos + centerOffset;

        Collider[] cols = Physics.OverlapBox(
            overlapCenter, half, finalRot, ~mask, QueryTriggerInteraction.Collide
        );

        foreach (var c in cols)
        {
            if (c.transform.IsChildOf(previewInstance.transform)) continue;
            if (((1 << c.gameObject.layer) & mask.value) != 0) continue;
            if (c.isTrigger) continue;
            if (c.CompareTag("Decor") || c.gameObject.layer == LayerMask.NameToLayer("Foliage")) continue;

            return false;
        }

        return true;
    }

    // ===== POMOCNICZE =====
    private Bounds GetObjectBounds(GameObject go)
    {
        if (go == null) return new Bounds(Vector3.zero, Vector3.one * 0.01f);

        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        var cols = go.GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            return b;
        }

        return new Bounds(go.transform.position, Vector3.one * 0.01f);
    }

    private LayerMask SafeGroundMask(LayerMask source)
    {
        if (source.value != 0) return source;

        if (!warnedGroundMask)
        {
            Debug.LogWarning("groundMask = 0. Używam ~0 (wszystkie warstwy). Ustaw w BuildableData docelową warstwę terenu.");
            warnedGroundMask = true;
        }
        return ~0;
    }

    private Quaternion LimitTilt(Quaternion tilt)
    {
        // ogranicz tilt do +/-10° w X/Z (żeby nie przechylało przesadnie)
        Vector3 e = tilt.eulerAngles;
        e.x = NormalizeAngle(e.x);
        e.z = NormalizeAngle(e.z);
        e.x = Mathf.Clamp(e.x, -10f, 10f);
        e.z = Mathf.Clamp(e.z, -10f, 10f);
        return Quaternion.Euler(e);
    }

    private float NormalizeAngle(float a) => (a > 180f) ? a - 360f : a;


}
