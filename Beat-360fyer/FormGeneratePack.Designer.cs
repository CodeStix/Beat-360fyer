namespace Stx.ThreeSixtyfyer
{
    partial class FormGeneratePack
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Songs", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGeneratePack));
            this.buttonSelectBeatSaber = new System.Windows.Forms.Button();
            this.textBoxBeatSaberPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonGeneratorSettings = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.listSongs = new System.Windows.Forms.ListView();
            this.columnHeaderSongName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAuthor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonUpdatePack = new System.Windows.Forms.Button();
            this.checkBoxForceGenerate = new System.Windows.Forms.CheckBox();
            this.textBoxPackName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.listDifficulties = new System.Windows.Forms.CheckedListBox();
            this.radioButtonMusicPack = new System.Windows.Forms.RadioButton();
            this.radioButtonExport = new System.Windows.Forms.RadioButton();
            this.radioButtonModify = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.panelMusicPack = new System.Windows.Forms.Panel();
            this.labelBeatSaberStatus = new System.Windows.Forms.Label();
            this.comboBoxGenerator = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.linkCreateGenerator = new System.Windows.Forms.LinkLabel();
            this.toolTipGeneratorDescription = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1.SuspendLayout();
            this.panelMusicPack.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSelectBeatSaber
            // 
            this.buttonSelectBeatSaber.Location = new System.Drawing.Point(12, 77);
            this.buttonSelectBeatSaber.Name = "buttonSelectBeatSaber";
            this.buttonSelectBeatSaber.Size = new System.Drawing.Size(126, 23);
            this.buttonSelectBeatSaber.TabIndex = 0;
            this.buttonSelectBeatSaber.Text = "Select directory...\r\n";
            this.buttonSelectBeatSaber.UseVisualStyleBackColor = true;
            this.buttonSelectBeatSaber.Click += new System.EventHandler(this.ButtonSelectBeatSaber_Click);
            // 
            // textBoxBeatSaberPath
            // 
            this.textBoxBeatSaberPath.Location = new System.Drawing.Point(144, 79);
            this.textBoxBeatSaberPath.Name = "textBoxBeatSaberPath";
            this.textBoxBeatSaberPath.ReadOnly = true;
            this.textBoxBeatSaberPath.Size = new System.Drawing.Size(666, 20);
            this.textBoxBeatSaberPath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Generate Mode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(455, 39);
            this.label2.TabIndex = 4;
            this.label2.Text = "First, select a directory containing songs, or select the Beat Saber\r\ninstallatio" +
    "n location to create a custom Music Pack (recommended). Then, tweak some setting" +
    "s\r\nand press the green button.";
            // 
            // buttonGeneratorSettings
            // 
            this.buttonGeneratorSettings.BackColor = System.Drawing.Color.White;
            this.buttonGeneratorSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGeneratorSettings.Location = new System.Drawing.Point(519, 492);
            this.buttonGeneratorSettings.Name = "buttonGeneratorSettings";
            this.buttonGeneratorSettings.Size = new System.Drawing.Size(278, 23);
            this.buttonGeneratorSettings.TabIndex = 3;
            this.buttonGeneratorSettings.Text = "Advanced generator settings...";
            this.buttonGeneratorSettings.UseVisualStyleBackColor = false;
            this.buttonGeneratorSettings.Click += new System.EventHandler(this.buttonGeneratorSettings_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGenerate.Location = new System.Drawing.Point(513, 582);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(297, 51);
            this.buttonGenerate.TabIndex = 2;
            this.buttonGenerate.Text = "Generate Music Pack\r\n";
            this.buttonGenerate.UseVisualStyleBackColor = false;
            this.buttonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
            // 
            // listSongs
            // 
            this.listSongs.CheckBoxes = true;
            this.listSongs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSongName,
            this.columnHeaderAuthor});
            this.listSongs.FullRowSelect = true;
            listViewGroup1.Header = "Songs";
            listViewGroup1.Name = "listViewGroup1";
            this.listSongs.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.listSongs.HideSelection = false;
            this.listSongs.Location = new System.Drawing.Point(12, 116);
            this.listSongs.Name = "listSongs";
            this.listSongs.Size = new System.Drawing.Size(496, 517);
            this.listSongs.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listSongs.TabIndex = 6;
            this.listSongs.UseCompatibleStateImageBehavior = false;
            this.listSongs.View = System.Windows.Forms.View.Details;
            this.listSongs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListSongs_KeyDown);
            // 
            // columnHeaderSongName
            // 
            this.columnHeaderSongName.Text = "Name";
            this.columnHeaderSongName.Width = 230;
            // 
            // columnHeaderAuthor
            // 
            this.columnHeaderAuthor.Text = "Author";
            this.columnHeaderAuthor.Width = 100;
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(9, 636);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(499, 48);
            this.label3.TabIndex = 7;
            this.label3.Text = "Tip: use shift to select multiple items, then use space to check or uncheck them." +
    "\r\nUse (Ctrl+A) to select all songs and (Ctrl+D) to deselect all songs.\r\nJump to " +
    "a song by pressing a letter.";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 684);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(822, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(114, 17);
            this.toolStripStatusLabel1.Text = "by CodeStix 2020 (c)";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.IsLink = true;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabel2.Text = "GitHub";
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // buttonUpdatePack
            // 
            this.buttonUpdatePack.Location = new System.Drawing.Point(632, 12);
            this.buttonUpdatePack.Name = "buttonUpdatePack";
            this.buttonUpdatePack.Size = new System.Drawing.Size(179, 23);
            this.buttonUpdatePack.TabIndex = 18;
            this.buttonUpdatePack.Text = "Update previous generated pack";
            this.buttonUpdatePack.UseVisualStyleBackColor = true;
            this.buttonUpdatePack.Click += new System.EventHandler(this.buttonUpdatePack_Click);
            // 
            // checkBoxForceGenerate
            // 
            this.checkBoxForceGenerate.AutoSize = true;
            this.checkBoxForceGenerate.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxForceGenerate.Location = new System.Drawing.Point(519, 456);
            this.checkBoxForceGenerate.Name = "checkBoxForceGenerate";
            this.checkBoxForceGenerate.Size = new System.Drawing.Size(167, 30);
            this.checkBoxForceGenerate.TabIndex = 19;
            this.checkBoxForceGenerate.Text = "Force generate\r\n(regenerates up-to-date maps)\r\n";
            this.checkBoxForceGenerate.UseVisualStyleBackColor = true;
            // 
            // textBoxPackName
            // 
            this.textBoxPackName.Location = new System.Drawing.Point(67, 0);
            this.textBoxPackName.MaxLength = 32;
            this.textBoxPackName.Name = "textBoxPackName";
            this.textBoxPackName.Size = new System.Drawing.Size(138, 20);
            this.textBoxPackName.TabIndex = 20;
            this.textBoxPackName.Text = "Generated 360 Levels";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Pack name:";
            // 
            // listDifficulties
            // 
            this.listDifficulties.FormattingEnabled = true;
            this.listDifficulties.Location = new System.Drawing.Point(611, 371);
            this.listDifficulties.Name = "listDifficulties";
            this.listDifficulties.Size = new System.Drawing.Size(186, 79);
            this.listDifficulties.TabIndex = 22;
            // 
            // radioButtonMusicPack
            // 
            this.radioButtonMusicPack.AutoSize = true;
            this.radioButtonMusicPack.Checked = true;
            this.radioButtonMusicPack.Location = new System.Drawing.Point(517, 135);
            this.radioButtonMusicPack.Name = "radioButtonMusicPack";
            this.radioButtonMusicPack.Size = new System.Drawing.Size(211, 17);
            this.radioButtonMusicPack.TabIndex = 23;
            this.radioButtonMusicPack.TabStop = true;
            this.radioButtonMusicPack.Text = "Create Music Pack (requires SongCore)";
            this.radioButtonMusicPack.UseVisualStyleBackColor = true;
            this.radioButtonMusicPack.CheckedChanged += new System.EventHandler(this.radioButtonMusicPack_CheckedChanged);
            // 
            // radioButtonExport
            // 
            this.radioButtonExport.AutoSize = true;
            this.radioButtonExport.Location = new System.Drawing.Point(517, 186);
            this.radioButtonExport.Name = "radioButtonExport";
            this.radioButtonExport.Size = new System.Drawing.Size(294, 30);
            this.radioButtonExport.TabIndex = 24;
            this.radioButtonExport.Text = "Export the generated modes to a specified location. \r\nThe location will be asked " +
    "when you start the generation.";
            this.radioButtonExport.UseVisualStyleBackColor = true;
            this.radioButtonExport.CheckedChanged += new System.EventHandler(this.radioButtonExport_CheckedChanged);
            // 
            // radioButtonModify
            // 
            this.radioButtonModify.AutoSize = true;
            this.radioButtonModify.Location = new System.Drawing.Point(517, 224);
            this.radioButtonModify.Name = "radioButtonModify";
            this.radioButtonModify.Size = new System.Drawing.Size(294, 43);
            this.radioButtonModify.TabIndex = 25;
            this.radioButtonModify.Text = "Modify maps directly, this will place the new difficulty files \r\nin the original " +
    "map\'s directory and modify the info.dat file.\r\n(A backup of info.dat is created," +
    " named info.dat.bak)";
            this.radioButtonModify.UseVisualStyleBackColor = true;
            this.radioButtonModify.CheckedChanged += new System.EventHandler(this.radioButtonModify_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(514, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Export mode";
            // 
            // panelMusicPack
            // 
            this.panelMusicPack.Controls.Add(this.label4);
            this.panelMusicPack.Controls.Add(this.textBoxPackName);
            this.panelMusicPack.Location = new System.Drawing.Point(517, 158);
            this.panelMusicPack.Name = "panelMusicPack";
            this.panelMusicPack.Size = new System.Drawing.Size(211, 22);
            this.panelMusicPack.TabIndex = 27;
            // 
            // labelBeatSaberStatus
            // 
            this.labelBeatSaberStatus.ForeColor = System.Drawing.Color.Red;
            this.labelBeatSaberStatus.Location = new System.Drawing.Point(728, 137);
            this.labelBeatSaberStatus.Name = "labelBeatSaberStatus";
            this.labelBeatSaberStatus.Size = new System.Drawing.Size(101, 30);
            this.labelBeatSaberStatus.TabIndex = 28;
            // 
            // comboBoxGenerator
            // 
            this.comboBoxGenerator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGenerator.FormattingEnabled = true;
            this.comboBoxGenerator.Location = new System.Drawing.Point(611, 331);
            this.comboBoxGenerator.Name = "comboBoxGenerator";
            this.comboBoxGenerator.Size = new System.Drawing.Size(186, 21);
            this.comboBoxGenerator.TabIndex = 29;
            this.toolTipGeneratorDescription.SetToolTip(this.comboBoxGenerator, "yes");
            this.comboBoxGenerator.SelectedIndexChanged += new System.EventHandler(this.comboBoxGenerator_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(516, 334);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Generator to use:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(516, 371);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 26);
            this.label7.TabIndex = 30;
            this.label7.Text = "Difficulties to \r\ngenerate:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(513, 313);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Settings";
            // 
            // linkCreateGenerator
            // 
            this.linkCreateGenerator.AutoSize = true;
            this.linkCreateGenerator.Location = new System.Drawing.Point(713, 355);
            this.linkCreateGenerator.Name = "linkCreateGenerator";
            this.linkCreateGenerator.Size = new System.Drawing.Size(84, 13);
            this.linkCreateGenerator.TabIndex = 32;
            this.linkCreateGenerator.TabStop = true;
            this.linkCreateGenerator.Text = "Create your own";
            this.linkCreateGenerator.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCreateGenerator_LinkClicked);
            // 
            // toolTipGeneratorDescription
            // 
            this.toolTipGeneratorDescription.AutomaticDelay = 10;
            this.toolTipGeneratorDescription.AutoPopDelay = 32767;
            this.toolTipGeneratorDescription.InitialDelay = 100;
            this.toolTipGeneratorDescription.IsBalloon = true;
            this.toolTipGeneratorDescription.ReshowDelay = 500;
            this.toolTipGeneratorDescription.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipGeneratorDescription.UseAnimation = false;
            // 
            // FormGeneratePack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 706);
            this.Controls.Add(this.linkCreateGenerator);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxGenerator);
            this.Controls.Add(this.listDifficulties);
            this.Controls.Add(this.labelBeatSaberStatus);
            this.Controls.Add(this.checkBoxForceGenerate);
            this.Controls.Add(this.radioButtonMusicPack);
            this.Controls.Add(this.buttonGeneratorSettings);
            this.Controls.Add(this.panelMusicPack);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.radioButtonModify);
            this.Controls.Add(this.radioButtonExport);
            this.Controls.Add(this.buttonUpdatePack);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.listSongs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxBeatSaberPath);
            this.Controls.Add(this.buttonSelectBeatSaber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormGeneratePack";
            this.Text = "Beat-360fyer - A 360 level generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGeneratePack_FormClosing);
            this.Load += new System.EventHandler(this.FormGenerate_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelMusicPack.ResumeLayout(false);
            this.panelMusicPack.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSelectBeatSaber;
        private System.Windows.Forms.TextBox textBoxBeatSaberPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listSongs;
        private System.Windows.Forms.ColumnHeader columnHeaderSongName;
        private System.Windows.Forms.ColumnHeader columnHeaderAuthor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonGeneratorSettings;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button buttonUpdatePack;
        private System.Windows.Forms.CheckBox checkBoxForceGenerate;
        private System.Windows.Forms.TextBox textBoxPackName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckedListBox listDifficulties;
        private System.Windows.Forms.RadioButton radioButtonMusicPack;
        private System.Windows.Forms.RadioButton radioButtonExport;
        private System.Windows.Forms.RadioButton radioButtonModify;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelMusicPack;
        private System.Windows.Forms.Label labelBeatSaberStatus;
        private System.Windows.Forms.ComboBox comboBoxGenerator;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.LinkLabel linkCreateGenerator;
        private System.Windows.Forms.ToolTip toolTipGeneratorDescription;
    }
}