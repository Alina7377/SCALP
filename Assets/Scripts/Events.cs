using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour
{
    public event Action OnInteractGenerator;
    public event Action<bool> OnBalckOut;
    public event Action<bool> OnOpenDoor;

    public static Events Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    /// <summary>
    /// Активация генератора
    /// </summary>
    public void InteractGenerator()
    {
        OnInteractGenerator?.Invoke();
    }

    /// <summary>
    /// Запуск/остановка блекаута
    /// </summary>
    /// <param name="state"></param>
    public void ChangeStateBlackOut(bool state)
    {
        OnBalckOut?.Invoke(state);
    }

    /// <summary>
    /// Открытие/закрытие всех доступных дверей
    /// </summary>
    /// <param name="state"></param>
    public void ChangeOpenDoor(bool state)
    {
        OnOpenDoor?.Invoke(state);
    }
}
