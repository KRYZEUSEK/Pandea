using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TimeAlterPlant : BasePlant
{
    [Header("Wartoœæ zmiany Czasu")]
    public float alterTimeValue = 0.5f; //czas który dodamy/odejmiemy po wejsciu w roslinê

    protected override void OnPlayerEnter(GameObject player)
    {
        TimeManager.Instance.ModifyTime(alterTimeValue);
        this.GameObject().SetActive(false); //dezaktywujemy roœlinê po u¿yciu
    }

}
