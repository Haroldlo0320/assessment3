using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [SerializeField] public Text scoreText;
    [SerializeField] public Text timerText;
    [SerializeField] public Text ghostTimerText;
    [SerializeField] public Text levelNameText;
    [SerializeField] public Image[] lifeImages;
    [SerializeField] public Button exitButton;

    [SerializeField] public Text countdownText;
    [SerializeField] public Image blockingOverlay;
    [SerializeField] public Text gameOverText;

    private int currentScore = 0;
    private int lives = 3;
    private float gameTimer = 0f;
    private bool isGameActive = false;

    private float scaredDuration = 0f;
    private bool isScaredActive = false;

    private PacStudentController player;
    private GhostController[] ghosts;

    private void Start()
    {
        player = FindAnyObjectByType<PacStudentController>();
        ghosts = FindObjectsByType<GhostController>(FindObjectsInactive.Exclude);

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        UpdateScoreUI();
        UpdateLivesUI();
        UpdateTimerUI();

        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        StartCoroutine(RoundStartRoutine());
    }

    private void Update()
    {
        if (isGameActive)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerUI();
        }

        if (isScaredActive)
        {
            scaredDuration -= Time.deltaTime;
            if (scaredDuration <= 0f)
            {
                EndScaredState();
            }
            else
            {
                if (ghostTimerText != null)
                {
                    ghostTimerText.text = Mathf.CeilToInt(scaredDuration).ToString();
                }

                if (scaredDuration <= 3f)
                {
                    foreach (GhostController g in ghosts)
                    {
                        if (g != null && g.GetState() == GhostController.GhostState.Scared)
                        {
                            g.SetState(GhostController.GhostState.Recovering);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator RoundStartRoutine()
    {
        isGameActive = false;
        if (player != null) player.SetControlEnabled(false);
        foreach (GhostController g in ghosts) if (g != null) g.SetMovementEnabled(false);

        if (blockingOverlay != null) blockingOverlay.gameObject.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        if (countdownText != null) countdownText.text = "3";
        yield return new WaitForSeconds(1.0f);

        if (countdownText != null) countdownText.text = "2";
        yield return new WaitForSeconds(1.0f);

        if (countdownText != null) countdownText.text = "1";
        yield return new WaitForSeconds(1.0f);

        if (countdownText != null) countdownText.text = "GO!";
        yield return new WaitForSeconds(1.0f);

        if (blockingOverlay != null) blockingOverlay.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        isGameActive = true;
        if (player != null) player.SetControlEnabled(true);
        foreach (GhostController g in ghosts) if (g != null) g.SetMovementEnabled(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostsNormalMusic();
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString("D6");
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer % 60F);
            int milliseconds = Mathf.FloorToInt((gameTimer * 100F) % 100F);
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }

    private void UpdateLivesUI()
    {
        if (lifeImages != null)
        {
            for (int i = 0; i < lifeImages.Length; i++)
            {
                if (lifeImages[i] != null)
                {
                    lifeImages[i].gameObject.SetActive(i < lives);
                }
            }
        }
    }

    public void TriggerPowerPellet()
    {
        AddScore(50);
        isScaredActive = true;
        scaredDuration = 10.0f;

        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(true);
            ghostTimerText.text = "10";
        }

        foreach (GhostController g in ghosts)
        {
            if (g != null && g.GetState() != GhostController.GhostState.Dead)
            {
                g.SetState(GhostController.GhostState.Scared);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostsScaredMusic();
        }
    }

    private void EndScaredState()
    {
        isScaredActive = false;
        scaredDuration = 0f;

        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(false);
        }

        foreach (GhostController g in ghosts)
        {
            if (g != null && g.GetState() != GhostController.GhostState.Dead)
            {
                g.SetState(GhostController.GhostState.Normal);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostsNormalMusic();
        }
    }

    public bool IsGhostScaredActive()
    {
        return isScaredActive;
    }

    public bool IsGhostRecovering()
    {
        return isScaredActive && scaredDuration <= 3.0f;
    }

    public void OnPlayerHitGhost(GhostController ghost)
    {
        if (ghost == null) return;

        if (ghost.GetState() == GhostController.GhostState.Scared || ghost.GetState() == GhostController.GhostState.Recovering)
        {
            ghost.SetState(GhostController.GhostState.Dead);
            AddScore(300);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEatGhostSFX();
                AudioManager.Instance.PlayGhostDeadMusic();
            }
        }
        else if (ghost.GetState() == GhostController.GhostState.Normal)
        {
            OnPlayerDied();
        }
    }

    private void OnPlayerDied()
    {
        lives--;
        UpdateLivesUI();

        if (player != null) player.SetControlEnabled(false);
        foreach (GhostController g in ghosts) if (g != null) g.SetMovementEnabled(false);

        if (lives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            if (player != null)
            {
                player.Die(() =>
                {
                    foreach (GhostController g in ghosts)
                    {
                        if (g != null) g.ResetGhost();
                    }
                    if (player != null) player.SetControlEnabled(true);
                    foreach (GhostController g in ghosts) if (g != null) g.SetMovementEnabled(true);
                });
            }
        }
    }

    public void CheckPelletsRemaining()
    {
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
        if (pellets == null || pellets.Length <= 1)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameActive = false;
        if (player != null) player.SetControlEnabled(false);
        foreach (GhostController g in ghosts) if (g != null) g.SetMovementEnabled(false);

        if (blockingOverlay != null) blockingOverlay.gameObject.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Game Over";
        }

        SaveHighScore();
        StartCoroutine(ReturnToStartSceneRoutine());
    }

    private void SaveHighScore()
    {
        string scoreKey = "Level1_HighScore";
        string timeKey = "Level1_BestTime";

        int prevScore = PlayerPrefs.GetInt(scoreKey, 0);
        float prevTime = PlayerPrefs.GetFloat(timeKey, 0f);

        if (currentScore > prevScore)
        {
            PlayerPrefs.SetInt(scoreKey, currentScore);
            PlayerPrefs.SetFloat(timeKey, gameTimer);
            PlayerPrefs.Save();
        }
        else if (currentScore == prevScore && (prevTime <= 0f || gameTimer < prevTime))
        {
            PlayerPrefs.SetFloat(timeKey, gameTimer);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator ReturnToStartSceneRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("StartScene");
    }

    public void OnExitClicked()
    {
        SceneManager.LoadScene("StartScene");
    }
}

