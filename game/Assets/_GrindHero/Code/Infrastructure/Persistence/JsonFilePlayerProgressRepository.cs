using System;
using System.IO;
using System.Text;
using GrindHero.Application.Progression;
using UnityEngine;

namespace GrindHero.Infrastructure.Persistence
{
    public sealed class JsonFilePlayerProgressRepository : IPlayerProgressRepository
    {
        private const int CurrentSchemaVersion = 1;
        private readonly string _savePath;
        private readonly string _backupPath;

        public JsonFilePlayerProgressRepository(string saveDirectory)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
            {
                throw new ArgumentException("A save directory is required.", nameof(saveDirectory));
            }

            _savePath = Path.Combine(saveDirectory, "grindhero-save.json");
            _backupPath = _savePath + ".bak";
        }

        public static JsonFilePlayerProgressRepository CreateForCurrentPlatform()
        {
            return new JsonFilePlayerProgressRepository(UnityEngine.Application.persistentDataPath);
        }

        public bool TryLoad(out PlayerProgressData data, out bool recoveredFromBackup)
        {
            recoveredFromBackup = false;
            if (File.Exists(_savePath))
            {
                try
                {
                    data = ReadValidated(_savePath);
                    return true;
                }
                catch (InvalidDataException) when (File.Exists(_backupPath))
                {
                    data = ReadValidated(_backupPath);
                    File.Copy(_backupPath, _savePath, true);
                    recoveredFromBackup = true;
                    return true;
                }
            }

            if (File.Exists(_backupPath))
            {
                data = ReadValidated(_backupPath);
                recoveredFromBackup = true;
                return true;
            }

            data = new PlayerProgressData();
            return false;
        }

        public void Save(PlayerProgressData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Validate(data);
            Directory.CreateDirectory(Path.GetDirectoryName(_savePath));

            var temporaryPath = _savePath + ".tmp";
            data.schemaVersion = CurrentSchemaVersion;
            File.WriteAllText(
                temporaryPath,
                JsonUtility.ToJson(data, true),
                new UTF8Encoding(false));

            if (File.Exists(_savePath))
            {
                File.Replace(temporaryPath, _savePath, _backupPath);
            }
            else
            {
                File.Move(temporaryPath, _savePath);
            }
        }

        private static PlayerProgressData ReadValidated(string path)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The player save file is empty.");
            }

            PlayerProgressData data;
            try
            {
                data = JsonUtility.FromJson<PlayerProgressData>(json);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("The player save file is invalid.", exception);
            }

            if (data == null)
            {
                throw new InvalidDataException("The player save file could not be read.");
            }

            Validate(data);
            return data;
        }

        private static void Validate(PlayerProgressData data)
        {
            if (data.schemaVersion != CurrentSchemaVersion)
            {
                throw new InvalidDataException(
                    $"Unsupported player save schema version: {data.schemaVersion}.");
            }

            if (data.healthPoints < 0 ||
                data.axeAttackPoints < 0 ||
                !IsFinite(data.currentHealth) ||
                data.currentHealth < 0f ||
                !IsFinite(data.heroPositionX) ||
                !IsFinite(data.heroPositionY) ||
                !IsFinite(data.heroPositionZ))
            {
                throw new InvalidDataException("The player save contains invalid values.");
            }

            data.enemyRespawns = data.enemyRespawns ?? Array.Empty<EnemyRespawnData>();
            foreach (var respawn in data.enemyRespawns)
            {
                if (respawn == null ||
                    string.IsNullOrWhiteSpace(respawn.spawnId) ||
                    respawn.nextRespawnUnixSeconds < 0 ||
                    respawn.respawnsToday < 0 ||
                    respawn.utcDayKey <= 0 ||
                    (respawn.isAlive && respawn.nextRespawnUnixSeconds > 0))
                {
                    throw new InvalidDataException("The player save contains invalid respawn data.");
                }
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
