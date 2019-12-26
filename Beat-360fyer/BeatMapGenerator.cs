using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public static class BeatMapGenerator
    {
        public static bool AddNewDifficultyTo(BeatMapInfo info, BeatMapDifficulty newDifficulty, string gameMode, bool replaceExisting = false)
        {
            BeatMapDifficultySet newDiffSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == gameMode);
            if (newDiffSet == null)
            {
                newDiffSet = new BeatMapDifficultySet()
                {
                    beatmapCharacteristicName = gameMode,
                    difficultyBeatmaps = new List<BeatMapDifficulty>()
                };
                info.difficultyBeatmapSets.Add(newDiffSet);
            }

            BeatMapDifficulty existingDiff = newDiffSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            if (existingDiff != null)
            {
                if (!replaceExisting)
                    return false;

                newDiffSet.difficultyBeatmaps.Remove(existingDiff);
            }

            newDiffSet.difficultyBeatmaps.Add(newDifficulty);
            newDiffSet.difficultyBeatmaps = newDiffSet.difficultyBeatmaps.OrderBy((diff) => diff.difficultyRank).ToList();

            return true;
        }

        public static BeatMapDifficulty CreateNewDifficulty(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            return new BeatMapDifficulty()
            {
                difficulty = difficulty.ToString(),
                difficultyRank = (int)difficulty,
                beatmapFilename = gameMode + difficulty.ToString() + ".dat",
                noteJumpMovementSpeed = 0.0f,
                noteJumpStartBeatOffset = 0.0f
            };
        }

        public static bool Generate360ModeAndSave(BeatMapInfo info, BeatMapDifficultyLevel difficulty, bool replaceExising360Mode = false)
        {
            info.CreateBackup();

            //BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = CreateNewDifficulty(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!AddNewDifficultyTo(info, newDiff, "360Degree", replaceExising360Mode))
                return false;

            newDiff.SaveBeatMap(info.mapDirectoryPath, Generate360ModeFromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.songTimeOffset));

            info.AddContributor("CodeStix's 360fyer", "360 degree mode");
            info.SaveToFile(info.mapInfoPath);
            return true;
        }

        public static bool Generate360ModeAndCopy(BeatMapInfo info, string destination, BeatMapDifficultyLevel difficulty)
        {
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = CreateNewDifficulty(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!AddNewDifficultyTo(info, newDiff, "360Degree", true)) // always replace when making a copy
                return false;

            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);
            Directory.CreateDirectory(mapDestination);

            newDiff.SaveBeatMap(mapDestination, Generate360ModeFromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.songTimeOffset));

            info.difficultyBeatmapSets.RemoveAll((diffSet) => diffSet.beatmapCharacteristicName != "Standard" && diffSet.beatmapCharacteristicName != "360Degree");
            info.RemoveGameModeDifficulty(difficulty, "Standard");

            File.Copy(Path.Combine(info.mapDirectoryPath, info.coverImageFilename), Path.Combine(mapDestination, info.coverImageFilename), true);
            File.Copy(Path.Combine(info.mapDirectoryPath, info.songFilename), Path.Combine(mapDestination, info.songFilename), true);
            info.AddContributor("CodeStix's 360fyer", "360 degree mode");
            info.SaveToFile(Path.Combine(mapDestination, "Info.dat"));
            return true;
        }


    }
}
