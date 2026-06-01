using System;
using System.Collections.Generic;
using NUnit.Framework;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Core.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for <see cref="DraftStateManager"/>.
    /// Covers: normal pick sequence, ban blocking a pick, duplicate pick rejection,
    /// draft completion when all slots are filled, and available mancer pool shrinkage.
    /// </summary>
    [TestFixture]
    public class DraftStateManagerTests
    {
        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private DraftStateManager _draft;

        [SetUp]
        public void SetUp()
        {
            _draft = new DraftStateManager();
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // ---------------------------------------------------------------------------
        // 1. Normal pick sequence
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A complete ban-then-pick sequence for two players should transition the
        /// draft through Banning → Picking in the correct turn order and record picks.
        /// </summary>
        [Test]
        public void PickMancer_NormalSequence_RecordsPicksInCorrectOrder()
        {
            _draft.StartDraft(new[] { Player1, Player2 });

            // Banning phase — 1 ban per player, alternating starting with p1.
            _draft.BanMancer(Player1, "pyromancer");
            _draft.BanMancer(Player2, "hydromancer");

            Assert.That(_draft.GetSessionSnapshot().Phase, Is.EqualTo(DraftPhase.Picking),
                "Phase should advance to Picking after all bans are done.");

            // Picking phase — 3 picks per player, alternating starting with p1.
            _draft.PickMancer(Player1, "cryomancer");
            _draft.PickMancer(Player2, "geomancer");
            _draft.PickMancer(Player1, "aeromancer");
            _draft.PickMancer(Player2, "electromancer");
            _draft.PickMancer(Player1, "necromancer");
            _draft.PickMancer(Player2, "chronomancer");

            DraftSessionData session = _draft.GetSessionSnapshot();
            Assert.That(session.PickList["cryomancer"], Is.EqualTo(Player1));
            Assert.That(session.PickList["geomancer"],  Is.EqualTo(Player2));
            Assert.That(session.PickList["necromancer"], Is.EqualTo(Player1));
        }

        // ---------------------------------------------------------------------------
        // 2. Ban blocks a subsequent pick
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Attempting to pick a banned Mancer should throw <see cref="InvalidOperationException"/>.
        /// </summary>
        [Test]
        public void PickMancer_BannedMancer_ThrowsInvalidOperationException()
        {
            _draft.StartDraft(new[] { Player1, Player2 });

            _draft.BanMancer(Player1, "pyromancer");
            _draft.BanMancer(Player2, "hydromancer");

            // pyromancer is banned — picking it should be rejected.
            Assert.Throws<InvalidOperationException>(() =>
                _draft.PickMancer(Player1, "pyromancer"),
                "Picking a banned Mancer should throw InvalidOperationException.");
        }

        // ---------------------------------------------------------------------------
        // 3. Duplicate pick rejected
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Attempting to pick the same Mancer a second time (by any player) should throw
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Test]
        public void PickMancer_DuplicatePick_ThrowsInvalidOperationException()
        {
            _draft.StartDraft(new[] { Player1, Player2 });

            _draft.BanMancer(Player1, "pyromancer");
            _draft.BanMancer(Player2, "hydromancer");

            _draft.PickMancer(Player1, "cryomancer");
            _draft.PickMancer(Player2, "geomancer");

            // cryomancer already picked — p1's second pick attempt on the same Mancer.
            Assert.Throws<InvalidOperationException>(() =>
                _draft.PickMancer(Player1, "cryomancer"),
                "Picking an already-picked Mancer should throw InvalidOperationException.");
        }

        // ---------------------------------------------------------------------------
        // 4. Draft completes when all slots are filled
        // ---------------------------------------------------------------------------

        /// <summary>
        /// After every player has used all their pick slots the draft phase should be
        /// Complete and <see cref="DraftStateManager.IsDraftComplete"/> should return true.
        /// A <see cref="DraftCompletedEvent"/> should also be fired.
        /// </summary>
        [Test]
        public void PickMancer_AllSlotsFilled_SetsDraftPhaseToComplete()
        {
            _draft.StartDraft(new[] { Player1, Player2 });

            bool completedEventFired = false;
            SimulationEventBus.Subscribe<DraftCompletedEvent>(_ => completedEventFired = true);

            _draft.BanMancer(Player1, "pyromancer");
            _draft.BanMancer(Player2, "hydromancer");

            // Fill all 6 pick slots (3 per player).
            _draft.PickMancer(Player1, "cryomancer");
            _draft.PickMancer(Player2, "geomancer");
            _draft.PickMancer(Player1, "aeromancer");
            _draft.PickMancer(Player2, "electromancer");
            _draft.PickMancer(Player1, "necromancer");
            _draft.PickMancer(Player2, "chronomancer");

            Assert.That(_draft.IsDraftComplete, Is.True, "IsDraftComplete should be true after all slots are filled.");
            Assert.That(_draft.GetSessionSnapshot().Phase, Is.EqualTo(DraftPhase.Complete));
            Assert.That(completedEventFired, Is.True, "DraftCompletedEvent should have been fired.");
        }

        // ---------------------------------------------------------------------------
        // 5. Available mancers shrinks correctly after bans and picks
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Each ban and pick should reduce <see cref="DraftStateManager.GetAvailableMancers"/> by one.
        /// Banned and picked Mancers must not appear in the available set.
        /// </summary>
        [Test]
        public void GetAvailableMancers_AfterBansAndPicks_ReturnsReducedSet()
        {
            _draft.StartDraft(new[] { Player1, Player2 });

            int initialCount = _draft.GetAvailableMancers().Count;
            Assert.That(initialCount, Is.EqualTo(DraftStateManager.FullRoster.Count),
                "All roster Mancers should be available before any bans or picks.");

            _draft.BanMancer(Player1, "pyromancer");
            Assert.That(_draft.GetAvailableMancers().Count, Is.EqualTo(initialCount - 1));
            Assert.That(_draft.GetAvailableMancers(), Does.Not.Contain("pyromancer"),
                "Banned Mancer must not appear in the available set.");

            _draft.BanMancer(Player2, "hydromancer");
            Assert.That(_draft.GetAvailableMancers().Count, Is.EqualTo(initialCount - 2));

            _draft.PickMancer(Player1, "cryomancer");
            Assert.That(_draft.GetAvailableMancers().Count, Is.EqualTo(initialCount - 3));
            Assert.That(_draft.GetAvailableMancers(), Does.Not.Contain("cryomancer"),
                "Picked Mancer must not appear in the available set.");
        }
    }
}
