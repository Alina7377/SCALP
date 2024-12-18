using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _batareyPreview;

    public void Interact()
    {
         Events.Instance.InteractGenerator();
        _batareyPreview.SetActive(false);
    }

    public bool Interact(ref GameObject interactingOject)
    {
        throw new System.NotImplementedException();
    }
}
