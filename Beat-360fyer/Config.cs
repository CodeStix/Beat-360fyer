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
    public abstract class Config
    {
        public const string CONFIG_FILENAME = "config.json";

        [JsonProperty("version")]
        public abstract int Version { get; }

        protected Config() { }

        public static TConfig Load<TConfig>() where TConfig : Config, new()
        {
            TConfig n = new TConfig();
            if (!File.Exists(CONFIG_FILENAME))
            {
                return n;
            }
            else
            {
                TConfig c = JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(CONFIG_FILENAME), Program.JsonSettings);
                if (c.Version < n.Version)
                    c = (TConfig)n.Upgrade(c, c.Version);
                return c;
            }
        }

        public void Save()
        {
            File.WriteAllText(CONFIG_FILENAME, JsonConvert.SerializeObject(this, Program.JsonSettings));
        }

        public static bool TryLoad<TConfig>(out TConfig config) where TConfig : Config, new()
        {
            try
            {
                config = Load<TConfig>();
                return true;
            }
            catch
            {
                config = null;
                return false;
            }
        }

        public bool TrySave()
        {
            try
            {
                Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual Config Upgrade(dynamic oldConfig, int oldConfigVersion)
        {
            return oldConfig;
        }
    }

    public class ThreeSixtyfyerConfig : Config 
    {
        public override int Version => 1;

        public string packPath;
        public string bulkPath;
        public string exportPath;
        public string lastGeneratedMusicPackSourcePath;
        public string lastGeneratedMusicPackPath;

        public string generatorToUse = BeatMapGenerator.DefaultGenerator.GetInformation().Name;
        public Dictionary<string, IBeatMapGeneratorSettings> generatorSettings = new Dictionary<string, IBeatMapGeneratorSettings>();
    }
}
