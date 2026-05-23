using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WordPlacement
{
    public string word;
    public int startX;
    public int startY;
    public int dx;
    public int dy;

    public WordPlacement(string w, int x, int y, int dirX, int dirY)
    {
        word = w; startX = x; startY = y; dx = dirX; dy = dirY;
    }
}

[System.Serializable]
public class LevelConfig
{
    public List<WordPlacement> wordPlacements = new List<WordPlacement>();
}

public class WordGridManager : MonoBehaviour
{
    [Header("Level Settings")]
    public List<LevelConfig> levels = new List<LevelConfig>();
    public int currentLevelIndex = 0;

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
        // Menüde tıklanan (veya Play denilerek kaydedilen) leveli al
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);

        InitializeDefaultLevelsIfEmpty();
        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    private void InitializeDefaultLevelsIfEmpty()
    {
        if (levels == null || levels.Count == 0)
        {
            levels = new List<LevelConfig>();

            // LEVEL 1 (YILAN GİBİ BİRBİRİNE BAĞLI TASARIM)
            LevelConfig level1 = new LevelConfig();
            level1.wordPlacements.Add(new WordPlacement("UNITY", 2, 18, 1, 0));
            level1.wordPlacements.Add(new WordPlacement("TIGER", 5, 18, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("GAME", 5, 16, 1, 0));
            level1.wordPlacements.Add(new WordPlacement("MAGIC", 7, 16, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("CODE", 7, 12, -1, 0));
            level1.wordPlacements.Add(new WordPlacement("OCEAN", 6, 12, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("APPLE", 6, 9, -1, 0));
            level1.wordPlacements.Add(new WordPlacement("SPACE", 4, 10, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("WORD", 1, 2, 1, 0));
            level1.wordPlacements.Add(new WordPlacement("LIGHT", 8, 2, 0, 1));
            levels.Add(level1);

            // LEVEL 2 (YENİ KELİMELER - BASİT BİR DİZİLİM)
            LevelConfig level2 = new LevelConfig();
            level2.wordPlacements.Add(new WordPlacement("BRAIN", 1, 18, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("NIGHT", 5, 18, 0, -1));
            level2.wordPlacements.Add(new WordPlacement("TRAIN", 5, 14, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("NINJA", 9, 14, 0, -1));
            level2.wordPlacements.Add(new WordPlacement("APPLE", 9, 10, -1, 0));
            level2.wordPlacements.Add(new WordPlacement("EAGLE", 5, 10, 0, -1));
            level2.wordPlacements.Add(new WordPlacement("EARTH", 5, 6, -1, 0));
            level2.wordPlacements.Add(new WordPlacement("HEART", 1, 6, 0, -1));
            level2.wordPlacements.Add(new WordPlacement("TABLE", 2, 2, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("CHAIR", 7, 2, 0, 1));
            levels.Add(level2);
        }
    }

    public void ClearGrid()
    {
        if (gridTiles != null)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (gridTiles[x, y] != null)
                    {
                        Destroy(gridTiles[x, y].gameObject);
                    }
                }
            }
        }
        gridTiles = null;
    }

    public void GenerateGrid()
    {
        ClearGrid();
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

        // Aktif olan seviyenin kelimelerini yerleştir
        if (levels != null && currentLevelIndex < levels.Count)
        {
            LevelConfig currentLevel = levels[currentLevelIndex];
            foreach (var placement in currentLevel.wordPlacements)
            {
                InjectWord(placement.word, placement.startX, placement.startY, placement.dx, placement.dy);
            }
        }
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
