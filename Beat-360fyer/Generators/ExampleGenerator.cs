using System;
using System.Linq;

namespace Stx.ThreeSixtyfyer.Generators
{
    [Serializable]
    public class ExampleGeneratorSettings : IBeatMapGeneratorSettings
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

    [BeatMapGenerator("Example 90Degree Generator", 3, "CodeStix", "This simple generator just swings to the left and to the right each x seconds.\n" +
        "This is to showcase how a generator is made.\n" +
        "Want to create your own? Check the GitHub page for instructions.")]
    public class ExampleGenerator : IBeatMapGenerator
    {
        public string GeneratedGameModeName => "90Degree";
        public IBeatMapGeneratorSettings Settings { get; set; } = new ExampleGeneratorSettings(); // Set the default settings

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

            // Sort the BeatMap so that the inserted rotation events are in the right spot and not appended at the end of the events list
            modified.Sort();

            // Return the modfied BeatMap
            return modified;
        }
    }
}
