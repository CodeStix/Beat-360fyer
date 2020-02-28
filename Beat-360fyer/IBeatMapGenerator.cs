using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public interface IBeatMapGenerator
    {
        int Version { get; }
        string GeneratedGameModeName { get; }
        string Author { get; }
        BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset);
    }

    public interface IBeatMapGeneratorSettingsProvider
    { }

    public interface IBeatMapGenerator<TSettings> : IBeatMapGenerator, IBeatMapGeneratorSettingsProvider
    {
        TSettings Settings { get; set; }
    }
}
