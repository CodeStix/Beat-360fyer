namespace Stx.ThreeSixtyfyer
{
    partial class FormGenerateBulk
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGenerateBulk));
            this.buttonOpenMap = new System.Windows.Forms.Button();
            this.textBoxMapPath = new System.Windows.Forms.TextBox();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.comboBoxDifficulty = new System.Windows.Forms.ComboBox();
            this.buttonConvertAllDifficulties = new System.Windows.Forms.Button();
            this.groupBoxDifficulties = new System.Windows.Forms.GroupBox();
            this.checkBoxReplace = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxMaps = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonGeneratorSettings = new System.Windows.Forms.Button();
            this.groupBoxDifficulties.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenMap
            // 
            this.buttonOpenMap.Location = new System.Drawing.Point(11, 70);
            this.buttonOpenMap.Name = "buttonOpenMap";
            this.buttonOpenMap.Size = new System.Drawing.Size(104, 23);
            this.buttonOpenMap.TabIndex = 0;
            this.buttonOpenMap.Text = "Open directory...";
            this.buttonOpenMap.UseVisualStyleBackColor = true;
            this.buttonOpenMap.Click += new System.EventHandler(this.ButtonOpenMap_Click);
            // 
            // textBoxMapPath
            // 
            this.textBoxMapPath.Location = new System.Drawing.Point(121, 73);
            this.textBoxMapPath.Name = "textBoxMapPath";
            this.textBoxMapPath.ReadOnly = true;
            this.textBoxMapPath.Size = new System.Drawing.Size(370, 20);
            this.textBoxMapPath.TabIndex = 1;
            this.textBoxMapPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxMapPath_KeyDown);
            // 
            // buttonConvert
            // 
            this.buttonConvert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConvert.Location = new System.Drawing.Point(133, 35);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(151, 23);
            this.buttonConvert.TabIndex = 2;
            this.buttonConvert.Text = "Convert selected difficulty";
            this.buttonConvert.UseVisualStyleBackColor = false;
            this.buttonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
            // 
            // comboBoxDifficulty
            // 
            this.comboBoxDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDifficulty.FormattingEnabled = true;
            this.comboBoxDifficulty.Location = new System.Drawing.Point(6, 36);
            this.comboBoxDifficulty.Name = "comboBoxDifficulty";
            this.comboBoxDifficulty.Size = new System.Drawing.Size(121, 21);
            this.comboBoxDifficulty.TabIndex = 3;
            // 
            // buttonConvertAllDifficulties
            // 
            this.buttonConvertAllDifficulties.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonConvertAllDifficulties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConvertAllDifficulties.Location = new System.Drawing.Point(133, 64);
            this.buttonConvertAllDifficulties.Name = "buttonConvertAllDifficulties";
            this.buttonConvertAllDifficulties.Size = new System.Drawing.Size(151, 23);
            this.buttonConvertAllDifficulties.TabIndex = 4;
            this.buttonConvertAllDifficulties.Text = "Convert all difficulties\r\n";
            this.buttonConvertAllDifficulties.UseVisualStyleBackColor = false;
            this.buttonConvertAllDifficulties.Click += new System.EventHandler(this.ButtonConvertAllDifficulties_Click);
            // 
            // groupBoxDifficulties
            // 
            this.groupBoxDifficulties.Controls.Add(this.buttonGeneratorSettings);
            this.groupBoxDifficulties.Controls.Add(this.label5);
            this.groupBoxDifficulties.Controls.Add(this.checkBoxReplace);
            this.groupBoxDifficulties.Controls.Add(this.label3);
            this.groupBoxDifficulties.Controls.Add(this.label2);
            this.groupBoxDifficulties.Controls.Add(this.comboBoxDifficulty);
            this.groupBoxDifficulties.Controls.Add(this.buttonConvertAllDifficulties);
            this.groupBoxDifficulties.Controls.Add(this.buttonConvert);
            this.groupBoxDifficulties.Enabled = false;
            this.groupBoxDifficulties.Location = new System.Drawing.Point(12, 370);
            this.groupBoxDifficulties.Name = "groupBoxDifficulties";
            this.groupBoxDifficulties.Size = new System.Drawing.Size(479, 99);
            this.groupBoxDifficulties.TabIndex = 5;
            this.groupBoxDifficulties.TabStop = false;
            this.groupBoxDifficulties.Text = "Generate modes";
            // 
            // checkBoxReplace
            // 
            this.checkBoxReplace.Location = new System.Drawing.Point(335, 33);
            this.checkBoxReplace.Name = "checkBoxReplace";
            this.checkBoxReplace.Size = new System.Drawing.Size(118, 31);
            this.checkBoxReplace.TabIndex = 13;
            this.checkBoxReplace.Text = "Replace already existing 360 mode";
            this.checkBoxReplace.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Location = new System.Drawing.Point(6, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "<All difficulties>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Location = new System.Drawing.Point(3, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Only for this difficulty:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // listBoxMaps
            // 
            this.listBoxMaps.Enabled = false;
            this.listBoxMaps.FormattingEnabled = true;
            this.listBoxMaps.Location = new System.Drawing.Point(12, 135);
            this.listBoxMaps.Name = "listBoxMaps";
            this.listBoxMaps.ScrollAlwaysVisible = true;
            this.listBoxMaps.Size = new System.Drawing.Size(479, 229);
            this.listBoxMaps.TabIndex = 6;
            this.listBoxMaps.ThreeDCheckBoxes = true;
            this.listBoxMaps.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxMaps_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(449, 39);
            this.label1.TabIndex = 8;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Enabled = false;
            this.textBoxSearch.Location = new System.Drawing.Point(12, 109);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(480, 20);
            this.textBoxSearch.TabIndex = 9;
            this.textBoxSearch.Click += new System.EventHandler(this.TextBoxSearch_Click);
            this.textBoxSearch.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(504, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(190, 17);
            this.toolStripStatusLabel1.Text = "by Stijn Rogiest (CodeStix) 2019 (c)";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.IsLink = true;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabel2.Text = "GitHub";
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.ToolStripStatusLabel2_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Generate And Modify Directly";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.Control;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(332, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Settings:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // buttonGeneratorSettings
            // 
            this.buttonGeneratorSettings.Enabled = false;
            this.buttonGeneratorSettings.Location = new System.Drawing.Point(335, 66);
            this.buttonGeneratorSettings.Name = "buttonGeneratorSettings";
            this.buttonGeneratorSettings.Size = new System.Drawing.Size(122, 23);
            this.buttonGeneratorSettings.TabIndex = 15;
            this.buttonGeneratorSettings.Text = "Generator settings...";
            this.buttonGeneratorSettings.UseVisualStyleBackColor = true;
            // 
            // FormGenerateBulk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 503);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxMaps);
            this.Controls.Add(this.groupBoxDifficulties);
            this.Controls.Add(this.textBoxMapPath);
            this.Controls.Add(this.buttonOpenMap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormGenerateBulk";
            this.Text = "Beat-360fyer - A 360 level generator";
            this.groupBoxDifficulties.ResumeLayout(false);
            this.groupBoxDifficulties.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenMap;
        private System.Windows.Forms.TextBox textBoxMapPath;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.ComboBox comboBoxDifficulty;
        private System.Windows.Forms.Button buttonConvertAllDifficulties;
        private System.Windows.Forms.GroupBox groupBoxDifficulties;
        private System.Windows.Forms.CheckedListBox listBoxMaps;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.CheckBox checkBoxReplace;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonGeneratorSettings;
    }
}

