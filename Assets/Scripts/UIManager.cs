using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Timer Settings")]
    public TMP_Text timeText;
    public float remainingTime = 120f; // 2 dakika (Saniye cinsinden)
    private bool isTimerRunning = false; // Oyuna başlarken power-up tutorialı çıkabilir, timer'ı isTimerRunning ile kontrol edeceğiz
    private Color originalTimeColor = Color.white;

    [Header("Coin Settings")]
    public TMP_Text coinsText;
    public RectTransform coinTargetUI; // Sol üstteki Coin ikonu veya paneli
    public AudioSource uiAudioSource;
    public AudioClip coinTickSound;
    private int currentCoins = 0;

    [Header("Coin Fly Animation")]
    public Canvas mainCanvas; // UI Coinlerin spawn olacağı ana canvas
    public Sprite coinSprite; // Uçacak olan coin görseli
    public float coinFlyDuration = 0.5f; // Havada kalma süresi

    [Header("Multiplier Settings")]
    public TMP_Text multiplierText; // İsteğe bağlı eski text sistemi
    
    [Header("Multiplier Custom Visuals (Canvas Objects)")]
    [Tooltip("Canvas'taki 4 eriyen bar Image'ini buraya sürükle (x2, x3, x4, x5)")]
    public Image[] multiplierFillImages; 
    [Tooltip("Canvas'taki 5 şekilli yazı Objesini buraya sürükle (x1, x2, x3, x4, x5)")]
    public GameObject[] multiplierIconObjects; 
    
    public float comboTime = 5f; // Ne kadar sürede yeni kelime bulunması lazım
    private float currentComboTimer = 0f;
    private int currentMultiplier = 1;
    private int wordsFoundInCombo = 0;
    
    // Obje boyutlarının animasyonda bozulmaması için orijinal boyutlarını saklayacağız
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    
    [Header("Word List UI")]
    public RectTransform wordListContainer;
    public GameObject wordTextPrefab;
    private Dictionary<string, TMP_Text> wordTexts = new Dictionary<string, TMP_Text>();

    [Header("End Game Panels")]
    public GameObject levelPassedPanel;
    [Tooltip("Level Passed panelindeki sayarak artan Coin Text'i")]
    public TMP_Text levelPassedCoinText; 
    public Button lpWatchAdButton;
    public Button lpNextLevelButton;
    public Button lpMainMenuButton;

    public GameObject levelFailedPanel;
    public Button lfWatchAdButton;
    public Button lfTryAgainButton;
    public Button lfMainMenuButton;

    [Tooltip("Popuplar açıldığında arkadaki oyun alanını karartan/blurlayan panel")]
    public GameObject blurBackgroundPanel;

    private Coroutine coinAnimCoroutine;
    private bool isCoinAnimRunning = false;

    public bool IsPopupActive()
    {
        return (levelPassedPanel != null && levelPassedPanel.activeSelf) || 
               (levelFailedPanel != null && levelFailedPanel.activeSelf);
    }

    private Vector2 levelFailedOriginalPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Canvas'ta senin ayarladığın orijinal boyutları (scale) kaydediyoruz
        if (multiplierIconObjects != null)
        {
            foreach (var icon in multiplierIconObjects)
                if (icon != null) originalScales[icon.transform] = icon.transform.localScale;
        }
        if (multiplierFillImages != null)
        {
            foreach (var bar in multiplierFillImages)
                if (bar != null) originalScales[bar.transform] = bar.transform.localScale;
        }
        if (multiplierText != null) originalScales[multiplierText.transform] = multiplierText.transform.localScale;

        if (timeText != null)
        {
            originalTimeColor = timeText.color;
        }

        if (coinsText != null)
        {
            coinsText.text = currentCoins.ToString();
        }

        // Eğer Canvas atanmamışsa sahnede bulmaya çalış
        if (mainCanvas == null)
        {
            mainCanvas = FindAnyObjectByType<Canvas>();
        }
        
        // Eğer hedef coin ikonu atandıysa ve sprite'ı boşsa, oradan çek
        if (coinSprite == null && coinTargetUI != null)
        {
            Image targetImage = coinTargetUI.GetComponent<Image>();
            if (targetImage != null) coinSprite = targetImage.sprite;
        }

        // Buton Dinleyicileri (Listeners)
        if (lpWatchAdButton) lpWatchAdButton.onClick.AddListener(OnWatchAdCoins);
        if (lpNextLevelButton) lpNextLevelButton.onClick.AddListener(OnNextLevel);
        if (lpMainMenuButton) lpMainMenuButton.onClick.AddListener(OnMainMenu);
        
        if (lfWatchAdButton) lfWatchAdButton.onClick.AddListener(OnWatchAdTime);
        if (lfTryAgainButton) lfTryAgainButton.onClick.AddListener(OnTryAgain);
        if (lfMainMenuButton) lfMainMenuButton.onClick.AddListener(OnMainMenu);
        
        // Panelleri başlangıçta gizle
        if (levelPassedPanel) levelPassedPanel.SetActive(false);
        if (levelFailedPanel) 
        {
            levelFailedPanel.SetActive(false);
            RectTransform rect = levelFailedPanel.GetComponent<RectTransform>();
            if (rect) levelFailedOriginalPos = rect.anchoredPosition;
        }
        if (blurBackgroundPanel) blurBackgroundPanel.SetActive(false);
    }

    private void Update()
    {
        HandleTimer();
        HandleMultiplier();

        // Level Passed panelindeki altın sayma animasyonunu tıklayarak geçme
        if (isCoinAnimRunning)
        {
            bool isSkipPressed = false;
            
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                isSkipPressed = true;
            }
            else if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0)
            {
                if (UnityEngine.InputSystem.Touchscreen.current.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    isSkipPressed = true;
                }
            }

            if (isSkipPressed)
            {
                isCoinAnimRunning = false; // Coroutine döngüsünü kırar ve anında sonucu yazar
            }
        }
    }

    private void HandleMultiplier()
    {
        if (wordsFoundInCombo > 0)
        {
            currentComboTimer -= Time.deltaTime;
            
            // Aktif olan barı bul ve fillAmount'unu (erime miktarını) düşür
            if (currentMultiplier > 1)
            {
                int barIndex = Mathf.Clamp(currentMultiplier - 2, 0, 3);
                if (multiplierFillImages != null && multiplierFillImages.Length > barIndex && multiplierFillImages[barIndex] != null)
                {
                    multiplierFillImages[barIndex].fillAmount = currentComboTimer / comboTime;
                }
            }

            if (currentComboTimer <= 0)
            {
                // Süre bitti, çarpanı sıfırla
                ResetMultiplier();
            }
        }
    }

    private void ResetMultiplier()
    {
        currentComboTimer = 0;
        currentMultiplier = 1;
        wordsFoundInCombo = 0;
        UpdateMultiplierUI();
    }

    private void UpdateMultiplierUI()
    {
        // Önce tüm obje ve barları gizleyelim (temizlik)
        if (multiplierIconObjects != null)
        {
            foreach (var icon in multiplierIconObjects)
            {
                if (icon != null) icon.SetActive(false);
            }
        }
        if (multiplierFillImages != null)
        {
            foreach (var bar in multiplierFillImages)
            {
                if (bar != null) bar.gameObject.SetActive(false);
            }
        }

        // 1. Şekilli Text İkonunu Aç (x1, x2, x3, x4, x5 -> index 0, 1, 2, 3, 4)
        int textIndex = Mathf.Clamp(currentMultiplier - 1, 0, 4);
        if (multiplierIconObjects != null && multiplierIconObjects.Length > textIndex && multiplierIconObjects[textIndex] != null)
        {
            multiplierIconObjects[textIndex].SetActive(true);
            StartCoroutine(MultiplierPopRoutine(multiplierIconObjects[textIndex].transform));
        }

        // 2. Eriyen Barı Aç (Sadece x2, x3, x4, x5 için -> index 0, 1, 2, 3)
        if (currentMultiplier > 1)
        {
            int barIndex = Mathf.Clamp(currentMultiplier - 2, 0, 3);
            if (multiplierFillImages != null && multiplierFillImages.Length > barIndex && multiplierFillImages[barIndex] != null)
            {
                multiplierFillImages[barIndex].gameObject.SetActive(true);
                StartCoroutine(MultiplierPopRoutine(multiplierFillImages[barIndex].transform));
            }
        }

        // Eski text sistemi varsa pop efekti ver
        if (multiplierText != null)
        {
            multiplierText.text = "x" + currentMultiplier.ToString();
            StartCoroutine(MultiplierPopRoutine(multiplierText.transform));
        }
    }

    private IEnumerator MultiplierPopRoutine(Transform targetTransform)
    {
        if (targetTransform == null) yield break;
        
        // Objenin ilk baştaki boyutunu al (Senin küçülttüğün hali)
        Vector3 originalScale = originalScales.ContainsKey(targetTransform) ? originalScales[targetTransform] : targetTransform.localScale;
        Vector3 targetScale = originalScale * 1.3f; // Kendi boyutunun %30'u kadar büyüsün

        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        targetTransform.localScale = originalScale;
    }

    private void HandleTimer()
    {
        if (!isTimerRunning || timeText == null) return;

        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            isTimerRunning = false;
            timeText.color = Color.red;
            UpdateTimerText();
            // Süre bittiğinde Level Failed göster
            ShowLevelFailed();
            return;
        }

        UpdateTimerText();

        // Son 10 saniye kala kırmızı yanıp sönme efekti (Blink)
        if (remainingTime <= 10f)
        {
            float blinkPingPong = Mathf.PingPong(Time.time * 5f, 1f); // Hızlıca yanıp sön
            timeText.color = Color.Lerp(Color.red, originalTimeColor, blinkPingPong);
            
            // Eğer istersen son 10 saniyede hafif büyüme küçülme eklenebilir
            float scaleBlink = 1f + Mathf.PingPong(Time.time * 2f, 0.1f);
            timeText.transform.localScale = new Vector3(scaleBlink, scaleBlink, 1f);
        }
        else
        {
            timeText.color = originalTimeColor;
            timeText.transform.localScale = Vector3.one;
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetUI(float newTime)
    {
        remainingTime = newTime;
        // isTimerRunning'i direkt true yapmıyoruz. PowerUpManager CheckTutorials'dan sonra ResumeTimer() ile yapacak.
        if (timeText != null)
        {
            timeText.color = originalTimeColor;
            timeText.transform.localScale = Vector3.one;
        }

        currentCoins = 0;
        if (coinsText != null)
        {
            coinsText.text = currentCoins.ToString();
        }

        ResetMultiplier();
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void ResumeTimer()
    {
        isTimerRunning = true;
    }

    public void AddExtraTime(float extra)
    {
        remainingTime += extra;
        UpdateTimerText();
    }

    public void InitializeWordList(List<string> words)
    {
        if (wordListContainer == null || wordTextPrefab == null) return;
        
        foreach (Transform child in wordListContainer)
        {
            Destroy(child.gameObject);
        }
        wordTexts.Clear();
        
        foreach (string word in words)
        {
            GameObject obj = Instantiate(wordTextPrefab, wordListContainer);
            TMP_Text textComp = obj.GetComponent<TMP_Text>();
            if (textComp != null)
            {
                textComp.text = word;
                wordTexts.Add(word, textComp);
            }
        }
    }
    
    public void CrossOutWord(string word)
    {
        if (wordTexts.TryGetValue(word, out TMP_Text textComp))
        {
            // Üstünü çiz ve rengini soluklaştır
            textComp.fontStyle |= FontStyles.Strikethrough;
            textComp.color = new Color(textComp.color.r, textComp.color.g, textComp.color.b, 0.5f);
            
            StartCoroutine(WordTextPopRoutine(textComp));
        }
    }

    private IEnumerator WordTextPopRoutine(TMP_Text textComp)
    {
        float elapsed = 0f;
        float duration = 0.2f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textComp.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textComp.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        textComp.transform.localScale = originalScale;
    }

    // Doğru kelime bulunduğunda WordSelectionManager tarafından çağrılır
    public void AnimateCoinsFromTiles(List<LetterTile> solvedTiles)
    {
        wordsFoundInCombo++;
        
        // 1. Oyuncunun bu kelimeden kazanacağı parayı hesapla (İlk kelimede 1x, ikinci kelimede 2x...)
        int coinsMultiplierToApply = Mathf.Min(wordsFoundInCombo, 5);
        StartCoroutine(SpawnCoinsRoutine(solvedTiles, coinsMultiplierToApply));
        
        // 2. Arayüzü (UI) bir SONRAKİ hedef için güncelle (İlk kelimeyi buldu, artık x2 peşinde)
        // Bu sayede 4 barımız tam oturuyor (x2, x3, x4, x5 hedefleri)
        currentMultiplier = Mathf.Min(wordsFoundInCombo + 1, 5); 
        
        currentComboTimer = comboTime; // Süreyi fulle
        UpdateMultiplierUI(); // Ekrana yansıt
    }

    private IEnumerator SpawnCoinsRoutine(List<LetterTile> solvedTiles, int multiplierToApply)
    {
        // Her harf için bir coin uçur, "tık tık tık" hissiyatı için aralarına gecikme koy
        for (int i = 0; i < solvedTiles.Count; i++)
        {
            LetterTile tile = solvedTiles[i];
            
            // Coin'i harfin 3D dünyadaki pozisyonundan UI pozisyonuna doğru uçur
            StartCoroutine(FlyCoinRoutine(tile.transform.position, i, multiplierToApply));
            
            // Tık tık tık gecikmesi
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FlyCoinRoutine(Vector3 startWorldPos, int index, int multiplierToApply)
    {
        if (mainCanvas == null || Camera.main == null || coinTargetUI == null) yield break;

        // 1. Yeni bir UI Objesi (Image) oluştur
        GameObject flyCoinObj = new GameObject("FlyCoin_" + index);
        flyCoinObj.transform.SetParent(mainCanvas.transform, false);
        flyCoinObj.transform.SetAsLastSibling(); // En üstte görünsün

        RectTransform rectTransform = flyCoinObj.AddComponent<RectTransform>();
        Image image = flyCoinObj.AddComponent<Image>();
        
        if (coinSprite != null) image.sprite = coinSprite;
        else image.color = Color.yellow; // Görsel yoksa sarı kare atalım

        // Boyutunu ayarla
        rectTransform.sizeDelta = new Vector2(50f, 50f);

        // 2. Başlangıç pozisyonunu Dünya Koordinatından UI Ekran koordinatına çevir
        Vector2 screenPos = Camera.main.WorldToScreenPoint(startWorldPos);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform, 
            screenPos, 
            mainCanvas.worldCamera, 
            out Vector2 localStartPos);

        rectTransform.anchoredPosition = localStartPos;

        // 3. Hedef pozisyon (Coin Target UI)
        Vector3[] corners = new Vector3[4];
        coinTargetUI.GetWorldCorners(corners);
        Vector3 targetWorldCenter = (corners[0] + corners[2]) / 2f; // Hedefin ortası
        
        Vector2 targetScreenPos = targetWorldCenter; // World köşeleri Canvas Space Overlay'de ScreenPos ile aynıdır
        if (mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
             targetScreenPos = RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, targetWorldCenter);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform, 
            targetScreenPos, 
            mainCanvas.worldCamera, 
            out Vector2 localTargetPos);

        // Uçuş Animasyonu
        float elapsed = 0f;
        Vector2 controlPoint = localStartPos + (localTargetPos - localStartPos) / 2f + new Vector2(Random.Range(-200f, 200f), Random.Range(100f, 300f)); // Kavisli (Bezier) uçması için rastgele bir kontrol noktası

        // Uçarken hafifçe büyüyüp küçülecek (Pop efekti)
        Vector3 startScale = Vector3.zero; // Harften çıkarken küçük başlasın
        Vector3 midScale = new Vector3(1.5f, 1.5f, 1.5f);
        Vector3 endScale = Vector3.one;

        while (elapsed < coinFlyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / coinFlyDuration;

            // Ease In (hızlanarak git)
            float easeT = t * t; 

            // Quadratic Bezier Curve (Kavisli gidiş)
            Vector2 currentPos = Vector2.Lerp(
                Vector2.Lerp(localStartPos, controlPoint, easeT), 
                Vector2.Lerp(controlPoint, localTargetPos, easeT), 
                easeT);
            
            rectTransform.anchoredPosition = currentPos;

            // Scale ayarı
            if (t < 0.5f)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, midScale, t * 2f);
            }
            else
            {
                rectTransform.localScale = Vector3.Lerp(midScale, endScale, (t - 0.5f) * 2f);
            }

            yield return null;
        }

        // Uçuş bitti, hedefe ulaştı.
        Destroy(flyCoinObj);

        // Tık sesi çal ve sayıyı artır
        IncrementCoin(multiplierToApply);
    }

    private void IncrementCoin(int amount)
    {
        // Coin Glaze power-up aktifse gelen coin 2 katına çıkar
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isCoinGlazeActive)
        {
            amount *= 2;
        }

        currentCoins += amount;
        if (coinsText != null)
        {
            coinsText.text = currentCoins.ToString();
            
            // Text'e zıplama efekti
            StartCoroutine(CoinTextPopRoutine());
        }

        if (uiAudioSource != null && coinTickSound != null)
        {
            // Tık tık tık sesinin pitch'ini hafif rastgele/artarak çalabiliriz
            uiAudioSource.pitch = Random.Range(1.0f, 1.2f);
            uiAudioSource.PlayOneShot(coinTickSound);
        }
    }

    private IEnumerator CoinTextPopRoutine()
    {
        if (coinsText == null) yield break;

        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.4f, 1.4f, 1.4f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Büyü
            coinsText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Küçül
            coinsText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        coinsText.transform.localScale = originalScale;
    }

    public void ShowLevelPassed()
    {
        isTimerRunning = false;
        if (blurBackgroundPanel) 
        {
            blurBackgroundPanel.SetActive(true);
            Image blurImg = blurBackgroundPanel.GetComponent<Image>();
            if (blurImg != null) StartCoroutine(FadeBlur(blurImg, 0.6f)); // %60 saydamlığa yavaşça geç
        }
        
        if (levelPassedPanel) 
        {
            levelPassedPanel.SetActive(true);
            StartCoroutine(BouncePopAnim(levelPassedPanel.transform));
        }

        if (lpWatchAdButton) lpWatchAdButton.interactable = true; // Yeni bölüme geçince butonu tekrar aktif et

        if (levelPassedCoinText) 
        {
            levelPassedCoinText.text = "0";
            if (coinAnimCoroutine != null) StopCoroutine(coinAnimCoroutine);
            coinAnimCoroutine = StartCoroutine(AnimateCoinsTo(currentCoins));
        }
    }

    private IEnumerator AnimateCoinsTo(int targetCoins)
    {
        isCoinAnimRunning = true;
        float duration = 1.5f; // Animasyon 1.5 saniye sürsün
        float elapsed = 0f;
        int startCoins = 0;

        while (elapsed < duration)
        {
            if (!isCoinAnimRunning) break; // Eğer kullanıcı ekrana tıklayıp geçmek isterse
            
            elapsed += Time.deltaTime;
            int current = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, elapsed / duration));
            levelPassedCoinText.text = current.ToString();
            yield return null;
        }

        levelPassedCoinText.text = targetCoins.ToString();
        isCoinAnimRunning = false;
    }

    public void ShowLevelFailed()
    {
        isTimerRunning = false;
        if (blurBackgroundPanel) 
        {
            blurBackgroundPanel.SetActive(true);
            Image blurImg = blurBackgroundPanel.GetComponent<Image>();
            if (blurImg != null) StartCoroutine(FadeBlur(blurImg, 0.6f));
        }
        
        if (levelFailedPanel) 
        {
            levelFailedPanel.SetActive(true);
            StartCoroutine(DropBounceAnim(levelFailedPanel.transform));
        }
    }

    private IEnumerator FadeBlur(Image blurImg, float targetAlpha, float duration = 0.5f)
    {
        Color c = blurImg.color;
        float startAlpha = 0f;
        c.a = startAlpha;
        blurImg.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            blurImg.color = c;
            yield return null;
        }
        c.a = targetAlpha;
        blurImg.color = c;
    }

    private IEnumerator BouncePopAnim(Transform target)
    {
        target.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // EaseOutBack Formülü
            float s = 1.70158f;
            float easedT = ((t = t - 1) * t * ((s + 1) * t + s) + 1);
            
            float scale = Mathf.LerpUnclamped(0f, 1f, easedT);
            target.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private IEnumerator DropBounceAnim(Transform target)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 endPos = levelFailedOriginalPos;
        Vector2 startPos = endPos + new Vector2(0, Screen.height + 500f);

        rect.anchoredPosition = startPos;
        float elapsed = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // EaseOutBounce Formülü
            float easedT = 0f;
            if (t < (1 / 2.75f)) {
                easedT = (7.5625f * t * t);
            } else if (t < (2 / 2.75f)) {
                easedT = (7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f);
            } else if (t < (2.5 / 2.75f)) {
                easedT = (7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f);
            } else {
                easedT = (7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f);
            }

            float yPos = Mathf.LerpUnclamped(startPos.y, endPos.y, easedT);
            rect.anchoredPosition = new Vector2(endPos.x, yPos);
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

    private void OnWatchAdCoins()
    {
        // Şimdilik 2 katı verelim direkt (Daha sonra gerçek AD sistemi eklenecek)
        currentCoins *= 2;
        if (coinsText) coinsText.text = currentCoins.ToString();
        if (levelPassedCoinText) levelPassedCoinText.text = currentCoins.ToString();
        
        // Butonu kapat ki 2 kere izleyemesin
        if (lpWatchAdButton) lpWatchAdButton.interactable = false;
    }

    private void OnNextLevel()
    {
        if (levelPassedPanel) levelPassedPanel.SetActive(false);
        if (blurBackgroundPanel) blurBackgroundPanel.SetActive(false);
        FindAnyObjectByType<WordSelectionManager>().LoadNextLevel();
    }

    private void OnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnWatchAdTime()
    {
        // 20 saniye ekle ve devam et
        remainingTime += 20f;
        isTimerRunning = true;
        if (levelFailedPanel) levelFailedPanel.SetActive(false);
        if (blurBackgroundPanel) blurBackgroundPanel.SetActive(false);
    }

    private void OnTryAgain()
    {
        if (levelFailedPanel) levelFailedPanel.SetActive(false);
        if (blurBackgroundPanel) blurBackgroundPanel.SetActive(false);
        FindAnyObjectByType<WordSelectionManager>().RestartCurrentLevel();
    }
}
