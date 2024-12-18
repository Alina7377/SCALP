using System;
using UnityEngine;

[Serializable]
public struct SEnemyWayPoint 
{
    public Transform Point;
    public bool IsAvail;
}

public struct SRoomSpawnEnemyData
{
    public RoomAccessControl Rroom;
    public int MaxCountEnemyInRoom;
    public int CurrentCountEnemy;
}