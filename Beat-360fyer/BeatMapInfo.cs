using Newtonsoft.Json;
using Stx.ThreeSixtyfyer.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    [Serializable]
    public class BeatMapInfo
    {
        [JsonProperty("_version")]
        public string version;
        [JsonProperty("_songName")]
        public string songName;
        [JsonProperty("_songSubName")]
        public string songSubName;
        [JsonProperty("_songAuthorName")]
        public string songAuthorName;
        [JsonProperty("_levelAuthorName")]
        public string levelAuthorName;
        [JsonProperty("_beatsPerMinute")]
        public float beatsPerMinute;
        [JsonProperty("_songTimeOffset")]
        public float songTimeOffset;
        [JsonProperty("_shuffle")]
        public float shuffle;
        [JsonProperty("_shufflePeriod")]
        public float shufflePeriod;
        [JsonProperty("_previewStartTime")]
        public float previewStartTime;
        [JsonProperty("_previewDuration")]
        public float previewDuration;
        [JsonProperty("_songFilename")]
        public string songFilename;
        [JsonProperty("_coverImageFilename")]
        public string coverImageFilename;
        [JsonProperty("_environmentName")]
        public string environmentName;
        [JsonProperty("_allDirectionsEnvironmentName")]
        public string allDirectionsEnvironmentName;
        [JsonProperty("_difficultyBeatmapSets")]
        public List<BeatMapDifficultySet> difficultyBeatmapSets;
        [JsonProperty("_customData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public BeatMapInfoCustomData customData;

        [JsonIgnore]
        public string mapDirectoryPath;
        [JsonIgnore]
        public string mapInfoPath;

        public static BeatMapInfo FromFile(string absoluteInfoFilePath)
        {
            BeatMapInfo info = JsonConvert.DeserializeObject<BeatMapInfo>(File.ReadAllText(absoluteInfoFilePath));
            info.mapInfoPath = absoluteInfoFilePath;
            info.mapDirectoryPath = new FileInfo(absoluteInfoFilePath).Directory.FullName;
            return info;
        }

        public void SaveToFile(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        public void CreateBackup(bool overwrite = false)
        {
            string backupFile = Path.Combine(mapDirectoryPath, "Info.dat.bak");
            if (overwrite || !File.Exists(backupFile))
                File.Copy(mapInfoPath, backupFile, true);
        }

        public BeatMapDifficultySet GetGameMode(string gameMode)
        {
            return difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == gameMode);
        }

        public BeatMapDifficulty GetGameModeDifficulty(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            BeatMapDifficultySet diffSet = GetGameMode(gameMode);
            return diffSet?.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());
        }

        public bool RemoveGameModeDifficulty(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            BeatMapDifficultySet diffSet = GetGameMode(gameMode);

            if (diffSet == null)
                return false; // it is already removed

            bool res = diffSet.difficultyBeatmaps.RemoveAll((diff) => diff.difficulty == difficulty.ToString()) > 0;

            if (diffSet.difficultyBeatmaps.Count == 0) // remove gamemode if all difficulties are removed
                difficultyBeatmapSets.RemoveAll((diff) => diff.beatmapCharacteristicName == gameMode);

            return res;
        }

        public bool AddGameModeDifficulty(BeatMapDifficulty newDifficulty, string gameMode, bool replaceExisting)
        {
            BeatMapDifficultySet newDiffSet = difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == gameMode);
            if (newDiffSet == null)
            {
                newDiffSet = new BeatMapDifficultySet()
                {
                    beatmapCharacteristicName = gameMode,
                    difficultyBeatmaps = new List<BeatMapDifficulty>()
                };
                difficultyBeatmapSets.Add(newDiffSet);
            }

            BeatMapDifficulty existingDiff = newDiffSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            if (existingDiff != null)
            {
                if (!replaceExisting)
                    return false;

                newDiffSet.difficultyBeatmaps.Remove(existingDiff);
            }

            newDiffSet.difficultyBeatmaps.Add(newDifficulty);
            newDiffSet.difficultyBeatmaps = newDiffSet.difficultyBeatmaps.OrderBy((diff) => diff.difficultyRank).ToList();
            return true;
        }

        public void AddContributor(string name, string role, string iconPath = "")
        {
            if (customData == null)
                customData = new BeatMapInfoCustomData();
            if (customData.contributors == null)
                customData.contributors = new List<BeatMapContributor>();
            if (!customData.contributors.Any((cont) => cont.name == name))
                customData.contributors.Add(new BeatMapContributor()
                {
                    name = name,
                    role = role,
                    iconPath = iconPath
                });
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(songAuthorName) ? songName : $"{songName} - {songAuthorName}";
        }
    }

    [Serializable]
    public class BeatMapInfoCustomData
    {
        [JsonProperty("_customEnvironment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string customEnvironment;
        [JsonProperty("_customEnvironmentHash", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string customEnvironmentHash;
        [JsonProperty("_contributors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<BeatMapContributor> contributors;
    }

    [Serializable]
    public class BeatMapGeneratorConfig
    {
        [JsonProperty("_settings")]
        public object settings;
        [JsonProperty("_version")]
        public int version;
        [JsonProperty("_difficulties")]
        public HashSet<BeatMapDifficultyLevel> difficulties;

        public bool ShouldRegenerate(object newSettings, int newVersion)
        {
            if (newVersion > version)
                return true;
            if (JsonConvert.SerializeObject(newSettings) != JsonConvert.SerializeObject(settings)) // Slow should fix!
                return true;
            return false;
        }

        public bool ShouldSaveInfo(HashSet<BeatMapDifficultyLevel> newDifficulties)
        {
            return !HasDifficulties(newDifficulties);
        }

        public static BeatMapGeneratorConfig FromGenerator(IBeatMapGenerator generator, HashSet<BeatMapDifficultyLevel> difficulties)
        {
            return new BeatMapGeneratorConfig()
            {
                settings = generator.Settings,
                version = generator.Version,
                difficulties = difficulties
            };
        }

        public void SaveToFile(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        public bool HasDifficulties(IReadOnlyCollection<BeatMapDifficultyLevel> all)
        {
            foreach (BeatMapDifficultyLevel diff in all)
            {
                if (!difficulties.Contains(diff))
                    return false;
            }
            return true;
        }

        public static BeatMapGeneratorConfig FromFile(string file)
        {
            return JsonConvert.DeserializeObject<BeatMapGeneratorConfig>(File.ReadAllText(file));
        }
    }

    [Serializable]
    public struct BeatMapContributor
    {
        [JsonProperty("_role")]
        public string role;
        [JsonProperty("_name")]
        public string name;
        [JsonProperty("_iconPath")]
        public string iconPath;
    }

    [Serializable]
    public class BeatMapDifficultySet
    {
        [JsonProperty("_beatmapCharacteristicName")]
        public string beatmapCharacteristicName;
        [JsonProperty("_difficultyBeatmaps")]
        public List<BeatMapDifficulty> difficultyBeatmaps;
    }

    [Serializable]
    public class BeatMapDifficulty
    {
        [JsonProperty("_difficulty")]
        public string difficulty;
        [JsonProperty("_difficultyRank")]
        public int difficultyRank;
        [JsonProperty("_beatmapFilename")]
        public string beatmapFilename;
        [JsonProperty("_noteJumpMovementSpeed")]
        public float noteJumpMovementSpeed;
        [JsonProperty("_noteJumpStartBeatOffset")]
        public float noteJumpStartBeatOffset;
        [JsonProperty("_customData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object customData;

        public static IEnumerable<BeatMapDifficultyLevel> AllDiffultyLevels => Enum.GetValues(typeof(BeatMapDifficultyLevel)).Cast<BeatMapDifficultyLevel>();

        public BeatMap LoadBeatMap(string mapDirectory)
        {
            string fullPath = Path.Combine(mapDirectory, beatmapFilename);
            return JsonConvert.DeserializeObject<BeatMap>(File.ReadAllText(fullPath));
        }

        public void SaveBeatMap(string mapDirectory, BeatMap map)
        {
            string fullPath = Path.Combine(mapDirectory, beatmapFilename);
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(map));
        }

        public static BeatMapDifficulty Create(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            return new BeatMapDifficulty()
            {
                difficulty = difficulty.ToString(),
                difficultyRank = (int)difficulty,
                beatmapFilename = gameMode + difficulty.ToString() + ".dat",
                noteJumpMovementSpeed = 0.0f,
                noteJumpStartBeatOffset = 0.0f
            };
        }

        public static BeatMapDifficulty CopyFrom(BeatMapDifficulty difficulty, string gameMode = "")
        {
            return new BeatMapDifficulty()
            {
                difficulty = difficulty.difficulty,
                difficultyRank = difficulty.difficultyRank,
                beatmapFilename = gameMode + difficulty.difficulty + ".dat",
                noteJumpMovementSpeed = difficulty.noteJumpMovementSpeed,
                noteJumpStartBeatOffset = difficulty.noteJumpStartBeatOffset,
                customData = difficulty.customData
            };
        }
    }
}
