using System;
using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{  
    [Header("Item Settings")]
    [SerializeField] private AccessCardColor _cardColor;
    [SerializeField] private ItemType _itemType;

    private void Awake()
    {
        Events.Instance.OnReloadLevel += Reload;
    }

    private void Reload()
    {
        gameObject.layer = 8;
        gameObject.SetActive(true);
    }

    public ItemType GetItemType() { return _itemType; }

    public AccessCardColor GetCardColor() { return _cardColor; }

    public void Interact()
    {
        gameObject.SetActive(false);
    }

    public bool Interact(ref GameObject interactingOject)
    {
        return true;
    }
}