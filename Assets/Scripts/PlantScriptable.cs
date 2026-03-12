using UnityEngine;

[CreateAssetMenu(fileName = "PlantScriptable", menuName = "ScriptableObjects/Plant")]
public class PlantScriptable : ScriptableObject
{
    [Header("Informacje")]
    public string plantName;
    [TextArea(1, 10)]
    public string plantDescription;
    public Sprite plantPicture;

    [Header("Warianty Wygl¹du")]
    [Tooltip("Dodaj tutaj wszystkie wersje tego obiektu (np. Sosna_A, Sosna_B, Sosna_C). System wylosuje jedn¹ z nich.")]
    public GameObject[] prefabs; // ZMIANA: Tablica zamiast pojedynczego obiektu

    [Tooltip("Czy to obiekt specjalny (np. do zbierania)?")]
    public bool isInteractable;

    [Range(0, 1)]
    [Tooltip("Szansa na pojawienie siê wewn¹trz skupiska (0-100%)")]
    public float density;

    [Header("Wysokoœæ i Nachylenie")]
    [Range(0, 1)] public float minHeight;
    [Range(0, 1)] public float maxHeight;

    [Tooltip("Maksymalny k¹t nachylenia terenu.")]
    [Range(0, 90)] public float maxSlope = 45f;

    [Header("Grupowanie (Noise)")]
    public float noiseScale = 10f;
    [Range(0, 1)] public float noiseThreshold = 0.4f;
    public Vector2 noiseOffset;

    [Header("Skalowanie")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
}