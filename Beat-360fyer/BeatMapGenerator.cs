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
        public static BeatMap UseGenerator<TGenerator, TGeneratorSettings>(BeatMap normal, TGeneratorSettings settings = null) where TGenerator : IBeatMapGenerator<TGeneratorSettings>, new() where TGeneratorSettings : class, new()
        {
            return new TGenerator().FromNormal(normal, settings ?? new TGeneratorSettings());
        }

        public static bool Generate360ModeAndSave(BeatMapInfo info, BeatMapDifficultyLevel difficulty, bool replaceExising360Mode = false)
        {
            info.CreateBackup();

            //BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = BeatMapDifficulty.Create(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!info.AddGameModeDifficulty(newDiff, "360Degree", replaceExising360Mode))
                return false;

            BeatMap360GeneratorSettings settings = new BeatMap360GeneratorSettings()
            {
                timeOffset = info.songTimeOffset
            };

            newDiff.SaveBeatMap(info.mapDirectoryPath, UseGenerator<BeatMap360Generator, BeatMap360GeneratorSettings>(standardDiff.LoadBeatMap(info.mapDirectoryPath), settings));

            info.AddContributor("CodeStix's 360fyer", "360 degree mode");
            info.SaveToFile(info.mapInfoPath);
            return true;
        }

        public static bool Generate360ModeAndCopy(BeatMapInfo info, string destination, BeatMapDifficultyLevel difficulty)
        {
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = BeatMapDifficulty.Create(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!info.AddGameModeDifficulty(newDiff, "360Degree", true)) // always replace when making a copy
                return false;

            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);
            Directory.CreateDirectory(mapDestination);

            BeatMap360GeneratorSettings settings = new BeatMap360GeneratorSettings()
            {
                timeOffset = info.songTimeOffset
            };

            newDiff.SaveBeatMap(mapDestination, UseGenerator<BeatMap360Generator, BeatMap360GeneratorSettings>(standardDiff.LoadBeatMap(info.mapDirectoryPath), settings));

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
