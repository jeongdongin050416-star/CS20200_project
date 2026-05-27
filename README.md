# CLI Fortress

A command-line artillery game built with **F# / .NET 10** for the CS20200 Programming Principles term project.

Two players, **User1** and **User2**, stand on separate buildings. On each turn, the current player fires a cannonball by entering a speed and an angle. The cannonball follows a projectile path. If it hits the opponent's tank, the current player wins. If it misses, the turn passes to the other player.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A terminal that can run `dotnet` commands

Check your .NET version with:

```bash
dotnet --version
```

The version should be `10.x.x`.

### Clone the Repository

```bash
git clone https://github.com/jeongdongin050416-star/CS20200_project.git
cd CS20200_project
```

### Run the Game

```bash
dotnet run
```

### Build the Project

```bash
dotnet build
```

---

## How to Play

### Game Setup

When the game starts, it creates a battlefield with:

- a fixed-width text screen,
- two buildings with randomly generated heights,
- `User1`'s tank on the left building,
- `User2`'s tank on the right building.

The tanks are displayed as:

| Symbol | Meaning |
|--------|---------|
| `1` | User1's tank |
| `2` | User2's tank |
| `#` | Building block |
| `*` | Cannonball during animation |
| `=` | Ground |

User1 attacks first.

### Taking a Turn

On each turn, the game prints the current battlefield and asks the current player to enter two values:

1. **Speed**
2. **Angle**

The default valid input ranges are:

| Input | Range |
|-------|-------|
| Speed | `10` to `40` |
| Angle | `0` to `90` degrees |

If the input is not numeric or is outside the valid range, the game asks the player to enter the value again. The turn does not advance until valid input is given.

### Projectile Motion

After a valid speed and angle are entered, the game simulates the cannonball using projectile motion.

- User1 fires from left to right.
- User2 fires from right to left.
- Gravity pulls the cannonball downward.
- The game animates the cannonball path in the terminal.

### Hit and Miss Conditions

A shot is a **hit** when the cannonball reaches the opponent tank's position.

A shot is a **miss** when one of the following happens:

- the cannonball hits a building,
- the cannonball falls to the ground,
- the cannonball leaves the battlefield horizontally.

The shooter cannot hit their own tank. If the cannonball is still on the launcher's own starting cell at the beginning of the shot, that cell is ignored.

### Winning the Game

| Result | Condition |
|--------|-----------|
| User1 wins | User1's shot hits User2's tank |
| User2 wins | User2's shot hits User1's tank |
| Turn passes | The current shot misses |

The game continues until one player hits the opponent's tank.

---

## Example Session

```text
CLI FORTRESS - User1 to attack
Speed range: 10..40 | Angle range: 0..90 degrees
Coordinates: User1=(5,6), User2=(74,9)    
                                                                                
                                                                                
                                                                                
                                                                          2     
                                                                        #####   
                                                                        #####   
     1                                                                  #####   
   #####                                                                #####   
   #####                                                                #####   
   #####                                                                #####   
   #####                                                                #####   
   #####                                                                #####   
================================================================================

User1's turn
Speed: 30
Angle: 45

User1 fires: speed=30.0 angle=45.0
...
The cannonball fell to the ground. Miss.
Attack opportunity passes to User2.
```

The exact building heights and cannonball path may differ because the building heights are randomly generated.

---

## Project Structure

```text
CS20200_project/
├── CS20200_project.fsproj   # .NET 10 F# project file
├── Game.fs                  # Game types, battlefield rendering, input handling, projectile simulation, game loop
├── Program.fs               # Program entry point
├── README.md                # Project instructions and documentation
├── bin/                     # Build output directory
└── obj/                     # Intermediate build files
```

> Note: `bin/` and `obj/` are generated build directories. They are not required to understand the source code.

---

## Main Types

```fsharp
type Player = User1 | User2

type Tank = {
    Player: Player
    X: int
    Y: int
    Symbol: char
}

type Shot = {
    Speed: float
    AngleDegrees: float
}

type ShotResult =
    | Hit of Player
    | Miss of string
```

---

## Module Overview

| File | Responsibility |
|------|----------------|
| `Game.fs` | Defines game data types, creates the world, renders the battlefield, reads player input, simulates shots, checks hit/miss conditions, and runs the turn loop. |
| `Program.fs` | Creates the initial world and starts the game loop. |
| `CS20200_project.fsproj` | Configures the F# executable project for .NET 10. |

---

## Requirements Summary

This implementation follows these observable requirements:

1. The game runs in the command line.
2. The game is implemented in F# using .NET 10.
3. The game creates a battlefield with two tanks placed on buildings.
4. User1 and User2 take turns attacking each other.
5. On each turn, the current player enters a speed and an angle.
6. Invalid speed or angle input is rejected and requested again.
7. The cannonball follows a projectile path affected by gravity.
8. A player wins by hitting the opponent's tank.
9. A shot misses if it hits a building, falls to the ground, or leaves the battlefield.
10. After a miss, the attack opportunity passes to the other player.

---

## Changes from the Original Plan

The following implementation details were adjusted during development:

1. **Projectile miss condition changed**
   - Original behavior: a shot could be treated as a miss immediately after passing a user or touching the ground.
   - Final behavior: a shot is treated as a miss when it hits a building, falls to the ground, or leaves the battlefield.
   - Reason: making the cannonball disappear only because it passed a tank looked unnatural, and building collision was necessary for more consistent gameplay.

---

## LLM Usage

An LLM was used during development to help create an initial version of the command-line fortress game in F# / .NET 10. The prompt asked for a command-line fortress game that satisfies the project requirements and tests whether the behavior conforms to those requirements.

Manual changes were necessary after the initial LLM-generated version. In particular, unnecessary test code was removed, the code was reorganized by module/file, the speed range was adjusted, and projectile simulation parameters were changed.

The main part the LLM did not handle correctly was projectile behavior. In the initial version, the simulation time step was too large, so the cannonball could visually pass through buildings or tanks between frames. To fix this, I reduced the simulation time step and adjusted the animation frame delay.

The miss condition also needed to be changed. In the initial version, the cannonball could disappear simply because it passed behind a tank, and building collision was not handled as a proper miss condition. This felt unnatural during gameplay. I changed the miss logic so that a shot is treated as a miss when it hits a building, falls to the ground, or leaves the battlefield.


---

## Author

- **Dongin Jeong**
- CS20200 Programming Principles, Spring 2026