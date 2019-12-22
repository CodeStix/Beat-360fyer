using Newtonsoft.Json;
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

        public BeatMapDifficulty GetGameModeDifficulty(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            BeatMapDifficultySet diffSet = difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == gameMode);
            return diffSet?.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());
        }

        public void AddContributor(string name, string role, string iconPath = "")
        {
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
            return $"{songName} (by {songAuthorName})";
        }
    }

    [Serializable]
    public struct BeatMapInfoCustomData
    {
        [JsonProperty("_customEnvironment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string customEnvironment;
        [JsonProperty("_customEnvironmentHash", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string customEnvironmentHash;
        [JsonProperty("_contributors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<BeatMapContributor> contributors;
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
    }
}
