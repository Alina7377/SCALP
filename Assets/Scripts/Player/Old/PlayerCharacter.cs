using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _speedWalk;
    [SerializeField] private float _speedRotate;
    [SerializeField] private float _speedRun;
    [SerializeField] private float _speedSneak;
    [SerializeField] private float _forceJump;
    [SerializeField] private Transform _pointGroundCheker;

    private PlayerControl _control;
  //  private AudioManager _audioManager;
    private Rigidbody _rigidbody;
    private float _currentSpeed;
    private float _walkColliderHeight;
    private float _sneakColliderSize = 1f;
    private Vector3 _sneakColliderCenter = new Vector3(0, -0.5f, 0);
    private Vector3 _camerPositionSneak  = new Vector3(0, -0.2f, -0.54f);
    private Vector3 _camerPositionWalk   = new Vector3(0,  0.6f, -0.54f);
    private CapsuleCollider _collider;
    private bool _isSneaking;

    private void Awake()
    {
        _control = new PlayerControl();
        SetCursorSetting();
        //_audioManager = GetComponent<AudioManager>();
        _rigidbody = GetComponent<Rigidbody>();
        _currentSpeed = _speedWalk;
        _collider = GetComponent<CapsuleCollider>();
        _walkColliderHeight = _collider.height;
        _control.Player.Jump.started += context => Jump();
        // Подпись на кнопки бега и приседа
        _control.Player.Run.started += context => Run();
        _control.Player.Sneak.started += context => Sneak();
        // Подпись на возвращение к хотьбе
        _control.Player.Run.canceled += context => Walk();
        _control.Player.Sneak.canceled += context => Walk();
    }

    /// <summary>
    /// Для плавного поворота
    /// </summary>
    private void Update()
    {
        Rotate(_control.Player.Look.ReadValue<Vector2>());      
    }

    /// <summary>
    /// Для корректного расчета физики при передвижении 
    /// </summary>
    private void FixedUpdate()
    {
        Move(_control.Player.Move.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        _control.Enable();
    }

    private void OnDisable()
    {
        _control.Disable();
    }

    private void Run() 
    {
        if (IsOnTheGraund())
            _currentSpeed = _speedRun;
    //    _audioManager.PlayAudioByNum(1);
    }

    private void Walk() 
    { 
        _currentSpeed = _speedWalk;
    //    _audioManager.PlayAudioByNum(0);
        if (_isSneaking)
        {
            _collider.height = _walkColliderHeight;
            _collider.center = Vector3.zero;
            _cameraTransform.localPosition = _camerPositionWalk;
            _isSneaking = false;
        }
    }

    private void Sneak() 
    {
        _currentSpeed = _speedSneak;
        _collider.height = _sneakColliderSize;
        _collider.center = _sneakColliderCenter;
        _cameraTransform.localPosition = _camerPositionSneak;
        _isSneaking = true;
     //   _audioManager.StopAudio();
    }

    private bool IsOnTheGraund() 
    {
        Collider[] colliders = Physics.OverlapSphere(_pointGroundCheker.position, 0.1f);
        foreach (Collider collider in colliders)
        {
            if (collider.tag != "Rooms" && collider.tag != "Player") return true;
        }
        return false;
    }

    private void Jump() 
    {
        
        if (IsOnTheGraund())
        {
            Vector3 jumpForse = Vector3.zero;
            jumpForse.y = _forceJump;
            _rigidbody.AddForce(jumpForse);
        }
        
    }

    private void Move(Vector2 direction) 
    {
        //   if (direction == Vector2.zero || _isSneaking) _audioManager.StopAudio();
        //    else _audioManager.PalyAudioIfNotPlaying();
        Vector3 velosity = (transform.forward * direction.y + transform.right * direction.x) * _currentSpeed;
        velosity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = velosity;
    }

    private void Rotate(Vector2 rotate)
    {
        float mauseX = rotate.x * _speedRotate * Time.deltaTime;
        float mauseY = rotate.y * _speedRotate * Time.deltaTime;

        float rotateX = Mathf.Clamp(-mauseY, -90, 90);

        Quaternion rotation = _cameraTransform.localRotation;       
        rotation *= Quaternion.Euler(rotateX, 0, 0);
        transform.Rotate(0, mauseX, 0);
        if (rotation.x < 0.55 && rotation.x > -0.55)
            _cameraTransform.localRotation = rotation;
        
    }

    public void SetCursorSetting() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DiactivateControl() 
    {
        _control.Disable();
    }

    public void ActivateControl() 
    {
        _control.Enable();
    }
    public Vector3 GetCameraPosition()
    {
        return _cameraTransform.position;
    }
}
