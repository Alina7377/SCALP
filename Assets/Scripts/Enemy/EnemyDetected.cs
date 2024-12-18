using System;
using System.Collections;
using System.Numerics;
using UnityEngine;

public class EnemyDetected : MonoBehaviour
{
    [SerializeField, Tooltip("Точка обзора.")] private Transform _viewPoint;
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField, Tooltip("Время отслеживания после потери из вида")] private float _waitToSearch;
    [SerializeField] private LayerMask _layerMask;
   
    private bool _isDetected = false;
    private bool _isInVievZone = false;

    // Время, спустя которое 
    private float _timeLoss;


    private void OnTriggerEnter(Collider other)
    {
         
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && GameMode.FirstPersonLook.GetVisability() && !_isInVievZone)
            _isInVievZone = true;
    }

    private void Update()
    {

        if (!_isInVievZone && _timeLoss == 0)  return;
        
        Ray ray = new Ray(_viewPoint.position,NormolizateVector(_viewPoint.position, GameMode.FirstPersonLook.transform.position));
        //Проверка        
        Debug.DrawRay(ray.origin, ray.direction * 200,Color.red);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,20, _layerMask))
        {
            if (hit.collider.tag == "Player")
            {
                _isDetected = true;
                _enemyAI.ChasePlayer();
            }
            else
            if (_isDetected)
            {
                // _isInVievZone = false; //Без этого, кажется лучше
                _isDetected = false;
                _timeLoss = Time.time;
            }
        }
        if (!_isDetected && _timeLoss > 0)
            WaitToSearch();


    }

    /// <summary>
    /// Некоторая задержка перед тем, как завершить слежение и перейти к поиску
    /// </summary>
    private void WaitToSearch()
    {
        if (Time.time - _timeLoss >= _waitToSearch && !_isDetected)
        {
            _enemyAI.StartSearchingPlayer();
            _timeLoss = 0;
            _isInVievZone = false; 
        }
        else
        {
            _enemyAI.ChasePlayer();
        }
    }

    private void OnTriggerExit(Collider other)
     {
         if (!_isDetected && other.gameObject.tag == "Player")
            _isInVievZone = false;
     }       

    private UnityEngine.Vector3 NormolizateVector(UnityEngine.Vector3 origin, UnityEngine.Vector3 endpoint) 
    {
        return (endpoint - origin) / UnityEngine.Vector3.Distance(endpoint, origin);
    }

 }
