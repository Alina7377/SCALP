using System;

public static class GameMode
{
    private static FirstPersonLook _playerCamera;
    private static PersonHand _personHand;
    private static PlayerUI _playerUI;
    private static FirstPersonMovement _personMovement;
    private static EnemyManager _enemyManager;
    private static float _startTime;
    private static float _endTime;

    public static PersonHand PersonHand
    {
        get { return _personHand; }
        set { _personHand = value; }
    }

    public static FirstPersonLook FirstPersonLook
    {
        get { return _playerCamera; }
        set { _playerCamera = value; }
    }

    public static FirstPersonMovement FirstPersonMovement
    {
        get { return _personMovement; }
        set { _personMovement = value; }
    }

    public static PlayerUI PlayerUI
    {
        get { return _playerUI; }
        set { _playerUI = value; }
    }

    public static EnemyManager EnemyManager
    {
        get { return _enemyManager; }
        set { _enemyManager = value; }
    }

    public static float StartTime
    { 
        get { return _startTime; } 
        set { _startTime = value; } 
    }

    public static float EndTime
    {
        get { return _endTime; }
        set { _endTime = value; }
    }
}
