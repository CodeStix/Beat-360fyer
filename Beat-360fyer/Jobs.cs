using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stx.ThreeSixtyfyer
{
    public static class Jobs
    {
        public static ParallelOptions MultithreadingOptions => new ParallelOptions() 
        { 
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        #region Finding songs

        public struct FindSongsJobResult
        {
            public List<BeatMapInfo> beatMaps;
        }

        public static void FindSongsUnderPath(string path, WorkerJobCompleted<string, FindSongsJobResult> completed)
        {
            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar;
            progressDialog.ShowCancelButton = true;
            progressDialog.Text = "Finding Beat Saber maps...";
            progressDialog.UseCompactPathsForDescription = true;
            progressDialog.Description = path;
            progressDialog.DoWork += FindSongsUnderPath_DoWork;
            progressDialog.RunWorkerCompleted += (sender, e) => completed.Invoke((WorkerJob<string, FindSongsJobResult>)e.Result);
            progressDialog.ShowDialog(null, new WorkerJob<string, FindSongsJobResult>(progressDialog, path));
        }

        private static void FindSongsUnderPath_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerJob<string, FindSongsJobResult> job = (WorkerJob<string, FindSongsJobResult>)e.Argument;
            job.result.beatMaps = new List<BeatMapInfo>();
            e.Result = job;

            try
            {
                foreach (string infoFile in FileEnumerator.GetFilesRecursive(job.argument, "?nfo.dat"))
                {
                    if (job.employer.CancellationPending)
                        throw new Exception("The searching process was cancelled.");
                    if (infoFile == null)
                        continue;

                    BeatMapInfo info = BeatMapInfo.FromFile(infoFile);

                    BeatMapDifficultySet difStandardSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "Standard");
                    if (difStandardSet == null)
                        continue; // Cannot convert if a normal version does not exist

                    job.result.beatMaps.Add(info);
                }
            }
            catch (Exception ex)
            {
                job.exceptions.Add(ex);
            }
        }

        #endregion // Finding songs

        #region Generating modes

        public struct GenerateMapsOptions
        {
            public HashSet<BeatMapDifficultyLevel> difficultyLevels;
            public List<BeatMapInfo> toGenerateFor;
            public string destination;
            public bool forceGenerate;
            public IBeatMapGenerator generator;
        }

        public struct GeneratorMapsResult
        {
            public int mapsGenerated;
            public int mapsIterated;
            public int difficultiesGenerated;
            public int mapsUpToDate;
            public bool cancelled;
        }

        public static void GenerateMaps(GenerateMapsOptions options, WorkerJobCompleted<GenerateMapsOptions, GeneratorMapsResult> completed)
        {
            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ShowCancelButton = true;
            progressDialog.WindowTitle = $"Generating modes using the {options.generator.GeneratedGameModeName}-generator...";
            progressDialog.UseCompactPathsForDescription = true;
            progressDialog.DoWork += Generate360Maps_DoWork;
            progressDialog.ShowTimeRemaining = true;
            progressDialog.RunWorkerCompleted += (sender, e) => completed.Invoke((WorkerJob<GenerateMapsOptions, GeneratorMapsResult>)e.Result);
            progressDialog.ShowDialog(null, new WorkerJob<GenerateMapsOptions, GeneratorMapsResult>(progressDialog, options));
        }

        private static void Generate360Maps_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerJob<GenerateMapsOptions, GeneratorMapsResult> job = (WorkerJob<GenerateMapsOptions, GeneratorMapsResult>)e.Argument;
            e.Result = job;

            Parallel.For(0, job.argument.toGenerateFor.Count, MultithreadingOptions, (i) => {

                if (job.employer.CancellationPending && !job.result.cancelled)
                    job.result.cancelled = true;
                if (job.result.cancelled)
                    return;

                BeatMapInfo info;
                lock (job.argument.toGenerateFor)
                    info = job.argument.toGenerateFor[i];

                BeatMapGenerator.Result r;
                if (string.IsNullOrEmpty(job.argument.destination))
                    r = BeatMapGenerator.UseGeneratorAndOverwrite(job.argument.generator, info, job.argument.difficultyLevels, job.argument.forceGenerate);
                else
                    r = BeatMapGenerator.UseGeneratorAndCopy(job.argument.generator, info, job.argument.difficultyLevels, job.argument.destination, job.argument.forceGenerate);

                job.result.difficultiesGenerated += r.generatedCount;
                if (r.generatedCount != 0)
                    job.result.mapsGenerated++;
                if (r.alreadyUpToDate)
                    job.result.mapsUpToDate++;
                job.result.mapsIterated++;
                job.Report((int)((float)job.result.mapsIterated / job.argument.toGenerateFor.Count * 100f), info.ToString(), info.mapDirectoryPath);
            });

            if (job.result.cancelled)
                job.exceptions.Add(new Exception("The generation process was cancelled."));
        }

        #endregion
    }
}
