using UnityEngine;

/// <summary>
/// Уневерсальный скрипт для активируемых звуков.
/// Если звук должен привлекать внимание новых ботов пока звучит, необходимо в _triggerCollider указать
/// объект с триггером и скриптом AudioTrigger. Для одиночного вызова не требуется
/// </summary>

public class InteractSoundSource : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioTrigger _triggerCollider;
    // [SerializeField] private float _soundDistance; Пока заменено на _audioSource.volume
    private AudioSource _audioSource;
    private bool _isPlaying;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_triggerCollider !=null &&!_isPlaying && !_audioSource.isPlaying) 
        {
            _triggerCollider.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (_audioSource != null)
        {
            
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _isPlaying = false;
            }
            else
            {
                _audioSource.Play();
                CheackEnemy();
            }
        }
    }

    private void CheackEnemy() 
    {
        if (_triggerCollider == null)
        {
            GameMode.EnemyManager.AlarmAtDistance(transform.position, _audioSource.maxDistance);
        }
        else 
        {
            _triggerCollider.gameObject.SetActive(true);
            _triggerCollider.SetColliderRadius(_audioSource.maxDistance);

        }
    }

    public bool Interact(ref GameObject interactingOject)
    {
        throw new System.NotImplementedException();
    }
}
