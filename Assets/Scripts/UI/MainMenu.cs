using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider _sliderVolume;
    [SerializeField] private Slider _sliderSensitiviti;
    [SerializeField] private GameObject _settingMenu;
    [SerializeField] private Image topBackground;
    [SerializeField] private Image bottomBackground;
    [SerializeField] private Text flashingText;

    private Color _topImageColor;
    private bool _isFlashing = true;
    
    private void Start()
    {
        _topImageColor = topBackground.color;
        StartCoroutine(FlashText());
        AudioListener.volume = float.Parse(Settings.Instance.GetParam("volume"));
    }
    
    private void Update()
    {
        if (Input.anyKeyDown && _isFlashing)
        {
            StopAllCoroutines(); // Îñòàíàâëèâàåì ìèãàíèå òåêñòà
            StartCoroutine(FadeOutTopImage());
            flashingText.gameObject.SetActive(false);
            _isFlashing = false; // Âûêëþ÷àåì ÷òîáû êîðóòèíà íå ïîâòîðÿëà àíèìàöèþ
        }
    }

    private IEnumerator FlashText()
    {
        while (_isFlashing)
        {
            flashingText.color = Color.gray; // Äåëàåì öâåò òåêñòà ñåðûì
            yield return new WaitForSeconds(0.6f); // Æäåì 0.6 ñåêóíä
            flashingText.color = Color.white; // Âîçâðàùàåì öâåò òåêñòà ê áåëîìó
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator FadeOutTopImage()
    {
        float duration = 2f; // Äëèòåëüíîñòü àíèìàöèè
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // Âû÷èñëÿåì íîâóþ ïðîçðà÷íîñòü
            _topImageColor.a = alpha; // Óñòàíàâëèâàåì íîâóþ ïðîçðà÷íîñòü äëÿ âåðõíåãî èçîáðàæåíèÿ
            topBackground.color = _topImageColor; // Ïðèìåíÿåì öâåò ê èçîáðàæåíèþ

            yield return null; // Æäåì ñëåäóþùåãî êàäðà
        }

        // ïðîâåðêà ÷òî ïðîçðà÷íîñòü óñòàíîâëåíà â 0 ïîñëå çàâåðøåíèÿ
        _topImageColor.a = 0f;
        topBackground.color = _topImageColor;
        topBackground.gameObject.SetActive(false); // Âûêëþ÷àåì (íî âðîäå íå îáÿçàòåëüíî)
    }

    // Âåøàåì íà êíîïêó, ïî êîòîðîé ìåíÿåì ÿçûê
    public void ChangeLang()
    {
        LocalizationManager.Instance.Change();
    }

    public void OpenSetting()
    {
        _settingMenu.SetActive(true);
        _sliderVolume.value = float.Parse(Settings.Instance.GetParam("volume"));
        _sliderSensitiviti.value = float.Parse(Settings.Instance.GetParam("sensitivity"));
    }

    public void LoadScene(int sceneIndex)
    {
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }



    public void ChangeValueSound()
    {         
        Settings.Instance.SetParam("volume", _sliderVolume.value.ToString());
        AudioListener.volume = _sliderVolume.value;
    }

    public void ChangeValueSensetiviti()
    {
        Settings.Instance.SetParam("sensitivity", _sliderSensitiviti.value.ToString());
    }

    public void QuitGame()
    {
        LocalizationManager.Instance.SafeCSV();
        Settings.Instance.SafeCSV();
        //Profile.Instance.SafeCSV();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}
