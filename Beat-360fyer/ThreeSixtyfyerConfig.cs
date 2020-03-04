using System.Collections.Generic;

namespace Stx.ThreeSixtyfyer
{
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
