using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelsPanelManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelsPanel; // Panel GameObject (to show/hide)
    public Button closeButton; // Geri dönme butonu
    
    [Header("Level Buttons")]
    public List<Button> levelButtons; // Inspector'dan sırayla 1, 2, 3... diye atanacak butonlar

    [Header("Button Sprites")]
    public Sprite doneSprite;     // Mavi
    public Sprite currentSprite;  // Yeşil
    public Sprite notDoneSprite;  // Gri

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void OnEnable()
    {
        RefreshButtons();
    }

    public void OpenPanel()
    {
        if (levelsPanel != null)
        {
            levelsPanel.SetActive(true);
            RefreshButtons(); // Açıldığında butonların durumunu güncelle
        }
    }

    public void ClosePanel()
    {
        if (levelsPanel != null)
        {
            levelsPanel.SetActive(false);
        }
    }

    public void RefreshButtons()
    {
        // En son geçilen (kilidi açılan) bölüm indexini al (0'dan başlar: 0 = Level 1)
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);

        for (int i = 0; i < levelButtons.Count; i++)
        {
            Button btn = levelButtons[i];
            if (btn == null) continue;

            Image btnImage = btn.GetComponent<Image>();
            
            // Tüm butonların önceki tıklama olaylarını temizle (üzerine tekrar eklememek için)
            btn.onClick.RemoveAllListeners();

            if (i < unlockedLevel)
            {
                // Geçilmiş Bölüm (Mavi)
                if (btnImage != null && doneSprite != null) btnImage.sprite = doneSprite;
                btn.interactable = true; // Opaklığın düşmemesi için interactable yapıyoruz (ayrıca tekrar oynanabilir)
                
                int levelIndexToLoad = i;
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelIndexToLoad));
            }
            else if (i == unlockedLevel)
            {
                // Şu An Oynanacak Bölüm (Yeşil)
                if (btnImage != null && currentSprite != null) btnImage.sprite = currentSprite;
                btn.interactable = true; // Tıklanabilir
                
                int levelIndexToLoad = i;
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelIndexToLoad));
            }
            else
            {
                // Gelecek Bölüm (Gri)
                if (btnImage != null && notDoneSprite != null) btnImage.sprite = notDoneSprite;
                btn.interactable = false; // Tıklanamaz
            }
        }
    }

    private void OnLevelButtonClicked(int levelIndex)
    {
        // Tıklanan bölümü hafızaya kaydet, Gameplay sahnesi oradan okuyacak
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.Save();

        // Oyunu Başlat
        SceneManager.LoadScene("Gameplay");
    }
}
