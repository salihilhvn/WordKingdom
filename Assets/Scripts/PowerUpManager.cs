using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Power-Up Buttons")]
    public Button tipButton;
    public Button wordyButton;
    public Button extraTimeButton;
    public Button coinGlazeButton;

    [Header("Tutorial Panel UI")]
    public GameObject tutorialPanel;
    public Image powerUpIconImage;
    public TMP_Text powerUpNameText;
    public TMP_Text powerUpDescText;
    public Button okayButton;
    
    [Header("Tutorial Icons")]
    public Sprite tipIcon;
    public Sprite wordyIcon;
    public Sprite extraTimeIcon;
    public Sprite coinGlazeIcon;

    [Header("Coin Glaze State")]
    public bool isCoinGlazeActive = false;
    private float coinGlazeTimer = 0f;
    private Coroutine coinGlazeCoroutine;

    // Which powerup is currently being tutorialized
    private string currentTutorialPowerUp = "";
    public bool isTutorialActive = false;

    private WordSelectionManager selectionManager;
    private WordGridManager gridManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        selectionManager = FindAnyObjectByType<WordSelectionManager>();
        gridManager = FindAnyObjectByType<WordGridManager>();

        if (tutorialPanel) tutorialPanel.SetActive(false);

        if (okayButton) okayButton.onClick.AddListener(OnOkayClicked);

        if (tipButton) tipButton.onClick.AddListener(UseTip);
        if (wordyButton) wordyButton.onClick.AddListener(UseWordy);
        if (extraTimeButton) extraTimeButton.onClick.AddListener(UseExtraTime);
        if (coinGlazeButton) coinGlazeButton.onClick.AddListener(UseCoinGlaze);

        UpdateButtonVisibility();
    }

    public void UpdateButtonVisibility()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        
        // 0-based index levels (Level 4 -> index 3, Level 8 -> index 7, Level 12 -> index 11, Level 14 -> index 13)
        // Adjust these numbers based on 0-based or 1-based index logic. User said: 
        // Level 4 (index 3), Level 8 (index 7), Level 12 (index 11), Level 14 (index 13).
        if (tipButton) tipButton.gameObject.SetActive(unlockedLevel >= 3);
        if (wordyButton) wordyButton.gameObject.SetActive(unlockedLevel >= 7);
        if (extraTimeButton) extraTimeButton.gameObject.SetActive(unlockedLevel >= 11);
        if (coinGlazeButton) coinGlazeButton.gameObject.SetActive(unlockedLevel >= 13);
    }

    public void CheckTutorials(int currentLevelIndex)
    {
        UpdateButtonVisibility();
        
        // Level 4 -> index 3
        if (currentLevelIndex == 3 && PlayerPrefs.GetInt("Tutorial_Tip_Done", 0) == 0)
        {
            ShowTutorial("Tip", tipIcon, "TIP", "This power-up uncovers 1 letter of a word.");
        }
        else if (currentLevelIndex == 7 && PlayerPrefs.GetInt("Tutorial_Wordy_Done", 0) == 0)
        {
            ShowTutorial("Wordy", wordyIcon, "WORDY", "This power-up uncovers a whole word completely.");
        }
        else if (currentLevelIndex == 11 && PlayerPrefs.GetInt("Tutorial_ExtraTime_Done", 0) == 0)
        {
            ShowTutorial("ExtraTime", extraTimeIcon, "EXTRA TIME", "This power-up adds +20 seconds to your remaining time.");
        }
        else if (currentLevelIndex == 13 && PlayerPrefs.GetInt("Tutorial_CoinGlaze_Done", 0) == 0)
        {
            ShowTutorial("CoinGlaze", coinGlazeIcon, "COIN GLAZE", "This power-up multiplies your coin earnings by 2x for 30 seconds.");
        }
        else
        {
            // Resume normally
            isTutorialActive = false;
            UIManager.Instance.ResumeTimer();
        }
    }

    private void ShowTutorial(string powerUpID, Sprite icon, string nameStr, string desc)
    {
        currentTutorialPowerUp = powerUpID;
        isTutorialActive = true;
        
        // Pause timer & interaction
        UIManager.Instance.PauseTimer();

        if (powerUpIconImage && icon != null) powerUpIconImage.sprite = icon;
        if (powerUpNameText) powerUpNameText.text = nameStr;
        if (powerUpDescText) powerUpDescText.text = desc;

        if (tutorialPanel) tutorialPanel.SetActive(true);

        // Make powerup buttons uninteractable initially
        SetPowerUpButtonsInteractable(false);
    }

    private void OnOkayClicked()
    {
        if (tutorialPanel) tutorialPanel.SetActive(false);

        // Highlight the forced powerup button
        SetPowerUpButtonsInteractable(false);
        
        switch (currentTutorialPowerUp)
        {
            case "Tip":
                if (tipButton) { tipButton.interactable = true; StartCoroutine(HighlightButton(tipButton.transform)); }
                break;
            case "Wordy":
                if (wordyButton) { wordyButton.interactable = true; StartCoroutine(HighlightButton(wordyButton.transform)); }
                break;
            case "ExtraTime":
                if (extraTimeButton) { extraTimeButton.interactable = true; StartCoroutine(HighlightButton(extraTimeButton.transform)); }
                break;
            case "CoinGlaze":
                if (coinGlazeButton) { coinGlazeButton.interactable = true; StartCoroutine(HighlightButton(coinGlazeButton.transform)); }
                break;
        }
    }

    private IEnumerator HighlightButton(Transform buttonTrans)
    {
        float elapsed = 0f;
        Vector3 origScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
        
        // Blink scale
        while (isTutorialActive)
        {
            elapsed += Time.deltaTime;
            float pingPong = Mathf.PingPong(elapsed * 2f, 1f);
            buttonTrans.localScale = Vector3.Lerp(origScale, targetScale, pingPong);
            yield return null;
        }
        
        buttonTrans.localScale = origScale;
    }

    private void SetPowerUpButtonsInteractable(bool state)
    {
        if (tipButton) tipButton.interactable = state;
        if (wordyButton) wordyButton.interactable = state;
        if (extraTimeButton) extraTimeButton.interactable = state;
        if (coinGlazeButton) coinGlazeButton.interactable = state;
    }

    private void FinishTutorial(string powerUpID)
    {
        if (isTutorialActive && currentTutorialPowerUp == powerUpID)
        {
            PlayerPrefs.SetInt("Tutorial_" + powerUpID + "_Done", 1);
            PlayerPrefs.Save();
            
            isTutorialActive = false;
            SetPowerUpButtonsInteractable(true);
            UIManager.Instance.ResumeTimer();
        }
    }

    public void UseTip()
    {
        if (selectionManager == null || gridManager == null) return;
        
        // Find an unfound word
        string targetWord = GetRandomUnfoundWord();
        if (string.IsNullOrEmpty(targetWord)) return;

        // Find its placement
        WordPlacement placement = GetPlacementForWord(targetWord);
        if (placement == null) return;

        // Find an unsolved letter tile for this word
        LetterTile targetTile = null;
        for (int i = 0; i < targetWord.Length; i++)
        {
            int nx = placement.startX + (i * placement.dx);
            int ny = placement.startY + (i * placement.dy);
            
            if (nx >= 0 && nx < gridManager.columns && ny >= 0 && ny < gridManager.rows)
            {
                LetterTile tile = gridManager.gridTiles[nx, ny];
                if (tile != null && !tile.isSolved)
                {
                    targetTile = tile;
                    break;
                }
            }
        }

        if (targetTile != null)
        {
            targetTile.HighlightAsPowerUp(0f);
        }

        FinishTutorial("Tip");
    }

    public void UseWordy()
    {
        if (selectionManager == null || gridManager == null) return;
        
        string targetWord = GetRandomUnfoundWord();
        if (string.IsNullOrEmpty(targetWord)) return;

        WordPlacement placement = GetPlacementForWord(targetWord);
        if (placement == null) return;

        List<LetterTile> wordTiles = new List<LetterTile>();
        
        for (int i = 0; i < targetWord.Length; i++)
        {
            int nx = placement.startX + (i * placement.dx);
            int ny = placement.startY + (i * placement.dy);
            
            if (nx >= 0 && nx < gridManager.columns && ny >= 0 && ny < gridManager.rows)
            {
                LetterTile tile = gridManager.gridTiles[nx, ny];
                if (tile != null)
                {
                    wordTiles.Add(tile);
                    tile.HighlightAsPowerUp(i * 0.05f); // Dalga efekti
                }
            }
        }

        // Add to found words and trigger logic
        selectionManager.foundWords.Add(targetWord);
        UIManager.Instance.CrossOutWord(targetWord);
        UIManager.Instance.AnimateCoinsFromTiles(wordTiles);
        
        // Need to check level completion from selectionManager
        selectionManager.CheckLevelCompletionFromExternal();

        FinishTutorial("Wordy");
    }

    public void UseExtraTime()
    {
        UIManager.Instance.AddExtraTime(20f);
        FinishTutorial("ExtraTime");
    }

    public void UseCoinGlaze()
    {
        if (coinGlazeCoroutine != null) StopCoroutine(coinGlazeCoroutine);
        coinGlazeCoroutine = StartCoroutine(CoinGlazeRoutine());
        FinishTutorial("CoinGlaze");
    }

    private IEnumerator CoinGlazeRoutine()
    {
        isCoinGlazeActive = true;
        coinGlazeTimer = 30f;
        
        while (coinGlazeTimer > 0)
        {
            coinGlazeTimer -= Time.deltaTime;
            yield return null;
        }

        isCoinGlazeActive = false;
    }

    private string GetRandomUnfoundWord()
    {
        List<string> unfound = new List<string>();
        foreach (string w in selectionManager.targetWords)
        {
            if (!selectionManager.foundWords.Contains(w))
            {
                unfound.Add(w);
            }
        }

        if (unfound.Count > 0)
        {
            return unfound[Random.Range(0, unfound.Count)];
        }
        return null;
    }

    private WordPlacement GetPlacementForWord(string word)
    {
        if (gridManager.levels.Count > gridManager.currentLevelIndex)
        {
            LevelConfig config = gridManager.levels[gridManager.currentLevelIndex];
            foreach (var p in config.wordPlacements)
            {
                if (p.word == word) return p;
            }
        }
        return null;
    }
}
