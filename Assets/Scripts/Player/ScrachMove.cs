using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class ScrachMove : MonoBehaviour
{
    [SerializeField] private GameObject _imageScrach;
    [SerializeField] private ColliderScratch _topCollider;
    [SerializeField] private ColliderScratch _rightCollider;
    [SerializeField] private ColliderScratch _leftCollider;
    [SerializeField] private ColliderScratch _downCollider;
    [SerializeField] private float _step;

    private InteractSoundSource _soundSourse;
    private Vector3 _directionToLeft;
    private Vector3 _directionToRight;
    private bool _isCheaking = false;
    private GameObject _hitObject;

    private void Awake()
    {
        _soundSourse = GetComponent<InteractSoundSource>();
    }
    private void Update()
    {
        if (!_isCheaking) return;
        if (_topCollider.IsHasCollisonOfObject(_hitObject)   &&
            _rightCollider.IsHasCollisonOfObject(_hitObject) &&
            _leftCollider.IsHasCollisonOfObject(_hitObject)  &&
            _downCollider.IsHasCollisonOfObject(_hitObject))
        {
            GoodPosition();
        }
        else
        {
            MoveScreach();
        }
    }

    private void MoveScreach()
    {
        // Ситуации, при которых разместить объект невозможно
        if (!_topCollider.IsHasCollisonOfObject(_hitObject) && !_downCollider.IsHasCollisonOfObject(_hitObject))
        {
            Debug.Log("Объект уничтожен, т.к. нет пересечений");
            Destroy(gameObject);
        }

        if (!_leftCollider.IsHasCollisonOfObject(_hitObject) && !_rightCollider.IsHasCollisonOfObject(_hitObject))
        {
            Debug.Log("Объект уничтожен, т.к. нет пересечений (горизонт)");
            Destroy(gameObject);
        }

        Vector3 newPosition = transform.position;
       
        if (!_topCollider.IsHasCollisonOfObject(_hitObject))
            newPosition.y -= _step;           
        if (!_downCollider.IsHasCollisonOfObject(_hitObject))
            newPosition.y += _step;

        
       
        if (!_leftCollider.IsHasCollisonOfObject(_hitObject))
        {
           /* Оставлю это на память
            * Vector3 newDirect = _directionToLeft * _step;
            Debug.Log(newDirect);
            //newPosition = new Vector3(newPosition.x + newDirect.x, newPosition.y, newPosition.z + newDirect.z); //newPosition + (_directionToLeft * _step);*/
            newPosition = newPosition + (_directionToRight * _step);
        }

        if (!_rightCollider.IsHasCollisonOfObject(_hitObject))
        {
            newPosition = newPosition + (_directionToLeft * _step);
        }
        transform.position = newPosition;

    }

    private void GoodPosition() 
    {
        _topCollider.gameObject.SetActive(false);
        _rightCollider.gameObject.SetActive(false);
        _leftCollider.gameObject.SetActive(false);
        _downCollider.gameObject.SetActive(false);
        _isCheaking = false;
        float angel = UnityEngine.Random.Range(-90, 90);
        _imageScrach.transform.localRotation = Quaternion.AngleAxis(angel,Vector3.back);
        _imageScrach.SetActive(true);
        _soundSourse.Interact();
        GameMode.SavedObject.AddScrach(gameObject);
    }

    // Микро задержка, чтобы отработали колайдеры
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.05f);
        _isCheaking = true;
        _directionToLeft = NormolizeVector(_rightCollider.transform.position, _leftCollider.transform.position);
        _directionToRight = NormolizeVector(_leftCollider.transform.position, _rightCollider.transform.position);
    }

    private Vector3 NormolizeVector(Vector3 origin, Vector3 derection)
    {
        float distance = Vector3.Distance(origin, derection);
        return (derection - origin) / distance;
    }

    public void StartCheack(GameObject hit) 
    {
        _hitObject = hit;
        StartCoroutine(Wait());
    }

 
}

