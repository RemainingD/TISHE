# Architecture and development map

## Execution

Scene House has a GameController. House.unity stores a 31.2 × 31.2 metre house with two floors 4.2 metres apart; startup generates a fallback only when player/keeper references are missing. Actual playable rooms exclude the exterior and partition walls. This is procedural Unity geometry, not the former browser renderer.

GameController owns the menu, pause, inventory, five task states, three attempts and ending. FirstPerson owns CharacterController movement and raycast interaction. Keeper handles patrol/investigation/chase and routes through HouseMap using the custom A*. HouseDoor animates a physical door. NoiseImpact turns object collisions into sound events.

HouseBuilder creates materials, walls, ceilings, stairs, doors, furniture, windows, lighting and the original simplified keeper mesh. The staircase has a smooth collision ramp under visible treads. Static architecture is batched. The flashlight casts low-resolution hard shadows; the quality option can turn off shadows.

## C# + Python

PythonBridge starts a local child process and exchanges one JSON object per line over stdin/stdout. No ports, server installation, API keys or network are involved. TISHE_PYTHON can specify an interpreter path. The child process is stopped when the game exits. Each response has a timeout.

Assets/StreamingAssets/Python/service.py dispatches five tasks to ai_engine.py and horror_ai.py. Unity sends task input and displays the response. Search navigation for the keeper remains C# so movement does not depend on IPC latency.

When Python cannot start or fails, the interface explicitly reports fallback to the equivalent C# algorithms in AiAlgorithms.cs. Do not present fallback results as Python results.

| Topic | Player task | Calculation |
| --- | --- | --- |
| Search, week 2 | Repair electrical circuit | BFS, DFS, A* or UCS on the editable weighted grid. The panel and archive show route, steps, cost and expanded nodes. |
| CSP, week 3 | Turn four lock discs | Backtracking with sum, distinctness, comparison and parity |
| Adversarial search, week 4 | Draw against the keeper's game | Minimax and alpha-beta, actual node and cutoff counters |
| Probability, week 6 | Choose ventilation channel | Bayesian posterior with positive or negative sensor evidence |
| HMM, week 7 | Configure radio / monitor hidden threat | Prediction, observation update and normalization over time |

The calibration panel takes a sequence chosen by the player. The tuned receiver processes audible keeper footsteps, doors opened by the keeper, and one silence observation per four quiet seconds. Each observation updates the same two-state HMM using its previous posterior. Separately, each audible event can stamp the keeper’s position at that instant on the same-floor map. The filter receives only the observation category.

## Important next development work

1. Complete manual gameplay acceptance: inspect every room, both stair directions, doors, dropped items, all tasks, captures and ending.
2. Replace simple character geometry with an original rigged low-poly model and better animation. Use legally sourced assets only and record their licences.
3. Improve room-specific art and believable clutter while keeping routes clear. Profile frame rate on the user's computer before adding effects.
4. Review subjective sound quality. Wardrobe now owns the 1.5-second movement into a hollow cabinet, two hinged doors, a viewing slit and exit. HorrorAudio plays recorded footsteps, door and floor foley, synthesized Pavel dialogue and two original music loops that crossfade on visual pursuit.
5. Consider splitting the IMGUI task panels into native UI Toolkit or Canvas screens after gameplay stabilizes. Current interface is functional and resolution-scaled.
6. Update the report, slides and video after the Unity gameplay is accepted. Old AURA materials are not a report about this new game.

## Testing

Editor menu TISHE / Check algorithms runs weighted path checks, impossible CSP, exhaustive player responses against minimax, Bayesian calculations, an independent HMM reference and house graph connectivity. These do not by themselves prove physical scene navigation or GUI usability.

Inspect validation-results.txt and unity-build.log for actual results. Logs and builds are local artifacts, not source assets. Keep .meta files for Assets in a future repository.

## 18 November redesign
The saved House scene now includes geometry, player, keeper and door references; GameController only generates a fallback house when those references are missing. HouseSceneTools.Bake produces this editable scene. ReferenceArt/ReferenceDetails add room-specific geometry and CC0 Poly Haven textures. CharacterRig builds joint-based characters; StoryDirector freezes gameplay through the existing Paused contract while animating with unscaled time. Wake lasts 7.8 seconds and leads into each of the three playable days. The third capture leads directly to the defeat screen. Escape follows the winding ForestYard trail at eye level. The final porch shot waits for the farewell recording before entering won / THE END. The flashlight now uses low-resolution hard shadows. CinemaLook adds a subtle vignette/desaturation. The cellar uses nodes 338–348 and a separate ramp; main house dimensions are unchanged. Five educational algorithms and the Python protocol are unchanged.

## 19 November room repair
RoomPortal replaces all wall-height gaps with lintels, masonry reveals, casings and 2.7-metre hinged doors. The room loops added on 18 November remain navigable. ReferenceArt and SoftFurnishing provide the imported old bed frame, fitted bookshelf, sewn cushions and densely tessellated linen. WindowWall cuts the six left-side window bays; their panes have a safety collider. Other decorative facade windows are not traversable openings.

SprintClock implements seven-second sprint and five-second exhaustion recovery; FirstPerson ticks it only during active gameplay. StoryDirector creates a separate first-person body with a shadow-casting head during wake-up and escape. The other rig casts the full player shadow during exploration. CharacterRig uses shaped limb meshes and joint animation; the characters remain procedural art, not photorealistic sculpted assets.
