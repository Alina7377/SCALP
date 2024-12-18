public enum EEnemyState 
{
    Patrolling,
    Alerted,
    Chasing,
    WaitAlert,
    Searching,
    WaitChasing
}

public enum AccessCardColor
{
    None, // »спользуетс€ дл€ объектов, не €вл€ющихс€ картой доступа
    Red,
    Green,
    Blue
}

public enum ItemType 
{
    AccessCard,
    PortableBattery,
    Collectible,
    StunGun,
    Securcard,
    Throwable
}

public enum CollectibleType
{
    None,
    Reports,
    AudioRecords  
}

public enum TextType 
{
    None,
    TMP,
    TextMesh
}