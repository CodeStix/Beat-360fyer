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
        public string Author { get; }
        public string Description { get; }

        public BeatMapGeneratorAttribute(string generatorName, int generatorVersion, string author, string description)
        {
            Version = generatorVersion;
            Name = generatorName;
            Author = author;
            Description = description;
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
