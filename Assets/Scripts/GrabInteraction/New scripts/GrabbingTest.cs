using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class GrabbingTest : MonoBehaviour
{
    private GrabInteractor interactor = null;

    private void Awake()
    {
        interactor = GetComponent<GrabInteractor>();
    }

    private void OnEnable()
    {
        interactor.WhenInteractableSet.Action += OnSet;
        interactor.WhenInteractableUnset.Action += OnUnSet;
        interactor.WhenInteractableUnselected.Action += OnUnSelected;
        interactor.WhenInteractableSelected.Action += OnSelected;
    }

    void OnDisable()
    {
        interactor.WhenInteractableSet.Action -= OnSet;
        interactor.WhenInteractableUnset.Action -= OnUnSet;
        interactor.WhenInteractableUnselected.Action -= OnUnSelected;
        interactor.WhenInteractableSelected.Action -= OnSelected;
    }

    void OnSet(GrabInteractable grabInteractor)
    {
        print("on set");
        if (grabInteractor.TryGetComponent<Interactable>(out var interactable))
        {
            print($"Setting {interactable.gameObject.name}");

            interactable.OnSet();
        }
    }
    
    void OnUnSet(GrabInteractable grabInteractor)
    {
        if (grabInteractor.TryGetComponent<Interactable>(out var interactable))
        {
            print($"Setting {interactable.gameObject.name}");

            interactable.OnUnSet();
        }
    }
    
    void OnSelected(GrabInteractable grabInteractor)
    {
        print("on selected");
        if (grabInteractor.TryGetComponent<Interactable>(out var interactable))
        {
            print($"Setting {interactable.gameObject.name}");

            interactable.Grab();
        }
    }
    
    void OnUnSelected(GrabInteractable grabInteractor)
    {
        print("on unselected");
        if (grabInteractor.TryGetComponent<Interactable>(out var interactable))
        {
            print($"Setting {interactable.gameObject.name}");
            interactable.UnGrab();
        }
    }
}
