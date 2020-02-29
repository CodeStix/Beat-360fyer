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
    public class Config
    {
        public const string CONFIG_FILENAME = "config.json";

        protected Config() { }

        public static TConfig Load<TConfig>() where TConfig : Config, new()
        {
            if (!File.Exists(CONFIG_FILENAME))
            {
                return new TConfig();
            }
            else
            {
                return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(CONFIG_FILENAME));
            }
        }

        public void Save()
        {
            File.WriteAllText(CONFIG_FILENAME, JsonConvert.SerializeObject(this));
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
    }

    public class ThreeSixtyfyerConfig : Config 
    {
        public string packPath;
        public string bulkPath;
        public string exportPath;
        public string lastGeneratedMusicPackPath;

        public string generatorToUse = BeatMapGenerator.DEFAULT_GENERATOR;
        public object generatorSettings = new BeatMap360GeneratorSettings();
    }
}
