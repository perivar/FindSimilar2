using System;
using System.Windows.Forms;

using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Diagnostics;

//using FindSimilar2.Audio; // AudioFileReader
using FindSimilar2.AudioProxies; // BassProxy

using Soundfingerprinting;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Hashing;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;

using CommonUtils.Audio; // IAudio

namespace FindSimilar2
{
	/// <summary>
	/// FindSimilarClientForm
	/// </summary>
	public partial class FindSimilarClientForm : Form
	{
		private const int DEFAULT_NUM_TO_TAKE = 200;
		
		// Instance Variables
		private IAudio player = null;
		private string selectedFilePath = null;
		
		// Soundfingerprinting
		private DatabaseService databaseService = null;
		private Repository repository = null;
		
		BindingSource bs = new BindingSource();
		BindingList<QueryResult> queryResultList;
		
		// Waiting splash screen
		private SplashSceenWaitingForm splashScreen;
		
		// Threshold tables for use with Soundfingerprinting searching
		enum ThresholdTables {
			Show_All = 1,
			Limit_2 = 2,
			Limit_3 = 3,
			Limit_4 = 4,
			Limit_5 = 5,
			Limit_6 = 6,
			Limit_7 = 7,
			Limit_8 = 8,
			Limit_9 = 9
		}
		
		public FindSimilarClientForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// Constructor code after the InitializeComponent() call.
			//
			this.version.Text = Program.VERSION;
			this.ThresholdTablesCombo.DataSource = Enum.GetValues(typeof(ThresholdTables));
			
			// Instansiate Soundfingerprinting Repository
			FingerprintService fingerprintService = Analyzer.GetSoundfingerprintingService();
			this.databaseService = DatabaseService.Instance;

			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			repository = new Repository(permutations, databaseService, fingerprintService);
			
			LessAccurateCheckBox.Visible = true;
			ThresholdTablesCombo.Visible = true;
			SearchAllFilesCheckbox.Visible = true;
			
			ReadAllTracks();
		}
		
		#region Play
		void AudioFilePlayBtnClick(object sender, EventArgs e)
		{
			string queryPath = AudioFileQueryTextBox.Text;
			if (player != null && !queryPath.Equals("")) {
				Play(queryPath);
			}
		}
		
		private void Play(string filePath) {
			
			// return if play is auto play is disabled
			if (!autoPlayCheckBox.Checked) return;
			
			player = BassProxy.Instance;
			if (player != null) {
				player.Stop();
				player.OpenFile(filePath);
				if (player.CanPlay) {
					player.Play();
				} else {
					Debug.WriteLine("Failed playing using Un4Seen Bass ...");
				}
			}
		}

		private void PlaySelected() {
			if (player != null) {
				Play(selectedFilePath);
			}
		}
		#endregion
		
		#region Drag and Drop
		void TabPage1DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effect = DragDropEffects.Copy;
			} else if (e.Data.GetDataPresent(DataFormats.Text)) {
				e.Effect = DragDropEffects.Copy;
			}
		}
		
		void TabPage1DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string inputFilePath in files) {
					string fileExtension = Path.GetExtension(inputFilePath);
					int pos = Array.IndexOf(Program.extensions, fileExtension);
					if (pos >- 1)
					{
						AudioFileQueryTextBox.Text = inputFilePath;
						break;
					}
				}
			} else if (e.Data.GetDataPresent(DataFormats.Text)) {
				string droppedText = (string)e.Data.GetData(DataFormats.Text);
				AudioFileQueryTextBox.Text = droppedText;
			}
		}

		#endregion
		
		#region DataGridView Navigation
		void DataGridView1SelectionChanged(object sender, EventArgs e)
		{
			// on first load the selectedfilepath is null
			bool doPlay = true;
			if (selectedFilePath == null) {
				doPlay = false;
			}
			
			var dgv = (DataGridView)sender;

			// User selected WHOLE ROW (by clicking in the margin)
			// or if SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			if (dgv.SelectedRows.Count> 0) {
				if (dgv.SelectedRows[0].Cells[1].Value != null) {
					selectedFilePath = dgv.SelectedRows[0].Cells[1].Value.ToString();
					if (doPlay) Play(selectedFilePath);
				}
			}
		}
		
		void DataGridView1KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Space) {
				PlaySelected();
			}
		}
		
		void DataGridView1MouseDown(object sender, MouseEventArgs e)
		{
			// Get the row index of the item the mouse is below.
			DataGridView.HitTestInfo hti = dataGridView1.HitTest(e.X, e.Y);

			if (hti.ColumnIndex >= 0 && hti.RowIndex >= 0) {
				DataGridViewCell dragCell = dataGridView1[hti.ColumnIndex, hti.RowIndex];
				
				// set current cell
				dataGridView1.CurrentCell = null;
				dataGridView1.CurrentCell = dragCell;

				// check value
				if (e.Button == MouseButtons.Left) {
					
					if (dragCell.Value != null) {
						
						// The DoDragDrop method of a control is used to start a drag and drop operation.
						// We call it from MouseDown event of the DataGridView.
						// The first parameter is the data that we want to send in drag and drop operation.
						// The second parameter is a DragDropEffects enumeration that provides the drag and drop operation effect.
						// The cursor style changes accordingly while the drag and drop is being performed.
						// Possible values are DragDropEffects.All, DragDropEffects.Copy, DragDropEffects.Link, DragDropEffects.Move,
						// DragDropEffects.None and DragDropEffects.Scroll.
						
						string cellContent = dragCell.Value.ToString();
						//string dataFormat = DataFormats.Text;
						//dataGridView1.DoDragDrop(cellContent, DragDropEffects.Copy);
						
						string filePath = cellContent;
						if (File.Exists(filePath)) {
							string dataFormat = DataFormats.FileDrop;
							string[] filePathArray = { filePath };
							var dataObject = new DataObject(dataFormat, filePathArray);
							dataGridView1.DoDragDrop(dataObject, DragDropEffects.Copy);
						}

					}
				}
			}
		}
		#endregion
		
		#region ToolStripMenu Clicks
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				FindById(queryId);
			}
		}
		
		void OpenFileLocationToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!File.Exists(selectedFilePath))	{
				MessageBox.Show("File does not exist!");
				return;
			}
			
			string args = string.Format("/e, /select, \"{0}\"", selectedFilePath);

			var info = new ProcessStartInfo();
			info.FileName = "explorer";
			info.Arguments = args;
			Process.Start(info);
		}
		
		void CopyFileURLToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			if (selectedFilePath != null) {
				Clipboard.SetText(selectedFilePath);
			}
		}

		void OpenEditorToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			if (selectedFilePath != null) {
				new WaveEditor(selectedFilePath).Show();
			}
		}
		
		void DumpDebugInfoToolStripMenuItemClick(object sender, EventArgs e)
		{
			var fileInfo = new FileInfo(selectedFilePath);
			if (!fileInfo.Exists) {
				MessageBox.Show("File does not exist!");
			}
		}
		#endregion
		
		#region Button Clicks, Combo and Checkbox Changes and Form Closing
		void ResetBtnClick(object sender, EventArgs e)
		{
			ReadAllTracks();
		}
		
		void GoBtnClick(object sender, EventArgs e)
		{
			if (tabControl1.SelectedTab == tabControl1.TabPages["tabFileSearch"])
			{
				string queryPath = AudioFileQueryTextBox.Text;
				FindByFilePath(queryPath);
			} else if (tabControl1.SelectedTab == tabControl1.TabPages["tabIdSearch"]) {
				int queryId = -1;
				int.TryParse(QueryIdTextBox.Text, out queryId);
				FindById(queryId);
			} else if (tabControl1.SelectedTab == tabControl1.TabPages["tabStringSearch"]) {
				string queryString = QueryStringTextBox.Text;
				FindByString(queryString);
			}
		}
		
		void AudioFileQueryBtnClick(object sender, EventArgs e)
		{
			const string filter = "All supported Audio Files|*.wav;*.ogg;*.mp1;*.m1a;*.mp2;*.m2a;*.mpa;*.mus;*.mp3;*.mpg;*.mpeg;*.mp3pro;*.aif;*.aiff;*.bwf;*.wma;*.wmv;*.aac;*.adts;*.mp4;*.m4a;*.m4b;*.mod;*.mdz;*.mo3;*.s3m;*.s3z;*.xm;*.xmz;*.it;*.itz;*.umx;*.mtm;*.flac;*.fla;*.oga;*.ogg;*.aac;*.m4a;*.m4b;*.mp4;*.mpc;*.mp+;*.mpp;*.ac3;*.wma;*.ape;*.mac|WAVE Audio|*.wav|Ogg Vorbis|*.ogg|MPEG Layer 1|*.mp1;*.m1a|MPEG Layer 2|*.mp2;*.m2a;*.mpa;*.mus|MPEG Layer 3|*.mp3;*.mpg;*.mpeg;*.mp3pro|Audio IFF|*.aif;*.aiff|Broadcast Wave|*.bwf|Windows Media Audio|*.wma;*.wmv|Advanced Audio Codec|*.aac;*.adts|MPEG 4 Audio|*.mp4;*.m4a;*.m4b|MOD Music|*.mod;*.mdz|MO3 Music|*.mo3|S3M Music|*.s3m;*.s3z|XM Music|*.xm;*.xmz|IT Music|*.it;*.itz;*.umx|MTM Music|*.mtm|Free Lossless Audio Codec|*.flac;*.fla|Free Lossless Audio Codec (Ogg)|*.oga;*.ogg|Advanced Audio Coding|*.aac|Advanced Audio Coding MPEG-4|*.m4a;*.m4b;*.mp4|Musepack|*.mpc;*.mp+;*.mpp|Dolby Digital AC-3|*.ac3|Windows Media Audio|*.wma|Monkey's Audio|*.ape;*.mac";
			openFileDialog.Filter = filter;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				AudioFileQueryTextBox.Text = openFileDialog.FileName;
			}
		}
		
		void AutoPlayCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (!autoPlayCheckBox.Checked) {
				if (player != null) {
					player.Stop();
				}
			}
		}
		
		void LessAccurateCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			// do nothing else than using the value in FindByFilePathSoundfingerprinting
		}
		
		void FindSimilarClientFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (player != null) player.Dispose();
		}
		#endregion

		#region ReadAllTracks
		private void ReadAllTracks() {
			ReadAllTracksSoundfingerprinting();
		}
		
		private void ReadAllTracksSoundfingerprinting() {
			
			string limitClause = string.Format("LIMIT {0}", DEFAULT_NUM_TO_TAKE);
			IList<Track> tracks = databaseService.ReadTracks(limitClause);
			
			var fingerprintList = (from row in tracks
			                       orderby row.Id
			                       select new QueryResult {
			                       	Id = row.Id,
			                       	Path = row.FilePath,
			                       	Duration = row.TrackLengthMs
			                       }).ToList();
			
			queryResultList = new BindingList<QueryResult>( fingerprintList );
			
			bs.DataSource = queryResultList;
			dataGridView1.DataSource = queryResultList;
			
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			
			this.database_count.Text = databaseService.GetTrackCount().ToString();
		}
		#endregion
		
		#region Find Soundfingerprinting methods
		class BackgroundWorkerArgument {
			
			public FileInfo QueryFile { get; set; }
			public List<QueryResult> QueryResultList { get; set; }
			public int ThresholdTables { get; set; }
			public bool OptimizeSignatureCount { get; set; }
			public bool DoSearchEverything { get; set; }
		}

		private void DoSoundfingerprintingsSearch(object bgWorkerArg) {
			
			// Start "please wait" screen
			splashScreen = new SplashSceenWaitingForm();
			splashScreen.DoWork += new SplashSceenWaitingForm.DoWorkEventHandler(findSimilarSearch_DoWork);
			splashScreen.Argument = bgWorkerArg;
			
			// check return value
			DialogResult result = splashScreen.ShowDialog();
			switch (result) {
				case DialogResult.Cancel:
					break;
				case DialogResult.Abort:
					MessageBox.Show(splashScreen.Result.Error.Message);
					break;
				case DialogResult.OK:
					var argObject = splashScreen.Result.Result as BackgroundWorkerArgument;
					if (argObject.QueryResultList != null) {
						// Get query list from the argument object
						queryResultList = new BindingList<QueryResult>(argObject.QueryResultList);
						// update grid
						bs.DataSource = queryResultList;
						dataGridView1.DataSource = queryResultList;
						this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
						this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					}
					break;
			}
		}
		
		/// <summary>
		/// Method to run in the background while showing a "Please wait" screen
		/// </summary>
		/// <param name="sender">The "Please wait" screen form</param>
		/// <param name="e">Event arguments</param>
		void findSimilarSearch_DoWork(SplashSceenWaitingForm sender, DoWorkEventArgs e)
		{
			// e.Argument always contains whatever was sent to the background worker
			// in RunWorkerAsync. We can simply cast it to its original type.
			var argObject = e.Argument as BackgroundWorkerArgument;
			
			// Perform a time consuming operation and report progress.
			List<QueryResult> queryList = Analyzer.SimilarTracksSoundfingerprintingList(argObject.QueryFile,
			                                                                            repository,
			                                                                            argObject.ThresholdTables,
			                                                                            argObject.OptimizeSignatureCount,
			                                                                            argObject.DoSearchEverything,
			                                                                            sender);
			
			// and set the result
			argObject.QueryResultList = queryList;
			e.Result = argObject;
		}
		
		private void FindByFilePathSoundfingerprinting(string queryPath) {
			if (queryPath != "") {
				var fi = new FileInfo(queryPath);
				if (fi.Exists) {
					
					// create background worker arugment
					var bgWorkerArg = new BackgroundWorkerArgument {
						QueryFile = fi,
						ThresholdTables = (int) ThresholdTablesCombo.SelectedValue,
						OptimizeSignatureCount = LessAccurateCheckBox.Checked,
						DoSearchEverything = SearchAllFilesCheckbox.Checked
					};
					
					// and do the search
					DoSoundfingerprintingsSearch(bgWorkerArg);

				} else {
					MessageBox.Show("File does not exist!");
				}
			}
		}
		
		private void FindByIdSoundfingerprinting(int queryId) {
			
			if (queryId != -1) {
				
				Track track = databaseService.ReadTrackById(queryId);
				if (track != null) {

					if (track.FilePath != null && File.Exists(track.FilePath)) {
						
						// create background worker arugment
						var bgWorkerArg = new BackgroundWorkerArgument {
							QueryFile = new FileInfo(track.FilePath),
							ThresholdTables = (int) ThresholdTablesCombo.SelectedValue,
							OptimizeSignatureCount = LessAccurateCheckBox.Checked,
							DoSearchEverything = SearchAllFilesCheckbox.Checked
						};
						
						// and do the search
						DoSoundfingerprintingsSearch(bgWorkerArg);
						
					} else {
						MessageBox.Show("File does not exist!");
					}
					
				} else {
					MessageBox.Show("File-id does not exist!");
				}
			}
		}
		
		private void FindByStringSoundfingerprinting(string queryString) {
			
			if (queryString != "") {
				
				// search for tracks
				string whereClause = string.Format("WHERE tags like '%{0}%' or title like '%{0}%' or filepath like '%{0}%'", queryString);
				IList<Track> tracks = databaseService.ReadTracks(whereClause);

				var fingerprintList = (from row in tracks
				                       orderby row.Id ascending
				                       select new QueryResult {
				                       	Id = row.Id,
				                       	Path = row.FilePath,
				                       	Duration = row.TrackLengthMs
				                       }).ToList();
				
				queryResultList = new BindingList<QueryResult>( fingerprintList );
				
				bs.DataSource = queryResultList;
				dataGridView1.DataSource = queryResultList;
				
				this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}
		}
		#endregion
		
		#region Find methods
		private void FindByFilePath(string queryPath) {
			FindByFilePathSoundfingerprinting(queryPath);
		}
		
		private void FindById(int queryId) {
			FindByIdSoundfingerprinting(queryId);
		}
		
		private void FindByString(string queryString) {
			FindByStringSoundfingerprinting(queryString);
		}
		#endregion
		
		#region Query Field Actions
		void QueryIdTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				int queryId = -1;
				int.TryParse(QueryIdTextBox.Text, out queryId);
				FindById(queryId);
			}
		}
		
		void AudioFileQueryTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				string queryPath = AudioFileQueryTextBox.Text;
				FindByFilePath(queryPath);
			}
		}
		
		void QueryStringTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				string queryString = QueryStringTextBox.Text;
				FindByString(queryString);
			}
		}
		#endregion
		
		#region Radio Button Change Events
		void RbSoundfingerprintingCheckedChanged(object sender, EventArgs e)
		{
			ReadAllTracks();
		}
		#endregion
		
		#region Filtering of the query results
		void TxtFilterResultsKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				if (queryResultList != null) {
					var filtered = new BindingList<QueryResult>(
						queryResultList.Where(result => result.Path.ToLower().Contains(txtFilterResults.Text.ToLower())).ToList());
					dataGridView1.DataSource = filtered;
					dataGridView1.Update();
				}
			}
		}
		
		void BtnClearFilterClick(object sender, EventArgs e)
		{
			txtFilterResults.Text = "";
			if (queryResultList != null) {
				dataGridView1.DataSource = queryResultList;
				dataGridView1.Update();
			}
		}
		#endregion
		
	}

	// http://stackoverflow.com/questions/17309270/datagridview-binding-source-filter
	public class QueryResult {
		public QueryResult() { }
		
		public QueryResult(int Id, string Path, long Duration, double Similarity) {
			this.Id = Id;
			this.Path = Path;
			this.Duration = Duration;
			this.Similarity = Similarity;
		}
		
		public int Id { get; set; }
		public string Path { get; set; }
		public long Duration { get; set; }
		public double Similarity { get; set; }
	}
}