using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiderPin : MonoBehaviour
{
    [SerializeField] private Text _ratingNum;
    [SerializeField] private Text _userName;
    [SerializeField] private Text _bestTime;
    [SerializeField] private Text _countWin;
    [SerializeField] private Text _winRate;

    private string TimeFormat(float timeFloat) 
    {
        TimeSpan time = TimeSpan.FromSeconds(timeFloat);
        return time.ToString("hh':'mm':'ss");
    }

    public void DisplayInfo(SProfileData record, int num) 
    {
        if (num > -1)
            _ratingNum.text = num.ToString();
        else _ratingNum.text = "-";
        _userName.text = record.UserName;
        _bestTime.text = TimeFormat(record.BestTime);
        _countWin.text = record.CountWin.ToString();
        if (record.CountGame == 0)
            _winRate.text = "0";
        else
            _winRate.text = MathF.Round(record.CountWin / record.CountGame * 100).ToString();
    }
   
}
