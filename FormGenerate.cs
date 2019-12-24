using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Stx.ThreeSixtyfyer
{
    public partial class FormGenerate : Form
    {
        public FormGenerate()
        {
            InitializeComponent();
        }

        private string CustomGenerated360LevelsPack => "Generated 360 Levels";
        private string CustomGenerated360LevelsPath => Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "CustomGenerated360Levels");
          
        private void ButtonSelectBeatSaber_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog()
            {
                SelectedPath = Properties.Settings.Default.RememberPath,
                Description = "Please select the directory containing 'Beat Saber.exe'.",
                ShowNewFolderButton = false,
                UseDescriptionForTitle = false
            };

            if (!folderBrowser.ShowDialog().Value)
                return;

            if (!File.Exists(Path.Combine(folderBrowser.SelectedPath, "Beat Saber.exe")))
            {
                if (MessageBox.Show("The selected directory does not contain 'Beat Saber.exe', which is required.", "Not found",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                    buttonSelectBeatSaber.PerformClick();
                return;
            }
            if (!File.Exists(Path.Combine(folderBrowser.SelectedPath, "UserData", "SongCore", "folders.xml")))
            {
                MessageBox.Show("You don't have SongCore installed, please use your mod installer of choise to install SongCore into BeatSaber, then come back here to generate a music pack.", "SongCore not found",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Properties.Settings.Default.RememberPath = folderBrowser.SelectedPath;
            Properties.Settings.Default.Save();

            textBoxBeatSaberPath.Text = folderBrowser.SelectedPath;

            RefreshSongs();
            EnsureCustomPack(CustomGenerated360LevelsPack, CustomGenerated360LevelsPath, "360.png");
        }

        private void RefreshSongs()
        {
            listSongs.Items.Clear();

            string customSongsPath = Path.Combine(textBoxBeatSaberPath.Text, "Beat Saber_Data", "CustomLevels");

            Jobs.FindSongsUnderPath(customSongsPath, (job) =>
            {
                listSongs.Items.AddRange(job.result.beatMaps.Select((bm) =>
                    new ListViewItem(new string[] { bm.songName, bm.songAuthorName }) {
                        Text = bm.songName,
                        Group = listSongs.Groups[0],
                        Tag = bm
                }).ToArray());
            });
        }
        
        private void EnsureCustomPack(string name, string songsDir, string imageToImport)
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

            string imagePath = Path.Combine(songsDir, "cover.png");
            File.Copy(imageToImport, imagePath, true);

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
            buttonGeneratorSettings.Enabled = enable;
            comboBoxMode.Enabled = enable;
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
            comboBoxMode.SelectedIndex = 0;
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            ConvertCheckedSongs(Enum.GetValues(typeof(BeatMapDifficultyLevel)).Cast<BeatMapDifficultyLevel>().ToList());
        }

        private void ConvertCheckedSongs(List<BeatMapDifficultyLevel> difficultyLevels)
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

            Jobs.Generate360ModesOptions options = new Jobs.Generate360ModesOptions()
            {
                difficultyLevels = difficultyLevels,
                destination = CustomGenerated360LevelsPath,
                toGenerateFor = new List<BeatMapInfo>(maps)
            };

            Jobs.Generate360Maps(options, (job) =>
            {
                if (job.result.modesGenerated > 0)
                {
                    MessageBox.Show($"{job.result.modesGenerated} (360) modes were added to {job.result.mapsChanged} different levels for these difficulties: " +
                        $"{string.Join(", ", job.argument.difficultyLevels)}\n\n" +
                        $"Just navigate to the level in the game and the 360 mode will appear.",
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
                            $" - The selected songs didn't have the standard mode, this is required for the conversion.\n" +
                            $" - The selected songs already have the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n",
                            "Nothing happened", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            });
        }
    }
}
