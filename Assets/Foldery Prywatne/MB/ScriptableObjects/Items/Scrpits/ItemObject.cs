using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Food,
    Equipment,
    Default
}
public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType type;
    [TextArea(15,20)]
    public string description;
    public GameObject uiPrefab;
    public GameObject worldPrefab;
    [Header("Hand settings")]
    public Vector3 handScale = Vector3.one; 
    public Vector3 handRotation;
}
