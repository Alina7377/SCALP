using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScratch : MonoBehaviour
{
    private List<GameObject> _objects = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "HorizontallHolst" || other.tag == "VerticalHolst")
        {
            _objects.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "HorizontallHolst" || other.tag == "VerticalHolst")
        {
            _objects.Remove(other.gameObject);
        }
    }

    public bool IsHasCollisonOfObject(GameObject gameObject) 
    {
        if (_objects.Count>1)
            return false;
        //return false;*/

        foreach (GameObject obj in _objects)
        {
            if(obj==gameObject)
                return true;
        }
        return false;
    }
}
