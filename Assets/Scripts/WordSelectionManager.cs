using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WordSelectionManager : MonoBehaviour
{
    [Header("Game Data (Testing)")]
    public List<string> targetWords = new List<string> { "UNITY", "GAME", "CODE", "WORD", "APPLE", "MAGIC", "OCEAN", "TIGER", "LIGHT", "SPACE" };
    
    [Tooltip("Bulunan kelimelerin kaydedildiği liste")]
    public List<string> foundWords = new List<string>();
    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Her harf seçildiğinde çıkacak kısa 'pıt' sesi")]
    public AudioClip hoverSound;
    [Tooltip("Doğru kelime bulunduğunda çıkacak ses")]
    public AudioClip successSound;
    [Tooltip("Yanlış kelimede çıkacak hata sesi")]
    public AudioClip errorSound;

    private float basePitch = 1.0f;
    private float pitchIncreaseStep = 0.08f; // Her harfte sesin ne kadar inceleceği

    [Header("Selection State")]
    private List<LetterTile> selectedTiles = new List<LetterTile>();
    private bool isSelecting = false;
    private LetterTile startTile;
    
    // Grid-Snapping için ekran-uzay vektörleri
    private Vector2 screenGridX;
    private Vector2 screenGridY;
    private bool screenGridCalculated = false;

    private WordGridManager gridManager;

    private void Start()
    {
        gridManager = FindAnyObjectByType<WordGridManager>();
        
        // UI listesini hedef kelimelerle doldur
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitializeWordList(targetWords);
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        bool isPointerDown = false;
        bool isPointerPressed = false;
        bool isPointerUp = false;
        Vector2 pointerPosition = Vector2.zero;

        // Mouse veya Dokunmatik kontrolü
        if (Mouse.current != null)
        {
            isPointerDown = Mouse.current.leftButton.wasPressedThisFrame;
            isPointerPressed = Mouse.current.leftButton.isPressed;
            isPointerUp = Mouse.current.leftButton.wasReleasedThisFrame;
            pointerPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            isPointerDown = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
            isPointerPressed = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary;
            isPointerUp = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            pointerPosition = touch.position.ReadValue();
        }

        // 1. Mouse'a (veya ekrana) ilk basıldığı an
        if (isPointerDown)
        {
            Debug.Log("Ekrana tıklandı! Pozisyon: " + pointerPosition);
            LetterTile hitTile = GetTileUnderPointer(pointerPosition);
            if (hitTile != null)
            {
                Debug.Log("Harfe başarıyla tıklandı: " + hitTile.letter);
                startTile = hitTile;
                CalculateScreenGridVectors();
                StartSelection(hitTile);
            }
            else
            {
                Debug.Log("Tıklanan yerde harf bulunamadı. (BoxCollider eksik olabilir!)");
            }
        }
        // 2. Basılı tutup sürüklendiği sürece
        else if (isPointerPressed && isSelecting && startTile != null)
        {
            ProcessDragSnap(pointerPosition);
        }
        // 3. Parmağını (veya mouse'u) çektiği an
        else if (isPointerUp)
        {
            if (isSelecting)
            {
                Debug.Log("Seçim tamamlandı, kontrol ediliyor...");
                EndSelection();
            }
        }
    }

    private LetterTile GetTileUnderPointer(Vector2 pointerPosition)
    {
        // Kameradan pointer pozisyonuna bir ışın yolla
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;

        // Işın bir objeye çarptıysa ve o obje LetterTile içeriyorsa onu döndür
        if (Physics.Raycast(ray, out hit))
        {
            // İhtiyaca göre hit.collider.transform.parent üzerinden de bulabiliriz
            // LetterCube_Prefab'ın en üstündeki LetterTile'ı bulmaya çalışıyoruz
            LetterTile tile = hit.collider.GetComponentInParent<LetterTile>();
            return tile;
        }
        return null;
    }

    private void StartSelection(LetterTile tile)
    {
        ClearSelection(); // Önceki seçimi temizle
        
        isSelecting = true;
        startTile = tile;
        selectedTiles.Add(tile);
        tile.Select();
        PlayHoverSound(1);
    }

    private void CalculateScreenGridVectors()
    {
        if (gridManager == null || gridManager.gridTiles == null || gridManager.columns < 2 || gridManager.rows < 2) return;
        
        LetterTile t00 = gridManager.gridTiles[0, 0];
        LetterTile t10 = gridManager.gridTiles[1, 0];
        LetterTile t01 = gridManager.gridTiles[0, 1];
        
        if (t00 == null) return;

        Vector2 pos00 = Camera.main.WorldToScreenPoint(t00.transform.position);
        
        if (t10 != null)
            screenGridX = ((Vector2)Camera.main.WorldToScreenPoint(t10.transform.position) - pos00);
        else
            screenGridX = Vector2.right * 100f;
            
        if (t01 != null)
            screenGridY = ((Vector2)Camera.main.WorldToScreenPoint(t01.transform.position) - pos00);
        else
            screenGridY = Vector2.down * 100f;
            
        screenGridCalculated = true;
    }

    private void ProcessDragSnap(Vector2 pointerPosition)
    {
        if (!screenGridCalculated) return;

        Vector2 startScreenPos = Camera.main.WorldToScreenPoint(startTile.transform.position);
        Vector2 dragVec = pointerPosition - startScreenPos;

        float det = screenGridX.x * screenGridY.y - screenGridX.y * screenGridY.x;
        if (Mathf.Abs(det) < 0.001f) return;

        float u = (dragVec.x * screenGridY.y - dragVec.y * screenGridY.x) / det;
        float v = (screenGridX.x * dragVec.y - screenGridX.y * dragVec.x) / det;

        // Hassasiyet eşiği: Eğer parmak başlangıç harfinden çok az çıktıysa 0 kabul et
        if (Mathf.Abs(u) < 0.3f && Mathf.Abs(v) < 0.3f)
        {
            UpdateSelectionLine(0, 0, 0);
            return;
        }

        float angle = Mathf.Atan2(v, u) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        
        // 45 derecelik açılara yuvarla
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        snappedAngle = snappedAngle % 360f;
        
        int stepX = 0; int stepY = 0;
        if (snappedAngle == 0f) { stepX = 1; stepY = 0; }
        else if (snappedAngle == 45f) { stepX = 1; stepY = 1; }
        else if (snappedAngle == 90f) { stepX = 0; stepY = 1; }
        else if (snappedAngle == 135f) { stepX = -1; stepY = 1; }
        else if (snappedAngle == 180f) { stepX = -1; stepY = 0; }
        else if (snappedAngle == 225f) { stepX = -1; stepY = -1; }
        else if (snappedAngle == 270f) { stepX = 0; stepY = -1; }
        else if (snappedAngle == 315f) { stepX = 1; stepY = -1; }

        float projection = (u * stepX + v * stepY) / (stepX * stepX + stepY * stepY);
        int count = Mathf.RoundToInt(projection);
        
        if (count < 0) count = 0;
        
        UpdateSelectionLine(stepX, stepY, count);
    }

    private void UpdateSelectionLine(int stepX, int stepY, int count)
    {
        List<LetterTile> newSelection = new List<LetterTile>();
        newSelection.Add(startTile);

        for (int i = 1; i <= count; i++)
        {
            int nx = startTile.x + stepX * i;
            int ny = startTile.y + stepY * i;
            
            if (nx >= 0 && nx < gridManager.columns && ny >= 0 && ny < gridManager.rows)
            {
                LetterTile tile = gridManager.gridTiles[nx, ny];
                if (tile != null)
                {
                    newSelection.Add(tile);
                }
                else break;
            }
            else break;
        }

        bool changed = false;
        if (newSelection.Count != selectedTiles.Count) changed = true;
        else
        {
            for (int i = 0; i < newSelection.Count; i++)
            {
                if (newSelection[i] != selectedTiles[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            // Eskiden seçili olup yeni listede olmayanların seçimini kaldır
            foreach (var t in selectedTiles)
            {
                if (!newSelection.Contains(t)) t.Deselect();
            }
            
            // Yeni listede olup eskiden seçili olmayanları seç
            for (int i = 0; i < newSelection.Count; i++)
            {
                var t = newSelection[i];
                if (!selectedTiles.Contains(t))
                {
                    t.Select();
                    PlayHoverSound(i + 1);
                }
            }
            
            selectedTiles = newSelection;
        }
    }

    private void PlayHoverSound(int tileIndex = -1)
    {
        if (audioSource != null && hoverSound != null)
        {
            int count = tileIndex == -1 ? selectedTiles.Count : tileIndex;
            audioSource.pitch = basePitch + (count * pitchIncreaseStep);
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void EndSelection()
    {
        isSelecting = false;

        if (selectedTiles.Count == 0) return;

        // Oluşan kelimeyi birleştir
        string formedWord = "";
        string reverseFormedWord = ""; // Tersten de geçerli sayabiliriz isteğe bağlı
        foreach (var t in selectedTiles)
        {
            formedWord += t.letter;
        }

        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            reverseFormedWord += selectedTiles[i].letter;
        }

        // Hangi kelimenin bulunduğunu tespit et
        string validWord = null;
        if (targetWords.Contains(formedWord)) validWord = formedWord;
        else if (targetWords.Contains(reverseFormedWord)) validWord = reverseFormedWord;

        // Kelime hedef listesinde varsa VE daha önce bulunmamışsa
        if (validWord != null && !foundWords.Contains(validWord))
        {
            // Kelimeyi bulunanlar listesine ekle ki tekrar kabul edilmesin
            foundWords.Add(validWord);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.CrossOutWord(validWord);
            }

            if (audioSource != null && successSound != null)
            {
                audioSource.pitch = 1f; // Başarı sesi normal incelikte çalsın
                audioSource.PlayOneShot(successSound);
            }

            // 5. Kural (Zıttı): Kelime listede varsa, renkler başarılı rengine bürünsün
            for (int i = 0; i < selectedTiles.Count; i++)
            {
                // Dalga efekti için her harfe 0.05 saniye gecikme veriyoruz
                selectedTiles[i].SetSolved(i * 0.05f);
            }

            // Şok dalgası (Ripple Effect) tetikle
            TriggerRippleEffect(selectedTiles);

            // Altınları uçur
            if (UIManager.Instance != null)
            {
                // Seçili harfleri kopyalayarak gönderiyoruz, çünkü EndSelection bitiminde selectedTiles temizleniyor.
                List<LetterTile> solvedTilesCopy = new List<LetterTile>(selectedTiles);
                UIManager.Instance.AnimateCoinsFromTiles(solvedTilesCopy);
            }
        }
        else
        {
            if (audioSource != null && errorSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(errorSound);
            }

            // Hata efekti: Tüm seçili harfler kırmızı yanıp titresin
            foreach (var t in selectedTiles)
            {
                t.PlayErrorAnimation();
            }
        }

        // Ses inceliğini (pitch) sıfırla
        if (audioSource != null)
        {
            audioSource.pitch = basePitch;
        }

        selectedTiles.Clear();
        startTile = null;
    }

    private void TriggerRippleEffect(List<LetterTile> solvedTiles)
    {
        if (gridManager == null || gridManager.gridTiles == null) return;

        // Bütün grid'i tara
        for (int x = 0; x < gridManager.columns; x++)
        {
            for (int y = 0; y < gridManager.rows; y++)
            {
                LetterTile tile = gridManager.gridTiles[x, y];
                if (tile == null || tile.isSolved) continue;

                // Seçili kelimenin harflerine olan en kısa mesafeyi bul
                float minDistance = float.MaxValue;
                foreach (var st in solvedTiles)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(st.x, st.y));
                    if (dist < minDistance) minDistance = dist;
                }

                // Sadece belli bir mesafedekilere (Örn: 8 birim) dalga gönder
                if (minDistance > 0 && minDistance < 8f)
                {
                    // Mesafeye göre gecikme hesapla (Dalganın yayılma hızı)
                    float delay = minDistance * 0.04f;
                    tile.PlayRippleAnimation(delay);
                }
            }
        }
    }

    private void ClearSelection()
    {
        foreach (var t in selectedTiles)
        {
            t.Deselect();
        }
        selectedTiles.Clear();
        startTile = null;
    }
}
