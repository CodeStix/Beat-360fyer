namespace Stx.ThreeSixtyfyer.Generators
{
    public class ExampleGeneratorSettings
    {
        public bool doSomeThing = true;
    }

    public class ExampleGenerator : IBeatMapGenerator
    {
        public int Version => 1;
        public string GeneratedGameModeName => "90Degree";
        public string Author => "CodeStix";
        public object Settings { get; set; }

        public BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset)
        {
            ExampleGeneratorSettings settings = (ExampleGeneratorSettings)Settings;
            BeatMap map = new BeatMap(standard);

            // Do stuff with the map
            if (settings.doSomeThing)
            {
                // ...
            }

            return map;
        }
    }
}
