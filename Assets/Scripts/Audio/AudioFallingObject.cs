using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFallingObject : MonoBehaviour
{
    [SerializeField] private float _responseSensitivity = 2.6f;
    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float rlSpeed = Mathf.Abs(collision.relativeVelocity.x) + Mathf.Abs(collision.relativeVelocity.y) + Mathf.Abs(collision.relativeVelocity.z);
        if (rlSpeed > _responseSensitivity) AlarmEnemy();
    }

    private void AlarmEnemy() 
    {
        _audioSource.Play();
        GameMode.EnemyManager.AlarmAtDistance(transform.position, _audioSource.maxDistance);
    }

}
