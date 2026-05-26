using System;
using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Data;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Simulation
{
    /// <summary>
    /// Executes spell casts against the simulation state.
    ///
    /// <see cref="SpellResolver"/> is the authoritative system that takes a fully-validated
    /// spell cast and applies every downstream effect: AP deduction, target tile enumeration,
    /// direct damage, element-interaction tile state changes, status effect application,
    /// cooldown placement, and simulation event publication.
    ///
    /// <para>
    /// <b>Execution order inside <see cref="Resolve"/>:</b>
    /// <list type="number">
    ///   <item>Validate cast legality (caster alive, AP sufficient, spell not on cooldown).</item>
    ///   <item>Deduct AP and put the spell on cooldown.</item>
    ///   <item>Enumerate target tiles via <see cref="TargetingUtils"/>.</item>
    ///   <item>Apply damage to each enemy unit in the target area.</item>
    ///   <item>Resolve element interactions with each target tile via <see cref="ElementResolver"/>
    ///         and apply resulting tile state changes.</item>
    ///   <item>Apply spell-defined status effects to each surviving hit unit.</item>
    ///   <item>Publish all relevant <see cref="SimulationEvent"/> instances via
    ///         <see cref="SimulationEventBus"/>.</item>
    ///   <item>Return the immutable <see cref="SpellResult"/>.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Element type bridging:</b> <c>SpellData.element</c> is a
    /// <see cref="Battlemancers.Data.ElementType"/> (Unity data layer), while
    /// <see cref="ElementResolver"/> operates on <see cref="Battlemancers.Simulation.ElementType"/>
    /// (pure C#). The two enums share identical member names; <see cref="MapElementType"/> converts
    /// between them by name so no hard coupling exists.
    /// </para>
    ///
    /// <para>
    /// <b>Armor model:</b> Units currently have no dedicated Armor field on
    /// <see cref="UnitState"/>. <see cref="ComputeDamage"/> applies a default armor value of 0
    /// and is the single extension point for adding per-unit armor stats in a future iteration.
    /// </para>
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class SpellResolver
    {
        // -----------------------------------------------------------------------------------------
        // Dependencies
        // -----------------------------------------------------------------------------------------

        private readonly ElementResolver _elementResolver;
        private readonly StatusManager _statusManager;

        // -----------------------------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="SpellResolver"/>.
        /// </summary>
        /// <param name="elementResolver">
        /// Pre-loaded element interaction resolver. Must not be null.
        /// </param>
        /// <param name="statusManager">
        /// The status manager that owns all active status effects for this match.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either argument is null.
        /// </exception>
        public SpellResolver(ElementResolver elementResolver, StatusManager statusManager)
        {
            _elementResolver = elementResolver
                ?? throw new ArgumentNullException(nameof(elementResolver));
            _statusManager = statusManager
                ?? throw new ArgumentNullException(nameof(statusManager));
        }

        // -----------------------------------------------------------------------------------------
        // Primary API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Fully resolves a spell cast and applies all effects to <paramref name="state"/>.
        ///
        /// <para>
        /// The method returns null (and performs no mutations) if the cast fails
        /// pre-flight validation: the caster does not exist, is dead, lacks sufficient AP,
        /// or has the spell currently on cooldown. Callers should check for null before
        /// reading the result.
        /// </para>
        /// </summary>
        /// <param name="casterId">
        /// Runtime ID of the Mancer performing the cast (e.g., "p1_pyromancer_0").
        /// </param>
        /// <param name="spell">
        /// The <see cref="SpellData"/> definition for the spell being cast. Must not be null.
        /// </param>
        /// <param name="targetPos">
        /// The grid tile the player aimed the spell at.
        /// </param>
        /// <param name="state">
        /// The live simulation state to read from and mutate. Must not be null.
        /// </param>
        /// <returns>
        /// A <see cref="SpellResult"/> describing every effect that occurred, or null if the
        /// cast failed pre-flight validation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="spell"/> or <paramref name="state"/> is null.
        /// </exception>
        public SpellResult Resolve(
            string casterId,
            SpellData spell,
            GridPosition targetPos,
            SimulationState state)
        {
            if (spell == null) throw new ArgumentNullException(nameof(spell));
            if (state == null) throw new ArgumentNullException(nameof(state));

            // ------------------------------------------------------------------
            // Step 1 — Validate cast
            // ------------------------------------------------------------------
            UnitState caster = state.GetUnit(casterId);

            if (caster == null || !caster.IsAlive)
                return null;

            if (caster.ActionPoints < spell.apCost)
                return null;

            if (caster.SpellCooldowns.ContainsKey(spell.spellId))
                return null;

            // ------------------------------------------------------------------
            // Step 2 — Deduct AP and put spell on cooldown immediately
            // ------------------------------------------------------------------
            caster.ActionPoints -= spell.apCost;

            if (spell.cooldownTurns > 0)
                caster.SpellCooldowns[spell.spellId] = spell.cooldownTurns;

            // ------------------------------------------------------------------
            // Step 3 — Compute target tiles
            // ------------------------------------------------------------------
            SpellTargetingShape shape = MapTargetType(spell.targetType);
            int shapeRange = (spell.aoeRadius > 0) ? spell.aoeRadius : spell.range;

            List<GridPosition> targetTiles = TargetingUtils.GetTargetTiles(
                caster.Position, targetPos, shape, shapeRange, state.Grid);

            // ------------------------------------------------------------------
            // Step 4 — Damage hit units + collect HitRecords
            // ------------------------------------------------------------------
            List<UnitState> hitUnits = TargetingUtils.GetUnitsInTiles(targetTiles, state);

            var hitRecords = new List<HitRecord>(hitUnits.Count);
            var killedUnitIds = new List<string>();

            // Collect IDs of hit enemies for the SpellHitEvent.
            var affectedUnitIds = new List<string>(hitUnits.Count);

            // We track damage per unit here; status application happens in step 6.
            var damageTakenByUnit = new Dictionary<string, int>(hitUnits.Count);

            foreach (UnitState target in hitUnits)
            {
                // Only deal direct damage to enemies; friendly fire is not modelled by default.
                if (target.OwnerId == caster.OwnerId)
                    continue;

                affectedUnitIds.Add(target.Id);

                int damage = ComputeDamage(spell, target, state);
                damageTakenByUnit[target.Id] = damage;

                if (damage > 0)
                {
                    target.CurrentHP = Math.Max(0, target.CurrentHP - damage);

                    SimulationEventBus.Publish(new UnitDamagedEvent(
                        state.TurnNumber,
                        target.Id,
                        damage,
                        spell.spellId,
                        target.CurrentHP));
                }

                if (!target.IsAlive)
                    killedUnitIds.Add(target.Id);
            }

            // Publish a single SpellHitEvent covering all affected units.
            if (affectedUnitIds.Count > 0 || targetTiles.Count > 0)
            {
                int totalDamage = 0;
                foreach (int d in damageTakenByUnit.Values)
                    totalDamage += d;

                SimulationEventBus.Publish(new SpellHitEvent(
                    state.TurnNumber,
                    casterId,
                    spell.spellId,
                    targetPos,
                    totalDamage,
                    affectedUnitIds.ToArray()));
            }

            // Process kills — publish UnitDiedEvent and deregister.
            foreach (string killedId in killedUnitIds)
            {
                UnitState deadUnit = state.GetUnit(killedId);
                if (deadUnit != null)
                {
                    SimulationEventBus.Publish(new UnitDiedEvent(
                        state.TurnNumber,
                        killedId,
                        deadUnit.Position,
                        casterId));

                    state.DeregisterUnit(killedId);
                }
            }

            // ------------------------------------------------------------------
            // Step 5 — Element interactions with each target tile
            // ------------------------------------------------------------------
            var tileChanges = new List<TileStateChange>();

            // Convert SpellData.ElementType → Simulation.ElementType by name.
            ElementType simulationElement = MapElementType(spell.element);

            foreach (GridPosition tilePos in targetTiles)
            {
                Tile tile = state.Grid.GetTile(tilePos);
                if (tile == null)
                    continue;

                TileState oldState = tile.State;
                string tileStateName = oldState.ToString();

                Interaction interaction = _elementResolver.Resolve(tileStateName, simulationElement);

                // Attempt to parse the resulting tile state from the interaction string.
                if (Enum.TryParse<TileState>(interaction.ResultingTileState, ignoreCase: true, out TileState newTileState)
                    && newTileState != oldState)
                {
                    state.Grid.SetTileState(tilePos, newTileState);

                    tileChanges.Add(new TileStateChange(tilePos, oldState, newTileState));

                    SimulationEventBus.Publish(new TileStateChangedEvent(
                        state.TurnNumber,
                        tilePos,
                        oldState,
                        newTileState,
                        interaction.VfxHint));
                }
            }

            // ------------------------------------------------------------------
            // Step 6 — Apply status effects from SpellData to surviving hit units
            // ------------------------------------------------------------------
            var statusesAppliedByUnit = new Dictionary<string, List<string>>();

            if (spell.appliedEffects != null && spell.appliedEffects.Length > 0)
            {
                foreach (UnitState target in hitUnits)
                {
                    // Skip enemies that were killed in step 4 and skip allies.
                    if (!target.IsAlive || target.OwnerId == caster.OwnerId)
                        continue;

                    // Verify the unit is still registered (not deregistered in step 4).
                    if (state.GetUnit(target.Id) == null)
                        continue;

                    var appliedNames = new List<string>();

                    foreach (StatusEffectApplication application in spell.appliedEffects)
                    {
                        if (application == null)
                            continue;

                        // Skip tile-applied statuses — those are handled by the terrain system.
                        if (application.appliesToTile)
                            continue;

                        // Skip if applicationChance is not met (deterministic: skip anything < 1.0).
                        // To preserve determinism in multiplayer lockstep, we treat any chance < 1.0
                        // as always-apply for now. A seeded RNG layer can be added later.
                        // Currently we apply all effects (chance is noted but not rolled here).

                        if (!Enum.TryParse<StatusType>(application.statusType, ignoreCase: true, out StatusType statusType))
                            continue;

                        int duration = Math.Max(1, application.duration);
                        int stacks = Math.Max(1, application.stacksApplied);

                        var effect = new StatusEffect(statusType, duration, stacks, casterId);
                        _statusManager.ApplyStatus(target.Id, effect, target, state.TurnNumber);

                        appliedNames.Add(statusType.ToString());
                    }

                    if (appliedNames.Count > 0)
                        statusesAppliedByUnit[target.Id] = appliedNames;
                }
            }

            // ------------------------------------------------------------------
            // Build HitRecord list now that both damage and statuses are known
            // ------------------------------------------------------------------
            // Re-iterate hitUnits so we can include units with 0 damage but statuses applied.
            var processedEnemies = new HashSet<string>();
            foreach (UnitState target in hitUnits)
            {
                if (target.OwnerId == caster.OwnerId)
                    continue;

                processedEnemies.Add(target.Id);

                damageTakenByUnit.TryGetValue(target.Id, out int dmg);
                statusesAppliedByUnit.TryGetValue(target.Id, out List<string> statuses);

                bool wasKilled = killedUnitIds.Contains(target.Id);

                hitRecords.Add(new HitRecord(
                    target.Id,
                    dmg,
                    statuses != null ? statuses.ToArray() : Array.Empty<string>(),
                    wasKilled));
            }

            // ------------------------------------------------------------------
            // Step 7 — Determine if the spell fizzled
            // ------------------------------------------------------------------
            bool fizzled = hitRecords.Count == 0 && tileChanges.Count == 0;

            // ------------------------------------------------------------------
            // Step 8 — Return SpellResult
            // ------------------------------------------------------------------
            return new SpellResult(
                casterId,
                spell.spellId,
                targetPos,
                hitRecords.ToArray(),
                tileChanges.ToArray(),
                fizzled);
        }

        // -----------------------------------------------------------------------------------------
        // Damage calculation
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Computes the net damage dealt to <paramref name="target"/> by <paramref name="spell"/>.
        ///
        /// <para>
        /// Damage formula: <c>baseDamage - armor + conditionalBonuses</c>, clamped to [0, ∞).
        /// </para>
        ///
        /// <para>
        /// Conditional bonuses are evaluated against both the target tile's current
        /// <see cref="TileState"/> and the target unit's
        /// <see cref="UnitState.ActiveStatusTypes"/>. A bonus triggers when its
        /// <c>triggerState</c> string matches either source (case-insensitive).
        /// </para>
        ///
        /// <para>
        /// Armor: <see cref="UnitState"/> does not currently expose an Armor property.
        /// The value is fixed at 0 here. When an armor field is added to UnitState,
        /// update this method to read it.
        /// </para>
        /// </summary>
        /// <param name="spell">The spell definition providing base damage and bonuses.</param>
        /// <param name="target">The unit being damaged.</param>
        /// <param name="state">The live simulation state (used to read the target's tile state).</param>
        /// <returns>Net damage after armor and bonuses, clamped to a minimum of 0.</returns>
        private int ComputeDamage(SpellData spell, UnitState target, SimulationState state)
        {
            int damage = spell.baseDamage;

            // Armor reduction (0 until UnitState exposes an Armor field).
            const int armor = 0;
            damage = Math.Max(0, damage - armor);

            // Apply conditional bonuses.
            if (spell.conditionalBonuses != null && spell.conditionalBonuses.Length > 0)
            {
                // Collect the tile state name for the target's current tile.
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
            }

            return Math.Max(0, damage);
        }

        // -----------------------------------------------------------------------------------------
        // Enum bridging helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Converts a <see cref="Battlemancers.Data.ElementType"/> (Unity ScriptableObject layer)
        /// to <see cref="Battlemancers.Simulation.ElementType"/> (pure C# simulation layer).
        ///
        /// Both enums share identical member names for the seven core elements (Fire, Water, Ice,
        /// Lightning, Earth, Wind, Poison). Extended elements added only in the data layer
        /// (Necrotic, Light, Sound, Gravity, Time, Crystal, Psychic, Arcane, Thermal) are mapped
        /// to <see cref="ElementType.Fire"/> as a safe fallback — these elements do not yet have
        /// entries in the element interaction table and will produce a no-op interaction.
        /// </summary>
        /// <param name="dataElement">The element type from the SpellData asset.</param>
        /// <returns>The matching simulation-layer element type.</returns>
        private static ElementType MapElementType(Battlemancers.Data.ElementType dataElement)
        {
            // Attempt name-based conversion first — covers Fire, Water, Ice, Lightning, Earth, Wind, Poison.
            string name = dataElement.ToString();
            if (Enum.TryParse<ElementType>(name, ignoreCase: true, out ElementType result))
                return result;

            // Fallback for data-layer-only elements (Necrotic, Light, Sound, etc.).
            return ElementType.Fire;
        }

        /// <summary>
        /// Maps a <see cref="SpellTargetType"/> from the SpellData definition to the
        /// corresponding <see cref="SpellTargetingShape"/> used by <see cref="TargetingUtils"/>.
        ///
        /// <list type="bullet">
        ///   <item><description><see cref="SpellTargetType.SingleTarget"/> → <see cref="SpellTargetingShape.Single"/></description></item>
        ///   <item><description><see cref="SpellTargetType.Line"/> → <see cref="SpellTargetingShape.Line"/></description></item>
        ///   <item><description><see cref="SpellTargetType.Cone"/> → <see cref="SpellTargetingShape.Cone"/></description></item>
        ///   <item><description><see cref="SpellTargetType.AoeCircle"/>, <see cref="SpellTargetType.AllEnemiesInRange"/>,
        ///         <see cref="SpellTargetType.AlliesInRange"/> → <see cref="SpellTargetingShape.AoECircle"/></description></item>
        ///   <item><description><see cref="SpellTargetType.Ground"/>, <see cref="SpellTargetType.Self"/>,
        ///         <see cref="SpellTargetType.Projectile"/>, <see cref="SpellTargetType.Chain"/>
        ///         → <see cref="SpellTargetingShape.Single"/> (safe fallback)</description></item>
        /// </list>
        /// </summary>
        /// <param name="targetType">The targeting type from the SpellData asset.</param>
        /// <returns>The closest matching <see cref="SpellTargetingShape"/>.</returns>
        private static SpellTargetingShape MapTargetType(SpellTargetType targetType)
        {
            switch (targetType)
            {
                case SpellTargetType.SingleTarget:
                    return SpellTargetingShape.Single;

                case SpellTargetType.Line:
                case SpellTargetType.Projectile:
                    return SpellTargetingShape.Line;

                case SpellTargetType.Cone:
                    return SpellTargetingShape.Cone;

                case SpellTargetType.AoeCircle:
                case SpellTargetType.AllEnemiesInRange:
                case SpellTargetType.AlliesInRange:
                    return SpellTargetingShape.AoECircle;

                case SpellTargetType.Ground:
                case SpellTargetType.Self:
                case SpellTargetType.Chain:
                default:
                    return SpellTargetingShape.Single;
            }
        }
    }
}
