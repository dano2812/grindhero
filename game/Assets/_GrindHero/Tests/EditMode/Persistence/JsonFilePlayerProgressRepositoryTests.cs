using System;
using System.IO;
using GrindHero.Application.Progression;
using GrindHero.Infrastructure.Persistence;
using NUnit.Framework;

namespace GrindHero.Tests.Persistence
{
    public sealed class JsonFilePlayerProgressRepositoryTests
    {
        private string _saveDirectory;

        [SetUp]
        public void SetUp()
        {
            _saveDirectory = Path.Combine(
                Path.GetTempPath(),
                "GrindHero.Persistence.Tests",
                Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_saveDirectory))
            {
                Directory.Delete(_saveDirectory, true);
            }
        }

        [Test]
        public void SaveAndLoad_RoundTripsProgressAndRespawnState()
        {
            var repository = new JsonFilePlayerProgressRepository(_saveDirectory);
            var data = new PlayerProgressData
            {
                healthPoints = 4,
                axeAttackPoints = 2,
                currentHealth = 87.5f,
                heroPositionX = -10f,
                heroPositionY = 0f,
                heroPositionZ = 3.5f,
                firstBossDefeated = true,
                acceptedQuestIds = new[] { "quest.mosswatch.warden" },
                completedQuestIds = new[] { "quest.mosswatch.warden" },
                lootedTreasureIds = new[] { "treasure.mosswatch.grove" },
                completedSectionIds = new[] { "section.mosswatch" },
                enemyRespawns = new[]
                {
                    new EnemyRespawnData
                    {
                        spawnId = "basic-west",
                        nextRespawnUnixSeconds = 1_800_000_000,
                        respawnsToday = 2,
                        utcDayKey = DateTime.UtcNow.Date.Ticks,
                        isAlive = false
                    }
                }
            };

            repository.Save(data);
            var loaded = repository.TryLoad(out var result, out var recoveredFromBackup);

            Assert.That(loaded, Is.True);
            Assert.That(recoveredFromBackup, Is.False);
            Assert.That(result.healthPoints, Is.EqualTo(4));
            Assert.That(result.axeAttackPoints, Is.EqualTo(2));
            Assert.That(result.currentHealth, Is.EqualTo(87.5f));
            Assert.That(result.heroPositionX, Is.EqualTo(-10f));
            Assert.That(result.firstBossDefeated, Is.True);
            Assert.That(result.enemyRespawns, Has.Length.EqualTo(1));
            Assert.That(result.enemyRespawns[0].spawnId, Is.EqualTo("basic-west"));
            Assert.That(result.enemyRespawns[0].respawnsToday, Is.EqualTo(2));
            Assert.That(result.schemaVersion, Is.EqualTo(2));
            Assert.That(result.completedQuestIds, Is.EqualTo(data.completedQuestIds));
            Assert.That(result.lootedTreasureIds, Is.EqualTo(data.lootedTreasureIds));
            Assert.That(result.completedSectionIds, Is.EqualTo(data.completedSectionIds));
        }

        [Test]
        public void FirstSection_RewardsAndCompletionAreGrantedOnlyOnceAndSurviveReload()
        {
            var repository = new JsonFilePlayerProgressRepository(_saveDirectory);
            var session = new ProgressionSession(new PlayerProgressData(), repository);
            Assert.That(session.ClaimQuestReward("quest.mosswatch.warden", 2, 2), Is.False);
            Assert.That(session.CompleteSection("section.mosswatch"), Is.False);
            Assert.That(session.AcceptQuest("quest.mosswatch.warden"), Is.True);
            Assert.That(session.AcceptQuest("quest.mosswatch.warden"), Is.False);
            Assert.That(session.ClaimQuestReward("quest.mosswatch.warden", 2, 2), Is.False);
            Assert.That(session.LootTreasure("treasure.mosswatch.grove", 3), Is.True);
            Assert.That(session.LootTreasure("treasure.mosswatch.grove", 3), Is.False);
            Assert.That(session.CompleteFirstBoss(), Is.True);
            Assert.That(session.ClaimQuestReward("quest.mosswatch.warden", 2, 2), Is.True);
            Assert.That(session.CompleteSection("section.mosswatch"), Is.True);
            session.Save();
            Assert.That(repository.TryLoad(out var data, out _), Is.True);
            var reloaded = new ProgressionSession(data, repository);
            Assert.That(reloaded.ClaimQuestReward("quest.mosswatch.warden", 2, 2), Is.False);
            Assert.That(reloaded.LootTreasure("treasure.mosswatch.grove", 3), Is.False);
            Assert.That(reloaded.CompleteSection("section.mosswatch"), Is.False);
            Assert.That(data.healthPoints, Is.EqualTo(5));
            Assert.That(data.axeAttackPoints, Is.EqualTo(2));
        }

        [Test]
        public void VersionOneSave_MigratesWithoutLosingStatsBossOrRespawns()
        {
            Directory.CreateDirectory(_saveDirectory);
            var path = Path.Combine(_saveDirectory, "grindhero-save.json");
            File.WriteAllText(path,
                "{\"schemaVersion\":1,\"healthPoints\":7,\"axeAttackPoints\":3," +
                "\"currentHealth\":45,\"heroPositionX\":-8,\"firstBossDefeated\":true," +
                "\"enemyRespawns\":[{\"spawnId\":\"basic-hp-west\",\"nextRespawnUnixSeconds\":1800000000," +
                "\"respawnsToday\":2,\"utcDayKey\":638700000000000000,\"isAlive\":false}]}");
            var repository = new JsonFilePlayerProgressRepository(_saveDirectory);
            Assert.That(repository.TryLoad(out var data, out _), Is.True);
            Assert.That(data.schemaVersion, Is.EqualTo(2));
            Assert.That(data.healthPoints, Is.EqualTo(7));
            Assert.That(data.firstBossDefeated, Is.True);
            Assert.That(data.enemyRespawns[0].respawnsToday, Is.EqualTo(2));
            Assert.That(data.completedQuestIds, Is.Empty);
            Assert.That(data.lootedTreasureIds, Is.Empty);
            repository.Save(data);
            Assert.That(repository.TryLoad(out var reloaded, out _), Is.True);
            Assert.That(reloaded.axeAttackPoints, Is.EqualTo(3));
        }

        [Test]
        public void TryLoad_WhenPrimarySaveIsCorrupt_RestoresLastKnownGoodBackup()
        {
            var repository = new JsonFilePlayerProgressRepository(_saveDirectory);
            repository.Save(new PlayerProgressData
            {
                healthPoints = 3,
                currentHealth = 95f
            });
            repository.Save(new PlayerProgressData
            {
                healthPoints = 9,
                currentHealth = 80f
            });

            File.WriteAllText(
                Path.Combine(_saveDirectory, "grindhero-save.json"),
                "{ invalid json");

            var loaded = repository.TryLoad(out var result, out var recoveredFromBackup);

            Assert.That(loaded, Is.True);
            Assert.That(recoveredFromBackup, Is.True);
            Assert.That(result.healthPoints, Is.EqualTo(3));
            Assert.That(result.currentHealth, Is.EqualTo(95f));
        }
    }
}
