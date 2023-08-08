namespace Hearthstone_Rank_Finder
{
    partial class RankLookup
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                StartStalkingButton.Click += new System.EventHandler(this.StartStalkingButton_Click);
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RankLookup));
            SearchPlayerLabel = new Label();
            SearchPlayerTextBox = new TextBox();
            CurrentRankRichTextBox = new RichTextBox();
            SelectModeComboBox = new ComboBox();
            SelectRegionComboBox = new ComboBox();
            SelectModeLabel = new Label();
            SelectRegionLabel = new Label();
            SelectSeasonLabel = new Label();
            SelectSeasonTextBox = new TextBox();
            SearchButton = new Button();
            LogRichTextbox = new RichTextBox();
            StartStalkingButton = new Button();
            StopStalkingButton = new Button();
            StalkingTimer = new System.Windows.Forms.Timer(components);
            LogsLabel = new Label();
            StopSearchingButton = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // SearchPlayerLabel
            // 
            SearchPlayerLabel.AutoSize = true;
            SearchPlayerLabel.BackColor = Color.Transparent;
            SearchPlayerLabel.ForeColor = Color.Transparent;
            SearchPlayerLabel.Location = new Point(10, 110);
            SearchPlayerLabel.Name = "SearchPlayerLabel";
            SearchPlayerLabel.Size = new Size(80, 15);
            SearchPlayerLabel.TabIndex = 0;
            SearchPlayerLabel.Text = "Search Player:";
            // 
            // SearchPlayerTextBox
            // 
            SearchPlayerTextBox.Location = new Point(99, 107);
            SearchPlayerTextBox.Name = "SearchPlayerTextBox";
            SearchPlayerTextBox.Size = new Size(121, 23);
            SearchPlayerTextBox.TabIndex = 4;
            // 
            // CurrentRankRichTextBox
            // 
            CurrentRankRichTextBox.BackColor = SystemColors.ControlLightLight;
            CurrentRankRichTextBox.Location = new Point(10, 165);
            CurrentRankRichTextBox.Name = "CurrentRankRichTextBox";
            CurrentRankRichTextBox.ReadOnly = true;
            CurrentRankRichTextBox.Size = new Size(210, 36);
            CurrentRankRichTextBox.TabIndex = 3;
            CurrentRankRichTextBox.TabStop = false;
            CurrentRankRichTextBox.Text = "";
            CurrentRankRichTextBox.TextChanged += CurrentRankRichTextBox_TextChanged;
            // 
            // SelectModeComboBox
            // 
            SelectModeComboBox.FormattingEnabled = true;
            SelectModeComboBox.Items.AddRange(new object[] { "Standard", "Wild", "Twist", "Battlegrounds", "Arena" });
            SelectModeComboBox.Location = new Point(99, 78);
            SelectModeComboBox.Name = "SelectModeComboBox";
            SelectModeComboBox.Size = new Size(121, 23);
            SelectModeComboBox.TabIndex = 3;
            // 
            // SelectRegionComboBox
            // 
            SelectRegionComboBox.FormattingEnabled = true;
            SelectRegionComboBox.Items.AddRange(new object[] { "Americas", "Asia-Pacific", "Europe" });
            SelectRegionComboBox.Location = new Point(99, 49);
            SelectRegionComboBox.Name = "SelectRegionComboBox";
            SelectRegionComboBox.Size = new Size(121, 23);
            SelectRegionComboBox.TabIndex = 2;
            // 
            // SelectModeLabel
            // 
            SelectModeLabel.AutoSize = true;
            SelectModeLabel.BackColor = Color.Transparent;
            SelectModeLabel.ForeColor = Color.Transparent;
            SelectModeLabel.Location = new Point(10, 78);
            SelectModeLabel.Name = "SelectModeLabel";
            SelectModeLabel.Size = new Size(75, 15);
            SelectModeLabel.TabIndex = 7;
            SelectModeLabel.Text = "Select Mode:";
            // 
            // SelectRegionLabel
            // 
            SelectRegionLabel.AutoSize = true;
            SelectRegionLabel.BackColor = Color.Transparent;
            SelectRegionLabel.ForeColor = SystemColors.ButtonFace;
            SelectRegionLabel.Location = new Point(10, 52);
            SelectRegionLabel.Name = "SelectRegionLabel";
            SelectRegionLabel.Size = new Size(81, 15);
            SelectRegionLabel.TabIndex = 8;
            SelectRegionLabel.Text = "Select Region:";
            // 
            // SelectSeasonLabel
            // 
            SelectSeasonLabel.AutoSize = true;
            SelectSeasonLabel.BackColor = Color.Transparent;
            SelectSeasonLabel.ForeColor = SystemColors.ButtonFace;
            SelectSeasonLabel.Location = new Point(10, 23);
            SelectSeasonLabel.Name = "SelectSeasonLabel";
            SelectSeasonLabel.Size = new Size(81, 15);
            SelectSeasonLabel.TabIndex = 9;
            SelectSeasonLabel.Text = "Select Season:";
            // 
            // SelectSeasonTextBox
            // 
            SelectSeasonTextBox.Location = new Point(99, 20);
            SelectSeasonTextBox.Name = "SelectSeasonTextBox";
            SelectSeasonTextBox.Size = new Size(121, 23);
            SelectSeasonTextBox.TabIndex = 1;
            // 
            // SearchButton
            // 
            SearchButton.BackColor = SystemColors.ButtonHighlight;
            SearchButton.FlatStyle = FlatStyle.System;
            SearchButton.Location = new Point(99, 136);
            SearchButton.Name = "SearchButton";
            SearchButton.Size = new Size(119, 23);
            SearchButton.TabIndex = 5;
            SearchButton.Text = "Search";
            SearchButton.UseVisualStyleBackColor = false;
            SearchButton.Click += SearchButton_Click;
            // 
            // LogRichTextbox
            // 
            LogRichTextbox.BackColor = SystemColors.ControlLightLight;
            LogRichTextbox.Location = new Point(225, 20);
            LogRichTextbox.Name = "LogRichTextbox";
            LogRichTextbox.ReadOnly = true;
            LogRichTextbox.Size = new Size(246, 208);
            LogRichTextbox.TabIndex = 12;
            LogRichTextbox.TabStop = false;
            LogRichTextbox.Text = "";
            LogRichTextbox.WordWrap = false;
            // 
            // StartStalkingButton
            // 
            StartStalkingButton.BackColor = SystemColors.ButtonHighlight;
            StartStalkingButton.Enabled = false;
            StartStalkingButton.FlatStyle = FlatStyle.System;
            StartStalkingButton.Location = new Point(10, 206);
            StartStalkingButton.Margin = new Padding(2, 1, 2, 1);
            StartStalkingButton.Name = "StartStalkingButton";
            StartStalkingButton.Size = new Size(93, 22);
            StartStalkingButton.TabIndex = 6;
            StartStalkingButton.Text = "Start Stalking";
            StartStalkingButton.UseVisualStyleBackColor = false;
            StartStalkingButton.Click += StartStalkingButton_Click_1;
            // 
            // StopStalkingButton
            // 
            StopStalkingButton.BackColor = SystemColors.ButtonHighlight;
            StopStalkingButton.Enabled = false;
            StopStalkingButton.FlatStyle = FlatStyle.System;
            StopStalkingButton.Location = new Point(127, 206);
            StopStalkingButton.Margin = new Padding(2, 1, 2, 1);
            StopStalkingButton.Name = "StopStalkingButton";
            StopStalkingButton.Size = new Size(92, 22);
            StopStalkingButton.TabIndex = 7;
            StopStalkingButton.Text = "Stop Stalking";
            StopStalkingButton.UseVisualStyleBackColor = false;
            StopStalkingButton.Click += StopStalkingButton_Click;
            // 
            // StalkingTimer
            // 
            StalkingTimer.Tick += StalkingTimer_Tick;
            // 
            // LogsLabel
            // 
            LogsLabel.AutoSize = true;
            LogsLabel.BackColor = Color.Transparent;
            LogsLabel.ForeColor = Color.Transparent;
            LogsLabel.Location = new Point(226, 2);
            LogsLabel.Name = "LogsLabel";
            LogsLabel.Size = new Size(32, 15);
            LogsLabel.TabIndex = 15;
            LogsLabel.Text = "Logs";
            // 
            // StopSearchingButton
            // 
            StopSearchingButton.BackColor = SystemColors.ButtonHighlight;
            StopSearchingButton.Enabled = false;
            StopSearchingButton.FlatStyle = FlatStyle.System;
            StopSearchingButton.Location = new Point(12, 137);
            StopSearchingButton.Margin = new Padding(2, 1, 2, 1);
            StopSearchingButton.Name = "StopSearchingButton";
            StopSearchingButton.Size = new Size(81, 22);
            StopSearchingButton.TabIndex = 16;
            StopSearchingButton.Text = "Cancel";
            StopSearchingButton.UseVisualStyleBackColor = false;
            StopSearchingButton.Click += StopSearchingButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.ForeColor = SystemColors.ButtonFace;
            label1.Location = new Point(358, 2);
            label1.Name = "label1";
            label1.Size = new Size(101, 15);
            label1.TabIndex = 17;
            label1.Text = "created by olanga";
            // 
            // RankLookup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(483, 233);
            Controls.Add(label1);
            Controls.Add(StopSearchingButton);
            Controls.Add(LogsLabel);
            Controls.Add(StopStalkingButton);
            Controls.Add(StartStalkingButton);
            Controls.Add(LogRichTextbox);
            Controls.Add(SearchButton);
            Controls.Add(SelectSeasonTextBox);
            Controls.Add(SelectSeasonLabel);
            Controls.Add(SelectRegionLabel);
            Controls.Add(SelectModeLabel);
            Controls.Add(SelectRegionComboBox);
            Controls.Add(SelectModeComboBox);
            Controls.Add(CurrentRankRichTextBox);
            Controls.Add(SearchPlayerTextBox);
            Controls.Add(SearchPlayerLabel);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "RankLookup";
            Text = "Rank Finder";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label SearchPlayerLabel;
        private TextBox SearchPlayerTextBox;
        private Label CurrentRankLabel;
        private RichTextBox CurrentRankRichTextBox;
        private ComboBox SelectModeComboBox;
        private ComboBox SelectRegionComboBox;
        private Label SelectModeLabel;
        private Label SelectRegionLabel;
        private Label SelectSeasonLabel;
        private TextBox SelectSeasonTextBox;
        private Button SearchButton;
        private RichTextBox LogRichTextbox;
        private Button StartStalkingButton;
        private Button StopStalkingButton;
        private System.Windows.Forms.Timer StalkingTimer;
        private Label LogsLabel;
        private Button StopSearchingButton;
        private Label label1;
    }
}