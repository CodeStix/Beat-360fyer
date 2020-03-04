using System;

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

}
