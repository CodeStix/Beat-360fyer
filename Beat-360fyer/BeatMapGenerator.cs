﻿using Newtonsoft.Json;
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
        public static string ContributorImagePath { get; set; } = null;

        public static bool Generate360ModeAndOverwrite(BeatMapInfo info, BeatMapDifficultyLevel[] difficulties, bool replaceExising360Mode = false)
        {
            info.CreateBackup();

            BeatMap360GeneratorSettings settings = new BeatMap360GeneratorSettings(info.beatsPerMinute, info.songTimeOffset) {/* default generator settings */};
            string generatorConfigFile = Path.Combine(info.mapDirectoryPath, "Generator.dat");
            if (File.Exists(generatorConfigFile))
            {
                BeatMap360GeneratorConfig config = BeatMap360GeneratorConfig.FromFile(generatorConfigFile);
                settings = config.settings;
            }

            BeatMap360Generator generator = new BeatMap360Generator()
            {
                Settings = settings
            };

            int ok = 0;
            foreach(BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, "360Degree");
                if (!info.AddGameModeDifficulty(newDiff, "360Degree", replaceExising360Mode))
                    continue;

                newDiff.SaveBeatMap(info.mapDirectoryPath, generator.FromNormal(standardDiff.LoadBeatMap(info.mapDirectoryPath)));
                ok++;
            }
            if (ok == 0)
                return false;

            info.AddContributor("CodeStix's 360fyer", "360 degree mode", ContributorImagePath);
            info.SaveToFile(info.mapInfoPath);

            BeatMap360GeneratorConfig.FromGenerator(generator, info.mapDirectoryPath, difficulties).SaveToFile(Path.Combine(info.mapDirectoryPath, "Generator.dat"));
            return true;
        }

        public static bool Generate360ModeAndCopy(BeatMapInfo info, string destination, BeatMapDifficultyLevel[] difficulties, BeatMap360GeneratorSettings generatorSettings = null)
        {
            BeatMap360GeneratorSettings settings = generatorSettings ?? new BeatMap360GeneratorSettings(info.beatsPerMinute, info.songTimeOffset) {/* default generator settings */};
            BeatMap360Generator generator = new BeatMap360Generator()
            {
                Settings = settings
            };

            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);
            Directory.CreateDirectory(mapDestination);

            int ok = 0;
            foreach (BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, "360Degree");
                if (!info.AddGameModeDifficulty(newDiff, "360Degree", true)) // always replace when making a copy
                    continue;

                newDiff.SaveBeatMap(mapDestination, generator.FromNormal(standardDiff.LoadBeatMap(info.mapDirectoryPath)));
                ok++;
            }
            if (ok == 0)
                return false;

            info.difficultyBeatmapSets.RemoveAll((diffSet) => diffSet.beatmapCharacteristicName != "360Degree");

            string coverImagePath = Path.Combine(info.mapDirectoryPath, info.coverImageFilename); // There are some songs without cover images
            if (File.Exists(coverImagePath))
                File.Copy(coverImagePath, Path.Combine(mapDestination, info.coverImageFilename), true);
            File.Copy(Path.Combine(info.mapDirectoryPath, info.songFilename), Path.Combine(mapDestination, info.songFilename), true);
            info.AddContributor("CodeStix's 360fyer", "360 degree mode", ContributorImagePath);
            info.SaveToFile(Path.Combine(mapDestination, "Info.dat"));

            BeatMap360GeneratorConfig.FromGenerator(generator, info.mapDirectoryPath, difficulties).SaveToFile(Path.Combine(mapDestination, "Generator.dat"));
            return true;
        }

        public static bool EnsureUpdated360Mode(string existingModeMapLocation)
        {
            string generatorConfigFile = Path.Combine(existingModeMapLocation, "Generator.dat");
            if (!File.Exists(generatorConfigFile))
                return false;

            BeatMap360GeneratorConfig generatorConfig = BeatMap360GeneratorConfig.FromFile(generatorConfigFile);
            BeatMap360Generator generator = new BeatMap360Generator()
            {
                Settings = generatorConfig.settings
            };

            if (generatorConfig.version >= generator.Version)
                return true;

            if (!Directory.Exists(generatorConfig.originalMapLocation))
            {
                File.Delete(generatorConfigFile);
                return false;
            }

            string[] modeInfoFiles = Directory.GetFiles(existingModeMapLocation, "?nfo.dat");
            if (modeInfoFiles.Length == 0)
                return false;
            BeatMapInfo modeMapInfo = BeatMapInfo.FromFile(modeInfoFiles[0]);

            string[] standardInfoFiles = Directory.GetFiles(generatorConfig.originalMapLocation, "?nfo.dat");
            if (standardInfoFiles.Length == 0)
                return false;
            BeatMapInfo originalMapInfo = BeatMapInfo.FromFile(standardInfoFiles[0]);

            foreach (BeatMapDifficultyLevel diff in generatorConfig.difficulties)
            {
                BeatMapDifficulty difficulty = modeMapInfo.GetGameModeDifficulty(diff, "360Degree");
                BeatMapDifficulty normalDifficulty = originalMapInfo.GetGameModeDifficulty(diff, "Standard");

                if (difficulty == null || normalDifficulty == null)
                    continue;

                difficulty.SaveBeatMap(existingModeMapLocation, generator.FromNormal(normalDifficulty.LoadBeatMap(generatorConfig.originalMapLocation)));
            }

            generatorConfig.version = generator.Version;
            generatorConfig.SaveToFile(generatorConfigFile);
            return true;
        }
    }
}
