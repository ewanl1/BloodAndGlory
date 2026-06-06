# Quest 3 Combat Training Manual Checklist

Run this checklist on Quest 3 standalone after automated Edit Mode and Play Mode tests pass.

## Setup

- Build target is Android.
- Scene is `CombatTrainingScene`.
- Player starts near the broadsword.
- One `PeasantBrown_Combat` enemy starts across the training floor.
- Combat debug overlay is visible in editor mirror or headset.

## Weapon Feel

- Broadsword follows the hand responsively enough for comfortable play.
- Slow contact produces chip/contact feedback.
- Committed swings produce visibly stronger damage.
- Wrist flicking does not dominate the fight.

## Hit And Damage

- Debug overlay shows last velocity, region, damage, and event.
- Head, torso, arm, and leg hits are distinguishable through debug output.
- Holding the blade inside the enemy does not repeatedly tick damage.

## Defense

- Player blocks enemy attacks with the broadsword.
- Blocked enemy attacks produce haptics, debug events, and no player damage.
- Parry events are readable in debug output.
- Peasant blocks or parries rarely.

## Enemy

- Peasant approaches using NavMesh.
- Peasant stops before in-place attacks.
- Telegraph is readable.
- Recovery is generous.
- Death stops combat behavior.

## Comfort And Performance

- Frame stability feels acceptable on Quest 3.
- Haptics are not noisy or constant.
- Debug overlay does not obscure basic play.
- No repeated console errors appear during the session.
