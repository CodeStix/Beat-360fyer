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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Stx.ThreeSixtyfyer
{
    public partial class FormGeneratePack : Form
    {
        public IBeatMapGenerator generator = new BeatMap360Generator()
        {
            Settings = new BeatMap360GeneratorSettings()
        };

        public bool updateMusicPackOnStart = false;

        public FormGeneratePack(bool updateMusicPackOnStart = false)
        {
            InitializeComponent();

            this.updateMusicPackOnStart = updateMusicPackOnStart;
        }

        private string CustomGenerated360LevelsPack => textBoxPackName.Text;
        private string CustomGenerated360LevelsPath => Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "Custom" + textBoxPackName.Text.Replace(" ", ""));
        private string CustomSongsPath => Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "CustomLevels");

        private void ButtonSelectBeatSaber_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog()
            {
                SelectedPath = Properties.Settings.Default.RememberPathPack,
                Description = "Please select the directory containing 'Beat Saber.exe'.",
                ShowNewFolderButton = false,
                UseDescriptionForTitle = false
            };

            if (!(folderBrowser.ShowDialog() ?? false))
                return;

            SetBeatSaberPath(folderBrowser.SelectedPath);
        }

        private void SetBeatSaberPath(string path)
        {
            if (!File.Exists(Path.Combine(path, "Beat Saber.exe")))
            {
                if (MessageBox.Show("The selected directory does not contain 'Beat Saber.exe', which is required.", "Not found",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    buttonSelectBeatSaber.PerformClick();
                return;
            }
            if (!File.Exists(Path.Combine(path, "UserData", "SongCore", "folders.xml")))
            {
                MessageBox.Show("You don't have SongCore installed, please use your mod installer of choise to install SongCore into BeatSaber, then come back here to generate a music pack.", "SongCore not found",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Properties.Settings.Default.RememberPathPack = path;
            Properties.Settings.Default.Save();

            textBoxBeatSaberPath.Text = path;
            this.Height = 627;

            RefreshSongs();
        }

        private void RefreshSongs()
        {
            listSongs.Items.Clear();

            Jobs.FindSongsUnderPath(CustomSongsPath, (job) =>
            {
                listSongs.Items.AddRange(job.result.beatMaps.Select((bm) =>
                    new ListViewItem(new string[] { bm.songName, bm.songAuthorName }) {
                        Text = bm.songName,
                        Group = listSongs.Groups[0],
                        Tag = bm
                }).ToArray());
            });
        }
        
        private void EnsureCustomPack(string name, string songsDir, string imagePath)
        {
            Directory.CreateDirectory(songsDir);

            string foldersFile = Path.Combine(textBoxBeatSaberPath.Text, "UserData", "SongCore", "folders.xml");
            XDocument document = XDocument.Load(foldersFile);

            // Remove old pack entry
            foreach (var folder in document.Root.Descendants("folder"))
            {
                if (folder.Element("Name").Value == name || folder.Element("Path").Value == songsDir)
                {
                    folder.Remove();
                    break;
                }
            }

            document.Root.Add(
                new XElement("folder",
                    new XElement("Name", name),
                    new XElement("Path", songsDir),
                    new XElement("Pack", 2),
                    new XElement("ImagePath", imagePath)));

            document.Save(foldersFile);
        }

        private void SetUI(bool enable)
        {
            buttonGenerate.Enabled = enable;
            buttonSelectBeatSaber.Enabled = enable;
            textBoxBeatSaberPath.Enabled = enable;
            listSongs.Enabled = enable;
            buttonGeneratorSettings.Enabled = enable && false;
            textBoxPackName.Enabled = enable;
            buttonUpdatePack.Enabled = enable;
        }

        private void ListSongs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in listSongs.Items)
                {
                    item.Checked = true;
                    item.Selected = true;
                }
            }
            else if (e.Control && e.KeyCode == Keys.D)
            {
                foreach (ListViewItem item in listSongs.Items)
                {
                    item.Checked = false;
                    item.Selected = false;
                }
            }
        }

        private void FormGenerate_Load(object sender, EventArgs e)
        {
            this.Height = 165;

            this.buttonUpdatePack.Visible = !string.IsNullOrEmpty(Properties.Settings.Default.LastGeneratedMusicPackPath);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.RememberPathPack))
            {
                SetBeatSaberPath(Properties.Settings.Default.RememberPathPack);
            }

            if (updateMusicPackOnStart)
                buttonUpdatePack.PerformClick();
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxPackName.Text.Length < 1)
                    throw new Exception($"The pack name '{textBoxPackName.Text}' is too short.");
                if (!Regex.IsMatch(textBoxPackName.Text, "^[a-zA-Z0-9 ]+$"))
                    throw new Exception($"The pack name '{textBoxPackName.Text}' contains illegal characters, it should only contain A-Z and spaces.");
                if (!File.Exists("360.png"))
                    throw new Exception("The cover image '360.png' was not found in the current directory, please place the '360.png' next to this tool.");

                Directory.CreateDirectory(CustomGenerated360LevelsPath);
                string imagePath = Path.Combine(CustomGenerated360LevelsPath, "cover.png");
                if (!File.Exists(imagePath))
                    File.Copy("360.png", imagePath, true);
                BeatMapGenerator.ContributorImagePath = imagePath;

                Properties.Settings.Default.LastGeneratedMusicPackPath = CustomGenerated360LevelsPath;
                Properties.Settings.Default.Save();

                EnsureCustomPack(CustomGenerated360LevelsPack, CustomGenerated360LevelsPath, imagePath);
                ConvertCheckedSongs(BeatMapDifficulty.AllLevels.ToHashSet());
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Cannot start generator: {ex.Message}", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void ConvertCheckedSongs(HashSet<BeatMapDifficultyLevel> difficultyLevels)
        {
            if (listSongs.CheckedItems.Count == 0)
            {
                MessageBox.Show("You have nothing checked!\n" +
                    "First, select the songs you want to add the mode to, then click the generate button.",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetUI(false);

            BeatMapInfo[] maps = new BeatMapInfo[listSongs.CheckedItems.Count];
            for (int i = 0; i < listSongs.CheckedItems.Count; i++)
                maps[i] = (BeatMapInfo)listSongs.CheckedItems[i].Tag;

            Jobs.GenerateMapsOptions options = new Jobs.GenerateMapsOptions()
            {
                difficultyLevels = difficultyLevels,
                destination = CustomGenerated360LevelsPath,
                toGenerateFor = new List<BeatMapInfo>(maps),
                forceGenerate = checkBoxForceGenerate.Checked,
                generator = generator
            };

            Jobs.GenerateMaps(options, (job) =>
            {
                if (job.result.mapsChanged > 0)
                {
                    MessageBox.Show($"360 modes were generated for {job.result.mapsChanged} different levels for these difficulties: " +
                        $"{string.Join(", ", job.argument.difficultyLevels)}\n\n" +
                        $"Navigate to custom levels in the game and a new music pack should appear named {CustomGenerated360LevelsPack}.\n\n" +
                        $"!!! NOTE: Due to a bug in SongCore, the new music pack is merged with normal levels in the default 'Custom Levels' pack. " +
                        $"Temporary fix: After all songs are loaded in the main menu, press Ctrl+R on your keyboard, this will hide the duplicates.",
                        "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (job.exceptions.Count > 0)
                    {
                        StringBuilder str = new StringBuilder();
                        for (int i = 0; i < job.exceptions.Count && i < 10; i++)
                            str.AppendLine(job.exceptions[i].Message);
                        if (job.exceptions.Count > 10)
                            str.AppendLine($"\nAnd {job.exceptions.Count - 10} more...");

                        MessageBox.Show(str.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"No modes were added, this can be due to:\n" +
                            " - All the maps are alreay up to date.\n" +
                            " - The selected songs didn't have the standard mode, this is required for the conversion.\n",
                            "Nothing happened", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            });
        }


        private void buttonUpdatePack_Click(object sender, EventArgs e)
        {
            Console.WriteLine(CustomSongsPath);

            Jobs.FindSongsUnderPath(CustomSongsPath, (findSongsJob) =>
            {
                Jobs.GenerateMaps(new Jobs.GenerateMapsOptions()
                {
                    difficultyLevels = BeatMapDifficulty.AllLevels.ToHashSet(),
                    destination = Properties.Settings.Default.LastGeneratedMusicPackPath,
                    toGenerateFor = findSongsJob.result.beatMaps,
                    forceGenerate = checkBoxForceGenerate.Checked,
                    generator = generator
                }, (updateJob) =>
                {
                    if (updateJob.result.mapsChanged > 0)
                    {
                        MessageBox.Show($"{updateJob.result.mapsChanged} different levels are up to date with the latest generator version.",
                            "Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (updateJob.exceptions.Count > 0)
                        {
                            StringBuilder str = new StringBuilder();
                            for (int i = 0; i < updateJob.exceptions.Count && i < 10; i++)
                                str.AppendLine(updateJob.exceptions[i].Message);
                            if (updateJob.exceptions.Count > 10)
                                str.AppendLine($"\nAnd {updateJob.exceptions.Count - 10} more...");

                            MessageBox.Show(str.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show($"All levels are already up to date!",
                                "Nothing happened", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }

                    BeginInvoke(new MethodInvoker(() => SetUI(true)));
                });
            });

            /*Jobs.UpdateExisting360Maps(Properties.Settings.Default.LastGeneratedMusicPackPath, (s) =>
            {
                MessageBox.Show($"{s.result.mapsUpdated} generated maps are up to date now.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetUI(true);
            });*/
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/CodeStix/Beat-360fyer");
        }

        private void buttonGeneratorSettings_Click(object sender, EventArgs e)
        {
            new FormGeneratorSettings(new BeatMap360GeneratorSettings()).ShowDialog();
        }
    }
}
