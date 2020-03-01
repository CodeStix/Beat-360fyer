namespace Stx.ThreeSixtyfyer.Generators
{
    public class ExampleGeneratorSettings
    {
        public int rotateEachNoteCount = 2;
        public int rotateCountPerDirectionSwitch = 4;

        // Override GetHashCode() is required to check if generator settings are equal
        public override int GetHashCode()
        {
            int hash = 13;
            unchecked
            {
                hash = (hash * 7) + rotateEachNoteCount.GetHashCode();
                hash = (hash * 7) + rotateCountPerDirectionSwitch.GetHashCode();
            }
            return hash;
        }

        // Override Equals() is required to check if generator settings are equal. Update the equals condition when adding more fields.
        public override bool Equals(object obj)
        {
            if (obj is ExampleGeneratorSettings s)
            {
                return rotateEachNoteCount == s.rotateEachNoteCount
                    && rotateCountPerDirectionSwitch == s.rotateCountPerDirectionSwitch;
            }
            else
            {
                return false;
            }
        }
    }

    public class ExampleGenerator : IBeatMapGenerator
    {
        public int Version => 1;
        public string Name => "Example 90Degree Generator";
        public string GeneratedGameModeName => "90Degree";
        public object Settings { get; set; }

        public BeatMap FromStandard(BeatMap standard, float bpm, float timeOffset)
        {
            ExampleGeneratorSettings settings = (ExampleGeneratorSettings)Settings;
            BeatMap modified = new BeatMap(standard); // Copy the original map

            // Implement your generator's logic
            int direction = 0;
            for(int i = 0; i < modified.notes.Count; i++)
            {
                BeatMapNote currentNote = modified.notes[i];
                if (i % settings.rotateEachNoteCount == 0)
                {
                    if (++direction % settings.rotateCountPerDirectionSwitch < settings.rotateCountPerDirectionSwitch / 2)
                        modified.AddGoLeftEvent(currentNote.time, 1);
                    else
                        modified.AddGoRightEvent(currentNote.time, 1);
                }
            }

            // Return the modfied BeatMap
            return modified;
        }
    }
}
