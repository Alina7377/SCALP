using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulb : MonoBehaviour, IInteractable
{
    [SerializeField] private List<Light> _lights;

    private void Start()
    {
        
    }    

    public void Interact()
    {
        if (_lights.Count>0)
        {
            for(int i = 0; i< _lights.Count; i++)
                _lights[i].enabled = !_lights[i].enabled;
        }
    }
       

    public void StopInteract() { }

    public bool Interact(ref GameObject interactingOject)
    {
        throw new System.NotImplementedException();
    }
}
