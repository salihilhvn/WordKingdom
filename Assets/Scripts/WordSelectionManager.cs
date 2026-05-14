using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WordSelectionManager : MonoBehaviour
{
    [Header("Game Data (Testing)")]
    public List<string> targetWords = new List<string> { "UNITY", "GAME", "CODE", "WORD" };

    [Header("Selection State")]
    private List<LetterTile> selectedTiles = new List<LetterTile>();
    private Vector2Int currentDirection;
    private bool isSelecting = false;

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
                StartSelection(hitTile);
            }
            else
            {
                Debug.Log("Tıklanan yerde harf bulunamadı. (BoxCollider eksik olabilir!)");
            }
        }
        // 2. Basılı tutup sürüklendiği sürece
        else if (isPointerPressed && isSelecting)
        {
            LetterTile hitTile = GetTileUnderPointer(pointerPosition);
            if (hitTile != null)
            {
                ProcessTileDuringDrag(hitTile);
            }
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
        if (tile.isSolved) return;

        ClearSelection(); // Önceki seçimi temizle
        
        isSelecting = true;
        selectedTiles.Add(tile);
        tile.Select();
    }

    private void ProcessTileDuringDrag(LetterTile newTile)
    {
        if (newTile.isSolved) return;
        if (selectedTiles.Count == 0) return;

        LetterTile lastTile = selectedTiles[selectedTiles.Count - 1];

        // Aynı tile'ın üstündeysek bir şey yapma
        if (newTile == lastTile) return;

        // Geri gitme (backtracking) kontrolü
        if (selectedTiles.Count >= 2 && newTile == selectedTiles[selectedTiles.Count - 2])
        {
            lastTile.Deselect();
            selectedTiles.RemoveAt(selectedTiles.Count - 1);
            return;
        }

        // Zaten seçili olan başka bir tile'a (geri gitme dışında) çarptıysak yoksay
        if (selectedTiles.Contains(newTile)) return;

        // İki tile arasındaki grid farkı
        int dx = newTile.x - lastTile.x;
        int dy = newTile.y - lastTile.y;

        // 4. Kural: Önceki bastığı butonun komşu karelerinden birine basmayıp diğer butonları basınca 
        // ilk bastığı buton rengi aynı şekilde eskiye dönecek yeni bastığı açık yeşil olacak.
        bool isNeighbor = (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1) && !(dx == 0 && dy == 0);

        if (!isNeighbor)
        {
            // Komşu olmayan tamamen alakasız bir yere sürüklediyse seçimi baştan başlat
            StartSelection(newTile);
            return;
        }

        // Eğer 2. tile'ı seçiyorsak, doğrultuyu belirle (örn: yatay, dikey, çapraz)
        if (selectedTiles.Count == 1)
        {
            currentDirection = new Vector2Int(dx, dy);
            AddTileToSelection(newTile);
        }
        else // Eğer 3. veya daha fazla tile seçiyorsak, doğrultunun aynı kalıp kalmadığını kontrol et
        {
            if (dx == currentDirection.x && dy == currentDirection.y)
            {
                AddTileToSelection(newTile);
            }
        }
    }

    private void AddTileToSelection(LetterTile tile)
    {
        selectedTiles.Add(tile);
        tile.Select();
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

        // Kelimenin hedef listesinde olup olmadığını kontrol et
        if (targetWords.Contains(formedWord) || targetWords.Contains(reverseFormedWord))
        {
            // 5. Kural (Zıttı): Kelime listede varsa, renkler başarılı rengine bürünsün
            foreach (var t in selectedTiles)
            {
                t.SetSolved();
            }
        }
        else
        {
            // 5. Kural: Kelime o levelde istenen listede yoksa tüm seçili karelerdeki renkler eskiye dönecek.
            ClearSelection();
        }

        selectedTiles.Clear();
    }

    private void ClearSelection()
    {
        foreach (var t in selectedTiles)
        {
            t.Deselect();
        }
        selectedTiles.Clear();
    }
}
