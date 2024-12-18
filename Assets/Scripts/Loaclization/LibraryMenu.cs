using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class LibraryMenu : MonoBehaviour
{
    [Header("Вывод информации")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TMP_Text _outText;
    [SerializeField] private Image _imageMin;
    [SerializeField] private Image _imageMax;
    [SerializeField] private Button _buttonPlay;

    [Header("Спрайты для кнопки Play")]
    [SerializeField] private Sprite _passPlay;
    [SerializeField] private Sprite _acctPlay;
    [SerializeField] private Sprite _passStop;
    [SerializeField] private Sprite _acctStop;

    [Header("Создание кнопки")]
    [SerializeField] private GameObject _prefabButton;
    [SerializeField] private Transform _buttonsGroup;
    [SerializeField] private Sprite _buttonNew;
    [SerializeField] private Sprite _buttonOldReports;
    [SerializeField] private Sprite _buttonOldAudios;

    [Header("Общие настройки")]
    [SerializeField] private SOCollections _repotsSO;
    [SerializeField] private SOCollections _audioSO;

    [SerializeField] private List<string> _tags = new List<string>();
    private List<GameObject> _buttons = new List<GameObject>();
    private LoaclizationText _localizateText;

    private void Awake()
    {
        _localizateText = _outText.GetComponent<LoaclizationText>();
    }

    private void SetPlayButtonImage(bool isPaly)        
    {
        SpriteState spriteState = _buttonPlay.spriteState;
        if (isPaly)
        {           
            _buttonPlay.image.sprite = _passStop;
            spriteState.highlightedSprite = _acctStop;
            spriteState.pressedSprite = _acctStop;
            _buttonPlay.spriteState = spriteState;
        }
        else 
        {
            _buttonPlay.image.sprite = _passPlay;
            spriteState.highlightedSprite = _acctPlay;
            spriteState.pressedSprite = _acctPlay;
            _buttonPlay.spriteState = spriteState;
        }
    }

    public void OpenLibrary()
    {
        GameObject newButtonObject;
        ButtonOpenCollect newButton;
        List<string> tags = new List<string>();
        _outText.gameObject.SetActive(false);
        _imageMin.gameObject.SetActive(false);
        _imageMax.gameObject.SetActive(false);
        _buttonPlay.gameObject.SetActive(false);
        tags = LocalizationManager.Instance.GetTagList("n", true);
        foreach (string tag in tags)
        {
            newButtonObject =  GameObject.Instantiate(_prefabButton,_buttonsGroup);
            newButton = newButtonObject.GetComponent<ButtonOpenCollect>();
            newButton.SetLibrary(this);
            if (tag.Contains("Reports."))
                newButton.SetSprits(_buttonOldReports, _buttonNew, true);
            else newButton.SetSprits(_buttonOldAudios, _buttonNew, true);
            newButton.SetTag(tag);
            _buttons.Add(newButtonObject);

        }
        tags = LocalizationManager.Instance.GetTagList("t", true);
        foreach (string tag in tags)
        {
            newButtonObject = GameObject.Instantiate(_prefabButton, _buttonsGroup);
            newButton = newButtonObject.GetComponent<ButtonOpenCollect>();
            newButton.SetLibrary(this);
            if (tag.Contains("Reports."))
                newButton.SetSprits(_buttonOldReports, _buttonNew, false);
            else newButton.SetSprits(_buttonOldAudios, _buttonNew, false);
            newButton.SetTag(tag);
            _buttons.Add(newButtonObject);
        }
    }

    public void ButtonClic(string tag) 
    {
        _audioSource.Stop();
        if (tag != "")
        {
            _outText.gameObject.SetActive(true);
            _localizateText.SetTag(tag);
        }
        if (tag.Contains("Image"))
        {
            _imageMax.sprite = _repotsSO.GetImageForTag(tag);
            _imageMax.gameObject.SetActive(true);
            _imageMin.gameObject.SetActive(false);
            _buttonPlay.gameObject.SetActive(false);
            return;
        }
        if (tag.Contains("Reports."))
        {
            _imageMin.sprite = _repotsSO.GetImageForTag(tag);
            _imageMin.preserveAspect = true;
            _imageMax.gameObject.SetActive(false);
            _imageMin.gameObject.SetActive(true);
            _buttonPlay.gameObject.SetActive(false);
            return;
        }
        if (tag.Contains("Audio."))
        {
            _audioSource.clip = _audioSO.GetAudioForTag(tag);
            _imageMin.sprite  = _audioSO.GetImageForTag(tag);
            _imageMin.preserveAspect = true;
            _imageMax.gameObject.SetActive(false);
            _imageMin.gameObject.SetActive(true);
            _buttonPlay.gameObject.SetActive(true);
            SetPlayButtonImage(false);
            return;
        }


    }

    public void PlayAudio() 
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Pause();
            SetPlayButtonImage(false);
        }
        else 
        {
            _audioSource.Play();
            SetPlayButtonImage(true);
        }
    }


    public void Back() 
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _audioSource.Stop();
        SetPlayButtonImage(false);
        _buttons.Clear();
        gameObject.SetActive(false);
    }


}
