using Newtonsoft.Json;
using Stx.ThreeSixtyfyer.Generators;
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
        public const string DEFAULT_GENERATOR = "CodeStix's 360fyer";
        public const string GENERATOR_CONFIG_NAME = "Generator.dat";

        private static IEnumerable<Type> generatorTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IBeatMapGenerator).IsAssignableFrom(p) && !p.IsInterface);

        public static IBeatMapGenerator GetGeneratorWithName(string name)
        {
            foreach(Type t in generatorTypes)
            {
                IBeatMapGenerator generator = (IBeatMapGenerator)Activator.CreateInstance(t);
                if (string.Compare(name, generator.Name) == 0)
                    return generator;
            }

            return null;
        }

        public static bool UseGeneratorAndOverwrite(IBeatMapGenerator generator, BeatMapInfo info, IReadOnlyCollection<BeatMapDifficultyLevel> difficultyLevels, bool forceGenerate = false)
        {
            HashSet<BeatMapDifficultyLevel> difficulties = new HashSet<BeatMapDifficultyLevel>(difficultyLevels);
            info.CreateBackup();

            bool saveNewInfo = true;
            string generatorConfigFile = Path.Combine(info.mapDirectoryPath, GENERATOR_CONFIG_NAME);
            if (File.Exists(generatorConfigFile))
            {
                BeatMapGeneratorConfig generatorConfig = BeatMapGeneratorConfig.FromFile(generatorConfigFile);

                saveNewInfo = generatorConfig.ShouldSaveInfo(difficulties);
                if (!saveNewInfo && !forceGenerate && !generatorConfig.ShouldRegenerate(generator.Settings, generator.Version))
                    return true; // Already up to date!

                difficulties.AddRange(generatorConfig.difficulties);
            }

            int ok = 0;
            foreach(BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, generator.GeneratedGameModeName);
                if (!info.AddGameModeDifficulty(newDiff, generator.GeneratedGameModeName, true))
                    continue;

                newDiff.SaveBeatMap(info.mapDirectoryPath, generator.FromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.beatsPerMinute, info.songTimeOffset));
                ok++;
            }
            if (ok == 0)
                return false;

            if (saveNewInfo)
            {
                info.AddContributor(generator.Name, generator.GeneratedGameModeName, ContributorImagePath);
                info.SaveToFile(info.mapInfoPath);
            }

            BeatMapGeneratorConfig.FromGenerator(generator, difficulties).SaveToFile(Path.Combine(info.mapDirectoryPath, GENERATOR_CONFIG_NAME));
            return true;
        }

        public static bool UseGeneratorAndCopy(IBeatMapGenerator generator, BeatMapInfo info, IReadOnlyCollection<BeatMapDifficultyLevel> difficultyLevels, string destination, bool forceGenerate = false)
        {
            HashSet<BeatMapDifficultyLevel> difficulties = new HashSet<BeatMapDifficultyLevel>(difficultyLevels);
            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);

            bool saveNewInfo = true;
            string generatorConfigFile = Path.Combine(mapDestination, GENERATOR_CONFIG_NAME);
            if (File.Exists(generatorConfigFile))
            {
                BeatMapGeneratorConfig generatorConfig = BeatMapGeneratorConfig.FromFile(generatorConfigFile);

                saveNewInfo = generatorConfig.ShouldSaveInfo(difficulties);
                if (!saveNewInfo && !forceGenerate && !generatorConfig.ShouldRegenerate(generator.Settings, generator.Version))
                    return true; // Already up to date!

                difficulties.AddRange(generatorConfig.difficulties);
            }

            Directory.CreateDirectory(mapDestination);

            int ok = 0;
            foreach (BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, generator.GeneratedGameModeName);
                if (!info.AddGameModeDifficulty(newDiff, generator.GeneratedGameModeName, true)) // always replace when making a copy
                    continue;

                newDiff.SaveBeatMap(mapDestination, generator.FromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.beatsPerMinute, info.songTimeOffset));
                ok++;
            }
            if (ok == 0)
                return false;

            info.difficultyBeatmapSets.RemoveAll((diffSet) => diffSet.beatmapCharacteristicName != generator.GeneratedGameModeName);

            if (saveNewInfo)
            {
                string coverImagePath = Path.Combine(info.mapDirectoryPath, info.coverImageFilename); // There are some songs without cover images
                if (File.Exists(coverImagePath))
                    File.Copy(coverImagePath, Path.Combine(mapDestination, info.coverImageFilename), true);
                File.Copy(Path.Combine(info.mapDirectoryPath, info.songFilename), Path.Combine(mapDestination, info.songFilename), true);
                info.AddContributor(generator.Name, generator.GeneratedGameModeName, ContributorImagePath);
                info.SaveToFile(Path.Combine(mapDestination, "Info.dat"));
            }

            BeatMapGeneratorConfig.FromGenerator(generator, difficulties).SaveToFile(Path.Combine(mapDestination, GENERATOR_CONFIG_NAME));
            return true;
        }





        /*[Obsolete]
        public static bool UpdateGenerated360Modes(string existingModeMapLocation)
        {
            string generatorConfigFile = Path.Combine(existingModeMapLocation, "Generator.dat");
            if (!File.Exists(generatorConfigFile))
                return false;

            BeatMapGeneratorConfig generatorConfig = BeatMapGeneratorConfig.FromFile(generatorConfigFile);
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

                difficulty.SaveBeatMap(existingModeMapLocation, generator.FromStandard(normalDifficulty.LoadBeatMap(generatorConfig.originalMapLocation), originalMapInfo.beatsPerMinute, originalMapInfo.songTimeOffset));
            }

            generatorConfig.version = generator.Version;
            generatorConfig.SaveToFile(generatorConfigFile);
            return true;
        }*/
    }
}
