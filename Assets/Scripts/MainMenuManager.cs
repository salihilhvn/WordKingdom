using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; // DOTween kütüphanesi eklendi

public class MainMenuManager : MonoBehaviour
{
    [Header("Top Buttons")]
    public Button settingsButton;
    public Button helpButton;

    [Header("Center Buttons")]
    public Button playButton;
    public Button levelsButton;

    [Header("Bottom Nav Buttons")]
    public Button homeButton;
    public Button storeButton;
    public Button rankButton;
    public Button collectionButton;

    [Header("Signifier Settings (Kayma Efekti)")]
    public RectTransform activeSignifier;
    public float slideDuration = 0.35f; // Kayma süresi

    [Header("Levels Panel")]
    public LevelsPanelManager levelsPanelManager;

    // Her butonun orijinal boyutunu aklımızda tutmak için sözlük
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();

    private void Start()
    {
        // Üst Butonlar
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (helpButton != null) helpButton.onClick.AddListener(OnHelpClicked);

        // Orta Butonlar
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (levelsButton != null) levelsButton.onClick.AddListener(OnLevelsClicked);

        // Alt Navigasyon Butonları
        if (homeButton != null) homeButton.onClick.AddListener(() => SwitchTab(homeButton.GetComponent<RectTransform>()));
        if (storeButton != null) storeButton.onClick.AddListener(() => SwitchTab(storeButton.GetComponent<RectTransform>()));
        if (rankButton != null) rankButton.onClick.AddListener(() => SwitchTab(rankButton.GetComponent<RectTransform>()));
        if (collectionButton != null) collectionButton.onClick.AddListener(() => SwitchTab(collectionButton.GetComponent<RectTransform>()));

        // Başlangıçta Home seçili gelsin
        if (activeSignifier != null && homeButton != null)
        {
            Vector3 startPos = homeButton.GetComponent<RectTransform>().position;
            activeSignifier.position = new Vector3(startPos.x, activeSignifier.position.y, activeSignifier.position.z);
        }
    }

    private void OnPlayClicked()
    {
        // Direk Play'e basarsa kalınan en son bölümü (Current_Box) açsın
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        PlayerPrefs.SetInt("SelectedLevel", unlockedLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Gameplay"); 
    }

    private void OnLevelsClicked() 
    {
        if (levelsPanelManager != null)
        {
            levelsPanelManager.OpenPanel();
        }
    }
    
    private void OnSettingsClicked() {}
    private void OnHelpClicked() {}

    private void SwitchTab(RectTransform targetButtonRect)
    {
        if (activeSignifier == null || targetButtonRect == null) return;

        // DOTween ile pürüzsüz kayma (Yaylanma hissi için OutBack kullanıyoruz)
        // Coroutine'den tek satırlık enfes bir koda geçtik :)
        activeSignifier.DOMoveX(targetButtonRect.position.x, slideDuration)
            .SetEase(Ease.OutBack)
            .SetLink(activeSignifier.gameObject); // Obje yok olduğunda animasyonu durdur

        // Tıklanan butona zıplama (bounce) efekti ver
        BounceEffect(targetButtonRect);
    }

    private void BounceEffect(RectTransform target)
    {
        // Ölçekleri kaydet
        if (!originalScales.ContainsKey(target))
        {
            originalScales[target] = target.localScale;
        }

        Vector3 originalScale = originalScales[target];
        
        // Eğer zaten bir DOTween animasyonu oynuyorsa durdur ki sapıtmasın
        target.DOKill();
        
        // Önce orijinal boyutuna al, sonra "Punch" efekti ile şişir ve jöle gibi titret
        target.localScale = originalScale;
        
        // DOPunchScale çok tatlı bir "juicy" zıplama efekti verir (şişme miktarı, süre, titreşim, esneklik)
        target.DOPunchScale(originalScale * 0.2f, 0.4f, 5, 0.5f)
            .SetLink(target.gameObject); // Obje yok olduğunda animasyonu durdur
    }

    private void OnDestroy()
    {
        // Sahne değiştiğinde çalışan tüm animasyonları temizle ki arkada kalıp hata vermesinler
        if (activeSignifier != null) activeSignifier.DOKill();
        
        if (homeButton != null) homeButton.GetComponent<RectTransform>().DOKill();
        if (storeButton != null) storeButton.GetComponent<RectTransform>().DOKill();
        if (rankButton != null) rankButton.GetComponent<RectTransform>().DOKill();
        if (collectionButton != null) collectionButton.GetComponent<RectTransform>().DOKill();
    }
}
