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

    [Tooltip("Sol taraftaki UI (Kelime listesi vb.) için bırakılacak boşluk miktarı")]
    public float leftSpace = 6f;

    [Tooltip("Sağ taraftaki boşluk miktarı")]
    public float rightSpace = 0.5f;

    [Tooltip("Kameranın bakış açısı (Tam tepeden 90, hafif açılı 85-80 vb.)")]
    public float cameraAngle = 80f;

    [Tooltip("Kameranın yerden yüksekliği (Y ekseni)")]
    public float cameraHeight = 40f;

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
        float requiredWidth = gridWidth + leftSpace + rightSpace;
        float requiredHeight = gridHeight + topSpace + bottomSpace;

        // Kameranın X eksenindeki kayması (Sağ boşluk ile sol boşluk arasındaki farkın yarısı)
        float xOffset = (rightSpace - leftSpace) / 2f;
        
        // Kameranın Z eksenindeki kayması (Üst boşluk ile alt boşluk arasındaki farkın yarısı)
        float zOffset = (topSpace - bottomSpace) / 2f;

        // Kamerayı ayarlanan yüksekliğe ve hesaplanan X, Z offsetlerine yerleştir, ayarlanan açıya göre döndür
        cam.transform.position = new Vector3(xOffset, cameraHeight, zOffset - 5f); // Açılı bakacağı için biraz daha geriye alıyoruz
        cam.transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
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
