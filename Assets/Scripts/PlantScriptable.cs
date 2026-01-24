using UnityEngine;

[CreateAssetMenu(fileName = "PlantScriptable", menuName = "ScriptableObjects/Plant")]
public class PlantScriptable : ScriptableObject
{
    [Header("Informacje")]
    public string plantName;
    [TextArea(1, 10)]
    public string plantDescription;
    public Sprite plantPicture;

    [Header("Ustawienia Spawnowania")]
    public GameObject prefab;

    [Range(0, 1)]
    [Tooltip("Szansa na pojawienie siê (0-100%)")]
    public float density;

    [Header("Wysokoœæ terenu (Noise 0-1)")]
    [Range(0, 1)] public float minHeight;
    [Range(0, 1)] public float maxHeight;

    [Header("Skalowanie")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
}