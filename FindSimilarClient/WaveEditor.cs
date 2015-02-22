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
				soundEngine.OpenFile(fileName);
			}
		}
		
		void BassProxy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			BassProxy soundEngine = BassProxy.Instance;
			switch (e.PropertyName)
			{
				case "ChannelPosition":
					//txtTime.Text = TimeSpan.FromSeconds(soundEngine.ChannelPosition).ToString();
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
			}
		}
		
		void BtnBrowseClick(object sender, EventArgs e)
		{
			openFileDialog.Filter = "Audio Files(*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				BassProxy.Instance.OpenFile(openFileDialog.FileName);
				txtFilePath.Text = openFileDialog.FileName;
			}
		}
		
		void BtnPlayClick(object sender, EventArgs e)
		{
			if (BassProxy.Instance.CanPlay)
				BassProxy.Instance.Play();
		}
		
		void BtnPauseClick(object sender, EventArgs e)
		{
			if (BassProxy.Instance.CanPause)
				BassProxy.Instance.Pause();
		}
		
		void BtnStopClick(object sender, EventArgs e)
		{
			if (BassProxy.Instance.CanStop)
				BassProxy.Instance.Stop();
			
			BassProxy.Instance.ChannelPosition = 0;
			BassProxy.Instance.SelectionBegin = TimeSpan.FromMilliseconds(0);
			BassProxy.Instance.SelectionEnd = TimeSpan.FromMilliseconds(0);
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			
			if (e.KeyCode == Keys.Space) {
				if (BassProxy.Instance.CanPlay)
					BassProxy.Instance.Play();
			}
		}
	}
}