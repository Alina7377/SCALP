using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacmentPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _interactObject;
    [SerializeField] private ItemType _itemType;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip genActivationSound;

    private IInteractable _interactable;

    private bool _isInteract = false;

    private void Awake()
    {
        _interactable = _interactObject.GetComponent<IInteractable>();
    }

    private IEnumerator ShowText()
    {
        yield return new WaitForSeconds(15);
        GameMode.PlayerUI.DeactivatePanel();
    }
    
    public void Interact()
    {
        if (_isInteract) { return; }
        else 
        {
            GameMode.PlayerUI.DeactivatePanel();
            GameMode.PlayerUI.ShowText("UI.Request",true);
            StartCoroutine(ShowText());
        }
    }

    public bool Interact(ref GameObject interactingOject)
    {      
        PickableItem pickable;
        if (interactingOject.TryGetComponent<PickableItem>(out pickable) && _itemType == pickable.GetItemType())
        {
            StartCoroutine(MoveToGenerator(interactingOject));

            if (audioSource != null && genActivationSound != null)
            {
                audioSource.PlayOneShot(genActivationSound);
            }
            else
            {
                Debug.LogWarning("AudioSource или звук активации не назначены для PlacementPoint.");
            }

            interactingOject = null;
            _isInteract = true;
            return true;
            /*interactingOject.transform.position = transform.position;
            interactingOject.transform.rotation = transform.rotation;
            interactingOject.transform.SetParent(transform);
            interactingOject.transform.tag = "Untagged";
            _interactable.Interact();
            interactingOject = null;
            _isInteract = true;
            return true;*/
        }
        else
        {
            GameMode.PlayerUI.DeactivatePanel();
            GameMode.PlayerUI.ShowText("UI.Request", true);
            StartCoroutine(ShowText());
        }
        return false;
    }
    private IEnumerator MoveToGenerator(GameObject item)
    {
        Vector3 startPosition = item.transform.position; // Начальная позиция
        Quaternion startRotation = item.transform.rotation; // Начальная ротация

        Vector3 targetPosition = transform.position; // Целевая позиция (позиция генератора)
        Quaternion targetRotation = transform.rotation; // Целевая ротация

        float duration = 1f; // Длительность анимации
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Плавное перемещение и вращение
            item.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            item.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);

            elapsedTime += Time.deltaTime; // Увеличиваем время
            yield return null; // Ждем следующего кадра
        }
        // Костыль (некгода искать более оптимальный способ)
        Vector3 costScale = new Vector3(0.5f, 0.5f, 0.5f);

        item.transform.SetParent(null);
        item.transform.localScale = costScale;

        // Убедитесь, что объект точно на целевой позиции и ротации
        item.transform.position = targetPosition;
        item.transform.rotation = targetRotation;

        // Устанавливаем родителя для объекта
        item.transform.SetParent(transform);
        item.tag = "Untagged"; // Сбрасываем тег
        _interactable.Interact(); // Взаимодействуем с объектом
    }
}
