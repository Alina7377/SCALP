using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportCollectebel : MonoBehaviour, IInteractable
{

    [SerializeField] private CollectibleType _collectibleType;
    [SerializeField] private string _tag;
    [SerializeField] private Sprite _image;
    [SerializeField] private AudioClip _clip;

    public CollectibleType CollectibleType 
    {
        get { return _collectibleType; }
        set { _collectibleType = value; }
    }

    public string Tag
    {
        get { return _tag; }
        set { _tag = value; }
    }

    public Sprite Image
    {
        get { return _image; }
        set { _image = value; }
    }

    public AudioClip Clip
    {
        get { return _clip; }
        set { _clip = value; }
    }

    public void Interact()
    {
              
    }

    public bool Interact(ref GameObject interactingOject)
    {    
        LocalizationManager.Instance.WriteAvailForTag(_tag, "grab");
        if (GameMode.PlayerUI._isTraining)
            GameMode.PlayerUI.ShowFleshText("Training.12");
        else GameMode.PlayerUI.ShowFleshText("UI.GrabNew");
        interactingOject = this.gameObject;
        return true;
    }
}
