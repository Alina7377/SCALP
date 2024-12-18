using System.Collections.Generic;
using UnityEngine;

public class CorridorPiece : MonoBehaviour
{
    // Булевые значения для сторон
    public bool up;
    public bool right;
    public bool down;
    public bool left;
    [SerializeField] private Transform _enemyPoint;

    public Transform EnemyPoint
        { get { return _enemyPoint; } }
}