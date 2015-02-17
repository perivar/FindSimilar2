using System;
using System.Windows.Forms;
using System.Collections.Generic;
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
	public partial class OpenstationForm : Form
	{
		class Library {
			
			public Library(string song) {
				for (int i = 0; i < 5; i++) {
					Songs.Add(new Track(song + " - " + i));
				}
			}
			
			public List<Track> _songs = new List<Track>();
			public List<Track> Songs { get { return _songs; } }
			
			public List<string> _schedule = new List<string>();
			public List<string> Schedule { get { return _schedule; } }
		}
		//private Library openStationDS = new Library();
		
		class Track {
			string song;
			Tags tags;
			int channels;
			string filename;
			long trackLength;
			
			int channelID;
			int scheduleID;
			long startTrackPos;
			long nextTrackPos;
			
			WaveForm waveform;
			SYNCPROC trackSync;
			int nextTrackSync;

			public override string ToString()
			{
				return filename;
			}
			
			public Track(string filename) {
				this.filename = filename;
				
				tags = new Tags();
				tags.title = Path.GetFileName(filename);
				tags.artist = "Unknown";

				channelID = Bass.BASS_StreamCreateFile(filename, 0, 0,
				                                       BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
			}

			public SYNCPROC TrackSync {
				get {
					return trackSync;
				}
				set {
					trackSync = value;
				}
			}

			public int NextTrackSync {
				get {
					return nextTrackSync;
				}
				set {
					nextTrackSync = value;
				}
			}
			public string Song {
				get {
					return song;
				}
				set {
					song = value;
				}
			}
			public long TrackLength {
				get {
					return trackLength;
				}
				set {
					trackLength = value;
				}
			}
			public WaveForm Waveform {
				get {
					return waveform;
				}
				set {
					waveform = value;
				}
			}
			public long NextTrackPos {
				get {
					return nextTrackPos;
				}
				set {
					nextTrackPos = value;
				}
			}
			public long StartTrackPos {
				get {
					return startTrackPos;
				}
				set {
					startTrackPos = value;
				}
			}
			public int ScheduleID {
				get {
					return scheduleID;
				}
				set {
					scheduleID = value;
				}
			}
			public int Channel {
				get {
					return channelID;
				}
				set {
					channelID = value;
				}
			}
			public string Filename {
				get {
					return filename;
				}
				set {
					filename = value;
				}
			}
			public int Channels {
				get {
					return channels;
				}
				set {
					channels = value;
				}
			}
			public Tags Tags {
				get {
					return tags;
				}
				set {
					tags = value;
				}
			}
		}
		
		class Tags {
			public string title;
			public string artist;
		}
		
		public OpenstationForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		#region Private Vars
		private int _mixer = 0;
		private SYNCPROC _mixerStallSync;
		private Track _currentTrack = null;
		private Track _previousTrack = null;
		private Queue TrackCuePointQueue;
		#endregion

		class Logger {
			public void Debug(string text) {
				Console.Out.WriteLine(text);
			}
			public void Error(string text) {
				Console.Error.WriteLine(text);
			}
			public void Error(Object e) {
				Console.Error.WriteLine(e);
			}
		}
		static Logger logger
		{
			get {
				return new Logger();
			}
		}

		private void Form_Load(object sender, System.EventArgs e)
		{

			// BassNet.Registration("your email", "your regkey");

			logger.Debug("************************Starting " + GetAppNameAndVersion() + "************************");

			SetFormTitle();
			
			InitializeSubfolders();

			logger.Debug("Initializing BASS");
			
			if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, this.Handle))
			{
				MessageBox.Show(this, "Bass_Init error!");
				logger.Error("Bass_Init error! Closing...");
				this.Close();
				return;
			}

			logger.Debug("Bass SetConfig, buffer = 200");
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 200);

			logger.Debug("Bass SetConfig, updateperiod = 20");
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 20);

			logger.Debug("Bass StreamCreate: Sample rate 44100, 2 channels, 32bit floating point output");
			_mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_SAMPLE_FLOAT);

			if (_mixer == 0)
			{
				logger.Error("Could not create mixer! Closing...");
				MessageBox.Show(this, "Could not create mixer!");
				Bass.BASS_Free();
				this.Close();
				return;
			}

			_mixerStallSync = new SYNCPROC(OnMixerStall);
			logger.Debug("Bass ChannelSetSync (setting a synchronizer): default mixer, Sync on stall, default user instance");
			Bass.BASS_ChannelSetSync(_mixer, BASSSync.BASS_SYNC_STALL, 0L, _mixerStallSync, IntPtr.Zero);

			timerUpdate.Start();
			logger.Debug("timerUpdate started");
			EditorTimer.Start();
			logger.Debug("EditorTimer started");
			Bass.BASS_ChannelPlay(_mixer, false);
			logger.Debug("Bass ChannelPlay started, don't restart playback from beginning");

			if (File.Exists("songs.xml"))
			{
				logger.Debug("Load song library from songs.xml");
			}
			
			// Dummy
			//libraryBindingSource.Add(new Library("Song A"));

			//LoadTodaySchedule();
			//InitializeCrons();
		}

		private void SetFormTitle()
		{
			this.Text = GetAppNameAndVersion();
		}

		private string GetAppNameAndVersion()
		{
			System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();

			return _assemblyInfo.GetName().Name.ToString() + " " + _assemblyInfo.GetName().Version.ToString();
		}

		private void InitializeSubfolders()
		{
			if (!Directory.Exists(@"Schedules"))
			{
				Directory.CreateDirectory(@"Schedules");
				logger.Debug("Created directory: Schedules");
			}
			if (!Directory.Exists(@"Debug"))
			{
				Directory.CreateDirectory(@"Debug");
				logger.Debug("Created directory: Debug");
			}
		}

		private void InitializeCrons()
		{
			logger.Debug("Creating trigger for AddNextDaySchedule");
		}

		private void AddNextDaySchedule()
		{
			string ScheduleFile = @"Schedules\"+DateTime.Today.AddDays(1).ToString("yyyyMMdd")+".log";
			if (File.Exists(ScheduleFile))
			{
				LoadSchedule(DateTime.Today.AddDays(1));
			}
		}

		private void LoadTodaySchedule()
		{
			try
			{
				string ScheduleFile = @"Schedules\" + DateTime.Today.ToString("yyyyMMdd") + ".log";
				if (File.Exists(ScheduleFile))
				{
					logger.Debug("Loading Today Schedule");
					LoadSchedule(DateTime.Today);
					InitializePlayerFromSchedule();
				}
				else
				{
					logger.Debug("Today Schedule (\\Schedules\\" + DateTime.Today.ToString("yyyyMMdd") + ".log) not found, do nothing");
					MessageBox.Show("Today Schedule (\\Schedules\\" + DateTime.Today.ToString("yyyyMMdd") + ".log) not found!","Error",MessageBoxButtons.OK);
				}
			}
			catch (Exception ex)
			{
				logger.Error("FATAL ERROR");
				logger.Error(ex.Message);
				if (ex.InnerException != null)
				{
					logger.Error(ex.InnerException);
				}
				logger.Error(ex.Data);
			}
		}

		private void LoadSchedule(DateTime ScheduleDate)
		{
			logger.Debug("Loading schedule for " + ScheduleDate.Date + @" from file \Schedules\" + ScheduleDate.ToString("yyyyMMdd") + ".log");
			logger.Debug("Schedule for " + ScheduleDate.Date + " loaded successfully");
		}

		private void InitializePlayerFromSchedule()
		{

			logger.Debug("Loading next 2 events to play from current schedule at current time");
			logger.Debug("Start player");
			StartPlayer();
		}

		private void Simple_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			EditorTimer.Stop();
			timerUpdate.Stop();
			logger.Debug("Closing Bass");
			Bass.BASS_StreamFree(_mixer);
			Bass.BASS_Free();
		}

		private void OnMixerStall(int handle, int channel, int data, IntPtr user)
		{
			BeginInvoke((MethodInvoker)delegate()
			            {
			            	// this code runs on the UI thread!
			            	if (data == 0)
			            	{
			            		logger.Debug("Mixer stalled");
			            		logger.Debug("Stopping timerUpdate");
			            		timerUpdate.Stop();
			            		logger.Debug("Stopping EditorTimer");
			            		EditorTimer.Stop();
			            		logger.Debug("Setting progress Bars to 0");
			            		progressBarLeft.Value = 0;
			            		progressBarRight.Value = 0;
			            	}
			            	else
			            	{
			            		logger.Debug("Mixer resumed");
			            		logger.Debug("Re-starting timerUpdate");
			            		timerUpdate.Start();
			            		logger.Debug("Re-starting EditorTimer");
			            		EditorTimer.Start();
			            	}
			            });
		}

		private void PlayTrack()
		{
			if (listBoxPlaylist.Items.Count > 0)
			{
				logger.Debug("Getting next track to play");
				_previousTrack = _currentTrack;
				_currentTrack = listBoxPlaylist.Items[0] as Track;
				listBoxPlaylist.Items.RemoveAt(0);
				
				// the channel was already added
				// so for instant playback, we just unpause the channel
				logger.Debug("START PLAYING: " +_currentTrack.Tags.title + " - " + _currentTrack.Tags.artist);
				BassMix.BASS_Mixer_ChannelPlay(_currentTrack.Channel);
				labelCurrentTrack.Text = _currentTrack.Tags.title + " - " + _currentTrack.Tags.artist;

				logger.Debug("Calling SetCurrentTrackWaveForm on " + _currentTrack);
				SetCurrentTrackWaveForm();
			}
		}

		private void UpdateSchedule(Track currentTrack)
		{
			logger.Debug("Finding ScheduleItem from CurrentTrack.ScheduleID (=" + currentTrack.ScheduleID + ")");
		}

		private void SetTrackCuePoints(Track tr)
		{
			logger.Debug("Rendering waveform for " + tr.Filename);
			TrackWF = new WaveForm(tr.Filename);
			TrackWF.RenderStart(true, BASSFlag.BASS_DEFAULT);
			while (!TrackWF.IsRendered)
			{
			}
			logger.Debug("Synchronizing waveform position with playback channel");
			TrackWF.SyncPlayback(tr.Channel);
			long startPos = 0L;
			long endPos = 0L;
			logger.Debug("Bass GetCuePoints on current waveform: calculating Start and Mix point using default Db thresholds: -24.0 on start, -12.0 on end");
			if (TrackWF.GetCuePoints(ref startPos, ref endPos, -24.0, -12.0, true))
			{
				tr.StartTrackPos = startPos;
				tr.NextTrackPos = endPos;
				logger.Debug("Start position (" + tr.StartTrackPos + ")");
				logger.Debug("Next Track position (" + tr.NextTrackPos + ")");
				logger.Debug("Calling SetTrackToPosition - Set start at calculated start position");
				SetTrackToPosition(tr.Channel, tr.StartTrackPos);
			}
			else
			{
				logger.Error("Error calculating Cue Points on current waveform");
			}
			logger.Debug("Adding calculated Waveform to its own Track...");
			tr.Waveform = TrackWF;
			if (tr == _currentTrack)
			{
				logger.Debug("Waveform rendered for " + tr + ". Since it's currently playing, we run SetCurrentTrackWaveForm");
				SetCurrentTrackWaveForm();
			}
		}

		private void OnTrackSync(int handle, int channel, int data, IntPtr user)
		{
			logger.Debug("Handle = " + handle + " Channel = " + channel + " Data = " + data + " User = " + user.ToInt32());
			if (user.ToInt32() == 0)
			{
				logger.Debug("Called a sync on end - deactivated");
				logger.Debug("Calling PlayTrack...");
				BeginInvoke(new MethodInvoker(PlayTrack));
			}
			else
			{
				logger.Debug("Called a sync on track position");
				BeginInvoke((MethodInvoker)delegate()
				            {
				            	// this code runs on the UI thread!
				            	logger.Debug("Calling PlayTrack...");
				            	PlayTrack();
				            	StopPreviousTrack();
				            });
			}
		}
		#region Wave Form

		// zoom helper varibales
		private bool _zoomed = false;
		private int _zoomStart = -1;
		private long _zoomStartBytes = -1;
		private int _zoomEnd = -1;
		private float _zoomDistance = 5.0f; // zoom = 5sec.

		private Un4seen.Bass.Misc.WaveForm _WF = null;
		private void SetCurrentTrackWaveForm()
		{
			if (_currentTrack.Waveform != null)
			{
				logger.Debug("Getting waveform for " + _currentTrack);
				_zoomStart = -1;
				_zoomStartBytes = -1;
				_zoomEnd = -1;
				_zoomed = false;
				logger.Debug("Rendering the wave form");
				_WF = _currentTrack.Waveform;
				_WF.FrameResolution = 0.01f; // 10ms are nice
				_WF.CallbackFrequency = 30000; // every 5min.
				_WF.ColorBackground = Color.FromArgb(20, 20, 20);
				_WF.ColorLeft = Color.Gray;
				_WF.ColorLeftEnvelope = Color.LightGray;
				_WF.ColorRight = Color.Gray;
				_WF.ColorRightEnvelope = Color.LightGray;
				_WF.ColorMarker = Color.Gold;
				_WF.ColorBeat = Color.LightSkyBlue;
				_WF.ColorVolume = Color.White;
				_WF.DrawEnvelope = false;
				_WF.DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.HalfMono;
				_WF.DrawMarker = WaveForm.MARKERDRAWTYPE.Line | WaveForm.MARKERDRAWTYPE.Name | WaveForm.MARKERDRAWTYPE.NamePositionAlternate;
				_WF.MarkerLength = 0.75f;
				logger.Debug("Syncing UI Waveform to currentTrack channel - " + _currentTrack.Tags.title);
				_WF.SyncPlayback(_currentTrack.Channel);
				logger.Debug("UI Waveform: adding Start marker at position " + _currentTrack.StartTrackPos);
				_WF.AddMarker("Start", _currentTrack.StartTrackPos);
				logger.Debug("UI Waveform: adding Next marker at position " + _currentTrack.NextTrackPos);
				_WF.AddMarker("Next", _currentTrack.NextTrackPos);
				logger.Debug("Drawing Waveform");
				DrawWave();
			}
			else
			{
				logger.Debug("Waveform not found for current Track " + _currentTrack + ". Maybe not enough time to generate it?");
			}
		}

		private void EditorGetWaveForm()
		{
			// unzoom...(display the whole wave form)
			_zoomStart = -1;
			_zoomStartBytes = -1;
			_zoomEnd = -1;
			_zoomed = false;
			// render a wave form
			EditorWf = new WaveForm(EditorTrack.Filename, new WAVEFORMPROC(EditorWaveFormCallback), this);
			EditorWf.FrameResolution = 0.01f; // 10ms are nice
			EditorWf.CallbackFrequency = 30000; // every 5min.
			EditorWf.ColorBackground = Color.FromArgb(20, 20, 20);
			EditorWf.ColorLeft = Color.Gray;
			EditorWf.ColorLeftEnvelope = Color.LightGray;
			EditorWf.ColorRight = Color.Gray;
			EditorWf.ColorRightEnvelope = Color.LightGray;
			EditorWf.ColorMarker = Color.Gold;
			EditorWf.ColorBeat = Color.LightSkyBlue;
			EditorWf.ColorVolume = Color.White;
			EditorWf.DrawEnvelope = false;
			EditorWf.DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.Stereo;
			EditorWf.DrawMarker = WaveForm.MARKERDRAWTYPE.Line | WaveForm.MARKERDRAWTYPE.Name | WaveForm.MARKERDRAWTYPE.NamePositionAlternate;
			EditorWf.MarkerLength = 0.80f;
			EditorWf.MarkerFont = new Font(FontFamily.GenericSansSerif, 10);
			EditorWf.RenderStart(true, BASSFlag.BASS_DEFAULT);
		}

		private void MyWaveFormCallback(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
		{

		}

		private void EditorWaveFormCallback(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
		{
			if (finished)
			{
				EditorWf.SyncPlayback(EditorTrack.Channel);

				// and do pre-calculate the next track position
				// in this example we will only use the end-position
				long startPos = 0L;
				long endPos = 0L;
				if (EditorWf.GetCuePoints(ref startPos, ref endPos, -24.0, -12.0, true))
				{
					EditorTrack.StartTrackPos = startPos;
					EditorTrack.NextTrackPos = endPos;
					EditorWf.AddMarker("Next", endPos);
					EditorWf.AddMarker("Start", startPos);
				}
			}
			//  will be called during rendering...
			DrawWave(EditorPB,EditorWf);
		}

		private void DrawWave()
		{
			DrawWave(this.UIWaveFormPB);
		}

		private void DrawWave(PictureBox pb)
		{
			DrawWave(pb, _WF);
		}

		private void DrawWave(PictureBox pb, WaveForm wf)
		{
			if (wf != null)
				pb.BackgroundImage = wf.CreateBitmap( pb.Width, pb.Height, _zoomStart, _zoomEnd, true);
			else
				pb.BackgroundImage = null;
		}

		private void DrawWavePosition(long pos, long len)
		{
			DrawWavePosition(this.UIWaveFormPB, _WF, pos, len);
		}

		private void DrawWavePosition(PictureBox pb, WaveForm wf, long pos, long len)
		{
			if (wf == null || len == 0 || pos < 0)
			{
				pb.Image = null;
				return;
			}

			Bitmap bitmap = null;
			Graphics g = null;
			Pen p = null;
			double bpp = 0;

			try
			{
				if (_zoomed)
				{
					// total length doesn't have to be _zoomDistance sec. here
					len = wf.Frame2Bytes(_zoomEnd) - _zoomStartBytes;

					int scrollOffset = 10; // 10*20ms = 200ms.
					// if we scroll out the window...(scrollOffset*20ms before the zoom window ends)
					if ( pos > (_zoomStartBytes + len - scrollOffset*wf.Wave.bpf) )
					{
						// we 'scroll' our zoom with a little offset
						_zoomStart = wf.Position2Frames(pos - scrollOffset*wf.Wave.bpf);
						_zoomStartBytes = wf.Frame2Bytes(_zoomStart);
						_zoomEnd = _zoomStart + wf.Position2Frames( _zoomDistance ) - 1;
						if (_zoomEnd >= wf.Wave.data.Length)
						{
							// beyond the end, so we zoom from end - _zoomDistance.
							_zoomEnd = wf.Wave.data.Length-1;
							_zoomStart = _zoomEnd - wf.Position2Frames( _zoomDistance ) + 1;
							if (_zoomStart < 0)
								_zoomStart = 0;
							_zoomStartBytes = wf.Frame2Bytes(_zoomStart);
							// total length doesn't have to be _zoomDistance sec. here
							len = wf.Frame2Bytes(_zoomEnd) - _zoomStartBytes;
						}
						// get the new wave image for the new zoom window
						DrawWave(pb,wf);
					}
					// zoomed: starts with _zoomStartBytes and is _zoomDistance long
					pos -= _zoomStartBytes; // offset of the zoomed window
					
					bpp = len/(double)pb.Width;  // bytes per pixel
				}
				else
				{
					// not zoomed: width = length of stream
					bpp = len/(double)pb.Width;  // bytes per pixel
				}

				p = new Pen(Color.LightGreen);
				bitmap = new Bitmap(pb.Width, pb.Height);
				g = Graphics.FromImage(bitmap);
				g.Clear( Color.Black );
				int x = (int)Math.Round(pos/bpp);  // position (x) where to draw the line
				p.Width = (float)x;
				g.DrawLine(p, x / 2, 0, x / 2, pb.Height - 1);

				bitmap.MakeTransparent( Color.Black );
			}
			catch
			{
				bitmap = null;
			}
			finally
			{
				// clean up graphics resources
				if (p != null)
					p.Dispose();
				if (g != null)
					g.Dispose();
			}

			pb.Image = bitmap;
		}

		private void ToggleZoom()
		{
			if (_WF == null)
				return;

			// WF is not null, so the stream must be playing...
			if (_zoomed)
			{
				logger.Debug("Unzooming...(display the whole wave form)");
				_zoomStart = -1;
				_zoomStartBytes = -1;
				_zoomEnd = -1;
			}
			else
			{
				logger.Debug("Zooming waveform - default zoom: " + _zoomDistance);
				long pos = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
				// calculate the window to display
				_zoomStart = _WF.Position2Frames(pos);
				_zoomStartBytes = _WF.Frame2Bytes(_zoomStart);
				_zoomEnd = _zoomStart + _WF.Position2Frames(_zoomDistance) - 1;
				if (_zoomEnd >= _WF.Wave.data.Length)
				{
					// beyond the end, so we zoom from end - _zoomDistance.
					_zoomEnd = _WF.Wave.data.Length - 1;
					_zoomStart = _zoomEnd - _WF.Position2Frames(_zoomDistance) + 1;
					_zoomStartBytes = _WF.Frame2Bytes(_zoomStart);
				}
			}
			_zoomed = !_zoomed;
			// and display this new wave form
			DrawWave();
		}

		private void pictureBoxWaveForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (_WF == null)
				return;

			bool doubleClick = e.Clicks > 1;
			bool lowerHalf = (e.Y > UIWaveFormPB.Height / 2);

			if (lowerHalf && doubleClick)
			{
				ToggleZoom();
			}
			else if (!lowerHalf && e.Button == MouseButtons.Left)
			{
				// left button will set the position
				long pos = _WF.GetBytePositionFromX(e.X, UIWaveFormPB.Width, _zoomStart, _zoomEnd);
				SetTrackToPosition(_currentTrack.Channel, pos);
			}
			else if (!lowerHalf)
			{
				_currentTrack.NextTrackPos = _WF.GetBytePositionFromX(e.X, UIWaveFormPB.Width, _zoomStart, _zoomEnd);
				// if there is already a sync set, remove it first
				if (_currentTrack.NextTrackSync != 0)
					BassMix.BASS_Mixer_ChannelRemoveSync(_currentTrack.Channel, _currentTrack.NextTrackSync);

				// right button will set a next track position sync
				_currentTrack.NextTrackSync = BassMix.BASS_Mixer_ChannelSetSync(_currentTrack.Channel, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, _currentTrack.NextTrackPos, _currentTrack.TrackSync, new IntPtr(1));

				_WF.AddMarker("Next", _currentTrack.NextTrackPos);
				DrawWave();
			}
		}
		#endregion

		private void timerUpdate_Tick(object sender, EventArgs e)
		{
			//logger.Debug("Bass ChannelGetLevel: Getting volume level from mixer and updating progress bars");
			int level = Bass.BASS_ChannelGetLevel(_mixer);
			progressBarLeft.Value = Utils.LowWord32(level);
			progressBarRight.Value = Utils.HighWord32(level);

			if (_currentTrack != null)
			{
				long pos = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
				labelTime.Text = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, pos), "HHMMSS");
				labelRemain.Text = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, _currentTrack.TrackLength - pos), "HHMMSS");

				DrawWavePosition(pos, _currentTrack.TrackLength);
			}
		}

		private void buttonAddFile_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK == openFileDialog.ShowDialog(this))
			{
				if (File.Exists(openFileDialog.FileName))
				{
					AddToPlayer(openFileDialog.FileName);
				}
			}
		}

		private void buttonSetEnvelope_Click(object sender, EventArgs e)
		{
			if (_currentTrack.Channel != 0)
			{
				BASS_MIXER_NODE[] nodes =
				{
					new BASS_MIXER_NODE(Bass.BASS_ChannelSeconds2Bytes(_mixer, 10d), 1f),
					new BASS_MIXER_NODE(Bass.BASS_ChannelSeconds2Bytes(_mixer, 13d), 0f),
					new BASS_MIXER_NODE(Bass.BASS_ChannelSeconds2Bytes(_mixer, 17d), 0f),
					new BASS_MIXER_NODE(Bass.BASS_ChannelSeconds2Bytes(_mixer, 20d), 1f)
				};
				BassMix.BASS_Mixer_ChannelSetEnvelope(_currentTrack.Channel, BASSMIXEnvelope.BASS_MIXER_ENV_VOL, nodes);
				// already align the envelope position to the current playback position
				// pause mixer
				Bass.BASS_ChannelLock(_mixer, true);
				long pos = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
				// convert source pos to mixer pos
				long envPos = Bass.BASS_ChannelSeconds2Bytes(_mixer, Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, pos));
				BassMix.BASS_Mixer_ChannelSetEnvelopePos(_currentTrack.Channel, BASSMIXEnvelope.BASS_MIXER_ENV_VOL, envPos);
				// resume mixer
				Bass.BASS_ChannelLock(_mixer, false);

				// and show it in our waveform
				_WF.DrawVolume = WaveForm.VOLUMEDRAWTYPE.Solid;
				foreach (BASS_MIXER_NODE node in nodes)
				{
					_WF.AddVolumePoint(node.pos, node.val);
				}
				DrawWave();
			}
		}

		private void buttonRemoveEnvelope_Click(object sender, EventArgs e)
		{
			BassMix.BASS_Mixer_ChannelSetEnvelope(_currentTrack.Channel, BASSMIXEnvelope.BASS_MIXER_ENV_VOL, null);
			_WF.ClearAllVolumePoints();
			_WF.DrawVolume = WaveForm.VOLUMEDRAWTYPE.None;
			DrawWave();
		}

		private void SetTrackToPosition(int source, long newPos)
		{
			logger.Debug("Bass ChannelLock: locking mixer");
			Bass.BASS_ChannelLock(_mixer, true);
			logger.Debug("Bass Mixer ChannelSetPosition: setting mixer or channel to the given position");
			BassMix.BASS_Mixer_ChannelSetPosition(source, newPos);
			logger.Debug("Calculating mixer position from source position");
			long envPos = Bass.BASS_ChannelSeconds2Bytes(_mixer, Bass.BASS_ChannelBytes2Seconds(source, newPos));
			logger.Debug("Bass Mixer ChannelSetEnvelopePos: calculating envelope (it's a part of the playback volume)");
			BassMix.BASS_Mixer_ChannelSetEnvelopePos(source, BASSMIXEnvelope.BASS_MIXER_ENV_VOL, envPos);
			logger.Debug("Bass ChannelLock: unlocking mixer");
			Bass.BASS_ChannelLock(_mixer, false);
		}

		private void BtOpenSchedule_Click(object sender, EventArgs e)
		{
			OfdOpenSchedule.FileName = "";

			DialogResult dr = OfdOpenSchedule.ShowDialog();

			if (dr == DialogResult.OK)
			{
			}
		}

		private void AddToPlayer(string song)
		{
			logger.Debug("Adding new song file (not from schedule) to Player internal playlist");
			Track track = new Track(song);
			AddToPlayer(track);
			//StartPlayer();
		}

		private void AddToPlayer(Track track)
		{
			logger.Debug("Adding track " + track + "to player playlist");

			listBoxPlaylist.Items.Add(track);

			logger.Debug("Bass StreamAddChannel: adding the track to the mixer in PAUSED mode, downmix to stereo, auto free the stream resource when ended");
			BassMix.BASS_Mixer_StreamAddChannel(_mixer, track.Channel, BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_MIXER_DOWNMIX | BASSFlag.BASS_STREAM_AUTOFREE);
			
			SetTrackCuePoints(track);
			SyncTrackWithMixer(track);
		}

		private void AddToLibrary(Track track)
		{
			//openStationDS._Songs.AddSongsRow(track.Filename, 0, track.NextTrackPos, track.TrackLength, 0);
			EditorLoadTrack(track);
		}

		private void BtPlay_Click(object sender, EventArgs e)
		{
			logger.Debug("User command - Play/Next");
			if (!AddScheduleItemToPlayerBGW.IsBusy)
			{
				PlayTrack();
				StopPreviousTrack(500);
			}
			else
			{
				logger.Debug("Cannot execute command since AddScheduleItemToPlayerBGW thread is busy. Try again later");
			}
		}

		private void StopPreviousTrack(int FadeLength)
		{
			if (_previousTrack != null)
			{
				logger.Debug("Bass ChannelSlideAttribute: Fading out and stopping the 'previous' track (for 2 seconds)");
				Bass.BASS_ChannelSlideAttribute(_previousTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, FadeLength);
			}
		}

		private void StopPreviousTrack()
		{
			StopPreviousTrack(2000);
		}

		private void StartPlayer()
		{
			try
			{
				if (_currentTrack != null)
				{
					BeginInvoke((MethodInvoker)delegate()
					            {
					            	logger.Debug("Calling PlayTrack...");
					            	PlayTrack();
					            });
				}
				else
				{
					logger.Debug("Current track is null");
					logger.Debug("Calling PlayTrack...");
					PlayTrack();
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error running StartPlayer!");
				logger.Error(ex.Message);
				if (ex.InnerException != null)
				{
					logger.Error(ex.InnerException);
				}
				logger.Error(ex.Data);
			}
		}

		private void BtToggleZoom_Click(object sender, EventArgs e)
		{
			ToggleZoom();
		}

		private void BtStop_Click(object sender, EventArgs e)
		{
			BeginInvoke((MethodInvoker)delegate()
			            {
			            	StopCurrentTrack();
			            });
		}

		private void StopCurrentTrack()
		{
			if (_currentTrack != null)
			{
				logger.Debug("Bass ChannelSlideAttribute: fade out and stop the current track - "+ _currentTrack +" - (for 2 seconds)");
				Bass.BASS_ChannelSlideAttribute(_currentTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 2000);
			}
		}

		private void BtRemoveTrack_Click(object sender, EventArgs e)
		{
			if (listBoxPlaylist.SelectedItem != null)
			{
				lock (listBoxPlaylist)
				{
					BassMix.BASS_Mixer_ChannelRemove((listBoxPlaylist.SelectedItem as Track).Channel);
					listBoxPlaylist.Items.Remove(listBoxPlaylist.SelectedItem);
				}
			}
		}

		private void DgvLibrary_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (DgvLibrary.SelectedRows != null)
			{
				//EditorLoadTrack(new Track(DgvLibrary.SelectedRows[0].Cells[0].Value.ToString()));
				//Console.Out.WriteLine(DgvLibrary.SelectedRows[0].Cells[0].Value.ToString());
			}
		}

		private void EditorLoadTrack(Track track)
		{
			EditorTrack = track;
			EditorCueTrack();
			EditorGetWaveForm();
			EditorGetTrackData();
		}

		private void EditorCueTrack()
		{
			// add the new track to the mixer (in PAUSED mode!)
			BassMix.BASS_Mixer_StreamAddChannel(_mixer, EditorTrack.Channel, BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_MIXER_DOWNMIX | BASSFlag.BASS_STREAM_AUTOFREE);

			// an BASS_SYNC_END is used to trigger the next track in the playlist (if no POS sync was set)
			//EditorTrack.TrackSync = new SYNCPROC(OnTrackSync);
			//BassMix.BASS_Mixer_ChannelSetSync(EditorTrack.Channel, BASSSync.BASS_SYNC_END, 0L, EditorTrack.TrackSync, new IntPtr(0));
		}

		private void EditorGetTrackData()
		{
			EditorArtistTitleTB.Text = EditorTrack.Tags.artist + " - " + EditorTrack.Tags.title;

			EditorStartTimeNUD.Value = decimal.Parse(EditorTrack.StartTrackPos.ToString()) / 1000;
			EditorSegueTimeNUD.Value = decimal.Parse(EditorTrack.NextTrackPos.ToString()) / 1000;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			AddTrackToLibraryOFD.ShowDialog();
		}

		private void AddTrackToLibraryOFD_FileOk(object sender, CancelEventArgs e)
		{
			Track t = new Track(AddTrackToLibraryOFD.FileName);
			AddToLibrary(t);
		}

		private void CueStartTimeBTN_Click(object sender, EventArgs e)
		{
			_WF.SyncPlayback(_currentTrack.Channel);
			SetTrackToPosition(_currentTrack.Channel, _currentTrack.StartTrackPos);
		}

		private void StartTimeNUD_ValueChanged(object sender, EventArgs e)
		{
			
		}

		private void StartTimeNUD_Click(object sender, EventArgs e)
		{
			bool ValueChanged = (bool)StartTimeNUD.Tag;
			if (ValueChanged)
			{
				_currentTrack.StartTrackPos = long.Parse(((int)StartTimeNUD.Value).ToString())*1000;
				_WF.AddMarker("Start", _currentTrack.StartTrackPos);
				DrawWave();
			}
			StartTimeNUD.Tag = false;
		}

		private void StartTimeNUD_ValueChanged_1(object sender, EventArgs e)
		{
			StartTimeNUD.Tag = true;
		}

		private void EditorPlayBTN_Click(object sender, EventArgs e)
		{
			BeginInvoke((MethodInvoker)delegate()
			            {
			            	BassMix.BASS_Mixer_ChannelSetPosition(EditorTrack.Channel, EditorTrack.StartTrackPos);
			            	BassMix.BASS_Mixer_ChannelPlay(EditorTrack.Channel);

			            });

		}

		private void EditorStopBTN_Click(object sender, EventArgs e)
		{
			BeginInvoke((MethodInvoker)delegate()
			            {
			            	Bass.BASS_ChannelSlideAttribute(EditorTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 0);
			            });
		}

		private void EditorTimer_Tick(object sender, EventArgs e)
		{
			int level = Bass.BASS_ChannelGetLevel(_mixer);
			//progressBarLeft.Value = Utils.LowWord32(level);
			//progressBarRight.Value = Utils.HighWord32(level);

			if (EditorTrack != null)
			{
				long pos = BassMix.BASS_Mixer_ChannelGetPosition(EditorTrack.Channel);
				//labelTime.Text = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(EditorTrack.Channel, pos), "HHMMSS");
				//labelRemain.Text = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(EditorTrack.Channel, EditorTrack.TrackLength - pos), "HHMMSS");

				DrawWavePosition(EditorPB, EditorWf, pos, EditorTrack.TrackLength);
			}
		}

		private void EditorPB_MouseDown(object sender, MouseEventArgs e)
		{
			if (EditorWf == null)
				return;

			bool doubleClick = e.Clicks > 1;
			bool lowerHalf = (e.Y > EditorPB.Height / 2);

			if (lowerHalf && doubleClick)
			{
				ToggleZoom();
			}
			else if (!lowerHalf && e.Button == MouseButtons.Left)
			{
				// left button will set the position
				long pos = EditorWf.GetBytePositionFromX(e.X, EditorPB.Width, _zoomStart, _zoomEnd);
				SetTrackToPosition(EditorTrack.Channel, pos);
			}
			else if (!lowerHalf)
			{
				EditorTrack.NextTrackPos = EditorWf.GetBytePositionFromX(e.X, EditorPB.Width, _zoomStart, _zoomEnd);
				// if there is already a sync set, remove it first
				//if (EditorTrack.NextTrackSync != 0)
				//    BassMix.BASS_Mixer_ChannelRemoveSync(EditorTrack.Channel, EditorTrack.NextTrackSync);

				// right button will set a next track position sync
				//EditorTrack.NextTrackSync = BassMix.BASS_Mixer_ChannelSetSync(EditorTrack.Channel, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, EditorTrack.NextTrackPos, EditorTrack.TrackSync, new IntPtr(1));

				EditorWf.AddMarker("Next", EditorTrack.NextTrackPos);
				DrawWave(EditorPB,EditorWf);
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			logger.Debug("Quitting OpenPlayout");
		}

		private void button2_Click(object sender, EventArgs e)
		{
			AddNextDaySchedule();
		}

		private void timerUpdateSeconds_Tick(object sender, EventArgs e)
		{
			if (_currentTrack != null)
			{
				if (DateTime.Now.Second % 10 == 0)
				{
					logger.Debug(_currentTrack.Tags.title + " - Playback position: " + labelTime.Text);
				}
			}
		}

		private void TrackCuePointsBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Track track = (Track)e.Result;
			SyncTrackWithMixer(track);
		}

		private void SyncTrackWithMixer(Track track)
		{
			logger.Debug("Bass Mixer ChannelSetSync: setting a sync on next track position, calling TrackSync() method when it happens");
			track.TrackSync = new SYNCPROC(OnTrackSync);
			logger.Debug("ChannelSetSync Channel = " + track.Channel);
			track.NextTrackSync = BassMix.BASS_Mixer_ChannelSetSync(track.Channel, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, track.NextTrackPos, track.TrackSync, new IntPtr(1));
		}

		private void TrackCuePointsBGW_DoWork(object sender, DoWorkEventArgs e)
		{
			logger.Debug("Running TrackCuePointQueue check on separate thread...");
			while (true)
			{
				if (TrackCuePointQueue.Count > 0)
				{
					logger.Debug("TrackCuePointQueue contains " + TrackCuePointQueue.Count + " elements.");
					Track tr = (Track)(TrackCuePointQueue.Dequeue());
					logger.Debug("Running SetTrackCuePoints on " + tr.Tags.title);
					SetTrackCuePoints(tr);
					logger.Debug("Running SyncTrackWithMixer on " + tr.Tags.title);
					SyncTrackWithMixer(tr);
				}
			}
		}

		private void AddScheduleItemToPlayerBGW_DoWork(object sender, DoWorkEventArgs e)
		{
			//OpenStationDS.ScheduleRow Item = (OpenStationDS.ScheduleRow)(e.Argument);
			//AddToPlayer(new Track(Item));
		}
		void BindingSource1CurrentChanged(object sender, EventArgs e)
		{
			
		}
	}
}