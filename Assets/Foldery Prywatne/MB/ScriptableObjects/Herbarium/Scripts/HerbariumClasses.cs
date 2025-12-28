using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string description;
    public string image;
}

[System.Serializable]
public class ItemDataList
{
    public List<ItemData> items;
}
