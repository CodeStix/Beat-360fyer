using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Stx.ThreeSixtyfyer
{
    [Serializable]
    public class BeatMapGeneratorConfig
    {
        [JsonProperty("_configVersion")]
        public int configVersion = 1;
        [JsonProperty("_generatorSettings")]
        public object generatorSettings;
        [JsonProperty("_generatorVersion")]
        public int generatorVersion;
        [JsonProperty("_generatorName")]
        public string generatorName;
        [JsonProperty("_difficulties")]
        public HashSet<BeatMapDifficultyLevel> difficulties;

        public bool ShouldRegenerate(object newSettings, string newGeneratorName, int newGeneratorVersion)
        {
            if (generatorName != newGeneratorName)
                return true;
            if (newGeneratorVersion > generatorVersion)
                return true;
            if (!generatorSettings.Equals(newSettings))
                return true;
            return false;
        }

        public bool ShouldSaveInfo(HashSet<BeatMapDifficultyLevel> newDifficulties)
        {
            return !HasDifficulties(newDifficulties);
        }

        public static BeatMapGeneratorConfig FromGenerator(IBeatMapGenerator generator, HashSet<BeatMapDifficultyLevel> difficulties)
        {
            var info = generator.GetInformation();
            return new BeatMapGeneratorConfig()
            {
                generatorSettings = generator.Settings,
                generatorVersion = info.Version,
                generatorName = info.Name,
                difficulties = difficulties
            };
        }

        public void SaveToFile(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this, Program.JsonSettings));
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
            return JsonConvert.DeserializeObject<BeatMapGeneratorConfig>(File.ReadAllText(file), Program.JsonSettings);
        }
    }
}
