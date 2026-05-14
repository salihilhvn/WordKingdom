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

        // --- TEST KELİMELERİNİ BİLİNÇLİ OLARAK YERLEŞTİRME ---
        // Sürüklemeyi test edebilmen için bazı kelimeleri grid içine zorla yazdırıyoruz:
        InjectWord("UNITY", 0, 0, 1, 0); // En alt satırda (veya ilk satırda) yatay
        InjectWord("GAME", 2, 1, 0, 1);  // 2. sütundan başlayıp yukarı doğru dikey
        InjectWord("CODE", 4, 2, 1, 1);  // 4,2 koordinatından çapraz
        InjectWord("WORD", 0, 5, 1, 0);  // 5. satırda yatay
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
