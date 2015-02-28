﻿using System;
using System.ComponentModel;
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
		private const int SliderSmallChange = 1;
		private const int SliderLargeChange = 32;
		#endregion

		#region Constructors
		public WaveEditor()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			InitializeScrollbar();

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
			
			InitializeScrollbar();

			BassProxy soundEngine = BassProxy.Instance;
			soundEngine.PropertyChanged += BassProxy_PropertyChanged;
			
			customWaveViewer1.RegisterSoundPlayer(soundEngine);
			customWaveViewer1.PropertyChanged += CustomWaveViewer_PropertyChanged;
			//customSpectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
			
			if (File.Exists(fileName)) {
				OpenFile(fileName);
				customWaveViewer1.FitToScreen(); // Force redraw
			}
		}
		#endregion

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
			lblBitdepth.Text = String.Format("{0} Bit", BassProxy.Instance.BitsPerSample);
			lblChannels.Text = String.Format("{0} Ch.", BassProxy.Instance.Channels);
			lblSamplerate.Text = String.Format("{0} Hz", BassProxy.Instance.SampleRate);
			
			string durationTime = TimeSpan.FromSeconds(BassProxy.Instance.ChannelLength).ToString(@"hh\:mm\:ss\.fff");
			int durationSamples = BassProxy.Instance.ChannelSampleLength;
			lblDuration.Text = String.Format("{0} [{1}]", durationTime, durationSamples);
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
		
		#region Key and Mouse handlers
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
		#endregion
		
		#region Label Clicks (Zoom)
		void LblZoomInClick(object sender, EventArgs e)
		{
			customWaveViewer1.ZoomHorizontal(+1);
		}
		void LblZoomOutClick(object sender, EventArgs e)
		{
			customWaveViewer1.ZoomHorizontal(-1);
		}
		void LblZoomSelectionClick(object sender, EventArgs e)
		{
			customWaveViewer1.Zoom(0, customWaveViewer1.WaveformDrawingWidth);
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

		#region Change Labels Methods
		private void ChangeChannelPosition(string channelPos) {
			if(this.InvokeRequired)
			{
				this.Invoke(new Action(() => ChangeChannelPosition(channelPos)));
			}
			else
			{
				lblPlayPosition.Text = channelPos;
			}
		}

		private void ChangeSelection(string selection) {
			if(this.InvokeRequired)
			{
				this.Invoke(new Action(() => ChangeSelection(selection)));
			}
			else
			{
				lblSelection.Text = selection;
			}
		}
		
		private void ChangeZoomRatio(string zoomRatio) {
			if(this.InvokeRequired)
			{
				this.Invoke(new Action(() => ChangeZoomRatio(zoomRatio)));
			}
			else
			{
				lblZoomRatio.Text = zoomRatio;
			}
		}
		
		#endregion
		
		#region PropertyChanged
		void BassProxy_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			BassProxy soundEngine = BassProxy.Instance;
			switch (e.PropertyName)
			{
				case "ChannelPosition":
					string channelPos = TimeSpan.FromSeconds(soundEngine.ChannelPosition).ToString(@"hh\:mm\:ss\.fff");
					ChangeChannelPosition(channelPos);
					break;
				case "IsPlaying":
					break;
				case "ChannelLength":
					break;
				case "SelectionBegin":
					break;
				case "SelectionEnd":
					double selBegin = soundEngine.SelectionBegin.TotalSeconds;
					double selEnd = soundEngine.SelectionEnd.TotalSeconds;
					string selectionBegin = TimeSpan.FromSeconds(selBegin).ToString(@"hh\:mm\:ss\.fff");
					string selectionEnd = TimeSpan.FromSeconds(selEnd).ToString(@"hh\:mm\:ss\.fff");
					string selectionDuration = TimeSpan.FromSeconds(selEnd-selBegin).ToString(@"hh\:mm\:ss\.fff");
					ChangeSelection(string.Format("{0} - {1} ({2})", selectionBegin, selectionEnd, selectionDuration));
					break;
				case "WaveformData":
					//hScrollBar.Maximum = (int) (soundEngine.ChannelSampleLength - 1);
					break;
			}
		}
		
		void CustomWaveViewer_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case "ZoomRatioString":
					ChangeZoomRatio(customWaveViewer1.ZoomRatioString);
					UpdateScrollbar();
					break;
			}
		}
		#endregion
		
		#region Scrollbar
		private void InitializeScrollbar() {
			// Min Usually set to zero or one
			// Max Set this to the number of rows in the file minus the number of rows displayed. If you want to scroll past the last row, then set it larger.
			// Value Where the slider is located.
			// LargeChange Amount Value is changed when the user clicks above or below the slider, or presses PgUp or PgDn keys.
			// SmallChange Amount Value is changed when the user clicks an arrow or presses the up and down arrow keys.
			
			hScrollBar.SmallChange = SliderSmallChange;
			hScrollBar.LargeChange = SliderLargeChange;
			hScrollBar.Value = 0;
			hScrollBar.Minimum = 0;
			hScrollBar.Maximum= 0;
		}
		
		private void UpdateScrollbar() {

			if (customWaveViewer1.EndZoomSamplePosition > 0) {
				
				// calculate the range
				int rangeInSamples = customWaveViewer1.EndZoomSamplePosition - customWaveViewer1.StartZoomSamplePosition;
				int channelSampleLength = BassProxy.Instance.ChannelSampleLength;
				int delta = rangeInSamples / 20;
				
				double ratio = channelSampleLength / rangeInSamples;
				
				hScrollBar.SmallChange = SliderSmallChange;
				hScrollBar.LargeChange = SliderLargeChange;
				hScrollBar.Value = customWaveViewer1.StartZoomSamplePosition;
				hScrollBar.Minimum = 0;
				hScrollBar.Maximum = channelSampleLength;
			}
		}
		
		void HScrollBarScroll(object sender, ScrollEventArgs e)
		{
			int channelSampleLength = BassProxy.Instance.ChannelSampleLength;
			int startPos = customWaveViewer1.StartZoomSamplePosition;
			int endPos = customWaveViewer1.EndZoomSamplePosition;
			endPos = endPos - (startPos-e.NewValue);
			startPos = e.NewValue;
			
			if (e.NewValue > e.OldValue) {
				//customWaveViewer1.ScrollRight();
				customWaveViewer1.ScrollTime(true, channelSampleLength, startPos, endPos);
			} else {
				//customWaveViewer1.ScrollLeft();
				customWaveViewer1.ScrollTime(false, channelSampleLength, startPos, endPos);
			}
		}
		#endregion
		
	}
}