using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStartLevel : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float delayBetweenSounds = 0.5f; // Задержка между звуками

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlaySoundsSequentially());
    }

    private IEnumerator PlaySoundsSequentially()
    {
        foreach (AudioClip clip in audioClips)
        {
            audioSource.clip = clip; // Устанавливаем текущий аудиофайл
            audioSource.Play(); // Воспроизводим звук

            // Ждем, пока звук закончится
            yield return new WaitForSeconds(clip.length + delayBetweenSounds);
        }
    }
}
