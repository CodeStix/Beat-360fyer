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
        object Settings { get; set; }
    }

    public static class IBeatMapGeneratorExtensions
    {
        public static TSettings GetSettings<TSettings>(this IBeatMapGenerator generator)
        {
            return (TSettings)generator;
        }
    }
}
