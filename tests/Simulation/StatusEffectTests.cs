using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Core.Simulation.StatusEffects;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// NUnit tests for the six concrete <see cref="IStatusEffect"/> implementations:
    /// <see cref="BurningStatus"/>, <see cref="WetStatus"/>, <see cref="PoisonedStatus"/>,
    /// <see cref="ChargedStatus"/>, <see cref="SilencedStatus"/>, and <see cref="CursedStatus"/>.
    ///
    /// Tests exercise both direct instantiation of concrete classes and their integration
    /// through <see cref="StatusManager"/>, verifying that:
    /// <list type="bullet">
    ///   <item><description>Per-tick damage is applied correctly.</description></item>
    ///   <item><description>Cross-status interactions fire (Wet extinguishes Burning).</description></item>
    ///   <item><description>Stack counting, duration expiry, and duplicate-apply stacking all behave
    ///     per the design specification.</description></item>
    /// </list>
    ///
    /// All tests are pure C# — no Unity dependencies.
    /// </summary>
    [TestFixture]
    public class StatusEffectTests
    {
        // ---------------------------------------------------------------------------
        // Named constants — no magic numbers
        // ---------------------------------------------------------------------------

        private const int BurningDamagePerTick  = BurningStatus.DamagePerTick;   // 5
        private const int PoisonDamagePerStack   = PoisonedStatus.DamagePerStack; // 3
        private const float CurseHealingMult     = CursedStatus.HealingMultiplier; // 0.5f

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
        // Test 1 — Poison stacks
        // =========================================================================

        /// <summary>
        /// Each application of POISONED via StatusManager adds one stack (up to max 5).
        /// Two applications must yield StackCount = 2; tick damage must reflect both stacks.
        /// </summary>
        [Test]
        public void Poisoned_Apply_StacksCount()
        {
            UnitState unit = MakeUnit("p1_toxi_0", Player1, hp: 200);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_toxi_0",
                new StatusEffect(StatusType.Poisoned, duration: 4, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_toxi_0",
                new StatusEffect(StatusType.Poisoned, duration: 4, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            // Stack count on the stored StatusEffect must be 2.
            StatusEffect poisoned = _statusManager
                .GetStatuses("p1_toxi_0")
                .FirstOrDefault(e => e.Type == StatusType.Poisoned);

            Assert.That(poisoned, Is.Not.Null,
                "POISONED must be active after two applications.");
            Assert.That(poisoned.StackCount, Is.EqualTo(2),
                "Each POISONED application must add one stack; two applications must yield StackCount = 2.");

            // Per-tick damage must be 3 HP × 2 stacks = 6 HP.
            int hpBefore = unit.CurrentHP;
            _statusManager.TickStatuses(_state);
            int damageDealt = hpBefore - unit.CurrentHP;
            const int expectedDamage = PoisonDamagePerStack * 2;

            Assert.That(damageDealt, Is.EqualTo(expectedDamage),
                $"At 2 POISONED stacks, tick damage must be {expectedDamage} HP ({PoisonDamagePerStack} HP × 2 stacks).");
        }

        // =========================================================================
        // Test 2 — Burning ticks damage
        // =========================================================================

        /// <summary>
        /// <see cref="BurningStatus.Tick"/> directly reduces the unit's HP by exactly
        /// <see cref="BurningStatus.DamagePerTick"/> (5 HP). Verified by calling Tick
        /// on a concrete instance without going through StatusManager.
        /// </summary>
        [Test]
        public void BurningStatus_Tick_DealsDamage()
        {
            UnitState unit = MakeUnit("p1_pyro_0", Player1, hp: 100);
            var burning = new BurningStatus(duration: 3, sourceId: "pyro");

            // Call Tick with null state (no fire spreading needed for this test).
            burning.Tick(unit, state: null);

            Assert.That(unit.CurrentHP, Is.EqualTo(100 - BurningDamagePerTick),
                $"BurningStatus.Tick must deal exactly {BurningDamagePerTick} HP damage per tick.");
        }

        // =========================================================================
        // Test 3 — Wet extinguishes Burning
        // =========================================================================

        /// <summary>
        /// Applying WET to a unit that already carries BURNING must remove the BURNING status
        /// as a side-effect, because water extinguishes fire.
        /// This cross-status interaction is enforced by <see cref="StatusManager"/> when
        /// Wet is applied.
        /// </summary>
        [Test]
        public void WetStatus_Apply_ExtinguishesBurning()
        {
            UnitState unit = MakeUnit("p1_hydro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            // Apply BURNING first.
            _statusManager.ApplyStatus("p1_hydro_0",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_hydro_0", StatusType.Burning), Is.True,
                "Pre-condition: BURNING must be active before WET is applied.");

            // Apply WET — must extinguish BURNING.
            _statusManager.ApplyStatus("p1_hydro_0",
                new StatusEffect(StatusType.Wet, duration: 3, stackCount: 1, sourceId: "hydro"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_hydro_0", StatusType.Burning), Is.False,
                "Applying WET must extinguish (remove) any active BURNING status.");
            Assert.That(_statusManager.HasStatus("p1_hydro_0", StatusType.Wet), Is.True,
                "WET must remain active after the side-effect removes BURNING.");
        }

        // =========================================================================
        // Test 4 — Charged amplifies lightning
        // =========================================================================

        /// <summary>
        /// After applying CHARGED via StatusManager, the unit's <see cref="UnitState.ActiveStatusTypes"/>
        /// must contain <c>"Charged"</c> and <see cref="StatusManager.HasStatus"/> must return
        /// <c>true</c> for <see cref="StatusType.Charged"/>.
        ///
        /// The lightning amplification itself is evaluated by the ElementResolver at spell-cast time;
        /// this test confirms the prerequisite — the unit is correctly marked as Charged.
        /// </summary>
        [Test]
        public void ChargedStatus_Apply_MarksUnitAsCharged()
        {
            UnitState unit = MakeUnit("p1_electro_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_electro_0",
                new StatusEffect(StatusType.Charged, duration: 2, stackCount: 1, sourceId: "electro"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_electro_0", StatusType.Charged), Is.True,
                "CHARGED must be reported as active by StatusManager after application.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Charged"),
                "UnitState.ActiveStatusTypes must contain 'Charged' after the status is applied, "
                + "enabling ElementResolver to detect and amplify incoming lightning.");
        }

        // =========================================================================
        // Test 5 — Silence blocks on-death effects
        // =========================================================================

        /// <summary>
        /// After applying SILENCED via StatusManager, the unit's <see cref="UnitState.ActiveStatusTypes"/>
        /// must contain <c>"Silenced"</c>. The death-resolution layer checks for this key to suppress
        /// passive on-death abilities (e.g. Necromancer corpse-fuel generation) while the unit is silenced.
        ///
        /// This test verifies the registration pre-condition that the resolver depends on.
        /// </summary>
        [Test]
        public void SilencedStatus_Apply_BlocksOnDeathEffects()
        {
            UnitState unit = MakeUnit("p1_soni_0", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_soni_0",
                new StatusEffect(StatusType.Silenced, duration: 2, stackCount: 1, sourceId: "soni"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_soni_0", StatusType.Silenced), Is.True,
                "SILENCED must be reported as active by StatusManager after application.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Silenced"),
                "UnitState.ActiveStatusTypes must contain 'Silenced', which the death-resolution "
                + "layer checks to block on-death passive effects while the unit is silenced.");
        }

        // =========================================================================
        // Test 6 — Curse reduces healing
        // =========================================================================

        /// <summary>
        /// <see cref="CursedStatus.ModifyHealing"/> must return exactly 50% of the raw healing
        /// amount (integer truncation). Callers apply this modifier before restoring HP to any
        /// unit whose <see cref="UnitState.ActiveStatusTypes"/> contains <c>"Cursed"</c>.
        /// </summary>
        [Test]
        public void CursedStatus_ModifyHealing_ReducesHealingByHalf()
        {
            var cursed = new CursedStatus(duration: 3, sourceId: "necro");

            const int rawHeal100 = 100;
            const int rawHeal15  = 15;
            const int rawHealOdd = 7;

            Assert.That(cursed.ModifyHealing(rawHeal100), Is.EqualTo(50),
                $"CursedStatus.ModifyHealing({rawHeal100}) must return 50 (50% of {rawHeal100}).");
            Assert.That(cursed.ModifyHealing(rawHeal15), Is.EqualTo(7),
                $"CursedStatus.ModifyHealing({rawHeal15}) must return 7 (integer floor of 50% of {rawHeal15}).");
            Assert.That(cursed.ModifyHealing(rawHealOdd), Is.EqualTo(3),
                $"CursedStatus.ModifyHealing({rawHealOdd}) must return 3 (integer floor of 50% of {rawHealOdd}).");

            // Verify the multiplier constant is exactly 0.5.
            Assert.That(CurseHealingMult, Is.EqualTo(0.5f),
                "CursedStatus.HealingMultiplier must be 0.5 (50% reduction).");
        }

        // =========================================================================
        // Test 7 — Status expiry after duration
        // =========================================================================

        /// <summary>
        /// A status with an initial duration of 2 must survive the first tick and expire
        /// (be fully removed from StatusManager) after the second tick. After expiry,
        /// <see cref="StatusManager.HasStatus"/> must return <c>false</c> and
        /// <see cref="UnitState.ActiveStatusTypes"/> must no longer contain the key.
        /// </summary>
        [Test]
        public void Poisoned_ExpiresAfterDuration()
        {
            UnitState unit = MakeUnit("p1_toxi_1", Player1, hp: 200);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_toxi_1",
                new StatusEffect(StatusType.Poisoned, duration: 2, stackCount: 1, sourceId: "toxi"),
                unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_toxi_1", StatusType.Poisoned), Is.True,
                "Pre-condition: POISONED must be active before any ticks.");

            // First tick — duration drops to 1, status still active.
            _statusManager.TickStatuses(_state);
            Assert.That(_statusManager.HasStatus("p1_toxi_1", StatusType.Poisoned), Is.True,
                "POISONED must still be active after the first tick (duration was 2, now 1).");

            // Second tick — duration reaches 0, status must be removed.
            _statusManager.TickStatuses(_state);
            Assert.That(_statusManager.HasStatus("p1_toxi_1", StatusType.Poisoned), Is.False,
                "POISONED with initial duration = 2 must be removed after two ticks.");
            Assert.That(unit.ActiveStatusTypes, Does.Not.Contain("Poisoned"),
                "ActiveStatusTypes must not contain 'Poisoned' after the status expires.");
        }

        // =========================================================================
        // Test 8 — Duplicate application behavior
        // =========================================================================

        /// <summary>
        /// Applying the same duration-stacking status type twice must extend the total
        /// remaining duration rather than resetting or ignoring the second application.
        /// Tested for <see cref="StatusType.Burning"/> (3 + 2 = 5 total turns).
        /// </summary>
        [Test]
        public void BurningStatus_DuplicateApplication_StacksDuration()
        {
            UnitState unit = MakeUnit("p1_pyro_1", Player1, hp: 100);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus("p1_pyro_1",
                new StatusEffect(StatusType.Burning, duration: 3, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);
            _statusManager.ApplyStatus("p1_pyro_1",
                new StatusEffect(StatusType.Burning, duration: 2, stackCount: 1, sourceId: "pyro"),
                unit, _state.TurnNumber);

            StatusEffect burning = _statusManager
                .GetStatuses("p1_pyro_1")
                .FirstOrDefault(e => e.Type == StatusType.Burning);

            Assert.That(burning, Is.Not.Null,
                "BURNING must remain active after two applications.");
            Assert.That(burning.Duration, Is.EqualTo(5),
                "Applying BURNING with duration 3 then duration 2 must yield a combined duration of 5 "
                + "(duration-stacking rule: 3 + 2 = 5).");
            Assert.That(burning.StackCount, Is.EqualTo(1),
                "BURNING stack count must remain 1 regardless of how many times it is applied.");
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
                position:          new GridPosition(5, 5),
                maxHP:             hp,
                moveRange:         4,
                pointCost:         100
            );
        }
    }
}
