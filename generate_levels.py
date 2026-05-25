import random

class WordSearchGenerator:
    def __init__(self, cols=10, rows=20):
        self.cols = cols
        self.rows = rows
        self.grid = {}
        self.placements = []
        
    def can_place(self, word, startX, startY, dx, dy):
        for i, char in enumerate(word):
            x = startX + i * dx
            y = startY + i * dy
            if x < 0 or x >= self.cols or y < 0 or y >= self.rows:
                return False
            if (x, y) in self.grid and self.grid[(x, y)] != char:
                return False
        return True

    def place_word(self, word, allowed_dirs):
        possible_placements = []
        for y in range(self.rows):
            for x in range(self.cols):
                for dx, dy in allowed_dirs:
                    if self.can_place(word, x, y, dx, dy):
                        score = 0
                        for i, char in enumerate(word):
                            nx = x + i * dx
                            ny = y + i * dy
                            if (nx, ny) in self.grid:
                                score += 1
                        possible_placements.append((score, x, y, dx, dy))
        
        if not possible_placements:
            return False
            
        max_score = max(p[0] for p in possible_placements)
        best_placements = [p for p in possible_placements if p[0] == max_score]
        
        chosen = random.choice(best_placements)
        _, x, y, dx, dy = chosen
        
        for i, char in enumerate(word):
            self.grid[(x + i * dx, y + i * dy)] = char
        self.placements.append((word, x, y, dx, dy))
        return True

DIRECTIONS = {
    'R': (1, 0),
    'D': (0, -1),
    'DR': (1, -1),
    'DL': (-1, -1),
    'L': (-1, 0),
    'U': (0, 1),
    'UL': (-1, 1),
    'UR': (1, 1)
}

themes = [
    ("TUTORIAL (Sadece Sag ve Asagi)", ["DOG", "CAT", "SUN", "HAT"], ['R', 'D']),
    ("BEGINNER (Capraz Eklendi)", ["BIRD", "FISH", "TREE", "STAR", "MOON"], ['R', 'D', 'DR', 'DL']),
    ("EASY (Karma 1)", ["APPLE", "GRAPE", "MELON", "LEMON", "PEACH", "BERRY"], ['R', 'D', 'DR', 'DL']),
    ("REVERSE (Tersler Eklendi)", ["PIZZA", "BURGER", "SALAD", "BREAD", "CHEESE", "MILK", "WATER"], ['R', 'D', 'DR', 'DL', 'L', 'U']),
    ("MIXED (Tumu Aktif)", ["TRAIN", "PLANE", "TRUCK", "BOAT", "BIKE", "SHIP", "HORSE", "CAR"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("SPORTS (Zorluk +1)", ["SOCCER", "TENNIS", "BOXING", "GOLF", "SWIM", "JUDO", "RUGBY", "TRACK"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("COLORS (Zorluk +2)", ["PURPLE", "YELLOW", "ORANGE", "BROWN", "GREEN", "BLACK", "WHITE", "PINK"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("SPACE (Zorluk +3)", ["GALAXY", "PLANET", "ORBIT", "COMET", "METEOR", "EARTH", "VENUS", "MARS", "SUN"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("ANIMALS (Zorluk +4)", ["ELEPHANT", "GIRAFFE", "MONKEY", "TIGER", "ZEBRA", "SNAKE", "EAGLE", "SHARK", "BEAR"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("NATURE (Zorluk +5)", ["MOUNTAIN", "FOREST", "RIVER", "OCEAN", "DESERT", "VALLEY", "CANYON", "ISLAND", "BEACH"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("WEATHER (Zorluk +6)", ["THUNDER", "LIGHTNING", "TORNADO", "BLIZZARD", "HURRICANE", "STORM", "RAIN", "SNOW", "WIND"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("BODY (Zorluk +7)", ["SHOULDER", "STOMACH", "FINGER", "MUSCLE", "BRAIN", "HEART", "BLOOD", "BONE", "SKIN", "TOE"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("MUSIC (Zorluk +8)", ["ACOUSTIC", "ELECTRIC", "CLASSICAL", "COUNTRY", "REGGAE", "RHYTHM", "MELODY", "CHORD", "VOCAL", "BASS"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("SCIENCE (Zorluk +9)", ["PHYSICS", "BIOLOGY", "CHEMISTRY", "ATOM", "GRAVITY", "ENERGY", "MATTER", "CELL", "GENE", "DNA", "DATA"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR']),
    ("MASTER (En Zor)", ["ALGORITHM", "FUNCTION", "VARIABLE", "INTEGER", "BOOLEAN", "STRING", "ARRAY", "OBJECT", "CLASS", "METHOD", "LOOP", "CODE"], ['R', 'D', 'DR', 'DL', 'L', 'U', 'UL', 'UR'])
]

code = ""
for i, (theme, words, dirs) in enumerate(themes):
    level_num = i + 1
    gen = WordSearchGenerator()
    allowed = [DIRECTIONS[d] for d in dirs]
    
    random.seed(42 + i) # Ensure consistency
    words_copy = list(words)
    random.shuffle(words_copy)
    
    for w in words_copy:
        success = gen.place_word(w, allowed)
        if not success:
            print(f"FAILED TO PLACE {w} IN LEVEL {level_num}")
            
    code += f"            // LEVEL {level_num} ({theme})\n"
    code += f"            LevelConfig level{level_num} = new LevelConfig();\n"
    for w, x, y, dx, dy in gen.placements:
        code += f'            level{level_num}.wordPlacements.Add(new WordPlacement("{w}", {x}, {y}, {dx}, {dy}));\n'
    code += f"            levels.Add(level{level_num});\n\n"

with open("generated_levels.cs", "w", encoding="utf-8") as f:
    f.write(code)

print("SUCCESS")
