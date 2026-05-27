using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI References")]
    public RectTransform settingsPanel;
    public Button settingsToggleButton;
    public Button mainMenuButton;
    
    [Header("Sound Settings")]
    public Button soundButton;
    public Image soundIconImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("Vibration Settings")]
    public Button vibrationButton;
    public Image vibrationIconImage;
    public Sprite vibrationOnSprite;
    public Sprite vibrationOffSprite;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isPanelOpen = false;
    private Coroutine animCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (settingsPanel != null)
        {
            shownPos = settingsPanel.anchoredPosition;
            hiddenPos = shownPos + new Vector2(0, -settingsPanel.rect.height - 200f); 
            settingsPanel.anchoredPosition = hiddenPos;
            settingsPanel.gameObject.SetActive(false);
        }

        if (settingsToggleButton) settingsToggleButton.onClick.AddListener(TogglePanel);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (soundButton) soundButton.onClick.AddListener(ToggleSound);
        if (vibrationButton) vibrationButton.onClick.AddListener(ToggleVibration);

        LoadSettings();
    }

    private void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        
        if (isPanelOpen && settingsPanel != null)
        {
            settingsPanel.gameObject.SetActive(true);
        }

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePanel(isPanelOpen ? shownPos : hiddenPos));
    }

    private IEnumerator AnimatePanel(Vector2 targetPos)
    {
        if (settingsPanel == null) yield break;

        Vector2 startPos = settingsPanel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            float easeT = t * (2f - t);
            
            settingsPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }

        settingsPanel.anchoredPosition = targetPos;

        if (!isPanelOpen)
        {
            settingsPanel.gameObject.SetActive(false);
        }
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ToggleSound()
    {
        int isSoundOn = PlayerPrefs.GetInt("SoundOn", 1);
        isSoundOn = isSoundOn == 1 ? 0 : 1;
        PlayerPrefs.SetInt("SoundOn", isSoundOn);
        PlayerPrefs.Save();
        
        UpdateSoundVisuals(isSoundOn == 1);
        ApplySoundSettings(isSoundOn == 1);
    }

    private void ToggleVibration()
    {
        int isVibOn = PlayerPrefs.GetInt("VibrationOn", 1);
        isVibOn = isVibOn == 1 ? 0 : 1;
        PlayerPrefs.SetInt("VibrationOn", isVibOn);
        PlayerPrefs.Save();
        
        UpdateVibrationVisuals(isVibOn == 1);
        
        if (isVibOn == 1)
        {
            Handheld.Vibrate(); 
        }
    }

    private void LoadSettings()
    {
        bool isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        bool isVibOn = PlayerPrefs.GetInt("VibrationOn", 1) == 1;

        UpdateSoundVisuals(isSoundOn);
        UpdateVibrationVisuals(isVibOn);
        ApplySoundSettings(isSoundOn);
    }

    private void UpdateSoundVisuals(bool isOn)
    {
        if (soundIconImage != null)
        {
            soundIconImage.sprite = isOn ? soundOnSprite : soundOffSprite;
        }
    }

    private void UpdateVibrationVisuals(bool isOn)
    {
        if (vibrationIconImage != null)
        {
            vibrationIconImage.sprite = isOn ? vibrationOnSprite : vibrationOffSprite;
        }
    }

    private void ApplySoundSettings(bool isOn)
    {
        AudioListener.volume = isOn ? 1f : 0f;
    }

    public static void TriggerVibration()
    {
        if (PlayerPrefs.GetInt("VibrationOn", 1) == 1)
        {
            Handheld.Vibrate();
        }
    }
}
