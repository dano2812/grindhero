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
