﻿using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public static class Jobs
    {
        #region Finding songs

        public struct FindSongsJobResult
        {
            public List<BeatMapInfo> beatMaps;
            public HashSet<string> beatMapDifficulties;
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
            job.result.beatMapDifficulties = new HashSet<string>();
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

                    difStandardSet.difficultyBeatmaps.ForEach((diff) => job.result.beatMapDifficulties.Add(diff.difficulty));
                    job.result.beatMaps.Add(info);
                }
            }
            catch (Exception ex)
            {
                job.exceptions.Add(ex);
            }
        }

        #endregion // Finding songs

        public struct Generate360ModesOptions
        {
            public List<BeatMapDifficultyLevel> difficultyLevels;
            public List<BeatMapInfo> toGenerateFor;
            public bool replacePreviousModes;
        }

        public struct Generate360ModesResult
        {
            public int modesGenerated;
            public int mapsChanged;
        }

        public static void Generate360Maps(Generate360ModesOptions options, WorkerJobCompleted<Generate360ModesOptions, Generate360ModesResult> completed)
        {
            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ShowCancelButton = true;
            progressDialog.WindowTitle = "Converting maps...";
            progressDialog.UseCompactPathsForDescription = true;
            progressDialog.DoWork += Generate360Maps_DoWork;
            progressDialog.RunWorkerCompleted += (sender, e) => completed.Invoke((WorkerJob<Generate360ModesOptions, Generate360ModesResult>)e.Result);
            progressDialog.ShowDialog(null, new WorkerJob<Generate360ModesOptions, Generate360ModesResult>(progressDialog, options));
        }

        private static void Generate360Maps_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerJob<Generate360ModesOptions, Generate360ModesResult> job = (WorkerJob<Generate360ModesOptions, Generate360ModesResult>)e.Argument;
            e.Result = job;

            try
            {
                for (int i = 0; i < job.argument.toGenerateFor.Count; i++)
                {
                    if (job.employer.CancellationPending)
                        throw new Exception("The generation process was cancelled.");

                    BeatMapInfo info = job.argument.toGenerateFor[i];

                    int first = job.result.modesGenerated;
                    foreach (BeatMapDifficultyLevel difficultyLevel in job.argument.difficultyLevels)
                    {
                        try
                        {
                            if (ModeGenerator.Add360ModeTo(info, difficultyLevel, job.argument.replacePreviousModes))
                                job.result.modesGenerated++;
                        }
                        catch(Exception ex)
                        {
                            job.exceptions.Add(ex);
                        }
                    }
                    if (first != job.result.modesGenerated)
                        job.result.mapsChanged++;

                    job.Report((int)((float)i / (float)job.argument.toGenerateFor.Count * 100f), info.ToString(), info.mapDirectoryPath);
                }
            }
            catch(Exception ex)
            {
                job.exceptions.Add(ex);
            }
        }
    }
}