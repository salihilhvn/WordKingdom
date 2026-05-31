# WordKingdom

All rights are reserved MIT License.

# 👑 Word Kingdom

**Word Kingdom** is a dynamic, level-based hybrid-casual word search puzzle game built with **Unity 6000**. The project innovates upon the traditional word search mechanic by introducing high-stakes Time Attack gameplay, a time-sensitive Streak Multiplier system, and a tightly balanced in-game economy. 

Rather than a static pen-and-paper experience, *Word Kingdom* is engineered to drive player engagement, focus, and session length through juicy game feel feedback and strategic power-up progression.

---

## 🛠 Tech Stack & Architecture

*   **Game Engine:** Unity 6000 (6000.3.5f2)
*   **Render Pipeline:** Universal Render Pipeline (URP)
*   **Animation & UI Motion:** DOTween (Demigiant)
*   **Target Platform:** Mobile (iOS & Android)

---

## 🚀 Key Features & Mechanics

### 1. Dynamic Matrix & Input System
*   Built on a highly optimized **15x20 grid matrix** that handles input drag, multi-directional word detection, and letter selection seamlessly without performance dips.

### 2. Time Attack & Loss Aversion Loop
*   Each level introduces a strict countdown timer to inject healthy friction and urgency into the gameplay. 
*   Failing to complete the level before the time runs out results in a loss, meaning players forfeit their potential coin earnings, which significantly boosts engagement through loss aversion.

### 3. Streak Multiplier (Assumption-Based Design)
*   Letters are directly mapped to coin values. Finding words back-to-back triggers a **time-sensitive Streak Multiplier meter**. 
*   If players maintain their momentum before the multiplier bar drains, their coin rewards scale exponentially, transforming a casual search into an active, high-focus playstyle.

### 4. Core Loop & Economy (Taps & Sinks)
*   **Sources (Taps):** Earning base coins from finding words + maximizing income via the Streak Multiplier.
*   **Sinks (Tüketim Noktaları):** Hard-earned coins are spent directly in-level to purchase **4 distinct power-ups**:
    1.  *Letter Hint:* Uncovers the position of a single random letter.
    2.  *Word Hint:* Instantly highlights an entire hidden word on the grid.
    3.  *Time Manipulator:* Grants an extra +20 seconds or temporarily freezes the countdown.
    4.  *2x Booster:* A short, time-limited booster that doubles all active coin gains.

---

## ⚡ Technical Optimizations

*   **Sprite Atlas & Draw Call Reduction:** All UI elements, icons, and gameplay sprites are packed into a **Sprite Atlas**. This minimizes **Draw Calls (Batching)** and heavily reduces CPU/GPU overhead to guarantee smooth, high-FPS performance on low-end mobile devices.
*   **Balanced Onboarding & Pacing:** Early levels feature a smooth learning curve with balanced tutorials for each power-up, allowing players to build a healthy coin float before the difficulty and time pressure scale up.
*   **Game Feel (Juice):** Integrated DOTween micro-animations for button interactions, letter selection transitions, and reward feedback to maximize tactile player gratification.

---

## 🛣 Roadmap / Future Polish

- [ ] Complete advanced animated polish (particle systems for word completions, floating coin animations via DOTween sequences).
- [ ] Implement dynamic color palettes and thematic asset swapping for effortless A/B testing.
- [ ] Integrate Firebase / GameAnalytics to measure and monitor live session lengths, DAU, and MAU metrics.
