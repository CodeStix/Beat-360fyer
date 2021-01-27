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
        public const string GENERATOR_CONFIG_NAME = "Generator.dat";

        public static IBeatMapGenerator DefaultGenerator => new BeatMap360Generator();

        public static IEnumerable<Type> GeneratorTypes => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IBeatMapGenerator).IsAssignableFrom(p) && !p.IsInterface);

        public static BeatMapGeneratorAttribute GetGeneratorInfo(Type generatorType)
        {
            var l = generatorType.GetCustomAttributes(typeof(BeatMapGeneratorAttribute), false);
            if (l.Length == 0)
                return null;
            return (BeatMapGeneratorAttribute)l[0];
        }

        public static IBeatMapGenerator GetGeneratorWithName(string name)
        {
            foreach(Type t in GeneratorTypes)
            {
                var l = t.GetCustomAttributes(typeof(BeatMapGeneratorAttribute), false);
                if (l.Length == 0)
                    continue;
                if (((BeatMapGeneratorAttribute)l[0]).Name == name)
                    return (IBeatMapGenerator)Activator.CreateInstance(t);
            }

            return null;
        }

        public struct Result
        {
            public byte generatedCount;
            public bool alreadyUpToDate;
        }

        public static Result UseGeneratorAndOverwrite(IBeatMapGenerator generator, BeatMapInfo info, IReadOnlyCollection<BeatMapDifficultyLevel> difficultyLevels, bool forceGenerate = false)
        {
            info = info.Clone();
            var generatorInfo = generator.GetInformation();
            Result result = new Result();
            HashSet<BeatMapDifficultyLevel> difficulties = new HashSet<BeatMapDifficultyLevel>(difficultyLevels);
            info.CreateBackup();

            bool saveNewInfo = true;
            string generatorConfigFile = Path.Combine(info.mapDirectoryPath, GENERATOR_CONFIG_NAME);
            if (File.Exists(generatorConfigFile))
            {
                BeatMapGeneratorConfig generatorConfig = BeatMapGeneratorConfig.FromFile(generatorConfigFile);

                saveNewInfo = generatorConfig.ShouldSaveInfo(difficulties);
                if (!saveNewInfo && !forceGenerate && !generatorConfig.ShouldRegenerate(generator.Settings, generatorInfo.Name, generatorInfo.Version))
                {
                    result.alreadyUpToDate = true;
                    return result; // Already up to date!
                }

                difficulties.AddRange(generatorConfig.difficulties);
            }

            foreach(BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, generator.GeneratedGameModeName);
                if (!info.AddGameModeDifficulty(newDiff, generator.GeneratedGameModeName, true))
                    continue;

                newDiff.SaveBeatMap(info.mapDirectoryPath, generator.FromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.beatsPerMinute, info.songTimeOffset));
                result.generatedCount++;
            }
            if (result.generatedCount == 0)
                return result;

            if (saveNewInfo)
            {
                info.AddContributor(generatorInfo.Name, generator.GeneratedGameModeName, ContributorImagePath);
                info.SaveToFile(info.mapInfoPath);
            }

            BeatMapGeneratorConfig.FromGenerator(generator, difficulties).SaveToFile(Path.Combine(info.mapDirectoryPath, GENERATOR_CONFIG_NAME));
            return result;
        }

        public static Result UseGeneratorAndCopy(IBeatMapGenerator generator, BeatMapInfo info, IReadOnlyCollection<BeatMapDifficultyLevel> difficultyLevels, string destination, bool forceGenerate = false)
        {
            info = info.Clone();
            var generatorInfo = generator.GetInformation();
            Result result = new Result();
            HashSet<BeatMapDifficultyLevel> difficulties = new HashSet<BeatMapDifficultyLevel>(difficultyLevels);
            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);

            bool saveNewInfo = true;
            string generatorConfigFile = Path.Combine(mapDestination, GENERATOR_CONFIG_NAME);
            if (File.Exists(generatorConfigFile))
            {
                BeatMapGeneratorConfig generatorConfig = BeatMapGeneratorConfig.FromFile(generatorConfigFile);

                saveNewInfo = generatorConfig.ShouldSaveInfo(difficulties);
                if (!saveNewInfo && !forceGenerate && !generatorConfig.ShouldRegenerate(generator.Settings, generatorInfo.Name, generatorInfo.Version))
                {
                    result.alreadyUpToDate = true;
                    return result; // Already up to date!
                }

                difficulties.AddRange(generatorConfig.difficulties);
            }

            Directory.CreateDirectory(mapDestination);

            foreach (BeatMapDifficultyLevel difficulty in difficulties)
            {
                BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");
                if (standardDiff == null)
                    continue;

                BeatMapDifficulty newDiff = BeatMapDifficulty.CopyFrom(standardDiff, generator.GeneratedGameModeName);
                if (!info.AddGameModeDifficulty(newDiff, generator.GeneratedGameModeName, true)) // always replace when making a copy
                    continue;

                newDiff.SaveBeatMap(mapDestination, generator.FromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.beatsPerMinute, info.songTimeOffset));
                result.generatedCount++;
            }
            if (result.generatedCount == 0)
                return result;

            info.difficultyBeatmapSets.RemoveAll((diffSet) => diffSet.beatmapCharacteristicName != generator.GeneratedGameModeName);

            if (saveNewInfo)
            {
                string coverImagePath = Path.Combine(info.mapDirectoryPath, info.coverImageFilename); // There are some songs without cover images
                if (File.Exists(coverImagePath))
                    File.Copy(coverImagePath, Path.Combine(mapDestination, info.coverImageFilename), true);
                File.Copy(Path.Combine(info.mapDirectoryPath, info.songFilename), Path.Combine(mapDestination, info.songFilename), true);
                info.AddContributor(generatorInfo.Name, generator.GeneratedGameModeName, ContributorImagePath);
                info.SaveToFile(Path.Combine(mapDestination, "Info.dat"));
            }

            BeatMapGeneratorConfig.FromGenerator(generator, difficulties).SaveToFile(Path.Combine(mapDestination, GENERATOR_CONFIG_NAME));
            return result;
        }

        public static bool IsDefaultSettings(object settings)
        {
            if (settings == null)
                return false;

            return Activator.CreateInstance(settings.GetType()).Equals(settings);
        }
    }

    public static class BeatMapGeneratorExtensions
    {
        public static BeatMapGeneratorAttribute GetInformation(this IBeatMapGenerator generator)
        {
            return BeatMapGenerator.GetGeneratorInfo(generator.GetType());
        }
    }
}
