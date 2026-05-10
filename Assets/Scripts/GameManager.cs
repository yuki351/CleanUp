using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int score = 0;
    private int highScore = 0;
    private bool pointsDoubled = false;

    [Header("Timer")]
    public float totalTime = 300f;
    private float timeRemaining;
    private bool timerRunning = false;
    private float totalElapsedTime = 0f;

    [Header("Time Periods")]
    private float periodDuration = 75f;
    private string[] periodNames = { "Morning", "Afternoon", "Evening", "Night" };
    private float[] multipliers = { 1f, 1f, 1.5f, 2f };
    private int currentPeriod = 0;

    [Header("Rewards")]
    private bool speedBoostActive = false;
    private float speedBoostTimer = 0f;
    private int rewardsGiven = 0;
    private int lastRewardScore = 0;

    [Header("Player")]
    public PlayerController playerController;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI periodText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI carryingText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject homePanel;

    void Awake()
    {
        Instance = this;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        timeRemaining = totalTime;

        if (homePanel != null) homePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        ShowHomeScreen();
        UpdateUI();
    }

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;
        totalElapsedTime += Time.deltaTime;

        UpdateTimePeriod();

        if (speedBoostActive)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                speedBoostActive = false;
                if (playerController != null)
                    playerController.moveSpeed = 5f;
                ShowReward("Speed Boost Ended!");
            }
        }

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            GameOver();
        }

        UpdateUI();
    }

    void UpdateTimePeriod()
    {
        int newPeriod = Mathf.Min((int)(totalElapsedTime / periodDuration), 3);
        if (newPeriod != currentPeriod)
        {
            currentPeriod = newPeriod;
            OnPeriodChanged();
        }
    }

    void OnPeriodChanged()
    {
        ShowReward(periodNames[currentPeriod] + 
           "! Multiplier: " + multipliers[currentPeriod] + "x");
    }

    public void OnPickupTrash(string color)
    {
        if (carryingText != null)
            carryingText.text = "Carrying: " + color.ToUpper() + " TRASH";
    }

    public void OnCorrectBin()
    {
        if (carryingText != null)
            carryingText.text = "";
        ShowReward("Correct Bin!");
    }

    public void OnWrongBin(string trashColor, string binColor)
    {
        ShowWrongBinWarning(trashColor);
    }

    void ShowWrongBinWarning(string trashColor)
    {
        if (rewardText != null)
        {
            rewardText.text = "WRONG BIN!\nThis is " + trashColor.ToUpper() + 
                  " trash!\nFind the " + trashColor.ToUpper() + " bin!";
            rewardText.color = Color.red;
            CancelInvoke("ClearWarning");
            Invoke("ClearWarning", 2.5f);
        }
    }

    void ClearWarning()
    {
        if (rewardText != null)
        {
            rewardText.text = "";
            rewardText.color = Color.white;
        }
    }

    public void AddScore(int basePoints)
    {
        int points = pointsDoubled ? basePoints * 2 : basePoints;
        float multiplier = multipliers[currentPeriod];
        int finalPoints = Mathf.RoundToInt(points * multiplier);
        score += finalPoints;

        CheckRewards();
        UpdateUI();
    }

    void CheckRewards()
    {
        int scoreSinceLastReward = score - lastRewardScore;

        if (scoreSinceLastReward >= 50 && !HasGivenReward(1))
        {
            timeRemaining += 30f;
            ShowReward("+30 Seconds Added!");
            MarkReward(1);
        }

        if (scoreSinceLastReward >= 100 && !HasGivenReward(2))
        {
            ActivateSpeedBoost();
            MarkReward(2);
        }

        if (scoreSinceLastReward >= 200 && !HasGivenReward(4))
        {
            pointsDoubled = true;
            ShowReward("Points DOUBLED!");
            MarkReward(4);
            lastRewardScore = score;
            rewardsGiven = 0;
        }
    }

    bool HasGivenReward(int level) => (rewardsGiven & level) != 0;
    void MarkReward(int level) => rewardsGiven |= level;

    void ActivateSpeedBoost()
    {
        speedBoostActive = true;
        speedBoostTimer = 15f;
        if (playerController != null)
            playerController.moveSpeed = 10f;
        ShowReward(" Speed Boost 15 Seconds!");
    }

    void ShowReward(string message)
    {
        if (rewardText != null)
        {
            rewardText.text = message;
            rewardText.color = Color.white;
            CancelInvoke("ClearRewardText");
            Invoke("ClearRewardText", 3f);
        }
        Debug.Log(message);
    }

    void ClearRewardText()
    {
        if (rewardText != null)
        {
            rewardText.text = "";
            rewardText.color = Color.white;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (periodText != null)
            periodText.text = periodNames[currentPeriod];

        if (multiplierText != null)
            multiplierText.text = multipliers[currentPeriod] + "x";
    }

    void GameOver()
{
    if (score > highScore)
    {
        highScore = score;
        PlayerPrefs.SetInt("HighScore", highScore);
    }

    if (gameOverPanel != null) gameOverPanel.SetActive(true);
    if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
    if (highScoreText != null) highScoreText.text = "High Score: " + highScore;

    // Unlock cursor for game over buttons
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Debug.Log("Game Over! Score: " + score);
}

   public void StartGame()
{
    score = 0;
    timeRemaining = totalTime;
    totalElapsedTime = 0f;
    currentPeriod = 0;
    pointsDoubled = false;
    speedBoostActive = false;
    rewardsGiven = 0;
    lastRewardScore = 0;
    timerRunning = true;

    if (homePanel != null) homePanel.SetActive(false);
    if (gameOverPanel != null) gameOverPanel.SetActive(false);

    // Lock cursor back for gameplay
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    UpdateUI();
}

    public void RestartGame() => StartGame();

    public void GoToMainMenu()
    {
        timerRunning = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        ShowHomeScreen();
    }

    void ShowHomeScreen()
{
    if (homePanel != null) homePanel.SetActive(true);
    if (highScoreText != null) 
        highScoreText.text = "High Score: " + highScore;
    
    // Unlock cursor so player can click buttons
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}
}