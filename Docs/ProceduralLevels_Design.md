# Design Document: Procedural Descent Levels

## 1. Vision

The game is a gradual descent into an abyss. Each new level is **procedural generation**, not a static prefab (except early tutorials).

The player moves **down and forward**, never in a sharp drop: they must push back the darkness and jump between platforms within jump-reach. Levels should often feel like a **megastructure** — huge walls, ceilings, columns, distant scenery.

### Core loop between levels

1. Traverse the level along a branching path (2–3 jump options per step).
2. Jump into a **downward tunnel** at the exit.
3. On entering the tunnel mouth mid-fall — commit: the next level is generated.
4. Exit the shaft → ~5 units of free fall → next level’s starting platform.
5. Missed jump → visible death floor (same look as tutorials) → respawn.

### What already exists

| Layer | Status |
|---|---|
| Orchestration (`LocationsGenerator`) | done |
| Level build + tunnels (`ProceduralLevelBuilder`) | done |
| Descent path + branching (`LevelArchetype`, `DescentPathGenerator`) | done |
| Enclosing shell (far walls / ceiling / death floor) | done |
| Palettes + weighted variant pool | done |
| Preview / regenerate seed | done |

---

## 2. Archetypes already implemented

Each archetype = **one class** + several **ScriptableObject variants** with different parameters.

| Archetype | Idea | Variant assets |
|---|---|---|
| **TwoWalls** | Canyon: two walls widen apart, beams between them | NarrowShaft, GrandChasm |
| **SingleWall** | One wall + protrusions (rocks / cantilevers) | Cliff, Overhang |
| **Ceiling** | Ceiling only: stalactites / mushrooms | StalactiteField, MushroomForest |
| **Columns** | No close walls/ceiling: standing columns + hanging monoliths | PillarField, HangingCity |

All of them get far walls/ceiling/floor from `EnclosingShellBuilder` when the archetype does not build those surfaces itself.

---

## 3. Archetypes still to create

From the original design request — not yet implemented as separate classes.

### 3.1 Pit / Kotlovan (priority: high)

**Idea:** A huge round pit. The player jumps along beams/ledges **on the inner wall** and gradually descends toward the bottom.

**How it differs from TwoWalls:** the path is not a linear corridor — it follows an **arc / spiral** around a vertical axis; the “wall” is the inner surface of a cylinder/ring.

**Key parameters (guide):**

- pit radius at start / at bottom;
- turn angle per step (spiral);
- depth / number of turns;
- beam density, radial jitter;
- decorative rings / cross-beams above and below the path.

**Technical hook:** override `BuildPath` to rotate heading around a center instead of meandering along +Z. One “inner” wall flag; shell fills far ceiling + death floor below the last platform.

---

### 3.2 Stone Tree (priority: medium)

**Idea:** A giant stone tree. Descent from canopy/trunk down to the roots.

**Gameplay:** walkable surfaces = branches / growths / trunk ledges; the path winds around the trunk downward.

**Key parameters:**

- trunk diameter, segment height;
- branches per step, angle from trunk;
- chance of hanging roots vs side branches;
- root density near the “bottom” of the level;
- scenery: distant other trunks/branches.

**Technical hook:** path meanders around an imaginary trunk axis; geometry = large vertical cylinder + branch boxes/cylinders with platforms. Shell: far walls/ceiling; the tree itself is structure.

---

### 3.3 Abstract / Worm / Perlin Weave (priority: medium–low)

**Idea:** Abstract generation — intertwined “worms”, noise tubes, chaotic beams that still produce a **traversable** descending path.

**Gameplay:** looks chaotic, but the path spine is always jumpable; extras = branching risk/reward.

**Key parameters:**

- noise scale / worm radius / turn rate;
- how many decorative worms off the path;
- intersection density;
- whether worms are solid (collider) or scenery-only.

**Technical hook:** `DescentPathGenerator` remains the source of walkable tops; on top of that — 3D curves (Perlin on heading/pitch) for decorative tubes. Must not break `MaxStepDistance` / `MaxStepDown`.

---

### 3.4 Optional later expansions

Not required from the original list, but fit the same system:

| Idea | Short description |
|---|---|
| **Spiral Shaft** | Narrow vertical shaft with a spiral of platforms |
| **Broken Floors** | Horizontal “floors” with holes; descent through gaps |
| **Bridge Network** | Network of bridges between distant pylons over a void |
| **Inverted City** | Buildings/balconies growing from the ceiling downward (Ceiling extension) |

---

## 4. Non-functional rules for any new archetype

1. **Always descend** within player jump limits (`DescentPathGenerator.MaxStepDistance` / `MaxStepDown`).
2. **Branching** via `GenerateJumpCandidates` — the primary route is always completable.
3. **Last step before the tunnel** — no branch (clean approach to the mouth).
4. Set `HasLeftWall` / `HasRightWall` / `HasCeiling` when the archetype builds those surfaces itself.
5. Override `RollEntryOverheadHeight` if geometry above the entry must be pierced by the shaft.
6. Add **≥2 variant assets** to the `ProceduralLevelsConfig` pool with different parameters.
7. Verify in `Tools → Procedural Level Preview`: branching, shell, tunnel mouth, death floor.

---

## 5. Recommended implementation order

1. **Pit / Kotlovan** — strongest new “wow”, clearly distinct from the existing four.
2. **Stone Tree** — second strong megastructure silhouette.
3. **Abstract Weave** — once the base is stable; harder to tune for traversability.
4. Variant assets + weights in config after each new class.

---

## 6. Definition of success

After the tutorials, the player never sees “the same corridor”: canyon, then wall, then ceiling, then columns, then pit, then tree, then abstract weave — but always knows where to jump, and always finds a downward tunnel at the end.

---

## 7. Key code entry points

| Task | Look at |
|---|---|
| When next level spawns / how many live at once | `LocationsGenerator` |
| Tunnel / entry shaft / free fall | `ProceduralLevelBuilder` |
| Shared path, branching, overhead height | `LevelArchetype` |
| New visual layout | new `*Archetype.cs` under `Archetypes/` |
| Far walls / death floor | `EnclosingShellBuilder` |
| Tunable numbers without code | `ProceduralLevelsConfig` + variant `.asset` files |
| Materials / light color | `ModulePalette` assets |
