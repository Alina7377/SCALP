using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Mathematics;
using UnityEngine;

public class PersonHand : MonoBehaviour
{
    [SerializeField] private Transform _handPoint;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _pickUpDistance;
    [SerializeField] private float _throwForce;
    [SerializeField] private LayerMask _layerMask;
    [Header("Настройки для царапанья")]
    [SerializeField] private GameObject _scrach;
    [SerializeField] private LayerMask _layerMaskScrach;
    [SerializeField] private float _scrachDistance;

    private PlayerControl _control;
    private GameObject _hitObject;
    private GameObject _grabObject;
    private Rigidbody _grabObjectRigidbody;
    private float _throwVerticalForce = 0.2f;
    private Vector3 _startScaleGrabObj = Vector3.zero;
    private Dictionary<AccessCardColor, bool> _inventaryCard = new Dictionary<AccessCardColor, bool>();
    //private List<AccessCardColor> _r;
    private float _offsetVertical = 0.04f;

    private void Awake()
    {
        _control = new PlayerControl();
        _control.Player.Interact.started += context => Interaction();
        _control.Player.Throw.started += context => ThrowObject();
        _control.Player.Throw.started += context => CreateScrach();
        _control.Player.Drop.started += context => DropObject();
        _inventaryCard.Add(AccessCardColor.None,true);
        GameMode.PersonHand = this;
    }

    private void CreateScrach()
    {
        if (_grabObject != null) return;
        RaycastHit hit;
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        if (Physics.Raycast(ray, out hit, _scrachDistance, _layerMaskScrach))
        {
            if (hit.transform.gameObject.tag == "VerticalHolst" || hit.transform.gameObject.tag == "HorizontallHolst")
            {
                // Повторот во круг оси (пригодиться)
                Vector3 direct = NormolizeVector(transform.position, hit.point);
                Vector3 position = hit.point;
                Quaternion quaternion;
                if (hit.transform.gameObject.tag == "VerticalHolst")
                {
                    if (math.abs(direct.x) > math.abs(direct.z) && 
                        math.abs(direct.x) - math.abs(direct.z) > 0.5 || CheackRayForWall(direct.x, hit.collider.gameObject))
                    {
                        // Костыль для деври
                        if (hit.collider.gameObject.layer != 3)
                            if (direct.x < 0)
                                position.x = position.x - _offsetVertical;
                            else position.x = position.x + _offsetVertical;
                        else
                            if (direct.x < 0)
                            position.x = position.x - _offsetVertical / 2.1f;
                        else position.x = position.x + _offsetVertical / 2.1f;
                        quaternion = Quaternion.AngleAxis(90, Vector3.up);
                    }
                    else
                    {
                        // Костыль для деври
                        if (hit.collider.gameObject.layer != 3)
                            if (direct.z < 0)
                                position.z = position.z - _offsetVertical;
                            else position.z = position.z + _offsetVertical;
                        else
                            if (direct.z < 0)
                               position.z = position.z - _offsetVertical /2.1f;
                           else position.z = position.z + _offsetVertical /2.1f;
                        quaternion = Quaternion.AngleAxis(0, Vector3.up);
                    }
                }
                else
                {
                    position.y = position.y + 0.006f;
                    quaternion = Quaternion.AngleAxis(90, Vector3.right);
                }
                GameObject gameObject = Instantiate(_scrach, position, quaternion);
                gameObject.transform.SetParent(hit.collider.transform);
                ScrachMove scrachMove = gameObject.GetComponent<ScrachMove>();
                scrachMove.StartCheack(hit.collider.gameObject);
            }
            else return;
        }
    }

    private bool CheackRayForWall(float  directX, GameObject hitObj) 
    {
        Vector3 newDirect = Vector3.zero;
        if (directX < 0)
            newDirect.x = -1;
        else
            newDirect.x = 1;
        RaycastHit hit;
        Ray ray = new Ray(_cameraTransform.position, newDirect);
        if (Physics.Raycast(ray, out hit, _scrachDistance, _layerMaskScrach))
        {
            if (hit.collider.gameObject == hitObj)
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 NormolizeVector(Vector3 origin, Vector3 derection) 
    {
        float distance = Vector3.Distance(origin, derection);
        return (derection - origin) / distance;
    }

    private void Update()
    {
        CheackInteract();
    }

    private void OnEnable()
    {
        _control.Enable();
    }

    private void OnDisable()
    {
        _control.Disable();
    }

    private void GrabObject() 
    {
        if (_hitObject == null) return;
        _grabObject = _hitObject;
        _grabObjectRigidbody = _hitObject.GetComponent<Rigidbody>();
        if (_grabObject != null)
        {
            _grabObjectRigidbody.isKinematic = true;
            _grabObject.layer = 2;
            StartCoroutine(MoveToHand()); // Плавно перемещаем объект к руке
        }
        if (GameMode.PlayerUI._isTraining)
        {
            PickableItem pickableItem;
            if (_grabObject.TryGetComponent<PickableItem>(out pickableItem) &&
                pickableItem.GetCardColor()==AccessCardColor.Green)
                GameMode.PlayerUI.ShowFleshTextOnlyTraing("Training.4");
        }
    }

    /// <summary>
    /// Плавное перемещение объекта к руке
    /// </summary>
    private IEnumerator MoveToHand()
    {
        // Делаем объект зависимым
        _startScaleGrabObj = _grabObject.transform.localScale;
        _grabObject.transform.SetParent(_handPoint);
        // А затем, пересещаем в центер координат
        Vector3 targetPosition = Vector3.zero;
        float duration = 0.3f; // Длительность анимации
        float elapsedTime = 0f;

        while (elapsedTime < duration && _grabObject != null)
        {
            // Плавно перемещаем объект к позиции руки
            _grabObject.transform.localPosition = Vector3.Lerp(_grabObject.transform.localPosition, targetPosition, (elapsedTime / duration));
            _grabObject.transform.rotation = Quaternion.Lerp(_grabObject.transform.rotation, transform.rotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime; // Увеличиваем время
            yield return null; // Ждем следующего кадра
        }
        if (_grabObject != null)
        {
            _grabObject.transform.localPosition = targetPosition; // Объект точно на позиции руки
            _grabObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// Выпуск лучей и поиск интерактивных объектов
    /// </summary>
    private void CheackInteract() 
    {
        RaycastHit hit;
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        if (Physics.Raycast(ray, out hit, _pickUpDistance, _layerMask))
        {
            SetInteractObject(hit.transform.gameObject);
        }
        else 
        {
            SetNullInteract();
        }
    }

    /// <summary>
    /// Нажатие на кнопку взаимодействия
    /// </summary>
    private void Interaction() 
    {
        if (_hitObject != null)
        {
            switch (_hitObject.tag) 
            {
                case "Grab":
                case "Placed":
                    // Если есть другой объект в руках - бросаем его
                    if (_grabObject != null) DropObject();
                    GrabObject();
                    break;
                case "Take":
                    _hitObject.transform.GetComponent<IInteractable>().Interact(ref _hitObject);
                    _hitObject.layer = 0;
                    _hitObject.SetActive(false);
                    _hitObject = null;
                    GameMode.PlayerUI.DeactivatePanel();
                    break;
                case "Interact":
                    _hitObject.transform.GetComponent<IInteractable>().Interact();
                    break;
                case "Place":
                    TransferObject();
                    break;
                default:
                    break;
            }            
            return;
        }
    }

    private void SetInteractObject(GameObject newInteractObject)
    {
        // Это для того, чтобы "не видеть" объекты у нас в руках
        _hitObject = newInteractObject;
        GameMode.PlayerUI.ShowText("UI.Interact",false);
    }
        
    private void DropObject() 
    {        
        if (_grabObject == null) return;
        _grabObject.layer = 8;
        _grabObject.transform.SetParent(null);
        _grabObject.transform.localScale = _startScaleGrabObj;
        _grabObjectRigidbody.isKinematic = false; // Делаем объект динамическим        
        _grabObject = null;
    }

    private void ThrowObject() 
    {
        Debug.LogWarning("Попытка создать объект на  " + _hitObject);
        if (_grabObject == null)
            if (_hitObject != null && (_hitObject.tag == "VerticalHolst" || _hitObject.tag == "HorizontallHolst"))
            {
                GameObject gameObject = Instantiate(_scrach, _hitObject.transform.position, new Quaternion(0,90,0,0));
            }
            else return;
        _grabObject.layer = 8;
        _grabObject.transform.SetParent(null);
        _grabObject.transform.localScale = _startScaleGrabObj;
        _grabObjectRigidbody.isKinematic = false;
        _grabObjectRigidbody.AddForce((_cameraTransform.forward + Vector3.up * _throwVerticalForce) * _throwForce);
        _grabObject = null;
    }

    private void SetNullInteract() 
    {
        if (_hitObject == null) return ;
        _hitObject = null;
        GameMode.PlayerUI.DeactivatePanel();
    }

    private void TransferObject()
    {
        IInteractable interactObj = _hitObject.GetComponent<IInteractable>();
        // Если у нас в руках есть предмет, который может взаимодействовть - пробуем им взаимодействовать
        if (_grabObject != null && interactObj.Interact(ref _grabObject))
        {
            if (_grabObject == null) 
                _grabObjectRigidbody = null;
        }
        // Если нет - обычное взаимодействие
        else 
        if (_grabObject == null)
        {
            interactObj.Interact();
        }
    }    

    public bool HasCard(AccessCardColor card) 
    {
        return _inventaryCard.ContainsKey(card);
    }

    public PickableItem GetGrabObject() 
    {
        if (_grabObject == null) return null;
        PickableItem pickableItem;
        if (_grabObject.TryGetComponent<PickableItem>(out pickableItem))
            return pickableItem;
        return null;
    }
}
