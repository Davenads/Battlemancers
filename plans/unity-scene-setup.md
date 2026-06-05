# Battlemancers — Unity BattleScene Setup Guide

This guide walks a developer through assembling the BattleScene in the Unity Editor from scratch. All code is already written. Follow these steps exactly to get the prototype running.

---

## Part 1 — Prerequisites

### Unity Version

Use **Unity 6 LTS** (or Unity 2022 LTS minimum) with the **Universal Render Pipeline (URP)**. The renderer is required because `GridRenderer` sets the `_BaseColor` material property, which is the URP Lit shader's albedo property. The Standard pipeline uses `_Color` — the code sets both, so it will degrade gracefully, but URP is the intended target.

### Required Packages

Install the following via **Window → Package Manager**:

| Package | Why Required |
|---|---|
| **TextMeshPro** (com.unity.textmeshpro) | `PlanningPhaseUI` and `SpellButtonPanel` use `TMP_Text` for all labels |
| **Universal RP** (com.unity.render-pipelines.universal) | GridRenderer uses `_BaseColor` MaterialPropertyBlock |
| **Cinemachine** (com.unity.cinemachine) | Listed in project tech stack; optional for prototype but expected |

After importing TextMeshPro, Unity will prompt you to import **TMP Essential Resources**. You must do this or all TMP_Text components will render blank (white boxes in the build). Select **Import TMP Essentials** from the prompt, or go to **Window → TextMeshPro → Import TMP Essential Resources**.

### No Additional Layers or Tags Required

The prototype uses no custom layers or tags. The default Unity setup is sufficient.

---

## Part 2 — Project Settings

### URP Pipeline Asset

1. Go to **Edit → Project Settings → Graphics**.
2. Under **Scriptable Render Pipeline Settings**, assign a URP Pipeline Asset. If none exists, create one via **Assets → Create → Rendering → URP Asset (with Universal Renderer)**.
3. Assign the same asset under **Edit → Project Settings → Quality** for each quality tier you intend to use.

### TextMeshPro Essential Resources

After adding the TextMeshPro package, immediately import essential resources:

**Window → TextMeshPro → Import TMP Essential Resources**

This creates `Assets/TextMesh Pro/` with the default font assets that `PlanningPhaseUI` and `SpellButtonPanel` depend on.

---

## Part 3 — Scene Hierarchy

Create a new scene named **BattleScene** (File → New Scene → Basic (URP)). Delete the default Directional Light if present (you will add your own or keep it — it does not affect logic). Build the following GameObject tree exactly:

```
BattleScene
├── [Simulation] SimulationBootstrapper
├── [Battle] BattleSceneController
├── [Grid] GridRenderer
├── [Units] UnitViewController
├── [Events] SimulationEventDispatcher
├── [Orchestrator] HotseatOrchestrator
├── [SpellUI] SpellSelectionUI
├── [UI] Canvas
│   ├── PlanningPhaseUI
│   │   ├── StatusLabel          (UI → Text - TextMeshPro)
│   │   ├── BudgetLabel          (UI → Text - TextMeshPro)
│   │   ├── UnitListContainer    (UI → Panel, or empty RectTransform)
│   │   ├── LockPlanButton       (UI → Button - TextMeshPro)
│   │   ├── ClearButton          (UI → Button - TextMeshPro)
│   │   ├── ResolvingOverlay     (UI → Panel)
│   │   └── NextTurnButton       (UI → Button - TextMeshPro)
│   └── SpellButtonPanel
│       └── ButtonContainer      (empty RectTransform — VerticalLayoutGroup recommended)
├── [Boot] BattleSceneBootstrapper
└── Main Camera
```

### Creating Each GameObject

**[Simulation] SimulationBootstrapper**
- **GameObject → Create Empty**, name it `[Simulation] SimulationBootstrapper`
- Add component: `SimulationBootstrapper` (namespace `Battlemancers.Unity`)
- `DataRegistry` is added automatically via `AddComponent` in `SimulationBootstrapper.Awake()`. Do not add it manually.

**[Battle] BattleSceneController**
- **GameObject → Create Empty**, name it `[Battle] BattleSceneController`
- Add component: `BattleSceneController` (namespace `Battlemancers.Presentation`)

**[Grid] GridRenderer**
- **GameObject → Create Empty**, name it `[Grid] GridRenderer`
- Add component: `GridRenderer` (namespace `Battlemancers.Presentation`)
- If no `_tilePrefab` is assigned, GridRenderer falls back to creating primitive Quad GameObjects automatically. No child objects are needed in advance — tiles are instantiated at runtime.

**[Units] UnitViewController**
- **GameObject → Create Empty**, name it `[Units] UnitViewController`
- Add component: `UnitViewController` (namespace `Battlemancers.Presentation`)
- If no `_defaultUnitPrefab` is assigned, UnitViewController falls back to creating Capsule primitives automatically. No child objects needed in advance.

**[Events] SimulationEventDispatcher**
- **GameObject → Create Empty**, name it `[Events] SimulationEventDispatcher`
- Add component: `SimulationEventDispatcher` (namespace `Battlemancers.Presentation`)

**[Orchestrator] HotseatOrchestrator**
- **GameObject → Create Empty**, name it `[Orchestrator] HotseatOrchestrator`
- Add component: `HotseatOrchestrator` (namespace `Battlemancers.Presentation`)

**[SpellUI] SpellSelectionUI**
- **GameObject → Create Empty**, name it `[SpellUI] SpellSelectionUI`
- Add component: `SpellSelectionUI` (namespace `Battlemancers.Presentation`)

**[UI] Canvas**
- **GameObject → UI → Canvas**
- Set **Render Mode** to `Screen Space - Overlay`
- Add a `CanvasScaler` set to **Scale With Screen Size** (reference resolution 1920×1080 is a reasonable starting point)

**PlanningPhaseUI** (child of Canvas)
- **GameObject → Create Empty** as a child of Canvas, name it `PlanningPhaseUI`
- Add component: `PlanningPhaseUI` (namespace `Battlemancers.UI.Battle`)
- Add child UI elements as described in Part 4

**SpellButtonPanel** (child of Canvas)
- **GameObject → Create Empty** as a child of Canvas, name it `SpellButtonPanel`
- Add component: `SpellButtonPanel` (namespace `Battlemancers.UI.Battle`)
- Add a `ButtonContainer` child (empty RectTransform; add a `VerticalLayoutGroup` component for automatic stacking)

**[Boot] BattleSceneBootstrapper**
- **GameObject → Create Empty**, name it `[Boot] BattleSceneBootstrapper`
- Add component: `BattleSceneBootstrapper` (namespace `Battlemancers.Presentation`)
- This is the wiring root. All other scene references flow through it.

**Main Camera**
- Use the default **Main Camera** from the new scene, or create one via **GameObject → Camera**
- Configuration is covered in Part 6

---

## Part 4 — Inspector Assignments

Assign every `[SerializeField]` field by dragging GameObjects from the Hierarchy onto the corresponding slot in the Inspector. Fields are listed by their exact C# name as they appear in the Inspector (Unity strips the leading `_` and capitalizes automatically, so `_sim` appears as **Sim**).

### SimulationBootstrapper (`[Simulation] SimulationBootstrapper`)

| Inspector Field | Assign |
|---|---|
| **Mancer Data Sub Path** | `data/mancers` (default, do not change) |
| **Map Data Sub Path** | `data/maps` (default, do not change) |

These are string fields, not object references. They are pre-populated with correct defaults from the field initializers in code. Only change them if you move the StreamingAssets folders.

### BattleSceneController (`[Battle] BattleSceneController`)

| Inspector Field | Assign |
|---|---|
| **Sim** | Drag `[Simulation] SimulationBootstrapper` |
| **Grid Renderer** | Drag `[Grid] GridRenderer` |
| **Unit View Controller** | Drag `[Units] UnitViewController` |

### GridRenderer (`[Grid] GridRenderer`)

| Inspector Field | Assign |
|---|---|
| **Tile Prefab** | Optional. Leave null to use the automatic Quad fallback. |
| **Sim** | Drag `[Simulation] SimulationBootstrapper` |

### UnitViewController (`[Units] UnitViewController`)

| Inspector Field | Assign |
|---|---|
| **Default Unit Prefab** | Optional. Leave null to use the automatic Capsule fallback. |

### SimulationEventDispatcher (`[Events] SimulationEventDispatcher`)

No `[SerializeField]` fields. This component has no Inspector assignments — it subscribes to `SimulationEventBus` automatically in `Awake()`.

### HotseatOrchestrator (`[Orchestrator] HotseatOrchestrator`)

No `[SerializeField]` fields. All dependencies are injected at runtime by `BattleSceneBootstrapper.Start()` via `SetDependencies()`. No Inspector assignments required.

### SpellSelectionUI (`[SpellUI] SpellSelectionUI`)

No `[SerializeField]` fields. All dependencies are injected at runtime by `BattleSceneBootstrapper.Start()` via `SetDependencies()`. No Inspector assignments required.

### PlanningPhaseUI (child of Canvas)

This component requires child UI GameObjects. Create them as described in Part 3, then assign:

| Inspector Field | Assign |
|---|---|
| **Controller** | Drag `[Battle] BattleSceneController` |
| **Sim** | Drag `[Simulation] SimulationBootstrapper` |
| **Budget Label** | Drag the `BudgetLabel` TMP_Text child |
| **Status Label** | Drag the `StatusLabel` TMP_Text child |
| **Lock Plan Button** | Drag the `LockPlanButton` Button child |
| **Clear Button** | Drag the `ClearButton` Button child |
| **Unit List Container** | Drag the `UnitListContainer` Transform child |
| **Unit Entry Prefab** | Assign a prefab containing a `Button` + `TMP_Text` child (see note below) |
| **Resolving Overlay** | Drag the `ResolvingOverlay` GameObject child |
| **Next Turn Button** | Drag the `NextTurnButton` Button child |

**Unit Entry Prefab note:** Create a prefab with a root `Button` component and a child `TMP_Text`. This is the row template for the unit list. A minimal setup: create a UI Button in the scene, add a TMP_Text child, drag it to `Assets/Prefabs/UnitEntryRow.prefab`, then delete the scene instance. If no prefab is assigned, `RebuildUnitList` silently skips building the list (the null check is on line 183 of PlanningPhaseUI.cs).

### SpellButtonPanel (child of Canvas)

| Inspector Field | Assign |
|---|---|
| **Spell Button Prefab** | Assign a prefab containing a `Button` + `TMP_Text` child. Optional — falls back to creating inline buttons if null. |
| **Button Container** | Drag the `ButtonContainer` RectTransform child |

### BattleSceneBootstrapper (`[Boot] BattleSceneBootstrapper`)

This is the most important component. Every field must be assigned or you will get `LogWarning` messages and features will not activate.

| Inspector Field | Assign |
|---|---|
| **Sim** | Drag `[Simulation] SimulationBootstrapper` |
| **Battle Controller** | Drag `[Battle] BattleSceneController` |
| **Grid Renderer** | Drag `[Grid] GridRenderer` |
| **Unit View Controller** | Drag `[Units] UnitViewController` |
| **Event Dispatcher** | Drag `[Events] SimulationEventDispatcher` |
| **Planning Phase UI** | Drag the `PlanningPhaseUI` GameObject (child of Canvas) |
| **Spell Button Panel** | Drag the `SpellButtonPanel` GameObject (child of Canvas) |
| **Hotseat Orchestrator** | Drag `[Orchestrator] HotseatOrchestrator` |
| **Spell Selection UI** | Drag `[SpellUI] SpellSelectionUI` |

---

## Part 5 — StreamingAssets Setup

`SimulationBootstrapper.Awake()` reads JSON data from `Application.streamingAssetsPath`, which maps to `Assets/StreamingAssets/` in the Unity project. The sub-paths are controlled by the two string fields on SimulationBootstrapper (`_mancerDataSubPath` defaults to `data/mancers`, `_mapDataSubPath` defaults to `data/maps`).

Create this folder structure under `Assets/StreamingAssets/`:

```
Assets/
└── StreamingAssets/
    └── data/
        ├── mancers/
        │   ├── pyromancer.json
        │   ├── hydromancer.json
        │   ├── aeromancer.json
        │   ├── chronomancer.json
        │   ├── cryomancer.json
        │   ├── crystalomancer.json
        │   ├── echomancer.json
        │   ├── electromancer.json
        │   ├── faunamancer.json
        │   ├── floramancer.json
        │   ├── geomancer.json
        │   ├── gravimancer.json
        │   ├── necromancer.json
        │   ├── osteomancer.json
        │   ├── photomancer.json
        │   ├── psychomancer.json
        │   ├── sonimancer.json
        │   ├── thermomancer.json
        │   └── toximancer.json
        └── maps/
            ├── crossroads.json
            ├── flooded-ruins.json
            ├── obsidian-spire.json
            ├── ember_ridge.json
            └── frozen_wastes.json
```

### Source Files

Copy from the repository:

- **Mancer JSON files:** `assets/data/mancers/*.json` → `Assets/StreamingAssets/data/mancers/`
- **Map JSON files:** `assets/data/maps/*.json` → `Assets/StreamingAssets/data/maps/`

The prototype only strictly requires `pyromancer.json` and `hydromancer.json` for the two units that spawn. All other files are loaded silently and indexed — missing files are skipped with a warning log, not an exception.

The `element-interactions.json` file (`assets/data/element-interactions.json`) is loaded separately by `ElementResolver.CreateDefault()` using a hardcoded path relative to the project. Check whether this path needs updating when the project structure moves — the current code uses internal defaults and does not read from StreamingAssets.

---

## Part 6 — Camera Setup

`GridRenderer.GridToWorld()` maps grid positions to world space using the formula:

```
worldPos = new Vector3(x * 1.0f, 0f, z * 1.0f)
```

where `TileWorldSize = 1.0f`. Tiles are laid on the **XZ plane** (Y = 0, with tiles placed at Y = -0.01f to avoid Z-fighting). Units are placed at Y = 0.

For a **10×10 grid**, the tile centers range from `(0, 0, 0)` to `(9, 0, 9)`. The grid center is at approximately `(4.5, 0, 4.5)`.

### Recommended Camera Settings (Top-Down Orthographic)

| Property | Value |
|---|---|
| **Position** | X: 4.5, Y: 12, Z: 4.5 |
| **Rotation** | X: 90, Y: 0, Z: 0 |
| **Projection** | Orthographic |
| **Orthographic Size** | 6 (shows the full 10×10 grid with slight margin) |
| **Near Clip Plane** | 0.1 |
| **Far Clip Plane** | 100 |

To set these values: select `Main Camera` in the Hierarchy, then in the Inspector set the Transform and Camera component values as above.

### Optional Isometric Camera

If you prefer an isometric angle instead of pure top-down:

| Property | Value |
|---|---|
| **Position** | X: 4.5, Y: 10, Z: -2 |
| **Rotation** | X: 60, Y: 0, Z: 0 |
| **Projection** | Orthographic |
| **Orthographic Size** | 7 |

Adjust `Orthographic Size` until the full grid is visible. The isometric angle shows tile depth, which better represents the intended HD-2D look.

---

## Part 7 — First Run Checklist

Press **Play** and verify each step in order. All logs appear in the Unity **Console** window (Window → General → Console).

1. **SimulationBootstrapper.Awake() runs first.**
   - Expected Console log: `[SimulationBootstrapper] Initializing simulation...`
   - Expected: several subsequent logs ending with `[SimulationBootstrapper] Simulation initialized. Turn 1 — Planning phase.`
   - Two units are spawned: `p1_pyromancer_0` at grid position (2, 5) and `p2_hydromancer_0` at grid position (7, 5).

2. **GridRenderer.Start() builds the 10×10 tile grid.**
   - Expected: 100 GameObjects named `Tile_0_0` through `Tile_9_9` appear as children of `[Grid] GridRenderer` in the Hierarchy.
   - Expected: tiles render as colored quads. Water tiles at (3,4), (4,4), (4,5), (5,5) appear blue (TileState.Wet). All other tiles appear dark grey (TileState.Normal).

3. **UnitViewController spawns unit visuals.**
   - If `_defaultUnitPrefab` is null: two Capsule primitives appear in the scene at world positions (2, 0, 5) and (7, 0, 5).
   - If a prefab is assigned: two instances of that prefab appear at those positions.

4. **BattleSceneBootstrapper.Start() wires the scene.**
   - Expected Console log: `[BattleSceneBootstrapper] Scene wiring complete. HotseatOrchestrator starting.`
   - Expected Console log: `[HotseatOrchestrator] Planning phase started. Player 1 plans first.`

5. **PlanningPhaseUI shows for Player 1.**
   - Expected: `StatusLabel` reads `Player 1 — Planning`
   - Expected: `BudgetLabel` reads `Budget: 100 / 100 pts`
   - Expected: unit entry buttons for `p1_pyromancer_0` appear in `UnitListContainer` (requires `_unitEntryPrefab` to be assigned).

6. **Click a unit entry button.**
   - Expected: budget decreases by the unit's activation cost (100 pts for a Mancer).
   - Expected: `StatusLabel` updates to show `Player 1 — Planning (1 unit(s) selected)`.

7. **Click "Lock Plan".**
   - Expected: UI switches to Player 2's planning turn.
   - Expected: `StatusLabel` reads `Player 2 — Planning`.
   - Expected Console log: `[HotseatOrchestrator] Player 1 locked plan (...)`.

8. **Player 2 locks their plan.**
   - Expected: `ResolvingOverlay` becomes visible.
   - Expected Console log: `[BattleSceneController] All plans submitted. Starting resolution.`
   - Expected Console log: `[BattleSceneController] Round complete. New turn number: 2.`

9. **Resolution completes.**
   - Expected: `ResolvingOverlay` hides, `NextTurnButton` becomes visible.
   - Expected: `StatusLabel` reads `Round 1 complete. Press Next Turn to continue.`

10. **Click "Next Turn".**
    - Expected: UI resets to Player 1 — Planning for Round 2.

---

## Part 8 — Common Issues and Fixes

### StreamingAssets directory missing or empty

**Symptom:** Console log `[MapLoader] Directory not found: <path>/data/maps` or `[SimulationBootstrapper] DataRegistry maps initialized. Loaded 0 map(s)`.

**Fix:** Create `Assets/StreamingAssets/data/maps/` and copy the JSON files from `assets/data/maps/` in the repository root.

**Symptom:** Console log `[SimulationBootstrapper] No JSON data found for 'pyromancer'. Using defaults: MaxHP=80, MoveRange=3, PointCost=100`.

**Fix:** Copy `assets/data/mancers/pyromancer.json` and `hydromancer.json` to `Assets/StreamingAssets/data/mancers/`. Units will still spawn with fallback stats but spell data will be missing, so SpellButtonPanel will show no buttons.

### TMP Essentials not imported

**Symptom:** All TMP_Text components render as pink/magenta boxes or empty rectangles in Game view.

**Fix:** Go to **Window → TextMeshPro → Import TMP Essential Resources** and click Import. Re-enter Play mode.

### NullReferenceException in BattleSceneBootstrapper

**Symptom:** `NullReferenceException` pointing to `BattleSceneBootstrapper.WireEventDispatcherToPresentation()` or `WireHotseatOrchestrator()`.

**Fix:** One or more `[SerializeField]` fields on `BattleSceneBootstrapper` are not assigned. Check every row in the Part 4 Inspector Assignments table. Each unassigned field also produces a `LogWarning` in the console naming the missing component.

### NullReferenceException in BattleSceneController

**Symptom:** `NullReferenceException` in `BattleSceneController.Start()` or `SubmitPlan()`.

**Fix:** `_sim` on `BattleSceneController` is not assigned. Drag `[Simulation] SimulationBootstrapper` onto the **Sim** field of `[Battle] BattleSceneController`.

### GridRenderer shows no tiles

**Symptom:** No tile GameObjects appear in the Hierarchy under `[Grid] GridRenderer`.

**Fix:** `_sim` on `GridRenderer` is not assigned. GridRenderer logs `[GridRenderer] SimulationBootstrapper is not assigned.` to the console. Drag `[Simulation] SimulationBootstrapper` onto the **Sim** field of `[Grid] GridRenderer`.

### Unit list is empty in PlanningPhaseUI

**Symptom:** PlanningPhaseUI shows budget and status labels but no unit entry buttons.

**Cause A:** `_unitEntryPrefab` is not assigned. Assign a prefab with a Button + TMP_Text child to the **Unit Entry Prefab** field on `PlanningPhaseUI`.

**Cause B:** `_unitListContainer` is not assigned. Drag the `UnitListContainer` Transform onto the **Unit List Container** field.

**Cause C:** `_sim` is not assigned on `PlanningPhaseUI`. The call to `_sim.State.GetUnitsByOwner()` is skipped silently when `_sim` is null.

### SpellButtonPanel shows no spell buttons when a unit is selected

**Symptom:** Clicking a unit entry does not populate the spell panel.

**Cause A:** Mancer JSON files are missing from StreamingAssets. `SpellSelectionUI.GetSpellsForCaster()` returns an empty list when `DataRegistry` has no data for the archetype.

**Cause B:** `SpellSelectionUI` was not wired by `BattleSceneBootstrapper`. Check that `[SpellUI] SpellSelectionUI` is assigned to the **Spell Selection UI** field on `BattleSceneBootstrapper`.

**Cause C:** `_spellButtonPrefab` is not assigned on `SpellButtonPanel`. The fallback inline button creation does work, but it produces unstyled buttons. Assign a proper prefab to get labels and interactable styling.

### Resolution hangs / no round complete log

**Symptom:** After both plans are locked, the resolving overlay stays up indefinitely and no `Round complete` log appears.

**Fix:** Check that `_sim` and `_gridRenderer` are both assigned on `BattleSceneController`. If `TurnManager.ResolveTurn()` throws, the coroutine yields break early and the Phase stays stuck at `Resolving`. Check the Console for any `[BattleSceneController] ResolveTurn failed:` error log.

### URP shader warning on tile materials

**Symptom:** Console shows warnings about `_BaseColor` not being found on the material.

**Fix:** The fallback Quad primitive uses the Standard shader, not URP Lit. Either assign a URP Lit material to `_tilePrefab`, or accept the warning — the code also sets `_Color` as a fallback, so tile coloring still works under the Standard shader with reduced visual fidelity.
