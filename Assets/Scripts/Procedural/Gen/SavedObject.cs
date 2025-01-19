using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedObject : MonoBehaviour
{
    [SerializeField] private GameObject _prefReport;
    [SerializeField] private GameObject _prefAudioRecord;

    private Dictionary<PickableItem, SObgectTransform> _listObject = new Dictionary<PickableItem, SObgectTransform>();
    private Dictionary<RoomAccessControl, bool[]> _doorData = new Dictionary<RoomAccessControl, bool[]>();
    private List<GameObject> _collectebleObjects = new List<GameObject>();
    private Vector3 _playerPosition;
    private Quaternion _playerRotation;
    private int _countScrach = 100;
    private List<GameObject> _listScrach = new List<GameObject>();


    private void Awake()
    {
        GameMode.SavedObject = this;
    }
    /// <summary>
    ///  Перемещение подбираемых предметов по метсам
    /// </summary>
    private void LoadPicableObgect()
    {
        SObgectTransform transform = new SObgectTransform();
        foreach (var item in _listObject)
        {            
            if(item.Key == null) continue;
            transform = item.Value;
            item.Key.transform.position = transform.Position;
            item.Key.transform.rotation = transform.Rotation;
        }
    }

    private void CreateCollectebleObject() 
    {
        // Активируем все подобранные предметы
        foreach (var item in _collectebleObjects)
        {
            item.SetActive(true);
            item.layer = 8;
        }
    }

    private void SetDoorAcces() 
    {
        foreach (var door in _doorData) 
        {
            door.Key.HasPower = door.Value[0];
            if (!door.Value[0])               
                door.Key.NoNav();
            if (door.Value[1])
                door.Key.LockDoor();
        }
    }

    private void MovementPlayerPosition() 
    {
        GameMode.FirstPersonMovement.transform.position = _playerPosition;
        GameMode.FirstPersonMovement.transform.rotation = _playerRotation;
    }

    private void ClearScrech()
    {
        foreach (var item in _listScrach)
        {
            Destroy(item);
        }
        _listScrach.Clear();
    }


    /// <summary>
    /// Подготавлеваем класс к перезаписи
    /// </summary>
    public void ResetObjectData() 
    {
        _listObject.Clear();
        _collectebleObjects.Clear();
        _doorData.Clear();
    }

    /// <summary>
    /// Сохраняем позиции всех поднимаемых объектов
    /// </summary>
    /// <param name="savedObject"></param>
    public void SafePicableObject()
    {
        PickableItem[] listObject = FindObjectsOfType<PickableItem>();
        foreach (PickableItem item in listObject)
        {
            SObgectTransform newObjectData = new SObgectTransform();
            newObjectData.Position = item.transform.position;
            newObjectData.Rotation = item.transform.rotation;
            _listObject.Add(item, newObjectData);
        }
    }

    /// <summary>
    /// Сохраняем местонахождение коллкционных предметов
    /// </summary>
    /// <param name="tag"></param>
    public void SafeCollectebl(GameObject collObject) 
    {
        _collectebleObjects.Add(collObject);
    }


    /// <summary>
    /// Сохраняем данные о закрытых дверях и дверях без питания
    /// </summary>
    public void SafeDoorData(RoomAccessControl room, int param, bool val) 
    {
        bool[] updVal;
        if (_doorData.ContainsKey(room))
        {
            updVal = _doorData[room];
            updVal[param] = val;
            _doorData[room] = updVal;
        }
        else
        {
            // Если у нас небыло значения, то устанваливаем стандартно - у двери есть питание и не заблочена на ключ
            updVal =  new bool [2]{ true, false };
            updVal [param] = val;
            _doorData.Add(room, updVal);
        }
    }

    // Методы загрузки данных
    public void LoadSaveData() 
    {
        LoadPicableObgect();
        CreateCollectebleObject();
        SetDoorAcces();
        ClearScrech();
    }

   
    public void AddScrach(GameObject newScrach) 
    {
        _listScrach.Add(newScrach);
        if (_listScrach.Count > _countScrach)
        {
            Destroy(_listScrach[0]);
            _listScrach.RemoveAt(0);
        }
    }

    
}
