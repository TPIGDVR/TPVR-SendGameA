using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float equipingDistance = 1f;
    public GameObject mesh;
    public Rigidbody rb;
    [SerializeField] private GameObject descriptionUI;
    [SerializeField] EquipDetection currentEquipDetection;

    //to be called by the eqipment detection
    public void Equip()
    {
        mesh.SetActive(false);
        rb.isKinematic = true;
        OnEquip();
    }

    public void Unequip()
    {
        mesh.SetActive(true);
        rb.isKinematic = false;
        currentEquipDetection = null;
        OnUnEquip();
    }

    public void Grab()
    {
        rb.isKinematic = true;
        mesh.SetActive(true);
        RemoveEquipmentDetection();
    }

    private void RemoveEquipmentDetection()
    {
        //unequip the object from the current equip detection
        if (currentEquipDetection != null) 
        {
            print("remove current equip detection from list");
            currentEquipDetection.UnequipCurrentEquipment();
            currentEquipDetection = null;
        }
    }

    public void UnGrab()
    {
        //try to find the equipment detection
        var colliders = Physics.OverlapSphere(transform.position, equipingDistance);
        print($"Colliders found with count {colliders.Length}");
        foreach(var collider in colliders)
        {
            print($"{collider.name} with tag {collider.tag} {collider.gameObject.name}");
            if(collider.tag == "Player Head")
            {
                //after finding it, equip it and then stop it
                if (collider.TryGetComponent<EquipDetection>(out var rt_currentEquipDetection))
                {
                    currentEquipDetection = rt_currentEquipDetection;
                    currentEquipDetection.Equip(this);
                    return;
                }
            }
        }
        print("unequiping");

        //else then
        rb.isKinematic = false;
        mesh.SetActive(true);
    }


    public void OnSet()
    {
        descriptionUI.gameObject.SetActive(true);
    }
    
    public void OnUnSet()
    {
        descriptionUI.gameObject.SetActive(false);
    }
    protected virtual void OnEquip()
    {

    }

    protected virtual void OnUnEquip() 
    { 
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, equipingDistance);
    }

}
