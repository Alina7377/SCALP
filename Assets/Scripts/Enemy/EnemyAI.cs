using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyAI : MonoBehaviour
{
    [SerializeField, Tooltip("����� �������� �� ������.")] private float _waitTime = 2f;
    [SerializeField, Tooltip("����� ��������� �������.")] private float _alertTime = 2f;
    [SerializeField, Tooltip("����� ������.")] private float _searchTime = 5f;

    [SerializeField, Tooltip("�������� �� ����� �������.")] private float _speedPatrol;
    [SerializeField, Tooltip("�������� �� ����� ������.")] private float _speedChase;
    [SerializeField, Tooltip("�������� �� ����� ������� ��� ������.")] private float _speedAlertOrSearching;
    [SerializeField, Tooltip("�������� ��������.")] private float _speedRotate;

  //  [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _flashlight;
    [SerializeField] private AudioSource _audioOther;
    [SerializeField] private AudioSource _audioSteps;
    [SerializeField] private List<AudioClip> _soundSteps;
    [SerializeField] private List<AudioClip> _soundOther;

    private NavMeshAgent _agent;
    private AnimaorHandler _handlerAnimate;
    private RoomAccessControl _room;
    private int _currentWaypointIndex = 0;
    private EnemyManager _enemyManager;

    private bool _isRotation = false;
    private Vector3 _targetPoint;
    private float _timeToRotate = 0.3f;

    private float _currentTime;
    // ��� ��������
    public EEnemyState _oldState;
    public EEnemyState _state = EEnemyState.Patrolling; // ��� �������� �������� ��������� ���� - �� ��������� - �������
    //

    private bool _isWalk = true;

    private float _countdownTimeSearch; // ����� ������� ��� ������ ������
    private bool _isLightAlways = false;
    private float _maxTimeToNextRandomSound = 6f;
    private float _timeToNextRandomSound;
    private float _curentTimeSound;

    // ��� �������� �����������
    private float _timeTojamming;
    private float _currentTimejamming;
    private Vector3 _oldPosition;

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
        _handlerAnimate = GetComponent<AnimaorHandler>();
        StartPatrol();
        Events.Instance.OnBalckOut += LightAlways;
        // ������������� ����������� ����� �������� ��� �������� �� �����������
        _timeTojamming = Mathf.Max(_waitTime, _alertTime, _searchTime);
        _currentTimejamming = Time.time;
        _oldPosition = _agent.transform.position;
        _oldState = _state;
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
        SeyRandomReplick();
        _handlerAnimate.SetSpeed(_agent);
        CheackJamming();
    }

    /// <summary>
    ///������ �������� � ������������ �����
    /// </summary>
    private void CheackJamming() 
    {
        // �� �� ��������� �� ����������� ��� ������������� (���� �������� ���� ���������� � ����� ������)
        if (_state == EEnemyState.Chasing) return;
        if (Time.time - _currentTimejamming > _timeTojamming)
        {
            // ���� ��� �������, �� ��������� ��� � ��������������
            if (Vector3.Distance(_oldPosition,_agent.transform.position)<0.05 && _oldState==_state)
            {
                Debug.Log(gameObject.name + " �������!");
                StartPatrol(); 
            }
            // ���������� ����� ������ ������������
            _oldPosition= _agent.transform.position;
            _currentTimejamming = Time.time;
        }

    }

    private void SeyRandomReplick() 
    {
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
        if(_state == EEnemyState.Patrolling) StartPatrol();
    }

    private IEnumerator WaitAlert()
    {
        if (_state == EEnemyState.Chasing) StopCoroutine(WaitAlert());
        yield return new WaitForSeconds(_alertTime);
        if (_state == EEnemyState.Alerted) StartPatrol(); // ������������ � ��������������
    }


    /// <summary>
    /// ������ ��������� �������
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    private void PlayRandomSound(int min, int max) 
    {
        if (UnityEngine.Random.Range(0,100)<40)
            PlaySound(_audioOther, UnityEngine.Random.Range(min, max), false, false);
    }

    /// <summary>
    ///  ������� � ���� � �������� ������� �����
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <returns></returns>
    private void Rotate()
    {
        if (_state != EEnemyState.Chasing && _state != EEnemyState.Alerted)
        {
            // ������ ������� ����
            if (Time.time - _currentTime < _timeToRotate)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_targetPoint - transform.position, Vector3.up), _speedRotate * Time.deltaTime);
            }
            else
            {
                SetPathcForAgent(_targetPoint);
               // _animator.SetInteger("State", 1);
                _isRotation = false;
                _isWalk = true;
                /// ���������� �����
                PlaySound(_audioSteps,0,true,true);
            }
        }
        else _isRotation = false;
    }


    /// <summary>
    /// ����� �����
    /// </summary>
    /// 
    private void Patrol()
    {
        if (_agent.remainingDistance < 0.5f && _agent.remainingDistance > 0 && _isWalk && !_isRotation)
        {
           // _animator.SetInteger("State", 0);
            _isWalk = false;
            _audioSteps.Stop();
            StartCoroutine(WaitAtWaypoint());
        }     
    }

    /// <summary>
    /// �������� � ��������� ����
    /// </summary>
    private void Alerted()
    {
        if (_agent.remainingDistance < 0.5f && _agent.remainingDistance > 0)
        {
            _isWalk = false;
         //   _animator.SetInteger("State", 3);
            StartCoroutine(WaitAlert());
            // �����
            _audioSteps.Stop();
            PlaySound(_audioOther, 5, true, false);
        }
    }

    /// <summary>
    /// ����� ������, ����� ��� ������
    /// </summary>
    private void Searching()
    {
        if (Time.time - _countdownTimeSearch >= _searchTime)
        {
            StartPatrol();
        }
    }

    private void GoToNextWaypoint()
    {
        Vector3 target = _enemyManager.GetNewPoint(ref _room, ref _currentWaypointIndex,false,true).position;
        _isWalk = false;
       // _animator.SetInteger("State", 0);
        SetPathcForAgent(transform.position);
        _isRotation = true;
        _targetPoint = target;
        _currentTime = Time.time;
    }


    /// <summary>
    /// ��������, � ����������� �� ��������� ����
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
    /// ��������� � ����� ��������������
    /// </summary>
    private void StartPatrol()
    {
        _oldState = _state;
        _state = EEnemyState.Patrolling;
        _agent.speed = _speedPatrol;
        if (_isLightAlways)
            ActivateFlashlight(true);
        else ActivateFlashlight(false);
        _handlerAnimate.SetState(EEnemyState.Patrolling);
     //   _animator.SetInteger("State", 1);
        GoToNextWaypoint();
        // ���������� �����
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
    /// ������ ������������� ������
    /// </summary>
    public void ChasePlayer(bool isVisiblePlayer) 
    {        
        if (_state != EEnemyState.Chasing)
        {            
            StartChasing();
        }
        /// ��� ��������, ��� ���������� ������ � ���� �����. ����� �������� �� �������� ��������� ����� ������� � ������ (������ ����� ���)
        if (_state == EEnemyState.Chasing && Vector3.Distance(transform.position, GameMode.FirstPersonMovement.transform.position) < 0.6 && isVisiblePlayer)
        {
            SetPathcForAgent(gameObject.transform.position);
            transform.LookAt(new Vector3(GameMode.FirstPersonMovement.transform.position.x, transform.position.y, GameMode.FirstPersonMovement.transform.position.z));
            if (GameMode.FirstPersonMovement.IsAlive())
            {
                // ������ ������� ������ ����� ����� - ��������� � ��������
                _handlerAnimate.Attack();
                GameMode.FirstPersonMovement.Die();
                // ���������� �����
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
                  //  _animator.SetInteger("State", 0);
                    _audioSteps.Stop();
                }
        }
    }

    /// <summary>
    /// ������������� ���������. ���������� �����
    /// </summary>
    public void StartSearchingPlayer()
    {
        _oldState = _state;
        _state = EEnemyState.Searching;
        _countdownTimeSearch = Time.time;
        _agent.speed = _speedAlertOrSearching;
        ActivateFlashlight(true);
        SetPathcForAgent(transform.position);
        _handlerAnimate.SetState(_state);
       // _animator.SetInteger("State", 3);
        // ���������� �����
        _audioSteps.Stop();

    }

    public void StartAlerted(Vector3 noiseSours) 
    {
        // ���� ��� ����������, �� �� �����������
        if (_state == EEnemyState.Chasing || _state == EEnemyState.Alerted || _state==EEnemyState.WaitChasing || _state == EEnemyState.WaitAlert) return;
        _isWalk = false;
        _oldState = _state;
        _state = EEnemyState.Alerted;  
        _agent.speed = _speedAlertOrSearching;
        SetPathcForAgent(transform.position);
        _handlerAnimate.SetState(_state);
       // _animator.SetInteger("State", 4);
        _targetPoint = noiseSours;
        transform.LookAt(noiseSours);
        ActivateFlashlight(true);
        // ���������� �����
        _audioSteps.Stop();
        PlaySound(_audioOther, 2, true, false);
    }  

    public void SetStartParameters(RoomAccessControl room, int waypointIndex, EnemyManager enemyManager) 
    {
        _room = room;
        _currentWaypointIndex = waypointIndex;
        _enemyManager = enemyManager;
        // ��������� ����� ����� �� �����, ����� ������ ����������� ������ ����� ����, ��� ������������ ��� ����
       // _animator.SetInteger("State", 0);
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
        if (_state == EEnemyState.WaitChasing) return;
        SetPathcForAgent(transform.position);
        _oldState = _state;
        _state = EEnemyState.WaitChasing;
        _agent.speed = _speedChase;
        _isWalk = true;
        _handlerAnimate.SetState(_state);
       // _animator.SetInteger("State", 2);
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
            //���������� �����
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
            _handlerAnimate.SetState(_state);
        }
    }


    public void PlaySoundAtack() 
    {
        PlaySound(_audioOther, 0, true, false);
    }


}