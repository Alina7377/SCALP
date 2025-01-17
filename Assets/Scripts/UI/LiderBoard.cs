using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiderBoard : MonoBehaviour
{
    // Это убрать после тестов
    [Header("Поля для добавления профиля")] 
    [SerializeField] private GameObject _panelAddPlayer;
    [SerializeField] private GameObject _panelLiderBoard;
    [SerializeField] private InputField _userName;
    [SerializeField] private InputField _recordData;

    [Header("Вывод информации об игроке")]
    [SerializeField] private Text _userNameProfile;
    [SerializeField] private Text _rating;
    [SerializeField] private Text _bestTime;
    [SerializeField] private Text _winCount;
    [SerializeField] private Text _gameCount;
    [SerializeField] private Text _progress;
    [SerializeField] private Text _timeToGame;
    [SerializeField] private Button _changeNikname;
    [SerializeField] private UserData _userData;

    [Header("Вывод информации для топа игроков")]
    [SerializeField] private GameObject _liderGroup;
    [SerializeField] private GameObject _gamerProfile;
    [SerializeField] private GameObject _prefabPin;
    [SerializeField] private int _topLider = 8;
    [SerializeField] private Text _infoConnection;

    private List<LiderPin> liderPins = new List<LiderPin>();
    private LiderPin _profilPin;
    private DBManager _dbManager;

    private void Awake()
    {
        _dbManager = GetComponent<DBManager>();
    }

    private string TimeFormat(float timeFloat)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeFloat);
        return time.ToString("hh':'mm':'ss");
    }

    public void DisplayProfilInfo() 
    {
        Profile.Instance.SetCSV();
        _userNameProfile.text = Profile.Instance.GetProfilParamSTR("nikname");
        _rating.text = "-";
        _bestTime.text = TimeFormat(Profile.Instance.GetProfilParamF("best_time"));
        _winCount.text = Profile.Instance.GetProfilParamSTR("count_win_game");
        //_gameCount.text = Profile.Instance.GetProfilParamSTR("count_game");
        _timeToGame.text = TimeFormat(Profile.Instance.GetProfilParamF("time_to_game"));
        _progress.text = LocalizationManager.Instance.GetProgress().ToString() + "/100 %";
    }

    public async void LoadLiderBoard() 
    {
        _changeNikname.gameObject.SetActive(false);
        _infoConnection.gameObject.SetActive(true);
        _infoConnection.text = LocalizationManager.Instance.GetTextForTag("Lid.ConnectionAttempt");
        try
        {
            // Сначала считаем все данные о пользователе
            await _dbManager.UpdateData();
            // Тепреь считываем данные из базы
            var tack = _dbManager.GetDBcollection();
            var records = await tack;
            int top;
            float minTime = 0, winrate = 0;
            SProfileData lider = new SProfileData();
            // Очищаем старый лидерборд
            foreach (var lid in liderPins)
            {
                Destroy(lid.gameObject);
            }
            liderPins.Clear();
            // Сортируем по времени
            minTime = 0;
            List<SProfileData> liders = new List<SProfileData>();
            List< SProfileData> nullLid = new List<SProfileData>();
            while (records.Count > 0 /*|| liders.Count<top*/)
            {
                foreach (var record in records)
                {
                    if (record.BestTime == 0)
                    {                        
                        nullLid.Add(record);
                        continue;
                    }
                    if (minTime == 0)
                    {
                        minTime = record.BestTime;
                        lider = record;
                    }
                    else
                        if (record.BestTime < minTime)
                    {
                        lider = record;
                        minTime = record.BestTime;
                    }
                    else
                    if (record.BestTime == minTime)
                    {
                        winrate = record.CountWin / record.CountGame;
                        // Если время одинаковое - сортируем по процент побед
                        if (winrate > (lider.CountWin / lider.CountGame))
                            lider = record;
                        else
                            // Если и процент побед одинаков, то сортируем по выйгрышам
                            if (winrate == (lider.CountWin / lider.CountGame))
                            if (record.CountWin > lider.CountWin) lider = record;
                    }
                }
                //Debug.Log(lider.UserName + " " + lider.BestTime);
                if (minTime != 0)
                {
                    liders.Add(lider);
                    minTime = 0;
                    records.Remove(lider);
                }               
                foreach(var nolid in nullLid)
                    records.Remove(nolid);
            }
            // Перед выводом топа игроков, нужно найти нашего игрока и вывести его
            if (_profilPin != null)
            {
                Destroy(_profilPin.gameObject);
                _profilPin = null;
            }
            bool _isCreatProfilPin = false;
            foreach (var record in liders)
            {
                if (record.UserName == Profile.Instance.GetProfilParamSTR("nikname"))
                {
                    GameObject rec = GameObject.Instantiate(_prefabPin, _gamerProfile.transform);
                    LiderPin liderPin = rec.GetComponent<LiderPin>();
                    liderPin.DisplayInfo(record, liders.IndexOf(record) + 1);
                    _profilPin = liderPin;
                    _rating.text = (liders.IndexOf(record) + 1).ToString();
                    _isCreatProfilPin = true;
                    break;
                }
            }
            if (!_isCreatProfilPin)
            {
                GameObject rec = GameObject.Instantiate(_prefabPin, _gamerProfile.transform);
                LiderPin liderPin = rec.GetComponent<LiderPin>();
                liderPin.DisplayInfo(Profile.Instance.GetProfilData, -1);
                _profilPin = liderPin;
            }

            // Определяем кол-во записей в лидерборде
            if (liders.Count < _topLider)
                top = liders.Count;
            else top = _topLider;

            // Теперь выводим топ
            for (int i = 0; i < top; i++)
            {
                GameObject rec = GameObject.Instantiate(_prefabPin, _liderGroup.transform);
                LiderPin liderPin = rec.GetComponent<LiderPin>();
                liderPin.DisplayInfo(liders[i], i + 1);
                liderPins.Add(liderPin);
            }
            _infoConnection.gameObject.SetActive(false);
            // После вывода топа, можем занятся профилем игрока
            ActivateConnectionProfile();
        }
        catch 
        {
            _infoConnection.text = LocalizationManager.Instance.GetTextForTag("Lid.ConnectionError");
        }
    }

    private void ActivateConnectionProfile()
    {
        _changeNikname.gameObject.SetActive(true);
        if (Profile.Instance.GetProfilParamSTR("nikname") == "")
        {
            _userData.ShowChangeProfilWindow();
        }
    }

    public void ResetProfil() 
    {
        Profile.Instance.ResetProfilData();
        DisplayProfilInfo();
    }
}
