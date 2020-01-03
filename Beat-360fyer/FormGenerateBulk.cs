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

    public partial class FormGenerateBulk : Form
    {
        public FormGenerateBulk()
        {
            InitializeComponent();
        }

        private List<BeatMapInfo> beatMaps = new List<BeatMapInfo>();
        private List<BeatMapDifficultyLevel> beatMapDifficulties = new List<BeatMapDifficultyLevel>((BeatMapDifficultyLevel[])Enum.GetValues(typeof(BeatMapDifficultyLevel)));

        private void SetUI(bool enabled)
        {
            listBoxMaps.Enabled = enabled;
            textBoxSearch.Enabled = enabled;
            groupBoxDifficulties.Enabled = enabled;
            buttonOpenMap.Enabled = enabled;
        }

        private void ButtonOpenMap_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select your BeatSaber_Data/CustomLevels directory or select a single custom level, " +
                "it will look for *.dat files, so either will work.\n" +
                "The selected path will be remembered.";
            folderBrowser.SelectedPath = Properties.Settings.Default.RememberPathBulk;//Directory.GetCurrentDirectory();

            if (folderBrowser.ShowDialog() != DialogResult.OK)
                return;

            textBoxMapPath.Text = folderBrowser.SelectedPath;
            listBoxMaps.Items.Clear();

            Properties.Settings.Default.RememberPathBulk = textBoxMapPath.Text;
            Properties.Settings.Default.Save();

            this.Height = 572;

            Jobs.FindSongsUnderPath(textBoxMapPath.Text, FindSongsUnderPath_Completed);
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
            ConvertCheckedSongs(new List<BeatMapDifficultyLevel>() { (BeatMapDifficultyLevel)comboBoxDifficulty.SelectedItem });
        }

        private void ButtonConvertAllDifficulties_Click(object sender, EventArgs e)
        {
            ConvertCheckedSongs(beatMapDifficulties);
        }

        private void ConvertCheckedSongs(List<BeatMapDifficultyLevel> difficultyLevels)
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

            Jobs.Generate360ModesOptions options = new Jobs.Generate360ModesOptions()
            {
                difficultyLevels = difficultyLevels.ToArray(),
                replacePreviousModes = checkBoxReplace.Checked,
                toGenerateFor = new List<BeatMapInfo>(maps)
            };

            Jobs.Generate360Maps(options, (job) =>
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
                            $" - The selected songs already have the 360 modes, you can override this by checking 'replacing already existing 360 mode'.\n",
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

        private void ButtonSelectAll_Click(object sender, EventArgs e)
        {
           
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

            foreach (BeatMapDifficultyLevel diff in beatMapDifficulties)
                comboBoxDifficulty.Items.Add(diff);
        }
    }
}
