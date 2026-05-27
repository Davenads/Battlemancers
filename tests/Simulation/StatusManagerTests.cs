using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// NUnit tests for <see cref="StatusManager"/>.
    ///
    /// Covers stacking rules, per-tick damage, duration expiry, dual-status interactions,
    /// and cleanse behaviour. All tests are pure C# — no Unity dependencies.
    ///
    /// Per-tick damage constants (from StatusManager):
    ///   BURNING  — 5 HP per tick (flat)
    ///   POISONED — 3 HP × stack count per tick (max 15 HP/turn at 5 stacks)
    ///   All others — 0 HP per tick
    /// </summary>
    [TestFixture]
    public class StatusManagerTests
    {
        // ---------------------------------------------------------------------------
        // Named constants — no magic numbers
        // ---------------------------------------------------------------------------

        private const int BurningDamagePerTick  = 5;
        private const int PoisonDamagePerStack   = 3;
        private const int PoisonMaxStacks        = 5;

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private GridData        _grid;
        private SimulationState _state;
        private StatusManager   _statusManager;

        // ---------------------------------------------------------------------------
        // Setup / Teardown
        // ---------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _grid          = GridData.Standard24x24();
            _state         = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase   = TurnPhase.Resolving;
            _statusManager = new StatusManager();
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // =========================================================================
        // Stacking rules
        // =========================================================================

        /// <summary>
        /// Applying BURNING a second time before the first expires adds the new duration
        /// to the remaining duration (duration-stacking rule).
        /// </summary>
        [Test]
        public void ApplyStatus_Burning_StacksDuration()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 2, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            IReadOnlyList<StatusEffect> statuses = _statusManager.GetStatuses("p1_pyro_0");
            StatusEffect burning = statuses.FirstOrDefault(e => e.Type == StatusType.Burning);

            Assert.That(burning, Is.Not.Null,
                "BURNING must remain active after two applications.");
            Assert.That(burning.Duration, Is.EqualTo(5),
                "Applying BURNING twice (duration 3 + 2) must yield a combined duration of 5.");
            Assert.That(burning.StackCount, Is.EqualTo(1),
                "BURNING stack count must remain 1 regardless of how many times it is applied.");
        }

        /// <summary>
        /// Each POISONED application adds one stack (up to max 5). At 2 stacks the
        /// per-tick damage doubles to 6 HP/turn.
        /// </summary>
        [Test]
        public void ApplyStatus_Poisoned_StacksCount()
        {
            UnitState unit = MakeUnit("p1_toxi_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_toxi_0",
                new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            _statusManager.ApplyStatus("p1_toxi_0",
                new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            IReadOnlyList<StatusEffect> statuses = _statusManager.GetStatuses("p1_toxi_0");
            StatusEffect poisoned = statuses.FirstOrDefault(e => e.Type == StatusType.Poisoned);

            Assert.That(poisoned, Is.Not.Null,
                "POISONED must be active after two applications.");
            Assert.That(poisoned.StackCount, Is.EqualTo(2),
                "Each POISONED application must add one stack; two applications must yield 2 stacks.");

            // Verify the expected per-tick damage via TickStatuses.
            int hpBefore = unit.CurrentHP;
            _statusManager.TickStatuses(_state);
            int damageDealt = hpBefore - unit.CurrentHP;
            const int expectedDamage = PoisonDamagePerStack * 2;

            Assert.That(damageDealt, Is.EqualTo(expectedDamage),
                $"At 2 POISONED stacks, tick damage must be {expectedDamage} HP (3 HP × 2 stacks).");
        }

        /// <summary>
        /// Applying FROZEN with a shorter (or equal) duration when one is already active
        /// must leave the existing duration unchanged. Only a strictly longer duration replaces it.
        /// </summary>
        [Test]
        public void ApplyStatus_Frozen_ReplacesIfLonger()
        {
            UnitState unit = MakeUnit("p1_cryo_0", Player1);
            _state.RegisterUnit(unit);

            // Apply with duration 3.
            _statusManager.ApplyStatus("p1_cryo_0",
                new StatusEffect(StatusType.Frozen, duration: 3, stackCount: 1, sourceId: "cryo"),
                unit, _state.TurnNumber);

            // Attempt to apply shorter duration — must be ignored.
            _statusManager.ApplyStatus("p1_cryo_0",
                new StatusEffect(StatusType.Frozen, duration: 1, stackCount: 1, sourceId: "cryo"),
                unit, _state.TurnNumber);

            StatusEffect frozen = _statusManager.GetStatuses("p1_cryo_0")
                .FirstOrDefault(e => e.Type == StatusType.Frozen);

            Assert.That(frozen, Is.Not.Null);
            Assert.That(frozen.Duration, Is.EqualTo(3),
                "A shorter FROZEN application (duration 1) must not replace an existing longer one (duration 3).");

            // Now apply a strictly longer duration — must replace.
            _statusManager.ApplyStatus("p1_cryo_0",
                new StatusEffect(StatusType.Frozen, duration: 5, stackCount: 1, sourceId: "cryo"),
                unit, _state.TurnNumber);

            frozen = _statusManager.GetStatuses("p1_cryo_0")
                .FirstOrDefault(e => e.Type == StatusType.Frozen);

            Assert.That(frozen.Duration, Is.EqualTo(5),
                "A longer FROZEN application (duration 5) must replace the existing shorter one (duration 3).");
        }

        /// <summary>
        /// STUNNED is a cannot-stack type where a second application resets the duration
        /// rather than adding to it. Stack count must always remain 1.
        /// </summary>
        [Test]
        public void ApplyStatus_Stunned_CannotStack()
        {
            UnitState unit = MakeUnit("p1_electro_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_electro_0",
                new StatusEffect(StatusType.Stunned, duration: 2, stackCount: 1, sourceId: "electro"),
                unit, _state.TurnNumber);

            // Tick once so the duration decrements to 1.
            _statusManager.TickStatuses(_state);

            // Second application — must reset to 2, not add to it.
            _statusManager.ApplyStatus("p1_electro_0",
                new StatusEffect(StatusType.Stunned, duration: 2, stackCount: 1, sourceId: "electro"),
                unit, _state.TurnNumber);

            StatusEffect stunned = _statusManager.GetStatuses("p1_electro_0")
                .FirstOrDefault(e => e.Type == StatusType.Stunned);

            Assert.That(stunned, Is.Not.Null,
                "STUNNED must still be active after re-application.");
            Assert.That(stunned.Duration, Is.EqualTo(2),
                "STUNNED re-application must reset duration to 2, not stack it to 3.");
            Assert.That(stunned.StackCount, Is.EqualTo(1),
                "STUNNED stack count must always remain 1.");
        }

        /// <summary>
        /// CHARMED cannot stack — while active a second application is silently ignored.
        /// </summary>
        [Test]
        public void ApplyStatus_Charmed_CannotStack()
        {
            UnitState unit = MakeUnit("p1_psycho_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_psycho_0",
                new StatusEffect(StatusType.Charmed, duration: 2, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);

            // Tick once so duration drops to 1.
            _statusManager.TickStatuses(_state);

            // Second application while still active — must be silently ignored.
            _statusManager.ApplyStatus("p1_psycho_0",
                new StatusEffect(StatusType.Charmed, duration: 5, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);

            StatusEffect charmed = _statusManager.GetStatuses("p1_psycho_0")
                .FirstOrDefault(e => e.Type == StatusType.Charmed);

            Assert.That(charmed, Is.Not.Null,
                "CHARMED must still be active.");
            Assert.That(charmed.Duration, Is.EqualTo(1),
                "CHARMED re-application while active must be ignored; duration must remain 1 (already decremented once).");
        }

        /// <summary>
        /// PANICKED cannot stack — while active a second application is silently ignored.
        /// </summary>
        [Test]
        public void ApplyStatus_Panicked_CannotStack()
        {
            UnitState unit = MakeUnit("p1_psycho_1", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_psycho_1",
                new StatusEffect(StatusType.Panicked, duration: 3, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);

            // Tick once so duration drops to 2.
            _statusManager.TickStatuses(_state);

            // Second application while still active — must be ignored.
            _statusManager.ApplyStatus("p1_psycho_1",
                new StatusEffect(StatusType.Panicked, duration: 10, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);

            StatusEffect panicked = _statusManager.GetStatuses("p1_psycho_1")
                .FirstOrDefault(e => e.Type == StatusType.Panicked);

            Assert.That(panicked, Is.Not.Null);
            Assert.That(panicked.Duration, Is.EqualTo(2),
                "PANICKED re-application while active must be ignored; duration must remain 2.");
        }

        // =========================================================================
        // Duration / tick behaviour
        // =========================================================================

        /// <summary>
        /// A unit with BURNING takes exactly 5 HP damage each time TickStatuses is called.
        /// </summary>
        [Test]
        public void TickStatuses_Burning_DealsTickDamage()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            int hpBefore = unit.CurrentHP;
            _statusManager.TickStatuses(_state);

            Assert.That(unit.CurrentHP, Is.EqualTo(hpBefore - BurningDamagePerTick),
                $"BURNING must deal exactly {BurningDamagePerTick} HP damage per tick.");
        }

        /// <summary>
        /// When a unit has both POISONED and FROZEN, the POISONED timer must not advance
        /// while FROZEN is active.
        ///
        /// Design rule from status-effects.md:
        ///   "POISONED | FROZEN — Stacks preserved (no decay timer while frozen)"
        ///
        /// NOTE: The current StatusManager implementation ticks ALL active statuses
        /// unconditionally. This test reflects the specified design intent. If this test
        /// fails, the TickStatuses method needs to skip POISONED duration decrement for
        /// units that also carry FROZEN.
        /// </summary>
        [Test]
        public void TickStatuses_Frozen_PausesPoison()
        {
            UnitState unit = MakeUnit("p1_cryo_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            // Apply POISONED (duration 4) then FROZEN (duration 2).
            _statusManager.ApplyStatus("p1_cryo_0",
                new StatusEffect(StatusType.Poisoned, duration: 4, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_cryo_0",
                new StatusEffect(StatusType.Frozen, duration: 2, stackCount: 1, sourceId: "cryo"),
                unit, _state.TurnNumber);

            // Tick once — FROZEN is present, so POISONED duration must not decrement.
            _statusManager.TickStatuses(_state);

            StatusEffect poisoned = _statusManager.GetStatuses("p1_cryo_0")
                .FirstOrDefault(e => e.Type == StatusType.Poisoned);

            Assert.That(poisoned, Is.Not.Null,
                "POISONED must still be active after one tick while FROZEN is also active.");
            Assert.That(poisoned.Duration, Is.EqualTo(4),
                "POISONED duration must not decrement while FROZEN is active (design: no decay timer while frozen).");
        }

        /// <summary>
        /// A status with duration = 1 must be removed after one tick call.
        /// After removal, HasStatus returns false and GetStatuses returns an empty list.
        /// </summary>
        [Test]
        public void TickStatuses_StatusExpires_RemovedAfterDuration()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 1, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Burning), Is.True,
                "BURNING must be active before the tick.");

            StatusTickResult[] results = _statusManager.TickStatuses(_state);

            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Burning), Is.False,
                "BURNING with duration=1 must be removed after one tick.");
            Assert.That(unit.ActiveStatusTypes, Does.Not.Contain("Burning"),
                "ActiveStatusTypes must not contain 'Burning' after the status expires.");

            // Verify the tick result reports expiry.
            StatusTickResult tickResult = results.FirstOrDefault(r =>
                r.UnitId == "p1_pyro_0" && r.StatusType == StatusType.Burning);
            Assert.That(tickResult, Is.Not.Null);
            Assert.That(tickResult.StatusExpired, Is.True,
                "The tick result for BURNING must report StatusExpired = true.");
        }

        /// <summary>
        /// A unit with both BURNING and POISONED takes damage from both sources on the same tick.
        /// Total damage = BurningDamagePerTick + PoisonDamagePerStack × stackCount.
        /// (FROZEN is not present, so POISONED is NOT paused.)
        /// </summary>
        [Test]
        public void TickStatuses_BurningAndPoison_BothTick()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 200);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            int hpBefore = unit.CurrentHP;
            _statusManager.TickStatuses(_state);

            const int expectedDamage = BurningDamagePerTick + PoisonDamagePerStack * 1;
            int actualDamage = hpBefore - unit.CurrentHP;

            Assert.That(actualDamage, Is.EqualTo(expectedDamage),
                $"A unit with BURNING and 1-stack POISONED must take {expectedDamage} HP damage per tick "
                + $"({BurningDamagePerTick} from BURNING + {PoisonDamagePerStack} from POISONED).");
        }

        // =========================================================================
        // Dual-status interactions
        // =========================================================================

        /// <summary>
        /// A unit with both BLINDED and CONFUSED has targeting range collapsed to 1 tile,
        /// guaranteeing friendly-fire at close range.
        ///
        /// Design rule from status-effects.md:
        ///   "BLINDED | CONFUSED — Both apply; stacking worst-case: CONFUSED nearest-unit
        ///    targeting restricted to BLINDED's reduced range (1 tile)"
        ///
        /// This test verifies that both statuses are simultaneously active and that
        /// ActiveStatusTypes reflects both, which is the prerequisite for targeting resolvers
        /// to apply the combined range clamp.
        /// </summary>
        [Test]
        public void ApplyStatus_BlindedAndConfused_BothActiveSimultaneously()
        {
            UnitState unit = MakeUnit("p1_photo_0", Player1);
            _state.RegisterUnit(unit);

            // BLINDED — duration-stacking type (uses default fallback in StatusManager).
            _statusManager.ApplyStatus("p1_photo_0",
                new StatusEffect(StatusType.Slowed, duration: 2, stackCount: 1, sourceId: "photo"),
                unit, _state.TurnNumber);

            // Verify both statuses are simultaneously active on the unit.
            // BLINDED and CONFUSED map to StatusType.Slowed and StatusType.Panicked here
            // because the StatusType enum does not yet define Blinded/Confused explicitly.
            // We use two distinct non-overlapping types to validate the dual-application logic.
            _statusManager.ApplyStatus("p1_photo_0",
                new StatusEffect(StatusType.Stunned, duration: 2, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);

            // Both must be active: unit.ActiveStatusTypes contains both keys.
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Slowed"),
                "ActiveStatusTypes must contain 'Slowed' (simulates BLINDED) after application.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Stunned"),
                "ActiveStatusTypes must contain 'Stunned' (simulates CONFUSED) after application.");
            Assert.That(_statusManager.GetStatuses("p1_photo_0").Count, Is.EqualTo(2),
                "Unit must carry exactly 2 simultaneous statuses when BLINDED and CONFUSED are both applied.");
        }

        /// <summary>
        /// A unit with CHARMED + SILENCED (Silenced = cannot cast; Charmed = compulsion toward allies)
        /// results in the unit being unable to cast (SILENCED suppresses spells) and only able to
        /// move toward the nearest ally.
        ///
        /// Design rule from status-effects.md:
        ///   "CHARMED | SILENCED — CHARMED compulsion toward allies is retained, but SILENCED
        ///    suppresses spellcasting. Result: unit moves toward nearest ally instead of casting."
        ///
        /// This test verifies that both statuses are simultaneously active on the unit.
        /// The interaction result (no spell fires; move toward ally) is enforced by the
        /// command/action resolution layer, not by StatusManager itself.
        /// </summary>
        [Test]
        public void ApplyStatus_CharmedAndSilenced_BothActiveSimultaneously()
        {
            UnitState unit = MakeUnit("p1_psycho_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_psycho_0",
                new StatusEffect(StatusType.Charmed, duration: 1, stackCount: 1, sourceId: "psycho"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_psycho_0",
                new StatusEffect(StatusType.Silenced, duration: 1, stackCount: 1, sourceId: "soni"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_psycho_0", StatusType.Charmed), Is.True,
                "CHARMED must be active after application.");
            Assert.That(_statusManager.HasStatus("p1_psycho_0", StatusType.Silenced), Is.True,
                "SILENCED must be active after application.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Charmed"),
                "ActiveStatusTypes must contain 'Charmed'.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Silenced"),
                "ActiveStatusTypes must contain 'Silenced'.");
        }

        /// <summary>
        /// A unit with both ROOTED and SILENCED cannot move (ROOTED) or cast (SILENCED),
        /// effectively causing a full turn skip.
        ///
        /// This test verifies that both statuses are simultaneously active, which is the
        /// prerequisite for the action resolution layer to enforce the full-skip rule.
        /// </summary>
        [Test]
        public void ApplyStatus_RootedAndSilenced_BothActiveSimultaneously()
        {
            UnitState unit = MakeUnit("p1_flora_0", Player1);
            _state.RegisterUnit(unit);

            // ROOTED maps to StatusType.Stunned (nearest available in current enum;
            // Stunned prevents full action, analogous to ROOTED + SILENCED combo).
            // We use Slowed (ROOTED proxy) and Silenced for this dual-status check.
            _statusManager.ApplyStatus("p1_flora_0",
                new StatusEffect(StatusType.Slowed, duration: 2, stackCount: 1, sourceId: "flora"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_flora_0",
                new StatusEffect(StatusType.Silenced, duration: 1, stackCount: 1, sourceId: "soni"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_flora_0", StatusType.Slowed), Is.True,
                "ROOTED proxy (Slowed) must be active.");
            Assert.That(_statusManager.HasStatus("p1_flora_0", StatusType.Silenced), Is.True,
                "SILENCED must be active.");
            Assert.That(_statusManager.GetStatuses("p1_flora_0").Count, Is.EqualTo(2),
                "Both ROOTED (proxy) and SILENCED must be simultaneously active, producing 2 status entries.");
        }

        // =========================================================================
        // Cleanse behaviour
        // =========================================================================

        /// <summary>
        /// After cleansing BURNING, the next TickStatuses call deals no fire damage
        /// because the status has been removed.
        /// </summary>
        [Test]
        public void RemoveStatus_CleanseBurning_StopsTickDamage()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 5, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            // Confirm BURNING is active.
            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Burning), Is.True,
                "Pre-condition: BURNING must be active before cleanse.");

            // Cleanse.
            _statusManager.RemoveStatus("p1_pyro_0", StatusType.Burning, unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Burning), Is.False,
                "BURNING must be removed after cleanse.");

            int hpBeforeTick = unit.CurrentHP;
            _statusManager.TickStatuses(_state);

            Assert.That(unit.CurrentHP, Is.EqualTo(hpBeforeTick),
                "After cleansing BURNING, TickStatuses must deal no fire damage.");
        }

        /// <summary>
        /// RemoveStatus on a unit that does not have the targeted status is a no-op;
        /// no exception is thrown and the unit's other statuses are not affected.
        /// </summary>
        [Test]
        public void RemoveStatus_NoMatchingStatus_IsNoOp()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            // Apply POISONED but not BURNING.
            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            // Attempt to remove BURNING (not present) — must not throw.
            Assert.DoesNotThrow(
                () => _statusManager.RemoveStatus("p1_pyro_0", StatusType.Burning, unit, _state.TurnNumber),
                "RemoveStatus on a non-existent status type must not throw.");

            // POISONED must still be active.
            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Poisoned), Is.True,
                "Other active statuses must be unaffected by a no-op RemoveStatus call.");
        }

        /// <summary>
        /// Cleansing all POISONED stacks at once removes the status entirely — HasStatus
        /// returns false and the next tick produces no poison damage.
        /// </summary>
        [Test]
        public void RemoveStatus_CleansePoison_RemovesAllStacks()
        {
            UnitState unit = MakeUnit("p1_toxi_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            // Build up 3 stacks.
            for (int i = 0; i < 3; i++)
            {
                _statusManager.ApplyStatus("p1_toxi_0",
                    new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                    unit, _state.TurnNumber);
            }

            StatusEffect poisonedBefore = _statusManager.GetStatuses("p1_toxi_0")
                .FirstOrDefault(e => e.Type == StatusType.Poisoned);
            Assert.That(poisonedBefore.StackCount, Is.EqualTo(3),
                "Pre-condition: POISONED must be at 3 stacks before cleanse.");

            _statusManager.RemoveStatus("p1_toxi_0", StatusType.Poisoned, unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_toxi_0", StatusType.Poisoned), Is.False,
                "POISONED must be fully removed (all stacks) after cleanse.");

            int hpBeforeTick = unit.CurrentHP;
            _statusManager.TickStatuses(_state);
            Assert.That(unit.CurrentHP, Is.EqualTo(hpBeforeTick),
                "After cleansing POISONED, TickStatuses must deal no poison damage.");
        }

        // =========================================================================
        // Additional stacking boundary cases
        // =========================================================================

        /// <summary>
        /// POISONED stacks cap at 5. A sixth application must not increase the stack count
        /// beyond 5; instead it refreshes duration if the new duration is longer.
        /// </summary>
        [Test]
        public void ApplyStatus_Poisoned_CapsAtMaxStacks()
        {
            UnitState unit = MakeUnit("p1_toxi_0", Player1, hp: 200);
            _state.RegisterUnit(unit);

            for (int i = 0; i < PoisonMaxStacks + 1; i++)
            {
                _statusManager.ApplyStatus("p1_toxi_0",
                    new StatusEffect(StatusType.Poisoned, duration: 3, stackCount: 1, sourceId: "toxi"),
                    unit, _state.TurnNumber);
            }

            StatusEffect poisoned = _statusManager.GetStatuses("p1_toxi_0")
                .FirstOrDefault(e => e.Type == StatusType.Poisoned);

            Assert.That(poisoned.StackCount, Is.EqualTo(PoisonMaxStacks),
                $"POISONED stack count must cap at {PoisonMaxStacks} regardless of how many times it is applied.");
        }

        /// <summary>
        /// Applying a status to a unit with 0 HP (dead) still mutates the status list,
        /// but TickStatuses skips dead units so no damage or expiry occurs.
        /// </summary>
        [Test]
        public void TickStatuses_DeadUnit_IsSkipped()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            // Kill the unit.
            unit.CurrentHP = 0;
            Assert.That(unit.IsAlive, Is.False, "Pre-condition: unit must be dead.");

            StatusTickResult[] results = _statusManager.TickStatuses(_state);

            Assert.That(results, Is.Empty,
                "TickStatuses must return no results for dead units.");
            // BURNING still registered but was never ticked.
            Assert.That(_statusManager.HasStatus("p1_pyro_0", StatusType.Burning), Is.True,
                "BURNING must remain in the status list (not yet ticked) for a dead unit.");
        }

        /// <summary>
        /// ActiveStatusTypes on UnitState is kept in sync: adding a status adds its key,
        /// and expiry removes it.
        /// </summary>
        [Test]
        public void ActiveStatusTypes_KeptInSync_WithStatusManagerState()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_0",
                new StatusEffect(StatusType.Burning, duration: 1, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            Assert.That(unit.ActiveStatusTypes, Contains.Item("Burning"),
                "ActiveStatusTypes must contain 'Burning' immediately after application.");

            _statusManager.TickStatuses(_state); // duration was 1, so it expires now.

            Assert.That(unit.ActiveStatusTypes, Does.Not.Contain("Burning"),
                "ActiveStatusTypes must no longer contain 'Burning' after the status expires.");
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private static UnitState MakeUnit(string id, string ownerId, int hp = 100)
        {
            return new UnitState(
                id:                id,
                mancerArchetypeId: "pyromancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          new GridPosition(0, 0),
                maxHP:             hp,
                moveRange:         4,
                pointCost:         100
            );
        }
    }
}
