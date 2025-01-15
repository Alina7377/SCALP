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

public struct SProfileData
{
    public string UserName;
    public float BestTime;
    public float CountWin;
    public float CountGame;
    public float TimeToGame;
}