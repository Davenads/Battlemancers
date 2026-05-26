# Multiplayer — Technical Integration Spec

This document covers the implementation architecture for Battlemancers online multiplayer. It is intended as a developer reference for implementing and maintaining all networked features.

For the player-facing feature overview (lobbies, matchmaking, async mode, spectator, replays), see `design/game-modes.md` — Multiplayer Mode section.

---

## 1. Unity Gaming Services — Integration Overview

Battlemancers uses three UGS packages for online multiplayer. Each has a distinct responsibility and must not be confused with the others.

| Package | NuGet / Package ID | Responsibility |
|---|---|---|
| **Lobby** | `com.unity.services.lobby` | Room creation, discovery, join codes, player metadata storage |
| **Relay** | `com.unity.services.relay` | NAT punchthrough via Unity's relay server infrastructure; allocation of join codes for transport |
| **Netcode for GameObjects (NGO)** | `com.unity.netcode.gameobjects` | Client-server message passing; NetworkVariable sync; RPC dispatch |
| **Authentication** | `com.unity.services.authentication` | Anonymous or Steam-linked identity; required by all other UGS services |
| **Cloud Save** (optional) | `com.unity.services.cloudsave` | Async mode plan storage; replay cloud backup |

**Initialization order:**
```
await UnityServices.InitializeAsync();
await AuthenticationService.Instance.SignInAnonymouslyAsync();
// or: AuthenticationService.Instance.LinkWithSteamAsync(steamTicket)
```

All subsequent UGS calls require a valid Authentication session. Initialize once at app launch; session tokens are refreshed automatically.

---

## 2. Lobby Flow

The full lifecycle from match creation to simulation start.

```
Player A                          UGS Lobby                        Player B
--------                          ---------                        --------
CreateLobbyAsync(
  name, maxPlayers: 2,
  options: { IsPrivate: true }
)
  <- lobbyId, joinCode ("XK7P2Q")

[shares code out-of-band
 or via Steam friend invite]

                                                     JoinLobbyByCodeAsync("XK7P2Q")
                                                       <- lobby joined

UpdatePlayerDataAsync(
  { "warbandHash": hash,
    "ready": false }
)
                                  <-- both players see lobby state -->

[Player selects warband,
 clicks Ready]

UpdatePlayerDataAsync(             UpdatePlayerDataAsync(
  { "ready": true })               { "ready": true })

                     LobbyService polls until both ready == true

[both clients receive          <-- lobby callback -->
 AllPlayersReady event]

SubscribeToLobbyEventsAsync()   [starts Relay allocation -- see section 3]
```

**Warband lock-in:** Each player stores a `warbandHash` (SHA-1 of their serialized warband JSON) in their lobby player data before readying up. This is not a validation mechanism — it is used post-match to confirm neither player swapped their warband after lock-in. Full warband data is sent in the first `PlanPacket` alongside turn 1 commands.

**Map selection:** Host (Player A) stores the chosen map ID in lobby data. If both players vote for different maps, the map is randomly selected from the two candidates (coin flip seeded from lobby creation timestamp — deterministic).

---

## 3. Relay + Transport

Unity Relay removes the need for peer-to-peer direct connections and sidesteps NAT issues entirely.

```
Host (Player A):
  alloc = await RelayService.Instance.CreateAllocationAsync(maxConnections: 4)
  // maxConnections: 2 players + up to 2 spectators
  joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId)
  // store joinCode in lobby data under key "relayCode"

  transport = GetComponent<UnityTransport>();
  transport.SetHostRelayData(
    alloc.RelayServer.IpV4,
    alloc.RelayServer.Port,
    alloc.AllocationIdBytes,
    alloc.Key,
    alloc.ConnectionData
  );
  NetworkManager.Singleton.StartHost();

Client (Player B):
  relayCode = lobbyData["relayCode"]
  joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayCode)

  transport.SetClientRelayData(
    joinAlloc.RelayServer.IpV4,
    joinAlloc.RelayServer.Port,
    joinAlloc.AllocationIdBytes,
    joinAlloc.Key,
    joinAlloc.ConnectionData,
    joinAlloc.HostConnectionData
  );
  NetworkManager.Singleton.StartClient();
```

**Topology:** Client-server with Player A as host. The host runs both the server-side validation logic and their own client simulation. Player B is a pure client.

**Spectators:** Spectators join via the same relay join code (stored in lobby data as public metadata on non-private lobbies). They connect as clients but are assigned a `SpectatorRole` and never send `PlanPackets`. The server does not process input from spectators.

**Max players:** Relay allocation is set to `maxConnections: 4` (2 players + 2 spectators). The lobby enforces the 2-player cap separately — spectators join Relay directly without going through Lobby.

---

## 4. Netcode for GameObjects Setup

### Topology

- **Host = Player A** — runs both `NetworkManager` server logic and their own client
- **Client = Player B** — connects to Player A's Relay allocation
- The `SimulationBootstrapper` runs on both host and client identically; simulation state is never replicated directly

### Key NetworkObjects

| GameObject | Owner | Purpose |
|---|---|---|
| `MatchNetworkManager` | Server (host) | Receives PlanPackets; validates turn numbers; broadcasts ResolutionPackets |
| `PlayerNetworkState` (×2) | One per player | Stores ready state, turn lock-in status, connection health |
| `SpectatorNetworkState` (×N) | Server | Tracks spectator connections; feeds delayed board state |

### SimulationBootstrapper

`SimulationBootstrapper` is a plain C# class (no MonoBehaviour). It is instantiated identically on both clients at match start:

```csharp
var bootstrapper = new SimulationBootstrapper(
    mapData: loadedMap,
    p1Warband: p1WarbandData,
    p2Warband: p2WarbandData,
    seed: matchSeed  // shared via first ResolutionPacket
);
SimulationState state = bootstrapper.Initialize();
```

`matchSeed` is generated by the host and included in the `MatchStartPacket` sent to the client before turn 1. Both clients use this seed for all seeded `Random` instances within the simulation. This is the only shared initialization data required.

---

## 5. Command Synchronization Protocol

### Packet Structures

```csharp
// Sent by each client when they lock in their turn plan
struct PlanPacket : INetworkSerializable
{
    public ulong PlayerId;
    public int   TurnNumber;
    public int   WarbandHashChecksum;  // included only on turn 1
    public CommandEntry[] Commands;    // ordered list of unit activations
}

struct CommandEntry : INetworkSerializable
{
    public int    UnitId;
    public int    ActionTypeId;    // maps to enum ActionType
    public int    TargetTileX;
    public int    TargetTileY;
    public int    SecondaryTargetTileX;  // for multi-target spells; -1 if unused
    public int    SecondaryTargetTileY;
}

// Broadcast by server once both plans are received
struct ResolutionPacket : INetworkSerializable
{
    public int            TurnNumber;
    public CommandEntry[] P1Commands;
    public CommandEntry[] P2Commands;
}

// Sent by each client immediately after simulating a completed turn
struct StateHashPacket : INetworkSerializable
{
    public ulong PlayerId;
    public int   TurnNumber;
    public uint  StateHash;  // see section 6 for hash composition
}

// Broadcast by server to start a match
struct MatchStartPacket : INetworkSerializable
{
    public int  MatchSeed;
    public int  MapId;
    public byte[] P1WarbandJson;  // UTF-8 encoded warband JSON
    public byte[] P2WarbandJson;
}
```

### Turn Phase Sequence

```
PLANNING PHASE
  Both clients plan locally. No network traffic.
  Planning timer runs client-side; server is notified on lock-in only.

LOCK-IN
  Client calls: MatchNetworkManager.ServerRpc_SubmitPlan(planPacket)
  Server records the packet and marks the player as locked.
  Server sends client-side acknowledgement: ClientRpc_PlanReceived(playerId)
  Locked client displays "Waiting for opponent..." UI state.

RESOLUTION TRIGGER
  When both players are locked (or planning timer expires for a player):
    Server broadcasts: ClientRpc_ResolveTurn(resolutionPacket)
  Timeout handling:
    If one player's timer expires, server submits an empty Commands[] for that player.
    An empty plan = all units hold position; no activations.

LOCAL SIMULATION
  Both clients independently call:
    SimulationState newState = TurnResolver.Resolve(currentState, resolutionPacket);
  Simulation is deterministic — both clients produce identical results.

STATE HASH EXCHANGE
  Each client calls: MatchNetworkManager.ServerRpc_SubmitStateHash(hashPacket)
  Server compares hashes — see section 6.

NEXT TURN
  Server broadcasts: ClientRpc_BeginPlanning(turnNumber)
  Planning phase begins.
```

---

## 6. Desync Detection

After each turn resolves, both clients compute a hash of the current `SimulationState` and submit it to the server.

### Hash Composition

The state hash is a CRC32 (not cryptographic — speed matters here) over the following fields, serialized in a fixed canonical order:

```csharp
uint ComputeStateHash(SimulationState state)
{
    using var buffer = new MemoryStream();
    using var writer = new BinaryWriter(buffer);

    // Units — sorted by UnitId ascending (deterministic order)
    foreach (var unit in state.Units.OrderBy(u => u.UnitId))
    {
        writer.Write(unit.UnitId);
        writer.Write(unit.HP);
        writer.Write(unit.TileX);
        writer.Write(unit.TileY);
        writer.Write((int)unit.StatusFlags);
        writer.Write(unit.Temperature);
    }

    // Terrain — sorted by (X, Y) ascending
    foreach (var tile in state.Grid.Tiles.OrderBy(t => t.X).ThenBy(t => t.Y))
    {
        writer.Write(tile.X);
        writer.Write(tile.Y);
        writer.Write((int)tile.TerrainState);
        writer.Write(tile.BurnDuration);
    }

    writer.Write(state.TurnNumber);

    return Crc32.Compute(buffer.ToArray());
}
```

### Mismatch Handling

1. Server receives both `StateHashPacket`s.
2. If hashes match: proceed to next planning phase.
3. If hashes do not match:
   - Server broadcasts `ClientRpc_Desync(turnNumber)` to both clients
   - Both clients display "Desync detected — match cannot continue" and return to lobby
   - Match is flagged in UGS Analytics with both players' last 3 turn command logs for post-hoc debugging
   - Match result is voided (no MMR change for either player)

Desync should be impossible if the simulation is correctly deterministic. A mismatch in production indicates a bug in the simulation layer — treat every desync report as a high-priority bug.

---

## 7. Reconnect Protocol

### Server-Side Hold

When a client disconnects (`OnClientDisconnectCallback`):
1. Server marks the player as disconnected; starts a 90-second countdown.
2. Opponent's planning timer is paused; opponent sees "Waiting for opponent to reconnect..." banner.
3. Server retains the last 3 turns of `ResolutionPacket` history in memory.

### Client-Side Reconnect

1. Client re-launches / returns to app and attempts to rejoin the active match.
2. Client calls `LobbyService.Instance.JoinLobbyByIdAsync(lobbyId)` — lobby is kept open during the hold window.
3. Client re-joins via Relay (same join code, which remains valid).
4. Server detects re-join; sends a `ReconnectPacket`:

```csharp
struct ReconnectPacket : INetworkSerializable
{
    public int              CurrentTurnNumber;
    public int              ResumeFromTurn;   // currentTurn - 3
    public ResolutionPacket Turn1;            // oldest retained turn
    public ResolutionPacket Turn2;
    public ResolutionPacket Turn3;            // most recent completed turn
    public CommandEntry[]   PendingP1Plan;    // current turn plan if already submitted
}
```

5. Client re-simulates from `ResumeFromTurn` using the provided command history.
6. If re-simulation produces a state hash matching the server's stored hash for `CurrentTurnNumber - 1`, the match resumes normally.
7. If the reconnecting player had already submitted a plan for the current turn before disconnecting, that plan is preserved on the server and the game continues without data loss.

### Timeout

If the player does not reconnect within 90 seconds, server broadcasts `ClientRpc_MatchAbandoned(abandonedPlayerId)`:
- Remaining player wins by default
- MMR update is applied (win for remaining player; loss for abandoner)
- Abandons are tracked; repeated abandons trigger a matchmaking penalty cooldown

---

## 8. Async Mode Architecture

Async mode ("Challenge Mode") uses UGS Cloud Save to store turn plans server-side, eliminating the need for both players to be online simultaneously.

### Data Model

Plans are stored in Cloud Save under a per-match key:

```
Key:   "match_{matchId}_turn_{turnNumber}_player_{playerId}"
Value: serialized PlanPacket JSON
TTL:   72 hours (plans expire if opponent never responds)
```

### Flow

```
Player A submits turn plan:
  CloudSaveService.Instance.Data.ForceSaveAsync(key, planJson)
  Push notification sent to Player B via Unity Push Notifications

Player B opens app:
  Notification indicates "Opponent has submitted their plan for Match XK7P2Q"
  Player B opens the challenge, sees the board state, plans their turn
  Player B submits → plan stored in Cloud Save

Resolution:
  Server-side Cloud Function detects both plans present for turn N
  Loads both plans, simulates turn, stores result
  Sends push notifications to both players: "Turn N resolved — view results"
  Players open the app to see the replay of the resolved turn and plan turn N+1
```

### Client Behavior

- Async matches appear in Main Menu → Challenges → Active Challenges
- Each entry shows: opponent name, match turn number, status (Your turn / Waiting for opponent / Turn resolved — view)
- Tapping "view" plays back the resolved turn as an animation before showing the new board state for planning
- Async matches do not count toward ranked MMR; they use a separate "Challenge rating"

### Push Notifications

Unity Push Notifications package (`com.unity.services.push-notifications`) handles delivery. On mobile, standard OS push. On desktop/Steam, in-app notification banner only (Steam does not support background push to non-running games).

---

## 9. Steam Integration

Battlemancers distributes on Steam via Steamworks.NET. The following integration points are relevant to multiplayer.

### Steam Authentication Linking

Anonymous UGS auth can be linked to a Steam identity for persistent ranked progression:

```csharp
var ticket = await SteamUser.GetAuthSessionTicketAsync();
await AuthenticationService.Instance.LinkWithSteamAsync(
    ticket.Data,
    SteamUtils.GetAppID().ToString()
);
```

This is prompted on first ranked match. Players who skip it retain anonymous auth but cannot carry MMR across reinstalls.

### Steam Friends — Private Invites

Private lobbies can be shared via the Steam friends overlay in addition to the 6-character join code:

1. Host calls `SteamFriends.InviteUserToGame(friendSteamId, lobbyId)` — standard Steam invite
2. Friend receives a Steam invite notification; clicking it deep-links into Battlemancers and auto-joins the lobby
3. The UGS Lobby join code is passed as the connect string parameter in the Steam invite

### Steam Leaderboards — Ranked ELO

MMR is stored in UGS Cloud Save (authoritative) and mirrored to a Steam Leaderboard for display in the Steam overlay.

```csharp
// Called after each ranked match result is processed
SteamUserStats.UploadLeaderboardScore(
    leaderboard: _rankedLeaderboard,  // handle obtained at init
    method: ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate,
    score: currentMMR,
    details: null
);
```

The Steam leaderboard is display-only. MMR calculations happen server-side (UGS Cloud Code function) and cannot be spoofed by a modified client.

### Steamworks.NET Integration Points Summary

| Feature | Steamworks call |
|---|---|
| Auth token for UGS linking | `SteamUser.GetAuthSessionTicketAsync()` |
| Friend invite to private lobby | `SteamFriends.InviteUserToGame()` |
| Rich presence (current match status) | `SteamFriends.SetRichPresence()` |
| MMR leaderboard mirror | `SteamUserStats.UploadLeaderboardScore()` |
| Achievements | `SteamUserStats.SetAchievement()` |
| DLC / ownership check | `SteamApps.IsDlcInstalled()` — reserved for future content |

---

## Appendix: Packet Flow Diagram

```
Turn N — full packet lifecycle:

  Client A                  Server (Host)               Client B
  --------                  -------------               --------
  [plan locally]                                        [plan locally]

  ServerRpc_SubmitPlan(A)-->
                            mark A locked
                            ClientRpc_PlanReceived(A) -->
                                                        [waiting...]
                        <-- ServerRpc_SubmitPlan(B)
                            mark B locked
  ClientRpc_ResolveTurn(resolution) -------broadcast------->
  [simulate turn N]                                     [simulate turn N]

  ServerRpc_SubmitStateHash(A) -->
                            store hash A
                        <-- ServerRpc_SubmitStateHash(B)
                            compare hashes
                            [match] --> ClientRpc_BeginPlanning(N+1) --broadcast-->

Turn N+1 planning phase begins on both clients simultaneously.
```
