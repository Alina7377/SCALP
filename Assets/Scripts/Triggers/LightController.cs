using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour, IInteractable
{
    [SerializeField] private List<Light> _lamps;
    [SerializeField] private LayerMask _layersMask;
    [SerializeField] private float _coffMinim = 1f;
    [SerializeField] private bool _startEnabled = true;

    private List<EnemyAI> _enemyAIs = new List<EnemyAI>();

    private bool _isEnabled = true;
    private bool _isBlackout = false;

    private void Start()
    {
        Events.Instance.OnBalckOut += ChangeBlackOut;
        _isEnabled = _startEnabled;
    }

    private void OnDisable()
    {
        Events.Instance.OnBalckOut -= ChangeBlackOut;
    }

    private void OnTriggerStay(Collider other)
    {
        float h, h1,r;
        Vector3 conusPos;
        if (other.tag == "Player" && _isEnabled && !_isBlackout)
        {
            foreach (Light light in _lamps)
            {
                if (!light.enabled) continue;
                // Запаминаем позицию по y лампочки
                h = light.transform.position.y;
                h1 = h - other.transform.position.y;
                r = (h1 / h) * (light.range * _coffMinim);
                conusPos = new Vector3(light.transform.position.x, GameMode.FirstPersonMovement.transform.position.y, light.transform.position.z);
                if (Vector3.Distance(other.transform.position, conusPos) < r)
                {
                    ReleaseRayCast(light);
                }
                else GameMode.FirstPersonLook.RemoveLight(light);
            } 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy") 
            _enemyAIs.Add(GameMode.EnemyManager.GetEnemyForGameObject(other.gameObject));
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            foreach (Light light in _lamps)
                GameMode.FirstPersonLook.RemoveLight(light);
        else if (other.tag == "Enemy") 
                _enemyAIs.Remove(GameMode.EnemyManager.GetEnemyForGameObject(other.gameObject));
    }

    private UnityEngine.Vector3 NormolizateVector(UnityEngine.Vector3 origin, UnityEngine.Vector3 endpoint)
    {
        return (endpoint - origin) / UnityEngine.Vector3.Distance(endpoint, origin);
    }

    private void ReleaseRayCast(Light light) 
    {
        Ray ray = new Ray(light.transform.position, NormolizateVector(light.transform.position, GameMode.FirstPersonMovement.transform.position));
        //Проверка        
        Debug.DrawRay(ray.origin, ray.direction * 200, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, _layersMask))
        {
            if (hit.collider.tag == "Player")
            {
                GameMode.FirstPersonLook.AddLight(light);
            }
            else
            {
                GameMode.FirstPersonLook.RemoveLight(light);
            }
        }
        else { Debug.Log("Лучь не попапл" + hit); }
    }

    public void Interact()
    {
        if (!_isBlackout)
        {
            _isEnabled = !_isEnabled;
            for (int i = 0; i < _lamps.Count; i++) 
            {
                _lamps[i].enabled = _isEnabled;
            }            
            foreach (EnemyAI enemy in _enemyAIs)
            {
                enemy.LightAlways(!_isEnabled);
            }
        }
    }

    public bool Interact(ref GameObject interactingOject)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeBlackOut(bool state) 
    {
        _isBlackout = state;
        if (_isBlackout)
            foreach (Light light in _lamps)
                GameMode.FirstPersonLook.RemoveLight(light);
    }
}

