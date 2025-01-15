using System.IO;
using UnityEngine;

public   class Settings: MonoBehaviour
{
    private  char _fieldSeperator = ';';
    private  char _lineSeperater = '\n';
    private  string[,] _settings;

    public static Settings Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetCSV();
        LocalizationManager.Instance.SetLanguageSetting();
    }

    private string ReadCSV() 
    {
        string filePath = Application.dataPath + "/Resources/Settings.csv";
        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        StreamReader streamReader = new StreamReader(fileStream);
        string result = streamReader.ReadToEnd();

        fileStream.Close();
        return result;
    }

    /// <summary>
    /// Оставим. Как ресет
    /// </summary>
    /// <param name="csvAsset"></param>
    public  void SetCSV()
    {
        string newFile = ReadCSV();
       /// string[] records = _csvAsset.text.Split(_lineSeperater);
        string[] records = newFile.Split(_lineSeperater);
        _settings = new string[records.Length, records[0].Split(_fieldSeperator).Length];
        for (int i = 0; i < records.Length - 1; i++)
        {
            string[] fields = records[i].Split(_fieldSeperator);
            if (fields[0] == "") continue;
            for (int j = 0; j < fields.Length; j++)
            {                
                _settings[i, j] = fields[j];
            }
        }
    }

    public  string GetParam(string tag) 
    {
        for(int i = 0;i< _settings.GetLength(0);i++) 
        {            
            if (_settings[i, 0] == tag)
            {
                return _settings[i, 1];
            }
        }
        return null;
    }

    public   void SetParam(string tag, string value)
    {
        for (int i = 0; i < _settings.GetLength(0); i++)
        {
            if (_settings[i, 0] == tag)
            {
                _settings[i, 1] = value;
            }
        }
    }

    public   void SafeCSV()
    {
        string line = "";
        for (int i = 0; i < _settings.GetLength(0); i++)
        {
            for (int j = 0; j < _settings.GetLength(1); j++)
            {
                if (j == 1) line = line + _settings[i, j];
                else line = line + _settings[i, j] + _fieldSeperator;
            }
            if(i<_settings.GetLength(0)-1) line = line + _lineSeperater;
        }
        File.WriteAllText(Application.dataPath + "/Resources/Settings.csv", line);
        Debug.Log("Настройки сохранены!");

    }

}
