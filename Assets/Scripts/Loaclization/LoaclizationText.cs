using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoaclizationText : MonoBehaviour
{
    [SerializeField] private TextType _type;
    private Text _text;
    private TMP_Text _textMeshPro;
    private TextMeshPro _textMeshPro2;
    private string _tag;

    private bool _isLoading = true; 

    private void Awake()
    {
          
    }

    private void Update()
    {
        if (_isLoading && LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnChangeLanguage += ChangeLanguage;
            _isLoading = false;
            switch (_type)
            {
                case TextType.None:
                    _text = transform.GetComponent<Text>();
                    _tag = _text.text;
                    break;
                case TextType.TMP:
                    _textMeshPro = transform.GetComponent<TMP_Text>();
                    _tag = _textMeshPro.text;
                    break;
                case TextType.TextMesh:
                    TryGetComponent<TextMeshPro>(out _textMeshPro2);
                    _tag = _textMeshPro2.text;
                    break;
            }
            OnEnable();
        }
    }

    private void OnEnable()
    {
        if (_text != null || _textMeshPro != null || _textMeshPro2 != null)
            SetTag(_tag);
    }

    private void ChangeLanguage() 
    {
        if (_text != null || _textMeshPro != null || _textMeshPro2 != null)
            SetTag(_tag);
    }

    public void SetTag(string newTag)
    {
        if (newTag == "") return;
        _tag = newTag;
        switch (_type)
        {
            case TextType.None:
                if (_text != null)
                    _text.text = LocalizationManager.Instance.GetTextForTag(_tag);
                break;
            case TextType.TMP:
                if(TryGetComponent<TMP_Text>(out _textMeshPro))
                    _textMeshPro.text = LocalizationManager.Instance.GetTextForTag(_tag);
                break;
            case TextType.TextMesh:
                // Это какая-то жесть.... Без этого гемороя, выходят сообщения об ошибке
                if ((_tag.Contains("RoomName") || _tag.Contains("UI.")) && TryGetComponent<TextMeshPro>(out _textMeshPro2)) 
                {
                _textMeshPro2.text = LocalizationManager.Instance.GetTextForTag(_tag);
        }
                break;
        }
    }
}
