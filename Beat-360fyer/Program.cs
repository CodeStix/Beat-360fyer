using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
                Enabled = false,
                CommandLinkNote = "Update the existing music pack, generate a mode in the pack for all new imported songs."
            });
            dialog.Buttons.Add(new TaskDialogButton()
            {
                Text = "Generate And Modify",
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
                else if (pressed.Text == "Generate And Modify")
                {
                    Application.Run(new FormGenerateBulk());
                }
            }
        }
    }
}
