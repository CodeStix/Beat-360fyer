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
        string Name { get; }
        string GeneratedGameModeName { get; }
        BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset);
        IBeatMapGeneratorSettings Settings { get; set; }
    }

    public interface IBeatMapGeneratorSettings
    { }

    }
}
