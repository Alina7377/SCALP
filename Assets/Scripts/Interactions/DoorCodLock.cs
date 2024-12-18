using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition.Attributes;

public class DoorCodLock : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorControl _doorControl;
    [SerializeField] private AudioClip _positivOpen;
    [SerializeField] private AudioClip _negativeOpen;

    private AudioSource _audioSource;
    private Renderer _renderer;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _renderer = GetComponent<Renderer>();
        Events.Instance.OnInteractGenerator += ChangeMaterial;
    }

    private void OnDisable()
    {
        Events.Instance.OnInteractGenerator -= ChangeMaterial;
    }

    private void ChangeMaterial()
    {
        Material[] materials;
        if (_doorControl.GetGrid()._isTraining)
        {
            materials = _renderer.materials;
            materials[1] = _doorControl.GetGrid().GetMaterial(_doorControl.GetCardColor());
            _renderer.materials = materials;
        }
    }

    private void ImpossibleOpen() 
    {
        _audioSource.clip = _negativeOpen;
        _audioSource.Play();
        if (GameMode.PlayerUI._isTraining && _doorControl.GetCardColor()==AccessCardColor.Green)
            GameMode.PlayerUI.ShowFleshTextOnlyTraing("Training.3");
        else if (GameMode.PlayerUI._isTraining && _doorControl.GetCardColor() == AccessCardColor.Blue)
            GameMode.PlayerUI.ShowFleshTextOnlyTraing("Training.13");
        GameMode.PlayerUI.ShowText("UI." + _doorControl.GetCardColor().ToString(),true);
    }

    public void Interact()
    {
        if(!_doorControl.IsHasPower()) 
        {
            GameMode.PlayerUI.ShowFleshTextOnlyTraing("Training.6");
            GameMode.PlayerUI.ShowText("UI.NoPower",true);
            return; 
        }
        if (_doorControl.IsLockDoor()) ImpossibleOpen();
        else
        {
            _audioSource.clip = _positivOpen;
            _audioSource.Play();
            _doorControl.Interact();
        }
    }

    public bool Interact(ref GameObject interactingOject)
    {
        if (!_doorControl.IsHasPower())
        {
            GameMode.PlayerUI.ShowText("UI.NoPower", true);
            return false;
        }
        if (!_doorControl.IsLockDoor())
        {
            _doorControl.Interact();
            _audioSource.clip = _positivOpen;
            _audioSource.Play();
            return true;
        }
        else
        {           
            if (interactingOject.GetComponent<PickableItem>().GetCardColor() == _doorControl.GetCardColor())
                _doorControl.UnLockDoor();
            else ImpossibleOpen();
        }
        return false;
    }
}


