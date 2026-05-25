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

    private void Awake()
    {
        // Awake içinde yapıyoruz ki WordSelectionManager Start'ta doğru leveli çekebilsin
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        InitializeDefaultLevelsIfEmpty();
    }

    private void Start()
    {
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

            // LEVEL 1: TUTORIAL (Sadece Sag ve Asagi)
            LevelConfig level1 = new LevelConfig();
            level1.wordPlacements.Add(new WordPlacement("SUN", 2, 4, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("CAT", 5, 6, 0, -1));
            level1.wordPlacements.Add(new WordPlacement("HAT", 4, 5, 1, 0));
            level1.wordPlacements.Add(new WordPlacement("DOG", 6, 11, 0, -1));
            levels.Add(level1);

            // LEVEL 2: BEGINNER (Capraz Eklendi)
            LevelConfig level2 = new LevelConfig();
            level2.wordPlacements.Add(new WordPlacement("BIRD", 6, 4, 0, -1));
            level2.wordPlacements.Add(new WordPlacement("FISH", 5, 3, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("MOON", 0, 1, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("STAR", 3, 2, 1, 0));
            level2.wordPlacements.Add(new WordPlacement("TREE", 2, 7, 1, -1));
            levels.Add(level2);

            // LEVEL 3: EASY (Karma 1)
            LevelConfig level3 = new LevelConfig();
            level3.wordPlacements.Add(new WordPlacement("BERRY", 5, 13, 1, 0));
            level3.wordPlacements.Add(new WordPlacement("PEACH", 6, 14, 0, -1));
            level3.wordPlacements.Add(new WordPlacement("MELON", 5, 14, 1, -1));
            level3.wordPlacements.Add(new WordPlacement("APPLE", 6, 12, -1, -1));
            level3.wordPlacements.Add(new WordPlacement("GRAPE", 6, 17, 0, -1));
            level3.wordPlacements.Add(new WordPlacement("LEMON", 7, 12, 0, -1));
            levels.Add(level3);

            // LEVEL 4: REVERSE (Tersler Eklendi)
            LevelConfig level4 = new LevelConfig();
            level4.wordPlacements.Add(new WordPlacement("BURGER", 5, 0, 0, 1));
            level4.wordPlacements.Add(new WordPlacement("WATER", 9, 6, -1, -1));
            level4.wordPlacements.Add(new WordPlacement("BREAD", 5, 8, 1, -1));
            level4.wordPlacements.Add(new WordPlacement("SALAD", 8, 4, 0, 1));
            level4.wordPlacements.Add(new WordPlacement("PIZZA", 4, 11, 1, -1));
            level4.wordPlacements.Add(new WordPlacement("CHEESE", 3, 6, 1, -1));
            level4.wordPlacements.Add(new WordPlacement("MILK", 6, 11, -1, -1));
            levels.Add(level4);

            // LEVEL 5: MIXED (Tumu Aktif)
            LevelConfig level5 = new LevelConfig();
            level5.wordPlacements.Add(new WordPlacement("PLANE", 8, 12, -1, -1));
            level5.wordPlacements.Add(new WordPlacement("BOAT", 8, 8, -1, 1));
            level5.wordPlacements.Add(new WordPlacement("HORSE", 4, 4, 0, 1));
            level5.wordPlacements.Add(new WordPlacement("CAR", 6, 6, -1, 0));
            level5.wordPlacements.Add(new WordPlacement("TRAIN", 5, 7, -1, -1));
            level5.wordPlacements.Add(new WordPlacement("SHIP", 2, 6, 0, -1));
            level5.wordPlacements.Add(new WordPlacement("TRUCK", 5, 5, -1, 1));
            level5.wordPlacements.Add(new WordPlacement("BIKE", 1, 11, 0, -1));
            levels.Add(level5);

            // LEVEL 6: SPORTS (Zorluk +1)
            LevelConfig level6 = new LevelConfig();
            level6.wordPlacements.Add(new WordPlacement("TRACK", 9, 14, 0, -1));
            level6.wordPlacements.Add(new WordPlacement("SOCCER", 4, 13, 1, 0));
            level6.wordPlacements.Add(new WordPlacement("SWIM", 4, 13, 1, 1));
            level6.wordPlacements.Add(new WordPlacement("GOLF", 4, 12, 1, 1));
            level6.wordPlacements.Add(new WordPlacement("TENNIS", 9, 14, -1, -1));
            level6.wordPlacements.Add(new WordPlacement("BOXING", 4, 14, 1, -1));
            level6.wordPlacements.Add(new WordPlacement("JUDO", 0, 17, 0, -1));
            level6.wordPlacements.Add(new WordPlacement("RUGBY", 1, 11, 1, 1));
            levels.Add(level6);

            // LEVEL 7: COLORS (Zorluk +2)
            LevelConfig level7 = new LevelConfig();
            level7.wordPlacements.Add(new WordPlacement("ORANGE", 7, 17, -1, -1));
            level7.wordPlacements.Add(new WordPlacement("WHITE", 2, 16, 0, -1));
            level7.wordPlacements.Add(new WordPlacement("BLACK", 7, 15, -1, 0));
            level7.wordPlacements.Add(new WordPlacement("PINK", 6, 12, -1, 1));
            level7.wordPlacements.Add(new WordPlacement("PURPLE", 9, 12, -1, 0));
            level7.wordPlacements.Add(new WordPlacement("BROWN", 7, 15, 0, 1));
            level7.wordPlacements.Add(new WordPlacement("GREEN", 4, 10, 0, 1));
            level7.wordPlacements.Add(new WordPlacement("YELLOW", 3, 14, 1, -1));
            levels.Add(level7);

            // LEVEL 8: SPACE (Zorluk +3)
            LevelConfig level8 = new LevelConfig();
            level8.wordPlacements.Add(new WordPlacement("COMET", 3, 7, 1, 1));
            level8.wordPlacements.Add(new WordPlacement("ORBIT", 4, 8, 1, -1));
            level8.wordPlacements.Add(new WordPlacement("MARS", 5, 9, 0, -1));
            level8.wordPlacements.Add(new WordPlacement("SUN", 5, 6, 1, -1));
            level8.wordPlacements.Add(new WordPlacement("PLANET", 7, 16, 0, -1));
            level8.wordPlacements.Add(new WordPlacement("GALAXY", 4, 9, 1, -1));
            level8.wordPlacements.Add(new WordPlacement("METEOR", 0, 12, 1, -1));
            level8.wordPlacements.Add(new WordPlacement("EARTH", 1, 11, 1, 0));
            level8.wordPlacements.Add(new WordPlacement("VENUS", 9, 2, -1, 1));
            levels.Add(level8);

            // LEVEL 9: ANIMALS (Zorluk +4)
            LevelConfig level9 = new LevelConfig();
            level9.wordPlacements.Add(new WordPlacement("BEAR", 6, 13, -1, 1));
            level9.wordPlacements.Add(new WordPlacement("TIGER", 5, 11, 0, 1));
            level9.wordPlacements.Add(new WordPlacement("SNAKE", 9, 14, -1, 0));
            level9.wordPlacements.Add(new WordPlacement("EAGLE", 1, 10, 1, 1));
            level9.wordPlacements.Add(new WordPlacement("GIRAFFE", 7, 15, -1, 0));
            level9.wordPlacements.Add(new WordPlacement("SHARK", 2, 18, 1, -1));
            level9.wordPlacements.Add(new WordPlacement("MONKEY", 5, 10, -1, 0));
            level9.wordPlacements.Add(new WordPlacement("ZEBRA", 0, 19, 1, -1));
            level9.wordPlacements.Add(new WordPlacement("ELEPHANT", 1, 12, 0, -1));
            levels.Add(level9);

            // LEVEL 10: NATURE (Zorluk +5)
            LevelConfig level10 = new LevelConfig();
            level10.wordPlacements.Add(new WordPlacement("ISLAND", 1, 2, 0, 1));
            level10.wordPlacements.Add(new WordPlacement("VALLEY", 0, 6, 1, -1));
            level10.wordPlacements.Add(new WordPlacement("DESERT", 3, 2, 1, 0));
            level10.wordPlacements.Add(new WordPlacement("BEACH", 6, 1, 0, 1));
            level10.wordPlacements.Add(new WordPlacement("MOUNTAIN", 8, 6, -1, 0));
            level10.wordPlacements.Add(new WordPlacement("RIVER", 4, 5, 0, -1));
            level10.wordPlacements.Add(new WordPlacement("OCEAN", 5, 10, -1, -1));
            level10.wordPlacements.Add(new WordPlacement("CANYON", 5, 8, 0, -1));
            level10.wordPlacements.Add(new WordPlacement("FOREST", 4, 10, 1, 0));
            levels.Add(level10);

            // LEVEL 11: WEATHER (Zorluk +6)
            LevelConfig level11 = new LevelConfig();
            level11.wordPlacements.Add(new WordPlacement("STORM", 1, 9, 1, -1));
            level11.wordPlacements.Add(new WordPlacement("LIGHTNING", 2, 12, 0, -1));
            level11.wordPlacements.Add(new WordPlacement("TORNADO", 3, 1, 0, 1));
            level11.wordPlacements.Add(new WordPlacement("RAIN", 4, 4, -1, 1));
            level11.wordPlacements.Add(new WordPlacement("WIND", 0, 9, 1, -1));
            level11.wordPlacements.Add(new WordPlacement("SNOW", 2, 3, 1, 1));
            level11.wordPlacements.Add(new WordPlacement("BLIZZARD", 1, 12, 1, 0));
            level11.wordPlacements.Add(new WordPlacement("HURRICANE", 9, 14, -1, -1));
            level11.wordPlacements.Add(new WordPlacement("THUNDER", 9, 3, -1, 0));
            levels.Add(level11);

            // LEVEL 12: BODY (Zorluk +7)
            LevelConfig level12 = new LevelConfig();
            level12.wordPlacements.Add(new WordPlacement("BONE", 1, 7, 1, 1));
            level12.wordPlacements.Add(new WordPlacement("BRAIN", 7, 9, -1, 0));
            level12.wordPlacements.Add(new WordPlacement("STOMACH", 1, 5, 1, 1));
            level12.wordPlacements.Add(new WordPlacement("SHOULDER", 6, 2, 0, 1));
            level12.wordPlacements.Add(new WordPlacement("SKIN", 6, 2, -1, 0));
            level12.wordPlacements.Add(new WordPlacement("BLOOD", 5, 5, -1, 1));
            level12.wordPlacements.Add(new WordPlacement("FINGER", 5, 0, -1, 1));
            level12.wordPlacements.Add(new WordPlacement("TOE", 2, 10, 1, 0));
            level12.wordPlacements.Add(new WordPlacement("MUSCLE", 9, 10, -1, 0));
            level12.wordPlacements.Add(new WordPlacement("HEART", 2, 14, 0, -1));
            levels.Add(level12);

            // LEVEL 13: MUSIC (Zorluk +8)
            LevelConfig level13 = new LevelConfig();
            level13.wordPlacements.Add(new WordPlacement("COUNTRY", 9, 1, 0, 1));
            level13.wordPlacements.Add(new WordPlacement("MELODY", 4, 12, 1, -1));
            level13.wordPlacements.Add(new WordPlacement("BASS", 1, 15, 1, 1));
            level13.wordPlacements.Add(new WordPlacement("CLASSICAL", 0, 17, 1, 0));
            level13.wordPlacements.Add(new WordPlacement("CHORD", 0, 17, 0, -1));
            level13.wordPlacements.Add(new WordPlacement("REGGAE", 7, 13, 0, 1));
            level13.wordPlacements.Add(new WordPlacement("VOCAL", 5, 13, -1, 1));
            level13.wordPlacements.Add(new WordPlacement("ELECTRIC", 5, 11, 0, -1));
            level13.wordPlacements.Add(new WordPlacement("RHYTHM", 5, 6, -1, -1));
            level13.wordPlacements.Add(new WordPlacement("ACOUSTIC", 0, 7, 1, 0));
            levels.Add(level13);

            // LEVEL 14: SCIENCE (Zorluk +9)
            LevelConfig level14 = new LevelConfig();
            level14.wordPlacements.Add(new WordPlacement("CELL", 3, 8, -1, 1));
            level14.wordPlacements.Add(new WordPlacement("GRAVITY", 0, 9, 1, -1));
            level14.wordPlacements.Add(new WordPlacement("CHEMISTRY", 0, 1, 1, 1));
            level14.wordPlacements.Add(new WordPlacement("ATOM", 6, 4, -1, 0));
            level14.wordPlacements.Add(new WordPlacement("PHYSICS", 8, 5, -1, 0));
            level14.wordPlacements.Add(new WordPlacement("MATTER", 6, 13, -1, -1));
            level14.wordPlacements.Add(new WordPlacement("DNA", 8, 2, -1, 1));
            level14.wordPlacements.Add(new WordPlacement("ENERGY", 0, 5, 1, -1));
            level14.wordPlacements.Add(new WordPlacement("GENE", 0, 6, 0, -1));
            level14.wordPlacements.Add(new WordPlacement("DATA", 1, 7, 1, 0));
            level14.wordPlacements.Add(new WordPlacement("BIOLOGY", 4, 6, 0, -1));
            levels.Add(level14);

            // LEVEL 15: MASTER (En Zor)
            LevelConfig level15 = new LevelConfig();
            level15.wordPlacements.Add(new WordPlacement("CODE", 4, 3, 0, 1));
            level15.wordPlacements.Add(new WordPlacement("FUNCTION", 1, 3, 1, 0));
            level15.wordPlacements.Add(new WordPlacement("BOOLEAN", 8, 2, -1, 1));
            level15.wordPlacements.Add(new WordPlacement("STRING", 3, 0, 1, 1));
            level15.wordPlacements.Add(new WordPlacement("INTEGER", 7, 6, -1, 0));
            level15.wordPlacements.Add(new WordPlacement("OBJECT", 0, 11, 1, -1));
            level15.wordPlacements.Add(new WordPlacement("LOOP", 6, 2, 1, 1));
            level15.wordPlacements.Add(new WordPlacement("CLASS", 4, 7, 1, 1));
            level15.wordPlacements.Add(new WordPlacement("VARIABLE", 7, 8, -1, 1));
            level15.wordPlacements.Add(new WordPlacement("METHOD", 3, 5, 1, 1));
            level15.wordPlacements.Add(new WordPlacement("ALGORITHM", 0, 14, 1, 0));
            level15.wordPlacements.Add(new WordPlacement("ARRAY", 6, 12, -1, 1));
            levels.Add(level15);
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
