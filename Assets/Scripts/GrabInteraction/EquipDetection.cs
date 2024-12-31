using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipDetection : MonoBehaviour
{
    [SerializeField]
    Transform p;
    [SerializeField]
    Transform defaultParent;
    public TMP_Text text;
    [SerializeField] Interactable currentEquipment;

    public void Equip(Interactable equipment)
    {
        if (currentEquipment)
        {
            UnequipCurrentEquipment();
        }

        equipment.Equip();
        currentEquipment = equipment;

        StartCoroutine(StartEquiping(equipment.transform.parent));
    }

    public void UnequipCurrentEquipment()
    {
        currentEquipment.transform.parent.parent = defaultParent;
        currentEquipment.Unequip();
        currentEquipment = null;
    }

    IEnumerator StartEquiping(Transform currentEQTransform)
    {
        currentEQTransform.parent = p;
        yield return null;
        Quaternion rotate = Quaternion.identity;
        currentEQTransform.localPosition = Vector3.zero;
        currentEQTransform.localRotation = rotate;
    }
}
