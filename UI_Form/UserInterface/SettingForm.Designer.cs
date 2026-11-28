namespace VideoServer.UserInterface
{
    partial class SettingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.gbMotionDetection = new System.Windows.Forms.GroupBox();
            this.btSetDetectionLevel = new System.Windows.Forms.Button();
            this.lbDetectionLevel = new System.Windows.Forms.Label();
            this.gbGlobalPath = new System.Windows.Forms.GroupBox();
            this.rtbDiskInfo = new System.Windows.Forms.RichTextBox();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.btSetDiskPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lbDisk = new System.Windows.Forms.Label();
            this.cbDisk = new System.Windows.Forms.ComboBox();
            this.cbxGlobalPath = new System.Windows.Forms.CheckBox();
            this.gbCamers = new System.Windows.Forms.GroupBox();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.tbSettingFile = new System.Windows.Forms.TextBox();
            this.btAddXmlCameras = new System.Windows.Forms.Button();
            this.dtbDetectionLevel = new DoubleTextBox();
            this.gbMotionDetection.SuspendLayout();
            this.gbGlobalPath.SuspendLayout();
            this.gbCamers.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMotionDetection
            // 
            this.gbMotionDetection.Controls.Add(this.dtbDetectionLevel);
            this.gbMotionDetection.Controls.Add(this.btSetDetectionLevel);
            this.gbMotionDetection.Controls.Add(this.lbDetectionLevel);
            this.gbMotionDetection.Location = new System.Drawing.Point(20, 20);
            this.gbMotionDetection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbMotionDetection.Name = "gbMotionDetection";
            this.gbMotionDetection.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbMotionDetection.Size = new System.Drawing.Size(688, 95);
            this.gbMotionDetection.TabIndex = 0;
            this.gbMotionDetection.TabStop = false;
            this.gbMotionDetection.Text = "Детектор Руху";
            // 
            // btSetDetectionLevel
            // 
            this.btSetDetectionLevel.Location = new System.Drawing.Point(512, 39);
            this.btSetDetectionLevel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btSetDetectionLevel.Name = "btSetDetectionLevel";
            this.btSetDetectionLevel.Size = new System.Drawing.Size(168, 42);
            this.btSetDetectionLevel.TabIndex = 1;
            this.btSetDetectionLevel.Text = "Встановити";
            this.btSetDetectionLevel.UseVisualStyleBackColor = true;
            this.btSetDetectionLevel.Click += new System.EventHandler(this.btSetDetectionLevel_Click);
            // 
            // lbDetectionLevel
            // 
            this.lbDetectionLevel.AutoSize = true;
            this.lbDetectionLevel.Location = new System.Drawing.Point(9, 44);
            this.lbDetectionLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDetectionLevel.Name = "lbDetectionLevel";
            this.lbDetectionLevel.Size = new System.Drawing.Size(219, 25);
            this.lbDetectionLevel.TabIndex = 0;
            this.lbDetectionLevel.Text = "Рівень детектування";
            // 
            // gbGlobalPath
            // 
            this.gbGlobalPath.Controls.Add(this.rtbDiskInfo);
            this.gbGlobalPath.Controls.Add(this.tbPath);
            this.gbGlobalPath.Controls.Add(this.btSetDiskPath);
            this.gbGlobalPath.Controls.Add(this.label2);
            this.gbGlobalPath.Controls.Add(this.lbDisk);
            this.gbGlobalPath.Controls.Add(this.cbDisk);
            this.gbGlobalPath.Controls.Add(this.cbxGlobalPath);
            this.gbGlobalPath.Location = new System.Drawing.Point(20, 125);
            this.gbGlobalPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbGlobalPath.Name = "gbGlobalPath";
            this.gbGlobalPath.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbGlobalPath.Size = new System.Drawing.Size(690, 369);
            this.gbGlobalPath.TabIndex = 1;
            this.gbGlobalPath.TabStop = false;
            // 
            // rtbDiskInfo
            // 
            this.rtbDiskInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbDiskInfo.Location = new System.Drawing.Point(14, 172);
            this.rtbDiskInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbDiskInfo.Name = "rtbDiskInfo";
            this.rtbDiskInfo.ReadOnly = true;
            this.rtbDiskInfo.Size = new System.Drawing.Size(666, 185);
            this.rtbDiskInfo.TabIndex = 7;
            this.rtbDiskInfo.Text = "";
            // 
            // tbPath
            // 
            this.tbPath.Enabled = false;
            this.tbPath.Location = new System.Drawing.Point(177, 102);
            this.tbPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(288, 31);
            this.tbPath.TabIndex = 6;
            this.tbPath.Text = "Video\\";
            // 
            // btSetDiskPath
            // 
            this.btSetDiskPath.Location = new System.Drawing.Point(513, 100);
            this.btSetDiskPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btSetDiskPath.Name = "btSetDiskPath";
            this.btSetDiskPath.Size = new System.Drawing.Size(168, 42);
            this.btSetDiskPath.TabIndex = 5;
            this.btSetDiskPath.Text = "Встановити";
            this.btSetDiskPath.UseVisualStyleBackColor = true;
            this.btSetDiskPath.Click += new System.EventHandler(this.btSetDiskPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 106);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Шлях до текі";
            // 
            // lbDisk
            // 
            this.lbDisk.AutoSize = true;
            this.lbDisk.Location = new System.Drawing.Point(9, 53);
            this.lbDisk.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDisk.Name = "lbDisk";
            this.lbDisk.Size = new System.Drawing.Size(62, 25);
            this.lbDisk.TabIndex = 2;
            this.lbDisk.Text = "Диск";
            // 
            // cbDisk
            // 
            this.cbDisk.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDisk.Enabled = false;
            this.cbDisk.FormattingEnabled = true;
            this.cbDisk.Location = new System.Drawing.Point(177, 41);
            this.cbDisk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbDisk.Name = "cbDisk";
            this.cbDisk.Size = new System.Drawing.Size(116, 33);
            this.cbDisk.TabIndex = 1;
            // 
            // cbxGlobalPath
            // 
            this.cbxGlobalPath.AutoSize = true;
            this.cbxGlobalPath.Location = new System.Drawing.Point(10, 0);
            this.cbxGlobalPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbxGlobalPath.Name = "cbxGlobalPath";
            this.cbxGlobalPath.Size = new System.Drawing.Size(374, 29);
            this.cbxGlobalPath.TabIndex = 0;
            this.cbxGlobalPath.Text = "Зберігати файли в однному місці";
            this.cbxGlobalPath.UseVisualStyleBackColor = true;
            this.cbxGlobalPath.CheckedChanged += new System.EventHandler(this.cbxGlobalPath_CheckedChanged);
            // 
            // gbCamers
            // 
            this.gbCamers.Controls.Add(this.btAddXmlCameras);
            this.gbCamers.Controls.Add(this.btSave);
            this.gbCamers.Controls.Add(this.btLoad);
            this.gbCamers.Location = new System.Drawing.Point(18, 503);
            this.gbCamers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbCamers.Name = "gbCamers";
            this.gbCamers.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbCamers.Size = new System.Drawing.Size(692, 95);
            this.gbCamers.TabIndex = 2;
            this.gbCamers.TabStop = false;
            this.gbCamers.Text = "Файл з камерами";
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(554, 36);
            this.btSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(118, 42);
            this.btSave.TabIndex = 4;
            this.btSave.Text = "Зберегти";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btLoad
            // 
            this.btLoad.Location = new System.Drawing.Point(270, 36);
            this.btLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(134, 42);
            this.btLoad.TabIndex = 1;
            this.btLoad.Text = "Загрузити";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // tbSettingFile
            // 
            this.tbSettingFile.Enabled = false;
            this.tbSettingFile.Location = new System.Drawing.Point(33, 542);
            this.tbSettingFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbSettingFile.Name = "tbSettingFile";
            this.tbSettingFile.Size = new System.Drawing.Size(247, 31);
            this.tbSettingFile.TabIndex = 7;
            // 
            // btAddXmlCameras
            // 
            this.btAddXmlCameras.Location = new System.Drawing.Point(412, 36);
            this.btAddXmlCameras.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btAddXmlCameras.Name = "btAddXmlCameras";
            this.btAddXmlCameras.Size = new System.Drawing.Size(134, 42);
            this.btAddXmlCameras.TabIndex = 5;
            this.btAddXmlCameras.Text = "Додати";
            this.btAddXmlCameras.UseVisualStyleBackColor = true;
            this.btAddXmlCameras.Click += new System.EventHandler(this.btAddXmlCameras_Click);
            // 
            // dtbDetectionLevel
            // 
            this.dtbDetectionLevel.Location = new System.Drawing.Point(242, 39);
            this.dtbDetectionLevel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtbDetectionLevel.Maximum = 1.7976931348623157E+308D;
            this.dtbDetectionLevel.Minimum = -1.7976931348623157E+308D;
            this.dtbDetectionLevel.Name = "dtbDetectionLevel";
            this.dtbDetectionLevel.Size = new System.Drawing.Size(148, 31);
            this.dtbDetectionLevel.TabIndex = 3;
            this.dtbDetectionLevel.Value = null;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.ClientSize = new System.Drawing.Size(726, 620);
            this.Controls.Add(this.tbSettingFile);
            this.Controls.Add(this.gbCamers);
            this.Controls.Add(this.gbGlobalPath);
            this.Controls.Add(this.gbMotionDetection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Налаштування";
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.gbMotionDetection.ResumeLayout(false);
            this.gbMotionDetection.PerformLayout();
            this.gbGlobalPath.ResumeLayout(false);
            this.gbGlobalPath.PerformLayout();
            this.gbCamers.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbMotionDetection;
        private System.Windows.Forms.Label lbDetectionLevel;
        private System.Windows.Forms.GroupBox gbGlobalPath;
        private System.Windows.Forms.Button btSetDetectionLevel;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Button btSetDiskPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbDisk;
        private System.Windows.Forms.ComboBox cbDisk;
        private System.Windows.Forms.CheckBox cbxGlobalPath;
        private System.Windows.Forms.RichTextBox rtbDiskInfo;
        private DoubleTextBox dtbDetectionLevel;
        private System.Windows.Forms.GroupBox gbCamers;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.TextBox tbSettingFile;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btAddXmlCameras;
    }
}