using System;
using System.Windows.Forms;
using System.IO;

using FindSimilar2.AudioProxies;

namespace FindSimilar2
{
	/// <summary>
	/// Wave Editor
	/// </summary>
	public partial class WaveEditor : Form
	{
		#region Private constants
		private const int DefaultScale = 1;
		private const int SliderSmallChange = 1;
		private const int SliderLargeChange = 32;

		public const int HorizontalMovementFast = 1024;
		public const int HorizontalMovementNormal = 256;
		public const int HorizontalMovementSlow = 1;
		#endregion

		public WaveEditor()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			BassProxy soundEngine = BassProxy.Instance;
			soundEngine.PropertyChanged += BassProxy_PropertyChanged;
			
			customWaveViewer1.RegisterSoundPlayer(soundEngine);
			//customSpectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
			
			//hScrollBar.SmallChange = SliderSmallChange;
			//hScrollBar.LargeChange = SliderLargeChange;
		}

		public WaveEditor(string fileName)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			BassProxy soundEngine = BassProxy.Instance;
			soundEngine.PropertyChanged += BassProxy_PropertyChanged;
			
			customWaveViewer1.RegisterSoundPlayer(soundEngine);
			//customSpectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
			
			if (File.Exists(fileName)) {
				OpenFile(fileName);
			}
		}

		#region Play and Open file methods
		void OpenFileDialog()
		{
			openFileDialog.Filter = "Audio Files(*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string fileName = openFileDialog.FileName;
				OpenFile(fileName);
			}
		}
		
		void OpenFile(string fileName) {
			BassProxy.Instance.OpenFile(fileName);
			lblFilename.Text = Path.GetFileName(fileName);
			lblBitdepth.Text = String.Format("{0} bit", BassProxy.Instance.BitsPerSample);
			lblChannels.Text = String.Format("{0} channels", BassProxy.Instance.Channels);
			lblSamplerate.Text = String.Format("{0} Hz", BassProxy.Instance.SampleRate);
			
			lblDuration.Text = String.Format("{0} samples", BassProxy.Instance.ChannelSampleLength);
		}
		
		void TogglePlay()
		{
			// Toggle Play
			if (BassProxy.Instance.IsPlaying) {
				if (BassProxy.Instance.CanPause) {
					BassProxy.Instance.Pause();
				}
			} else {
				if (BassProxy.Instance.CanPlay) {
					BassProxy.Instance.Play();
				}
			}
		}
		
		void Stop()
		{
			if (BassProxy.Instance.CanStop)
				BassProxy.Instance.Stop();
			
			BassProxy.Instance.ChannelPosition = 0;
			BassProxy.Instance.SelectionBegin = TimeSpan.FromMilliseconds(0);
			BassProxy.Instance.SelectionEnd = TimeSpan.FromMilliseconds(0);
		}
		#endregion
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			
			// Space toggles play
			if (e.KeyCode == Keys.Space) {
				TogglePlay();
				
			} else if (((Control.ModifierKeys & Keys.Control) == Keys.Control)
			           && e.KeyCode == Keys.A)
			{
				customWaveViewer1.SelectAll();
			}
		}
		
		#region Label Clicks (Zoom)
		void LblZoomInClick(object sender, EventArgs e)
		{
			
		}
		void LblZoomOutClick(object sender, EventArgs e)
		{
			
		}
		void LblZoomSelectionClick(object sender, EventArgs e)
		{
			
		}
		void LblZoomInAmplitudeClick(object sender, EventArgs e)
		{
			customWaveViewer1.ZoomInAmplitude();
		}
		void LblZoomOutAmplitudeClick(object sender, EventArgs e)
		{
			customWaveViewer1.ZoomOutAmplitude();
		}
		void LblIncreaseSelectionClick(object sender, EventArgs e)
		{
			
		}
		void LblDecreaseSelectionClick(object sender, EventArgs e)
		{
			
		}
		#endregion
		
		void BassProxy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			BassProxy soundEngine = BassProxy.Instance;
			switch (e.PropertyName)
			{
				case "ChannelPosition":
					lblPlayPosition.Text = TimeSpan.FromSeconds(soundEngine.ChannelPosition).ToString();
					break;
				case "IsPlaying":
					Console.Out.WriteLine("IsPlaying");
					break;
				case "ChannelLength":
					Console.Out.WriteLine("ChannelLength");
					break;
				case "SelectionBegin":
					Console.Out.WriteLine("SelectionBegin");
					break;
				case "SelectionEnd":
					Console.Out.WriteLine("SelectionEnd");
					break;
				case "WaveformData":
					hScrollBar.Maximum = (int) (soundEngine.ChannelSampleLength - 1);
					break;
			}
		}
		
		void HScrollBarValueChanged(object sender, EventArgs e)
		{
		}
		
		void HScrollBarScroll(object sender, ScrollEventArgs e)
		{
			if (e.NewValue > e.OldValue) {
				customWaveViewer1.ScrollRight();
			} else {
				customWaveViewer1.ScrollLeft();
			}
		}
		
	}
}