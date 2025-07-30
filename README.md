# 🍕 Welcome to Doughminating Pizza!
This document provides essential information about the game, including how to play, game mechanics, controls, and additional notes.

---

### 📖 Table of Contents
---
1. [Game Overview](#-game-overview)
2. [How to Play](#-how-to-play)
3. [Controls](#-controls)
4. [Game Mechanics](#-game-mechanics)
5. [Ingredients & Stations](#-ingredients--stations)
6. [Tips & Tricks](#-tips--tricks)
7. [Credits](#-credits)

---

## 🎮 Game Overview
---
- **Genre:** Pizza Simulation / Time Management  
- **Platform:** Windows  
- **Team:** Maxim Shteingard, Hana Silverberg, Yossi Matatov
- **Engine:** Unity  

	Step into the chaotic, cheesy world of *Doughminating Pizza*, where you craft mouthwatering pizzas from scratch in a fully interactive 3D kitchen. Assemble, blend, bake, and serve—every step is in your hands!

![UML Diagram](https://www.plantuml.com/plantuml/png/hPBTIWCn48NlynIvhCZs0Of8QK4e28hw0jDqMYFPJ4XcbL9zToTkobQoTzszMixvSZ8_MIQ6o5thbFMCSGy6zMQJVy4mR1tgazBotHwWnQVj1nhMlM0BDBjPw4-oku8XdqzRye-mHPU1nD7wW_fHoZX8IM_y8UBEecV9lpxk5Jg3aoIzKGjC-ZGeQ-hokOdIoUdJfFDx49BYyOhK7aa1llmGE-DZm3CXwE0CHQmS-Xt6gwyEaGocDBC-TBmeAn3cRwlvklyFykvxFh-iftRFEnkpyWf6z7gYOr3fUCGgnQeXHkIcASrKyhFr224uh9mL6F8rZo5W96yhP_sEcEgOKaxkjXTUo3cPk5oiLFrwckPUNqGij8a8Z4cLZZFGPuh74NpQKsoJPheaIhU2T4dEBiNeMlSB)

---

## 🍕 How to Play
---
1. **Start the Game:** Begin in the kitchen with basic ingredients and a few orders.  
2. **Take Orders:** Look at the customers to know their orders or open the order panel using 'O' button
3. **Sauce It:** Throw the tomato into the blender for some wild blending and excellent sauce.
4. **Make the Pizza:** Use stations to prepare dough, add sauce, cheese, and toppings.
5. **Bake It:** Place pizzas into the oven and wait for the timer. Don’t burn them!  
6. **Trash Junk:** Messed up the ingredients? Or accidently burnt the pizza? Trash it!
7. **Serve:** Hand each customer his desired pizza.  
8. **Don't mess up order:** Don't mess up or run out of time! Customers grow impatient when you mess up!

---

## 🎮 Controls
---
- **Movement:** `W/A/S/D` + Mouse to look around  
- **Interact:** `E` or `Left Mouse Button (LMB)`  
- **Pause / Menu:** `ESC Key`
- **Menu Navigation:** Left Mouse Button (LMB)
- **Order Menu:** `O Key`

---

## ⚙️ Game Mechanics
---
- **Interactable Objects:** Every station (e.g., ingredient spawns, blender, oven) uses interfaces for interaction.  
- **Ingredient Handling:** Pick up, place, and combine items using a hand system.  
- **Timer-Based Cooking:** Pizza must be baked within a set time.  
- **Health:** Player loses health points for serving wrong pizzas or not serving at all
- **Failure State:** Game over screen appears if time runs out or too many wrong pizzas are made.
- **Inventory (Hand) System:** Player can hold a single item in his hand at a time.
- **Leveling System:** After 2 successful orders, a day will pass, as the levels progress more customers can arrive simultaneously 
- **Patience System:** Customers arrive with a set patience level, if you mess up orders the patience of your customers will grow shorter!
- **Order Menu:** Annoyed with watching the customers to see their orders? Press `O Key` to pop it right on top of your  screen

---

## 🍅 Ingredients & Stations
---
- **Stations Include:**  
  - **Blender:** Make sauce  
  - **Ingredient Stock:** Grab ingredient  
  - **Oven:** Bake pizzas  
  - **Work counter:** Place items, combine, assemble and flatten dough with the Rolling Pin

- **Ingredients:**  
  - Tomato, Cheese, Dough, Bacon, Pepperoni, Pineapple (yes, even that 😅)
  
-  **Tools:**  
  - Rolling Pin

---

## 🎮Cheat Manager
---
There are few available cheats in our game in case you struggle, press `ESC Key` to hit the pause menu and see the toggles:
1. **Just Right!** - You can hand a customer any ingredient, they will always be 😄
2. **Can't Burn** - Your hands are busy? Don't have time to grab the pizza from the oven? With this cheat it will wait for you as long as needed 🔥
3. **Instant Pizza** - Got a long line of customers? Want to kick them out faster? With this cheat your pizza is ready as soon as it touches the oven! 🍕
4. **God mode** - Play the game as you want, mess your orders, you won't lose health no matter what! (Patience? They might still lose it 😁)

---

## 💡 Tips & Tricks
---
- Keep an eye on timers to avoid burnt pizzas  
- Prep multiple ingredients in advance for faster combos  
- Prioritize orders based on difficulty and time left  
- Don't forget to take out the pizza — trust us

---

## 👨‍🍳 Credits
---
- **Art & Assets:** Unity Asset Store / Blender / Meshy.AI / ChatGPT 
- **Special Thanks:** Everyone who gave feedback & **YOU**, the player!

---

## ℹ️ Asset links
* [Steve by BenAissa_Karim](https://sketchfab.com/3d-models/pizza-steve-e468c1abff564a2981ef0974b0160f84) 
* [Restaurant Scene by Brick Project Studio](https://assetstore.unity.com/packages/3d/environments/fast-food-restaurant-kit-239419)
* [Trash Bin by LerGer](https://sketchfab.com/3d-models/trash-bin-96c6c18cff6b4e1abcdf9f85cb8c690d)
* [Blender & more by ???](https://rayb2.itch.io/kitchen-stuff)
* [Cheese by Grasbock](https://sketchfab.com/3d-models/cheese-6aca23ca300b474888ad72b00be9595d)
* [Pizza Oven by fakefurfetishferre](https://sketchfab.com/3d-models/pizza-oven-c1b6ebc28668499a9f42388e41825180)
* [Pizza Oven 2 by ???](https://3dwarehouse.sketchup.com/model/24c37c6b-a5e4-4e1d-b3f2-4d1f25d90173/Unity-Z-Pizza-Oven-Unit-Modular-Outdoor-Kitchen-With-Wood-Fired-Oven?hl=enhttps://3dwarehouse.sketchup.com/model/24c37c6b-a5e4-4e1d-b3f2-4d1f25d90173/Unity-Z-Pizza-Oven-Unit-Modular-Outdoor-Kitchen-With-Wood-Fired-Oven?hl=en)
* [Direction Arrow by Alihan](https://sketchfab.com/3d-models/direction-arrow-6ef46718c7b242e39fcad7f27ee858a5)

Special thanks to Meshy.AI for creating a lot of simple ingredients!

---

## 🖼️ Gallery
---
### Main Menu
---
![Main Menu](https://i.imgur.com/riQvzxn.jpeg)

### First Glance
---
![First Glance](https://i.imgur.com/xCOrjeh.png)

### Tutorial
---
![Tutorial 1](https://i.imgur.com/NOICieR.png)

![Tutorial 2](https://i.imgur.com/RxlV2v4.png)

### Pause Menu
---
![Pause Menu](https://i.imgur.com/LDVdpMi.png)

### Pizza Making
---
![Blender Animation](https://i.imgur.com/RsTywEN.png)

![Ingredients](https://i.imgur.com/ngUXAKz.png)

![Pizza Assembly](https://i.imgur.com/CX8ZzRK.png)

![Pizza Baking](https://i.imgur.com/srWrbWQ.png)

### Customers
---
![Customers](https://i.imgur.com/77V2wEC.png)
