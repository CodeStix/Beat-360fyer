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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGeneratePack));
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Songs", System.Windows.Forms.HorizontalAlignment.Left);
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
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSelectBeatSaber
            // 
            this.buttonSelectBeatSaber.Location = new System.Drawing.Point(12, 77);
            this.buttonSelectBeatSaber.Name = "buttonSelectBeatSaber";
            this.buttonSelectBeatSaber.Size = new System.Drawing.Size(155, 23);
            this.buttonSelectBeatSaber.TabIndex = 0;
            this.buttonSelectBeatSaber.Text = "Select Beat Saber directory...\r\n";
            this.buttonSelectBeatSaber.UseVisualStyleBackColor = true;
            this.buttonSelectBeatSaber.Click += new System.EventHandler(this.ButtonSelectBeatSaber_Click);
            // 
            // textBoxBeatSaberPath
            // 
            this.textBoxBeatSaberPath.Location = new System.Drawing.Point(173, 79);
            this.textBoxBeatSaberPath.Name = "textBoxBeatSaberPath";
            this.textBoxBeatSaberPath.ReadOnly = true;
            this.textBoxBeatSaberPath.Size = new System.Drawing.Size(335, 20);
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
            this.label2.Size = new System.Drawing.Size(458, 39);
            this.label2.TabIndex = 4;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // buttonGeneratorSettings
            // 
            this.buttonGeneratorSettings.Location = new System.Drawing.Point(386, 503);
            this.buttonGeneratorSettings.Name = "buttonGeneratorSettings";
            this.buttonGeneratorSettings.Size = new System.Drawing.Size(122, 23);
            this.buttonGeneratorSettings.TabIndex = 3;
            this.buttonGeneratorSettings.Text = "Generator settings...";
            this.buttonGeneratorSettings.UseVisualStyleBackColor = true;
            this.buttonGeneratorSettings.Click += new System.EventHandler(this.buttonGeneratorSettings_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGenerate.Location = new System.Drawing.Point(367, 464);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(141, 33);
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
            listViewGroup3.Header = "Songs";
            listViewGroup3.Name = "listViewGroup1";
            this.listSongs.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3});
            this.listSongs.HideSelection = false;
            this.listSongs.Location = new System.Drawing.Point(12, 116);
            this.listSongs.Name = "listSongs";
            this.listSongs.Size = new System.Drawing.Size(496, 342);
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
            this.label3.Location = new System.Drawing.Point(9, 461);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(352, 65);
            this.label3.TabIndex = 7;
            this.label3.Text = "Tip: use shift to select multiple items, then use space to check or uncheck them." +
    "\r\nUse (Ctrl+A) to select all songs and (Ctrl+D) to deselect all songs.";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 566);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(521, 22);
            this.statusStrip1.TabIndex = 14;
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
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // buttonUpdatePack
            // 
            this.buttonUpdatePack.Location = new System.Drawing.Point(321, 9);
            this.buttonUpdatePack.Name = "buttonUpdatePack";
            this.buttonUpdatePack.Size = new System.Drawing.Size(188, 23);
            this.buttonUpdatePack.TabIndex = 18;
            this.buttonUpdatePack.Text = "Update previous generated pack";
            this.buttonUpdatePack.UseVisualStyleBackColor = true;
            this.buttonUpdatePack.Click += new System.EventHandler(this.buttonUpdatePack_Click);
            // 
            // checkBoxForceGenerate
            // 
            this.checkBoxForceGenerate.AutoSize = true;
            this.checkBoxForceGenerate.Location = new System.Drawing.Point(12, 529);
            this.checkBoxForceGenerate.Name = "checkBoxForceGenerate";
            this.checkBoxForceGenerate.Size = new System.Drawing.Size(167, 30);
            this.checkBoxForceGenerate.TabIndex = 19;
            this.checkBoxForceGenerate.Text = "Force generate\r\n(regenerates up-to-date maps)\r\n";
            this.checkBoxForceGenerate.UseVisualStyleBackColor = true;
            // 
            // textBoxPackName
            // 
            this.textBoxPackName.Location = new System.Drawing.Point(82, 505);
            this.textBoxPackName.MaxLength = 32;
            this.textBoxPackName.Name = "textBoxPackName";
            this.textBoxPackName.Size = new System.Drawing.Size(138, 20);
            this.textBoxPackName.TabIndex = 20;
            this.textBoxPackName.Text = "Generated 360 Levels";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 508);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Pack name:";
            // 
            // FormGeneratePack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 588);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxPackName);
            this.Controls.Add(this.checkBoxForceGenerate);
            this.Controls.Add(this.buttonUpdatePack);
            this.Controls.Add(this.buttonGeneratorSettings);
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
            this.Load += new System.EventHandler(this.FormGenerate_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
    }
}