using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingLevel : MonoBehaviour
{
    [SerializeField] private Color _noActiveColor;
    [SerializeField] private Color _activeColor;

    [Header("Титульные поля")]
    [SerializeField] private Text _handlerLevel;

    [Header("Слайдеры и их настройки")]
    [SerializeField] private Slider _sliderCountRoomNP;
    [SerializeField] private Image _sliderCountRoomNPBackground;
    [SerializeField] private Image _sliderCountRoomNPHandler;

    [SerializeField] private Slider _sliderCountCloseRoom;
    [SerializeField] private Image _sliderCountCloseRoomBackground;
    [SerializeField] private Image _sliderCountCloseRoomHandler;

    [SerializeField] private Slider _sliderCountDoc;
    [SerializeField] private Image _sliderCountDocBackground;
    [SerializeField] private Image _sliderCountDocHandler;

    [SerializeField] private Slider _sliderCountEnemy;
    [SerializeField] private Image _sliderCountEnemyBackground;
    [SerializeField] private Image _sliderCountEnemyHandler;

    [Header("Текстовые поля вывода")]
    [SerializeField] private Text _outCountRoomNP;
    [SerializeField] private Text _outCountCloseRoom;
    [SerializeField] private Text _outCountDoc;
    [SerializeField] private Text _outCountEnemy;

    [Header("Текстовые поля описания")]
    [SerializeField] private Text _infoCountRoomNP;
    [SerializeField] private Text _infoCountCloseRoom;
    [SerializeField] private Text _infoCountDoc;
    [SerializeField] private Text _infoCountEnemy;

    private string _currentlevel;

    private void SetSettingParam(string level)
    {
        if (level == "Custom")
        {
            // Настриваем первый Параметр
            _sliderCountRoomNP.value = float.Parse(Settings.Instance.GetParam("count_room_np"));
            _sliderCountRoomNP.interactable = true;
            _sliderCountRoomNPBackground.color = _activeColor;
            _sliderCountRoomNPHandler.color = _activeColor;

            _outCountRoomNP.text = _sliderCountRoomNP.value.ToString();
            _outCountRoomNP.color = _activeColor;

            _infoCountRoomNP.color = _activeColor;

            // Настриваем второй параметр
            _sliderCountCloseRoom.value = float.Parse(Settings.Instance.GetParam("count_close_room"));
            _sliderCountCloseRoom.interactable = true;
            _sliderCountCloseRoomBackground.color = _activeColor;
            _sliderCountCloseRoomHandler.color = _activeColor;

            _outCountCloseRoom.text = _sliderCountCloseRoom.value.ToString();
            _outCountCloseRoom.color = _activeColor;

            _infoCountCloseRoom.color = _activeColor;

            // Настраиваем третий параметр
            _sliderCountDoc.value = float.Parse(Settings.Instance.GetParam("count_doc")); ;
            _sliderCountDoc.interactable = true;
            _sliderCountDocBackground.color = _activeColor;
            _sliderCountDocHandler.color = _activeColor;

            _outCountDoc.text = _sliderCountDoc.value.ToString();
            _outCountDoc.color = _activeColor;

            _infoCountDoc.color = _activeColor;

            // Настраиваем четвертый параметр
            _sliderCountEnemy.value = float.Parse(Settings.Instance.GetParam("count_enemy")); ;
            _sliderCountEnemy.interactable = true;
            _sliderCountEnemyBackground.color = _activeColor;
            _sliderCountEnemyHandler.color = _activeColor;

            _outCountEnemy.text = _sliderCountEnemy.value.ToString();
            _outCountEnemy.color = _activeColor;

            _infoCountEnemy.color = _activeColor;
        }
        else
        {
            // Настриваем первый Параметр
            _sliderCountRoomNP.value = _sliderCountRoomNP.maxValue;
            _sliderCountRoomNP.interactable = false;
            _sliderCountRoomNPBackground.color = _noActiveColor;
            _sliderCountRoomNPHandler.color = _noActiveColor;

            _outCountRoomNP.text = _sliderCountRoomNP.value.ToString();
            _outCountRoomNP.color = _noActiveColor;

            _infoCountRoomNP.color = _noActiveColor;

            // Настриваем второй параметр
            _sliderCountCloseRoom.value = _sliderCountCloseRoom.maxValue;
            _sliderCountCloseRoom.interactable = false;
            _sliderCountCloseRoomBackground.color = _noActiveColor;
            _sliderCountCloseRoomHandler.color = _noActiveColor;

            _outCountCloseRoom.text = _sliderCountCloseRoom.value.ToString();
            _outCountCloseRoom.color = _noActiveColor;

            _infoCountCloseRoom.color = _noActiveColor;

            // Настраиваем третий параметр
            _sliderCountDoc.value = 4;
            _sliderCountDoc.interactable = false;
            _sliderCountDocBackground.color = _noActiveColor;
            _sliderCountDocHandler.color = _noActiveColor;

            _outCountDoc.text = _sliderCountDoc.value.ToString();
            _outCountDoc.color = _noActiveColor;

            _infoCountDoc.color = _noActiveColor;

            // Настраиваем четвертый параметр
            _sliderCountEnemy.value = 4;
            _sliderCountEnemy.interactable = false;
            _sliderCountEnemyBackground.color = _noActiveColor;
            _sliderCountEnemyHandler.color = _noActiveColor;

            _outCountEnemy.text = _sliderCountEnemy.value.ToString();
            _outCountEnemy.color = _noActiveColor;

            _infoCountEnemy.color = _noActiveColor;

        }
    }

    private void SafeCustomSetting() 
    {
        Settings.Instance.SetParam("count_room_np", _sliderCountRoomNP.value.ToString());
        Settings.Instance.SetParam("count_close_room", _sliderCountCloseRoom.value.ToString());
        Settings.Instance.SetParam("count_doc", _sliderCountDoc.value.ToString());
        Settings.Instance.SetParam("count_enemy", _sliderCountEnemy.value.ToString());
    }

    public void OpenSettingLevel() 
    {
        _currentlevel = "Standard";
        _handlerLevel.text = LocalizationManager.Instance.GetTextForTag("LevelSet." + _currentlevel);
        SetSettingParam(_currentlevel);
    }

    public void ChangeLevel() 
    {
        if (_currentlevel == "Custom")
        {
            _currentlevel = "Standard";
            SafeCustomSetting();
        }
        else
            _currentlevel = "Custom";
        _handlerLevel.text = LocalizationManager.Instance.GetTextForTag("LevelSet." + _currentlevel);
        Settings.Instance.SetParam("level",_currentlevel);
       
        SetSettingParam(_currentlevel);
    }

    public void ChangeParam() 
    {
        _outCountRoomNP.text = _sliderCountRoomNP.value.ToString();
        _outCountCloseRoom.text = _sliderCountCloseRoom.value.ToString();
        _outCountDoc.text = _sliderCountDoc.value.ToString();
        _outCountEnemy.text = _sliderCountEnemy.value.ToString();
    }

    public void SafeSetting() 
    {
        Settings.Instance.SetParam("level", _currentlevel);
        if (_currentlevel != "Standard")
            SafeCustomSetting();
        Debug.Log("Уровень сложность " + _currentlevel);
        Settings.Instance.SafeCSV();

    }
    
}
