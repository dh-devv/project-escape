using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Carten.Tests
{
    public class GameplayBehaviorTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject NewObject(string name)
        {
            GameObject go = new GameObject(name);
            objects.Add(go);
            return go;
        }
        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
        private PlayerController Player()
        {
            GameObject go = NewObject("Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player");
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            go.AddComponent<BoxCollider2D>();
            return go.AddComponent<PlayerController>();
        }
        private BossController Boss(bool final = false)
        {
            GameObject go = NewObject("Boss");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            BossController boss = go.AddComponent<BossController>();
            SetField(boss, "bossKind", final ? BossController.BossKind.FinalBoss : BossController.BossKind.MiniBoss);
            go.SetActive(true);
            return boss;
        }
        [TearDown]
        public void Cleanup()
        {
            foreach (BossFieldAttack field in Object.FindObjectsByType<BossFieldAttack>(FindObjectsSortMode.None))
                Object.DestroyImmediate(field.gameObject);
            foreach (GameObject go in objects) if (go != null) Object.DestroyImmediate(go);
            objects.Clear();
        }

        [UnityTest]
        public IEnumerator StunExpiresAndClearsMovementWithoutRemovingGravity()
        {
            PlayerController player = Player();
            player.StartGravityBoost(12f, 3f);
            player.TakeDamage(10f, 0.08f);
            Assert.That(player.IsStunned, Is.True);
            Assert.That(player.CanAct, Is.False);
            Assert.That(player.IsSkillMovementLocked, Is.False);
            Assert.That(player.GetComponent<Rigidbody2D>().gravityScale, Is.GreaterThan(0f));
            yield return new WaitForSeconds(0.12f);
            Assert.That(player.IsStunned, Is.False);
            Assert.That(player.CanAct, Is.True);
        }

        [Test]
        public void RestoringMapHealthPreservesPhaseAndRejectsInvalidHealth()
        {
            PlayerController player = Player();
            MethodInfo restore = typeof(PlayerController).GetMethod("RestoreHealthForMap", BindingFlags.Instance | BindingFlags.NonPublic);
            restore.Invoke(player, new object[] { 25f });
            Assert.That(player.CurrentHealth, Is.EqualTo(25f));
            Assert.That(player.CurrentPhase, Is.EqualTo(PlayerController.PlayerPhase.Phase3));
            restore.Invoke(player, new object[] { float.NaN });
            restore.Invoke(player, new object[] { 0f });
            Assert.That(player.CurrentHealth, Is.EqualTo(25f));
        }

        [Test]
        public void SceneStartKeepsTransferredHealthAndDamageUpdatesPersistentState()
        {
            GameStateManager state = NewObject("Game state").AddComponent<GameStateManager>();
            PlayerController player = Player();
            typeof(PlayerController).GetMethod("RestoreHealthForMap", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(player, new object[] { 25f });
            player.SendMessage("Start");
            Assert.That(player.CurrentHealth, Is.EqualTo(25f));
            player.TakeDamage(5f);
            Assert.That(state.PlayerHP, Is.EqualTo(20f));
            Assert.That(state.PlayerPhase, Is.EqualTo(3));
            PlayerController next = Player();
            next.SendMessage("Start");
            Assert.That(next.CurrentHealth, Is.EqualTo(20f));
            Assert.That(next.CurrentPhase, Is.EqualTo(PlayerController.PlayerPhase.Phase3));
        }

        [Test]
        public void AllSkillCooldownsRestoreAndUsingSkillPersistsItsCooldown()
        {
            GameStateManager state = NewObject("Game state").AddComponent<GameStateManager>();
            string[] timers = { "heavyStrikeTimer", "defenseSkillTimer", "gravityBoostTimer",
                "overdriveTimer", "dashSlashTimer", "boostExplosionTimer",
                "limitBreakTimer", "blinkDashTimer", "overloadBlastTimer" };
            for (int i = 0; i < timers.Length; i++)
                state.StartSkillCooldown((GameStateManager.SkillCooldownType)i, 10f + i);
            PlayerController player = Player();
            NewObject("AttackPoint").transform.SetParent(player.transform);
            PlayerSkillController skills = player.gameObject.AddComponent<PlayerSkillController>();
            skills.SendMessage("Start");
            for (int i = 0; i < timers.Length; i++)
                Assert.That((float)typeof(PlayerSkillController).GetField(timers[i], BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(skills), Is.EqualTo(10f + i).Within(0.01f));
            state.ResetSkillCooldowns();
            skills.SendMessage("Start");
            typeof(PlayerSkillController).GetMethod("UseSkill1", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(skills, null);
            Assert.That(state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.HeavyStrike), Is.GreaterThan(0f));
        }

        [Test]
        public void EncounterPersistsOnlyAfterSpawnAndParentCannotBypassOneTimeTrigger()
        {
            GameStateManager state = NewObject("Game state").AddComponent<GameStateManager>();
            PlayerController player = Player();
            EncounterController encounter = NewObject("Encounter").AddComponent<EncounterController>();
            GameObject child = NewObject("Trigger");
            child.transform.SetParent(encounter.transform);
            EncounterTrigger trigger = child.AddComponent<EncounterTrigger>();
            SetField(trigger, "triggerId", "test-encounter");
            LogAssert.Expect(LogType.Error, "[EncounterController] Spawner, spawn points and a positive enemy count are required.");
            encounter.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
            Assert.That(state.HasTriggered("test-encounter"), Is.False);
            GameObject template = NewObject("Enemy template");
            template.AddComponent<Rigidbody2D>();
            template.AddComponent<Enemy1Controller>();
            EnemySpawner spawner = NewObject("Spawner").AddComponent<EnemySpawner>();
            SetField(spawner, "enemy1Prefab", template);
            SetField(encounter, "enemySpawner", spawner);
            SetField(encounter, "spawnPoints", new[] { NewObject("Spawn").transform });
            encounter.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
            Assert.That(state.HasTriggered("test-encounter"), Is.True);
            Assert.That(spawner.GetSpawnedEnemies().Count, Is.EqualTo(2));
            SetField(encounter, "encounterStarted", false);
            encounter.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
            Assert.That(encounter.IsStarted(), Is.False);
            Assert.That(spawner.GetSpawnedEnemies().Count, Is.EqualTo(2));
            foreach (GameObject enemy in spawner.GetSpawnedEnemies()) objects.Add(enemy);
        }

        [Test]
        public void OfflineBossResultBelongsToSessionAfterBossObjectIsDestroyed()
        {
            PlayerController player = Player();
            LastSparkSession session = NewObject("Session").AddComponent<LastSparkSession>();
            BossController boss = Boss();
            BossResultReporter reporter = boss.gameObject.AddComponent<BossResultReporter>();
            boss.BeginEncounter();
            player.TakeDamage(80f);
            boss.TakeDamage(1000f);
            Assert.That(session.PendingResultCount, Is.EqualTo(1));
            reporter.Submit();
            Assert.That(session.PendingResultCount, Is.EqualTo(1));
            Object.DestroyImmediate(boss.gameObject);
            Assert.That(session.PendingResultCount, Is.EqualTo(1));
        }

        [Test]
        public void EncounterCannotSpawnASecondWaveWhenStartedTwice()
        {
            GameObject template = NewObject("Enemy template");
            template.AddComponent<Rigidbody2D>();
            template.AddComponent<Enemy1Controller>();
            EnemySpawner spawner = NewObject("Spawner").AddComponent<EnemySpawner>();
            SetField(spawner, "enemy1Prefab", template);
            EncounterController encounter = NewObject("Encounter").AddComponent<EncounterController>();
            SetField(encounter, "enemySpawner", spawner);
            SetField(encounter, "spawnPoints", new[] { NewObject("Spawn").transform });
            encounter.StartEncounter();
            encounter.StartEncounter();
            Assert.That(spawner.GetSpawnedEnemies().Count, Is.EqualTo(2));
            foreach (GameObject enemy in spawner.GetSpawnedEnemies()) objects.Add(enemy);
        }

        [Test]
        public void ProjectileConsumedByWallCannotThenHitPlayer()
        {
            PlayerController player = Player();
            GameObject wall = NewObject("Wall");
            wall.layer = LayerMask.NameToLayer("Ground");
            Collider2D wallCollider = wall.AddComponent<BoxCollider2D>();
            GameObject go = NewObject("Bullet");
            go.AddComponent<Rigidbody2D>();
            Enemy2Bullet bullet = go.AddComponent<Enemy2Bullet>();
            SetField(bullet, "obstacleLayer", LayerMask.GetMask("Ground"));
            bullet.Initialize(Vector2.right, 10f, LayerMask.GetMask("Player"));
            bullet.SendMessage("OnTriggerEnter2D", wallCollider);
            bullet.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
            Assert.That(player.CurrentHealth, Is.EqualTo(player.MaxHealth));
        }

        [Test]
        public void ZeroDamageAndCompleteDefenseDoNotApplyStun()
        {
            PlayerController player = Player();
            player.TakeDamage(0f, 1f);
            Assert.That(player.IsStunned, Is.False);
            player.SetAdditionalDamageReduction(1f);
            player.TakeDamage(20f, 1f);
            Assert.That(player.IsStunned, Is.False);
            Assert.That(player.CurrentHealth, Is.EqualTo(player.MaxHealth));
        }

        [Test]
        public void StunInterruptsAnExecutingMovementSkill()
        {
            PlayerController player = Player();
            GameObject point = NewObject("AttackPoint");
            point.transform.SetParent(player.transform);
            PlayerSkillController skills = player.gameObject.AddComponent<PlayerSkillController>();
            player.TakeDamage(80f);
            typeof(PlayerSkillController).GetMethod("UseSkill3", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(skills, null);
            Assert.That(skills.IsSkillExecuting, Is.True);
            Assert.That(player.IsSkillMovementLocked, Is.True);
            player.TakeDamage(1f, 0.5f);
            Assert.That(skills.IsSkillExecuting, Is.False);
            Assert.That(skills.IsBoostExplosionActive, Is.False);
            Assert.That(player.IsSkillMovementLocked, Is.False);
        }

        [Test]
        public void ShorterRepeatedStunDoesNotShortenRemainingStunAndDisableClearsIt()
        {
            PlayerController player = Player();
            player.TakeDamage(1f, 0.5f);
            player.TakeDamage(1f, 0.1f);
            Assert.That(player.HitReaction.RemainingStun, Is.GreaterThan(0.45f));
            player.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            Assert.That(player.IsStunned, Is.False);
        }

        [Test]
        public void LethalHitDoesNotLeaveAStunOrAllowActions()
        {
            PlayerController player = Player();
            player.TakeDamage(10000f, 0.5f);
            Assert.That(player.IsDead, Is.True);
            Assert.That(player.IsStunned, Is.False);
            Assert.That(player.CanAct, Is.False);
        }

        [Test]
        public void RangedProjectileHitsOnlyOnceWithMultiplePlayerColliders()
        {
            PlayerController player = Player();
            GameObject go = NewObject("Bullet");
            go.AddComponent<Rigidbody2D>();
            Enemy2Bullet bullet = go.AddComponent<Enemy2Bullet>();
            bullet.Initialize(Vector2.right, 10f, LayerMask.GetMask("Player"));
            Collider2D collider = player.GetComponent<Collider2D>();
            bullet.SendMessage("OnTriggerEnter2D", collider);
            bullet.SendMessage("OnTriggerEnter2D", collider);
            Assert.That(player.CurrentHealth, Is.EqualTo(96f).Within(0.01f));
            Assert.That(player.IsStunned, Is.True);
        }

        [Test]
        public void HeavyEnemyCanDamagePlayerThroughDamageInterfaceWithoutDuplicateHits()
        {
            PlayerController player = Player();
            GameObject extra = NewObject("Extra player collider");
            extra.transform.SetParent(player.transform);
            extra.layer = player.gameObject.layer;
            extra.AddComponent<CircleCollider2D>();
            GameObject go = NewObject("Heavy enemy");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<Enemy3Controller>();
            go.AddComponent<Enemy3AI>();
            Enemy3Combat combat = go.AddComponent<Enemy3Combat>();
            SetField(combat, "attackPoint", go.transform);
            go.SetActive(true);
            Physics2D.SyncTransforms();
            combat.PerformAttack();
            Assert.That(player.CurrentHealth, Is.EqualTo(94f).Within(0.01f));
            Assert.That(player.IsStunned, Is.True);
        }

        [Test]
        public void MiniBossNeverEntersThirdPhaseAndFinalBossDoes()
        {
            BossController mini = Boss();
            mini.TakeDamage(80f);
            Assert.That(mini.CurrentPhase, Is.EqualTo(BossController.BossPhase.Phase2));
            BossController final = Boss(true);
            final.TakeDamage(80f);
            Assert.That(final.CurrentPhase, Is.EqualTo(BossController.BossPhase.Phase3));
        }

        [UnityTest]
        public IEnumerator FinalBossCannotBeMovedByPhysics()
        {
            Player();
            BossController boss = Boss(true);
            boss.gameObject.AddComponent<BossAI>();
            Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
            Vector2 start = rb.position;
            rb.linearVelocity = Vector2.one * 20f;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(rb.position, start), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator FieldWarnsBeforeDamageAndHitsOnlyOnce()
        {
            PlayerController player = Player();
            BossController boss = Boss(true);
            BossFieldAttack field = BossFieldAttack.Create(boss, Vector2.zero, new Vector2(5, 5),
                LayerMask.GetMask("Player"), 10f, 0.25f, 0.15f, 0.2f);
            Assert.That(field.IsArmed, Is.False);
            Assert.That(player.CurrentHealth, Is.EqualTo(100f));
            yield return new WaitForSeconds(0.25f);
            Assert.That(player.CurrentHealth, Is.EqualTo(96f).Within(0.01f));
            yield return new WaitForSeconds(0.15f);
            Assert.That(player.CurrentHealth, Is.EqualTo(96f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator BossDeathCancelsPendingFieldDamage()
        {
            PlayerController player = Player();
            BossController boss = Boss(true);
            BossFieldAttack.Create(boss, Vector2.zero, new Vector2(5, 5), LayerMask.GetMask("Player"),
                10f, 0.25f, 0.15f, 0.2f);
            boss.TakeDamage(1000f);
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void InteractionSelectsNearestAndIsBlockedDuringStun()
        {
            PlayerController player = Player();
            GameObject close = NewObject("Close switch");
            close.transform.position = Vector3.right;
            close.AddComponent<BoxCollider2D>().isTrigger = true;
            InteractiveObject near = close.AddComponent<InteractiveObject>();
            GameObject far = NewObject("Far switch");
            far.transform.position = Vector3.right * 1.7f;
            far.AddComponent<BoxCollider2D>().isTrigger = true;
            far.AddComponent<InteractiveObject>();
            Physics2D.SyncTransforms();
            Assert.That(player.GetComponent<PlayerInteractor>().FindNearest(), Is.SameAs(near));
            player.TakeDamage(1f, 1f);
            Assert.That(player.GetComponent<PlayerInteractor>().FindNearest(), Is.Null);
            Assert.That(near.TryInteract(player), Is.False);
        }

        [Test]
        public void SingleUseInteractionTogglesGateOnlyOnce()
        {
            PlayerController player = Player();
            GameObject gate = NewObject("Gate");
            GameObject go = NewObject("Switch");
            go.AddComponent<BoxCollider2D>();
            InteractiveObject interactable = go.AddComponent<InteractiveObject>();
            SetField(interactable, "targets", new[] { gate });
            SetField(interactable, "singleUse", true);
            Assert.That(interactable.TryInteract(player), Is.True);
            Assert.That(gate.activeSelf, Is.False);
            Assert.That(interactable.TryInteract(player), Is.False);
            Assert.That(gate.activeSelf, Is.False);
        }
    }
}
