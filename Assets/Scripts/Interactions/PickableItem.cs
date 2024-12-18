using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{  
    [Header("Item Settings")]
    [SerializeField] private AccessCardColor _cardColor;
    [SerializeField] private ItemType _itemType;


    public ItemType GetItemType() { return _itemType; }

    public AccessCardColor GetCardColor() { return _cardColor; }

    public void Interact()
    {
        Destroy(gameObject);
    }

    public bool Interact(ref GameObject interactingOject)
    {
        return true;
    }
}