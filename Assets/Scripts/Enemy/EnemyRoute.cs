using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoute : MonoBehaviour
{
    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private int _countMaxEnemyInRoom;
    
    [SerializeField] private bool _hasExit = true;

    public List<Transform> GetWayPoints() { return _wayPoints; }

    public int CountMaxEnemyInRoom 
    { get { if (_hasExit) return _countMaxEnemyInRoom;
            return 1; } 
    }

    public bool HasExit
    {
        get { return _hasExit; }
        set { _hasExit = value; }
    }
}
