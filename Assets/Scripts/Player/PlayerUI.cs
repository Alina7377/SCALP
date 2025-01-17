using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public bool _isTraining = false;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject finishScreen;
    [SerializeField] private GameObject _panel;
    [SerializeField] private LoaclizationText _centerText;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject _settingMenu;
    [SerializeField] private GameObject _impactObject;
    [SerializeField] private Image _visibleImage;
    [SerializeField] private Slider _sliderVolume;
    [SerializeField] private Slider _sliderSensitiviti;
    [SerializeField] private Animation _animate;    
    [SerializeField] private List<AnimationClip> _clips;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private AudioSource winSound;

    [Header("Настройки временного текста")]
    [SerializeField] private Animation _animateShowText;
    [SerializeField] private LoaclizationText _showTextMax;
    [SerializeField] private LoaclizationText _showTextMin;
    [SerializeField] private List<AnimationClip> _anumateClips;

    private AudioSource _audioSource;
    private PlayerControl _playerControl;
    private AudioSource[] _audios;
    private List<AudioSource> _pauseAudios = new List<AudioSource>();
    private bool _lastVisible = true;
    private bool _isImpact = false;
    private bool _isSaveProfilDate = false;
    private List<bool> _startParamActivaeObjects = new List<bool>();
    private Color _imageColor;

    // Добавляем ссылку на GridManager
    private GridManager _gridManager;

    private void Awake()
    {
        GameMode.PlayerUI = this;
        _audioSource = GetComponent<AudioSource>();
        _playerControl = new PlayerControl();
        _playerControl.UI.PauseMenu.started += context => Resume();

        // Получаем ссылку на GridManager
        _gridManager = FindObjectOfType<GridManager>();
    }

    private void Start()
    {
        SetStartParam();
        _isImpact = true;
        _animate.clip = _clips[3];
        _animate.Play();
        Events.Instance.OnBalckOut += BlackOut;
        Events.Instance.OnInteractGenerator += HasPower;
        Events.Instance.OnOpenDoor += OpenAllDoors;
        Events.Instance.OnReloadLevel += ReloadPlayer;

    }

    private void SetStartParam()
    {
        _startParamActivaeObjects.Clear();
        _startParamActivaeObjects.Add(pauseScreen.activeSelf);
        _startParamActivaeObjects.Add(finishScreen.activeSelf);
        _startParamActivaeObjects.Add(_panel.activeSelf);
        _startParamActivaeObjects.Add(_centerText.gameObject.activeSelf);
        _startParamActivaeObjects.Add(winScreen.activeSelf);
        _startParamActivaeObjects.Add(deathScreen.activeSelf);
        _startParamActivaeObjects.Add(_settingMenu.activeSelf);
        _startParamActivaeObjects.Add(_showTextMax.gameObject.activeSelf);
        _startParamActivaeObjects.Add(_showTextMin.gameObject.activeSelf);
        _startParamActivaeObjects.Add(_impactObject.activeSelf);
        _imageColor = _visibleImage.color;
    }

    private void ReloadStartParam() 
    {
        pauseScreen.SetActive(_startParamActivaeObjects[0]);
        finishScreen.SetActive(_startParamActivaeObjects[1]);
        _panel.SetActive(_startParamActivaeObjects[2]);
        _centerText.gameObject.SetActive(_startParamActivaeObjects[3]);
        winScreen.SetActive(_startParamActivaeObjects[4]);
        deathScreen.SetActive(_startParamActivaeObjects[5]);
        _settingMenu.SetActive(_startParamActivaeObjects[6]);
        _showTextMax.gameObject.SetActive(_startParamActivaeObjects[7]);
        _showTextMin.gameObject.SetActive (_startParamActivaeObjects[8]);
        _impactObject.gameObject.SetActive(_startParamActivaeObjects[9]);
        _visibleImage.color = _imageColor;
    }

    private void ReloadPlayer()
    {
        ReloadStartParam();
        _isImpact = false;
        _animate.clip = _clips[3];
        _animate.Play();
        _playerControl.Enable();
        
    }

    private void OnDisable()
    {
        Events.Instance.OnBalckOut -= BlackOut;
        Events.Instance.OnInteractGenerator -= HasPower;
        Events.Instance.OnOpenDoor -= OpenAllDoors;
    }
    private void OpenAllDoors(bool obj)
    {
        if (obj)
            if (_isTraining)
            {
                ShowFleshText("Training.16");
                StartCoroutine(Wait());
            }
            else ShowFleshText("UI.OpenDoors");
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
        ShowFleshText("Training.17");
    }

    private void HasPower()
    {
        if (_isTraining)
            ShowFleshText("Training.9");
        else ShowFleshText("UI.PowerEnable");
    }

    private void BlackOut(bool obj)
    {
        if (obj)
            ShowFleshText("UI.LightClose");
    }

    private void Rebase(bool isWin) 
    {
        if (_isSaveProfilDate == false && Settings.Instance.GetParam("level") == "Standard" && GameMode.Generate)
        {
            Profile.Instance.SafeProfilDate(Time.time - GameMode.StartTime, isWin);
            Profile.Instance.SafeCSV();
            _isSaveProfilDate = true;
        }
    }

    public void StopAllSound()
    {
        if (_audios == null)
        {
            _audios = FindObjectsOfType<AudioSource>();
        }
        foreach (var audio in _audios)
        {
            if (audio != null)
            if (audio.isPlaying)
            {
                audio.Pause();
                _pauseAudios.Add(audio);
            }
        }
    }

    public void PlayPausedAudios()
    { 
        foreach (var audio in _pauseAudios)
            { 
            if (audio != null)
                audio.UnPause(); 
        }
        _pauseAudios.Clear(); // Очищаем список после воспроизведения
    }

    private void ReplaceAvail(string current, string newAvail)
    {
        List<string> tags = LocalizationManager.Instance.GetTagList(current, true);
        foreach (var tag in tags)
        {
            LocalizationManager.Instance.WriteAvailForTag(tag, newAvail);
        }
        LocalizationManager.Instance.SetProgress();
    }

    /// <summary>
    /// Показать текст в центре экрана
    /// </summary>
    /// <param name="text"> Показываемый текст</param>
    /// <param name="isRewrite"> Перезаписать текст, если в данный момент выводится другой</param>
    public void ShowText(string text, bool isRewrite)
    {
        if (!isRewrite && _panel.activeSelf) return;
        _centerText.SetTag(text);
        _panel.SetActive(true);
    }

    public void DeactivatePanel()
    {
        _panel.SetActive(false);
    }

    public void PlayAudioClip(AudioClip clip)
    {
        if (!_audioSource.isPlaying)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }

    public void Restart(bool generate)
    {
        ReplaceAvail("grab", "f");
        if (_isTraining)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(2);
            return;
        }
         Rebase(false);
        _playerControl.UI.PauseMenu.started -= context => Resume();
        _playerControl.Disable();

        // Удаляем строку загрузки сцены
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Вызываем метод ResetLevel() у GridManager
        if (_gridManager != null)
        {
            _gridManager.ResetLevel(generate);
        }
        else
        {
            Debug.LogError("GridManager не найден!");
        }
        _isSaveProfilDate = false;
        // Сбрасываем время игры
        Time.timeScale = 1;

        // Возобновляем звуки
        PlayPausedAudios();

        // Закрываем экраны завершения игры, если они активны
        finishScreen.SetActive(false);

        // Скрываем панель паузы, если она активна
        pauseScreen.SetActive(false);

        // Скрываем курсор и блокируем его
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        

    }

    public void LoadMainMenu()
    {
        Rebase(false);
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    public void Pause()
    {
        StopAllSound();
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerControl.Enable();
    }

    public void Resume()
    {
        Time.timeScale = 1;
        _playerControl.Disable();
        if (pauseScreen !=null)
            pauseScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayPausedAudios();
    }

    public void Finish()
    {
        ReplaceAvail("grab", "n");        
        StopAllSound();
        finishScreen.SetActive(true);
        winScreen.SetActive(true);
        Rebase(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winSound.Play();
    }

    public void GameOver()
    {
        ReplaceAvail("grab", "f");
        StopAllSound();
        finishScreen.SetActive(true);
        deathScreen.SetActive(true);
        Rebase(false);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameMode.FirstPersonLook.BlockPlayerController();
        deathSound.Play();
    }

    public void OpenSetting()
    {
        _settingMenu.SetActive(true);
        _sliderVolume.value = float.Parse(Settings.Instance.GetParam("volume"));
        _sliderSensitiviti.value = float.Parse(Settings.Instance.GetParam("sensitivity"));
    }

    public void SaveSetting()
    {
        GameMode.FirstPersonLook.ChangeSettings();
        _settingMenu.SetActive(false);
    }

    public void ChangeValueSound()
    {
        Settings.Instance.SetParam("volume", _sliderVolume.value.ToString());
        AudioListener.volume = _sliderVolume.value; // Исправлено: используем _sliderVolume.value
    }

    public void ChangeValueSensetiviti()
    {
        Settings.Instance.SetParam("sensitivity", _sliderSensitiviti.value.ToString());
    }

    public void ChangeVisiblePayer(bool isVisible)
    {        
    if (_lastVisible != isVisible && !_isImpact)
        {         
            if (isVisible )
                _animate.clip = _clips[0];
            else
                _animate.clip = _clips[1];
            _lastVisible = isVisible;
            _animate.Play();
         }
    }

    public void ImpactAnimate() 
    {
        _animate.clip = _clips[2];
        _isImpact = true;
        _animate.Play();
    }

    public void StartGame() 
    {
        _isImpact = false;
        GameMode.FirstPersonMovement.UnBlockControl();
        if (_isTraining)
        {
            ShowFleshText("Training.1");
        }
    }

    public void ShowFleshText(string tag)
    {
        if (_animateShowText.isPlaying)
            _animateShowText.Stop();
        if (_isTraining)
        {
            _animateShowText.clip = _anumateClips[0];
            _showTextMax.SetTag(tag);

        }
        else 
        {
            _animateShowText.clip = _anumateClips[1];
            _showTextMin.SetTag(tag);
        }
        _animateShowText.Play();
    }

    public void ShowFleshTextOnlyTraing(string tag)
    {
        if (!_isTraining) { return; }
        if (_animateShowText.isPlaying)
            _animateShowText.Stop();
        _animateShowText.clip = _anumateClips[0];
        _showTextMax.SetTag(tag);
        _animateShowText.Play();
    }

    public bool IsPlayingAnimator() 
    {
        return _animateShowText.isPlaying;
    }
}
