using UnityEngine;
using TMPro;
using System.Collections;

public class LetterTile : MonoBehaviour
{
    [Header("Grid Position")]
    public int x;
    public int y;

    [Header("Data")]
    public char letter;
    public bool isSolved = false;

    [Header("References")]
    [Tooltip("Prefab içindeki Canvas altındaki TextMeshPro objesi")]
    public TMP_Text letterText;
    [Tooltip("Prefab içindeki button objesinin MeshRenderer'ı")]
    public MeshRenderer buttonRenderer;
    [Tooltip("Prefab içindeki base (alt zemin) objesinin MeshRenderer'ı")]
    public MeshRenderer baseRenderer;
    [Tooltip("Kelime çözüldüğünde patlayacak parçacık efekti (Particle System)")]
    public ParticleSystem solveParticle;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.green;
    public Color solvedColor = Color.yellow;
    public Color errorColor = new Color(1f, 0.4f, 0.4f); // Yumuşak bir kırmızı
    public Color powerUpColor = new Color(1f, 0.65f, 0f); // Tip ve Wordy için (Altın/Koyu Sarı)

    [Header("Animation Settings")]
    [Tooltip("Çözüldüğünde butonun Y ekseninde ne kadar aşağı ineceği (Örn: -0.25)")]
    public float pressedYOffset = -0.25f;

    // Animasyon durumları için orijinal değerler
    private Vector3 originalLocalPos;
    private Vector3 originalScale;
    private Coroutine currentAnimCoroutine;

    private void Start()
    {
        // Unity Inspector'daki eski kırmızı rengi ezmek için koddan zorla altın/turuncu atıyoruz
        powerUpColor = new Color(1f, 0.65f, 0f);

        if (buttonRenderer != null)
        {
            originalLocalPos = buttonRenderer.transform.localPosition;
            originalScale = buttonRenderer.transform.localScale;
            
            // Başlangıç rengini ayarla (Üst normal, alt %25 daha koyu)
            SetRendererColor(buttonRenderer, defaultColor);
            if (baseRenderer != null)
            {
                SetRendererColor(baseRenderer, GetDarkerColor(defaultColor));
            }
        }
    }

    public void SetData(int posX, int posY, char c)
    {
        x = posX;
        y = posY;
        letter = c;
        if (letterText != null)
        {
            letterText.text = letter.ToString();
        }
    }

    public void Select()
    {
        if (isSolved) return;
        
        // Basılma hissini artırmak için: 
        // 1. Büyüme yerine hafif küçülme (ezilme) hissiyatı
        Vector3 targetScale = originalScale * 0.95f; 
        
        // 2. Aşağı çökme
        Vector3 targetPos = originalLocalPos + new Vector3(0, pressedYOffset, 0);
        
        // 3. Çok daha hızlı (0.1s) ve zıplamadan (useBounce: false) aşağı inme
        AnimateToState(targetPos, targetScale, selectedColor, 0.1f, 0f, false, false);
    }

    public void Deselect()
    {
        if (isSolved) return;
        
        // Seçimden çıkıldığında orijinal pozisyona yaylanarak (zıplayarak) dönme
        AnimateToState(originalLocalPos, originalScale, defaultColor, 0.2f, 0f, false, true);
    }

    public void PlayErrorAnimation()
    {
        if (isSolved) return;
        
        if (currentAnimCoroutine != null) StopCoroutine(currentAnimCoroutine);
        if (gameObject.activeInHierarchy)
        {
            currentAnimCoroutine = StartCoroutine(ErrorRoutine());
        }
    }

    private IEnumerator ErrorRoutine()
    {
        if (buttonRenderer == null) yield break;

        Color startColor = buttonRenderer.material.color; 
        if (buttonRenderer.material.HasProperty("_BaseColor"))
            startColor = buttonRenderer.material.GetColor("_BaseColor");

        float duration = 0.4f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Sağa sola titreme (Wiggle) - Sinüs dalgası kullanarak 3 kez git-gel yapar
            float wiggle = Mathf.Sin(t * Mathf.PI * 6f) * 0.15f; 
            buttonRenderer.transform.localPosition = originalLocalPos + new Vector3(wiggle, 0, 0);
            
            // Kırmızıya yanıp sönme
            Color lerpedColor;
            if (t < 0.5f) lerpedColor = Color.Lerp(startColor, errorColor, t * 2f);
            else lerpedColor = Color.Lerp(errorColor, defaultColor, (t - 0.5f) * 2f);
            
            SetRendererColor(buttonRenderer, lerpedColor);

            yield return null;
        }

        // Animasyon bitince her şeyi yerine oturt
        buttonRenderer.transform.localPosition = originalLocalPos;
        SetRendererColor(buttonRenderer, defaultColor);
    }

    public void PlayRippleAnimation(float delay)
    {
        if (isSolved) return;
        
        // Eğer o an fareyle üstünden geçiliyorsa şok dalgasından etkilenmesin (oynanışı bozmamak için)
        if (currentAnimCoroutine != null) return;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RippleRoutine(delay));
        }
    }

    private IEnumerator RippleRoutine(float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (buttonRenderer == null || isSolved || currentAnimCoroutine != null) yield break;

        float duration = 0.35f;
        float elapsed = 0f;
        
        Vector3 startPos = originalLocalPos;
        Vector3 startScale = originalScale;
        Quaternion startRot = buttonRenderer.transform.localRotation;

        while (elapsed < duration)
        {
            if (isSolved) yield break; // Tam dalga sıçrarken kelime çözülürse çık
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 1. Yukarı daha çok sıçrama
            float jump = Mathf.Sin(t * Mathf.PI) * 0.25f; 
            
            // 2. Tepeden bakarken anlaşılması için BÜYÜME efekti (%15)
            float scalePop = Mathf.Sin(t * Mathf.PI) * 0.15f;
            
            // 3. Jöle gibi sağa sola sallanma (Wobble - 6 derece)
            float rotX = Mathf.Sin(t * Mathf.PI * 2f) * 6f;
            float rotZ = Mathf.Cos(t * Mathf.PI * 2f) * 6f;
            
            buttonRenderer.transform.localPosition = startPos + new Vector3(0, jump, 0);
            buttonRenderer.transform.localScale = startScale + new Vector3(scalePop, scalePop, scalePop);
            buttonRenderer.transform.localRotation = startRot * Quaternion.Euler(rotX, 0, rotZ);
            
            yield return null;
        }

        if (!isSolved)
        {
            buttonRenderer.transform.localPosition = startPos;
            buttonRenderer.transform.localScale = startScale;
            buttonRenderer.transform.localRotation = startRot;
        }
    }

    public void SetSolved(float delay = 0f)
    {
        if (isSolved) return; // Zaten çözüldüyse tekrar işlem yapmasını engelle

        isSolved = true;

        // Doğru kelimede daha derin bir basılma ve sarı renk
        Vector3 targetPos = originalLocalPos + new Vector3(0, pressedYOffset, 0);
        AnimateToState(targetPos, originalScale, solvedColor, 0.35f, delay, true, true); // true parametresi partikül patlaması için
    }

    public void HighlightAsPowerUp(float delay = 0f)
    {
        if (isSolved) return; 

        isSolved = true;

        Vector3 targetPos = originalLocalPos + new Vector3(0, pressedYOffset, 0);
        AnimateToState(targetPos, originalScale, powerUpColor, 0.35f, delay, true, true);
    }

    private void AnimateToState(Vector3 targetPos, Vector3 targetScale, Color targetColor, float duration, float delay = 0f, bool isSolveAnim = false, bool useBounce = true)
    {
        if (currentAnimCoroutine != null)
        {
            StopCoroutine(currentAnimCoroutine);
        }
        
        if (gameObject.activeInHierarchy) // Sadece aktif objelerde coroutine çalıştırılabilir
        {
            currentAnimCoroutine = StartCoroutine(AnimRoutine(targetPos, targetScale, targetColor, duration, delay, isSolveAnim, useBounce));
        }
    }

    private IEnumerator AnimRoutine(Vector3 targetPos, Vector3 targetScale, Color targetColor, float duration, float delay, bool isSolveAnim, bool useBounce)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        // Animasyon başladığında, eğer bu bir solve animasyonuysa partikülü patlat
        if (isSolveAnim && solveParticle != null)
        {
            solveParticle.Play();
        }

        if (buttonRenderer == null) yield break;

        Vector3 startPos = buttonRenderer.transform.localPosition;
        Vector3 startScale = buttonRenderer.transform.localScale;
        
        Color startColor = buttonRenderer.material.color; 
        if (buttonRenderer.material.HasProperty("_BaseColor"))
            startColor = buttonRenderer.material.GetColor("_BaseColor");

        Color startBaseColor = Color.white;
        if (baseRenderer != null)
        {
            startBaseColor = baseRenderer.material.color;
            if (baseRenderer.material.HasProperty("_BaseColor"))
                startBaseColor = baseRenderer.material.GetColor("_BaseColor");
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Renk için yumuşak geçiş (EaseInOut)
            float easeT = t * t * (3f - 2f * t);
            
            // Pozisyon ve Scale için sekme/yaylanma (EaseOutBack)
            float moveT = useBounce ? EaseOutBack(t) : easeT; // Basarken yaylanma, çekerken yaylan!

            // LerpUnclamped kullanıyoruz çünkü moveT 1'in üzerine çıkabilir (sekme efekti)
            buttonRenderer.transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, moveT);
            buttonRenderer.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, moveT);
            
            Color lerpedColor = Color.Lerp(startColor, targetColor, easeT);
            SetRendererColor(buttonRenderer, lerpedColor);
            
            // Eğer çözüldüyse base rengini de yumuşakça hedefe geçir
            if (baseRenderer != null && isSolved)
            {
                Color lerpedBaseColor = Color.Lerp(startBaseColor, targetColor, easeT);
                SetRendererColor(baseRenderer, lerpedBaseColor);
            }

            yield return null;
        }

        // Animasyon bittiğinde tam olarak hedef değerlere sabitle (olası küsürat hatalarını önler)
        buttonRenderer.transform.localPosition = targetPos;
        buttonRenderer.transform.localScale = targetScale;
        SetRendererColor(buttonRenderer, targetColor);
        
        if (baseRenderer != null && isSolved)
        {
            SetRendererColor(baseRenderer, targetColor);
        }
    }

    // Yaylanma matematiği (Overshoot effect)
    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        float f = x - 1f; // Mathf.Pow yerine direkt çarpım kullanmak Unity'de çok daha güvenli ve hızlıdır.
        return 1f + c3 * (f * f * f) + c1 * (f * f);
    }

    private void SetRendererColor(MeshRenderer renderer, Color newColor)
    {
        if (renderer == null) return;

        // Standart materyaller için
        renderer.material.color = newColor;
        
        // URP Lit materyali için
        if (renderer.material.HasProperty("_BaseColor"))
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    private Color GetDarkerColor(Color baseColor)
    {
        float h, s, v;
        Color.RGBToHSV(baseColor, out h, out s, out v);
        v = Mathf.Max(0, v - 0.25f); // Rengi %25 koyulaştırır
        return Color.HSVToRGB(h, s, v);
    }
}
