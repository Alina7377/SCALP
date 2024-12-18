using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightEnemy : MonoBehaviour
{
    [SerializeField] private Light _flashlight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            GameMode.FirstPersonLook.AddLight(_flashlight);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            GameMode.FirstPersonLook.RemoveLight(_flashlight);
    }

    private void OnDisable()
    {
        GameMode.FirstPersonLook.RemoveLight(_flashlight);
    }
}
