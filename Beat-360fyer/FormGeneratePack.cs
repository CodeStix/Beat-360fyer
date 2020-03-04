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
using System.Reflection;
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
                Description = "Please select the directory containing 'Beat Saber.exe' to create a music pack. Or open any other directory that contains songs to export or modify.",
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
            songCoreAvailable = beatSaberAvailable && File.Exists(Path.Combine(path, "UserData", "SongCore", "folders.xml"));

            bool enablePack = beatSaberAvailable && songCoreAvailable;
            radioButtonMusicPack.Enabled = enablePack;
            panelMusicPack.Enabled = enablePack;
            if (!enablePack)
                radioButtonExport.PerformClick();

            if (!beatSaberAvailable)
                labelBeatSaberStatus.Text = "Beat Saber not found in specfied directory.";
            else if (!songCoreAvailable)
                labelBeatSaberStatus.Text = "SongCore is not installed (required).";
            else
                labelBeatSaberStatus.Text = "";

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
            this.Enabled = enabled;
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

            GitHubBasedUpdateCheck updateChecker = new GitHubBasedUpdateCheck("CodeStix", "Beat-360fyer", "Build/latestVersion.txt");
            updateChecker.CheckForUpdate(Assembly.GetExecutingAssembly().GetName().Version.ToString(3)).ContinueWith((update) => {

                if (update.Result && MessageBox.Show("There is an update available for Beat-360fyer, please download the newest version " +
                        "to receive new generator features and improvements. Go to the download page right now?", "An update!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Process.Start(@"https://github.com/CodeStix/Beat-360fyer/releases");
                    Environment.Exit(0);
                }
            });

            if (!Config.TryLoad(out config))
            {
                MessageBox.Show("Could not load the config file, no permission? Maybe run as administrator?", "Could not load config.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            generator = BeatMapGenerator.GetGeneratorWithName(config.generatorToUse);
            if (generator == null)
            {
                MessageBox.Show($"Generator with name {config.generatorToUse} not found (found in config file). Setting to default generator.", "Unknown generator.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                generator = BeatMapGenerator.DefaultGenerator;
                config.generatorToUse = generator.GetInformation().Name;
            }
            if (config.generatorSettings.ContainsKey(config.generatorToUse))
                generator.Settings = config.generatorSettings[config.generatorToUse];

            foreach (Type generatorType in BeatMapGenerator.GeneratorTypes)
                comboBoxGenerator.Items.Add(BeatMapGenerator.GetGeneratorInfo(generatorType).Name);
            comboBoxGenerator.SelectedItem = config.generatorToUse;

            UpdateGeneratorSettingsButton();
            buttonUpdatePack.Visible = !string.IsNullOrEmpty(config.lastGeneratedMusicPackPath) && !string.IsNullOrEmpty(config.lastGeneratedMusicPackSourcePath);

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

                    EnsureCustomPack(textBoxPackName.Text, customGeneratedLevelsPath, imagePath);

                    config.lastGeneratedMusicPackSourcePath = CustomSongsPath;
                    config.lastGeneratedMusicPackPath = customGeneratedLevelsPath;

                    ConvertCheckedSongs(customGeneratedLevelsPath);
                }
                else if (radioButtonExport.Checked)
                {
                    VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
                    folderBrowser.SelectedPath = config.exportPath;
                    folderBrowser.Description = "Select a root directory where all the generated 360 songs should be exported to. Each exported song will be placed in his own directory under this path.";
                    if (!(folderBrowser.ShowDialog() ?? false))
                        return;

                    config.lastGeneratedMusicPackSourcePath = CustomSongsPath;
                    config.lastGeneratedMusicPackPath = folderBrowser.SelectedPath;

                    ConvertCheckedSongs(folderBrowser.SelectedPath);
                }
                else if (radioButtonModify.Checked)
                {
                    if (CustomSongsPath.EndsWith(@"Beat Saber_Data\CustomLevels"))
                    {
                        if (MessageBox.Show("Are you sure you want to modify maps directly in your BeatSaber/CustomLevels directory? This will break ScoreSaber submission on the selected maps.\nYou should create a music pack instead.", "Sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }

                    config.lastGeneratedMusicPackSourcePath = null;
                    config.lastGeneratedMusicPackPath = null;

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
                CreateResultTaskDialog(job, "Generation process complete!").ShowDialog();

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            });
        }

        private TaskDialog CreateResultTaskDialog(WorkerJob<Jobs.GenerateMapsOptions, Jobs.GeneratorMapsResult> job, string windowTitle)
        {
            StringBuilder message = new StringBuilder();
            StringBuilder footer = new StringBuilder();
            TaskDialogIcon icon = TaskDialogIcon.Information;

            if (job.result.mapsGenerated == 0)
            {
                if (job.result.mapsIterated == 0)
                {
                    message.AppendLine("No maps were selected.");
                }
                else if (job.result.mapsUpToDate > 0)
                {
                    message.AppendLine("All the selected maps are already up to date. If you want to force mode generation, please check 'Force Generate' in the menu.");
                }
                else
                {
                    message.AppendLine("No modes were added, this can be due to:");
                    message.AppendLine(" - The selected songs didn't have the standard mode, this is required for the conversion.");
                    message.AppendLine(" - Unknown reason...");
                }
                icon = TaskDialogIcon.Warning;
            }
            else if (radioButtonMusicPack.Checked)
            {
                message.AppendLine($"Done. Navigate to custom levels in the game and a new music pack named '{textBoxPackName.Text}' should appear.\n");
                message.AppendLine($"!!! NOTE: Due to a bug in SongCore, the new music pack is merged with normal levels in the default 'Custom Levels' pack. ");
                message.AppendLine($"Temporary fix: After all songs are loaded in the main menu, press Ctrl+R on your keyboard, this will hide the duplicates.");
                icon = TaskDialogIcon.Information;
            }
            else if (radioButtonExport.Checked)
            {
                message.AppendLine($"Done. The generated levels were exported to '{job.argument.destination}'.");
                icon = TaskDialogIcon.Information;
            }
            else if (radioButtonModify.Checked)
            {
                message.AppendLine($"Done. The selected levels were modified directly.");
                icon = TaskDialogIcon.Information;
            }
            if (job.result.cancelled)
            {
                footer.Append("The operation was cancelled. ");
                icon = TaskDialogIcon.Warning;
            }
            if (job.exceptions.Count > 0)
            {
                footer.Append($"{job.exceptions.Count} problems occured. ");
                icon = TaskDialogIcon.Error;
            }
            else
            {
                footer.Append($"No problems occured. ");
            }

            StringBuilder stats = new StringBuilder();
            stats.AppendLine($"{job.result.mapsIterated} maps iterated.");
            stats.AppendLine($"{job.result.mapsGenerated} maps generated.");
            stats.AppendLine($"{job.result.difficultiesGenerated} difficulties generated. ({string.Join(", ", job.argument.difficultyLevels)})");
            if (job.result.mapsUpToDate > 0)
                stats.AppendLine($"{job.result.mapsUpToDate} maps were already up to date.");

            TaskDialog dialog = new TaskDialog();
            dialog.WindowTitle = windowTitle;
            dialog.MainIcon = icon;
            dialog.Content = message.ToString();
            dialog.Footer = footer.ToString();
            dialog.ExpandedByDefault = true;
            dialog.ExpandedInformation = stats.ToString();
            dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
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

            return dialog;
        }

        private void buttonUpdatePack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(config.lastGeneratedMusicPackPath) || string.IsNullOrEmpty(config.lastGeneratedMusicPackSourcePath))
            {
                Console.WriteLine("Update pack button was pressed but nothing to update.");
                return;
            }

            Jobs.FindSongsUnderPath(config.lastGeneratedMusicPackSourcePath, (findSongsJob) =>
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
                    CreateResultTaskDialog(updateJob, "Updating maps complete!").ShowDialog();

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
            new FormGeneratorSettings(ref generator).ShowDialog();
            var info = generator.GetInformation();
            config.generatorToUse = info.Name;
            if (!config.generatorSettings.ContainsKey(info.Name))
                config.generatorSettings.Add(info.Name, generator.Settings);
            else
                config.generatorSettings[info.Name] = generator.Settings;
            UpdateGeneratorSettingsButton();
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

        private void UpdateGeneratorSettingsButton()
        {
            bool isDefault = generator.Settings.IsDefault();

            buttonGeneratorSettings.BackColor = isDefault ? Color.White : Color.Yellow;
            buttonGeneratorSettings.Text = isDefault ? "Generator settings... (default)" : "Generator settings... (modified!)";
        }

        private void comboBoxGenerator_SelectedIndexChanged(object sender, EventArgs e)
        {
            string generatorName = (string)comboBoxGenerator.SelectedItem;
            generator = BeatMapGenerator.GetGeneratorWithName(generatorName);
#if DEBUG
            Debug.Assert(generator != null);
#endif
            if (config.generatorSettings.ContainsKey(generatorName))
                generator.Settings = config.generatorSettings[generatorName];
            config.generatorToUse = generatorName;

            var info = generator.GetInformation();
            toolTipGeneratorDescription.ToolTipTitle = $"{info.Name} by {info.Author}";
            toolTipGeneratorDescription.SetToolTip(comboBoxGenerator, $"{info.Description} (version {info.Version} by {info.Author})");

            UpdateGeneratorSettingsButton();
        }

        private void linkCreateGenerator_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/CodeStix/Beat-360fyer");
        }
    }
}
