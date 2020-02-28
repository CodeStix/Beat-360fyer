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
        public FormGeneratorSettings(BeatMap360GeneratorSettings settings)
        {
            InitializeComponent();

            textBoxJson.Text = JsonConvert.SerializeObject(settings, Formatting.Indented);
        }

        private void FormGeneratorSettings_Load(object sender, EventArgs e)
        {
            



        }
    }
}
