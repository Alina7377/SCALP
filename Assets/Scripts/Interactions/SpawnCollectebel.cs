using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpawnCollectebel : MonoBehaviour
{
    [SerializeField] private int _maxCountCpllectebel;
    [SerializeField] private List<SEnemyWayPoint> _spawnPoints;

    private int _currentCollect;
    private CollectibleType _lastType = CollectibleType.None;

    private void Awake()
    {
        if (_maxCountCpllectebel ==0 || _maxCountCpllectebel>_spawnPoints.Count) 
            _maxCountCpllectebel = _spawnPoints.Count;
    }


    public bool CanSpawn() 
    {
        if (_currentCollect>=_maxCountCpllectebel) return false;
        foreach(SEnemyWayPoint spawnPoint in _spawnPoints)
            if (spawnPoint.IsAvail) return true;
        return false;
    }

    public CollectibleType GetLastSpawnType() 
    {
        return _lastType;
    } 

    public Transform GetPointSpawnObject() 
    {
        
        int index;
        SEnemyWayPoint sEnemyWayPoint;
        while (true) 
        {
            index = UnityEngine.Random.Range(0, _spawnPoints.Count);
            if (!_spawnPoints[index].IsAvail) continue;
            sEnemyWayPoint = _spawnPoints[index];
            sEnemyWayPoint.IsAvail = false;
            _spawnPoints[index] = sEnemyWayPoint;
            _currentCollect++;
            return sEnemyWayPoint.Point;
        }
    }

    public void SetLastSpawnType(CollectibleType type) 
    {
        _lastType = type;
    }

}
