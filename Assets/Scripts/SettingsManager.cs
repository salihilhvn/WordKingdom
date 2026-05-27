using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

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
    public float animationDuration = 0.5f;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private Vector3 originalPanelScale = Vector3.one;
    private bool isPanelOpen = false;

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
            originalPanelScale = settingsPanel.localScale;
            // Artık ekranın dışından değil, direkt ayarlar butonunun olduğu yerden çıkacak
            if (settingsToggleButton != null)
            {
                hiddenPos = settingsToggleButton.GetComponent<RectTransform>().anchoredPosition;
            }
            else
            {
                hiddenPos = shownPos + new Vector2(0, -200f);
            }
            settingsPanel.anchoredPosition = hiddenPos;
            settingsPanel.localScale = Vector3.zero; // Başlangıçta 0 boyutunda
            settingsPanel.gameObject.SetActive(false);
        }

        // Eğer kullanıcı Inspector'da Image objelerini sürüklemeyi unuttuysa butonun kendi resmini al
        if (soundIconImage == null && soundButton != null) soundIconImage = soundButton.GetComponent<Image>();
        if (vibrationIconImage == null && vibrationButton != null) vibrationIconImage = vibrationButton.GetComponent<Image>();

        // Eğer kullanıcı On (Açık) resimlerini sürüklemeyi unuttuysa, UI'daki varsayılan resmi "On" resmi olarak kabul et
        if (soundOnSprite == null && soundIconImage != null) soundOnSprite = soundIconImage.sprite;
        if (vibrationOnSprite == null && vibrationIconImage != null) vibrationOnSprite = vibrationIconImage.sprite;

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

        // Ayarlar butonunu tatlı bir şekilde döndür
        if (settingsToggleButton != null)
        {
            float targetZ = isPanelOpen ? -180f : 0f;
            settingsToggleButton.transform.DORotate(new Vector3(0, 0, targetZ), animationDuration, RotateMode.Fast)
                .SetEase(Ease.OutBack);
        }

        if (settingsPanel != null)
        {
            // Eğer halihazırda çalan bir animasyon varsa durdur ki üst üste binmesin
            settingsPanel.DOKill();

            if (isPanelOpen)
            {
                settingsPanel.anchoredPosition = hiddenPos;
                settingsPanel.localScale = Vector3.zero; // Butonun içinden küçücükten başlayıp büyüyecek

                // Hem pozisyon olarak yerine gitsin hem de büyüsün (Juicy)
                settingsPanel.DOAnchorPos(shownPos, animationDuration).SetEase(Ease.OutBack);
                settingsPanel.DOScale(originalPanelScale, animationDuration).SetEase(Ease.OutBack);
            }
            else
            {
                // Çarkın içine doğru küçülerek geri dönsün
                settingsPanel.DOAnchorPos(hiddenPos, animationDuration * 0.8f).SetEase(Ease.InBack);
                settingsPanel.DOScale(Vector3.zero, animationDuration * 0.8f).SetEase(Ease.InBack)
                    .OnComplete(() => settingsPanel.gameObject.SetActive(false));
            }
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
            Sprite targetSprite = isOn ? soundOnSprite : soundOffSprite;
            if (targetSprite != null) soundIconImage.sprite = targetSprite;
        }
    }

    private void UpdateVibrationVisuals(bool isOn)
    {
        if (vibrationIconImage != null)
        {
            Sprite targetSprite = isOn ? vibrationOnSprite : vibrationOffSprite;
            if (targetSprite != null) vibrationIconImage.sprite = targetSprite;
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
