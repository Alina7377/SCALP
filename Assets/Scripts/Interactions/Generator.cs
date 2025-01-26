using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _batareyPreview;

    private void Start()
    {
        Events.Instance.OnReloadLevel += Reload;
    }

    private void OnDisable()
    {
        Events.Instance.OnReloadLevel -= Reload;
       
    }
    private void Reload()
    {
        _batareyPreview.SetActive(true);
    }

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
