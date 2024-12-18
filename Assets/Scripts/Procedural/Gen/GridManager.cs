using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class RoomType
{
    public string typeName; // Название типа комнаты (например, "Охранная", "Генераторная")
    public List<GameObject> prefabs; // Список префабов для этого типа комнаты
    public int roomSize; // Размер комнаты (например, 3 для комнаты 3x3)
}

public class GridManager : MonoBehaviour
{
    private List<Light> allLights = new List<Light>();

    public bool _isTraining = false;

    [Header("Door Outline Materials")]
    public Material outlineWhiteMaterial;
    public Material outlineBlackMaterial;
    public Material outlineRedMaterial;
    public Material outlineGreenMaterial;
    public Material outlineBlueMaterial;

    [Header("Tertiary Paths Settings")]
    public int minTertiaryPaths = 1; // Минимальное количество третичных путей
    public int maxTertiaryPaths = 3; // Максимальное количество третичных путей
    public int minTertiaryPathLength = 2; // Минимальная длина пути
    public int maxTertiaryPathLength = 4; // Максимальная длина пути
    [Range(0f, 1f)]
    public float tertiaryPathTurnChance = 0.5f; // Шанс поворота на каждом шаге

    public GameObject cellPrefab;
    public int gridWidth = 20;
    public int gridHeight = 20;
    public Material blackMaterial;
    public Material whiteMaterial;
    public float cellSize = 1f;
    public int borderOffset = 2;

    // Список типов комнат с префабами для каждого типа
    public List<RoomType> roomTypes = new List<RoomType>();

    public GameObject startRoomPrefab;
    public GameObject finishRoomPrefab;

    private GridCell[,] cells;

    private GameObject startRoomInstance; // Ссылка на стартовую комнату
    private GameObject finishRoomInstance; // Ссылка на финишную комнату

    public Material pathMaterial; // Материал для основного пути
    public Material secondaryPathMaterial; // Материал для вторичных путей
    public Material tertiaryPathMaterial; // Материал для третичных путей

    private HashSet<GridCell> connectedPathCells = new HashSet<GridCell>(); // Множество клеток соединенных с основным путем

    public List<GameObject> corridorPrefabs; // Список всех префабов коридоров

    Dictionary<GridCell, bool[]> cellPassages = new Dictionary<GridCell, bool[]>(); // Словарь для хранения сторон проходов для каждой клетки

    private int[,] distanceToRoom; // Массив для хранения расстояний до ближайшей комнаты

    public int minDistanceToRoom = 1;
    public int maxDistanceToRoom = 2;
    public int maxPlacementAttempts = 20;

    [Header("Interactive Items")]
    public GameObject redCardPrefab;
    public GameObject greenCardPrefab;
    public GameObject blueCardPrefab;
    public GameObject portableBatteryPrefab;
    public GameObject blackCardPrefab;
    public GameObject stunGunPrefab;

    [Header("Collectebel parameters")]
    public int _countCollectebelItems;
    public SOCollections _reportSO;
    public SOCollections _audioSO;

    private EnemyManager _enemyManager;
    private List<SpawnCollectebel> _spawnConteiners = new List<SpawnCollectebel>();

    // Глобальные списки для имен предметов и комнат, где они размещены
    private List<string> itemNames = new List<string>
    {
    "Красный ключ",       // соответствует redCardPrefab
    "Зелёный ключ",       // соответствует greenCardPrefab
    "Синий ключ",         // соответствует blueCardPrefab
    "Чёрный ключ",        // соответствует blackCardPrefab
    "Электрошокер",       // соответствует stunGunPrefab
    "Переносной аккумулятор" // соответствует portableBatteryPrefab
    };

    private List<GameObject> itemsPlacedRooms = new List<GameObject>();
    private List<string> itemsPlaced = new List<string>();
    private NavMeshSurface _navMeshSurface;

    private struct LightState
    {
        public float intensity;
        public float range;
        public Color color;
        public bool isActive;
    }

    private Dictionary<Light, LightState> originalLightStates = new Dictionary<Light, LightState>();
    private Dictionary<Light, Coroutine> activePulsations = new Dictionary<Light, Coroutine>();

    // Новые списки для отслеживания созданных объектов
    private List<GameObject> instantiatedCells = new List<GameObject>();
    private List<GameObject> instantiatedRooms = new List<GameObject>();
    private List<GameObject> instantiatedCorridors = new List<GameObject>();
    private List<GameObject> instantiatedItems = new List<GameObject>();


    void Start()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();

        if (!_isTraining)
        {
            InitializeLevel();
        }

        FindAllLights();
        StartCoroutine(FlickerLights());
    }

    void InitializeLevel()
    {
        _enemyManager = GetComponent<EnemyManager>();
        
        GenerateGrid();
        PlaceRooms();
        ComputeDistanceToRooms();
        FindAndMarkPath();
        BuildSecondaryPaths();
        BuildTertiaryPaths();
        PlaceCorridors();
        List<GameObject> itemRooms = PlaceInteractiveItems();
        AssignAccessLevelsToRooms(itemRooms);
        DisablePowerInRooms(itemRooms);
        GeneratePathReport();
        SpawnCollectebelsObject();
        UpdateDoorMaterials();
        StartCoroutine(WaitToBake());
        Events.Instance.OnInteractGenerator += BakeSurfce;
        Events.Instance.OnBalckOut += StartBlackOut;
    }

    private IEnumerator WaitToBake()
    {
        yield return new WaitForSeconds(2);
        BakeSurfce();
        _enemyManager.CreateEnemy();
    }

    public void ResetLevel()
    {
        // Отписываемся от событий
        Events.Instance.OnBalckOut -= StartBlackOut;
        Events.Instance.OnInteractGenerator -= BakeSurfce;

        // Останавливаем все корутины
        StopAllCoroutines();

        // Уничтожаем созданные клетки
        foreach (GameObject cell in instantiatedCells)
        {
            if (cell != null)
                Destroy(cell);
        }
        instantiatedCells.Clear();

        // Уничтожаем созданные комнаты
        foreach (GameObject room in instantiatedRooms)
        {
            if (room != null)
                Destroy(room);
        }
        instantiatedRooms.Clear();

        // Уничтожаем созданные коридоры
        foreach (GameObject corridor in instantiatedCorridors)
        {
            if (corridor != null)
                Destroy(corridor);
        }
        instantiatedCorridors.Clear();

        // Уничтожаем созданные предметы
        foreach (GameObject item in instantiatedItems)
        {
            if (item != null)
                Destroy(item);
        }
        instantiatedItems.Clear();

        // Уничтожаем врагов через EnemyManager
        if (_enemyManager != null)
        {
            _enemyManager.Reset();
        }

        // Очищаем другие структуры данных
        connectedPathCells.Clear();
        cellPassages.Clear();
        itemsPlaced.Clear();
        itemsPlacedRooms.Clear();
        allLights.Clear();
        originalLightStates.Clear();
        activePulsations.Clear();

        // **Добавляем очистку списка _spawnConteiners**
        _spawnConteiners.Clear(); // <-- Добавлено здесь

        // Сбрасываем переменные
        startRoomInstance = null;
        finishRoomInstance = null;

        // Реинициализируем уровень
        InitializeLevel();
    }

    private void OnDisable()
    {
        Events.Instance.OnBalckOut -= StartBlackOut;
        Events.Instance.OnInteractGenerator -=BakeSurfce;
    }
    private void StartBlackOut(bool state)
    {
        // Запускаем отключение света
        if (state) ModifyLightSources();
    }

    // Перезапекаем карту для агентов
    private void BakeSurfce() 
    {
        Debug.Log("Запуск перезапечки");
        _navMeshSurface.BuildNavMesh();
    }

    private void SpawnCollectebelsObject()
    {
        List<string> collectebelItems = new List<string>();
        string currentTag = "";
        int indexRoom,indexItem;
        List<int> ExclRooms = new List<int>();
        Transform spawnPoint;
        List<string> _reports = new List<string>();
        List<string> _audios = new List<string>();
        List<string> _currentList = new List<string>();

        // Получаем список не открытых предметов
        collectebelItems = LocalizationManager.Instance.GetTagList("f", true);
        // Если все объекты открыты, то получаем их
        if (collectebelItems.Count == 0) collectebelItems = LocalizationManager.Instance.GetTagList("f", false);
        // Если кол-во неоткрытых объектов меньше заданного, то устанавливаем это число
        _reports = GetListForTag("Reports.", collectebelItems);
        _audios = GetListForTag("Audio.", collectebelItems);
        if (collectebelItems.Count<_countCollectebelItems)
            _countCollectebelItems = collectebelItems.Count;
        for (int i = 0; i < _countCollectebelItems; i++) 
        {    
            /// Ищем комнату и точку спавна
            while (true) 
            {
                // Если все комнаты заняты, то нет смысла продолжать
                if (ExclRooms.Count == _spawnConteiners.Count)
                {
                   // Debug.Log("Размещение коллекционных предметов остановлено - все комнаты исключены");
                    return;
                }
                indexRoom = UnityEngine.Random.Range(0,_spawnConteiners.Count);
                if (IsExclRoom(ExclRooms, indexRoom))
                {
                  //  Debug.Log("Попытка размещения объекта " + currentTag + " в исключенной комнате " + _spawnConteiners[index].gameObject.name);
                    continue;
                }
                if (_spawnConteiners[indexRoom].CanSpawn())
                {
                    spawnPoint = _spawnConteiners[indexRoom].GetPointSpawnObject();
                    break;
                }
                else
                {
                    Debug.Log("Комната " + _spawnConteiners[indexRoom].gameObject.name + " исключена");
                    ExclRooms.Add(indexRoom);
                }
            }
            // Определяем, какой список использваоть
            if (_spawnConteiners[indexRoom].GetLastSpawnType() == CollectibleType.None)
                if (UnityEngine.Random.Range(0, 2) > 0)
                    _currentList = _reports;
                else _currentList = _audios;
            else 
            if ((_spawnConteiners[indexRoom].GetLastSpawnType() == CollectibleType.Reports || _audios.Count==0) && _reports.Count>0)
                _currentList = _reports;
            else _currentList = _audios;
            if (_currentList.Count == 0)
                if (_reports.Count > 0) _currentList = _reports;
                else if (_audios.Count > 0) _currentList = _audios;
                else return;
            /// Оперделяем, что спавним и исключаем спавн одинаковых объектов
            indexItem = UnityEngine.Random.Range(0, _currentList.Count);
            currentTag = _currentList[indexItem];
            // Чистим список
            _currentList.RemoveAt(indexItem); 
            /// Наконец, спавним самих объектов
            if (currentTag.Contains("Reports."))
            {
                GameObject collectebelObject = GameObject.Instantiate(_reportSO.GetPrefab(), spawnPoint);
                ReportCollectebel collec = collectebelObject.GetComponent<ReportCollectebel>();
                collec.CollectibleType = CollectibleType.Reports;
                collec.Tag = currentTag;
                collec.Image = _reportSO.GetImageForTag(currentTag);
                _spawnConteiners[indexRoom].SetLastSpawnType(CollectibleType.Reports);
                _reports = _currentList;
                Debug.Log("Объект " + currentTag + " типа " + CollectibleType.Reports + " размещен в комнате " + _spawnConteiners[indexRoom].gameObject.name);
                continue;
            }           
            if (currentTag.Contains("Audio."))
            {
                GameObject collectebelObject = GameObject.Instantiate(_audioSO.GetPrefab(), spawnPoint);
                ReportCollectebel collec = collectebelObject.GetComponent<ReportCollectebel>();
                collec.CollectibleType = CollectibleType.AudioRecords;
                collec.Tag = currentTag;
                collec.Clip = _audioSO.GetAudioForTag(currentTag);
                _spawnConteiners[indexRoom].SetLastSpawnType(CollectibleType.AudioRecords);
                _audios = _currentList;
                Debug.Log("Объект " + currentTag + " типа " + CollectibleType.AudioRecords + " размещен в комнате " + _spawnConteiners[indexRoom].gameObject.name);
                continue;
            }
        }
    }

    private List<string> GetListForTag(string tag,List<string> list) 
    {
        List<string> resultList = new List<string>();
        foreach (string item in list)
        {
            if(item.Contains(tag)) resultList.Add(item);
        }
        return resultList;
    }

    private bool IsExclRoom(List<int> ExclRooms,int index) 
    {
        foreach(int iRoom in ExclRooms)
            if (iRoom == index) return true;
        return false;
    }

    void GenerateGrid()
    {
        cells = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newCell.transform.parent = transform;

                instantiatedCells.Add(newCell); // Добавляем в список

                GridCell cellScript = newCell.GetComponent<GridCell>();
                cellScript.gridX = x;
                cellScript.gridY = y;

                cells[x, y] = cellScript;

                Renderer renderer = newCell.GetComponent<Renderer>();
                renderer.material = (x + y) % 2 == 0 ? blackMaterial : whiteMaterial;
            }
        }
    }

    void PlaceRooms()
    {
        // Размещаем стартовую комнату в левом верхнем квадранте
        PlaceSpecificRoom(startRoomPrefab, 3, 0, gridHeight / 2, gridWidth / 2, gridHeight);

        // Размещаем финишную комнату в правом нижнем квадранте
        PlaceSpecificRoom(finishRoomPrefab, 3, gridWidth / 2, 0, gridWidth, gridHeight / 2);

        // Размещаем по одной комнате каждого типа, используя префабы и размеры, добавленные в инспекторе
        foreach (RoomType roomType in roomTypes)
        {
            if (roomType.prefabs.Count > 0)
            {
                // Выбираем случайный префаб из списка префабов для данного типа комнаты
                GameObject selectedPrefab = roomType.prefabs[Random.Range(0, roomType.prefabs.Count)];

                // Используем roomSize из RoomType
                PlaceRoom(selectedPrefab, roomType.roomSize);
            }
        }
    }

    void PlaceRoom(GameObject roomPrefab, int roomSize)
    {
        // Список потенциальных позиций
        List<Vector2Int> potentialPositions = new List<Vector2Int>();

        // Ищем подходящие позиции
        for (int x = borderOffset; x <= gridWidth - roomSize - borderOffset; x++)
        {
            for (int y = borderOffset; y <= gridHeight - roomSize - borderOffset; y++)
            {
                if (IsNearOccupiedRoom(x, y, roomSize, minDistanceToRoom, maxDistanceToRoom) && CanPlaceRoom(x, y, roomSize))
                {
                    potentialPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // Если нашли позиции, размещаем комнату
        if (potentialPositions.Count > 0)
        {
            Vector2Int chosenPosition = potentialPositions[Random.Range(0, potentialPositions.Count)];
            Vector3 roomPosition = new Vector3(chosenPosition.x + roomSize / 2f - 0.5f, 0, chosenPosition.y + roomSize / 2f - 0.5f);

            GameObject newRoom = Instantiate(roomPrefab, roomPosition, Quaternion.identity);

            // Устанавливаем ссылку на GridManager в RoomAccessControl
            RoomAccessControl roomAccess = newRoom.GetComponent<RoomAccessControl>();
            if (roomAccess != null)
            {
                roomAccess.Initialize(this);
            }

            // После создания комнаты добавляем её в список
            instantiatedRooms.Add(newRoom); // <-- Вставлено сюда

            MarkCellsAsOccupied(chosenPosition.x, chosenPosition.y, roomSize, newRoom);
        }
        else
        {
            // Попытка разместить комнату без учета расстояния
            List<Vector2Int> fallbackPositions = new List<Vector2Int>();

            for (int x = borderOffset; x <= gridWidth - roomSize - borderOffset; x++)
            {
                for (int y = borderOffset; y <= gridHeight - roomSize - borderOffset; y++)
                {
                    if (CanPlaceRoom(x, y, roomSize))
                    {
                        fallbackPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (fallbackPositions.Count > 0)
            {
                Vector2Int chosenPosition = fallbackPositions[Random.Range(0, fallbackPositions.Count)];
                Vector3 roomPosition = new Vector3(chosenPosition.x + roomSize / 2f - 0.5f, 0, chosenPosition.y + roomSize / 2f - 0.5f);

                GameObject newRoom = Instantiate(roomPrefab, roomPosition, Quaternion.identity);

                // Устанавливаем ссылку на GridManager в RoomAccessControl
                RoomAccessControl roomAccess = newRoom.GetComponent<RoomAccessControl>();
                if (roomAccess != null)
                {
                    roomAccess.Initialize(this);
                }

                // После создания комнаты добавляем её в список
                instantiatedRooms.Add(newRoom); // <-- Вставлено сюда

                MarkCellsAsOccupied(chosenPosition.x, chosenPosition.y, roomSize, newRoom);
            }
            else
            {
                Debug.LogError($"Не удалось разместить комнату '{roomPrefab.name}' даже без учета расстояния после {maxPlacementAttempts} попыток.");
            }
        }
    }


    bool IsNearOccupiedRoom(int startX, int startY, int roomSize, int minDistance, int maxDistance)
    {
        for (int x = startX - maxDistance; x <= startX + roomSize + maxDistance; x++)
        {
            for (int y = startY - maxDistance; y <= startY + roomSize + maxDistance; y++)
            {
                // Пропускаем клетки вне сетки
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;

                // Пропускаем клетки, которые слишком близко
                if (Mathf.Abs(x - startX) < minDistance && Mathf.Abs(y - startY) < minDistance)
                    continue;

                // Если клетка занята комнатой и находится в пределах максимального расстояния
                if (cells[x, y].isOccupied && Mathf.Abs(x - startX) <= maxDistance && Mathf.Abs(y - startY) <= maxDistance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void PlaceSpecificRoom(GameObject roomPrefab, int roomSize, int minX, int minY, int maxX, int maxY)
    {
        // Учитываем отступ от границ
        minX = Mathf.Max(minX + borderOffset, borderOffset);
        minY = Mathf.Max(minY + borderOffset, borderOffset);
        maxX = Mathf.Min(maxX - borderOffset, gridWidth - roomSize - borderOffset);
        maxY = Mathf.Min(maxY - borderOffset, gridHeight - roomSize - borderOffset);

        // Проверка корректности диапазонов
        if (minX > maxX || minY > maxY)
        {
            Debug.LogWarning($"Неверный диапазон размещения для комнаты '{roomPrefab.name}'. minX: {minX}, maxX: {maxX}, minY: {minY}, maxY: {maxY}");
            return;
        }

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            int x = Random.Range(minX, maxX + 1);
            int y = Random.Range(minY, maxY + 1);

            float randomRotation = 90f * Random.Range(0, 4);

            if (CanPlaceRoom(x, y, roomSize))
            {
                Vector3 roomPosition = new Vector3(x + roomSize / 2f - 0.5f, 0, y + roomSize / 2f - 0.5f);
                GameObject newRoom = Instantiate(roomPrefab, roomPosition, Quaternion.Euler(0, randomRotation, 0));

                // Устанавливаем ссылку на GridManager в RoomAccessControl
                RoomAccessControl roomAccess = newRoom.GetComponent<RoomAccessControl>();
                if (roomAccess != null)
                {
                    roomAccess.Initialize(this);
                }

                // После создания комнаты добавляем её в список
                instantiatedRooms.Add(newRoom); // <-- Вставлено сюда

                MarkCellsAsOccupied(x, y, roomSize, newRoom);

                // Сохраняем ссылки на стартовую и финишную комнаты
                if (roomPrefab == startRoomPrefab)
                {
                    startRoomInstance = newRoom;
                }
                else if (roomPrefab == finishRoomPrefab)
                {
                    finishRoomInstance = newRoom;
                }

                break;
            }
        }

        // Логирование, если не удалось разместить комнату
        if (startRoomInstance == null && roomPrefab == startRoomPrefab ||
            finishRoomInstance == null && roomPrefab == finishRoomPrefab)
        {
            Debug.LogWarning($"Не удалось разместить стартовую/финишную комнату '{roomPrefab.name}' после {maxPlacementAttempts} попыток.");
        }
    }

    bool CanPlaceRoom(int startX, int startY, int roomSize)
    {
        // Проверяем пространство вокруг комнаты
        int checkStartX = startX - 1;
        int checkStartY = startY - 1;
        int checkEndX = startX + roomSize;
        int checkEndY = startY + roomSize;

        for (int x = checkStartX; x <= checkEndX; x++)
        {
            for (int y = checkStartY; y <= checkEndY; y++)
            {
                // Проверка, что клетка в пределах сетки
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    // Если клетка занята, то нельзя разместить комнату
                    if (cells[x, y].isOccupied)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    void MarkCellsAsOccupied(int startX, int startY, int roomSize, GameObject room)
    {
        // Получаем RoomAccessControl из комнаты
        RoomAccessControl roomAccess = room.GetComponent<RoomAccessControl>();

        // Передаем ссылку на массив точек перемещения ботов деспетчеру ботов
        EnemyRoute enemyRoute;
        if (room.TryGetComponent<EnemyRoute>(out enemyRoute))
        {            
            float canExit;
            if (enemyRoute.HasExit)
                canExit = 1;
            else canExit = 0;
            _enemyManager.AddWaypoints(roomAccess, enemyRoute.CountMaxEnemyInRoom, enemyRoute.GetWayPoints(), canExit);
        }

        //Получаем ссылки на контейнеры с точками дял спавна коллбоксов
        SpawnCollectebel spawnCollectebel;
        if (room.TryGetComponent<SpawnCollectebel>(out spawnCollectebel))
            _spawnConteiners.Add(spawnCollectebel);

        // Отмечаем клетки как занятые
        for (int x = startX; x < startX + roomSize; x++)
        {
            for (int y = startY; y < startY + roomSize; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    cells[x, y].isOccupied = true;
                    cells[x, y].occupyingRoom = room;
                    cells[x, y].occupancyType = OccupancyType.Room;
                    cells[x, y].roomAccessControl = roomAccess; // Сохраняем ссылку на RoomAccessControl
                }
            }
        }

        // Проверяем дверные проемы
        foreach (Transform child in room.transform)
        {
            // Проверяем, есть ли на модуле компонент Doorway
            Doorway doorway = child.GetComponent<Doorway>();
            if (doorway != null)
            {
                // Получаем мировую позицию модуля
                Vector3 moduleWorldPos = child.position;

                // Преобразуем мировую позицию в координаты сетки
                int gridX = Mathf.RoundToInt(moduleWorldPos.x);
                int gridY = Mathf.RoundToInt(moduleWorldPos.z); // Используем 'z' для 'y' на сетке

                // Проверяем, что координаты в пределах сетки
                if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                {
                    cells[gridX, gridY].isDoorway = true;
                }
                else
                {
                    Debug.LogWarning($"Модуль с дверью находится вне сетки на позиции ({gridX}, {gridY})");
                }
            }
        }
    }

    List<GridCell> GetDoorwayCells(GameObject room)
    {
        List<GridCell> doorwayCells = new List<GridCell>();

        foreach (Transform child in room.transform)
        {
            Doorway doorway = child.GetComponent<Doorway>();
            if (doorway != null)
            {
                Vector3 moduleWorldPos = child.position;
                int gridX = Mathf.RoundToInt(moduleWorldPos.x);
                int gridY = Mathf.RoundToInt(moduleWorldPos.z);

                if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                {
                    GridCell cell = cells[gridX, gridY];
                    if (cell != null)
                    {
                        doorwayCells.Add(cell);
                    }
                }
            }
        }

        return doorwayCells;
    }

    // Метод для получения свободной соседней клетки
    GridCell GetFreeAdjacentCell(GridCell doorwayCell)
    {
        int x = doorwayCell.gridX;
        int y = doorwayCell.gridY;

        List<GridCell> neighbors = new List<GridCell>();

        // Соседи по четырем направлениям
        if (x > 0) neighbors.Add(cells[x - 1, y]);
        if (x < gridWidth - 1) neighbors.Add(cells[x + 1, y]);
        if (y > 0) neighbors.Add(cells[x, y - 1]);
        if (y < gridHeight - 1) neighbors.Add(cells[x, y + 1]);

        foreach (GridCell neighbor in neighbors)
        {
            if (!neighbor.isOccupied)
            {
                return neighbor;
            }
        }

        // Если свободная соседняя клетка не найдена
        return null;
    }

    void ComputeDistanceToRooms()
    {
        distanceToRoom = new int[gridWidth, gridHeight];

        // Инициализируем все расстояния как максимально возможные
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                distanceToRoom[x, y] = int.MaxValue;
            }
        }

        Queue<GridCell> queue = new Queue<GridCell>();

        // Добавляем все клетки, занятые комнатами, в очередь и устанавливаем расстояние 0
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (cells[x, y].occupancyType == OccupancyType.Room)
                {
                    distanceToRoom[x, y] = 0;
                    queue.Enqueue(cells[x, y]);
                }
            }
        }

        // Выполняем BFS для вычисления расстояний
        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();
            int currentDistance = distanceToRoom[current.gridX, current.gridY];

            foreach (GridCell neighbor in GetNeighbors(current))
            {
                if (distanceToRoom[neighbor.gridX, neighbor.gridY] > currentDistance + 1)
                {
                    distanceToRoom[neighbor.gridX, neighbor.gridY] = currentDistance + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    void FindAndMarkPath()
    {
        if (startRoomInstance == null || finishRoomInstance == null)
        {
            Debug.LogError("Стартовая или финишная комната не найдены!");
            return;
        }

        // Получаем дверные клетки
        List<GridCell> startDoorways = GetDoorwayCells(startRoomInstance);
        List<GridCell> finishDoorways = GetDoorwayCells(finishRoomInstance);

        if (startDoorways.Count == 0 || finishDoorways.Count == 0)
        {
            Debug.LogError("Не найдены дверные проемы в стартовой или финишной комнате!");
            return;
        }

        // Находим стартовую и конечную клетки
        GridCell startCell = null;
        GridCell endCell = null;

        foreach (GridCell doorwayCell in startDoorways)
        {
            startCell = GetFreeAdjacentCell(doorwayCell);
            if (startCell != null)
            {
                break;
            }
        }

        foreach (GridCell doorwayCell in finishDoorways)
        {
            endCell = GetFreeAdjacentCell(doorwayCell);
            if (endCell != null)
            {
                break;
            }
        }

        if (startCell == null || endCell == null)
        {
            Debug.LogError("Не удалось найти свободные клетки рядом с дверными проемами для начала и конца пути.");
            return;
        }

        // Инициализируем поиск пути
        Pathfinding pathfinding = new Pathfinding(cells, gridWidth, gridHeight);

        // Сначала пробуем с ограничением на расстояние
        System.Func<GridCell, bool> isWalkableMainPath = (cell) =>
            !cell.isOccupied && distanceToRoom[cell.gridX, cell.gridY] <= 3;

        List<GridCell> mainPath = pathfinding.FindPath(startCell, endCell, isWalkableMainPath);

        if (mainPath == null)
        {
            // Если путь не найден, пробуем без ограничения
            System.Func<GridCell, bool> isWalkableFallback = (cell) => !cell.isOccupied;

            mainPath = pathfinding.FindPath(startCell, endCell, isWalkableFallback);

            if (mainPath == null)
            {
                Debug.LogError("Путь не найден даже без ограничения на расстояние!");
                return;
            }
        }

        // Меняем материал клеток на пути и отмечаем их как занятые
        foreach (GridCell cell in mainPath)
        {
            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer != null && pathMaterial != null)
            {
                renderer.material = pathMaterial;
            }
            cell.isOccupied = true;
            cell.occupancyType = OccupancyType.MainPath;
            cell.isConnectedToMainPath = true;
            connectedPathCells.Add(cell); // Добавляем клетку в множество соединенных с основным путем
        }

        // Строим вторичные пути от других комнат
        BuildSecondaryPaths();
    }

    void BuildSecondaryPaths()
    {
        // Получаем список всех комнат, кроме стартовой и финишной
        HashSet<GameObject> otherRooms = new HashSet<GameObject>();
        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupyingRoom != null && cell.occupancyType == OccupancyType.Room)
            {
                if (cell.occupyingRoom != startRoomInstance && cell.occupyingRoom != finishRoomInstance)
                {
                    otherRooms.Add(cell.occupyingRoom);
                }
            }
        }

        // Для каждой комнаты строим вторичный путь
        foreach (GameObject room in otherRooms)
        {
            List<GridCell> doorways = GetDoorwayCells(room);
            foreach (GridCell doorwayCell in doorways)
            {
                GridCell freeCell = GetFreeAdjacentCell(doorwayCell);
                if (freeCell == null)
                {
                    continue;
                }

                // Находим ближайшую клетку, соединенную с основным путем
                GridCell closestConnectedCell = FindClosestConnectedCell(freeCell);
                if (closestConnectedCell == null)
                {
                    continue;
                }

                // Определяем функцию, которая определяет, можно ли пройти через клетку
                System.Func<GridCell, bool> isWalkableSecondaryPath = (cell) =>
                {
                    // Клетка проходима, если она не занята или если она уже соединена с основным путем
                    return !cell.isOccupied || (cell.isConnectedToMainPath);
                };

                // Ищем путь от свободной клетки до ближайшей клетки, соединенной с основным путем
                Pathfinding pathfinding = new Pathfinding(cells, gridWidth, gridHeight);
                List<GridCell> secondaryPath = pathfinding.FindPath(freeCell, closestConnectedCell, isWalkableSecondaryPath);

                if (secondaryPath == null)
                {
                    continue;
                }

                // Меняем материал клеток на пути и отмечаем их как занятые
                foreach (GridCell cell in secondaryPath)
                {
                    if (!cell.isConnectedToMainPath)
                    {
                        Renderer renderer = cell.GetComponent<Renderer>();
                        if (renderer != null && secondaryPathMaterial != null)
                        {
                            renderer.material = secondaryPathMaterial;
                        }
                        cell.isOccupied = true;
                        cell.occupancyType = OccupancyType.SecondaryPath;
                        cell.isConnectedToMainPath = true; // Отмечаем клетку как соединенную с основным путем
                        connectedPathCells.Add(cell); // Добавляем клетку в множество соединенных с основным путем
                    }
                }
            }
        }
    }

    GridCell FindClosestConnectedCell(GridCell startCell)
    {
        GridCell closestCell = null;
        float minDistance = float.MaxValue;

        foreach (GridCell cell in connectedPathCells)
        {
            float distance = Mathf.Abs(cell.gridX - startCell.gridX) + Mathf.Abs(cell.gridY - startCell.gridY);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell;
    }

    // Метод для получения соседей
    private List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        int x = cell.gridX;
        int y = cell.gridY;

        // Соседи по четырем направлениям
        if (x > 0) neighbors.Add(cells[x - 1, y]);
        if (x < gridWidth - 1) neighbors.Add(cells[x + 1, y]);
        if (y > 0) neighbors.Add(cells[x, y - 1]);
        if (y < gridHeight - 1) neighbors.Add(cells[x, y + 1]);

        return neighbors;
    }

    void DetermineCellPassages()
    {
        foreach (GridCell cell in cells)
        {
            if (cell != null && (cell.occupancyType == OccupancyType.MainPath ||
                                 cell.occupancyType == OccupancyType.SecondaryPath ||
                                 cell.occupancyType == OccupancyType.TertiaryPath))
            {
                bool[] passages = new bool[4]; // [0]: Up, [1]: Right, [2]: Down, [3]: Left

                int x = cell.gridX;
                int y = cell.gridY;

                // Верхняя клетка
                if (y < gridHeight - 1)
                {
                    GridCell neighbor = cells[x, y + 1];
                    if (neighbor != null && (
                        neighbor.occupancyType == OccupancyType.MainPath ||
                        neighbor.occupancyType == OccupancyType.SecondaryPath ||
                        neighbor.occupancyType == OccupancyType.TertiaryPath ||
                        (neighbor.occupancyType == OccupancyType.Room && neighbor.isDoorway)
                        ))
                    {
                        passages[0] = true; // Up is a passage
                    }
                }

                // Правая клетка
                if (x < gridWidth - 1)
                {
                    GridCell neighbor = cells[x + 1, y];
                    if (neighbor != null && (
                        neighbor.occupancyType == OccupancyType.MainPath ||
                        neighbor.occupancyType == OccupancyType.SecondaryPath ||
                        neighbor.occupancyType == OccupancyType.TertiaryPath ||
                        (neighbor.occupancyType == OccupancyType.Room && neighbor.isDoorway)
                        ))
                    {
                        passages[1] = true; // Right is a passage
                    }
                }

                // Нижняя клетка
                if (y > 0)
                {
                    GridCell neighbor = cells[x, y - 1];
                    if (neighbor != null && (
                        neighbor.occupancyType == OccupancyType.MainPath ||
                        neighbor.occupancyType == OccupancyType.SecondaryPath ||
                        neighbor.occupancyType == OccupancyType.TertiaryPath ||
                        (neighbor.occupancyType == OccupancyType.Room && neighbor.isDoorway)
                        ))
                    {
                        passages[2] = true; // Down is a passage
                    }
                }

                // Левая клетка
                if (x > 0)
                {
                    GridCell neighbor = cells[x - 1, y];
                    if (neighbor != null && (
                        neighbor.occupancyType == OccupancyType.MainPath ||
                        neighbor.occupancyType == OccupancyType.SecondaryPath ||
                        neighbor.occupancyType == OccupancyType.TertiaryPath ||
                        (neighbor.occupancyType == OccupancyType.Room && neighbor.isDoorway)
                        ))
                    {
                        passages[3] = true; // Left is a passage
                    }
                }

                cellPassages[cell] = passages;
            }
        }
    }

    void PlaceCorridors()
    {
        // Создаем объект "Corridors" и добавляем компонент RoomAccessControl
        GameObject corridorsParent = new GameObject("Corridors");
        instantiatedCorridors.Add(corridorsParent); // Добавляем родительский объект в список
        RoomAccessControl accessControl = corridorsParent.AddComponent<RoomAccessControl>();

        // Устанавливаем параметры доступа и питания для коридора
        accessControl.RequiredAccessLevel = AccessCardColor.None;
        accessControl.HasPower = true;

        DetermineCellPassages();

        foreach (var kvp in cellPassages)
        {
            GridCell cell = kvp.Key;
            bool[] passages = kvp.Value;

            // Ищем подходящий префаб
            GameObject prefabToInstantiate = null;
            int rotation = 0;

            foreach (GameObject prefab in corridorPrefabs)
            {
                CorridorPiece corridor = prefab.GetComponent<CorridorPiece>();

                if (corridor == null)
                    continue;

                // Сохраняем оригинальные значения
                bool originalUp = corridor.up;
                bool originalRight = corridor.right;
                bool originalDown = corridor.down;
                bool originalLeft = corridor.left;

                for (int i = 0; i < 4; i++)
                {
                    bool matches = corridor.up == passages[0] &&
                                   corridor.right == passages[1] &&
                                   corridor.down == passages[2] &&
                                   corridor.left == passages[3];

                    if (matches)
                    {
                        prefabToInstantiate = prefab;
                        rotation = i * 90;
                        break;
                    }

                    // Поворачиваем проходы для следующей проверки
                    RotatePassages(corridor);
                }

                // Восстанавливаем исходные значения префаба
                corridor.up = originalUp;
                corridor.right = originalRight;
                corridor.down = originalDown;
                corridor.left = originalLeft;

                if (prefabToInstantiate != null)
                {                   
                    break;
                }
            }

            if (prefabToInstantiate != null)
            {
                Vector3 position = new Vector3(cell.gridX, 0, cell.gridY);
                Quaternion rot = Quaternion.Euler(0, rotation, 0);
                GameObject passage = Instantiate(prefabToInstantiate, position, rot);
                passage.transform.parent = corridorsParent.transform;
                instantiatedCorridors.Add(passage); // Добавляем коридор в список

                CorridorPiece corridor = passage.GetComponent<CorridorPiece>();
                if (corridor.EnemyPoint != null)
                    _enemyManager.AddFreePoint(corridor.EnemyPoint);
            }
            else
            {
                Debug.LogError($"Не удалось найти подходящий префаб для клетки ({cell.gridX}, {cell.gridY}) с проходами [{passages[0]}, {passages[1]}, {passages[2]}, {passages[3]}]");
            }
        }
    }

    void RotatePassages(CorridorPiece corridor)
    {
        bool tempUp = corridor.up;
        corridor.up = corridor.left;
        corridor.left = corridor.down;
        corridor.down = corridor.right;
        corridor.right = tempUp;
    }

    void BuildTertiaryPaths()
    {
        int tertiaryPathsCount = Random.Range(minTertiaryPaths, maxTertiaryPaths + 1);
        int attempts = 0;
        int maxAttempts = tertiaryPathsCount * 10; // Максимальное количество попыток

        List<GridCell> connectedCells = new List<GridCell>(connectedPathCells);

        int pathsCreated = 0;

        while (pathsCreated < tertiaryPathsCount && attempts < maxAttempts)
        {
            attempts++;

            // Выбираем случайную клетку из уже соединенных
            GridCell startCell = connectedCells[Random.Range(0, connectedCells.Count)];

            // Определяем случайную длину пути
            int pathLength = Random.Range(minTertiaryPathLength, maxTertiaryPathLength + 1);

            List<GridCell> path = new List<GridCell>();
            GridCell currentCell = startCell;
            Vector2Int currentDirection = GetRandomDirection();
            Vector2Int previousDirection = currentDirection;

            for (int step = 0; step < pathLength; step++)
            {
                // Решаем, делать ли поворот
                if (Random.value < tertiaryPathTurnChance)
                {
                    // Получаем новое направление, исключая обратное
                    currentDirection = GetRandomDirectionExcept(-previousDirection);
                }

                int newX = currentCell.gridX + currentDirection.x;
                int newY = currentCell.gridY + currentDirection.y;

                if (newX < 0 || newX >= gridWidth || newY < 0 || newY >= gridHeight)
                {
                    break;
                }

                GridCell nextCell = cells[newX, newY];

                if (nextCell.isOccupied)
                {
                    // Пытаемся найти другое направление
                    List<Vector2Int> availableDirections = new List<Vector2Int>
                    {
                        new Vector2Int(0, 1),  // Вверх
                        new Vector2Int(1, 0),  // Вправо
                        new Vector2Int(0, -1), // Вниз
                        new Vector2Int(-1, 0)  // Влево
                    };

                    // Исключаем обратное и текущее направления
                    availableDirections.RemoveAll(dir => dir == -previousDirection || dir == currentDirection);

                    bool found = false;
                    ShuffleList(availableDirections);

                    foreach (var dir in availableDirections)
                    {
                        int tempX = currentCell.gridX + dir.x;
                        int tempY = currentCell.gridY + dir.y;

                        if (tempX >= 0 && tempX < gridWidth && tempY >= 0 && tempY < gridHeight)
                        {
                            GridCell tempCell = cells[tempX, tempY];
                            if (!tempCell.isOccupied)
                            {
                                currentDirection = dir;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        // Нет доступных направлений, прекращаем путь
                        break;
                    }
                    else
                    {
                        // Обновляем координаты с новым направлением
                        newX = currentCell.gridX + currentDirection.x;
                        newY = currentCell.gridY + currentDirection.y;
                        nextCell = cells[newX, newY];
                    }
                }

                // Добавляем клетку в путь
                path.Add(nextCell);
                previousDirection = currentDirection;
                currentCell = nextCell;
            }

            if (path.Count >= minTertiaryPathLength)
            {
                // Отмечаем клетки как занятые
                foreach (GridCell cell in path)
                {
                    Renderer renderer = cell.GetComponent<Renderer>();
                    if (renderer != null && tertiaryPathMaterial != null)
                    {
                        renderer.material = tertiaryPathMaterial;
                    }
                    cell.isOccupied = true;
                    cell.occupancyType = OccupancyType.TertiaryPath;
                    cell.isConnectedToMainPath = true;
                    connectedPathCells.Add(cell);
                    connectedCells.Add(cell);
                }

                pathsCreated++;
            }
            else
            {
                // Путь слишком короткий, пробуем снова
                continue;
            }
        }

        if (pathsCreated < tertiaryPathsCount)
        {
            Debug.LogWarning($"Не удалось создать требуемое количество третичных путей. Создано {pathsCreated} из {tertiaryPathsCount}");
        }
    }

    Vector2Int GetRandomDirection()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // Вверх
            new Vector2Int(1, 0),  // Вправо
            new Vector2Int(0, -1), // Вниз
            new Vector2Int(-1, 0)  // Влево
        };

        return directions[Random.Range(0, directions.Count)];
    }

    Vector2Int GetRandomDirectionExcept(Vector2Int excludeDirection)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // Вверх
            new Vector2Int(1, 0),  // Вправо
            new Vector2Int(0, -1), // Вниз
            new Vector2Int(-1, 0)  // Влево
        };

        directions.RemoveAll(dir => dir == excludeDirection);

        return directions[Random.Range(0, directions.Count)];
    }

    // Метод для перемешивания списка
    void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, n);
            T temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }

    void AssignAccessLevelsToRooms(List<GameObject> excludeRooms)
    {
        // Get all rooms except the start room and excluded rooms
        HashSet<GameObject> allRooms = new HashSet<GameObject>();
        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupyingRoom != null && cell.occupancyType == OccupancyType.Room)
            {
                if (cell.occupyingRoom != startRoomInstance && !excludeRooms.Contains(cell.occupyingRoom))
                {
                    allRooms.Add(cell.occupyingRoom);
                }
            }
        }

        // Include the finish room
        if (!allRooms.Contains(finishRoomInstance))
        {
            allRooms.Add(finishRoomInstance);
        }

        // Check that we have enough rooms
        if (allRooms.Count < 3)
        {
            Debug.LogError("Недостаточно комнат для назначения карт доступа.");
            return;
        }

        // Convert to list for easier handling
        List<GameObject> roomList = new List<GameObject>(allRooms);

        // Shuffle the list
        ShuffleList(roomList);

        // Assign access levels
        AccessCardColor[] accessLevels = new AccessCardColor[]
        {
            AccessCardColor.Red,
            AccessCardColor.Green,
            AccessCardColor.Blue
        };

        // Ensure that the finish room is assigned the highest access level
        AccessCardColor finishRoomAccessLevel = accessLevels[accessLevels.Length - 1];
        RoomAccessControl finishRoomAccessControl = finishRoomInstance.GetComponent<RoomAccessControl>();
        if (finishRoomAccessControl != null)
        {
            finishRoomAccessControl.RequiredAccessLevel = finishRoomAccessLevel;
            Debug.Log($"Комната {finishRoomInstance.name} закрыта на {finishRoomAccessControl.RequiredAccessLevel} ключ.");
        }
        else
        {
            Debug.LogWarning($"Финишная комната {finishRoomInstance.name} не имеет RoomAccessControl.");
        }

        int assignedAccessLevels = 1; // We have already assigned one access level to the finish room

        for (int i = 0; assignedAccessLevels < accessLevels.Length && i < roomList.Count; i++)
        {
            GameObject room = roomList[i];

            // Skip the finish room
            if (room == finishRoomInstance)
                continue;

            RoomAccessControl accessControl = room.GetComponent<RoomAccessControl>();
            if (accessControl != null)
            {
                accessControl.RequiredAccessLevel = accessLevels[assignedAccessLevels - 1];
                Debug.Log($"Комната {room.name} закрыта на {accessControl.RequiredAccessLevel} ключ.");
                assignedAccessLevels++;
            }
            else
            {
                Debug.LogWarning($"Комната {room.name} не имеет RoomAccessControl.");
            }
        }
    }


    void DisablePowerInRooms(List<GameObject> excludeRooms)
    {
        HashSet<GameObject> allRooms = new HashSet<GameObject>();
        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupyingRoom != null && cell.occupancyType == OccupancyType.Room)
            {
                if (cell.occupyingRoom != startRoomInstance && !excludeRooms.Contains(cell.occupyingRoom))
                {
                    allRooms.Add(cell.occupyingRoom);
                }
            }
        }

        if (!allRooms.Contains(finishRoomInstance))
        {
            allRooms.Add(finishRoomInstance);
        }

        // Удаляем условие, связанное с уровнем доступа
        List<GameObject> availableRooms = new List<GameObject>(allRooms);

        if (availableRooms.Count < 3)
        {
            Debug.LogError("Недостаточно комнат для отключения питания.");
            return;
        }

        ShuffleList(availableRooms);

        int roomsToDisablePower = 3;

        RoomAccessControl finishRoomAccessControl = finishRoomInstance.GetComponent<RoomAccessControl>();
        if (finishRoomAccessControl != null && finishRoomAccessControl.HasPower)
        {
            finishRoomAccessControl.HasPower = false;
            finishRoomAccessControl.NoNav();
            Debug.Log($"Питание отключено в комнате {finishRoomInstance.name}.");
            roomsToDisablePower--;
        }

        for (int i = 0; roomsToDisablePower > 0 && i < availableRooms.Count; i++)
        {
            GameObject room = availableRooms[i];

            // Пропускаем финишную комнату и комнаты с генератором
            if (room == finishRoomInstance || room.name.Contains("Gen"))
                continue;

            RoomAccessControl accessControl = room.GetComponent<RoomAccessControl>();
            if (accessControl != null)
            {
                accessControl.HasPower = false;
                // В отключенных комнатах блокируем дверь
                accessControl.NoNav();
                Debug.Log($"Питание отключено в комнате {room.name}.");
                roomsToDisablePower--;
            }
            else
            {
                Debug.LogWarning($"Комната {room.name} не имеет RoomAccessControl.");
            }
        }
    }

    List<GameObject> PlaceInteractiveItems()
    {
        List<GameObject> placedItemRooms = new List<GameObject>();

        // Получаем все комнаты, кроме стартовой и финишной
        HashSet<GameObject> uniqueRooms = new HashSet<GameObject>();
        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupancyType == OccupancyType.Room && cell.occupyingRoom != null)
            {
                if (cell.occupyingRoom != startRoomInstance && cell.occupyingRoom != finishRoomInstance)
                {
                    uniqueRooms.Add(cell.occupyingRoom);
                }
            }
        }

        List<GameObject> rooms = new List<GameObject>(uniqueRooms);

        // Список предметов для размещения
        List<GameObject> itemsToPlace = new List<GameObject>
    {
        redCardPrefab,
        greenCardPrefab,
        blueCardPrefab,
        blackCardPrefab,
        stunGunPrefab,
        portableBatteryPrefab
    };

        // Проверяем, что списки имеют одинаковую длину
        if (itemsToPlace.Count != itemNames.Count)
        {
            Debug.LogError("Количество предметов и названий предметов не совпадает.");
            return placedItemRooms;
        }

        // Собираем список комнат, имеющих хотя бы одну точку спауна
        List<GameObject> roomsWithSpawnPoints = new List<GameObject>();
        foreach (GameObject room in rooms)
        {
            // Получаем все точки спауна в комнате
            Transform[] childTransforms = room.GetComponentsInChildren<Transform>();
            bool hasSpawnPoint = false;
            foreach (Transform t in childTransforms)
            {
                if (t.name == "ObjectSpawnPoint")
                {
                    hasSpawnPoint = true;
                    break;
                }
            }
            if (hasSpawnPoint)
            {
                roomsWithSpawnPoints.Add(room);
            }
        }

        // Проверяем, достаточно ли комнат
        if (roomsWithSpawnPoints.Count < itemsToPlace.Count)
        {
            Debug.LogError("Недостаточно комнат с точками спауна для размещения предметов.");
            return placedItemRooms;
        }

        // Перемешиваем комнаты с точками спауна
        ShuffleList(roomsWithSpawnPoints);

        // Размещаем предметы
        for (int i = 0; i < itemsToPlace.Count; i++)
        {
            GameObject item = itemsToPlace[i];
            GameObject room = roomsWithSpawnPoints[i];

            // Получаем все точки спауна в комнате
            List<Transform> objectSpawnPoints = new List<Transform>();
            foreach (Transform t in room.GetComponentsInChildren<Transform>())
            {
                if (t.name == "ObjectSpawnPoint")
                {
                    objectSpawnPoints.Add(t);
                }
            }

            if (objectSpawnPoints.Count > 0)
            {
                // Выбираем случайную точку спауна
                Transform spawnPoint = objectSpawnPoints[Random.Range(0, objectSpawnPoints.Count)];

                // Размещаем предмет в выбранной точке спауна
                GameObject instantiatedItem = Instantiate(item, spawnPoint.position, Quaternion.identity);

                instantiatedItems.Add(instantiatedItem); // <-- Добавляем предмет в список

                placedItemRooms.Add(room);
                itemsPlacedRooms.Add(room);
                itemsPlaced.Add(itemNames[i]);
                Debug.Log($"{itemNames[i]} размещён в комнате {room.name}.");
            }
            else
            {
                Debug.LogWarning($"В комнате {room.name} не найдено ни одной ObjectSpawnPoint.");
            }
        }

        return placedItemRooms;
    }

    void GeneratePathReport()
    {
        // Collect information about rooms
        List<string> disabledPowerRooms = new List<string>();
        List<string> lockedRoomsDescriptions = new List<string>();
        HashSet<GameObject> processedRooms = new HashSet<GameObject>();

        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupancyType == OccupancyType.Room)
            {
                RoomAccessControl accessControl = cell.roomAccessControl;
                GameObject room = cell.occupyingRoom;

                if (accessControl != null && !processedRooms.Contains(room))
                {
                    if (!accessControl.HasPower)
                    {
                        disabledPowerRooms.Add(room.name);
                    }

                    if (accessControl.RequiredAccessLevel != AccessCardColor.None)
                    {
                        lockedRoomsDescriptions.Add($"{room.name} закрыта на {accessControl.RequiredAccessLevel} ключ");
                    }

                    processedRooms.Add(room);
                }
            }
        }

        // Log the information
        Debug.Log($"Игрок появился в комнате {startRoomInstance.name}.");

        if (disabledPowerRooms.Count > 0)
        {
            Debug.Log($"Комнаты с отключенным питанием: {string.Join(", ", disabledPowerRooms)}.");
        }

        if (lockedRoomsDescriptions.Count > 0)
        {
            Debug.Log($"Комнаты, закрытые на ключ: {string.Join(", ", lockedRoomsDescriptions)}.");
        }

        if (itemsPlaced.Count > 0)
        {
            // List of items and their rooms
            List<string> itemPaths = new List<string>();
            for (int i = 0; i < itemsPlaced.Count; i++)
            {
                string itemName = itemsPlaced[i];
                string roomName = itemsPlacedRooms[i].name;
                itemPaths.Add($"{itemName} появился в комнате {roomName}");
            }

            Debug.Log(string.Join(", ", itemPaths) + ".");

            // Construct a possible player path
            Debug.Log($"Путь игрока: от {startRoomInstance.name} к {itemsPlacedRooms[0].name} за {itemsPlaced[0]}, затем к {itemsPlacedRooms[1].name} за {itemsPlaced[1]}, и т.д., чтобы открыть комнату {finishRoomInstance.name}.");
        }
        else
        {
            Debug.Log("Предметы не были размещены.");
        }
    }
    private void ModifyLightSources()
    {
        // Находим все источники света
        Light[] lights = FindObjectsOfType<Light>();
        List<Light> directionalLights = new List<Light>();

        foreach (Light light in lights)
        {
            if (light.name.StartsWith("Directional Light"))
            {
                directionalLights.Add(light);

                // Сохраняем начальные параметры света
                originalLightStates[light] = new LightState
                {
                    intensity = light.intensity,
                    range = light.range,
                    color = light.color,
                    isActive = light.enabled
                };
            }
        }

        // Перебираем найденные источники и изменяем параметры
        foreach (Light light in directionalLights)
        {
            // Изменяем параметры для аварийного освещения
            light.intensity *= 0.5f;
            light.color = new Color(0.8f, 0.2f, 0.2f); // Менее яркий красный
            light.range *= 0.75f; // Уменьшаем радиус освещения

            // Запускаем пульсацию с задержкой
            float delay = Random.Range(0f, 1f); // Задержка до 1 секунды
            Coroutine pulsation = StartCoroutine(PulseLight(light, delay));
            activePulsations[light] = pulsation;
        }

        // Запускаем восстановление параметров через 30 секунд
        StartCoroutine(RestoreLightSourcesAfterDelay(30f));
    }

    private IEnumerator PulseLight(Light light, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;
        float duration = 3f; // Полный цикл пульсации
        float randomFactor = Random.Range(0.8f, 1.2f); // Уникальная амплитуда для каждого источника света

        while (true)
        {
            elapsedTime += Time.deltaTime;
            float phase = Mathf.Sin((elapsedTime / duration) * Mathf.PI * 2); // Плавная синусоида

            // Интенсивность пульсирует от -80% до +80% от текущего значения
            light.intensity = originalLightStates[light].intensity * 0.5f * (1f + randomFactor * phase);
            light.range = originalLightStates[light].range * (1f + 0.4f * phase * randomFactor);

            yield return null;
        }
    }

    private IEnumerator RestoreLightSourcesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var pair in activePulsations)
        {
            Light light = pair.Key;
            Coroutine pulsation = pair.Value;

            // Останавливаем пульсацию
            if (pulsation != null)
            {
                StopCoroutine(pulsation);
            }
        }

        activePulsations.Clear();

        foreach (var pair in originalLightStates)
        {
            Light light = pair.Key;
            LightState state = pair.Value;

            // Восстанавливаем параметры
            light.intensity = state.intensity;
            light.color = state.color;
            light.range = state.range;
            light.enabled = state.isActive;
        }
        // Вызываем событие включения света
        Events.Instance.ChangeStateBlackOut(false);
        Debug.Log("Световые параметры восстановлены.");
    }
    private void FindAllLights()
    {
        // Находим все источники света
        Light[] lights = FindObjectsOfType<Light>();

        foreach (Light light in lights)
        {
            if (light.name.StartsWith("Directional Light"))
            {
                allLights.Add(light);
            }
        }
    }

    private IEnumerator FlickerLights()
    {
        while (true)
        {
            // Длительность мерцания
            float flickerDuration = 4f;
            float intervalBetweenFlickers = Random.Range(6f, 9f);

            // Мерцание в течение 4 секунд
            float elapsed = 0f;
            while (elapsed < flickerDuration)
            {
                foreach (Light light in allLights)
                {
                    if (Random.Range(0f, 1f) < 0.10f) // 10% шанс для каждой лампы
                    {
                        StartCoroutine(SingleLightFlicker(light));
                    }
                }

                elapsed += 1f;
                yield return new WaitForSeconds(1f); // Проверяем каждую секунду
            }

            // Пауза перед следующим запуском
            yield return new WaitForSeconds(intervalBetweenFlickers);
        }
    }

    private IEnumerator SingleLightFlicker(Light light)
    {
        if (light == null) yield break;

        int flickerCount = Random.Range(2, 8); // Случайное количество мерцаний (2–5 раз)

        for (int i = 0; i < flickerCount; i++)
        {
            light.enabled = false;
            yield return new WaitForSeconds(0.05f); // Быстрое выключение
            light.enabled = true;
            yield return new WaitForSeconds(0.05f); // Быстрое включение
        }
    }

    public Material GetMaterial(AccessCardColor cardColor) 
    {
        switch (cardColor)
        {
            case AccessCardColor.None:
                return outlineWhiteMaterial;
            case AccessCardColor.Red:
                return outlineRedMaterial;
            case AccessCardColor.Green:
                return outlineGreenMaterial;
            case AccessCardColor.Blue:
                return outlineBlueMaterial;
            default:
                return outlineWhiteMaterial;
        }
    }

    public void UpdateDoorMaterials()
    {
        if (_isTraining) return;
        // Iterate over all cells to find rooms
        HashSet<GameObject> roomsProcessed = new HashSet<GameObject>();
        
        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.occupancyType == OccupancyType.Room && cell.occupyingRoom != null)
            {
                GameObject room = cell.occupyingRoom;

                // Ensure we process each room only once
                if (roomsProcessed.Contains(room))
                    continue;

                roomsProcessed.Add(room);

                // Get the RoomAccessControl component
                RoomAccessControl roomAccess = room.GetComponent<RoomAccessControl>();
                if (roomAccess == null)
                {
                    Debug.LogWarning($"Комната {room.name} не имеет компонента RoomAccessControl.");
                    continue;
                }

                // Determine the material based on access level and power status
                Material outlineMaterial = outlineWhiteMaterial; // Default to white

                if (!roomAccess.HasPower)
                {
                    outlineMaterial = outlineBlackMaterial;
                }
                else
                {
                    outlineMaterial = GetMaterial(roomAccess.RequiredAccessLevel);                    
                }

                // Iterate over door objects in the room
                foreach (Transform child in room.transform)
                {
                    if (child.name.StartsWith("DoorPref"))
                    {
                        Transform doorPref = child;

                        Transform door = doorPref.Find("Door");
                        if (door == null)
                        {
                            Debug.LogWarning($"В {doorPref.name} в комнате {room.name} не найден объект Door.");
                            continue;
                        }

                        Transform door_2 = door.Find("Door_2");
                        if (door_2 == null)
                        {
                            Debug.LogWarning($"В Door в {doorPref.name} в комнате {room.name} не найден объект Door_2.");
                            continue;
                        }

                        Transform codeLock = door_2.Find("CodeLock");
                        if (codeLock == null)
                        {
                            Debug.LogWarning($"В Door_2 в {doorPref.name} в комнате {room.name} не найден объект CodeLock.");
                            continue;
                        }

                        Renderer renderer = codeLock.GetComponent<Renderer>();
                        if (renderer == null)
                        {
                            Debug.LogWarning($"У объекта CodeLock в комнате {room.name} отсутствует компонент Renderer.");
                            continue;
                        }

                        // Replace the second material (element1)
                        Material[] materials = renderer.materials;

                        if (materials.Length < 2)
                        {
                            Debug.LogWarning($"У объекта CodeLock в комнате {room.name} недостаточно материалов.");
                            continue;
                        }

                        materials[1] = outlineMaterial;
                        renderer.materials = materials;

                        Debug.Log($"Дверь {doorPref.name} в комнате {room.name} обновлена с материалом {outlineMaterial.name}.");
                    }
                }
            }
        }
    }
}