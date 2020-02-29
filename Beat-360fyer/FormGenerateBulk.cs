using Newtonsoft.Json;
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

    public partial class FormGenerateBulk : Form
    {
        private IBeatMapGenerator generator;
        private ThreeSixtyfyerConfig config;
        private List<BeatMapInfo> beatMaps = new List<BeatMapInfo>();

        public FormGenerateBulk()
        {
            InitializeComponent();
        }

        private void SetUI(bool enabled)
        {
            listBoxMaps.Enabled = enabled;
            textBoxSearch.Enabled = enabled;
            buttonOpenMap.Enabled = enabled;
            radioButtonExport.Enabled = enabled;
            radioButtonModify.Enabled = enabled;
            checkBoxForceGenerate.Enabled = enabled;
        }

        private void ButtonOpenMap_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
            folderBrowser.Description = "Select your BeatSaber_Data/CustomLevels directory or select a single custom level, " +
                "it will look for *.dat files, so either will work.\n" +
                "The selected path will be remembered.";
            folderBrowser.SelectedPath = config.bulkPath;

            if (!(folderBrowser.ShowDialog() ?? false))
                return;

            SetPath(folderBrowser.SelectedPath);

            config.bulkPath = folderBrowser.SelectedPath;
        }

        private void SetPath(string path)
        {
            textBoxMapPath.Text = path;
            listBoxMaps.Items.Clear();

            this.Height = 650;

            Jobs.FindSongsUnderPath(path, FindSongsUnderPath_Completed);
        }

        private void FindSongsUnderPath_Completed(WorkerJob<string, Jobs.FindSongsJobResult> job)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                foreach (Exception exception in job.exceptions)
                    MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                beatMaps = job.result.beatMaps;

                listBoxMaps.Items.Clear();
                listBoxMaps.Items.AddRange(beatMaps.ToArray());

                if (listBoxMaps.Items.Count > 0)
                {
                    SetUI(true);
                    comboBoxDifficulty.SelectedIndex = 0;
                }
                else
                {
                    SetUI(false);
                    buttonOpenMap.Enabled = true;

                    MessageBox.Show("No compatible maps where found in the selected path.\n" +
                        "Select the map's directory containing the 'Info.dat' file. Or your CustomLevels directory.",
                        "No maps found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }));
        }

        private void ButtonConvert_Click(object sender, EventArgs e)
        {
            ConvertCheckedSongs(new HashSet<BeatMapDifficultyLevel>() { (BeatMapDifficultyLevel)comboBoxDifficulty.SelectedItem });
        }

        private void ButtonConvertAllDifficulties_Click(object sender, EventArgs e)
        {
            ConvertCheckedSongs(BeatMapDifficulty.AllDiffultyLevels.ToHashSet());
        }

        private void ConvertCheckedSongs(HashSet<BeatMapDifficultyLevel> difficultyLevels)
        {
            if (listBoxMaps.CheckedItems.Count == 0)
            {
                MessageBox.Show("You have nothing selected!\n" +
                    "First, select the songs you want to add the 360 mode to, then click the convert button.",
                    "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetUI(false);

            BeatMapInfo[] maps = new BeatMapInfo[listBoxMaps.CheckedItems.Count];
            listBoxMaps.CheckedItems.CopyTo(maps, 0);

            string destination = null;
            if (radioButtonExport.Checked)
            {
                VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
                folderBrowser.SelectedPath = config.exportPath;
                folderBrowser.Description = "Select a root directory where all the generated 360 songs should be exported to. Each exported song will be placed in his own directory under this path.";
            
                if (!(folderBrowser.ShowDialog() ?? false))
                    return;

                destination = folderBrowser.SelectedPath;
                config.exportPath = destination;
            }

            Jobs.GenerateMapsOptions options = new Jobs.GenerateMapsOptions()
            {
                difficultyLevels = difficultyLevels,
                toGenerateFor = new List<BeatMapInfo>(maps),
                destination = destination,
                forceGenerate = checkBoxForceGenerate.Checked,
                generator = generator
            };

            Jobs.GenerateMaps(options, (job) =>
            {
                if (job.result.mapsChanged > 0)
                {
                    MessageBox.Show($"360 modes were added to {job.result.mapsChanged} different levels for these difficulties: " +
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
                            $" - The selected songs already have the newest 360 mode generated; generation not needed for the second time.\n",
                            "Nothing happened", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                BeginInvoke(new MethodInvoker(() => SetUI(true)));
            });
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
        }

        private void ButtonSelectNone_Click(object sender, EventArgs e)
        {
            SelectAll(false);
        }

        private void TextBoxMapPath_KeyDown(object sender, KeyEventArgs e)
        {
           if (e.KeyCode == Keys.Enter && !textBoxMapPath.ReadOnly)
           {
                beatMaps.Clear();

                Jobs.FindSongsUnderPath(textBoxMapPath.Text, FindSongsUnderPath_Completed);
           }
        } 

        private void TextBoxSearch_Click(object sender, EventArgs e)
        {
            if (beatMaps.Count == listBoxMaps.CheckedItems.Count)
            {
                SelectAll(false);
            }
        }

        private void ToolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/CodeStix/Beat-360fyer");
        }

        private void ListBoxMaps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAll(true);
            }
            else if (e.Control && e.KeyCode == Keys.D)
            {
                SelectAll(false);
            }
        }

        private void FormGenerateBulk_Load(object sender, EventArgs e)
        {
            this.Height = 180;

            if (!Config.TryLoad(out config))
            {
                MessageBox.Show("Could not load the config file, no permission? Maybe run as administrator?", "Could not load config.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            generator = BeatMapGenerator.GetGeneratorWithName(config.generatorToUse);
            if (generator == null)
            {
                config.generatorToUse = BeatMapGenerator.DEFAULT_GENERATOR;
                MessageBox.Show($"Generator with name {config.generatorToUse} not found. Setting to default generator {config.generatorToUse}.");
                generator = BeatMapGenerator.GetGeneratorWithName(config.generatorToUse);
            }
            generator.Settings = config.generatorSettings;

            foreach (BeatMapDifficultyLevel diff in BeatMapDifficulty.AllDiffultyLevels)
                comboBoxDifficulty.Items.Add(diff);

            if (!string.IsNullOrEmpty(config.bulkPath))
            {
                SetPath(config.bulkPath);
            }
        }

        private void buttonGeneratorSettings_Click(object sender, EventArgs e)
        {
            new FormGeneratorSettings(generator).ShowDialog();
            config.generatorToUse = generator.Name;
            config.generatorSettings = generator.Settings;
        }

        private void FormGenerateBulk_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.TrySave();
        }
    }
}
