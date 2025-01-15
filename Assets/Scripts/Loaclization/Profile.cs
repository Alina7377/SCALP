using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public  class Profile: MonoBehaviour
{
    private  char _fieldSeperator = ';';
    private  char _lineSeperater = '\n';
    private  string[,] _settings;
    private SProfileData _profileData  = new SProfileData();

    public static Profile Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
     

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetCSV();
    }

    private string ReadCSV() 
    {
        string filePath = Application.dataPath + "/Resources/Profile.csv";
        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

        StreamReader streamReader = new StreamReader(fileStream);

        string result = streamReader.ReadToEnd();

        fileStream.Close();
        return result;
    }

    public SProfileData GetProfilData => _profileData;

    public void SafeProfilDate(float timeGame, bool isWin)
    {
        SProfileData profile = _profileData;
        profile.TimeToGame += timeGame;
        if (isWin && (timeGame<profile.BestTime|| profile.BestTime==0))
            profile.BestTime = timeGame;
        if(isWin)
            profile.CountWin += 1f;
        _profileData = profile;
    }

    public void SetProfilParam(string profilField, string profilVal) 
    {
        SProfileData profile = _profileData;
        switch (profilField)
        {
            case "nikname":
                profile.UserName = profilVal;
                break;
            case "best_time":
                profile.BestTime = float.Parse(profilVal);
                break;
            case "count_game":
                profile.CountGame = float.Parse(profilVal);
                break;
            case "count_win_game":
                profile.CountWin = float.Parse(profilVal);
                break;
            case "time_to_game":
                profile.TimeToGame = float.Parse(profilVal);
                break;
        }
        _profileData = profile;
    }

    public string GetProfilParamSTR(string profilField)
    {
        switch (profilField)
        {
            case "nikname":
                return _profileData.UserName;
            case "best_time":
                return _profileData.BestTime.ToString();
            case "count_game":
                return _profileData.CountGame.ToString();
            case "count_win_game":
                return _profileData.CountWin.ToString();
            case "time_to_game":
                return _profileData.TimeToGame.ToString();
        }
        return null;
    }

    public float GetProfilParamF(string profilField)
    {
        switch (profilField)
        {
            case "best_time":
                return _profileData.BestTime;
            case "count_game":
                return _profileData.CountGame;
            case "count_win_game":
                return _profileData.CountWin;
            case "time_to_game":
                return _profileData.TimeToGame;
        }
        return -1;
    }

    /// <summary>
    /// Оставим. Как ресет
    /// </summary>
    /// <param name="csvAsset"></param>
    public  void SetCSV()
    {
        string newFile = ReadCSV();
        string[] records = newFile.Split(_lineSeperater);
        for (int i = 0; i < records.Length - 1; i++)
        {
            string[] fields = records[i].Split(_fieldSeperator);
            if (fields[0] == "") continue;
            SetProfilParam(fields[0], fields[1]);
        }
    }

    public void SafeCSV()
    {
        string line = "";
        line = "nikname"         + _fieldSeperator + _profileData.UserName + _fieldSeperator + _lineSeperater;
        line += "best_time"      + _fieldSeperator + _profileData.BestTime.ToString()   + _fieldSeperator + _lineSeperater;
        line += "count_game"     + _fieldSeperator + _profileData.CountGame.ToString()  + _fieldSeperator + _lineSeperater;
        line += "count_win_game" + _fieldSeperator + _profileData.CountWin.ToString()   + _fieldSeperator + _lineSeperater;
        line += "time_to_game"   + _fieldSeperator + _profileData.TimeToGame.ToString() + _fieldSeperator + _lineSeperater;
        File.WriteAllText(Application.dataPath + "/Resources/Profile.csv", line);

    }

    public void ResetProfilData() 
    {
        SProfileData profile = _profileData;
        profile.CountWin = 0;
        profile.CountGame = 0;
        profile.TimeToGame = 0;
        profile.BestTime = 0;
        _profileData = profile;
        SafeCSV();
        List<string> tags = LocalizationManager.Instance.GetTagList("t", true);
        foreach (var tag in tags)
        {
            LocalizationManager.Instance.WriteAvailForTag(tag, "f");
        }
        tags = LocalizationManager.Instance.GetTagList("n", true);
        foreach (var tag in tags)
        {
            LocalizationManager.Instance.WriteAvailForTag(tag, "f");
        }
        LocalizationManager.Instance.SetProgress();
        LocalizationManager.Instance.SafeCSV();
    }

}
