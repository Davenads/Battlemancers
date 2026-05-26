# Warband Builder — UI/UX Design Specification

## Overview

The warband builder is a full-screen pre-game interface where players assemble, save, and manage their warband lists before entering a match. It is accessible from the main menu and from the pre-match lobby screen.

Players may maintain multiple saved warbands simultaneously (e.g., "Aggressive Fire List", "Verdant Control Board", "Horde Stack"). Warbands are serialized as JSON and persisted to `Application.persistentDataPath/warbands/`. All saved warbands are loaded on application startup. Exactly one warband is designated as "active" — this is the list carried into the next match.

The builder is built entirely with **UI Toolkit** (per project architecture: UI Toolkit for menus, Canvas for in-game HUD). Faction color theming is applied at the document root so it cascades to all child elements when the faction selection changes.

---

## Section 1: Screen Layout

The builder occupies the full screen and is divided into four named regions: a left panel, center panel, right panel, and a bottom bar.

```
+--------------------+-------------------------+--------------------+
|                    |                         |                    |
|   ROSTER BROWSER   |    ACTIVE WARBAND       |   UNIT DETAIL      |
|   (Left Panel)     |    (Center Panel)       |   (Right Panel)    |
|                    |                         |                    |
|                    |                         |                    |
|                    |                         |                    |
|                    |                         |                    |
|                    |                         |                    |
+--------------------+-------------------------+--------------------+
|                        BOTTOM BAR                                 |
+-------------------------------------------------------------------+
```

### Left Panel — Roster Browser

A scrollable list of all available units for the currently selected faction. Units are grouped into two sections separated by a divider with a section label:

**Section: Mancers**
- All 19 Mancer archetypes are always shown, regardless of faction.
- Each Mancer is displayed as a compact card containing:
  - Name (e.g., "Pyromancer")
  - Point cost ("100 pts" base; shows current cost if already in warband with upgrades)
  - Primary element icon (colored icon glyph, e.g., a flame for Fire, a snowflake for Ice)
  - Tactical identity — a single-line descriptor shown as a subtitle on the card (e.g., "DoT, area denial, spreading terrain fire")
- Cards for Mancers already present in the Active Warband are shown with a colored "In Warband" badge and reduced opacity (70%). They remain clickable to remove.
- The roster list does not scroll horizontally — all cards are full-width single-column entries.

**Section: Support Units**
- Shows Chaff and Ranged unit types available for the currently selected faction (T1 and T2 variants).
- Each unit card shows: name, tier badge (T1 / T2), point cost, and a one-line tactical description.
- Support units are not added individually from this panel — they are adjusted via counters in the Active Warband panel. The roster entries here are informational and serve as the hover target for the Unit Detail panel.
- Hovering or clicking a support unit entry populates the Unit Detail panel with that unit's full data.

**Search / Filter Bar** (top of Left Panel):
- A text input field for filtering by name.
- A row of element filter icons (fire, water, ice, earth, wind, lightning, etc.) that toggle to narrow the Mancer list by primary element.
- Filter state persists for the duration of the session but resets on screen exit.

### Center Panel — Active Warband

The live build being constructed. Contains:

**Point Budget Bar** (top of panel):
- Displays "680 / 1,000 pts" as a numeric readout.
- Behind the text, a horizontal fill bar depletes left-to-right as points are spent. The fill uses a smooth CSS transition animation (not instant) to give a satisfying visual response to adds/removes.
- Color states for the fill bar and counter text:
  - 0–799 pts: neutral (faction accent color at 60% saturation)
  - 800–999 pts: amber warning tone
  - Exactly 1,000 pts: gold color with a subtle pulse glow (satisfying milestone state)
  - Over 1,000 pts: red, counter shows "1,050 / 1,000 pts" (overage explicit)

**Validation Badge** (top right of panel, beside point counter):
- Green checkmark icon + "Ready" label: warband is valid and saveable.
- Red warning icon + short reason string when invalid. Reason strings:
  - "Over budget by X pts"
  - "At least 1 Mancer required"
  - "Add at least one Mancer to save" (empty/neutral state — no icon, just muted prompt text)

**Mancer Slots** (3 slots, always visible even when empty):
- Each slot is a fixed-height card region.
- Empty state: dashed border, centered placeholder text "Click a Mancer to add" with a faint plus icon.
- Filled state: shows Mancer name, element icon, base cost + upgrade cost breakdown (e.g., "Pyromancer — 100 + 50 pts upgrades = 150 pts"), and a remove button (X) in the top-right corner of the slot.
- Each filled slot has an **Upgrades** expand button at the bottom of the slot card (see Section 4).
- Slots are ordered top-to-bottom. Units fill the first available empty slot.

**Support Units Section** (below Mancer slots):
- Two rows, one for Chaff and one for Ranged.
- Each row shows: faction-specific unit name, tier icons (T1 and T2 separately), and +/- counter buttons.
- T1 and T2 counts are tracked separately. Example layout:

  ```
  Chaff
    Conscript Spearmen (T1)   [−] 24 [+]   240 pts
    Iron Vanguard (T2)        [−]  2 [+]    40 pts

  Ranged
    Crossbow Corps (T1)       [−]  6 [+]   150 pts
    Siege Arbalest (T2)       [−]  0 [+]     0 pts
  ```

- The − button is disabled (greyed) when count is 0.
- Counts have no hard cap other than the budget ceiling. Attempting to add a unit when doing so would exceed 1,000 pts shows a brief shake animation on the point counter and does not increment the count.
- Point subtotals update in real time as counters change.

**Hover behavior in Active Warband:**
- Hovering over any filled Mancer slot or any support unit row populates the Right Panel with that unit's detail view.

### Right Panel — Unit Detail

Populated when any unit is hovered or selected in either the Roster Browser or the Active Warband panel. Defaults to an empty state ("Select a unit to view details") when nothing is hovered.

Content when a Mancer is displayed:
- Name and primary element (large heading with element color accent)
- Stat block: HP, Move range, AP (action points), any resistances or elemental affinities
- Spell list: each spell as a sub-card showing name, AP cost, range, description, and `temperatureDelta` effect if non-zero
- Tactical identity paragraph (expanded version of the roster card's one-liner)
- Available upgrades summary: list of upgrade names and costs (full detail shown inline in Active Warband when a slot is expanded — this is a read-only preview)
- Faction synergy note if applicable (e.g., "Strong synergy with The Ashen Covenant — Grave Husks absorb Burning terrain")

Content when a support unit is displayed:
- Name, tier badge, point cost, activation cost
- Stat block: HP, Move, Attack, Defense, special passive
- Faction trait note (Iron Discipline / Terrain Bond / Deathless Ranks)
- T1 vs T2 comparison if T2 variant exists: brief delta description (e.g., "T2 Iron Vanguard: +HP, +armor, Shield Wall passive, spear reach retained")
- Any element interaction notes (e.g., Grave Husks: "Regenerate 1 HP/turn in Burning, Poisoned, or Corrupted terrain")

### Bottom Bar

A fixed-height bar spanning the full width at the bottom of the screen. Contains left-to-right:

1. **Warband Name Field** — editable text input. Defaults to "Warband [YYYY-MM-DD HH:MM]" for new warbands. Click to focus and rename. Pressing Enter or clicking away confirms the name.
2. **Active Badge** — a small "ACTIVE" pill badge shown when this warband is the one selected for the next match. Clicking it on a different loaded warband sets it as active.
3. **Save button** — disabled when warband is invalid (see Section 5). Shows "Saved" in green with a brief fade-out animation after a successful save.
4. **New Warband button** — creates a blank warband and loads it into the builder (with unsaved-changes prompt if needed).
5. **Load Warband button** — opens the Warband List Overlay (see Section 2).
6. **Faction Selector** — three faction icon buttons in a row (Gilded Throne crest, Verdant Pact leaf sigil, Ashen Covenant skull motif). The active faction is highlighted with its accent color. Changing faction updates the Support Units section and Roster Browser immediately.
7. **Back to Menu button** — returns to main menu (with unsaved-changes prompt if needed).

---

## Section 2: Warband List Overlay

Triggered by clicking "Load Warband" in the Bottom Bar. Renders as a modal overlay on top of the builder (background darkened at 60% opacity).

**Layout:**
- Overlay header: "Your Warbands" title + a close (X) button in the top-right corner.
- Scrollable list of saved warband cards.
- "New Warband" entry pinned at the top of the list (above saved warbands), styled as a dashed-border add card with a plus icon and "New Warband" label.

**Warband Card (per saved warband):**
- Warband name (large text)
- Faction icon + faction name
- Row of Mancer icons (up to 3; empty slots shown as greyed placeholder icons)
- Point total (e.g., "840 / 1,000 pts")
- Valid/invalid badge (green checkmark or red warning icon)
- "ACTIVE" pill badge if this is the currently active warband
- Action buttons:
  - **Select** — loads this warband into the builder and closes the overlay. Becomes the displayed warband in the center panel.
  - **Set Active** — marks this warband as the active one for the next match without loading it into the editor.
  - **Duplicate** — creates a copy named "[Original Name] Copy" and adds it to the list.
  - **Rename** — makes the name field on this card inline-editable.
  - **Delete** — shows an inline confirmation ("Delete [Name]? This cannot be undone. [Confirm] [Cancel]") before removing.

**Empty State:**
When no saved warbands exist, the list area shows centered text: "No saved warbands yet. Build one and save it to get started."

**Keyboard behavior in overlay:**
- `Escape` closes the overlay without selecting.
- Arrow keys navigate between warband cards.
- `Enter` on a focused card triggers Select.

---

## Section 3: Adding Units Flow

### Adding a Mancer
1. Player clicks a Mancer card in the Roster Browser.
2. If fewer than 3 Mancer slots are filled and that Mancer is not already in the warband: the Mancer is added to the first empty slot. The card in the Roster Browser gains the "In Warband" badge. The point counter updates.
3. If all 3 slots are already filled: the clicked card briefly shakes with a "Warband full (3 Mancers max)" tooltip. No change occurs.
4. If the Mancer is already in the warband: clicking its Roster Browser card triggers removal (see below).

### Removing a Mancer
- Click the X button on a Mancer slot in the Active Warband panel, OR
- Click the Mancer's card in the Roster Browser when it already has the "In Warband" badge.

If the Mancer has one or more upgrades toggled on, a confirmation dialog appears inline above the slot: "Remove [Name]? Their upgrades will be lost. [Remove] [Cancel]". If no upgrades are attached, removal is immediate with no confirmation.

After removal, the slot returns to its empty dashed state. Remaining slots do not reorder — the gap stays in place. If the player added Mancer A to slot 1 and Mancer B to slot 2, removing Mancer A leaves slot 1 empty and Mancer B in slot 2.

### Adding and Removing Support Units
- +/- counter buttons in the Active Warband panel control counts independently for T1 Chaff, T2 Chaff, T1 Ranged, and T2 Ranged.
- Each press of + increments the count by 1 and adds the unit's cost to the running total.
- If adding 1 unit would exceed 1,000 pts, the + button is disabled (greyed) and hovering it shows a tooltip: "Not enough budget remaining".
- Each press of − decrements the count by 1. − is disabled at count 0.
- Point total updates immediately with each press; the budget bar animates smoothly to the new value.

### Changing Faction
- Clicking a different faction icon in the Bottom Bar:
  1. Updates the Support Units section to show the new faction's Chaff and Ranged unit types.
  2. Resets all support unit counts to 0 (since the unit types have changed).
  3. Updates the Roster Browser support unit cards to the new faction's units.
  4. Applies the new faction's accent color theme to the UI.
  5. Updates the point counter (support units were zeroed, so total drops accordingly).
- If the player had counts on the old faction's units, a single brief confirmation prompt appears: "Changing faction will reset your support unit counts. Continue? [Yes] [Cancel]". If the player had 0 support units, the change is instant with no prompt.

---

## Section 4: Mancer Upgrade Flow

Upgrades are managed inline within each Mancer slot in the Active Warband panel. There is no modal or separate screen for upgrades.

**Expand / Collapse:**
- Each filled Mancer slot has an "Upgrades" toggle button at the bottom of the slot card, displaying a down-arrow chevron and the label "Upgrades (X active)" where X is the number of currently toggled-on upgrades.
- Clicking the button expands an upgrade list beneath the slot card. The center panel scrolls to accommodate the expanded content. Clicking again collapses it.
- Only one Mancer slot can have its upgrades expanded at a time. Expanding a second slot collapses the first.

**Upgrade List:**
Each upgrade entry in the list shows:
- Upgrade name (e.g., "Inferno Variant", "Heat Aura", "Scorched Earth Signature")
- Upgrade type badge (Spell Variant / Passive Trait / Stat Enhancement / Signature Ability)
- Cost: "+25 pts" displayed next to the name
- Description: 1–2 sentence explanation of what the upgrade does
- A toggle checkbox/button on the right side of the entry

**Toggle Behavior:**
- Clicking a toggled-off upgrade turns it on, adds its cost to the warband total, and recalculates the point counter immediately.
- Clicking a toggled-on upgrade turns it off, subtracts its cost, and recalculates immediately.
- The Mancer slot's displayed cost updates to reflect toggled upgrades: "Pyromancer — 100 + 50 pts = 150 pts total".
- The "Upgrades (X active)" label on the expand button updates in real time.

**Upgrade Unavailability:**
- If toggling on an upgrade would push the warband total over 1,000 pts, the upgrade entry is shown with red cost text, the toggle button is disabled and grayed out, and hovering it shows a tooltip: "Costs X pts — you are Y pts over budget if enabled."
- As the player adjusts other units and budget frees up, previously red upgrades become available automatically (no page refresh required — UI is reactive).

---

## Section 5: Validation States

Validation is evaluated reactively on every change to the warband (unit added/removed, upgrade toggled, support count changed). The validation badge and save button state update immediately.

### State: Empty Warband
- Condition: no Mancers in any slot, all support counts at 0.
- Point counter: "0 / 1,000 pts" (neutral, unstyled).
- Validation badge: none shown; instead, muted placeholder text below the Mancer slots reads "Add at least one Mancer to save."
- Save button: disabled.

### State: Valid — Under Budget
- Condition: at least 1 Mancer, total points <= 1,000.
- Point counter: neutral or faction-accent color.
- Validation badge: green checkmark + "Ready" label.
- Save button: enabled.

### State: Valid — Exactly At Budget
- Condition: total points == 1,000.
- Point counter: gold color, with a brief pulse glow animation triggered at the moment the total hits exactly 1,000.
- The budget fill bar shows completely full in gold.
- Validation badge: green checkmark + "Ready" label.
- Save button: enabled.

### State: Over Budget
- Condition: total points > 1,000.
- Point counter: red, displaying "1,050 / 1,000 pts" (actual over-budget number shown explicitly).
- Budget fill bar overflows its container slightly and turns red (visual overshoot indicator).
- Validation badge: red warning icon + "Over budget by X pts".
- Save button: disabled.

### State: No Mancers (Support Units Only)
- Condition: all Mancer slots empty but support unit counts > 0 (possible if player removes all Mancers after adding support).
- Point counter: shows current support unit total, in neutral color.
- Validation badge: red warning icon + "At least 1 Mancer required".
- Save button: disabled.

### Duplicate Mancer Prevention
- Duplication is prevented at the point of interaction rather than flagged post-add.
- A Mancer already in the warband cannot be added again. Its Roster Browser card shows the "In Warband" badge and clicking it triggers removal instead of a duplicate add.
- No validation warning is needed for this state; the UI makes the state visually unambiguous.

---

## Section 6: Save / Load Behavior

### Save

On Save button press:
1. Validation is re-checked. If invalid, the button remains disabled and does not trigger (belt-and-suspenders guard, since the button should already be disabled).
2. The warband is serialized to JSON.
3. The file is written to `Application.persistentDataPath/warbands/<warband-id>.json` where `<warband-id>` is a stable GUID assigned on warband creation.
4. The Save button briefly changes to a green "Saved" state with a checkmark, then fades back to its default state after ~1.5 seconds.
5. The warband card in the overlay list is updated to reflect any name or composition changes.

### Auto-Save

Every time the warband changes (unit added/removed, upgrade toggled, support count changed, name edited), the warband is auto-saved silently if it meets the minimum validity threshold (at least 1 Mancer). The auto-save trigger fires 800ms after the last change (debounced, not on every keypress for name edits).

A subtle "Saved" indicator appears in the bottom bar near the warband name field — a small grey checkmark with the text "Auto-saved" that fades out over 2 seconds. It does not appear prominently enough to distract; it is a peripheral confirmation only.

Invalid warbands (e.g., over budget) are not auto-saved. The auto-save indicator does not appear in those states.

### Load

Loading a warband from the Warband List Overlay:
1. If the current builder state has unsaved changes relative to the last saved snapshot of the current warband, an unsaved-changes dialog appears (see Unsaved Changes Prompt below).
2. If no unsaved changes (or the player confirms they want to discard), the selected warband's data is loaded into the builder.
3. All panels update to reflect the loaded warband: Mancer slots populate, support unit counts update, faction selector switches to the loaded warband's faction, point counter recalculates.
4. The overlay closes.

### Unsaved Changes Prompt

Triggers when the user attempts to:
- Click "Load Warband" and select a different warband when the current one has unsaved modifications.
- Click "New Warband" when the current one has unsaved modifications.
- Click "Back to Menu" when the current warband has unsaved modifications.

Dialog appearance: a centered modal dialog (smaller than the overlay — more of a confirmation pop-up) with:
- Title: "Unsaved Changes"
- Body: "You have unsaved changes to '[Warband Name]'. What would you like to do?"
- Three buttons: **Save and Continue**, **Discard Changes**, **Cancel**

Button behavior:
- Save and Continue: saves the current warband, then proceeds with the triggering action (load/new/back).
- Discard Changes: discards the current state, proceeds with the triggering action.
- Cancel: closes the dialog, returns to the builder with no changes.

### Warband Name Defaults

New warbands default to the name "Warband [YYYY-MM-DD HH:MM]" using the creation timestamp. Players can rename at any time by clicking the name field in the Bottom Bar. The rename is committed on Enter or focus-out. An empty name field is not accepted — if the player clears the field and leaves focus, the name reverts to the previous value.

---

## Section 7: Keyboard Shortcuts (Desktop)

| Shortcut | Action |
|---|---|
| `Escape` | Close the currently open overlay (Warband List, upgrade panel, confirmation dialog). If no overlay is open, trigger the "Back to Menu" action (with unsaved-changes prompt). |
| `Ctrl+S` | Save the current warband. No-op if the warband is invalid. Plays the same "Saved" animation as the button press. |
| `Ctrl+N` | Open the "New Warband" flow (with unsaved-changes prompt if applicable). |
| `Del` | When a Mancer slot is focused (keyboard focus ring visible on slot), removes that Mancer from the slot. If the Mancer has upgrades, shows the inline confirmation first. |
| `Tab` / `Shift+Tab` | Navigate focusable elements: name field, faction icons, panel cards, counter buttons. Standard browser-style tab order. |
| `Arrow keys` | Navigate within a panel (e.g., scroll through Roster Browser cards when the panel has focus). |
| `Enter` / `Space` | Activate focused button or toggle. |

All shortcuts are disabled while a text input field has focus, except `Escape` (which unfocuses the input without closing overlays).

---

## Section 8: Visual Design Notes

### Framework

The warband builder is implemented in **UI Toolkit**. All layout uses USS (Unity Style Sheets). Faction theming is applied by swapping a CSS custom property set at the root VisualElement — child elements reference `var(--faction-accent-primary)`, `var(--faction-accent-secondary)`, and `var(--faction-accent-glow)` for all faction-colored elements. Swapping faction swaps three variables; the entire UI recolors with no additional logic.

### Faction Color Themes

| Faction | --faction-accent-primary | --faction-accent-secondary | --faction-accent-glow |
|---|---|---|---|
| The Gilded Throne | Gold (#C9A84C) | Deep Crimson (#8B1A1A) | Warm amber glow |
| The Verdant Pact | Forest Green (#2E6B3A) | Amber (#C87941) | Soft green-gold glow |
| The Ashen Covenant | Deep Violet (#5B2D8E) | Bone White (#E8E0D0) | Cold violet glow |

Faction accent colors are applied to:
- The active faction selector button border and background tint
- The budget fill bar color (non-overbudget states)
- Mancer slot borders when filled
- Section header text in the Roster Browser
- Bottom bar dividers and Save button accent

### Mancer Card Element Colors

Each Mancer card in the Roster Browser has a left-edge colored border (3px) keyed to their primary element:

| Element | Color |
|---|---|
| Fire | #E84A1F |
| Water | #2A8FC4 |
| Ice | #91D4E8 |
| Earth | #8B6340 |
| Wind | #A8D4A0 |
| Lightning | #F0D020 |
| Poison/Toxin | #6BBF44 |
| Death/Necrotic | #6A3F8E |
| Light | #F5F0A0 |
| Mind/Psychic | #D080C0 |
| Plant | #3A8040 |
| Beast | #A07840 |
| Bone | #D4C8A8 |
| Gravity | #404060 |
| Sound | #80C0D0 |
| Crystal | #A0D0E8 |
| Echo | #8090B0 |
| Temperature | #C04020 (hot side) / #2060A0 (cold side) — gradient border |

### Disabled / Unavailable State

All disabled or unavailable interactive elements use:
- 40% opacity on the element itself.
- Cost text displays with strikethrough (CSS `text-decoration: line-through`).
- Cursor changes to `not-allowed` on hover.
- No hover highlight or active state styling applied.

This applies to:
- Upgrades that would exceed budget.
- The − counter button at count 0.
- The + counter button when budget is exhausted.
- The Save button when warband is invalid.

### Budget Bar Animation

The budget fill bar uses a CSS `transition: width 300ms ease-out` so that adding or removing a unit produces a smooth fill change rather than an instant jump. The transition duration is 300ms — fast enough to feel responsive, slow enough to register as a satisfying "weight" to each decision.

When the bar crosses from under-budget into the exact 1,000 pt milestone, a one-shot pulse glow animation plays on the bar and the point counter text (keyframe animation: scale 1.0 → 1.08 → 1.0 over 400ms, gold glow expands and fades).

When the bar crosses into over-budget, the bar immediately snaps red (no easing on the color change — the abrupt shift communicates the error state urgency).

### Typography

- Panel headers: semi-bold, faction-accent-primary color, 14px uppercase with letter-spacing
- Mancer/unit names on cards: bold, 15px
- Cost values: monospaced or tabular-figures numeric font to prevent layout shift as numbers change
- Stat block values in Unit Detail: right-aligned in a two-column label/value layout
- Validation messages: 13px, red (#C03030) for errors, green (#2A8A3A) for ready state
- Tooltip text: 12px, dark background (#1A1A1A at 90% opacity), white text, 4px border radius

### Transitions and Micro-animations

- Upgrade expand/collapse: the upgrade list height animates from 0 to full height over 200ms (CSS max-height transition). Collapse reverses.
- "In Warband" badge on Roster Browser cards: fades in over 150ms when a Mancer is added; fades out over 150ms when removed.
- Validation badge state changes: cross-fade over 200ms between valid/invalid icons.
- Overlay open/close: the overlay background darkens over 150ms; the overlay card scales from 0.95 to 1.0 and fades in over 200ms. Close reverses.
- Save button "Saved" flash: text changes to "Saved" with green color, holds for 1.2 seconds, then cross-fades back to "Save" over 300ms.
