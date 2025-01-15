using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Отображение прогресса сбора документов")]
    [SerializeField] private Text _progressText;
    [SerializeField] private Image _progressImage;

    private List<GameObject> _buttons = new List<GameObject>();

    private void Awake()
    {

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

    private void ClearButtons()
    {
        foreach (GameObject button in _buttons)
        {
            Destroy(button);
        }
        _audioSource.Stop();
        SetPlayButtonImage(false);
        _buttons.Clear();
    }

    private void ShowProgressInfo() 
    {
        float progressPercent = LocalizationManager.Instance.GetProgress();
        _progressText.text = LocalizationManager.Instance.GetTextForTag("Menu.ProgressDocum") + " " + progressPercent.ToString() + " / 100 %";
        _progressImage.fillAmount = progressPercent / 100;
    }

    public void OpenLibrary(string type)
    {
        _outText.gameObject.SetActive(false);
        _imageMin.gameObject.SetActive(false);
        _imageMax.gameObject.SetActive(false);
        _buttonPlay.gameObject.SetActive(false);
        ClearButtons();
        ShowProgressInfo();
        GameObject newButtonObject;
        ButtonOpenCollect newButton;
        List<string> tags = new List<string>();
        tags = LocalizationManager.Instance.GetTagList("n", true);
        foreach (string tag in tags)
        {
            if (!tag.Contains(type)) continue;
            newButtonObject = GameObject.Instantiate(_prefabButton, _buttonsGroup);
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
            if (!tag.Contains(type)) continue;
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
            Debug.Log(tag);
            _outText.gameObject.SetActive(true);
            _outText.text = LocalizationManager.Instance.GetTextForTag(tag);
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

}
