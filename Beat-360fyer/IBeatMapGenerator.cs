
namespace Stx.ThreeSixtyfyer
{
    public interface IBeatMapGenerator
    {
        string GeneratedGameModeName { get; }
        BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset);
        object Settings { get; set; }
    }
}
