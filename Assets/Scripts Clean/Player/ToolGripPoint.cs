using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolGripPoint : MonoBehaviour
{
    [Header("Setup Helper (Only for Editor)")]
    [Tooltip("Przeci¹gnij tu 'HandSocket' gracza ze SCENY, aby przetestowaæ chwyt.")]
    public Transform handSocketForTesting;

    [Header("Saved Grip Transform")]
    [Tooltip("Zapisana pozycja, jak¹ narzêdzie powinno przyj¹æ w rêce.")]
    public Vector3 gripLocalPosition;
    [Tooltip("Zapisana rotacja, jak¹ narzêdzie powinno przyj¹c w rêce.")]
    public Quaternion gripLocalRotation = Quaternion.identity;

    // Prywatna referencja do naszego testowego duplikatu
    private GameObject duplicateTool;

    /// <summary>
    /// KROK 1: Tworzy duplikat narzêdzia i podpina go do rêki testowej.
    /// </summary>
    [ContextMenu("1. Create Test Duplicate in Hand")]
    private void CreateDuplicate()
    {
        if (handSocketForTesting == null)
        {
            Debug.LogError("Musisz przeci¹gn¹æ 'HandSocket' do pola 'Hand Socket For Testing'!", this);
            return;
        }

        if (duplicateTool != null)
        {
            Debug.LogWarning("Duplikat ju¿ istnieje. Usuwam stary.", this);
            DestroyImmediate(duplicateTool);
        }

        // Stwórz duplikat samego siebie jako dziecko 'HandSocket'
        duplicateTool = Instantiate(this.gameObject, handSocketForTesting);
        duplicateTool.name = this.gameObject.name + "_GripTestDuplicate";

        // Zablokuj skrypty na duplikacie, aby unikn¹æ b³êdów lub dziwnych zachowañ
        foreach (var script in duplicateTool.GetComponents<MonoBehaviour>())
        {
            script.enabled = false;
        }

        // Usuñ rigidbody i collidery z duplikatu, aby nie wariowa³
        if (duplicateTool.TryGetComponent(out Rigidbody rb)) DestroyImmediate(rb);
        foreach (var col in duplicateTool.GetComponents<Collider>()) DestroyImmediate(col);


        Debug.Log("Stworzono duplikat w rêce. Ustaw go teraz rêcznie w oknie Sceny (przesuñ/obróæ). Gdy skoñczysz, kliknij '2. Save Grip Transform' na *oryginalnym* obiekcie.", this);
    }

    /// <summary>
    /// KROK 2: Zapisuje transform duplikatu i usuwa go.
    /// </summary>
    [ContextMenu("2. Save Grip Transform and Delete Duplicate")]
    private void SaveTransform()
    {
        if (duplicateTool == null)
        {
            Debug.LogError("Nie znaleziono duplikatu. U¿yj '1. Create Test Duplicate' najpierw.", this);
            return;
        }

        // Zapisz transform duplikatu
        gripLocalPosition = duplicateTool.transform.localPosition;
        gripLocalRotation = duplicateTool.transform.localRotation;

        // Usuñ duplikat
        DestroyImmediate(duplicateTool);
        duplicateTool = null;

        Debug.Log("Zapisano transform! Wartoœci: Pos=" + gripLocalPosition + ", Rot=" + gripLocalRotation.eulerAngles, this);
    }
}