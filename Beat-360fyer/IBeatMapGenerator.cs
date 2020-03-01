using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BeatMapGeneratorAttribute : Attribute
    {
        public int Version { get; }
        public string Name { get; }

        public BeatMapGeneratorAttribute(string generatorName, int generatorVersion)
        {
            this.Version = generatorVersion;
            this.Name = generatorName;
        }
    }

    public interface IBeatMapGenerator
    {
        string GeneratedGameModeName { get; }
        BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset);
        IBeatMapGeneratorSettings Settings { get; set; }
    }

    public interface IBeatMapGeneratorSettings
    { }

    public static class BeatMapGeneratorExtensions
    {
        public static bool IsDefault(this IBeatMapGeneratorSettings settings)
        {
            if (settings == null)
                return false;

            return Activator.CreateInstance(settings.GetType()).Equals(settings);
        }

        public static BeatMapGeneratorAttribute GetInformation(this IBeatMapGenerator generator)
        {
            return BeatMapGenerator.GetGeneratorInfo(generator.GetType());
        }
    }
}
