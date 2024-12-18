using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Collection",
                 menuName = "Collection", order = 55)]

public class SOCollections: ScriptableObject
{
    [SerializeField] private CollectibleType _typeColect;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private List<string> _tagList;
    [SerializeField] private List<Sprite> _images;
    [SerializeField] private List<AudioClip> _audios;

    /// <summary>
    /// Три варианта avail:
    /// f - недоступно
    /// t - доступно
    /// n - новое, недавно получено, но не просмотренно
    /// </summary>
    /// <param name="avail"></param>
    /// <returns></returns>
    
    public GameObject GetPrefab() => _prefab;

    public CollectibleType GetTypeColl() => _typeColect;

    public AudioClip GetAudioForTag(string tag) 
    {
        if (_audios.Count == 0) return null;
        for (int i =0;i<_tagList.Count;i++) 
        {
            if (tag.Contains(_tagList[i]) && i<_audios.Count) return _audios[i]; 
        }
        return null;
    }

    public Sprite GetImageForTag(string tag)
    {
        if (_images.Count == 0) return null;
        for (int i = 0; i < _tagList.Count; i++)
        {
            if (tag.Contains(_tagList[i]) && i < _images.Count) return _images[i];
        }
        return null;
    }


}