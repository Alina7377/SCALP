using UnityEngine;

public enum OccupancyType
{
    None,
    Room,
    MainPath,
    SecondaryPath,
    TertiaryPath 
}

public class GridCell : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public bool isOccupied = false; // Флаг занятости клетки
    public GameObject occupyingRoom; // Комната, которая занимает клетку
    public bool isDoorway = false; // Флаг, показывающий, является ли клетка дверным проемом
    public OccupancyType occupancyType = OccupancyType.None; // Тип занятости клетки
    public bool isConnectedToMainPath = false; // Флаг, показывающий, соединена ли клетка с основным путем

    public RoomAccessControl roomAccessControl; // Ссылка на скрипт RoomAccessControl, если клетка принадлежит комнате

    // Для удобства, отображаем название комнаты в инспекторе
    public string OccupyingRoomName => occupyingRoom != null ? occupyingRoom.name : "None";
}
