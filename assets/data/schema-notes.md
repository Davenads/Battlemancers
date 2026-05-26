# Data Schema Architecture Notes

## File Inventory

| File | Type | Location |
|---|---|---|
| SpellData.cs | ScriptableObject | src/data/ |
| UpgradeOption.cs | ScriptableObject | src/data/ |
| MancerData.cs | ScriptableObject | src/data/ |
| TileTypeData.cs | ScriptableObject | src/data/ |
| FactionData.cs | ScriptableObject | src/data/ |
| WarbandSave.cs | Pure C# (JSON) | src/data/ |
| element-interactions.json | JSON data table | assets/data/ (authored separately) |

---

## ScriptableObjects — What, Why, and How

### Which files are ScriptableObjects

SpellData, UpgradeOption, MancerData, TileTypeData, and FactionData are all Unity ScriptableObjects (inherit from `UnityEngine.ScriptableObject`).

### Why ScriptableObjects for these files

ScriptableObjects are the correct tool for static game configuration data that:

- Is authored once and does not change at runtime
- Benefits from the Unity Inspector for designer-facing editing (drag-drop spell references, sliders for stat ranges, color pickers for faction colors)
- Needs to be referenced by other assets (a MancerData asset references its SpellData assets by direct Unity asset reference, not by string key — this gives compile-time type safety and eliminates broken reference bugs)
- Lives in the project as addressable assets rather than embedded in MonoBehaviours

The key constraint: **ScriptableObject assets must never be mutated at runtime.** They are shared assets — if you write to a field on a SpellData SO at runtime, that change persists across play sessions in the editor and is undefined behavior in a build. All runtime state (current HP, active cooldowns, applied status effects) lives in pure C# simulation classes, not in SOs.

### How ScriptableObject references are resolved at runtime

Two approaches are used depending on context:

**Direct Inspector assignment:** MancerData.baseSpells is a `SpellData[]` field. Unity serializes these as direct asset references. When MancerData is loaded, its spell references are already resolved — no lookup needed. The same applies to FactionData referencing SupportUnitData (inline struct, not a separate SO).

**Addressables / Resources.Load for broad registry access:** At game start, a `DataRegistry` MonoBehaviour loads all MancerData, SpellData, TileTypeData, and FactionData assets from an Addressables group (or from `Resources/Data/` folders in early development). These are indexed into dictionaries keyed by their ID strings (e.g., `Dictionary<string, MancerData>` keyed by mancerId). The simulation layer can then resolve a mancerArchetypeId string from a WarbandSave into its MancerData asset via a single dictionary lookup.

Addressables is preferred over `Resources.Load` for production: it supports async loading, build-time asset stripping, and content updates. `Resources.Load` is acceptable during prototyping.

---

## WarbandSave — Pure C# JSON Serialization

### Why WarbandSave is not a ScriptableObject

WarbandSave represents player-specific runtime data — it is created during play, modified when the player edits their warband, and must persist to disk between sessions. ScriptableObjects cannot cleanly serve this role:

- SOs are project assets, not player data. Serializing player saves as SO assets would pollute the project file structure and would not work in a shipped game without a content bundle pipeline.
- SOs cannot easily be created dynamically at runtime in a shipping build without Addressables overhead.
- JSON is universally portable: player saves can be backed up, inspected, migrated between versions, and synced via Steam Cloud Saves without Unity involvement.

WarbandSave uses only base C# types (`string`, `int`, `long`, `List<T>`, `bool`) with no Unity dependencies. This makes it trivially serializable by any JSON library and headless-executable (can be deserialized in a server context or unit test without Unity).

### JSON serialization approach

**Simple saves (development / prototyping):** `UnityEngine.JsonUtility.ToJson(save)` / `JsonUtility.FromJson<WarbandSave>(json)`. Works for flat structures but does not support computed properties or polymorphic types. Sufficient while the warband schema is stable.

**Production:** `Newtonsoft.Json` (Json.NET for Unity, free on Asset Store). Advantages over JsonUtility:
- Supports `List<T>` nested inside `List<T>` correctly (JsonUtility has known edge cases with nested generics)
- `[JsonIgnore]` to exclude computed properties from serialization (TotalPointCost, IsValid etc.)
- `ISerializationBinder` for forward-compatible schema migration (reads an older schemaVersion and transforms the JSON before deserialization)
- Better error reporting on malformed saves

The `WarbandSave.schemaVersion` field exists specifically for forward compatibility. When the schema changes incompatibly, increment `schemaVersion` in code, write a migration handler in `WarbandMigrationService`, and `WarbandLoader` runs the migration on load if `save.schemaVersion < currentVersion`.

### Runtime-to-save boundary

When the player finishes editing in the warband builder:

1. The warband builder's ViewModel holds a live `WarbandSave` object being mutated directly (it is pure C# — safe to mutate).
2. On save, `WarbandSave.MarkModified()` updates the timestamp.
3. `WarbandLoader.Save(save)` serializes to JSON and writes to `Application.persistentDataPath/warbands/{saveId}.json`.
4. `WarbandSaveIndex` is updated and re-serialized to `warband-index.json`.

On load:
1. `WarbandLoader.LoadIndex()` deserializes `warband-index.json` for fast listing.
2. On warband selection, `WarbandLoader.Load(saveId)` reads the individual save JSON.
3. `WarbandLoader` resolves all string ID references to ScriptableObject assets via the DataRegistry.
4. The resolved `ResolvedWarband` (a separate runtime class holding actual SO references) is handed to the simulation.

---

## Static Definition vs. Runtime State

A critical architectural boundary: **SpellData** (ScriptableObject) is the spell's static definition. It holds what the spell does. It does not hold what state the spell is currently in.

Runtime spell state — which spells are on cooldown, how many turns remain on each cooldown — belongs in the simulation layer's `UnitState` class:

```csharp
// In the simulation layer (pure C#, no Unity dependencies):
class UnitState {
    string unitId;
    int currentHP;
    int remainingMoveRange;
    Dictionary<string, int> spellCooldowns; // spellId -> turns remaining
    List<ActiveStatus> activeStatuses;
}
```

The same pattern applies to terrain: `TileTypeData` (ScriptableObject) defines what a tile state does. The simulation's `GridData.Tile` struct holds what state a tile is actually in right now:

```csharp
struct Tile {
    Vector2Int position;
    TileState state;       // enum value — resolved to TileTypeData via DataRegistry when needed
    int elevation;
    int stateRemainingDuration; // ticks down each turn; 0 = permanent
    bool passable;
    bool occupied;
    string occupantId;
}
```

---

## Element Interaction Matrix — Why JSON, Not a ScriptableObject

The element interaction matrix (what happens when Fire hits a Wet tile, Lightning hits a Frozen tile, etc.) is defined in `assets/data/element-interactions.json`, not as ScriptableObjects.

The reason is structural: the interaction matrix is a 2D data table — rows are existing tile states, columns are incoming element types, cells are the resulting interaction (new tile state + effects + VFX hint). This is inherently tabular, not hierarchical.

ScriptableObjects model hierarchical or entity-style data well (a Mancer has spells; a spell has effects). They are awkward for 2D matrix data:
- You would need one SO per cell (hundreds of assets), or one giant SO with a flat list that designers must navigate without row/column structure.
- JSON lets a designer or scripter view, edit, and diff the entire matrix in one file with clear row/column semantics.

`ElementResolver.cs` (simulation layer) loads `element-interactions.json` at startup via `DataRegistry` and builds a `Dictionary<(TileState, ElementType), Interaction>` lookup table. Adding a new interaction requires only editing the JSON file — no code change, no recompile, no new asset.

The `Interaction` struct the JSON deserializes into mirrors the simulation's needs exactly:

```csharp
struct Interaction {
    string resultingTileState;   // TileState enum value name
    int damageBonus;             // extra damage applied on interaction
    string[] statusEffects;      // StatusType enum value names applied to units in area
    string vfxHint;              // tag sent to VFXDirector
    string sfxHint;              // tag sent to AudioDirector
}
```

This is the only data file that the simulation layer loads directly. All other data (SpellData, MancerData, etc.) is loaded by the presentation layer's DataRegistry and passed into the simulation as resolved C# values.
