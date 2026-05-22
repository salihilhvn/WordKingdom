using UnityEngine;
using DG.Tweening; // DOTween eklendi

public class FloatingUI : MonoBehaviour
{
    [Header("Dalgalanma Ayarları")]
    [Tooltip("Animasyonun bir turu ne kadar sürecek? (Saniye)")]
    public float floatDuration = 2f; 
    
    [Tooltip("Ne kadar mesafe yukarı aşağı inecek?")]
    public float floatAmount = 15f; 

    private RectTransform rectTransform;

    private Tween floatTween;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Animasyonu floatTween değişkenine atıyoruz
        floatTween = rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        // Unity sahne değiştirirken objeyi sildiğinde, animasyonu da %100 kesin olarak öldürüyoruz.
        if (floatTween != null)
        {
            floatTween.Kill();
        }
    }
}
