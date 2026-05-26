# Warband Builder — UI/UX Design Specification

## Document Purpose

This spec defines the complete UI/UX for the warband list builder: the interface where players
create, edit, save, and manage their warband lists before a match. It is written for a developer
implementing the feature and should answer all design questions without follow-up.

**Tech stack:** UI Toolkit (UXML/USS). No Canvas. No MonoBehaviours for layout logic.
**Data layer:** `DataRegistry` for Mancer/spell/unit data. `WarbandSaveManager` for persistence.
**Validation:** `WarbandValidator` (pure C#) runs on save and is queried for live feedback.

---

## 1. Overview and Entry Points

The warband builder is accessed from three locations in the game. The core interface is identical
in all three; only the available exit actions differ.

### 1.1 Entry from Main Menu

Navigation path: Main Menu → "My Warbands" button.

- Lands on the **Warband List Management Screen** (Section 2).
- Exit: "Back" returns to main menu.
- Match context is absent — the "Save & Play" button is hidden on the Review screen. Only
  "Save Warband" is shown.

### 1.2 Entry During Game Mode Selection

Navigation path: Mode Selection Screen → "Create Warband" button, or "Edit Warband" beside an
already-selected warband entry.

- "Create Warband" opens the builder flow at Step A (blank warband).
- "Edit Warband" opens the Warband List Management Screen with the named warband pre-highlighted;
  clicking Edit from there enters the builder flow with that warband pre-loaded.
- Exit: "Save & Play" is visible on the Review screen and, on save, returns to mode selection with
  the new warband pre-selected. "Cancel" at any step discards unsaved changes and returns to mode
  selection.

### 1.3 Entry from Campaign Hub

Navigation path: Campaign Hub → "Adjust Warband" button (visible during pre-mission prep only).

- Whether warband editing is permitted depends on the mission. If the mission locks composition,
  the "Adjust Warband" button is greyed out with tooltip: "Warband locked for this mission."
- When permitted, faction and Mancer archetype selections are locked (greyed out, non-interactive).
  A persistent banner at the top of the builder reads: "Faction and Mancer choices are locked for
  this campaign run. You may adjust upgrades and support unit counts."
- Exit: "Confirm Changes" saves and returns to campaign hub. "Cancel" discards and returns.

---

## 2. Warband List Management Screen

This is the hub screen showing all saved warbands. It is the first screen shown when entering from
main menu or mode selection.

### 2.1 Screen Layout

```
+----------------------------------------------------------------+
|  MY WARBANDS                            [+ New Warband]        |
|  Sort: [Newest v]                                              |
+----------------------------------------------------------------+
|  [Warband Card]    [Warband Card]    [Warband Card]            |
|  [Warband Card]    [Warband Card]    [Warband Card]            |
|  [Warband Card]    [Warband Card]    ...                       |
|  (scrollable grid)                                             |
+----------------------------------------------------------------+
|  [< Back]                                                      |
+----------------------------------------------------------------+
```

Cards are laid out in a 3-column grid on 16:9 and wider viewports. On narrower viewports
(e.g., 4:3 or windowed), fall back to 2 columns. The grid is vertically scrollable when more
cards are present than fit in the viewport.

### 2.2 Warband Card

Each saved warband is a card containing:

- **Warband name** — large text, top of card
- **Faction icon + faction name** — icon left-aligned, name beside it; color-tinted to faction
  accent (see Section 8.2)
- **Mancer portrait thumbnails** — row of up to 3 small portrait icons; empty slots render as
  grey silhouette placeholders
- **Total point cost** — e.g., "850 / 1000 pts"
- **Last modified date** — e.g., "Modified May 24, 2026"
- **Action row** (bottom of card): [Edit] [Duplicate] [Rename] [Delete]

Card visual states:
- Default: neutral border, light background with a subtle faction-color tint at 10% opacity
- Hover: elevated drop shadow, border brightens
- Selected (in mode-selection context only): faction-colored border at full opacity, checkmark
  badge in top-right corner of card

### 2.3 Per-Warband Actions

**Edit:** Opens the builder flow at Step B (Mancer Selection) with all saved choices pre-loaded.
Faction is pre-selected but can be changed.

**Duplicate:** Immediately creates a copy named "[Original Name] (Copy)", places it at the top of
the list (Newest sort). No confirmation required.

**Rename:** Replaces the name text on the card with an inline text input, pre-filled with the
current name. Pressing Enter or clicking away commits the new name. Pressing Escape cancels and
restores the original name. Same 32-character validation as the name field in Step A.

**Delete:** Shows a confirmation modal before deleting (see Section 7.3). On confirm, deletes the
warband JSON file, removes the entry from the manifest, and removes the card from the list with a
brief fade-out animation.

### 2.4 New Warband Button

Positioned in the top-right of the screen header, always visible. Clicking it starts the builder
flow at Step A with a blank warband. When the saved warband count is at the 20-warband limit, the
button is disabled and shows tooltip: "Warband limit reached (20/20). Delete an existing warband
to create a new one."

### 2.5 Saved Warband Limit

Soft limit of 20 saved warbands. Enforced client-side in `WarbandSaveManager`. The limit can be
increased in a future update without changing the file format — it is a constant, not hardcoded.

### 2.6 Sort Options

Dropdown control at top-left of the list area. Default: Newest. Options:

| Option | Behavior |
|---|---|
| Newest | Sort by `lastModified` descending |
| Oldest | Sort by `lastModified` ascending |
| Alphabetical (A–Z) | Sort by warband name ascending |
| Alphabetical (Z–A) | Sort by warband name descending |
| Point Cost (High–Low) | Sort by total points descending |
| Point Cost (Low–High) | Sort by total points ascending |
| Faction | Group by faction, alphabetical within group |

Selected sort preference is saved to `PlayerPrefs` and restored on next launch.

---

## 3. Warband Builder Flow

The builder is a 5-step linear flow. A persistent step indicator bar is shown at the top of the
screen during all steps:

```
  [1 Name & Faction]  >  [2 Mancers]  >  [3 Upgrades]  >  [4 Support]  >  [5 Review]
```

- Completed steps: filled circle indicator
- Current step: highlighted circle indicator
- Future steps: empty circle indicator (non-interactive)
- Clicking a completed step navigates directly back to it

The **Point Budget Display** (Section 4) is always visible in the top-right corner during all
five steps.

---

### Step A: Name and Faction Selection

**Purpose:** Name the warband and choose a faction. Both are required to advance.

#### Layout

```
+----------------------------------------------------------------+
|  Step 1 of 5: Name Your Warband & Choose a Faction            |
+----------------------------------------------------------------+
|  Warband Name:  [_____________________________]  14 / 32       |
+----------------------------------------------------------------+
|                                                                |
|  [Gilded Throne Card]  [Verdant Pact Card]  [Ashen Covenant]  |
|                                                                |
+----------------------------------------------------------------+
|  [Cancel]                           [Next: Choose Mancers >]  |
+----------------------------------------------------------------+
```

#### Warband Name Field

- Single-line text input. Placeholder: "Enter warband name..."
- Max length: 32 characters. Character counter displayed to the right of the field: "14 / 32"
- Allowed: letters (A–Z, a–z), digits (0–9), spaces, hyphens (-), apostrophes ('). Disallowed
  characters are silently rejected — the field does not accept them; no error shown for this.
- Validation on Next: field must not be empty. If empty, the field outline turns red and a message
  appears below: "Warband name is required."
- When editing an existing warband, the field is pre-filled with the saved name.

#### Faction Cards

Three cards in a horizontal row, one per faction. Each card contains:

- **Faction name** (large heading)
- **Faction icon** (unique heraldic emblem per faction)
- **Tagline** (italic, one line):
  - Gilded Throne: *"Order. Discipline. The line holds."*
  - Verdant Pact: *"The forest remembers every scar."*
  - Ashen Covenant: *"Death is not the end. It is the resource."*
- **Faction Trait summary** (one line):
  - Gilded Throne: "Iron Discipline — support units immune to Panic and Charm"
  - Verdant Pact: "Terrain Bond — bonus movement and regen on natural tiles"
  - Ashen Covenant: "Deathless Ranks — no morale; Chaff deaths generate Necromancer fuel"
- **Chaff unit names** (two lines): "Chaff T1: [name]" / "Chaff T2: [name]"
- **Ranged unit names** (two lines): "Ranged T1: [name]" / "Ranged T2: [name]"
- **Synergy tip** (muted text, bottom of card):
  - Gilded Throne: "Pairs well with Psychomancer (your units are immune to its own debuffs)"
  - Verdant Pact: "Pairs well with Floramancer and Geomancer"
  - Ashen Covenant: "Pairs well with Necromancer and Pyromancer"

Card interaction:
- Click to select. Only one faction may be selected at a time.
- Selected: border highlights in faction accent color, background tinted to faction color at 20%
  opacity, checkmark badge in top-right corner of card.
- Unselected: neutral border, neutral background.
- Hover: slight elevation shadow, pointer cursor.

Faction is required to advance. If "Next" is clicked with no faction selected, the faction area
gains a red outline and a message appears below: "Select a faction to continue."

When editing an existing warband, the saved faction card is pre-selected. Changing faction when
re-editing shows a confirmation modal warning that support unit counts will be reset (see
Section 7.3).

---

### Step B: Mancer Selection

**Purpose:** Choose up to 3 Mancer archetypes for the warband.

#### Layout

```
+----------------------------------+--------------------------------+
|  MANCER ROSTER                   |  MANCER DETAIL                 |
|  [Search: _______________]  [x]  |                                |
|  [All] [Fire] [Water] [Ice]      |  Pyromancer                    |
|  [Earth] [Wind] [Lightning]      |  Element: Fire                 |
|  [Death] [Time] [Light] ...      |  Base cost: 100 pts            |
|                                  |                                |
|  [Card] [Card] [Card]            |  DoT, area denial, spreading   |
|  [Card] [Card] [Card]            |  terrain fire. Excels at zone  |
|  [Card] [Card] [Card]            |  control and forcing enemies   |
|  [Card] [Card] [Card]            |  off key tiles...              |
|  [Card] [Card] [Card]            |                                |
|  [Card] [Card] [Card]            |  BASE SPELLS                   |
|  [Card] [Card] [Card]            |  - Fireball: ...               |
|                                  |  - Scorch: ...                 |
|                                  |  - Immolation: ...             |
|                                  |                                |
|                                  |  SYNERGIES                     |
|                                  |  - Pairs well with Hydromancer |
|                                  |  - Strong in Ashen Covenant    |
+----------------------------------+--------------------------------+
|  Slots: [Slot 1 ────────] [Slot 2 ────────] [Slot 3 ────────]  |
+----------------------------------------------------------------+
|  [< Back]                        [Next: Configure Upgrades >]  |
+----------------------------------------------------------------+
```

#### Mancer Grid (Left Panel)

A scrollable 3-column grid of Mancer portrait cards. Each card shows:

- Portrait thumbnail (placeholder: colored rectangle with element-color accent until art exists)
- Mancer name
- Element icon + element name (text label mandatory — not icon only)
- Base cost: "100 pts"
- Tactical identity: one-line blurb (truncate with ellipsis at card edge)

Card states:
- **Default:** neutral border, light background
- **Hover:** elevated shadow, pointer cursor; detail panel updates to this Mancer
- **Selected (in warband):** faction-colored border, slot badge on portrait ("Slot 1" / "2" / "3"),
  checkmark in corner
- **Disabled (warband full):** 50% opacity, cursor `not-allowed`, tooltip: "Warband is full.
  Remove a Mancer to add another."

Click behavior:
- Clicking an unselected card when fewer than 3 Mancers are chosen: adds the Mancer to the next
  available slot.
- Clicking a selected card (already in warband): removes it from the warband and clears the slot.
  If the Mancer has upgrades configured (from a prior edit session), show inline confirmation:
  "Remove [Name]? Their upgrades will be lost. [Remove] [Cancel]"
- Slots are contiguous — removing a Mancer from Slot 2 shifts Slot 3 down to Slot 2 (no gaps).

**Filter bar:** Row of pill buttons above the grid. Single-select. "All" shows all 19 Mancers.
Each element button filters the grid to Mancers of that type. Active filter pill is highlighted.

Element filter buttons:

| Label | Mancers |
|---|---|
| All | All 19 |
| Fire | Pyromancer |
| Water | Hydromancer |
| Ice | Cryomancer |
| Earth | Geomancer |
| Wind | Aeromancer |
| Lightning | Electromancer |
| Death | Necromancer |
| Time | Chronomancer |
| Light | Photomancer |
| Mind | Psychomancer |
| Nature | Floramancer, Faunamancer |
| Poison | Toximancer |
| Bone | Osteomancer |
| Gravity | Gravimancer |
| Sound | Sonimancer |
| Crystal | Crystalomancer |
| Echo | Echomancer |
| Heat | Thermomancer |

**Search bar:** Text input at top of left panel. Filters by Mancer name (case-insensitive substring
match). Clear button (X) inside the field. Search and element filter are additive.

#### Mancer Slot Bar (Bottom)

Three slot panels always visible at the bottom of the screen throughout Steps B and C.

- **Empty slot:** dashed border, grey silhouette icon, label "Empty Slot"
- **Filled slot:** Mancer portrait thumbnail, Mancer name, current cost (base 100 pts; reflects
  upgrades when returning from Step C)
- Clicking a filled slot during Step B removes the Mancer (with upgrade-loss confirmation if
  applicable)

#### Detail Panel (Right Side)

Shows detail for the Mancer currently hovered or last clicked in the grid.

Default state (nothing hovered): "Hover a Mancer to view details." in muted centered text.

When a Mancer is shown:
- Name (heading)
- Element icon + element name (text label)
- "Base cost: 100 pts"
- Tactical identity paragraph (3–5 sentences)
- Base spells list (bullet list: spell name, brief effect, temperature effect if `temperatureDelta`
  is non-zero — e.g., "Fireball — deals fire damage, temperatureDelta: +3")
- Synergies (2–3 bullet points)
- Faction synergy note if applicable

The detail panel does not need to scroll for the current data volume. If content overflows in the
future, the panel becomes internally scrollable with a visible scrollbar.

"Next: Configure Upgrades" is disabled if zero Mancers are in the warband. Tooltip on disabled
button: "Add at least one Mancer to continue."

---

### Step C: Mancer Upgrades

**Purpose:** Configure optional upgrades for each selected Mancer.

Accessing this step: clicking "Next" from Step B, or clicking a filled Mancer slot during the
flow (slots remain visible and clickable at the bottom of all steps).

#### Layout

```
+----------------------------------------------------------------+
|  UPGRADES          [Pyromancer Tab] [Electromancer Tab] [—]   |
+----------------------------------+-----------------------------+
|  Pyromancer                      |  COST BREAKDOWN             |
|  Base cost: 100 pts              |                             |
|                                  |  Base:             100 pts  |
|  HP: —  Move: —  AP: —           |  Inferno Aspect:   +25 pts  |
|                                  |  Blazing Form:     +20 pts  |
|  BASE SPELLS                     |  ─────────────────────────  |
|  - Fireball (...)                |  Mancer total:     145 pts  |
|  - Scorch (...)                  |                             |
|  - Immolation (...)              |                             |
|                                  |                             |
|  SPELL VARIANTS         [+15–25] |                             |
|  [x] Inferno Aspect     +25 pts  |                             |
|      Replaces Fireball with ...  |                             |
|  [ ] Searing Nova       +20 pts  |                             |
|      Replaces Scorch with ...    |                             |
|                                  |                             |
|  PASSIVE TRAITS         [+20–30] |                             |
|  [x] Blazing Form       +20 pts  |                             |
|      Immune to Burning terrain   |                             |
|  [ ] Pyromaniac         +25 pts  |                             |
|      +1 AP when adj. to fire...  |                             |
|                                  |                             |
|  STAT ENHANCEMENTS      [+10–20] |                             |
|  [ ] Reinforced Frame   +15 pts  |                             |
|      +20 max HP                  |                             |
|                                  |                             |
|  SIGNATURE ABILITY      [+25–50] |                             |
|  [ ] Conflagration      +50 pts  |                             |
|      Once per match: ...         |                             |
+----------------------------------+-----------------------------+
|  [< Back to Mancers]             [Next: Support Units >]       |
+----------------------------------------------------------------+
```

#### Mancer Tabs

One tab per Mancer slot, shown at the top of the upgrade panel. Tab label: Mancer name, or "Empty"
if the slot is unfilled. Unfilled tabs are greyed out and non-interactive. Clicking a tab switches
the upgrade panel to that Mancer.

#### Upgrade Toggle Rows

Each upgrade is a full-width row containing:
- Checkbox (left)
- Upgrade name
- Cost badge (right-aligned): "+X pts"
- Description (below the name, muted text, 1–2 lines)

Click anywhere on the row to toggle. The checkbox reflects the toggled state.

Toggle ON: checkbox fills, row background gets a light tint, cost is added to the right sidebar
and the global budget display.

Toggle ON when it would exceed 1,000 pts: toggle does not activate. Inline message appears below
the row: "Not enough budget (+X pts needed)." Cost badge displays in red. Resolves automatically
when budget frees up elsewhere.

#### Upgrade Categories

Four labeled sections, each collapsible via a chevron in the section header. Default: all
expanded. Collapse state is per-session (not persisted). Cost range shown in section header
as a hint.

| Section | Cost Range |
|---|---|
| Spell Variants | +15–25 pts |
| Passive Traits | +20–30 pts |
| Stat Enhancements | +10–20 pts |
| Signature Ability | +25–50 pts |

If a Mancer has no upgrades in a given category, that section is hidden entirely for that Mancer.

A "Reset Upgrades" button appears in the top-right of the upgrade panel (beside the Mancer tab
row). Clicking it shows a confirmation modal (see Section 7.3) before clearing all active upgrades
for the current Mancer.

#### Cost Breakdown Sidebar (Right)

Displays a running cost breakdown for the Mancer currently shown in the left panel:
- One line per active upgrade: upgrade name, cost
- Subtotal line: "Mancer total: X pts"
- Updates live as upgrades are toggled

This sidebar is Mancer-specific. The global budget display (Section 4) in the header reflects
the entire warband.

#### Implementation Note

Upgrade options are data-driven. The panel reads upgrade definitions from `DataRegistry`
(via `MancerData` ScriptableObjects). No upgrade names or costs are hardcoded in UI logic.
Specific upgrade options per Mancer archetype are TBD (per `design/warbands.md`) — the panel
renders whatever the data provides.

---

### Step D: Support Unit Allocation

**Purpose:** Set counts for each faction-specific support unit type (T1/T2 Chaff and T1/T2 Ranged).

#### Layout

```
+----------------------------------------------------------------+
|  SUPPORT UNITS — The Gilded Throne                             |
+----------------------------------------------------------------+
|  CHAFF UNITS                                                   |
|  +-----------------------------+  +---------------------------+ |
|  | Conscript Spearmen    T1   |  | Iron Vanguard        T2  | |
|  | 10 pts each                |  | 20 pts each              | |
|  | HP: —  Move: —  Atk: —     |  | HP: —  Move: —  Atk: —   | |
|  | Spear reach; no special    |  | Shield Wall; spear reach  | |
|  | ability                    |  |                           | |
|  |  [−]  [   24   ]  [+]      |  |  [−]  [    0   ]  [+]    | |
|  | Subtotal: 240 pts          |  | Subtotal: 0 pts           | |
|  +-----------------------------+  +---------------------------+ |
|                                                                |
|  RANGED UNITS                                                  |
|  +-----------------------------+  +---------------------------+ |
|  | Crossbow Corps        T1   |  | Siege Arbalest       T2  | |
|  | 25 pts each                |  | 50 pts each              | |
|  | HP: —  Move: —  Rng: —     |  | HP: —  Move: —  Rng: —   | |
|  | Armor pierce; alt turns    |  | No reload; can brace      | |
|  |  [−]  [    8   ]  [+]      |  |  [−]  [    0   ]  [+]    | |
|  | Subtotal: 200 pts          |  | Subtotal: 0 pts           | |
|  +-----------------------------+  +---------------------------+ |
+----------------------------------------------------------------+
|  [< Back]                              [Next: Review >]        |
+----------------------------------------------------------------+
```

#### Unit Cards

Each of the four unit types has a card showing:
- Unit name and tier badge (T1 / T2)
- Cost per unit: "X pts each"
- Key stats: HP, Movement, Attack or Range (values from `DataRegistry`)
- Key ability summary (1–2 lines)
- Stepper control: [−] count [+]
- Per-unit subtotal: "N × X pts = Y pts", updates live

#### Stepper Behavior

- Minimum count: 0. The [−] button is disabled at 0.
- Maximum count: no hard cap other than budget ceiling.
- The [+] button is disabled when adding one more unit would exceed 1,000 pts. Tooltip on
  disabled [+]: "Not enough budget."
- The count display is directly editable: clicking it transforms into a number input. If the
  manually-entered value would exceed the budget, it snaps to the maximum affordable count and
  shows a brief inline note: "Adjusted to fit budget."
- Counts update the global budget display in real time.

---

### Step E: Review and Save

**Purpose:** Show the complete warband summary, surface any errors or warnings, and save.

#### Layout

```
+----------------------------------------------------------------+
|  REVIEW YOUR WARBAND                                           |
+----------------------------------------------------------------+
|  [Faction Icon]  Iron Sentinels          The Gilded Throne     |
+----------------------------------------------------------------+
|  MANCERS (3)                                                   |
|  [Portrait] Pyromancer        145 pts   Inferno Aspect,        |
|                                         Blazing Form           |
|  [Portrait] Electromancer     100 pts   (no upgrades)         |
|  [Portrait] Cryomancer        120 pts   Glacial Resilience     |
+----------------------------------------------------------------+
|  SUPPORT UNITS                                                 |
|  Conscript Spearmen (T1)   × 24  =  240 pts                   |
|  Iron Vanguard (T2)        ×  0  =    0 pts                   |
|  Crossbow Corps (T1)       ×  8  =  200 pts                   |
|  Siege Arbalest (T2)       ×  0  =    0 pts                   |
+----------------------------------------------------------------+
|  TOTAL: 805 / 1000 pts                    195 pts remaining    |
+----------------------------------------------------------------+
|  [Warnings/Errors panel — if any]                              |
+----------------------------------------------------------------+
|  [< Back to Edit]     [Save Warband]        [Save & Play]      |
+----------------------------------------------------------------+
```

#### Warband Summary

- **Header row:** warband name (large), faction icon, faction name
- **Mancers section:** one row per filled slot. Each row: portrait thumbnail, Mancer name, total
  cost (base + upgrades), comma-separated upgrade names. If no upgrades: "(no upgrades)" in
  muted text.
- **Support Units section:** one row per unit type (all four always shown, even at count 0).
  Zero-count rows are displayed in muted text — they are not hidden, so the player sees they
  are intentionally empty.
- **Total row:** "TOTAL: X / 1000 pts" and "Y pts remaining". Colors per Section 4 rules.

#### Errors and Warnings Panel

Errors are blocking (prevent save). Warnings are advisory (do not prevent save).

**Errors (red-bordered panel, blocking):**

| Condition | Message |
|---|---|
| Total > 1,000 pts | "Warband exceeds the 1,000-point cap. Remove units or upgrades before saving." |
| Zero Mancers | "You must include at least one Mancer." |
| Duplicate Mancer archetypes | "Duplicate Mancers found: [name]. Each archetype may appear only once." |

When any error is present: "Save Warband" and "Save & Play" are disabled. The step indicator
highlights the step(s) responsible for the error (e.g., Step B highlight for a Mancer error).

**Warnings (yellow-bordered panel, non-blocking):**

| Condition | Message |
|---|---|
| Total < 500 pts | "Your warband is very small (X pts). Consider adding more units." |
| No Ranged units | "You have no ranged support. Enemy ranged units may engage freely." |
| No Chaff units | "You have no Chaff. Your Mancers will be directly exposed to melee." |

Warnings shown: player may save without any additional confirmation. Warnings are advisory only.

#### Save Buttons

- **"Back to Edit":** Returns to Step D (or whichever step was last visited). All choices preserved.
- **"Save Warband":** Calls `WarbandValidator`. If valid, writes warband JSON and manifest, shows
  toast notification "Warband saved." (2-second auto-dismiss at bottom of screen), returns to the
  Warband List Management Screen.
- **"Save & Play":** Visible only when entering from mode selection or campaign hub. Same as Save
  Warband but on success proceeds to match setup instead of returning to the list screen.

No save confirmation dialog. Save is not a destructive action.

---

## 4. Point Budget Display

The budget display is a persistent widget in the top-right corner of the screen, visible at all
times during the five builder steps.

### Widget Appearance

```
+------------------------------+
|  750 / 1000 pts              |
|  + 250 pts remaining         |
+------------------------------+
```

Both lines update live as the player makes any change. The widget has a colored border matching
the current budget state.

### Color States

| Condition | Color applied |
|---|---|
| Total <= 900 pts | Green (#4CAF50) |
| Total 901–1000 pts | Yellow (#FFC107) |
| Total > 1000 pts | Red (#F44336) |

Color applies to: the "X / 1000 pts" text, and the border of the budget widget.

The "pts remaining" line reads negative when over budget: e.g., "− 50 pts over budget" in red.

### Breakdown Tooltip

Hovering the budget display (or focusing it via keyboard and pressing Space/Enter) shows a tooltip:

```
Mancers:          365 pts
  Pyromancer        145 pts
  Electromancer     100 pts
  Cryomancer        120 pts
Chaff:            240 pts
Ranged:           200 pts
──────────────────────────
Total:            805 pts
Remaining:        195 pts
```

Tooltip dismisses on focus loss or Escape. It is keyboard accessible (Tab to focus the widget,
Space/Enter to open, Escape to close).

---

## 5. Validation Rules

### 5.1 Blocking Errors (Prevent Save)

| Rule | Live feedback | On-save feedback |
|---|---|---|
| Total > 1,000 pts | Budget display turns red immediately | Error message in Review panel |
| Zero Mancers | "Next" in Step B is disabled with tooltip | Error message in Review panel |
| Duplicate Mancer archetypes | Prevented by UI; add of a duplicate removes the existing one instead | Validated by `WarbandValidator` as safety check |
| Warband name empty | "Next" in Step A is disabled with inline message | N/A — blocked before reaching Review |

### 5.2 Advisory Warnings (Non-blocking)

| Rule | When shown |
|---|---|
| Total < 500 pts | Review screen only |
| No Ranged units | Review screen only |
| No Chaff units | Review screen only |

Warnings appear on the Review screen only — they are not shown as live feedback during Steps A–D.
The budget cap is the only live blocking feedback during building (the [+] stepper disables and
upgrade toggles refuse when the cap is hit).

### 5.3 Validation Responsibility Split

`WarbandValidator` (pure C# — zero Unity dependencies) is the source of truth for all validation
logic. The UI calls `WarbandValidator.Validate(WarbandDraft)` and reads the returned
`ValidationResult`. No validation logic is duplicated in UI controller code.

Live feedback during building (disabled [+] buttons, disabled Next buttons) is computed directly
from `WarbandDraft.TotalPoints` compared to the cap constant — this is arithmetic, not validation
logic, and does not bypass `WarbandValidator`.

---

## 6. Warband Sharing (Future Feature)

Not required at launch. The save format must accommodate it without future breaking changes.

### 6.1 Export (Future)

- "Share Code" button on warband cards (list screen) and Review screen.
- Generates a version-prefixed base64-encoded string from the warband JSON:
  `BMW1:[base64encodedJSON]` (Battlemancers Warband v1).
- Modal displays the code string with a "Copy to Clipboard" button and a QR code thumbnail.

### 6.2 Import (Future)

- "Import from Code" button on the Warband List Management Screen, beside "New Warband".
- Modal with text input: "Paste warband code here..."
- On confirm: decodes, validates (version prefix, JSON structure, all Mancer IDs present in
  `DataRegistry`, point total within cap, no duplicate Mancers), adds as new warband named
  "[Imported] [original name]".
- On failure: error shown in modal. Specific reason stated (e.g., "Unknown Mancer archetype:
  'Blademancer'").

### 6.3 Version Field

Each warband JSON includes a `"version": 1` field. Future format changes increment this value.
`WarbandSaveManager` contains a migration function that upgrades old versions on load. The importer
checks the version prefix of share codes before attempting to decode.

---

## 7. Accessibility and UX

### 7.1 Keyboard Navigation

All interactive elements are reachable via Tab. Tab order follows visual reading order (left to
right, top to bottom) within each step.

Step-specific tab orders:

**Step A:**
1. Warband name field
2. Gilded Throne faction card
3. Verdant Pact faction card
4. Ashen Covenant faction card
5. Cancel button
6. Next button

**Step B:**
1. Search field
2. Element filter buttons (left to right)
3. Mancer grid cards (row by row, left to right)
4. Mancer slot bar entries (Slot 1, Slot 2, Slot 3)
5. Back button
6. Next button

**Step C:**
1. Mancer tabs (Slot 1, Slot 2, Slot 3)
2. Upgrade rows within current tab (top to bottom)
3. Reset Upgrades button
4. Back button
5. Next button

**Step D:**
1. T1 Chaff stepper: [−], count field, [+]
2. T2 Chaff stepper: [−], count field, [+]
3. T1 Ranged stepper: [−], count field, [+]
4. T2 Ranged stepper: [−], count field, [+]
5. Back button
6. Next button

**Step E:**
1. Back to Edit button
2. Save Warband button
3. Save & Play button (if shown)

Global keyboard shortcuts active during all builder steps:

| Key | Action |
|---|---|
| Escape | Go back one step. If on Step A, opens "Cancel building?" prompt. If a modal is open, closes modal first. |
| Ctrl+S | Save warband (no-op if invalid; same behavior as pressing Save Warband) |
| Tab / Shift+Tab | Forward / backward focus navigation |
| Enter / Space | Activate focused button; select focused faction card; toggle focused upgrade row |
| Arrow keys | Navigate within a grid (Mancer cards in Step B) when grid has focus |

Shortcuts are suppressed while a text input field has keyboard focus, except Escape (which
removes focus from the field without triggering navigation).

### 7.2 Icon and Label Requirements

All icons in the warband builder must be accompanied by a visible text label. Icon-only display
is not permitted.

- Faction icons: always shown with faction name text beside the icon
- Element type icons in filter pills: icon + element name text in every pill button
- Element type on Mancer cards: icon + text below icon
- Slot badges: text label ("Slot 1", "Slot 2", "Slot 3"), not a number badge alone
- Tier badges: text "T1" or "T2", not a colored dot

Element type icons must use both **color** and a **unique shape/symbol** to distinguish types.
Color alone is not sufficient (colorblind players must be able to distinguish element types by
shape). Do not define element type identity by color only anywhere in the builder.

### 7.3 Confirmation Modals for Destructive Actions

All destructive or potentially-data-losing actions require a confirmation modal before executing.

**Delete warband:**
```
+------------------------------------------------+
|  Delete "Iron Sentinels"?                      |
|                                                |
|  This cannot be undone.                        |
|                                                |
|  [Cancel]                    [Delete]          |
+------------------------------------------------+
```
Default focus: Cancel. Delete button styled in red. Enter activates focused button. Escape
dismisses modal (same as Cancel).

**Reset upgrades for a Mancer:**
```
+------------------------------------------------+
|  Reset upgrades for Pyromancer?                |
|                                                |
|  All upgrades will be removed.                 |
|                                                |
|  [Cancel]                    [Reset]           |
+------------------------------------------------+
```
Default focus: Cancel.

**Change faction on an existing warband (when faction was previously saved):**
```
+------------------------------------------------+
|  Change faction to Verdant Pact?               |
|                                                |
|  Your support unit counts will be reset to 0   |
|  because unit types have changed.              |
|                                                |
|  [Cancel]             [Change Faction]         |
+------------------------------------------------+
```
Default focus: Cancel. This modal is shown only when re-editing a warband and changing away
from the saved faction. It is not shown when changing faction during initial creation before
any support units have been added (zero counts require no warning).

### 7.4 Tooltips

Disabled interactive elements always display a tooltip on hover explaining why they are disabled.
Tooltips appear after a 400ms hover delay. Tooltip text must be set via the UI Toolkit `tooltip`
attribute (or equivalent mechanism) so screen readers can access it.

All tooltips dismiss on mouse-out or Escape.

---

## 8. Technical Implementation Notes

### 8.1 Technology Constraints

- The warband builder uses **UI Toolkit** exclusively. Do not use Canvas components.
- All layout is defined in UXML templates. All styling is in USS. Do not set colors, margins, or
  font sizes in C# code — use USS class names toggled from C#.
- Faction color theming uses CSS custom properties (USS variables) set at the root VisualElement:
  `--faction-accent-primary`, `--faction-accent-secondary`, `--faction-accent-glow`. Changing
  faction swaps these three variables; the entire builder recolors via cascade with no additional
  C# logic.

Faction color values:

| Faction | --faction-accent-primary | --faction-accent-secondary | --faction-accent-glow |
|---|---|---|---|
| Gilded Throne | Gold (#C9A84C) | Deep Crimson (#8B1A1A) | Warm amber |
| Verdant Pact | Forest Green (#2E6B3A) | Amber (#C87941) | Soft green-gold |
| Ashen Covenant | Deep Violet (#5B2D8E) | Bone White (#E8E0D0) | Cold violet |

### 8.2 Save File Format

Files are stored in `Application.persistentDataPath/warbands/`.

**Manifest file: `manifest.json`**
```json
{
  "version": 1,
  "warbands": [
    { "id": "uuid-1", "name": "Iron Sentinels", "lastModified": "2026-05-24T14:32:00Z" },
    { "id": "uuid-2", "name": "Forest Vanguard", "lastModified": "2026-05-20T09:10:00Z" }
  ]
}
```

**Per-warband file: `{id}.json`**
```json
{
  "version": 1,
  "id": "uuid-1",
  "name": "Iron Sentinels",
  "faction": "GildedThrone",
  "lastModified": "2026-05-24T14:32:00Z",
  "mancers": [
    { "archetype": "Pyromancer", "upgrades": ["InfernoAspect", "BlazingForm"] },
    { "archetype": "Electromancer", "upgrades": [] },
    { "archetype": "Cryomancer", "upgrades": ["GlacialResilience"] }
  ],
  "supportUnits": {
    "chaffT1": 24,
    "chaffT2": 0,
    "rangedT1": 8,
    "rangedT2": 0
  }
}
```

Field constraints:
- `faction`: one of `"GildedThrone"`, `"VerdantPact"`, `"AshenCovenant"`
- `archetype`: matches `MancerData.Id` in `DataRegistry`
- `upgrades`: each string matches an upgrade ID on the corresponding `MancerData`
- `supportUnits`: the four field names are fixed regardless of faction; faction determines what
  the units *are*, not how counts are stored
- `version`: checked on load; future format changes increment this; a migration path in
  `WarbandSaveManager` handles version upgrades

### 8.3 C# Class Architecture

**`WarbandBuilderController`** (MonoBehaviour, builder scene root)
- Owns the step state machine: tracks current step, handles navigation forward/back
- Holds the live `WarbandDraft` (in-progress, not written to disk until Step E save)
- Receives an `EntryContext` enum parameter on scene load: `MainMenu | ModeSelection | CampaignHub`
- The entry context determines: which buttons are visible on Review, whether faction/Mancer
  selection is locked (campaign context), and what the exit action does

**`WarbandDraft`** (pure C#, no MonoBehaviour)
- Mutable representation of the in-progress warband
- Fields: `Name`, `Faction`, `Mancers` (list of archetype + upgrade IDs), `SupportUnits`
- Computed properties: `TotalPoints`, `MancerCount`, `RemainingBudget`
- No validation logic — it is a data container only

**`WarbandSaveManager`** (pure C#, no MonoBehaviour)
- Owns all file I/O for warbands
- Uses `System.Text.Json` with `BattlemancersJsonHelper` for serialization options
- Methods: `LoadManifest()`, `SaveWarband(WarbandDraft)`, `LoadWarband(string id)`,
  `DeleteWarband(string id)`, `DuplicateWarband(string id)`, `GetSavedCount()`
- Generates stable GUIDs for new warbands (`System.Guid.NewGuid().ToString()`)

**`WarbandValidator`** (pure C#, no MonoBehaviour — existing class)
- Called by `WarbandSaveManager.SaveWarband()` before writing
- Also called by `WarbandBuilderController` for the Review screen error/warning display
- Returns `ValidationResult` containing `List<ValidationError>` and `List<ValidationWarning>`
- No validation logic lives in UI controllers

**`WarbandListScreen`** (UI Toolkit controller)
- Reads manifest via `WarbandSaveManager.LoadManifest()`
- Instantiates warband card UXML templates, one per entry
- Handles sort dropdown state and re-sort
- Handles New Warband button, limit check

**`WarbandBuilderScreen`** (UI Toolkit controller)
- Manages the step indicator bar
- Owns the `WarbandDraft` reference
- Coordinates sub-step panels; passes draft by reference so each panel mutates the same object

**Sub-step panels** (each a UXML template with a corresponding UI Toolkit controller):
- `StepNameFactionPanel` — manages name field and faction card selection
- `StepMancerSelectionPanel` — manages Mancer grid, search/filter, slot bar
- `StepUpgradesPanel` — manages upgrade toggles per Mancer tab
- `StepSupportUnitsPanel` — manages stepper controls
- `StepReviewPanel` — reads `WarbandDraft` as read-only, renders summary, calls `WarbandValidator`

### 8.4 DataRegistry Queries Required

The builder requires the following queries on `DataRegistry`. Add any that do not yet exist:

- `DataRegistry.GetAllMancers()` → `IReadOnlyList<MancerData>` — all 19 Mancers; used in Step B grid
- `DataRegistry.GetMancer(string archetypeId)` → `MancerData` — single Mancer; used in detail panel
  and upgrade panel
- `DataRegistry.GetFactionUnits(string faction)` → `FactionUnitSet` — the four unit type
  definitions (T1 Chaff, T2 Chaff, T1 Ranged, T2 Ranged) for a given faction; used in Step D cards

`FactionUnitSet` is a new plain C# struct (or class) containing four `SupportUnitData` references.
`SupportUnitData` is a new ScriptableObject type to be created if not yet present, with fields for:
name, tier, cost, HP, movement, attack/range stats, key ability summary text.

### 8.5 Scene Setup

The warband builder is a dedicated Unity scene (`WarbandBuilderScene`). It is loaded additively
over the calling scene. On exit, it is unloaded.

`WarbandBuilderController` receives its `EntryContext` via a static setup call before the scene
loads (e.g., `WarbandBuilderController.SetEntryContext(EntryContext.ModeSelection)`) or via a
ScriptableObject event channel — whichever pattern is already established in the project.

The budget display widget is a single UXML instance at the scene root, not re-instantiated per
step. It holds a reference to the live `WarbandDraft` and re-reads `TotalPoints` on each change
notification from `WarbandBuilderController`.

### 8.6 Animation Specifications

- Budget fill bar: CSS `transition: width 300ms ease-out` on fill element width
- Budget color change into over-budget (>1000): instant (no easing — abrupt red communicates error)
- Faction card selection: border color fade 150ms
- Upgrade toggle on: row background tint fade-in 100ms
- Upgrade list expand/collapse: CSS `max-height` transition 200ms ease-in-out
- Mancer "In Warband" badge: fade-in 150ms on add, fade-out 150ms on remove
- Modal open: background darkens 150ms; modal card scales 0.95→1.0 and fades in 200ms
- Toast notification ("Warband saved."): slide up from bottom 150ms, hold 2s, fade out 300ms
