using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoorControl : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _hasOpenBots = true;
    [SerializeField] private Animation _animate;
    [SerializeField] private RoomAccessControl _roomAccessControl;
    [SerializeField] private GameObject _door1;
    [SerializeField] private GameObject _door2;
    [SerializeField] private LoaclizationText _roomName1;
    [SerializeField] private LoaclizationText _roomName2;
    [SerializeField] private AudioClip _doorOpen;
    [SerializeField] private AudioClip _doorClose;
    [SerializeField] private AudioSource _doorAudioSource;

    // Список всех, кто взаимодействует с дверью
    private List<GameObject> _interactors = new List<GameObject>();
    private bool _isOpen = false;
    private bool _isLock = true;
    private bool _isAlwaysOpen = false;

    private void Awake()
    {
        _roomAccessControl.NoPower += BlockDoors;
        if (_roomAccessControl.GetTagNameRoom() != "")
        {
            _roomName1.SetTag(_roomAccessControl.GetTagNameRoom());
            _roomName2.SetTag(_roomAccessControl.GetTagNameRoom());
        }
    }

    private void Start()
    {
        Events.Instance.OnInteractGenerator += AnBlockDoors;
        Events.Instance.OnOpenDoor += GloabalOpenDoor;        
    }

    private void OnDisable()
    {
        Events.Instance.OnInteractGenerator -= AnBlockDoors;
        Events.Instance.OnOpenDoor -= GloabalOpenDoor;
    }

    private void GloabalOpenDoor(bool obj)
    {
        _isAlwaysOpen = true;
        if (!_isOpen && _roomAccessControl.HasPower)
        { 
            PlayClip("OpenDoor");
            _isOpen = true;
        }
        StartCoroutine(OpenDoor(30f));

    }

    private IEnumerator OpenDoor(float second)
    {
        yield return new WaitForSeconds(second);
        _isAlwaysOpen = false;
        if (_isOpen && _interactors.Count == 0)
        {
            PlayClip("CloseDoor");            
            _isOpen = false;
        }
    }

    private void BlockDoors()
    {
        // Устанавливаем слой, учитываемый навмеш
        Debug.Log("Меняем наименования на дверях");
        _door1.layer = 0;
        _door2.layer = 0;
        _roomName1.SetTag("UI.NoPower");
        _roomName2.SetTag("UI.NoPower");
        _roomName1.GetComponent<TextMeshPro>().color = Color.red;
        _roomName2.GetComponent<TextMeshPro>().color = Color.red;
    }

    private void AnBlockDoors()
    {
        _door1.layer = 3;
        _door2.layer = 3;
        _roomName1.SetTag(_roomAccessControl.GetTagNameRoom());
        _roomName2.SetTag(_roomAccessControl.GetTagNameRoom());
        _roomName1.GetComponent<TextMeshPro>().color = Color.white;
        _roomName2.GetComponent<TextMeshPro>().color = Color.white;
    }

    /// <summary>
    /// Открытие дверей для Ботов
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    { 
        if(_isLock)
            if(_roomAccessControl != null && _roomAccessControl.GetCardColor() == AccessCardColor.None || _roomAccessControl == null)
                _isLock = false;
        if (((_roomAccessControl == null || _roomAccessControl.HasPower)) && other.gameObject.tag == "Enemy" && !_isAlwaysOpen && _hasOpenBots)
        { 
            PlayClip("OpenDoor");
            _interactors.Add(other.gameObject);
            _isOpen = true;
        }       
    }

    /// <summary>
    /// Закрытие двери игроком и ботоами
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (_isOpen && 
           (_roomAccessControl == null || _roomAccessControl.HasPower) && 
           (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Player"))
        {
            // Закрываем дверь, только если все, кто может, через нее прошли
            if (_interactors.Count > 0) _interactors.Remove(other.gameObject);
            if (_interactors.Count == 0 && !_isAlwaysOpen) _isOpen = false;
            if (!_isAlwaysOpen)
                PlayClip("CloseDoor");
        }
    }

    /// <summary>
    /// Завпуск анимации
    /// </summary>
    /// <param name="other"></param>
    private void PlayClip(string name)
    {
        if (_interactors.Count > 0 && !_isAlwaysOpen) return;
        _animate.clip = _animate.GetClip(name);
        _animate.Play();
        if (name == "OpenDoor") _doorAudioSource.clip = _doorOpen;
        else _doorAudioSource.clip = _doorClose;
        _doorAudioSource.Play();
    }

  

    /// <summary>
    ///  Открытие двери через кодлок
    /// </summary>
    public void Interact()
    {
        if (!_isLock && !_isOpen) 
        {
            _isOpen = true;
            //_interactors.Add(GameMode.PersonHand.gameObject);
            PlayClip("OpenDoor");
        }
    }

    public bool Interact(ref GameObject interactingOject)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    ///  Разблокирование и открытие двери
    /// </summary>
    public void UnLockDoor() 
    {
        _isLock = false;
        Interact();
    }

    public bool IsLockDoor() => _isLock;

    public bool IsHasPower()
    {
        if (_roomAccessControl != null) 
            return _roomAccessControl.HasPower;
        else return true;
    }

    public AccessCardColor GetCardColor() => _roomAccessControl.GetCardColor();

    public GridManager GetGrid() 
    {
        return _roomAccessControl.GetGrid;
    }
}
