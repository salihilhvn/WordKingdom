using UnityEngine;

public class GridCameraFitter : MonoBehaviour
{
    [Header("Referanslar")]
    public WordGridManager gridManager;
    
    [Header("Boşluk Ayarları (UI İçin)")]
    [Tooltip("Üst taraftaki UI (Hedefler, Hamle sayısı vb.) için bırakılacak boşluk miktarı")]
    public float topSpace = 4f;
    
    [Tooltip("Alt taraftaki UI (Güçlendiriciler, Ayarlar vb.) için bırakılacak boşluk miktarı")]
    public float bottomSpace = 3f;

    [Tooltip("Yanlardan (sağ ve sol) bırakılacak boşluk miktarı")]
    public float sidePadding = 0.5f;

    void Start()
    {
        // Grid oluşturulduktan biraz sonra kamerayı ayarla
        Invoke(nameof(AdjustCamera), 0.1f);
    }

    public void AdjustCamera()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<WordGridManager>();
            if (gridManager == null) return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        // Grid'in sadece objelerden oluşan genişliği ve yüksekliği
        float gridWidth = gridManager.columns * gridManager.spacingX;
        float gridHeight = gridManager.rows * gridManager.spacingY;

        // İhtiyaç duyulan toplam alan (Grid + Boşluklar)
        float requiredWidth = gridWidth + (sidePadding * 2);
        float requiredHeight = gridHeight + topSpace + bottomSpace;

        // Kameranın Z eksenindeki kayması (Üst boşluk ile alt boşluk arasındaki farkın yarısı)
        // Eğer üst boşluk daha fazlaysa, kamera Z'de yukarı kayar, böylece grid ekranda aşağıda görünür.
        float zOffset = (topSpace - bottomSpace) / 2f;

        // Kamerayı Y=10 yüksekliğine ve hesaplanan Z offsetine yerleştir, aşağı bakmasını sağla
        cam.transform.position = new Vector3(0, 10f, zOffset);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.orthographic = true;

        // Ekranın en-boy oranına göre kameranın "size" değerini ayarlayarak sığdırıyoruz
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = requiredWidth / requiredHeight;

        if (screenRatio >= targetRatio)
        {
            // Ekran yatayda daha geniş (veya tam uygun), yüksekliğe göre fit et
            cam.orthographicSize = requiredHeight / 2f;
        }
        else
        {
            // Ekran dikeyde daha dar (örn. telefon), genişliğe göre fit et
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = (requiredHeight / 2f) * differenceInSize;
        }
    }
}
