using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTrigger : MonoBehaviour
{
    [SerializeField] private string _tag;
    [SerializeField] private bool _isHasCondition;

    [SerializeField] private ItemType _itemType;
    [SerializeField] private AccessCardColor _cardColor;



    private void OnTriggerEnter(Collider other)
    {
        if (!_isHasCondition || (_isHasCondition && CheackCondition()) && !GameMode.PlayerUI.IsPlayingAnimator())
        {
            GameMode.PlayerUI.ShowFleshText(_tag);
            Destroy(gameObject);
        }
    }

    private bool CheackCondition() 
    {
        PickableItem pickableItem = GameMode.PersonHand.GetGrabObject();
        if (pickableItem == null) return false;
        if (pickableItem.GetItemType()==_itemType && pickableItem.GetCardColor()== _cardColor) 
            return true;
        return false;

    }
}
