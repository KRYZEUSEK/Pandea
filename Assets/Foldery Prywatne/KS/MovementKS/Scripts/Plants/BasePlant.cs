using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]

public abstract class BasePlant : MonoBehaviour
{
    [Header("Collider Roœliny")]
    public Collider plantCollider;

    public void Awake()
    {
        plantCollider.isTrigger = true; // upewnij siê, ¿e collider jest ustawiony jako trigger, aby poprawnie wykrywaæ eventy 
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
            return;
        OnPlayerEnter(other.GameObject());
    }
    protected virtual void OnPlayerEnter(GameObject player)
    {
        //Domyœlnie nic nie robi - funkcjonalnoœæ w innym skrypcie 
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExit(other.gameObject);
        }
    }
    protected virtual void OnPlayerExit(GameObject player)
    {
        // Do nadpisania w dzieciach
    }
}


