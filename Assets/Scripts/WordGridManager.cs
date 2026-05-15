using UnityEngine;
using System.Collections.Generic;

public class WordGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject letterPrefab;
    public int columns = 10; // X axis
    public int rows = 20;    // Y axis
    public float spacingX = 1.1f;
    public float spacingY = 1.1f;

    [Header("Grid Generation (For Editor/Testing)")]
    public bool generateOnStart = true;
    public string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Tüm oluşturulan tile'ları tutacağımız array
    public LetterTile[,] gridTiles;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    public void GenerateGrid()
    {
        gridTiles = new LetterTile[columns, rows];

        // Kameranın ortalaması için başlangıç pozisyonu (isteğe bağlı, merkeze alabiliriz)
        float startX = -(columns * spacingX) / 2f + (spacingX / 2f);
        float startZ = -(rows * spacingY) / 2f + (spacingY / 2f);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Pozisyonu belirle (X ve Z ekseninde, Y = 0)
                Vector3 spawnPos = new Vector3(startX + (x * spacingX), 0, startZ + (y * spacingY));
                
                // Obje üret
                GameObject go = Instantiate(letterPrefab, spawnPos, Quaternion.identity, this.transform);
                go.name = $"Tile_{x}_{y}";

                LetterTile tile = go.GetComponent<LetterTile>();
                if (tile != null)
                {
                    // Rastgele harf ata
                    char randomLetter = alphabet[Random.Range(0, alphabet.Length)];
                    tile.SetData(x, y, randomLetter);
                    gridTiles[x, y] = tile;
                }
                else
                {
                    Debug.LogWarning("Prefab'de LetterTile componenti bulunamadı!");
                }
            }
        }

        // --- LEVEL 1 (YILAN GİBİ BİRBİRİNE BAĞLI TASARIM) ---
        
        // 1. UNITY yatay (Y=18)
        InjectWord("UNITY", 2, 18, 1, 0); 
        // 2. TIGER dikey aşağı (UNITY'nin T'si ile kesişir: 5,18)
        InjectWord("TIGER", 5, 18, 0, -1);
        // 3. GAME yatay (TIGER'ın G'si ile kesişir: 5,16)
        InjectWord("GAME", 5, 16, 1, 0);
        // 4. MAGIC dikey aşağı (GAME'in M'si ile kesişir: 7,16)
        InjectWord("MAGIC", 7, 16, 0, -1);
        // 5. CODE yatay SOLA doğru (MAGIC'in C'si ile kesişir: 7,12)
        InjectWord("CODE", 7, 12, -1, 0);
        // 6. OCEAN dikey aşağı (CODE'un O'su ile kesişir: 6,12)
        InjectWord("OCEAN", 6, 12, 0, -1);
        // 7. APPLE yatay SOLA doğru (OCEAN'in A'sı ile kesişir: 6,9)
        InjectWord("APPLE", 6, 9, -1, 0);
        // 8. SPACE dikey aşağı (APPLE'ın sol P'si ile kesişir: 4,9)
        InjectWord("SPACE", 4, 10, 0, -1);

        // Kesişmeyen, köşelerde saklanan bağımsız kelimeler:
        InjectWord("WORD", 1, 2, 1, 0);  // Sol alt köşede yatay
        InjectWord("LIGHT", 8, 2, 0, 1); // Sağ alt köşeden yukarı dikey
    }

    private void InjectWord(string word, int startX, int startY, int dx, int dy)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + (i * dx);
            int y = startY + (i * dy);
            
            // Grid sınırları içinde mi kontrol et
            if (x >= 0 && x < columns && y >= 0 && y < rows)
            {
                if (gridTiles[x, y] != null)
                {
                    gridTiles[x, y].SetData(x, y, word[i]);
                }
            }
        }
    }
}
