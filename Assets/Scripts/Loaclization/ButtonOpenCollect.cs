
using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenCollect : MonoBehaviour
{
    [SerializeField] private Button _button;
    private string _tag;
    private LibraryMenu _library;
    private bool _isNew;
    private Sprite _spriteOld;
    private Sprite _spriteNew;


    public void ButtonClick()
    {
        if (_isNew)
        { 
            _button.image.sprite = _spriteOld;
            _isNew = false;
           LocalizationManager.Instance.WriteAvailForTag(_tag, "t");
        }
        _library.ButtonClic(_tag);
    }

    public void SetTag(string tag) 
    {
        _tag = tag;
    }

    public void SetLibrary(LibraryMenu library)
    {
        _library = library;
    }    

    public void SetSprits(Sprite sold, Sprite snew, bool isNew)    
    { 
        _spriteNew = snew;
        _spriteOld = sold;
        _isNew = isNew;
        if (isNew) _button.image.sprite = _spriteNew;
        else _button.image.sprite = _spriteOld;
    }

}
