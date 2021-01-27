using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.ThreeSixtyfyer
{
    public partial class FormGeneratorSettings : Form
    {
        private IBeatMapGenerator generator;
        private bool saved = true;

        public FormGeneratorSettings(ref IBeatMapGenerator generator)
        {
            this.generator = generator;

            InitializeComponent();

            LoadSettings();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            if (saved)
                this.Close();
        }

        private void LoadSettings()
        {
            textBoxJson.Text = JsonConvert.SerializeObject(this.generator.Settings, Program.JsonSettings);
            saved = true;
        }

        private void SaveSettings()
        {
            try
            {
                generator.Settings = JsonConvert.DeserializeObject(textBoxJson.Text, generator.Settings.GetType(), Program.JsonSettings);
                Message("Saved!", Color.Green);
                saved = true;
            }
            catch (Exception ex)
            {
                Message("Could not save: \n" + ex.Message, Color.DarkRed);
            }
        }

        private void Message(string message, Color color)
        {
            textBoxError.Text = $"[{DateTime.Now.ToString()}] {message}";
            textBoxError.ForeColor = color;
            this.Height = 593;
        }

        private void FormGeneratorSettings_Load(object sender, EventArgs e)
        {
            this.Height = 488;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (!saved && MessageBox.Show("You have unsaved changes, are you sure you want to exit?", "Not saved.", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                return;

            this.Close();
        }

        private void textBoxJson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveSettings();
            }
        }

        private void buttonDefaults_Click(object sender, EventArgs e)
        {
            generator.Settings = Activator.CreateInstance(generator.Settings.GetType());
            LoadSettings();
            Message("Restored settings to default! (not saved yet)", Color.Green);
        }

        private void textBoxJson_TextChanged(object sender, EventArgs e)
        {
            saved = false;
        }
    }
}
