using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InnovationController : MonoBehaviour
{
    [SerializeField] public Button speedBoostButton;
    [SerializeField] public Button returnMenuButton;
    [SerializeField] public Text innovationScoreText;
    [SerializeField] public Text innovationTimerText;

    private int score = 0;
    private float timer = 0f;
    private bool isBoostActive = false;
    private float boostDuration = 0f;

    private void Start()
    {
        if (speedBoostButton != null)
        {
            speedBoostButton.onClick.AddListener(OnSpeedBoostClicked);
        }

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.AddListener(OnReturnMenuClicked);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (innovationTimerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer % 60F);
            int ms = Mathf.FloorToInt((timer * 100F) % 100F);
            innovationTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, ms);
        }

        if (isBoostActive)
        {
            boostDuration -= Time.deltaTime;
            if (boostDuration <= 0f)
            {
                isBoostActive = false;
            }
        }
    }

    public void AddInnovationScore(int pts)
    {
        score += pts;
        if (innovationScoreText != null)
        {
            innovationScoreText.text = score.ToString("D6");
        }
        SaveInnovationHigh();
    }

    private void SaveInnovationHigh()
    {
        int prev = PlayerPrefs.GetInt("Level2_HighScore", 0);
        float prevTime = PlayerPrefs.GetFloat("Level2_BestTime", 0f);

        if (score > prev)
        {
            PlayerPrefs.SetInt("Level2_HighScore", score);
            PlayerPrefs.SetFloat("Level2_BestTime", timer);
            PlayerPrefs.Save();
        }
        else if (score == prev && (prevTime <= 0f || timer < prevTime))
        {
            PlayerPrefs.SetFloat("Level2_BestTime", timer);
            PlayerPrefs.Save();
        }
    }

    public void OnSpeedBoostClicked()
    {
        isBoostActive = true;
        boostDuration = 5.0f;
        AddInnovationScore(50);
    }

    public void OnReturnMenuClicked()
    {
        SaveInnovationHigh();
        SceneManager.LoadScene("StartScene");
    }
}
