using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.UltraDimension
{
    public enum BeatMapDifficultyLevel
    {
        Easy = 1,
        Normal = 3,
        Hard = 5,
        Expert = 7,
        ExpertPlus = 9
    }

    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private List<BeatMapInfo> beatMaps = new List<BeatMapInfo>();
        private HashSet<string> beatMapDifficulties = new HashSet<string>();
        private string selectedPath = string.Empty;

        private void SetUI(bool enabled)
        {
            listBoxMaps.Enabled = enabled;
            textBoxSearch.Enabled = enabled;
            groupBoxDifficulties.Enabled = enabled;
            buttonSelectAll.Visible = enabled;
            buttonSelectNone.Visible = enabled;
            labelInfo.Visible = enabled;
            buttonOpenMap.Enabled = enabled;
        }

        private void ButtonOpenMap_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select your BeatSaber_Data/CustomLevels directory or select a single custom level, " +
                "it will look for *.dat files, so either will work.\n" +
                "The selected path will be remembered.";
            folderBrowser.SelectedPath = Properties.Settings.Default.RememberPath;//Directory.GetCurrentDirectory();

            if (folderBrowser.ShowDialog() != DialogResult.OK)
                return;

            selectedPath = folderBrowser.SelectedPath;
            Properties.Settings.Default.RememberPath = selectedPath;
            Properties.Settings.Default.Save();

            textBoxMapPath.Text = selectedPath;
            listBoxMaps.Items.Clear();

            FindSongs();
        }

        private void FindSongs()
        {
            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle.MarqueeProgressBar;
            progressDialog.ShowCancelButton = false;
            progressDialog.Text = "Finding Beat Saber maps...";
            progressDialog.DoWork += ProgressMaps_DoWork;
            progressDialog.RunWorkerCompleted += (sender2, e2) =>
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    listBoxMaps.Items.AddRange(beatMaps.ToArray());
                    SelectAll(true);

                    comboBoxDifficulty.Items.Clear();
                    comboBoxDifficulty.Items.AddRange(beatMapDifficulties.ToArray());

                    if (listBoxMaps.Items.Count > 0 && comboBoxDifficulty.Items.Count > 0)
                    {
                        comboBoxDifficulty.SelectedIndex = 0;
                        SetUI(true);
                    }
                    else
                    {
                        SetUI(false);

                        MessageBox.Show("No compatible maps where found, please select the Beat Saber CustomLevels directory.",
                            "No maps found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }));
            };
            progressDialog.ShowDialog();
        }

        private void ProgressMaps_DoWork(object sender, DoWorkEventArgs e)
        {
            beatMaps.Clear();
            beatMapDifficulties.Clear();

            foreach (BeatMapInfo info in GetAllMapInfos(textBoxMapPath.Text))
            {
                BeatMapDifficultySet difStandardSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "Standard");
                if (difStandardSet == null)
                    continue; // Cannot convert if a normal version does not exist

                difStandardSet.difficultyBeatmaps.ForEach((diff) => beatMapDifficulties.Add(diff.difficulty));

                beatMaps.Add(info);
            }
        }

        private void ButtonConvert_Click(object sender, EventArgs e)
        {
            if (listBoxMaps.CheckedItems.Count == 0)
            {
                MessageBox.Show("You have nothing selected!\n" +
                    "First, select the songs you want to add the 360 mode to, then click the convert button.",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetUI(false);
            int added = 0;

            string difficulty = comboBoxDifficulty.SelectedItem.ToString();

            Log($"Adding 360 mode to all selected maps for difficulty '{difficulty}'");

            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ShowCancelButton = false;
            progressDialog.WindowTitle = "Converting maps...";
            progressDialog.UseCompactPathsForDescription = true;
            progressDialog.DoWork += (sender2, e2) =>
            {
                int converted = 0;
                foreach (BeatMapInfo info in listBoxMaps.CheckedItems)
                {
                    Log($"\tAdding mode to '{info}'");
                    if (TryConvert(info.mapRoot, difficulty, checkBoxReplace.Checked))
                        added++;

                    int progress = Math.Min((int)((float)++converted / listBoxMaps.CheckedItems.Count * 100f), 100);
                    progressDialog.ReportProgress(progress, info.ToString(), info.mapRoot);
                }
            };
            progressDialog.RunWorkerCompleted += (sender2, e2) =>
            {
                if (added > 0)
                    MessageBox.Show($"{added} 360 maps were added to {listBoxMaps.CheckedItems.Count} different songs. Just navigate to the map in the game and the 360 mode should appear.",
                        "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"No modes were added, this can be due to:\n" +
                        $" - The selected song does not have the standard mode, this is required for the conversion.\n" +
                        $" - The selected song already has the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n" +
                        $" - An unkown file system error.\n", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            };
            progressDialog.ShowDialog();
        }

        public void Log(string str)
        {
            Console.WriteLine("[LOGGED] " + str);
        }

        private bool TryConvert(string mapRoot, string difficulty, bool? forceReplace = null)
        {
            BeatMapDifficultyLevel diff = (BeatMapDifficultyLevel)Enum.Parse(typeof(BeatMapDifficultyLevel), difficulty);

            try
            {
                Add360Mode(mapRoot, diff, forceReplace.Value);
                return true;
            }
            catch (OverflowException)
            {
                Log($"\tWarning: the map already has a 360 mode for '{difficulty}'");
                if (forceReplace == null && MessageBox.Show($"The difficulty '{diff}' already has a 360 mode, do you want to overwrite it?", "Duplicate", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    return TryConvert(mapRoot, difficulty, true);

                return false;
            }
            catch (Exception ex)
            {
                Log($"\tCannot add 360 mode: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<BeatMapInfo> GetAllMapInfos(string mapRoot)
        {
            string[] datFiles = Directory.GetFiles(mapRoot, "*.dat", SearchOption.AllDirectories);

            foreach (string datFile in datFiles)
            {
                if (new FileInfo(datFile).Name.ToLower() == "info.dat")
                    yield return BeatMapInfo.FromFile(datFile);
            }
        }

        public BeatMapInfo GetMapInfo(string mapRoot, out string infoFile)
        {
            string[] datFiles = Directory.GetFiles(mapRoot, "*.dat");

            infoFile = datFiles.First((file) => new FileInfo(file).Name.ToLower() == "info.dat");

            return BeatMapInfo.FromFile(infoFile);
        }

        public void Add360Mode(string mapRoot, BeatMapDifficultyLevel difficulty, bool replacePrevious = false)
        {
            Console.WriteLine("Getting information...");

            BeatMapInfo info = GetMapInfo(mapRoot, out string infoFile);
            BeatMapDifficultySet difStandardSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "Standard");
            BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());
            BeatMapDifficultySet dif360Set = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "360Degree");

            Console.WriteLine("Checking difficulties...");

            if (dif360Set == null)
            {
                dif360Set = new BeatMapDifficultySet()
                {
                    beatmapCharacteristicName = "360Degree",
                    difficultyBeatmaps = new List<BeatMapDifficulty>()
                };
                info.difficultyBeatmapSets.Add(dif360Set);
            }

            BeatMapDifficulty dif360 = dif360Set.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());

            if (dif360 != null)
            {
                if (!replacePrevious)
                    throw new OverflowException("The level alreay consists of a 360 mode.");

                dif360Set.difficultyBeatmaps.Remove(dif360);
            }
            if (difStandard == null)
            {
                throw new DataException("The normal version must be available to convert it into a 360 mode.");
            }

            Console.WriteLine("Creating new difficulty...");

            BeatMapDifficulty newDif = new BeatMapDifficulty()
            {
                difficulty = difficulty.ToString(),
                difficultyRank = (int)difficulty,
                beatmapFilename = "360Degree" + difficulty.ToString() + ".dat",
                noteJumpMovementSpeed = 0.0f,
                noteJumpStartBeatOffset = 0.0f
            };

            dif360Set.difficultyBeatmaps.Add(newDif);
            dif360Set.difficultyBeatmaps = dif360Set.difficultyBeatmaps.OrderBy((diff) => diff.difficultyRank).ToList();

            newDif.SaveBeatMap(mapRoot, Generate360MapFromStandard(difStandard.LoadBeatMap(mapRoot), info.songTimeOffset));

            Console.WriteLine("Saving...");

            info.SaveToFile(infoFile);

            Console.WriteLine("\t-> Done");
        }

        public BeatMap Generate360MapFromStandard(BeatMap standardMap, float timeOffset = 0f)
        {
            BeatMap map = new BeatMap(standardMap);

            if (map.notes.Count == 0)
                return map;

            const float FRAME_LENGTH = 1f / 8f; // in beats
            float minTime = timeOffset;
            float firstNoteTime = map.notes.First().time;
            float maxTime = map.notes.Last().time;

            List<BeatMapNote> GetNotes(float time, float futureTime) // <- look in the future for notes coming
            {
                return map.notes.Where((note) => note.time >= time && note.time < time + futureTime).ToList();
            }

            int spinsRemaining = 0;
            bool spinDirection = true;

            int noLeftRightStreak = 0;

            int prevBeat = 0;
            for (float time = minTime; time < maxTime; time += FRAME_LENGTH)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, FRAME_LENGTH);
                List<BeatMapNote> notesInBeat = GetNotes(time, 1f);
                List<BeatMapObstacle> activeObstacles = map.obstacles.Where((obst) =>  time > obst.time && time < obst.time + obst.duration).ToList();
                bool enableGoLeft = !activeObstacles.Any((obst) => (obst.lineIndex == 0 || obst.lineIndex == 1) && obst.type == 0);
                bool enableGoRight = !activeObstacles.Any((obst) => (obst.lineIndex == 2 || obst.lineIndex == 3) && obst.type == 0);

                if (spinsRemaining > 0)
                {
                    spinsRemaining--;
                    if (spinDirection)
                        map.AddGoLeftEvent(time, 1);
                    else
                        map.AddGoRightEvent(time, 1);

                    if (spinsRemaining == 1 && GetNotes(time, 7).Count == 0) // still no notes, spin more
                        spinsRemaining += 24;

                    continue;
                }
                else if (GetNotes(time, 8).Count == 0 && activeObstacles.Count == 0 && time > firstNoteTime) // if 0 notes in the following 32 frames (8 beats), spin effect
                {
                    spinsRemaining += 24; // 24 spins is one 360
                    spinDirection = !spinDirection;
                }

                if (notesInFrame.Count == 0)
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || note.cutDirection == 6 || note.cutDirection == 4) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, 4));
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || note.cutDirection == 5 || note.cutDirection == 7) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, 4));
                    continue;
                }

                BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 5) && (note.type == 0 || note.type == 1));
                if (leftRightNotes != null)
                {
                    if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                    {
                        map.AddGoLeftEvent(leftRightNotes.time, 1);
                        continue;
                    }
                    else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5) && enableGoRight)
                    {
                        map.AddGoRightEvent(leftRightNotes.time, 1);
                        continue;
                    }

                    /*if (noLeftRightStreak >= 8 / FRAME_LENGTH) // if streak was high and is now broken, add epic walls
                    {
                        Console.WriteLine("epicwall");

                        map.AddGoLeftEvent(time - FRAME_LENGTH, 1);
                        map.AddWall(time, 0);
                        map.AddGoRightEvent(time, 2);
                        map.AddWall(time, 3);
                        map.AddGoLeftEvent(time + FRAME_LENGTH, 1);
                    }*/

                    noLeftRightStreak = 0;
                }
                else
                {
                    noLeftRightStreak++;
                }

                if (notesInBeat.Count > 0)
                {
                    if (notesInBeat.All((note) => Math.Abs(note.time - notesInBeat[0].time) < FRAME_LENGTH))
                    {
                        BeatMapNote[] groundLeftNotes = notesInFrame.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                        BeatMapNote[] groundRightNotes = notesInFrame.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                        if (!(groundLeftNotes.Length == groundRightNotes.Length && enableGoLeft && enableGoRight))
                        {
                            if (groundLeftNotes.Length > 0 && enableGoLeft)
                            {
                                map.AddGoLeftEvent(groundLeftNotes[0].time, 1);
                                continue;
                            }

                            if (groundRightNotes.Length > 0 && enableGoRight)
                            {
                                map.AddGoRightEvent(groundRightNotes[0].time, 1);
                                continue;
                            }
                        }
                    }
                }

                // This only activates one time per beat, not per frame
                if (prevBeat < time)
                {
                    prevBeat = (int)(time + 1f);

                    // Add movement for all direction notes, only if they are the only one in the beat
                    if (notesInBeat.All((note) => note.cutDirection == 8 && (note.type == 0 || note.type == 1)))
                    {
                        int rightNoteCount = notesInBeat.Count((note) => (note.lineIndex == 2 || note.lineIndex == 3) && (note.type == 0 || note.type == 1));
                        int leftNoteCount = notesInBeat.Count((note) => (note.lineIndex == 0 || note.lineIndex == 1) && (note.type == 0 || note.type == 1));

                        if (rightNoteCount > leftNoteCount && enableGoRight)
                        {
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount > rightNoteCount && enableGoLeft)
                        {
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                    }

                    // bombs seizure
                    if (GetNotes(time, 4f).All((note) => note.type == 3))
                    {
                        map.AddGoLeftEvent(time - FRAME_LENGTH, 2);
                        map.AddGoRightEvent(time, 4);
                        map.AddGoLeftEvent(time + FRAME_LENGTH, 4);
                        map.AddGoRightEvent(time + 2f * FRAME_LENGTH, 2);
                        continue;
                    }
                }
            }

            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }

        private void ButtonConvertAllDifficulties_Click(object sender, EventArgs e)
        {
            if (listBoxMaps.CheckedItems.Count == 0)
            {
                MessageBox.Show("You have nothing selected!\n" +
                    "First, select the songs you want to add the 360 mode to, then click the convert button.",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetUI(false);
            int added = 0;

            ProgressDialog progressDialog = new ProgressDialog();
            progressDialog.ShowCancelButton = false;
            progressDialog.WindowTitle = "Converting maps...";
            progressDialog.UseCompactPathsForDescription = true;
            progressDialog.DoWork += (sender2, e2) =>
            {
                int converted = 0;
                foreach (BeatMapInfo info in listBoxMaps.CheckedItems)
                {
                    foreach (string difficulty in comboBoxDifficulty.Items)
                    {
                        Log($"\tAdding mode to '{info}'");
                        if (TryConvert(info.mapRoot, difficulty, checkBoxReplace.Checked))
                            added++;

                        int progress = Math.Min((int)((float)converted / listBoxMaps.CheckedItems.Count * 100f), 100);
                        progressDialog.ReportProgress(progress, info.ToString(), info.mapRoot);
                    }
                    converted++;
                }
            };
            progressDialog.RunWorkerCompleted += (sender2, e2) => 
            {
                if (added > 0)
                    MessageBox.Show($"{added}x 360 levels were added to {listBoxMaps.CheckedItems.Count} different songs. Just navigate to the level in the game and the 360 mode should appear.",
                        "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"No modes were added, this can be due to:\n" +
                        $" - The selected song does not have the standard mode, this is required for the conversion.\n" +
                        $" - The selected song already has the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n" +
                        $" - An unkown file system error.\n", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            };
            progressDialog.ShowDialog();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            List<BeatMapInfo> checkedMaps = new List<BeatMapInfo>();
            foreach(BeatMapInfo info in listBoxMaps.CheckedItems)
                checkedMaps.Add(info);
            
            listBoxMaps.Items.Clear();
            listBoxMaps.Items.AddRange(beatMaps.Where((map) =>
                map.songName.ToLower().Contains(textBoxSearch.Text.ToLower())
                || map.songSubName.ToLower().Contains(textBoxSearch.Text.ToLower())
                || map.songAuthorName.ToLower().Contains(textBoxSearch.Text.ToLower())
                || checkedMaps.Contains(map)).ToArray());

            for (int i = 0; i < listBoxMaps.Items.Count; i++)
                listBoxMaps.SetItemChecked(i, checkedMaps.Contains(listBoxMaps.Items[i]));
        }

        private void SelectAll(bool selected)
        {
            for (int i = 0; i < listBoxMaps.Items.Count; i++)
                listBoxMaps.SetItemChecked(i, selected);

            UpdateStatus();
        }

        private void ButtonSelectNone_Click(object sender, EventArgs e)
        {
            SelectAll(false);
        }

        private void ButtonSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll(true);
        }

        public void UpdateStatus()
        {
            if (listBoxMaps.CheckedItems.Count == 0)
                labelInfo.Text = "The 360 will be added to no levels, please select some in the list.";
            else
                labelInfo.Text = $"The 360 mode will be added to {listBoxMaps.CheckedItems.Count} levels.";
        }

        private void ListBoxMaps_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateStatus();
        }

        private void TextBoxMapPath_KeyDown(object sender, KeyEventArgs e)
        {
           if (e.KeyCode == Keys.Enter && !textBoxMapPath.ReadOnly)
           {
               FindSongs();
           }
        }

        private void TextBoxSearch_Click(object sender, EventArgs e)
        {
            if (listBoxMaps.Items.Count == listBoxMaps.CheckedItems.Count)
            {
                SelectAll(false);
            }
        }

        private void ToolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/CodeStix");
        }
    }
}
