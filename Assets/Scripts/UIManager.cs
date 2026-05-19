using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Timer Settings")]
    public TMP_Text timeText;
    public float remainingTime = 120f; // 2 dakika (Saniye cinsinden)
    private bool isTimerRunning = true;
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
    public TMP_Text multiplierText;
    public Image multiplierFill; // Type'ı Filled olmalı (Unity Inspector)
    public float comboTime = 5f; // Ne kadar sürede yeni kelime bulunması lazım
    private float currentComboTimer = 0f;
    private int currentMultiplier = 1;
    private int wordsFoundInCombo = 0;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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
    }

    private void Update()
    {
        HandleTimer();
        HandleMultiplier();
    }

    private void HandleMultiplier()
    {
        if (wordsFoundInCombo > 0)
        {
            currentComboTimer -= Time.deltaTime;
            
            if (multiplierFill != null)
            {
                multiplierFill.fillAmount = currentComboTimer / comboTime;
            }

            if (currentComboTimer <= 0)
            {
                // Süre bitti, çarpanı sıfırla
                ResetMultiplier();
            }
        }
        else
        {
            if (multiplierFill != null) multiplierFill.fillAmount = 0;
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
        if (multiplierText != null)
        {
            if (currentMultiplier > 1)
            {
                multiplierText.text = "x" + currentMultiplier.ToString();
                multiplierText.gameObject.SetActive(true);
            }
            else
            {
                multiplierText.gameObject.SetActive(false); // Çarpan 1 iken yazıyı gizleyebiliriz
            }
            
            // Tatlı bir büyüme efekti
            StartCoroutine(MultiplierPopRoutine());
        }
    }

    private IEnumerator MultiplierPopRoutine()
    {
        if (multiplierText == null) yield break;
        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            multiplierText.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            multiplierText.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        multiplierText.transform.localScale = originalScale;
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
            // BURAYA SÜRE BİTTİĞİNDE OLACAKLAR EKLENEBİLİR (Game Over vs.)
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

    // Doğru kelime bulunduğunda WordSelectionManager tarafından çağrılır
    public void AnimateCoinsFromTiles(List<LetterTile> solvedTiles)
    {
        // Multiplier Mantığı
        wordsFoundInCombo++;
        if (wordsFoundInCombo >= 2)
        {
            // İlk kelime 1x, ikinci 2x, üçüncü 3x (Maksimum 5x ile sınırlayabiliriz)
            currentMultiplier = Mathf.Min(wordsFoundInCombo, 5); 
        }
        else
        {
            currentMultiplier = 1;
        }
        
        currentComboTimer = comboTime; // Süreyi fulle
        UpdateMultiplierUI(); // Ekrana yansıt

        StartCoroutine(SpawnCoinsRoutine(solvedTiles, currentMultiplier));
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
}
