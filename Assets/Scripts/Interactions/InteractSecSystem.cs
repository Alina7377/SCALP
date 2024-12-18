using UnityEngine;

public class InteractSecSystem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemType _typeInteractObg;
    [SerializeField] private AudioSource securityRoom;
    [SerializeField] private AudioSource powerRoom;
    [SerializeField] private AudioSource powerDown;
    private bool _isActivate = false;


    private void Impossible() 
    {
        if (_isActivate) GameMode.PlayerUI.ShowText("UI.AlreadyActive", true);
        else GameMode.PlayerUI.ShowText("UI.Request", true);

    }

    public void Interact()
    {
        Impossible();
    }

    public bool Interact(ref GameObject interactingOject)
    {
        PickableItem item = interactingOject.GetComponent<PickableItem>();
        if (item.GetItemType() == _typeInteractObg && !_isActivate)
        {
            if (_typeInteractObg == ItemType.StunGun)
            {
                powerRoom.Play();
                powerDown.Play();
                GameMode.PlayerUI.ShowText("UI.LightClose",true);
                Events.Instance.ChangeStateBlackOut(true);
            }
            else
            {
                securityRoom.Play();
                GameMode.PlayerUI.ShowText("UI.OpenDoors", true);
                Events.Instance.ChangeOpenDoor(true);
            }
            _isActivate = true;
        }
        else Impossible();
        return false;
    }
}


