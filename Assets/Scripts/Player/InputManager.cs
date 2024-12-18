using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Синглтон InputManager, позволяющий легко получить доступ к экземпляру из любого места в коде.
    public static InputManager Instance { get; private set; }

    // Свойства для доступа к значениям осей движения.
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }

    // Свойства для доступа к значениям движения мыши.
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }

    // Свойство для проверки, была ли нажата кнопка прыжка в текущем кадре.
    public bool JumpPressed { get; private set; }

    private void Awake()
    {
        // Устанавливаем синглтон, убеждаемся, что существует только один экземпляр InputManager.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожаем объект при загрузке новой сцены.
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликат, если он существует.
        }
    }

    void Update()
    {
        // Получаем значения осей движения и мыши из ввода пользователя.
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");
        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");

        // Проверяем, была ли нажата кнопка прыжка в этом кадре.
        JumpPressed = Input.GetButtonDown("Jump");
    }
}