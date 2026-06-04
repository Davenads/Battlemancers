using System;
using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Data;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Simulation.Effects
{
    /// <summary>
    /// Applies every downstream effect produced by a spell cast and returns a structured
    /// <see cref="SpellResolutionResult"/> describing what happened.
    ///
    /// <para>
    /// <b>Execution order inside <see cref="Apply"/>:</b>
    /// <list type="number">
    ///   <item>Guard: if the caster has SILENCED, STUNNED, or FROZEN status return
    ///         <see cref="SpellResolutionResult.Blocked"/> immediately.</item>
    ///   <item>Apply base damage to each target, reduced by armor (currently 0).</item>
    ///   <item>Apply temperature delta via <see cref="TemperatureManager"/>.</item>
    ///   <item>Apply status effects from <see cref="SpellData.appliedEffects"/>, using the
    ///         seeded <see cref="Random"/> to gate probabilistic applications.</item>
    ///   <item>Apply terrain changes from <see cref="SpellData.terrainChanges"/> to the
    ///         grid tiles at each target's position.</item>
    ///   <item>Check for element-combo interactions via <see cref="ElementResolver"/> and
    ///         record fired combos.</item>
    ///   <item>Apply displacement if <see cref="DisplacementEffect.hasDisplacement"/> is set.</item>
    ///   <item>Record summon requests if <see cref="SpellData.summonUnitTag"/> is set.</item>
    ///   <item>Return the fully populated <see cref="SpellResolutionResult"/>.</item>
    /// </list>
    /// </para>
    ///
    /// <b>Design contract:</b> <see cref="SpellEffectApplicator"/> applies effects by calling
    /// through to <see cref="StatusManager"/> and <see cref="TemperatureManager"/>. Direct
    /// HP mutation and position mutation happen on the provided <see cref="UnitState"/> objects
    /// (passed by reference through the list). The caller owns the simulation state and is
    /// responsible for deregistering killed units.
    ///
    /// <b>Armor model:</b> No dedicated Armor field exists on <see cref="UnitState"/> yet.
    /// The private constant <see cref="DefaultArmor"/> is the single extension point — update
    /// it (or read it from UnitState) when armor stats are introduced.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class SpellEffectApplicator
    {
        // -----------------------------------------------------------------------------------------
        // Constants
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Default armor value subtracted from base spell damage before application.
        /// Fixed at 0 until UnitState exposes a dedicated Armor property.
        /// </summary>
        private const int DefaultArmor = 0;

        /// <summary>
        /// Minimum damage after armor reduction. Damage is clamped to this floor.
        /// </summary>
        private const int MinimumDamage = 0;

        // -----------------------------------------------------------------------------------------
        // Dependencies (injected via constructor)
        // -----------------------------------------------------------------------------------------

        private readonly StatusManager _statusManager;
        private readonly TemperatureManager _temperatureManager;
        private readonly ElementResolver _elementResolver;
        private readonly Random _random;

        // -----------------------------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="SpellEffectApplicator"/>.
        /// </summary>
        /// <param name="statusManager">
        /// Applies and tracks status effects on units. Must not be null.
        /// </param>
        /// <param name="temperatureManager">
        /// Applies temperature deltas and manages threshold crossings. Must not be null.
        /// </param>
        /// <param name="elementResolver">
        /// Resolves element-tile-state combo interactions. Must not be null.
        /// </param>
        /// <param name="random">
        /// Seeded random instance for probabilistic status application. Must not be null.
        /// Callers are responsible for seeding deterministically (e.g., from match seed).
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        public SpellEffectApplicator(
            StatusManager statusManager,
            TemperatureManager temperatureManager,
            ElementResolver elementResolver,
            Random random)
        {
            _statusManager      = statusManager      ?? throw new ArgumentNullException(nameof(statusManager));
            _temperatureManager = temperatureManager ?? throw new ArgumentNullException(nameof(temperatureManager));
            _elementResolver    = elementResolver    ?? throw new ArgumentNullException(nameof(elementResolver));
            _random             = random             ?? throw new ArgumentNullException(nameof(random));
        }

        // -----------------------------------------------------------------------------------------
        // Primary API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Applies all effects of a spell cast and returns a structured result describing
        /// every mutation that occurred.
        ///
        /// <para>
        /// If the caster has any cast-blocking status (SILENCED, STUNNED, FROZEN), the method
        /// returns <see cref="SpellResolutionResult.Blocked"/> immediately without applying any
        /// effects to <paramref name="state"/> or to the target units.
        /// </para>
        /// </summary>
        /// <param name="spell">
        /// The spell definition. Must not be null.
        /// </param>
        /// <param name="caster">
        /// The unit performing the cast. Must not be null.
        /// </param>
        /// <param name="targets">
        /// The ordered list of target units. May be empty (e.g., terrain-only spells).
        /// Must not be null.
        /// </param>
        /// <param name="state">
        /// The live simulation state. Used for tile queries, turn number stamping, and grid mutations.
        /// Must not be null.
        /// </param>
        /// <returns>
        /// A <see cref="SpellResolutionResult"/> summarising all effects applied.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="spell"/>, <paramref name="caster"/>,
        /// <paramref name="targets"/>, or <paramref name="state"/> is null.
        /// </exception>
        public SpellResolutionResult Apply(
            SpellData spell,
            UnitState caster,
            List<UnitState> targets,
            SimulationState state)
        {
            if (spell   == null) throw new ArgumentNullException(nameof(spell));
            if (caster  == null) throw new ArgumentNullException(nameof(caster));
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (state   == null) throw new ArgumentNullException(nameof(state));

            // ------------------------------------------------------------------
            // Step 1 — Guard: block cast if caster has a cast-preventing status
            // ------------------------------------------------------------------
            if (CasterIsBlocked(caster.Id))
                return SpellResolutionResult.Blocked();

            // ------------------------------------------------------------------
            // Accumulators for building the result
            // ------------------------------------------------------------------
            var damageDealt        = new List<DamageEvent>();
            var statusesApplied    = new List<StatusApplicationEvent>();
            var temperatureChanges = new List<TemperatureEvent>();
            var tileChanges        = new List<TileChangeEvent>();
            var displacements      = new List<DisplacementEvent>();
            var comboEffects       = new List<ComboEffect>();
            var summons            = new List<SummonRequest>();

            // Map the data-layer element to the simulation-layer element for the resolver.
            ElementType simulationElement = MapElementType(spell.element);

            // ------------------------------------------------------------------
            // Per-target effects
            // ------------------------------------------------------------------
            foreach (UnitState target in targets)
            {
                if (!target.IsAlive)
                    continue;

                // --- Step 2: Base damage ---
                int damage = ComputeDamage(spell, target, state);
                if (damage > 0)
                {
                    target.CurrentHP = Math.Max(0, target.CurrentHP - damage);
                    damageDealt.Add(new DamageEvent(target.Id, damage));
                }

                // --- Step 3: Temperature delta ---
                if (spell.temperatureDelta != 0)
                {
                    int prevTemp = target.Temperature;
                    _temperatureManager.ApplyTemperatureChange(
                        target.Id, spell.temperatureDelta, target, state);
                    int newTemp = target.Temperature;

                    temperatureChanges.Add(new TemperatureEvent(
                        target.Id,
                        delta:               spell.temperatureDelta,
                        previousTemperature: prevTemp,
                        newTemperature:      newTemp));
                }

                // --- Step 4: Status effects ---
                if (spell.appliedEffects != null)
                {
                    foreach (StatusEffectApplication application in spell.appliedEffects)
                    {
                        if (application == null)
                            continue;

                        // Skip tile-targeted statuses — those are handled by terrain changes.
                        if (application.appliesToTile)
                            continue;

                        // Gate on applicationChance using the seeded random.
                        if (application.applicationChance < 1.0f
                            && _random.NextDouble() >= application.applicationChance)
                            continue;

                        if (!Enum.TryParse<StatusType>(
                                application.statusType,
                                ignoreCase: true,
                                out StatusType statusType))
                            continue;

                        int duration   = Math.Max(1, application.duration);
                        int stackCount = Math.Max(1, application.stacksApplied);

                        var effect = new StatusEffect(statusType, duration, stackCount, caster.Id);
                        _statusManager.ApplyStatus(target.Id, effect, target, state.TurnNumber);

                        statusesApplied.Add(new StatusApplicationEvent(
                            target.Id,
                            statusType.ToString(),
                            duration));
                    }
                }

                // --- Step 5: Terrain changes at the target's tile ---
                if (spell.terrainChanges != null)
                {
                    foreach (TerrainChangeApplication terrainChange in spell.terrainChanges)
                    {
                        if (terrainChange == null)
                            continue;

                        // Only apply HitTile-targeted changes in the per-target loop.
                        // AdjacentTiles and AllAreaTiles would require a separate AoE pass
                        // (not implemented in this loop — extend when those target types are used).
                        if (terrainChange.changeTarget != TerrainChangeTarget.HitTile)
                            continue;

                        GridPosition pos = target.Position;
                        Tile tile = state.Grid.GetTile(pos);
                        if (tile == null)
                            continue;

                        if (!Enum.TryParse<TileState>(
                                terrainChange.targetTileState,
                                ignoreCase: true,
                                out TileState newTileState))
                            continue;

                        TileState oldTileState = tile.State;

                        // Respect overwriteExistingState flag: skip if occupied and not overwriting.
                        if (!terrainChange.overwriteExistingState && oldTileState != TileState.Normal)
                            continue;

                        if (newTileState == oldTileState)
                            continue;

                        state.Grid.SetTileState(pos, newTileState);

                        tileChanges.Add(new TileChangeEvent(
                            pos.X, pos.Y,
                            oldTileState.ToString(),
                            newTileState.ToString()));
                    }
                }

                // --- Step 6: Element-combo check ---
                Tile targetTile = state.Grid.GetTile(target.Position);
                if (targetTile != null)
                {
                    string tileStateName = targetTile.State.ToString();
                    Interaction interaction = _elementResolver.Resolve(tileStateName, simulationElement);

                    // A non-trivial interaction means a combo fired.
                    if (_elementResolver.HasInteraction(tileStateName, simulationElement)
                        && !string.IsNullOrEmpty(interaction.VfxHint))
                    {
                        comboEffects.Add(new ComboEffect(
                            comboName:          interaction.VfxHint,
                            triggerStateName:   tileStateName,
                            triggerElementName: simulationElement.ToString(),
                            tileX:              target.Position.X,
                            tileY:              target.Position.Y));
                    }
                }

                // --- Step 7: Displacement ---
                if (spell.displacement != null && spell.displacement.hasDisplacement)
                {
                    GridPosition fromPos = target.Position;
                    GridPosition toPos   = ComputeDisplacedPosition(
                        caster.Position, target.Position, spell.displacement.tiles, state.Grid);

                    if (toPos != fromPos)
                    {
                        int tilesMoved = fromPos.ManhattanDistance(toPos);

                        // Update occupancy on the grid.
                        state.Grid.ClearOccupant(fromPos);
                        target.Position = toPos;
                        state.Grid.SetOccupant(toPos, target.Id);

                        displacements.Add(new DisplacementEvent(
                            target.Id,
                            tilesMoved,
                            fromPos.X, fromPos.Y,
                            toPos.X,   toPos.Y));
                    }
                }
            }

            // ------------------------------------------------------------------
            // Step 8: Summon requests (not per-target — spell summons are global)
            // ------------------------------------------------------------------
            if (!string.IsNullOrEmpty(spell.summonUnitTag) && spell.maxSummonCount > 0)
            {
                // Preferred spawn is adjacent to the caster; exact placement is caller-owned.
                GridPosition spawnHint = caster.Position;
                summons.Add(new SummonRequest(
                    unitTag:          spell.summonUnitTag,
                    summonerId:       caster.Id,
                    preferredSpawnX:  spawnHint.X,
                    preferredSpawnY:  spawnHint.Y));
            }

            return new SpellResolutionResult(
                wasCast:            true,
                damageDealt:        damageDealt,
                statusesApplied:    statusesApplied,
                temperatureChanges: temperatureChanges,
                tileChanges:        tileChanges,
                displacements:      displacements,
                comboEffects:       comboEffects,
                summons:            summons);
        }

        // -----------------------------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> if the caster currently has any status that prevents casting:
        /// SILENCED, STUNNED, or FROZEN.
        /// </summary>
        private bool CasterIsBlocked(string casterId)
        {
            return _statusManager.HasStatus(casterId, StatusType.Silenced)
                || _statusManager.HasStatus(casterId, StatusType.Stunned)
                || _statusManager.HasStatus(casterId, StatusType.Frozen);
        }

        /// <summary>
        /// Computes the net damage dealt to <paramref name="target"/> by <paramref name="spell"/>.
        ///
        /// Formula: <c>max(0, baseDamage - armor + conditionalBonuses)</c>.
        ///
        /// Conditional bonuses are evaluated against both the target tile's
        /// <see cref="TileState"/> and the target unit's <see cref="UnitState.ActiveStatusTypes"/>.
        /// </summary>
        private static int ComputeDamage(SpellData spell, UnitState target, SimulationState state)
        {
            int damage = Math.Max(MinimumDamage, spell.baseDamage - DefaultArmor);

            if (spell.conditionalBonuses == null || spell.conditionalBonuses.Length == 0)
                return damage;

            Tile targetTile = state.Grid.GetTile(target.Position);
            string tileStateName = targetTile != null ? targetTile.State.ToString() : string.Empty;

            foreach (ConditionalDamageBonus bonus in spell.conditionalBonuses)
            {
                if (bonus == null || string.IsNullOrEmpty(bonus.triggerState))
                    continue;

                bool triggered =
                    string.Equals(tileStateName, bonus.triggerState, StringComparison.OrdinalIgnoreCase)
                    || target.ActiveStatusTypes.Contains(bonus.triggerState);

                if (!triggered)
                    continue;

                if (bonus.isMultiplicative)
                    damage = (int)(damage * (1f + bonus.bonusDamage / 100f));
                else
                    damage += bonus.bonusDamage;
            }

            return Math.Max(MinimumDamage, damage);
        }

        /// <summary>
        /// Computes the destination tile after displacing <paramref name="targetPos"/> by
        /// <paramref name="tiles"/> steps directly away from <paramref name="casterPos"/>
        /// (positive = push), clamped to grid bounds and stopping at the first impassable tile.
        /// </summary>
        private static GridPosition ComputeDisplacedPosition(
            GridPosition casterPos,
            GridPosition targetPos,
            int tiles,
            GridData grid)
        {
            if (tiles == 0)
                return targetPos;

            // Direction of displacement: positive = push away from caster, negative = pull toward.
            int dx = targetPos.X - casterPos.X;
            int dy = targetPos.Y - casterPos.Y;

            // Normalise to unit step on the dominant axis.
            int stepX, stepY;
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                stepX = tiles >= 0 ? Math.Sign(dx != 0 ? dx : 1) : -Math.Sign(dx != 0 ? dx : 1);
                stepY = 0;
            }
            else
            {
                stepX = 0;
                stepY = tiles >= 0 ? Math.Sign(dy != 0 ? dy : 1) : -Math.Sign(dy != 0 ? dy : 1);
            }

            int absSteps = Math.Abs(tiles);
            GridPosition dest = targetPos;

            for (int i = 0; i < absSteps; i++)
            {
                GridPosition next = new GridPosition(dest.X + stepX, dest.Y + stepY);
                if (!grid.IsInBounds(next))
                    break;

                Tile nextTile = grid.GetTile(next);
                if (nextTile == null || !nextTile.IsPassable)
                    break;

                dest = next;
            }

            return dest;
        }

        /// <summary>
        /// Converts a <see cref="Battlemancers.Data.ElementType"/> (Unity data layer) to
        /// <see cref="Battlemancers.Simulation.ElementType"/> (pure C# simulation layer) by name.
        /// Extended elements that exist only in the data layer fall back to
        /// <see cref="ElementType.Fire"/> (a no-op interaction for unlisted elements).
        /// </summary>
        private static ElementType MapElementType(Battlemancers.Data.ElementType dataElement)
        {
            string name = dataElement.ToString();
            if (Enum.TryParse<ElementType>(name, ignoreCase: true, out ElementType result))
                return result;

            return ElementType.Fire;
        }
    }
}
