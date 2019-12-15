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

        [JsonIgnore]
        public string mapRoot;

        public static BeatMapInfo FromFile(string file)
        {
            BeatMapInfo info = JsonConvert.DeserializeObject<BeatMapInfo>(File.ReadAllText(file));
            info.mapRoot = new FileInfo(file).Directory.FullName;
            return info;
        }

        public void SaveToFile(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        public override string ToString()
        {
            return $"{songName} (by {songAuthorName})";
        }
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
