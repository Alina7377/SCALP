using UnityEngine;
using UnityEngine.UI;

public class UserData : MonoBehaviour
{
    [SerializeField] private Text _discriptionText;
    [SerializeField] private Text _ruleText;
    [SerializeField] private InputField _inputNikname;
    [SerializeField] private Text _resultText;
    [SerializeField] private DBManager _dbManager;
    [SerializeField] private LiderBoard _board;

    private Color _color;
    private string _oldNikname;

    private string _noValidChar = @"{}[]:/\|;,""";

    private bool ChackNiknameValid(string nikname)
    {
        for (int i = 0; i < _noValidChar.Length; i++)
        {
            for (int j = 0; j < nikname.Length; j++) 
            {
                if (_noValidChar[i] == nikname[j]) return true;
            }
        }
        return false;
    }

    private bool CheakNiknameAvailability(string nikname) 
    {
        var list = _dbManager.CheackRecord(nikname);
    
        return true;
    }    

   private void ShowText(bool newUser) 
    {
        if (newUser)
        {
            _discriptionText.text = LocalizationManager.Instance.GetTextForTag("Lid.NewUser");
        }
        else _discriptionText.text = LocalizationManager.Instance.GetTextForTag("Lid.ChangeNikname");
        _ruleText.text = LocalizationManager.Instance.GetTextForTag("Lid.Info") + " " +_noValidChar;
        _resultText.gameObject.SetActive(false);
    }

    public void ShowChangeProfilWindow()
    {
        gameObject.SetActive(true);
        _oldNikname = Profile.Instance.GetProfilParamSTR("nikname");
        if (_oldNikname == "") ShowText(true);
        else ShowText(false);
        _inputNikname.text = _oldNikname;
    }

    public async void SetNewNikanme() 
    {
        _resultText.gameObject.SetActive(true);
        if (_inputNikname.text == "" || _inputNikname.text == " ")
        {
            _resultText.text =  LocalizationManager.Instance.GetTextForTag("Lid.NoNull");
            return;
        }
        if (ChackNiknameValid(_inputNikname.text)) 
        {
            _resultText.text = LocalizationManager.Instance.GetTextForTag("Lid.NoValid");
            return;
        }
        var list = _dbManager.CheackRecord(_inputNikname.text);
        var record = await list;
        foreach (var item in record)
        {
            Debug.Log("Найдена запись " + item);
        }
        if (record.Count > 0)
        {
            _resultText.text = LocalizationManager.Instance.GetTextForTag("Lid.UsingNikname");
            return;
        }

        _resultText.text = "OK";
        await _dbManager.UpdateNiknameData(_inputNikname.text);
        Profile.Instance.SetProfilParam("nikname", _inputNikname.text);
        Profile.Instance.SafeCSV();
        _board.DisplayProfilInfo();
        _board.LoadLiderBoard();
        gameObject.SetActive(false);
    }

   
}
