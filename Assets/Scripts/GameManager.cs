using UnityEngine;
using TMPro;
using Tenkoku.Core;
using UnityEngine.SceneManagement;

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
    private bool timerRunning = true;
    private float totalElapsedTime = 0f;

    [Header("Time Periods")]
    private string[] periodNames = { "Morning", "Afternoon", "Evening", "Night" };
    private float[] multipliers = { 1f, 1f, 1.5f, 2f };
    private int currentPeriod = 0;

    [Header("Rewards")]
    private bool speedBoostActive = false;
    private float speedBoostTimer = 0f;
    private int rewardsGiven = 0;

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

    [Header("Tenkoku")]
    public TenkokuModule tenK;

    void Awake()
    {
        Instance = this;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        timeRemaining = totalTime;
        totalElapsedTime = 0f;
        currentPeriod = 0;
        timerRunning = true;

        tenK.currentHour = 6;
        tenK.currentMinute = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;
        totalElapsedTime += Time.deltaTime;

        UpdateTimePeriod();
        HandleSpeedBoost();

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            GameOver();
        }

        UpdateUI();
    }

    // ================= DAY SYSTEM =================
// ================= DAY SYSTEM =================
void UpdateTimePeriod()
{
    int newPeriod;
    int targetHour;
    int targetMinute;

    if (totalElapsedTime < 75f)
    {
        newPeriod = 0;
        // Morning: 6:00 to 11:59
        float t = totalElapsedTime / 75f;
        targetHour = 6 + Mathf.FloorToInt(t * 6);
        targetMinute = Mathf.FloorToInt((t * 6 - Mathf.Floor(t * 6)) * 60);
    }
    else if (totalElapsedTime < 150f)
    {
        newPeriod = 1;
        // Afternoon: 12:00 to 16:59
        float t = (totalElapsedTime - 75f) / 75f;
        targetHour = 12 + Mathf.FloorToInt(t * 5);
        targetMinute = Mathf.FloorToInt((t * 5 - Mathf.Floor(t * 5)) * 60);
    }
    else if (totalElapsedTime < 225f)
    {
        newPeriod = 2;
        // Evening: 17:00 to 19:59
        float t = (totalElapsedTime - 150f) / 75f;
        targetHour = 17 + Mathf.FloorToInt(t * 3);
        targetMinute = Mathf.FloorToInt((t * 3 - Mathf.Floor(t * 3)) * 60);
    }
    else
    {
        newPeriod = 3;
        // Night: 20:00 to 23:59
        float t = (totalElapsedTime - 225f) / 75f;
        targetHour = 20 + Mathf.FloorToInt(t * 4);
        targetMinute = Mathf.FloorToInt((t * 4 - Mathf.Floor(t * 4)) * 60);
    }

    if (newPeriod != currentPeriod)
    {
        currentPeriod = newPeriod;
        Debug.Log("Period changed to: " + periodNames[currentPeriod] + " | Elapsed: " + totalElapsedTime);
    }

    tenK.currentHour = targetHour;
    tenK.currentMinute = targetMinute;
    
    // Set brightness based on time of day
    SetBrightnessForTime();
}

// This method goes OUTSIDE of UpdateTimePeriod
void SetBrightnessForTime()
{
    if (currentPeriod == 3) // Night time
        RenderSettings.ambientIntensity = 0.5f;
    else // Day time (Morning, Afternoon, Evening)
        RenderSettings.ambientIntensity = 0.8f;
}

    // ================= SCORE =================
    public void AddScore(int basePoints)
    {
        int points = pointsDoubled ? basePoints * 2 : basePoints;
        float multiplier = multipliers[currentPeriod];
        score += Mathf.RoundToInt(points * multiplier);

        CheckRewards();
    }

    // ================= REWARDS =================
    void CheckRewards()
    {
        if (score >= 50 && !HasGivenReward(1))
        {
            timeRemaining += 30f;
            tenK.currentHour = 6;
            tenK.currentMinute = 0;
            ShowReward("+30 Seconds!");
            MarkReward(1);
        }

        if (score >= 100 && !HasGivenReward(2))
        {
            ActivateSpeedBoost();
            MarkReward(2);
        }

        if (score >= 200 && !HasGivenReward(4))
        {
            pointsDoubled = true;
            ShowReward("Points DOUBLED!");
            MarkReward(4);
        }
    }

    bool HasGivenReward(int level) => (rewardsGiven & level) != 0;
    void MarkReward(int level) => rewardsGiven |= level;

    // ================= SPEED BOOST =================
    void ActivateSpeedBoost()
    {
        speedBoostActive = true;
        speedBoostTimer = 15f;

        if (playerController != null)
            playerController.moveSpeed *= 2f;

        ShowReward("Speed Boost 15 Seconds!");
    }

    void HandleSpeedBoost()
    {
        if (!speedBoostActive) return;

        speedBoostTimer -= Time.deltaTime;

        if (speedBoostTimer <= 0f)
        {
            speedBoostActive = false;

            if (playerController != null)
                playerController.moveSpeed /= 2f;

            ShowReward("Speed Boost Ended!");
        }
    }

    // ================= PLAYER INTERACTIONS =================
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
        if (rewardText != null)
        {
            rewardText.text = "WRONG BIN!";
            CancelInvoke("ClearReward");
            Invoke("ClearReward", 2f);
        }
    }

    void ClearReward()
    {
        if (rewardText != null)
            rewardText.text = "";
    }

    // ================= UI =================
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (timerText != null)
        {
            int m = Mathf.FloorToInt(timeRemaining / 60);
            int s = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{m:00}:{s:00}";
        }

        if (periodText != null)
            periodText.text = periodNames[currentPeriod];

        if (multiplierText != null)
            multiplierText.text = multipliers[currentPeriod] + "x";
    }

    void ShowReward(string msg)
    {
        if (rewardText != null)
            rewardText.text = msg;

        CancelInvoke("ClearReward");
        Invoke("ClearReward", 2f);
    }

    // ================= GAME OVER =================
    void GameOver()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ================= BUTTONS =================
    public void PlayAgain()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("UIManager");
    }
}

