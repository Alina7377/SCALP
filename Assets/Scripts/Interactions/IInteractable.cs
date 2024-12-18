using UnityEngine;

public interface IInteractable
{
    void Interact();
    bool Interact(ref GameObject interactingOject);
}
