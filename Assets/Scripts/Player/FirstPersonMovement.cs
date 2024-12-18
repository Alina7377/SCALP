using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 18f; // Базовая скорость
    public float jumpHeight = 3f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    [Header("Настройки звука")]
    [SerializeField] private float _radiusStandartShere;
    [SerializeField] private List<AudioClip> _sounds;
    [SerializeField] private AudioTrigger _audioCollision;

    private const float GRAVITY = -10f;
    private const float GROUNDED_VELOCITY = -1f;
    private bool isGrounded;
    private Vector3 velocity;
    private PlayerControl _control;
    private float _currentSpeed;

    private float originalHeight;
    private bool _isAlive = true;
    private AudioSource _audioSource;
    private bool _isChangeSpeed = false;
    private bool _isJump = false;
    public bool _isBlockControl = true;

    void Awake()
    {
        originalHeight = controller.height;
        _control = new PlayerControl();
        Walk();
        // Подпись на кнопки бега и приседа
        _control.Player.Run.started += context => Run();
        _control.Player.Sneak.started += context => Sneak();
        // Подпись на возвращение к хотьбе
        _control.Player.Run.canceled += context => Walk();
        _control.Player.Sneak.canceled += context => Walk();
        GameMode.FirstPersonMovement = this;
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _control.Enable();
    }

    private void OnDisable()
    {
        _control.Disable();
    }

    private void Sneak()
    {
        _currentSpeed = speed / 1.5f; // Приседание
        controller.height = originalHeight / 2;
        _audioCollision.gameObject.SetActive(true);
        _audioCollision.SetColliderRadius(_radiusStandartShere / 1.5f);
        _audioCollision.gameObject.SetActive(false);
        _isChangeSpeed = true;
    }

    private void Walk()
    {
        _currentSpeed = speed;
        controller.height = originalHeight;
        _audioCollision.gameObject.SetActive(true);
        _audioCollision.SetColliderRadius(_radiusStandartShere);
        _audioCollision.gameObject.SetActive(false);
        _isChangeSpeed = true;
    }

    private void Run()
    {
        _currentSpeed =  speed * 1.5f; // Спринт
        _audioCollision.gameObject.SetActive(true);
        _audioCollision.SetColliderRadius(_radiusStandartShere * 3f);
        _audioCollision.gameObject.SetActive(false);
        _isChangeSpeed = true;
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(3f);
        GameMode.PlayerUI.GameOver();
        
    }

    void Update()
    {
        Vector3 moveDirection = GetMoveDirection();
        HandleNormalMovement(moveDirection);
    }
    
    Vector3 GetMoveDirection()
    {
        float x = InputManager.Instance.Horizontal;
        float z = InputManager.Instance.Vertical;
        float y = 0;


        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;
        return move * _currentSpeed * Time.deltaTime;
    }

    public void UnBlockControl() 
    {
        _isBlockControl = false;
    }

    void HandleNormalMovement(Vector3 moveDirection)
    {
        if (!_isAlive || _isBlockControl)
        {
            _audioSource.Stop();
            return;
        }
         isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_currentSpeed == speed)
            _audioSource.clip = _sounds[1];
        else 
        if (_currentSpeed > speed)
            _audioSource.clip = _sounds[2];
        else
            _audioSource.clip = _sounds[0];

        if (moveDirection.x == 0 && moveDirection.z == 0 || !isGrounded)
        {
            _audioSource.Stop();
            _isChangeSpeed = true;
            if (_audioCollision.gameObject.activeSelf)
                _audioCollision.gameObject.SetActive(false);
        }
        else 
        {
            if (!_audioCollision.gameObject.activeSelf)
                _audioCollision.gameObject.SetActive(true);
            if (_isChangeSpeed)
            {
                _audioSource.Play();
                _isChangeSpeed = false;
            }            
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = GROUNDED_VELOCITY;
        }

        controller.Move(moveDirection);

        if (_isJump && isGrounded && velocity.y < 0)
        {
            _audioSource.PlayOneShot(_sounds[3]);
            _isJump = false;
        }

        if (InputManager.Instance.JumpPressed && isGrounded)
        {
            _isJump = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * GRAVITY);
        }
       

            velocity.y += GRAVITY * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Die() 
    {
        _isAlive = false;
        _control.Disable();
        GameMode.PlayerUI.ImpactAnimate();
        StartCoroutine(GameOver());
    }

    public bool IsAlive() => _isAlive;
}