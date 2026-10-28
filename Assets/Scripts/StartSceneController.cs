using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] public Button level1Button;
    [SerializeField] public Button level2Button;
    [SerializeField] public Text level1HighScoreText;
    [SerializeField] public Text level1BestTimeText;
    [SerializeField] public Text level2HighScoreText;
    [SerializeField] public Text level2BestTimeText;

    private void Start()
    {
        if (level1Button != null)
        {
            level1Button.onClick.AddListener(OnLevel1Clicked);
        }

        if (level2Button != null)
        {
            level2Button.onClick.AddListener(OnLevel2Clicked);
        }

        LoadScores();
    }

    private void LoadScores()
    {
        int l1Score = PlayerPrefs.GetInt("Level1_HighScore", 0);
        float l1Time = PlayerPrefs.GetFloat("Level1_BestTime", 0f);

        if (level1HighScoreText != null)
        {
            level1HighScoreText.text = l1Score.ToString("D6");
        }

        if (level1BestTimeText != null)
        {
            level1BestTimeText.text = FormatTime(l1Time);
        }

        int l2Score = PlayerPrefs.GetInt("Level2_HighScore", 0);
        float l2Time = PlayerPrefs.GetFloat("Level2_BestTime", 0f);

        if (level2HighScoreText != null)
        {
            level2HighScoreText.text = l2Score.ToString("D6");
        }

        if (level2BestTimeText != null)
        {
            level2BestTimeText.text = FormatTime(l2Time);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100F) % 100F);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void OnLevel1Clicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnLevel2Clicked()
    {
        if (Application.CanStreamedLevelBeLoaded("InnovationScene"))
        {
            SceneManager.LoadScene("InnovationScene");
        }
    }
}

