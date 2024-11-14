# Portfolio
May not be clean, but code indeed (trying to learn)

# Froguelike
## New (WIP) Advanced Tilemap Procedural Generation
- [Tilemap](TilemapIO.cs)
- [GraphGenerator](GraphGenerator.cs)

## Main Player State Machine Scripts
- [Main State Machine](PlayerStateMachine.cs)

Parent:
- [Base State](BaseState.cs)

Children:
- [Idle State](PlayerIdleState.cs)[Uploading Modifier.cs…]()

- [Run State](PlayerRunState.cs)
- [Roll State](PlayerRollState.cs)
- [Pre-Attack State](PlayerPreAttackState.cs)
- [Attack State](PlayerAttackState.cs)
- [Charged Attack State](PlayerChargedAttackState.cs)
- [Tongue State](PlayerTongueState.cs)

## Behavior Tree Scripts

Trees:
- [Bloxzor](BossBT.cs)
- [Shogun](SecondBossBT.cs)
- [Normal Enemy](EnemyBT.cs)
- [Ranged Enemy](RangedEnemyBT.cs)
 
Decoraters (Basic)
- [Selector](Selector.cs)
- [Sequence](Sequence.cs)
- [Switch](Switch.cs)
- [Timer](Timer.cs)
 
Decoraters (Expanded)
- [Deferred Sequence](DeferredSequence.cs)
- [Fail Switch](FailSwitch.cs)
- [RandomizedSelector](RandomizedSelector.cs)
- [BossSwitch](BossSwitch.cs)

## Skill Tree Scripts

Main Tree Script
- [Skill Tree Alg](SkillTree.cs)

Idv Skill Scripts
- [Skill](Skill.cs)
  - [Modifier](Modifier.cs)
  - [Health Modifier](HealthModifier.cs)
  - [Damage Modifier](DamageModifier.cs)
  - [StaminaModifier](StaminaModifier.cs)
- Augments (Coming Soon)
- Spells (Also Coming Soon

## Various ManagerScripts

[Game Manager](GameManager.cs)
[





