using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField, Tooltip("Количество врагов.")] private int _countEnemy;
    [SerializeField, Tooltip("Количество врагов, блуждающих по коридорам")] private int _countEnemyNoRoom;
    [SerializeField] private GameObject _enemyPref;

    [Header("Настройки для обучающего уровня")]
    [SerializeField] private List<RoomAccessControl> _rooms;
    [SerializeField] private List<Transform> _wayPointsNoroom;

    private List<EnemyAI> _enemys;
    private bool _isTrainingLevel;
    private int _currnetCountInCoridors = 0;

    private Dictionary<RoomAccessControl, List<SEnemyWayPoint>> _waypoints = new Dictionary<RoomAccessControl, List<SEnemyWayPoint>>();
    // Vector2 - двумерный массив, где x - макс. кол-во, y - текущее кол-во мобов, z - принимает значение 0 или 1 - могут ли боты покидать комнату, где 0 - НЕТ
    private Dictionary<RoomAccessControl, Vector3> _roomData = new();
    private List<SEnemyWayPoint> _corridorPoints = new List<SEnemyWayPoint>();

    private void Awake()
    {
        _enemys = new List<EnemyAI>();
        GameMode.EnemyManager = this;
        if (_rooms.Count > 0 || _wayPointsNoroom.Count > 0) _isTrainingLevel = true;
        // Если комнаты заданы вручную, значит спавним вручную
        if (_isTrainingLevel)
        {
            if (_rooms.Count > 0)
            {
                EnemyRoute enemyRoute = null;
                float hasExit = 0;
                foreach (RoomAccessControl room in _rooms)
                {
                    if (room.TryGetComponent<EnemyRoute>(out enemyRoute))
                    {
                        if (enemyRoute.HasExit) hasExit = 1;
                        else hasExit = 0;
                        AddWaypoints(room, enemyRoute.CountMaxEnemyInRoom, enemyRoute.GetWayPoints(), hasExit);
                    }
                }

            }
            if (_wayPointsNoroom.Count > 0)
            {
                SEnemyWayPoint enemyFreePoint = new SEnemyWayPoint();
                foreach (Transform freePoint in _wayPointsNoroom)
                {
                    enemyFreePoint.IsAvail = true;
                    enemyFreePoint.Point = freePoint;
                    _corridorPoints.Add(enemyFreePoint);
                }
            }
            CreateEnemy();
        }
    }
    // Добавьте метод Reset для очистки врагов и связанных данных
    public void Reset()
    {
        DestroyAllEnemies();
        _waypoints.Clear();
        _roomData.Clear();
        _corridorPoints.Clear();
        _currnetCountInCoridors = 0;
    }
    public void DestroyAllEnemies()
    {
        foreach (EnemyAI enemy in _enemys)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        _enemys.Clear();
    }

    /// <summary>
    /// Провекра на возможность спавна нового врага
    /// </summary>
    /// <returns></returns>
    private bool CheackPossibilityPlacement(RoomAccessControl room, bool canChange)
    {
        if (room.HasPower && _roomData[room].x > _roomData[room].y && (_roomData[room].z== 1) == canChange)  return true;
        return false;        
    }


    private RoomAccessControl GetNewRoom(RoomAccessControl currentRoom, bool canChange) 
    {
        foreach (KeyValuePair<RoomAccessControl, List<SEnemyWayPoint>> room in _waypoints)
        {
            // Проверка на возможность спавна и на соответсвие выбранной комнаты текущей
            if (currentRoom==room.Key || !CheackPossibilityPlacement(room.Key, canChange)) continue;
            // Вероятность того, что враг заспавниться в этой комнате (только для основного уровня)
            if (!_isTrainingLevel && UnityEngine.Random.Range(0, 100) < 50)  continue;
            return room.Key;
        }
        return currentRoom;
    }

    /// <summary>
    /// Создаем ботов и назначаем им маршруты
    /// </summary>
    public void CreateEnemy() 
    {
        // Добавить, что сначала спавним в обязательных комнатах, потом в коридорах, а затем в остальных
        int indexPatch =-1;
        RoomAccessControl room = null;
        Transform spawnPoint;
        for (int i = 0; i < _countEnemy; i++) 
        {
            indexPatch = -1;
            room = null;
            // Перебираем комнаты, в которых можем заспавинть бота
            // Сначала обязательные кoмнтаы
            spawnPoint = GetNewPoint(ref room, ref indexPatch,true,false);
            // Потом коридоры
            if (spawnPoint == null)
            {
                if (_currnetCountInCoridors >= _countEnemyNoRoom) spawnPoint = null;
                else
                {
                    _currnetCountInCoridors++;
                    spawnPoint = GetFreePoint(ref indexPatch);
                }
            }            
            // Затем обычные комнтаы
            if (spawnPoint == null)
                spawnPoint = GetNewPoint(ref room, ref indexPatch, true, true);
            if (spawnPoint == null)  continue;
            GameObject newEnmy = Instantiate(_enemyPref, spawnPoint);
            EnemyAI enemyAi = newEnmy.GetComponent<EnemyAI>();
            enemyAi.SetStartParameters(room, indexPatch, this);
            _enemys.Add(enemyAi);        
        }
    }

    private Transform GetFreePoint(ref int indexPatch)
    {        
        SEnemyWayPoint newPoint;
        int newIndex;       
        while (true)
        {
            newIndex = UnityEngine.Random.Range(0, _corridorPoints.Count);
            if (_corridorPoints[newIndex].IsAvail) break;
        }
        newPoint = _corridorPoints[newIndex];
        newPoint.IsAvail = false;
        _corridorPoints[newIndex] = newPoint;
        if (indexPatch > -1)
        {
            newPoint = _corridorPoints[indexPatch];
            newPoint.IsAvail = true;
            _corridorPoints[indexPatch] = newPoint;
        }
        indexPatch = newIndex;
        return _corridorPoints[newIndex].Point;
    }

    /// <summary>
    /// Добовление маршрута из комнаты
    /// </summary>
    /// <param name="room"></param>
    /// <param name="maxCount"></param>
    /// <param name="patch"></param>
    public void AddWaypoints(RoomAccessControl room, int maxCount, List<Transform> patch, float HasChange) 
    {
        if (patch==null || patch.Count == 0) return; 
        List<SEnemyWayPoint> sEnemyWayPoints = new List<SEnemyWayPoint>();
        SEnemyWayPoint sEnemyWayPoint = new SEnemyWayPoint();
        // Если максимальное кол-во врагов не задано или больше, чем точек - берем от кол-ва точек (-1 чтобы оствалась вариативность)
        if (maxCount == 0 || maxCount>= patch.Count)  maxCount = patch.Count - 1;
        _roomData.Add(room, new Vector3(maxCount, 0,HasChange));
        foreach (Transform transform in patch) 
        {
            sEnemyWayPoint.Point = transform;
            sEnemyWayPoint.IsAvail = true;
            sEnemyWayPoints.Add(sEnemyWayPoint);
        }
        _waypoints.Add(room, sEnemyWayPoints);        
    }

    public void AddFreePoint(Transform point) 
    {
        SEnemyWayPoint newPoint;
        newPoint.Point = point;
        newPoint.IsAvail = true;
        _corridorPoints.Add(newPoint);
    }


    public Transform GetNewPoint(ref RoomAccessControl room, ref int indexRout, bool isSpawn, bool canChange)
    {
        RoomAccessControl newRoom;
        int newIndex;
        // Если это запрос от бота
        if (!isSpawn)
        {
            if (room != null)
            {
                if (_roomData[room].z==1) canChange = true;
                else canChange = false;
                // Шанс того, что бот захочет поменять комнату и он может ее поменять
                if (canChange && UnityEngine.Random.Range(0, 100) < 50 && canChange) newRoom = GetNewRoom(room, true);
                else newRoom = room;
            }
            else return GetFreePoint(ref indexRout);
        }
        // Если метод вызывается при спавне ботов
        else
        {
            newRoom = GetNewRoom(room, canChange);
            if (newRoom == null)
            {
                newIndex = -1;
                return null;
            }
        }
        while (true) 
        {
            newIndex = UnityEngine.Random.Range(0, _waypoints[newRoom].Count);
            if (_waypoints[newRoom][newIndex].IsAvail) break;
        }
        SEnemyWayPoint editWaypoint;
        // Сначала освободим занимаемое место, если оно было
        if (room != null)
        {
            editWaypoint = _waypoints[room][indexRout];
            editWaypoint.IsAvail = true;
            _waypoints[room][indexRout] = editWaypoint;
        }
        // Если бот поменял комнату
        if (room != newRoom)
        {
            if (room!=null) _roomData[room] = new Vector3(_roomData[room].x, _roomData[room].y-1, _roomData[room].z);
            _roomData[newRoom] = new Vector3(_roomData[newRoom].x, _roomData[newRoom].y + 1, _roomData[newRoom].z);
        }
        editWaypoint = _waypoints[newRoom][newIndex];
        editWaypoint.IsAvail = false;
        _waypoints[newRoom][newIndex] = editWaypoint;
        room = newRoom;
        indexRout = newIndex;
        return _waypoints[newRoom][newIndex].Point;
    }

    /// <summary>
    /// Напарвляем ботов к источнику шума
    /// </summary>
    /// <param name="pointAlaem"></param>
    /// <param name="distance"></param>
    public void AlarmAtDistance(Vector3 pointAlaem, float distance) 
    {
        foreach (EnemyAI enemyAI in _enemys)
        {
            if (Vector3.Distance(enemyAI.transform.position, pointAlaem) <= distance) enemyAI.StartAlerted(pointAlaem);
        }
    }

    public EnemyAI GetEnemyForGameObject(GameObject enemyObgect) 
    {
        foreach (EnemyAI enemy in _enemys) 
        {
            if(enemy.gameObject == enemyObgect) return enemy;
        }
        return null;
    }
}
