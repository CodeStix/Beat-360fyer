using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.ThreeSixtyfyer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.Run(new FormGeneratorSettings(new BeatMap360GeneratorSettings(120f, 0f)));
            //return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GitHubBasedUpdateCheck updateChecker = new GitHubBasedUpdateCheck("CodeStix", "Beat-360fyer", "Build/latestVersion.txt");
            updateChecker.CheckForUpdate(Assembly.GetExecutingAssembly().GetName().Version.ToString(3)).ContinueWith((update) => {

                if (update.Result && MessageBox.Show("There is an update available for Beat-360fyer, please download the newest version " +
                        "to ensure that everything can work correctly. Go to the download page right now?", "An update!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Process.Start(@"https://github.com/CodeStix/Beat-360fyer/releases");
                    Environment.Exit(0);
                }
            });

            TaskDialog dialog = new TaskDialog();
            dialog.Buttons.Add(new TaskDialogButton()
            {
                Text = "Music Pack",
                ButtonType = ButtonType.Custom,
                Default = true,
                CommandLinkNote = "Create a music pack with generated modes for selected songs."
            });
            dialog.Buttons.Add(new TaskDialogButton()
            {
                Text = "Update Music Pack",
                ButtonType = ButtonType.Custom,
                Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.LastGeneratedMusicPackPath),
                CommandLinkNote = "Update the previously generated music pack, generate modes for newly imported songs."
            });
            dialog.Buttons.Add(new TaskDialogButton()
            {
                Text = "Generate And Overwrite",
                ButtonType = ButtonType.Custom,
                CommandLinkNote = "Add generated modes to a existing maps and modify them directly."
            });
            dialog.Buttons.Add(new TaskDialogButton()
            {
                ButtonType = ButtonType.Close
            });

            dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
            dialog.Content = "Welcome to Beat-360fyer, this tool will generate 360 modes for you.\n" +
               "What would you like to do?";

            TaskDialogButton pressed = dialog.ShowDialog();
            if (pressed.ButtonType == ButtonType.Custom)
            {
                if (pressed.Text == "Music Pack")
                {
                    Application.Run(new FormGeneratePack());
                }
                else if (pressed.Text == "Update Music Pack")
                {
                    Application.Run(new FormGeneratePack(true));
                }
                else if (pressed.Text == "Generate And Overwrite")
                {
                    Application.Run(new FormGenerateBulk());
                }
            }
        }
    }
}
