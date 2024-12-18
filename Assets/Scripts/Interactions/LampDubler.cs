using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampDubler : MonoBehaviour
{
    [SerializeField] private Light _baseLamp;

    private Light _currentLight;

    private void Awake()
    {
        _currentLight = GetComponent<Light>();
    }

    private void Update()
    {
        _currentLight.enabled = _baseLamp.enabled;
        _currentLight.color = _baseLamp.color;
    }
}
