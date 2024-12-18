using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyAI : MonoBehaviour
{
    [SerializeField, Tooltip("Время ожидания на точках.")] private float _waitTime = 2f;
    [SerializeField, Tooltip("Время состояния тревоги.")] private float _alertTime = 2f;
    [SerializeField, Tooltip("Время поиска.")] private float _searchTime = 5f;

    [SerializeField, Tooltip("Скорость во время патруля.")] private float _speedPatrol;
    [SerializeField, Tooltip("Скорость во время погони.")] private float _speedChase;
    [SerializeField, Tooltip("Скорость во время тревоги или поиска.")] private float _speedAlertOrSearching;
    [SerializeField, Tooltip("Скорость поворота.")] private float _speedRotate;

    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _flashlight;
    [SerializeField] private AudioSource _audioOther;
    [SerializeField] private AudioSource _audioSteps;
    [SerializeField] private List<AudioClip> _soundSteps;
    [SerializeField] private List<AudioClip> _soundOther;

    private NavMeshAgent _agent;
    private RoomAccessControl _room;
    private int _currentWaypointIndex = 0;
    private EnemyManager _enemyManager;

    private bool _isRotation = false;
    private Vector3 _targetPoint;
    private float _timeToRotate = 0.3f;

    private float _currentTime;
   
    private EEnemyState _state = EEnemyState.Patrolling; // Для хранение текущего состояния бота - по умолчанию - патруль
    private bool _isWalk = true;
    private MeshRenderer _meshRenderer;
    private float _countdownTimeSearch; // Время отсчета для поиска игрока
    private bool _isLightAlways = false;
    private float _maxTimeToNextRandomSound = 6f;
    private float _timeToNextRandomSound;
    private float _curentTimeSound;

    private void SetPathcForAgent(Vector3 patch) 
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        _agent.CalculatePath(patch, navMeshPath);
        if (navMeshPath.status == NavMeshPathStatus.PathComplete) 
        {
            _agent.SetDestination(patch);
        }
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _meshRenderer = GetComponent<MeshRenderer>();
        StartPatrol();
        Events.Instance.OnBalckOut += LightAlways;
    }

    private void OnDisable()
    {
        Events.Instance.OnBalckOut -= LightAlways;
        SetPathcForAgent(transform.position);
    }

    private void Update()
    {
        if (_isRotation) Rotate();
        CheckingState();
        if (_state == EEnemyState.Patrolling &&
            Time.time - _curentTimeSound >= _timeToNextRandomSound)  
        {
            PlayRandomSound(6, _soundOther.Count);
            _curentTimeSound = Time.time;
            _timeToNextRandomSound = UnityEngine.Random.Range(2, _maxTimeToNextRandomSound);
        }
    }
    
    private IEnumerator WaitAtWaypoint()
    {
        if (_state == EEnemyState.Chasing) StopCoroutine(WaitAtWaypoint());
        yield return new WaitForSeconds(_waitTime);
        if(_state == EEnemyState.Patrolling) GoToNextWaypoint();
    }

    private IEnumerator WaitAlert()
    {
        if (_state == EEnemyState.Chasing) StopCoroutine(WaitAlert());
        yield return new WaitForSeconds(_alertTime);
        if (_state == EEnemyState.Alerted) StartPatrol(); // Возвращаемся к патрулированию
    }

    private void PlayRandomSound(int min, int max) 
    {
        if (UnityEngine.Random.Range(0,100)<40)
            PlaySound(_audioOther, UnityEngine.Random.Range(min, max), false, false);
    }

    /// <summary>
    ///  Поворот к цели и передача целевой точки
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <returns></returns>
    private void Rotate()
    {
        if (_state != EEnemyState.Chasing && _state != EEnemyState.Alerted)
        {
            // Плавно вращаем моба
            if (Time.time - _currentTime < _timeToRotate)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_targetPoint - transform.position, Vector3.up), _speedRotate * Time.deltaTime);
            }
            else
            {
                SetPathcForAgent(_targetPoint);
                _animator.SetInteger("State", 1);
                _isRotation = false;
                _isWalk = true;
                /// Управление аудио
                PlaySound(_audioSteps,0,true,true);
            }
        }
        else _isRotation = false;
    }


    /// <summary>
    /// Обход точек
    /// </summary>
    /// 
    private void Patrol()
    {
        if (_agent.remainingDistance < 0.5f && _agent.remainingDistance > 0 && _isWalk && !_isRotation)
        {
            _animator.SetInteger("State", 0);
            _isWalk = false;
            _audioSteps.Stop();
            StartCoroutine(WaitAtWaypoint());
        }        
    }

    /// <summary>
    /// Движение к источнику шума
    /// </summary>
    private void Alerted()
    {
        if (_agent.remainingDistance < 0.5f && _agent.remainingDistance > 0)
        {
            _isWalk = false;
            _animator.SetInteger("State", 3);
            StartCoroutine(WaitAlert());
            // Аудио
            _audioSteps.Stop();
            PlaySound(_audioOther, 5, true, false);
        }
        else Debug.Log("Находится в состянии тервоги");
    }

    /// <summary>
    /// Поиск игрока, после его побега
    /// </summary>
    private void Searching()
    {
        if (Time.time - _countdownTimeSearch >= _searchTime)
        {
            Debug.Log("Заканчиваем поиск");
            StartPatrol();
        }
        else Debug.Log("Находится в состянии поиска");
    }

    private void GoToNextWaypoint()
    {
        Vector3 target = _enemyManager.GetNewPoint(ref _room, ref _currentWaypointIndex,false,true).position;
        _isWalk = false;
        _animator.SetInteger("State", 0);
        SetPathcForAgent(transform.position);
        _isRotation = true;
        _targetPoint = target;
        _currentTime = Time.time;
    }


    /// <summary>
    /// Действия, в зависимости от состояния бота
    /// </summary>
    private void CheckingState() 
    {
        switch (_state)
        {
            case EEnemyState.Patrolling:
                Patrol();
                break;
            case EEnemyState.Alerted:
                Alerted();
                break;                
            case EEnemyState.Searching:
                Searching();
                break;
            default:
                break;
        }
    }    

    /// <summary>
    /// Переходим в режим патрулирования
    /// </summary>
    private void StartPatrol()
    {
        if (_state == EEnemyState.Alerted) 
            Debug.Log("Переход в патрулирование после тревоги");
        Debug.Log("Переходим в патрулирование");
        _state = EEnemyState.Patrolling;
        _agent.speed = _speedPatrol;
        if (_isLightAlways)
            ActivateFlashlight(true);
        else ActivateFlashlight(false);
        _animator.SetInteger("State", 1);
        GoToNextWaypoint();
        // Управление Аудио
        _audioOther.Stop();
        PlaySound(_audioSteps, 0, true, true);
    }

    private void ActivateFlashlight(bool activate)
    {
        if (_flashlight.activeSelf!= activate)
            _audioOther.PlayOneShot(_soundOther[4]);
        if(_flashlight!=null)
            _flashlight.SetActive(activate);
    }

    private void PlaySound(AudioSource source, int indexSound, bool replay, bool loop)
    {
        if (source.isPlaying && !replay) return;
        if (source == _audioSteps && indexSound < _soundSteps.Count)
            source.clip = _soundSteps[indexSound];
        else
        if (source == _audioOther && indexSound < _soundOther.Count)
            source.clip = _soundOther[indexSound];
        else return;
        source.loop = loop;
        source.Play();
    }



    /// <summary>
    /// Начало преследования игрока
    /// </summary>
    public void ChasePlayer() 
    {        
        if (_state != EEnemyState.Chasing)
        {            
            StartChasing();
        }
        if (_state == EEnemyState.Chasing && Vector3.Distance(transform.position, GameMode.FirstPersonMovement.transform.position) < 0.6)
        {
            SetPathcForAgent(gameObject.transform.position);
            transform.LookAt(new Vector3(GameMode.FirstPersonMovement.transform.position.x, transform.position.y, GameMode.FirstPersonMovement.transform.position.z));
            if (GameMode.FirstPersonMovement.IsAlive())
            {

                _animator.SetBool("Found", true);
                GameMode.FirstPersonMovement.Die();
                // Управление аудио
                _audioSteps.Stop();
            }
            else
            {
                _animator.SetInteger("State", 0);
                _audioSteps.Stop();
            }

        }
        else
        {
            if (_state == EEnemyState.Chasing)
                if (GameMode.FirstPersonMovement.IsAlive())
                {
                    if (_agent.pathStatus == NavMeshPathStatus.PathComplete)
                    SetPathcForAgent(GameMode.FirstPersonMovement.transform.position);
                }
                else
                {
     
                    _animator.SetInteger("State", 0);
                    _audioSteps.Stop();
                }
        }
    }

    /// <summary>
    /// Перследование закончено. Начинается поиск
    /// </summary>
    public void StartSearchingPlayer()
    {
        _state = EEnemyState.Searching;
        _countdownTimeSearch = Time.time;
        _agent.speed = _speedAlertOrSearching;
        ActivateFlashlight(true);
        SetPathcForAgent(transform.position);
        _animator.SetInteger("State", 3);
        // Управление аудио
        _audioSteps.Stop();

    }

    public void StartAlerted(Vector3 noiseSours) 
    {
        // Если бот преследует, то не отвлекается
        if (_state == EEnemyState.Chasing || _state == EEnemyState.Alerted || _state==EEnemyState.WaitChasing || _state == EEnemyState.WaitAlert) return;
        _isWalk = false;
        _state = EEnemyState.Alerted;  
        _agent.speed = _speedAlertOrSearching;
        SetPathcForAgent(transform.position);
        _animator.SetInteger("State", 4);
        _targetPoint = noiseSours;
        transform.LookAt(noiseSours);
        ActivateFlashlight(true);
        // Управление аудио
        _audioSteps.Stop();
        PlaySound(_audioOther, 2, true, false);
    }  

    public void SetStartParameters(RoomAccessControl room, int waypointIndex, EnemyManager enemyManager) 
    {
        _room = room;
        _currentWaypointIndex = waypointIndex;
        _enemyManager = enemyManager;
        // Некоторое время стоит на месте, чтобы начать действовать только после того, как заспавняться все боты
        _animator.SetInteger("State", 0);
        ActivateFlashlight(false);
        StartCoroutine(WaitAtWaypoint());
    }

    public void LightAlways(bool state) 
    {
        _isLightAlways = state;
        if (_state== EEnemyState.Patrolling)
            _flashlight.SetActive(state);
    }

    public void StartChasing() 
    {
        SetPathcForAgent(transform.position);
        _state = EEnemyState.WaitChasing;
        _agent.speed = _speedChase;
        _isWalk = true;
        _animator.SetInteger("State", 2);
        ActivateFlashlight(true);
        _audioSteps.Stop();
        PlaySound(_audioOther, 2, true, false);
        //_targetPoint = _agent.destination;
    }


    public void GoAlertTarget() 
    {
        if (_state == EEnemyState.Alerted)
        {
            _agent.SetDestination(_targetPoint);
            // _animator.SetInteger("State", 1);
            _isWalk = true;
            //Управление аудио
            PlaySound(_audioSteps, 2, true, true);
        }
    }

    public void GoChasing()
    {      
        if (_state == EEnemyState.WaitChasing)
        {
            _audioOther.PlayOneShot(_soundOther[3]);
            PlaySound(_audioSteps, 1, true, true);
            PlaySound(_audioOther, 1, true, true);
            _state = EEnemyState.Chasing;
        }
    }


    public void PlaySoundAtack() 
    {
        PlaySound(_audioOther, 0, true, false);
    }


}