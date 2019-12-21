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

namespace Stx.ThreeSixtyfyer
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

            textBoxMapPath.Text = folderBrowser.SelectedPath;
            listBoxMaps.Items.Clear();

            Properties.Settings.Default.RememberPath = textBoxMapPath.Text;
            Properties.Settings.Default.Save();

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
                    MessageBox.Show($"{added} (360) maps were added to {listBoxMaps.CheckedItems.Count} different songs. Just navigate to the map in the game and the 360 mode should appear.",
                        "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"No modes were added, this can be due to:\n" +
                        $" - The selected song does not have the standard mode, this is required for the conversion.\n" +
                        $" - The selected song already has the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n" +
                        $" - An unkown file system error. (Run as administrator?)\n", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            BeatMapInfo info = GetMapInfo(mapRoot, out string infoFile);
            BeatMapDifficultySet difStandardSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "Standard");
            BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());
            BeatMapDifficultySet dif360Set = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "360Degree");

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

            if (info.customData.contributors == null)
                info.customData.contributors = new List<BeatMapContributor>();
            if (!info.customData.contributors.Any((cont) => cont.name == "CodeStix's 360fyer"))
                info.customData.contributors.Add(new BeatMapContributor()
                {
                    name = "CodeStix's 360fyer",
                    role = "360 degree mode"
                });
            info.SaveToFile(infoFile);
        }

        public BeatMap Generate360MapFromStandard(BeatMap standardMap, float timeOffset = 0f)
        {
            const float WALL_LOOKAHEAD_TIME = 2f; // The time to look in the future for walls

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
            List<BeatMapObstacle> GetStartingObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => obst.time >= time && obst.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetActiveObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => time >= obst.time && time < obst.time + obst.duration + futureTime).ToList();
            }

            int spinsRemaining = 0;
            bool spinDirection = true;

            bool goDirection = false;

            int prevBeat = 0;
            for (float time = minTime; time < maxTime; time += FRAME_LENGTH)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, FRAME_LENGTH);
                List<BeatMapNote> notesInBeat = GetNotes(time, 1f);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, FRAME_LENGTH);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time, WALL_LOOKAHEAD_TIME); // look WALL_LOOKAHEAD_TIME beats in the future, on top of obstacle duration
                bool enableGoLeft = !activeObstacles.Any((obst) => (obst.lineIndex == 0 || obst.lineIndex == 1));
                bool enableGoRight = !activeObstacles.Any((obst) => (obst.lineIndex == 2 || obst.lineIndex == 3));
                bool heat = notesInBeat.Count >= 4;

                bool shouldSpin = GetActiveObstacles(time, 24f * FRAME_LENGTH).Count == 0 && GetNotes(time, 24f * FRAME_LENGTH + 2f).Count == 0;
                if (spinsRemaining > 0)
                {
                    spinsRemaining--;
                    if (spinDirection)
                        map.AddGoLeftEvent(time, 1);
                    else
                        map.AddGoRightEvent(time, 1);

                    if (spinsRemaining == 1 && shouldSpin) // still no notes, spin more
                        spinsRemaining += 24;

                    continue;
                }
                else if (time > firstNoteTime && shouldSpin) // spin effect
                {
                    spinDirection = !spinDirection; // spin any direction
                    spinsRemaining += 24; // 24 spins is one 360
                }

                if (obstaclesInFrame.Count == 1/* && activeObstacles.Count == 1*/)
                {
                    BeatMapObstacle obstacle = obstaclesInFrame[0];
                    if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                    {
                        map.AddGoRightEvent(time, 1);
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        map.AddGoLeftEvent(time, 1);
                        continue;
                    }
                }

                if (notesInFrame.Count == 0)
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || (note.cutDirection == 6 && note.lineIndex <= 2) || (note.cutDirection == 4 && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, heat ? 2 : 4));
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || (note.cutDirection == 5 && note.lineIndex >= 3) || (note.cutDirection == 7 && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, heat ? 2 : 4));
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
                }

                if (notesInBeat.Count > 0 && notesInBeat.All((note) => Math.Abs(note.time - notesInBeat[0].time) < FRAME_LENGTH))
                {
                    BeatMapNote[] groundLeftNotes = notesInFrame.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                    BeatMapNote[] groundRightNotes = notesInFrame.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                  
                    if (groundLeftNotes.Length != 0 && groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                    {
                        if (goDirection)
                            map.AddGoLeftEvent(time, 1);
                        else
                            map.AddGoRightEvent(time, 1);

                        goDirection = !goDirection;
                        continue;
                    }
                    else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft)
                    {
                        map.AddGoLeftEvent(groundLeftNotes[0].time, 1);
                        continue;
                    }
                    else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight)
                    {
                        map.AddGoRightEvent(groundRightNotes[0].time, 1);
                        continue;
                    }
                }

                // Bombs seizure
                if (GetNotes(time, 4f).All((note) => note.type == 3))
                {
                    map.AddGoLeftEvent(time, 2);
                    map.AddGoRightEvent(time + 0.25f, 4);
                    map.AddGoLeftEvent(time + 0.5f, 4);
                    map.AddGoRightEvent(time + 0.75f, 2);
                    continue;
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
                        else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                        {
                            if (goDirection)
                                map.AddGoLeftEvent(time, 1);
                            else
                                map.AddGoRightEvent(time, 1);

                            goDirection = !goDirection;
                            continue;
                        }
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
                    MessageBox.Show($"{added} (360) levels were added to {listBoxMaps.CheckedItems.Count} different songs. Just navigate to the level in the game and the 360 mode should appear.",
                        "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"No modes were added, this can be due to:\n" +
                        $" - The selected song does not have the standard mode, this is required for the conversion.\n" +
                        $" - The selected song already has the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n" +
                        $" - An unkown file system error. (Run as administrator?)\n", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            Process.Start("https://github.com/CodeStix/Beat-360fyer");
        }
    }
}
