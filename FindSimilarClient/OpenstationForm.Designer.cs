using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.Misc;
using System.Linq;

namespace FindSimilar2
{
	partial class OpenstationForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#region Private Fields

		private System.Windows.Forms.Button buttonAddFile;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ProgressBar progressBarLeft;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBarRight;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelTime;
		private ListBox listBoxPlaylist;
		private System.Windows.Forms.Timer timerUpdate;
		private Label labelCurrentTrack;
		private Label labelRemain;
		private Label label3;
		private Label label4;
		private Button buttonSetEnvelope;
		private Button buttonRemoveEnvelope;
		private Button BtOpenSchedule;
		private OpenFileDialog OfdOpenSchedule;
		private Button BtPlay;
		private Button BtToggleZoom;
		private Button BtStop;
		private Panel panel1;
		private Panel Info;
		private Panel panel3;
		private Panel panel4;
		private Panel Buttons;
		private Panel panel6;
		private PictureBox PbWaveform2;
		private Button BtRemoveTrack;
		private Panel PanelLibrary;
		private DataGridView DgvLibrary;
		private DataGridViewTextBoxColumn fileDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn startDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn introDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn segueDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn endDataGridViewTextBoxColumn;
		private Panel PanelLibraryDetail;
		private PictureBox EditorPB;
		private Button button1;
		private OpenFileDialog AddTrackToLibraryOFD;
		private System.Windows.Forms.PictureBox UIWaveFormPB;
		private Panel panel2;
		private Label SegueTimeLBL;
		private Label label10;
		private Label label9;
		private Button CueStartTimeBTN;
		private NumericUpDown StartTimeNUD;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private NumericUpDown EditorSegueTimeNUD;
		private Button EditorSegueTimeCueBTN;
		private NumericUpDown EditorStartTimeNUD;
		private Button EditorStartTimeCueBTN;
		private Label label6;
		private Label label7;
		private TextBox EditorArtistTitleTB;
		private Button EditorPlayBTN;
		private Button EditorStopBTN;
		private WaveForm TrackWF;
		private WaveForm EditorWf;
		private System.Windows.Forms.Timer EditorTimer;
		private TabControl tabControl1;
		private TabPage tabPage1;
		private TabPage tabPage2;
		private DataGridView ScheduleDgw;
		private DataGridViewTextBoxColumn airtimeDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn itemTitleDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn itemFilenameDataGridViewTextBoxColumn;
		private System.Windows.Forms.Timer timerUpdateSeconds;
		private BackgroundWorker AddScheduleItemToPlayerBGW;
		private Track EditorTrack;
		private System.Windows.Forms.BindingSource libraryBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn lengthDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn lengthDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn songDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn trackLengthDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn channelDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn filenameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn channelsDataGridViewTextBoxColumn;
		#endregion
		
		#region Windows Form Designer generated Code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.buttonAddFile = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.progressBarLeft = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.progressBarRight = new System.Windows.Forms.ProgressBar();
			this.label2 = new System.Windows.Forms.Label();
			this.labelTime = new System.Windows.Forms.Label();
			this.UIWaveFormPB = new System.Windows.Forms.PictureBox();
			this.listBoxPlaylist = new System.Windows.Forms.ListBox();
			this.timerUpdate = new System.Windows.Forms.Timer(this.components);
			this.labelCurrentTrack = new System.Windows.Forms.Label();
			this.labelRemain = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonSetEnvelope = new System.Windows.Forms.Button();
			this.buttonRemoveEnvelope = new System.Windows.Forms.Button();
			this.BtOpenSchedule = new System.Windows.Forms.Button();
			this.OfdOpenSchedule = new System.Windows.Forms.OpenFileDialog();
			this.BtPlay = new System.Windows.Forms.Button();
			this.BtToggleZoom = new System.Windows.Forms.Button();
			this.BtStop = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.PanelLibrary = new System.Windows.Forms.Panel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.ScheduleDgw = new System.Windows.Forms.DataGridView();
			this.lengthDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.libraryBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.PanelLibraryDetail = new System.Windows.Forms.Panel();
			this.EditorPlayBTN = new System.Windows.Forms.Button();
			this.EditorStopBTN = new System.Windows.Forms.Button();
			this.EditorSegueTimeNUD = new System.Windows.Forms.NumericUpDown();
			this.EditorSegueTimeCueBTN = new System.Windows.Forms.Button();
			this.EditorStartTimeNUD = new System.Windows.Forms.NumericUpDown();
			this.EditorStartTimeCueBTN = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.EditorArtistTitleTB = new System.Windows.Forms.TextBox();
			this.EditorPB = new System.Windows.Forms.PictureBox();
			this.DgvLibrary = new System.Windows.Forms.DataGridView();
			this.songDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.trackLengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.channelDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.filenameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.channelsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Buttons = new System.Windows.Forms.Panel();
			this.BtRemoveTrack = new System.Windows.Forms.Button();
			this.Info = new System.Windows.Forms.Panel();
			this.PbWaveform2 = new System.Windows.Forms.PictureBox();
			this.panel6 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.StartTimeNUD = new System.Windows.Forms.NumericUpDown();
			this.CueStartTimeBTN = new System.Windows.Forms.Button();
			this.SegueTimeLBL = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.AddTrackToLibraryOFD = new System.Windows.Forms.OpenFileDialog();
			this.EditorTimer = new System.Windows.Forms.Timer(this.components);
			this.timerUpdateSeconds = new System.Windows.Forms.Timer(this.components);
			this.AddScheduleItemToPlayerBGW = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.UIWaveFormPB)).BeginInit();
			this.panel1.SuspendLayout();
			this.PanelLibrary.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScheduleDgw)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.libraryBindingSource)).BeginInit();
			this.tabPage2.SuspendLayout();
			this.PanelLibraryDetail.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EditorSegueTimeNUD)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EditorStartTimeNUD)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EditorPB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DgvLibrary)).BeginInit();
			this.Buttons.SuspendLayout();
			this.Info.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PbWaveform2)).BeginInit();
			this.panel6.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartTimeNUD)).BeginInit();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonAddFile
			// 
			this.buttonAddFile.Location = new System.Drawing.Point(911, 15);
			this.buttonAddFile.Name = "buttonAddFile";
			this.buttonAddFile.Size = new System.Drawing.Size(147, 23);
			this.buttonAddFile.TabIndex = 0;
			this.buttonAddFile.Text = "Add Track to Playlist";
			this.buttonAddFile.Visible = true;
			this.buttonAddFile.Click += new System.EventHandler(this.buttonAddFile_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.Filter = "Audio Files (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav";
			this.openFileDialog.Title = "Select an audio file to play";
			// 
			// progressBarLeft
			// 
			this.progressBarLeft.Location = new System.Drawing.Point(158, 12);
			this.progressBarLeft.Maximum = 32768;
			this.progressBarLeft.Name = "progressBarLeft";
			this.progressBarLeft.Size = new System.Drawing.Size(181, 12);
			this.progressBarLeft.Step = 1;
			this.progressBarLeft.TabIndex = 9;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(141, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(16, 12);
			this.label1.TabIndex = 8;
			this.label1.Text = "L";
			// 
			// progressBarRight
			// 
			this.progressBarRight.Location = new System.Drawing.Point(158, 28);
			this.progressBarRight.Maximum = 32768;
			this.progressBarRight.Name = "progressBarRight";
			this.progressBarRight.Size = new System.Drawing.Size(181, 12);
			this.progressBarRight.Step = 1;
			this.progressBarRight.TabIndex = 11;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(141, 28);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(16, 12);
			this.label2.TabIndex = 10;
			this.label2.Text = "R";
			// 
			// labelTime
			// 
			this.labelTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTime.Location = new System.Drawing.Point(0, 24);
			this.labelTime.Name = "labelTime";
			this.labelTime.Size = new System.Drawing.Size(65, 15);
			this.labelTime.TabIndex = 12;
			this.labelTime.Text = "00:00:00";
			this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UIWaveFormPB
			// 
			this.UIWaveFormPB.BackColor = System.Drawing.Color.WhiteSmoke;
			this.UIWaveFormPB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.UIWaveFormPB.Dock = System.Windows.Forms.DockStyle.Top;
			this.UIWaveFormPB.ErrorImage = null;
			this.UIWaveFormPB.InitialImage = null;
			this.UIWaveFormPB.Location = new System.Drawing.Point(0, 0);
			this.UIWaveFormPB.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
			this.UIWaveFormPB.Name = "UIWaveFormPB";
			this.UIWaveFormPB.Size = new System.Drawing.Size(1152, 60);
			this.UIWaveFormPB.TabIndex = 15;
			this.UIWaveFormPB.TabStop = false;
			this.UIWaveFormPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxWaveForm_MouseDown);
			// 
			// listBoxPlaylist
			// 
			this.listBoxPlaylist.Dock = System.Windows.Forms.DockStyle.Top;
			this.listBoxPlaylist.FormattingEnabled = true;
			this.listBoxPlaylist.Location = new System.Drawing.Point(15, 186);
			this.listBoxPlaylist.Name = "listBoxPlaylist";
			this.listBoxPlaylist.Size = new System.Drawing.Size(1152, 56);
			this.listBoxPlaylist.TabIndex = 16;
			// 
			// timerUpdate
			// 
			this.timerUpdate.Interval = 50;
			this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
			// 
			// labelCurrentTrack
			// 
			this.labelCurrentTrack.BackColor = System.Drawing.Color.Transparent;
			this.labelCurrentTrack.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCurrentTrack.Location = new System.Drawing.Point(3, 4);
			this.labelCurrentTrack.Name = "labelCurrentTrack";
			this.labelCurrentTrack.Size = new System.Drawing.Size(599, 29);
			this.labelCurrentTrack.TabIndex = 10;
			// 
			// labelRemain
			// 
			this.labelRemain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRemain.Location = new System.Drawing.Point(70, 24);
			this.labelRemain.Name = "labelRemain";
			this.labelRemain.Size = new System.Drawing.Size(65, 15);
			this.labelRemain.TabIndex = 12;
			this.labelRemain.Text = "00:00:00";
			this.labelRemain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Elapsed";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(76, 4);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(42, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Remain";
			// 
			// buttonSetEnvelope
			// 
			this.buttonSetEnvelope.Location = new System.Drawing.Point(611, 15);
			this.buttonSetEnvelope.Name = "buttonSetEnvelope";
			this.buttonSetEnvelope.Size = new System.Drawing.Size(110, 23);
			this.buttonSetEnvelope.TabIndex = 0;
			this.buttonSetEnvelope.Text = "Set Envelope";
			this.buttonSetEnvelope.Visible = true;
			this.buttonSetEnvelope.Click += new System.EventHandler(this.buttonSetEnvelope_Click);
			// 
			// buttonRemoveEnvelope
			// 
			this.buttonRemoveEnvelope.Location = new System.Drawing.Point(611, 59);
			this.buttonRemoveEnvelope.Name = "buttonRemoveEnvelope";
			this.buttonRemoveEnvelope.Size = new System.Drawing.Size(110, 23);
			this.buttonRemoveEnvelope.TabIndex = 0;
			this.buttonRemoveEnvelope.Text = "Remove Envelope";
			this.buttonRemoveEnvelope.Visible = true;
			this.buttonRemoveEnvelope.Click += new System.EventHandler(this.buttonRemoveEnvelope_Click);
			// 
			// BtOpenSchedule
			// 
			this.BtOpenSchedule.Location = new System.Drawing.Point(749, 15);
			this.BtOpenSchedule.Name = "BtOpenSchedule";
			this.BtOpenSchedule.Size = new System.Drawing.Size(110, 23);
			this.BtOpenSchedule.TabIndex = 17;
			this.BtOpenSchedule.Text = "Open Playlist";
			this.BtOpenSchedule.UseVisualStyleBackColor = true;
			this.BtOpenSchedule.Visible = true;
			this.BtOpenSchedule.Click += new System.EventHandler(this.BtOpenSchedule_Click);
			// 
			// OfdOpenSchedule
			// 
			this.OfdOpenSchedule.FileName = "openFileDialog1";
			// 
			// BtPlay
			// 
			this.BtPlay.Location = new System.Drawing.Point(18, 15);
			this.BtPlay.Name = "BtPlay";
			this.BtPlay.Size = new System.Drawing.Size(110, 23);
			this.BtPlay.TabIndex = 18;
			this.BtPlay.Text = "Play";
			this.BtPlay.UseVisualStyleBackColor = true;
			this.BtPlay.Click += new System.EventHandler(this.BtPlay_Click);
			// 
			// BtToggleZoom
			// 
			this.BtToggleZoom.Location = new System.Drawing.Point(749, 59);
			this.BtToggleZoom.Name = "BtToggleZoom";
			this.BtToggleZoom.Size = new System.Drawing.Size(110, 23);
			this.BtToggleZoom.TabIndex = 19;
			this.BtToggleZoom.Text = "Toggle Zoom";
			this.BtToggleZoom.UseVisualStyleBackColor = true;
			this.BtToggleZoom.Visible = true;
			this.BtToggleZoom.Click += new System.EventHandler(this.BtToggleZoom_Click);
			// 
			// BtStop
			// 
			this.BtStop.Location = new System.Drawing.Point(18, 59);
			this.BtStop.Name = "BtStop";
			this.BtStop.Size = new System.Drawing.Size(110, 23);
			this.BtStop.TabIndex = 20;
			this.BtStop.Text = "Stop";
			this.BtStop.UseVisualStyleBackColor = true;
			this.BtStop.Click += new System.EventHandler(this.BtStop_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.PanelLibrary);
			this.panel1.Controls.Add(this.Buttons);
			this.panel1.Controls.Add(this.listBoxPlaylist);
			this.panel1.Controls.Add(this.Info);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(15);
			this.panel1.Size = new System.Drawing.Size(1182, 741);
			this.panel1.TabIndex = 0;
			// 
			// PanelLibrary
			// 
			this.PanelLibrary.Controls.Add(this.tabControl1);
			this.PanelLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelLibrary.Location = new System.Drawing.Point(15, 343);
			this.PanelLibrary.Name = "PanelLibrary";
			this.PanelLibrary.Padding = new System.Windows.Forms.Padding(15);
			this.PanelLibrary.Size = new System.Drawing.Size(1152, 383);
			this.PanelLibrary.TabIndex = 23;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(15, 15);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1122, 353);
			this.tabControl1.TabIndex = 11;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.ScheduleDgw);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(1114, 327);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Schedule";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// ScheduleDgw
			// 
			this.ScheduleDgw.AllowUserToAddRows = false;
			this.ScheduleDgw.AllowUserToDeleteRows = false;
			this.ScheduleDgw.AutoGenerateColumns = false;
			this.ScheduleDgw.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.ScheduleDgw.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.ScheduleDgw.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			                                  	this.lengthDataGridViewTextBoxColumn1});
			this.ScheduleDgw.DataMember = "Schedule";
			this.ScheduleDgw.DataSource = this.libraryBindingSource;
			this.ScheduleDgw.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScheduleDgw.Location = new System.Drawing.Point(3, 3);
			this.ScheduleDgw.Name = "ScheduleDgw";
			this.ScheduleDgw.Size = new System.Drawing.Size(1108, 321);
			this.ScheduleDgw.TabIndex = 0;
			// 
			// lengthDataGridViewTextBoxColumn1
			// 
			this.lengthDataGridViewTextBoxColumn1.DataPropertyName = "Length";
			this.lengthDataGridViewTextBoxColumn1.HeaderText = "Length";
			this.lengthDataGridViewTextBoxColumn1.Name = "lengthDataGridViewTextBoxColumn1";
			this.lengthDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// libraryBindingSource
			// 
			this.libraryBindingSource.AllowNew = false;
			this.libraryBindingSource.DataSource = typeof(FindSimilar2.OpenstationForm.Library);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.PanelLibraryDetail);
			this.tabPage2.Controls.Add(this.DgvLibrary);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(1114, 327);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Library";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// PanelLibraryDetail
			// 
			this.PanelLibraryDetail.Controls.Add(this.EditorPlayBTN);
			this.PanelLibraryDetail.Controls.Add(this.EditorStopBTN);
			this.PanelLibraryDetail.Controls.Add(this.EditorSegueTimeNUD);
			this.PanelLibraryDetail.Controls.Add(this.EditorSegueTimeCueBTN);
			this.PanelLibraryDetail.Controls.Add(this.EditorStartTimeNUD);
			this.PanelLibraryDetail.Controls.Add(this.EditorStartTimeCueBTN);
			this.PanelLibraryDetail.Controls.Add(this.label6);
			this.PanelLibraryDetail.Controls.Add(this.label7);
			this.PanelLibraryDetail.Controls.Add(this.button1);
			this.PanelLibraryDetail.Controls.Add(this.EditorArtistTitleTB);
			this.PanelLibraryDetail.Controls.Add(this.EditorPB);
			this.PanelLibraryDetail.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelLibraryDetail.Location = new System.Drawing.Point(397, 3);
			this.PanelLibraryDetail.Margin = new System.Windows.Forms.Padding(0);
			this.PanelLibraryDetail.Name = "PanelLibraryDetail";
			this.PanelLibraryDetail.Padding = new System.Windows.Forms.Padding(15, 0, 15, 15);
			this.PanelLibraryDetail.Size = new System.Drawing.Size(714, 321);
			this.PanelLibraryDetail.TabIndex = 10;
			this.PanelLibraryDetail.Visible = true;
			// 
			// EditorPlayBTN
			// 
			this.EditorPlayBTN.Location = new System.Drawing.Point(574, 267);
			this.EditorPlayBTN.Name = "EditorPlayBTN";
			this.EditorPlayBTN.Size = new System.Drawing.Size(136, 23);
			this.EditorPlayBTN.TabIndex = 25;
			this.EditorPlayBTN.Text = "Play";
			this.EditorPlayBTN.UseVisualStyleBackColor = true;
			this.EditorPlayBTN.Click += new System.EventHandler(this.EditorPlayBTN_Click);
			// 
			// EditorStopBTN
			// 
			this.EditorStopBTN.Location = new System.Drawing.Point(574, 296);
			this.EditorStopBTN.Name = "EditorStopBTN";
			this.EditorStopBTN.Size = new System.Drawing.Size(136, 23);
			this.EditorStopBTN.TabIndex = 26;
			this.EditorStopBTN.Text = "Stop";
			this.EditorStopBTN.UseVisualStyleBackColor = true;
			this.EditorStopBTN.Click += new System.EventHandler(this.EditorStopBTN_Click);
			// 
			// EditorSegueTimeNUD
			// 
			this.EditorSegueTimeNUD.Location = new System.Drawing.Point(81, 288);
			this.EditorSegueTimeNUD.Maximum = new decimal(new int[] {
			                                              	1410065407,
			                                              	2,
			                                              	0,
			                                              	0});
			this.EditorSegueTimeNUD.Name = "EditorSegueTimeNUD";
			this.EditorSegueTimeNUD.Size = new System.Drawing.Size(72, 21);
			this.EditorSegueTimeNUD.TabIndex = 24;
			// 
			// EditorSegueTimeCueBTN
			// 
			this.EditorSegueTimeCueBTN.Location = new System.Drawing.Point(153, 287);
			this.EditorSegueTimeCueBTN.Name = "EditorSegueTimeCueBTN";
			this.EditorSegueTimeCueBTN.Size = new System.Drawing.Size(38, 23);
			this.EditorSegueTimeCueBTN.TabIndex = 23;
			this.EditorSegueTimeCueBTN.Text = "Cue";
			this.EditorSegueTimeCueBTN.UseVisualStyleBackColor = true;
			// 
			// EditorStartTimeNUD
			// 
			this.EditorStartTimeNUD.Location = new System.Drawing.Point(81, 241);
			this.EditorStartTimeNUD.Maximum = new decimal(new int[] {
			                                              	1410065407,
			                                              	2,
			                                              	0,
			                                              	0});
			this.EditorStartTimeNUD.Name = "EditorStartTimeNUD";
			this.EditorStartTimeNUD.Size = new System.Drawing.Size(72, 21);
			this.EditorStartTimeNUD.TabIndex = 22;
			// 
			// EditorStartTimeCueBTN
			// 
			this.EditorStartTimeCueBTN.Location = new System.Drawing.Point(153, 240);
			this.EditorStartTimeCueBTN.Name = "EditorStartTimeCueBTN";
			this.EditorStartTimeCueBTN.Size = new System.Drawing.Size(38, 23);
			this.EditorStartTimeCueBTN.TabIndex = 21;
			this.EditorStartTimeCueBTN.Text = "Cue";
			this.EditorStartTimeCueBTN.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 290);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(62, 13);
			this.label6.TabIndex = 19;
			this.label6.Text = "Segue Time";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 243);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 13);
			this.label7.TabIndex = 18;
			this.label7.Text = "Start Time";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(574, 238);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(136, 23);
			this.button1.TabIndex = 17;
			this.button1.Text = "Add Track to Library";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// EditorArtistTitleTB
			// 
			this.EditorArtistTitleTB.BackColor = System.Drawing.Color.Gray;
			this.EditorArtistTitleTB.Dock = System.Windows.Forms.DockStyle.Top;
			this.EditorArtistTitleTB.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditorArtistTitleTB.Location = new System.Drawing.Point(15, 193);
			this.EditorArtistTitleTB.Name = "EditorArtistTitleTB";
			this.EditorArtistTitleTB.ReadOnly = true;
			this.EditorArtistTitleTB.Size = new System.Drawing.Size(684, 26);
			this.EditorArtistTitleTB.TabIndex = 2;
			// 
			// EditorPB
			// 
			this.EditorPB.BackColor = System.Drawing.Color.WhiteSmoke;
			this.EditorPB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.EditorPB.Dock = System.Windows.Forms.DockStyle.Top;
			this.EditorPB.ErrorImage = null;
			this.EditorPB.InitialImage = null;
			this.EditorPB.Location = new System.Drawing.Point(15, 0);
			this.EditorPB.Margin = new System.Windows.Forms.Padding(0);
			this.EditorPB.Name = "EditorPB";
			this.EditorPB.Size = new System.Drawing.Size(684, 193);
			this.EditorPB.TabIndex = 16;
			this.EditorPB.TabStop = false;
			this.EditorPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.EditorPB_MouseDown);
			// 
			// DgvLibrary
			// 
			this.DgvLibrary.AllowUserToAddRows = false;
			this.DgvLibrary.AllowUserToResizeColumns = false;
			this.DgvLibrary.AllowUserToResizeRows = false;
			this.DgvLibrary.AutoGenerateColumns = false;
			this.DgvLibrary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvLibrary.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			                                 	this.songDataGridViewTextBoxColumn,
			                                 	this.trackLengthDataGridViewTextBoxColumn,
			                                 	this.channelDataGridViewTextBoxColumn,
			                                 	this.filenameDataGridViewTextBoxColumn,
			                                 	this.channelsDataGridViewTextBoxColumn});
			this.DgvLibrary.DataMember = "Songs";
			this.DgvLibrary.DataSource = this.libraryBindingSource;
			this.DgvLibrary.Dock = System.Windows.Forms.DockStyle.Left;
			this.DgvLibrary.Location = new System.Drawing.Point(3, 3);
			this.DgvLibrary.Name = "DgvLibrary";
			this.DgvLibrary.ReadOnly = true;
			this.DgvLibrary.Size = new System.Drawing.Size(394, 321);
			this.DgvLibrary.TabIndex = 0;
			this.DgvLibrary.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvLibrary_CellClick);
			// 
			// songDataGridViewTextBoxColumn
			// 
			this.songDataGridViewTextBoxColumn.DataPropertyName = "Song";
			this.songDataGridViewTextBoxColumn.HeaderText = "Song";
			this.songDataGridViewTextBoxColumn.Name = "songDataGridViewTextBoxColumn";
			this.songDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// trackLengthDataGridViewTextBoxColumn
			// 
			this.trackLengthDataGridViewTextBoxColumn.DataPropertyName = "TrackLength";
			this.trackLengthDataGridViewTextBoxColumn.HeaderText = "TrackLength";
			this.trackLengthDataGridViewTextBoxColumn.Name = "trackLengthDataGridViewTextBoxColumn";
			this.trackLengthDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// channelDataGridViewTextBoxColumn
			// 
			this.channelDataGridViewTextBoxColumn.DataPropertyName = "Channel";
			this.channelDataGridViewTextBoxColumn.HeaderText = "Channel";
			this.channelDataGridViewTextBoxColumn.Name = "channelDataGridViewTextBoxColumn";
			this.channelDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// filenameDataGridViewTextBoxColumn
			// 
			this.filenameDataGridViewTextBoxColumn.DataPropertyName = "Filename";
			this.filenameDataGridViewTextBoxColumn.HeaderText = "Filename";
			this.filenameDataGridViewTextBoxColumn.Name = "filenameDataGridViewTextBoxColumn";
			this.filenameDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// channelsDataGridViewTextBoxColumn
			// 
			this.channelsDataGridViewTextBoxColumn.DataPropertyName = "Channels";
			this.channelsDataGridViewTextBoxColumn.HeaderText = "Channels";
			this.channelsDataGridViewTextBoxColumn.Name = "channelsDataGridViewTextBoxColumn";
			this.channelsDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// Buttons
			// 
			this.Buttons.Controls.Add(this.BtRemoveTrack);
			this.Buttons.Controls.Add(this.BtToggleZoom);
			this.Buttons.Controls.Add(this.BtOpenSchedule);
			this.Buttons.Controls.Add(this.BtPlay);
			this.Buttons.Controls.Add(this.BtStop);
			this.Buttons.Controls.Add(this.buttonAddFile);
			this.Buttons.Controls.Add(this.buttonSetEnvelope);
			this.Buttons.Controls.Add(this.buttonRemoveEnvelope);
			this.Buttons.Dock = System.Windows.Forms.DockStyle.Top;
			this.Buttons.Location = new System.Drawing.Point(15, 242);
			this.Buttons.Name = "Buttons";
			this.Buttons.Size = new System.Drawing.Size(1152, 101);
			this.Buttons.TabIndex = 22;
			// 
			// BtRemoveTrack
			// 
			this.BtRemoveTrack.Location = new System.Drawing.Point(911, 59);
			this.BtRemoveTrack.Name = "BtRemoveTrack";
			this.BtRemoveTrack.Size = new System.Drawing.Size(147, 23);
			this.BtRemoveTrack.TabIndex = 21;
			this.BtRemoveTrack.Text = "Remove Track from Playlist";
			this.BtRemoveTrack.Visible = true;
			this.BtRemoveTrack.Click += new System.EventHandler(this.BtRemoveTrack_Click);
			// 
			// Info
			// 
			this.Info.Controls.Add(this.PbWaveform2);
			this.Info.Controls.Add(this.UIWaveFormPB);
			this.Info.Controls.Add(this.panel6);
			this.Info.Dock = System.Windows.Forms.DockStyle.Top;
			this.Info.Location = new System.Drawing.Point(15, 15);
			this.Info.Name = "Info";
			this.Info.Size = new System.Drawing.Size(1152, 171);
			this.Info.TabIndex = 21;
			// 
			// PbWaveform2
			// 
			this.PbWaveform2.BackColor = System.Drawing.Color.WhiteSmoke;
			this.PbWaveform2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PbWaveform2.Dock = System.Windows.Forms.DockStyle.Top;
			this.PbWaveform2.ErrorImage = null;
			this.PbWaveform2.InitialImage = null;
			this.PbWaveform2.Location = new System.Drawing.Point(0, 60);
			this.PbWaveform2.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
			this.PbWaveform2.Name = "PbWaveform2";
			this.PbWaveform2.Size = new System.Drawing.Size(1152, 10);
			this.PbWaveform2.TabIndex = 16;
			this.PbWaveform2.TabStop = false;
			this.PbWaveform2.Visible = true;
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.panel2);
			this.panel6.Controls.Add(this.panel3);
			this.panel6.Controls.Add(this.panel4);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel6.Location = new System.Drawing.Point(0, 78);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(1152, 93);
			this.panel6.TabIndex = 11;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.StartTimeNUD);
			this.panel2.Controls.Add(this.CueStartTimeBTN);
			this.panel2.Controls.Add(this.SegueTimeLBL);
			this.panel2.Controls.Add(this.label10);
			this.panel2.Controls.Add(this.label9);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(608, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(195, 93);
			this.panel2.TabIndex = 18;
			this.panel2.Visible = true;
			// 
			// StartTimeNUD
			// 
			this.StartTimeNUD.Location = new System.Drawing.Point(75, 2);
			this.StartTimeNUD.Maximum = new decimal(new int[] {
			                                        	1410065407,
			                                        	2,
			                                        	0,
			                                        	0});
			this.StartTimeNUD.Name = "StartTimeNUD";
			this.StartTimeNUD.Size = new System.Drawing.Size(72, 21);
			this.StartTimeNUD.TabIndex = 5;
			this.StartTimeNUD.ValueChanged += new System.EventHandler(this.StartTimeNUD_ValueChanged_1);
			this.StartTimeNUD.Click += new System.EventHandler(this.StartTimeNUD_Click);
			// 
			// CueStartTimeBTN
			// 
			this.CueStartTimeBTN.Location = new System.Drawing.Point(147, 1);
			this.CueStartTimeBTN.Name = "CueStartTimeBTN";
			this.CueStartTimeBTN.Size = new System.Drawing.Size(38, 23);
			this.CueStartTimeBTN.TabIndex = 4;
			this.CueStartTimeBTN.Text = "Cue";
			this.CueStartTimeBTN.UseVisualStyleBackColor = true;
			this.CueStartTimeBTN.Click += new System.EventHandler(this.CueStartTimeBTN_Click);
			// 
			// SegueTimeLBL
			// 
			this.SegueTimeLBL.AutoSize = true;
			this.SegueTimeLBL.Location = new System.Drawing.Point(72, 25);
			this.SegueTimeLBL.Name = "SegueTimeLBL";
			this.SegueTimeLBL.Size = new System.Drawing.Size(0, 13);
			this.SegueTimeLBL.TabIndex = 3;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 25);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(62, 13);
			this.label10.TabIndex = 1;
			this.label10.Text = "Segue Time";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 4);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(56, 13);
			this.label9.TabIndex = 0;
			this.label9.Text = "Start Time";
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.SystemColors.Control;
			this.panel3.Controls.Add(this.labelCurrentTrack);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(608, 93);
			this.panel3.TabIndex = 16;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.label4);
			this.panel4.Controls.Add(this.label3);
			this.panel4.Controls.Add(this.label1);
			this.panel4.Controls.Add(this.labelRemain);
			this.panel4.Controls.Add(this.progressBarLeft);
			this.panel4.Controls.Add(this.label2);
			this.panel4.Controls.Add(this.labelTime);
			this.panel4.Controls.Add(this.progressBarRight);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel4.Location = new System.Drawing.Point(807, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(345, 93);
			this.panel4.TabIndex = 17;
			// 
			// AddTrackToLibraryOFD
			// 
			this.AddTrackToLibraryOFD.FileOk += new System.ComponentModel.CancelEventHandler(this.AddTrackToLibraryOFD_FileOk);
			// 
			// EditorTimer
			// 
			this.EditorTimer.Interval = 50;
			this.EditorTimer.Tick += new System.EventHandler(this.EditorTimer_Tick);
			// 
			// timerUpdateSeconds
			// 
			this.timerUpdateSeconds.Enabled = true;
			this.timerUpdateSeconds.Interval = 1000;
			this.timerUpdateSeconds.Tick += new System.EventHandler(this.timerUpdateSeconds_Tick);
			// 
			// AddScheduleItemToPlayerBGW
			// 
			this.AddScheduleItemToPlayerBGW.WorkerReportsProgress = true;
			this.AddScheduleItemToPlayerBGW.DoWork += new System.ComponentModel.DoWorkEventHandler(this.AddScheduleItemToPlayerBGW_DoWork);
			// 
			// OpenstationForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(1182, 741);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "OpenstationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OpenPlayout";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Simple_Closing);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.Form_Load);
			((System.ComponentModel.ISupportInitialize)(this.UIWaveFormPB)).EndInit();
			this.panel1.ResumeLayout(false);
			this.PanelLibrary.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ScheduleDgw)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.libraryBindingSource)).EndInit();
			this.tabPage2.ResumeLayout(false);
			this.PanelLibraryDetail.ResumeLayout(false);
			this.PanelLibraryDetail.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EditorSegueTimeNUD)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EditorStartTimeNUD)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EditorPB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DgvLibrary)).EndInit();
			this.Buttons.ResumeLayout(false);
			this.Info.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PbWaveform2)).EndInit();
			this.panel6.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartTimeNUD)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
	}
}