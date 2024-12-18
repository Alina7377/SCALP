using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private FirstPersonLook _cameraControl;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed = 18f; // Базовая скорость
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;

    private float _currentSpeed;
    private const float GRAVITY = -10f;
    private const float GROUNDED_VELOCITY = -1f;
    private bool _isGrounded;
    private Vector3 _velocity;
    private PlayerControl _control;
    private bool _isJump;
    private bool _isSneaking;

    private float originalHeight;

    private void Awake()
    {
        SetCursorSetting();
        originalHeight = _controller.height;
        _control = new PlayerControl();
        _currentSpeed = _speed;
        _control.Player.Jump.started += context => Jump();
        // Подпись на кнопки бега и приседа
        _control.Player.Run.started += context => Run();
        _control.Player.Sneak.started += context => Sneak();
        // Подпись на возвращение к хотьбе
        _control.Player.Run.canceled += context => Walk();
        _control.Player.Sneak.canceled += context => Walk();

    }

    private void Update()
    {
        //_cameraControl.Rotate(_control.Player.Look.ReadValue<Vector2>()); // Закоменнировать это, если по варианту Никиты
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
        if (!_isJump)
            _currentSpeed = _speed * 1.5f;
        //    _audioManager.PlayAudioByNum(1);
    }

    private void Walk()
    {
        _currentSpeed = _speed;
        //    _audioManager.PlayAudioByNum(0);
        if (_isSneaking)
        {
           /* _collider.height = _walkColliderHeight;
            _collider.center = Vector3.zero;
            _cameraTransform.localPosition = _camerPositionWalk;*/
            _isSneaking = false;
        }
    }

    private void Sneak()
    {
        _currentSpeed = _speed / 1.5f;
        /*_collider.height = _sneakColliderSize;
        _collider.center = _sneakColliderCenter;
        _cameraTransform.localPosition = _camerPositionSneak;*/
        _isSneaking = true;
        //   _audioManager.StopAudio();
    }

    private void Jump() 
    {
        if (IsOnTheGraund() && !_isJump)
            _isJump = true;
    }    

    private void Move(Vector2 direction) 
    {
        Vector3 moveDirection = GetMoveDirection(direction);
        HandleNormalMovement(moveDirection);
    }

    private Vector3 GetMoveDirection(Vector2 inputValue)
    {        
        Vector3 move = transform.right * inputValue.x + transform.forward * inputValue.y;
        return move * _currentSpeed * Time.deltaTime;
    }

    private bool IsOnTheGraund() 
    {
        return Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);
    }

    private void HandleNormalMovement(Vector3 moveDirection)
    {   
        if (IsOnTheGraund() && _velocity.y < 0)
        {
            _velocity.y = GROUNDED_VELOCITY;
        }

        _controller.Move(moveDirection);

        if (_isJump && IsOnTheGraund())
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * GRAVITY);
            _isJump = false;
        }

        _velocity.y += GRAVITY * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    public void SetCursorSetting()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
