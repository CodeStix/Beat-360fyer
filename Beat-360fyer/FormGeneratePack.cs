using Ookii.Dialogs.Wpf;
using Stx.ThreeSixtyfyer.Generators;
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
        private IBeatMapGenerator generator;
        private ThreeSixtyfyerConfig config;
        private bool updateMusicPackOnStart = false;
        private bool beatSaberAvailable = false;
        private bool songCoreAvailable = false;

        private string CustomSongsPath => beatSaberAvailable ? Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "CustomLevels") : textBoxBeatSaberPath.Text;

        public FormGeneratePack(bool updateMusicPackOnStart = false)
        {
            InitializeComponent();

            this.updateMusicPackOnStart = updateMusicPackOnStart;
        }

        private void ButtonSelectBeatSaber_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog()
            {
                SelectedPath = config.packPath,
                Description = "Please select the directory containing 'Beat Saber.exe' to create a music pack. Or open any other directory to export or modify.",
                ShowNewFolderButton = false,
                UseDescriptionForTitle = false
            };

            if (!(folderBrowser.ShowDialog() ?? false))
                return;

            SetBeatSaberPath(folderBrowser.SelectedPath);
        }

        private void SetBeatSaberPath(string path)
        {
            beatSaberAvailable = File.Exists(Path.Combine(path, "Beat Saber.exe"));
            /*if (MessageBox.Show("The selected directory does not contain 'Beat Saber.exe', which is required.", "Not found",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                buttonSelectBeatSaber.PerformClick();*/
            songCoreAvailable = File.Exists(Path.Combine(path, "UserData", "SongCore", "folders.xml"));

            /*MessageBox.Show("You don't have SongCore installed, please use your mod installer of choice to install SongCore into BeatSaber, then come back here to generate a music pack.", "SongCore not found",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);*/

            panelMusicPack.Enabled = beatSaberAvailable && songCoreAvailable;
            if (!panelMusicPack.Enabled)
                radioButtonExport.PerformClick();

            config.packPath = path;

            textBoxBeatSaberPath.Text = path;
            this.Height = 736;

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

        private void SetUI(bool enabled)
        {
            buttonGenerate.Enabled = enabled;
            buttonSelectBeatSaber.Enabled = enabled;
            textBoxBeatSaberPath.Enabled = enabled;
            listSongs.Enabled = enabled;
            buttonGeneratorSettings.Enabled = enabled;
            textBoxPackName.Enabled = enabled;
            buttonUpdatePack.Enabled = enabled;
            checkBoxForceGenerate.Enabled = enabled;
            listDifficulties.Enabled = enabled;
            radioButtonModify.Enabled = enabled;
            radioButtonExport.Enabled = enabled;
            radioButtonMusicPack.Enabled = enabled;
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

            if (!Config.TryLoad(out config))
            {
                MessageBox.Show("Could not load the config file, no permission? Maybe run as administrator?", "Could not load config.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            generator = BeatMapGenerator.GetGeneratorWithName(config.generatorToUse);
            if (generator == null)
            {
                MessageBox.Show($"Generator with name {config.generatorToUse} not found. Setting to default generator {BeatMapGenerator.DEFAULT_GENERATOR}.", "Unknown generator.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                config.generatorToUse = BeatMapGenerator.DEFAULT_GENERATOR;
                generator = BeatMapGenerator.GetGeneratorWithName(config.generatorToUse);
            }
            generator.Settings = config.generatorSettings;

            this.buttonUpdatePack.Visible = !string.IsNullOrEmpty(config.lastGeneratedMusicPackPath);

            if (!string.IsNullOrEmpty(config.packPath))
                SetBeatSaberPath(config.packPath);

            if (updateMusicPackOnStart)
                buttonUpdatePack.PerformClick();

            foreach(BeatMapDifficultyLevel level in BeatMapDifficulty.AllDiffultyLevels)
                listDifficulties.Items.Add(level, true);
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButtonMusicPack.Checked && beatSaberAvailable && songCoreAvailable)
                {
                    if (textBoxPackName.Text.Length < 1)
                        throw new Exception($"The pack name '{textBoxPackName.Text}' is too short.");
                    if (!Regex.IsMatch(textBoxPackName.Text, "^[a-zA-Z0-9 ]+$"))
                        throw new Exception($"The pack name '{textBoxPackName.Text}' contains illegal characters, it should only contain A-Z and spaces.");

                    string customGeneratedLevelsPath = Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "Custom" + textBoxPackName.Text.Replace(" ", ""));

                    Directory.CreateDirectory(customGeneratedLevelsPath);
                    string imagePath = Path.Combine(customGeneratedLevelsPath, "cover.png");
                    Properties.Resources.PackThumbnail.Save(imagePath);
                    BeatMapGenerator.ContributorImagePath = imagePath;

                    config.lastGeneratedMusicPackPath = customGeneratedLevelsPath;

                    EnsureCustomPack(textBoxPackName.Text, customGeneratedLevelsPath, imagePath);
                    ConvertCheckedSongs(customGeneratedLevelsPath);
                }
                else if (radioButtonExport.Checked)
                {
                    VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
                    folderBrowser.SelectedPath = config.exportPath;
                    folderBrowser.Description = "Select a root directory where all the generated 360 songs should be exported to. Each exported song will be placed in his own directory under this path.";
                    if (!(folderBrowser.ShowDialog() ?? false))
                        return;
                    ConvertCheckedSongs(folderBrowser.SelectedPath);
                }
                else if (radioButtonModify.Checked)
                {
                    if (CustomSongsPath.EndsWith(@"Beat Saber_Data\CustomLevels"))
                    {
                        if (MessageBox.Show("Are you sure you want to modify maps directly in your BeatSaber/CustomLevels directory? This will break ScoreSaber submission on the selected maps.\nYou should create a music pack instead.", "Sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }

                    ConvertCheckedSongs(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot start generator: {ex.Message}", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void ConvertCheckedSongs(string destination)
        {
            if (listSongs.CheckedItems.Count == 0)
            {
                MessageBox.Show("You have nothing checked!\n" +
                    "First, select the songs you want to add the new mode to, then click the generate button.",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (listDifficulties.CheckedItems.Count == 0)
            {
                MessageBox.Show("No difficulties were selected!\n" +
                    "First, select some difficulties you wish to create a new mode for.");
                return;
            }

            SetUI(false);

            BeatMapInfo[] maps = new BeatMapInfo[listSongs.CheckedItems.Count];
            for (int i = 0; i < listSongs.CheckedItems.Count; i++)
                maps[i] = (BeatMapInfo)listSongs.CheckedItems[i].Tag;

            BeatMapDifficultyLevel[] levels = new BeatMapDifficultyLevel[listDifficulties.CheckedItems.Count];
            listDifficulties.CheckedItems.CopyTo(levels, 0);

            Jobs.GenerateMapsOptions options = new Jobs.GenerateMapsOptions()
            {
                difficultyLevels = new HashSet<BeatMapDifficultyLevel>(levels),
                destination = destination,
                toGenerateFor = new List<BeatMapInfo>(maps),
                forceGenerate = checkBoxForceGenerate.Checked,
                generator = generator
            };

            Jobs.GenerateMaps(options, (job) =>
            {
                string message = "";
                if (job.result.mapsChanged == 0)
                {
                    message = "No modes were added, this can be due to:\n" +
                        " - All the maps are alreay up to date.\n" +
                        " - The selected songs didn't have the standard mode, this is required for the conversion.\n";
                }
                else if (radioButtonMusicPack.Checked)
                {
                    message = $"Done. Navigate to custom levels in the game and a new music pack named {textBoxPackName.Text} should appear.\n\n" +
                    $"!!! NOTE: Due to a bug in SongCore, the new music pack is merged with normal levels in the default 'Custom Levels' pack. " +
                    $"Temporary fix: After all songs are loaded in the main menu, press Ctrl+R on your keyboard, this will hide the duplicates.";
                }
                else if (radioButtonExport.Checked)
                {
                    message = $"Done. The generated levels were exported to '{job.argument.destination}'.";
                }
                else if (radioButtonModify.Checked)
                {
                    message = $"Done. The selected levels were modified directly.";
                }

                TaskDialog dialog = new TaskDialog();
                dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                dialog.Content = message;
                dialog.Footer = $"Modes were generated for {job.result.mapsChanged} different levels for these difficulties: {string.Join(", ", job.argument.difficultyLevels)}.\n{job.exceptions.Count} problems occured.";
                dialog.WindowTitle = "Completed!";
                dialog.MainIcon = job.exceptions.Count > 0 ? TaskDialogIcon.Error : job.result.mapsChanged == 0 ? TaskDialogIcon.Warning : TaskDialogIcon.Information;
                if (job.exceptions.Count > 0)
                {
                    dialog.Buttons.Add(new TaskDialogButton("Show problems"));
                    dialog.ButtonClicked += (sender, e) =>
                    {
                        if (e.Item.Text == "Show problems")
                        {
                            StringBuilder str = new StringBuilder();
                            for (int i = 0; i < job.exceptions.Count && i < 10; i++)
                                str.AppendLine(job.exceptions[i].Message);
                            if (job.exceptions.Count > 10)
                                str.AppendLine($"\nAnd {job.exceptions.Count - 10} more...");

                            MessageBox.Show(str.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                }
                dialog.ShowDialog();

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            });
        }

        private void Dialog_ButtonClicked(object sender, TaskDialogItemClickedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void buttonUpdatePack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(config.lastGeneratedMusicPackPath))
            {
                Console.WriteLine("Update pack button was pressed but nothing to update.");
                return;
            }

            Jobs.FindSongsUnderPath(CustomSongsPath, (findSongsJob) =>
            {
                Jobs.GenerateMaps(new Jobs.GenerateMapsOptions()
                {
                    difficultyLevels = BeatMapDifficulty.AllDiffultyLevels.ToHashSet(),
                    destination = config.lastGeneratedMusicPackPath,
                    toGenerateFor = findSongsJob.result.beatMaps,
                    forceGenerate = checkBoxForceGenerate.Checked,
                    generator = generator
                }, (updateJob) =>
                {
                    if (updateJob.result.mapsChanged > 0)
                    {
                        MessageBox.Show($"{updateJob.result.mapsChanged} different levels are up to date with the latest generator version. (Will include newly imported songs)",
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
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/CodeStix/Beat-360fyer");
        }

        private void buttonGeneratorSettings_Click(object sender, EventArgs e)
        {
            new FormGeneratorSettings(generator).ShowDialog();
            config.generatorToUse = generator.Name;
            config.generatorSettings = generator.Settings;
        }

        private void FormGeneratePack_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.TrySave();
        }

        private void radioButtonMusicPack_CheckedChanged(object sender, EventArgs e)
        {
            buttonGenerate.Text = "Generate Music Pack";
        }

        private void radioButtonExport_CheckedChanged(object sender, EventArgs e)
        {
            buttonGenerate.Text = "Export to directory";
        }

        private void radioButtonModify_CheckedChanged(object sender, EventArgs e)
        {
            buttonGenerate.Text = "Modify selected maps";
        }
    }
}
