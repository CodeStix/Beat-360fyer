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

            foreach (FieldInfo field in settings.GetType().GetFields())
            {
                FlowLayoutPanel settingPanel = new FlowLayoutPanel()
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false
                };
                settingPanel.Controls.Add(new Label()
                {
                    Text = field.Name,
                    Width = 120,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Left
                });

                object value = field.GetValue(settings);
                if (value is bool b)
                {
                    CheckBox check = new CheckBox()
                    {
                        Checked = b
                    };
                    check.CheckedChanged += (sender, e) => field.SetValue(settings, check.Checked);
                    settingPanel.Controls.Add(check);

                }
                else if (value is int || value is long || value is float || value is double || value is decimal)
                {
                    NumericUpDown numeric;
                    if (value is int)
                    {
                        numeric = new NumericUpDown()
                        {
                            Value = (int)value,
                            Minimum = int.MinValue,
                            Maximum = int.MaxValue,
                            DecimalPlaces = (value is int || value is long) ? 0 : 3
                            //Increment = (value is int || value is long) ? 1.0m : 0.001m
                        };
                    }

                    numeric.ValueChanged += (sender, e) => field.SetValue(settings, numeric.Value);
                    settingPanel.Controls.Add(numeric);
                }
                else
                {
                    settingPanel.Controls.Add(new Label() { Text = "[Not settable]" });
                }


                settingPanel.Controls.Add(new Label()
                {
                    Text = field.GetValue(settings).ToString(),
                    TextAlign = ContentAlignment.MiddleRight,
                    Dock = DockStyle.Right
                });


                flowPanelSettings.Controls.Add(settingPanel);

            }

            for (int i = 0; i < 3; i++)
            {
            }
        }

        private void FormGeneratorSettings_Load(object sender, EventArgs e)
        {
            



        }
    }
}
