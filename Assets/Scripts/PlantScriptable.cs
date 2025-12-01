using UnityEngine;

[CreateAssetMenu(fileName = "PlantScriptable", menuName = "ScriptableObjects/Plant")]
public class PlantScriptable : ScriptableObject {
    public string plantName;
    [TextArea(1, int.MaxValue)]
    public string plantDescription;
    public Sprite plantPicture;
}