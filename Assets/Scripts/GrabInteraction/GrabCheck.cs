using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class GrabCheck : MonoBehaviour
{
    [SerializeField]
    bool IsRight;
    [SerializeField] float searchRadius = 0.05f;

    [SerializeField] LayerMask interactableMask;
    private void Start()
    {
        if (IsRight)
        {
            EventSystem.player.AddListener(PlayerEvents.GRAB_DOWN_RIGHT, Retrieve);
        }
        else
        {
            EventSystem.player.AddListener(PlayerEvents.GRAB_DOWN_LEFT, Retrieve);
        }
    }

    private GameObject Retrieve()
    {
        //get all the collider around the sphere and try and get the interactable game object
        var collider = Physics.OverlapSphere(transform.position, searchRadius, interactableMask);
        print($"this is running {collider?.Length}");
        if (collider.Length == 0) return null;
        //get the first one.
        else return collider[0].gameObject;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}



