namespace Stx.ThreeSixtyfyer
{
    partial class FormGenerate
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Songs", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGenerate));
            this.buttonSelectBeatSaber = new System.Windows.Forms.Button();
            this.textBoxBeatSaberPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonGeneratorSettings = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.listSongs = new System.Windows.Forms.ListView();
            this.columnHeaderSongName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAuthor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSelectBeatSaber
            // 
            this.buttonSelectBeatSaber.Location = new System.Drawing.Point(12, 53);
            this.buttonSelectBeatSaber.Name = "buttonSelectBeatSaber";
            this.buttonSelectBeatSaber.Size = new System.Drawing.Size(155, 23);
            this.buttonSelectBeatSaber.TabIndex = 0;
            this.buttonSelectBeatSaber.Text = "Select Beat Saber directory...\r\n";
            this.buttonSelectBeatSaber.UseVisualStyleBackColor = true;
            this.buttonSelectBeatSaber.Click += new System.EventHandler(this.ButtonSelectBeatSaber_Click);
            // 
            // textBoxBeatSaberPath
            // 
            this.textBoxBeatSaberPath.Location = new System.Drawing.Point(173, 55);
            this.textBoxBeatSaberPath.Name = "textBoxBeatSaberPath";
            this.textBoxBeatSaberPath.ReadOnly = true;
            this.textBoxBeatSaberPath.Size = new System.Drawing.Size(407, 20);
            this.textBoxBeatSaberPath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Generate Music Pack";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(301, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "The SongCore mod is required to create a custom music pack.";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonGeneratorSettings);
            this.groupBox2.Controls.Add(this.comboBoxMode);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 476);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(568, 62);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Generator";
            // 
            // buttonGeneratorSettings
            // 
            this.buttonGeneratorSettings.Enabled = false;
            this.buttonGeneratorSettings.Location = new System.Drawing.Point(176, 26);
            this.buttonGeneratorSettings.Name = "buttonGeneratorSettings";
            this.buttonGeneratorSettings.Size = new System.Drawing.Size(122, 23);
            this.buttonGeneratorSettings.TabIndex = 3;
            this.buttonGeneratorSettings.Text = "Generator settings...";
            this.buttonGeneratorSettings.UseVisualStyleBackColor = true;
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGenerate.Location = new System.Drawing.Point(439, 440);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(141, 33);
            this.buttonGenerate.TabIndex = 2;
            this.buttonGenerate.Text = "Generate Music Pack\r\n";
            this.buttonGenerate.UseVisualStyleBackColor = false;
            this.buttonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.Enabled = false;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Items.AddRange(new object[] {
            "360 degrees"});
            this.comboBoxMode.Location = new System.Drawing.Point(49, 27);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(121, 21);
            this.comboBoxMode.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Mode:\r\n";
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
            this.listSongs.Location = new System.Drawing.Point(12, 92);
            this.listSongs.Name = "listSongs";
            this.listSongs.Size = new System.Drawing.Size(568, 342);
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
            this.label3.Location = new System.Drawing.Point(9, 437);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(387, 26);
            this.label3.TabIndex = 7;
            this.label3.Text = "Tip: use shift to select multiple items, then use space to check or uncheck them." +
    "\r\nUse (Ctrl+A) to select all songs and (Ctrl+D) to deselect all songs.";
            // 
            // FormGenerate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 548);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.listSongs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxBeatSaberPath);
            this.Controls.Add(this.buttonSelectBeatSaber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormGenerate";
            this.Text = "Beat-360fyer - A 360 level generator";
            this.Load += new System.EventHandler(this.FormGenerate_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSelectBeatSaber;
        private System.Windows.Forms.TextBox textBoxBeatSaberPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView listSongs;
        private System.Windows.Forms.ColumnHeader columnHeaderSongName;
        private System.Windows.Forms.ColumnHeader columnHeaderAuthor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonGeneratorSettings;
    }
}