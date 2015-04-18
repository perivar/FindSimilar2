using System;
using System.Text; // StringBuilder
using System.IO; // StreamWriter etc.
using System.Linq; // Enumerable.Range
using System.Globalization; // CultureInfo
using System.Collections.Generic;

using Soundfingerprinting; // Repository
using Soundfingerprinting.Image; // ImageService
using Soundfingerprinting.Audio.Services; // IAudioService
using Soundfingerprinting.Fingerprinting; // FingerprintService
using Soundfingerprinting.Fingerprinting.FFT; // ISpectrumService
using Soundfingerprinting.Fingerprinting.Wavelets; // IWaveletService
using Soundfingerprinting.Fingerprinting.WorkUnitBuilder; // WorkUnitParameterObject
using Soundfingerprinting.DbStorage.Entities; // Track
using Soundfingerprinting.Fingerprinting.Configuration; // IFingerprintingConfiguration
using Soundfingerprinting.Hashing; // PermutationGeneratorService

using Mirage; // Dbg
using CommonUtils; // StringUtils

using FindSimilar2.Audio; // AudioFileReader
using FindSimilar2.AudioProxies; // BassProxy

// drawing graph
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;

namespace FindSimilar2
{
	/// <summary>
	/// Description of Analyzer.
	/// </summary>
	public static class Analyzer
	{
		public const bool DEBUG_INFO_VERBOSE = false;
		public const bool DEFAULT_DEBUG_INFO = false;
		public const bool DEBUG_OUTPUT_TEXT = false;
		public const bool DEBUG_DO_INVERSE_TESTS = false;
		
		// Using 32000 (instead of 44100) gives us a max of 16 khz resolution, which is OK for normal adult human hearing
		public const int SAMPLING_RATE = 32000;
		public const int SECONDS_TO_ANALYZE = 60;
		
		// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
		// Matlab multiplies with 2^15 (32768)
		public const int AUDIO_MULTIPLIER = 65536; // 32768 still makes alot of mfcc feature computations fail!
		
		// Soundfingerprinting static variables
		private static IFingerprintingConfiguration fingerprintingConfigCreation = new FullFrequencyFingerprintingConfiguration();
		private static IFingerprintingConfiguration fingerprintingConfigQuerying = new FullFrequencyFingerprintingConfiguration(true);
		
		/// <summary>
		/// Return the Soundfingerprinting Service
		/// </summary>
		/// <returns>the Soundfingerprinting Service</returns>
		public static FingerprintService GetSoundfingerprintingService() {

			// Audio service
			IAudioService audioService = new AudioService();
			
			// Fingerprint Descriptor
			var fingerprintDescriptor = new FingerprintDescriptor();
			
			// SpectrumService
			var spectrumService = new SpectrumService();
			
			// Wavelet Service
			IWaveletDecomposition waveletDecomposition = new StandardHaarWaveletDecomposition();
			IWaveletService waveletService = new WaveletService(waveletDecomposition);

			// Fingerprint Service
			var fingerprintService = new FingerprintService(audioService,
			                                                fingerprintDescriptor,
			                                                spectrumService,
			                                                waveletService);
			
			return fingerprintService;
		}
		
		/// <summary>
		/// Return information from the Audio File
		/// </summary>
		/// <param name="filePath">filepath object</param>
		/// <returns>a WorkUnitParameter object</returns>
		public static WorkUnitParameterObject GetWorkUnitParameterObjectFromAudioFile(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO) {
			var t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("GetWorkUnitParameterObjectFromAudioFile - Error - No Audio Found!");
				return null;
			}
			
			// Name of file being processed
			string fileName = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata, fileName + "_audiodata.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata, fileName + "_audiodata.txt");
			}
			#endif
			
			if (doOutputDebugInfo) {
				DrawGraph(MathUtils.FloatToDouble(audiodata), fileName + "_audiodata.png");
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER);
			
			// zero pad if the audio file is too short to perform a fft
			if (audiodata.Length < (fingerprintingConfigCreation.WindowSize + fingerprintingConfigCreation.Overlap))
			{
				int lenNew = fingerprintingConfigCreation.WindowSize + fingerprintingConfigCreation.Overlap;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// work config
			var param = new WorkUnitParameterObject();
			param.AudioSamples = audiodata;
			param.PathToAudioFile = filePath.FullName;
			param.MillisecondsToProcess = SECONDS_TO_ANALYZE * 1000;
			param.StartAtMilliseconds = 0;
			param.FileName = fileName;
			param.DurationInMs = duration;
			param.Tags = GetTagInfoFromFile(filePath.FullName);

			Dbg.WriteLine ("Get Audio File Parameters - Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return param;
		}
		
		/// <summary>
		/// Method to analyze and add using the soundfingerprinting methods
		/// </summary>
		/// <param name="filePath">full file path</param>
		/// <param name="repository">Soundfingerprinting Repository</param>
		/// <param name="doOutputDebugInfo">decide whether to output debug info like spectrogram and audiofile (default value can be set)</param>
		/// <param name="useHaarWavelet">decide whether to use haar wavelet compression or DCT compression</param>
		/// <returns>true if successful</returns>
		public static bool AnalyzeAndAddSoundfingerprinting(FileInfo filePath, Repository repository, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO, bool useHaarWavelet = true) {
			var t = new DbgTimer();
			t.Start ();

			// get work config from the audio file
			WorkUnitParameterObject param = GetWorkUnitParameterObjectFromAudioFile(filePath);
			if (param == null) return false;
			
			param.FingerprintingConfiguration = fingerprintingConfigCreation;
			string fileName = param.FileName;

			// build track
			var track = new Track();
			track.Title = param.FileName;
			track.TrackLengthMs = (int) param.DurationInMs;
			track.FilePath = param.PathToAudioFile;
			track.Tags = param.Tags;
			track.Id = -1; // this will be set by the insert method
			
			// Get fingerprint signatures using the Soundfingerprinting methods
			double[][] logSpectrogram;
			List<bool[]> fingerprints;
			if (repository.InsertTrackInDatabaseUsingSamples(track, param.FingerprintingConfiguration.NumberOfHashTables, param.FingerprintingConfiguration.NumberOfKeys,  param, out logSpectrogram, out fingerprints)) {

				// store logSpectrogram as Matrix
				try {
					//Comirva.Audio.Util.Maths.Matrix logSpectrogramMatrix = new Comirva.Audio.Util.Maths.Matrix(logSpectrogram);
					//logSpectrogramMatrix = logSpectrogramMatrix.Transpose();
					
					#region Debug for Soundfingerprinting Method
					if (doOutputDebugInfo) {
						// Image Service
						var imageService = new ImageService(repository.FingerprintService.SpectrumService, repository.FingerprintService.WaveletService);
						imageService.GetLogSpectralImages(logSpectrogram, fingerprintingConfigCreation.Stride, fingerprintingConfigCreation.FingerprintLength, fingerprintingConfigCreation.Overlap, 2).Save(fileName + "_specgram_logimages.png");
						
						//logSpectrogramMatrix.DrawMatrixImageLogValues(fileName + "_specgram_logimage.png", true);
						
						if (DEBUG_OUTPUT_TEXT) {
							//logSpectrogramMatrix.WriteCSV(fileName + "_specgram_log.csv", ";");
						}
					}
					#endregion
					
				} catch (Exception) {
					Console.Out.WriteLine("Failed! Could not store log spectrogram as matrix {0}!", fileName);
					// Failed, but ignore!
				}
			} else {
				// failed
				Console.Out.WriteLine("Failed! Could not compute the soundfingerprint {0}!", fileName);
				return false;
			}

			Dbg.WriteLine ("AnalyzeAndAddSoundfingerprinting - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return true;
		}
		
		/// <summary>
		/// Method to analyse and add all the different types of audio features
		/// </summary>
		/// <param name="filePath">full file path</param>
		/// <param name="repository">Soundfingerprinting Repository</param>
		/// <param name="doOutputDebugInfo">decide whether to output debug info like spectrogram and audiofile (default value can be set)</param>
		/// <param name="useHaarWavelet">decide whether to use haar wavelet compression or DCT compression</param>
		/// <returns>true if successful</returns>
		public static bool AnalyzeAndAddComplete(FileInfo filePath, Repository repository, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO, bool useHaarWavelet = true) {
			var t = new DbgTimer();
			t.Start ();
			
			// get work config from the audio file
			WorkUnitParameterObject param = GetWorkUnitParameterObjectFromAudioFile(filePath);
			if (param == null) return false;
			
			param.FingerprintingConfiguration = fingerprintingConfigCreation;
			string fileName = param.FileName;
			
			// build track
			var track = new Track();
			track.Title = param.FileName;
			track.TrackLengthMs = (int) param.DurationInMs;
			track.FilePath = param.PathToAudioFile;
			track.Tags = param.Tags;
			track.Id = -1; // this will be set by the insert method
			
			double[][] logSpectrogram;
			List<bool[]> fingerprints;
			if (repository.InsertTrackInDatabaseUsingSamples(track, param.FingerprintingConfiguration.NumberOfHashTables, param.FingerprintingConfiguration.NumberOfKeys, param, out logSpectrogram, out fingerprints)) {

				// store logSpectrogram as Matrix
				try {
					//Comirva.Audio.Util.Maths.Matrix logSpectrogramMatrix = new Comirva.Audio.Util.Maths.Matrix(logSpectrogram);
					//logSpectrogramMatrix = logSpectrogramMatrix.Transpose();
					
					#region Output debugging information (Saving spectrograms and/or csv files)
					if (doOutputDebugInfo) {
						//logSpectrogramMatrix.DrawMatrixImageLogValues(fileName + "_matrix_spectrogram.png", true);

						if (DEBUG_OUTPUT_TEXT) {
							//logSpectrogramMatrix.WriteCSV(fileName + "_matrix_spectrogram.csv", ";");
						}

						// Save debug images using fingerprinting methods
						SaveFingerprintingDebugImages(fileName, logSpectrogram, fingerprints, repository.FingerprintService, param.FingerprintingConfiguration);
					}
					#endregion
					
					// Insert Statistical Cluster Model Similarity Audio Feature as well
					//if (!AnalyseAndAddScmsUsingLogSpectrogram(logSpectrogramMatrix, param, db, track.Id, doOutputDebugInfo, useHaarWavelet)) {
					//	Dbg.WriteLine("AnalyzeAndAddComplete - Failed inserting Statistical Cluster Model Similarity Audio Feature");
					//	// Failed, but ignore!
					//}
				} catch (Exception e) {
					Dbg.WriteLine("AnalyzeAndAddComplete - Failed creating Statistical Cluster Model Similarity Audio Feature");
					Dbg.WriteLine(e.Message);
					// Failed, but ignore!
				}
			} else {
				// Failed
				return false;
			}
			
			Dbg.WriteLine("AnalyzeAndAddComplete - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return true;
		}
		
		#region Find Similar Tracks using Soundfingerprinting Methods
		/// <summary>
		/// Query the database for perceptually similar tracks using the sound fingerprinting methods
		/// </summary>
		/// <param name="filePath">input file</param>
		/// <param name="repository">the database (repository)</param>
		/// <returns>a dictionary of similar tracks</returns>
		public static Dictionary<Track, double> SimilarTracksSoundfingerprinting(FileInfo filePath, Repository repository) {
			var t = new DbgTimer();
			t.Start ();

			// get work config from the audio file
			WorkUnitParameterObject param = GetWorkUnitParameterObjectFromAudioFile(filePath);
			if (param == null) {
				return null;
			}
			
			param.FingerprintingConfiguration = fingerprintingConfigQuerying;
			
			// Find similar using 0 for threshold tables, meaning all matches
			Dictionary<Track, double> candidates = repository.FindSimilarFromAudioSamples(param.FingerprintingConfiguration.NumberOfHashTables,
			                                                                              param.FingerprintingConfiguration.NumberOfKeys,
			                                                                              0,
			                                                                              param);

			Dbg.WriteLine ("SimilarTracksSoundfingerprinting - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return candidates;
		}
		
		/// <summary>
		/// Query the database for perceptually similar tracks using the sound fingerprinting methods
		/// </summary>
		/// <param name="filePath">input file</param>
		/// <param name="repository">the database (repository)</param>
		/// <param name="thresholdTables">Minimum number of hash tables that must be found for one signature to be considered a candidate (0 and 1 = return all candidates, 2+ = return only exact matches)</param>
		/// <param name="optimizeSignatureCount">Reduce the number of signatures in order to increase the search performance</param>
		/// <param name="doSearchEverything">disregard the local sensitivity hashes and search the whole database</param>
		/// <param name="splashScreen">The "please wait" splash screen (or null)</param>
		/// <returns>a list of query results objects (e.g. similar tracks)</returns>
		public static List<QueryResult> SimilarTracksSoundfingerprintingList(FileInfo filePath,
		                                                                     Repository repository,
		                                                                     int thresholdTables,
		                                                                     bool optimizeSignatureCount,
		                                                                     bool doSearchEverything,
		                                                                     SplashSceenWaitingForm splashScreen) {
			var t = new DbgTimer();
			t.Start ();

			if (splashScreen != null) splashScreen.SetProgress(0, "Reading audio file ...");
			
			// get work config from the audio file
			WorkUnitParameterObject param = GetWorkUnitParameterObjectFromAudioFile(filePath);
			if (param == null) {
				if (splashScreen != null) splashScreen.SetProgress(0, "Failed reading audio file!");
				return null;
			}
			
			param.FingerprintingConfiguration = fingerprintingConfigQuerying;
			
			if (splashScreen != null) splashScreen.SetProgress(1, "Successfully reading audio file!");

			// This is how the threshold tables work:
			// each signature created from a query file we retrieve a number of candidates
			// based on how many fingerprints that are associated to the same hash bucket.
			// if the number of fingerprints associated to the same hash bucket is relatively high
			// the likelyhood for this being an exact match is also very high.
			// Therefore a value of 0 or 1 basically means return every track that has an association
			// to the same hash bucket, while a number higher than that increases the accuracy for
			// only matching identical matches.
			// 0 and 1 returns many matches
			// 2 returns sometimes only the one we search for (exact match)
			List<QueryResult> similarFiles = repository.FindSimilarFromAudioSamplesList(param.FingerprintingConfiguration.NumberOfHashTables,
			                                                                            param.FingerprintingConfiguration.NumberOfKeys,
			                                                                            thresholdTables,
			                                                                            param,
			                                                                            optimizeSignatureCount,
			                                                                            doSearchEverything,
			                                                                            splashScreen);

			Dbg.WriteLine ("SimilarTracksSoundfingerprintingList - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return similarFiles;
		}
		#endregion
		
		#region Read Tag Info From Files using BASS
		/// <summary>
		/// Read tags from file using the BASS plugin
		/// </summary>
		/// <param name="filePath">filepath to file</param>
		/// <returns>a dictionary with tag names and tag values</returns>
		private static Dictionary<string, string> GetTagInfoFromFile(string filePath) {
			
			// Read TAGs using BASS
			Un4seen.Bass.AddOn.Tags.TAG_INFO tag_info = BassProxy.GetTagInfoFromFile(filePath);

			var tags = new Dictionary<string, string>();
			if (tag_info != null) {
				//if (tag_info.title != string.Empty) tags.Add("title", CleanTagValue(tag_info.title));
				if (tag_info.artist != string.Empty) tags.Add("artist", CleanTagValue(tag_info.artist));
				if (tag_info.album != string.Empty) tags.Add("album", CleanTagValue(tag_info.album));
				if (tag_info.albumartist != string.Empty) tags.Add("albumartist", CleanTagValue(tag_info.albumartist));
				if (tag_info.year != string.Empty) tags.Add("year", CleanTagValue(tag_info.year));
				if (tag_info.comment != string.Empty) tags.Add("comment", CleanTagValue(tag_info.comment));
				if (tag_info.genre != string.Empty) tags.Add("genre", CleanTagValue(tag_info.genre));
				if (tag_info.track != string.Empty) tags.Add("track", CleanTagValue(tag_info.track));
				if (tag_info.disc != string.Empty) tags.Add("disc", CleanTagValue(tag_info.disc));
				if (tag_info.copyright != string.Empty) tags.Add("copyright", CleanTagValue(tag_info.copyright));
				if (tag_info.encodedby != string.Empty) tags.Add("encodedby", CleanTagValue(tag_info.encodedby));
				if (tag_info.composer != string.Empty) tags.Add("composer", CleanTagValue(tag_info.composer));
				if (tag_info.publisher != string.Empty) tags.Add("publisher", CleanTagValue(tag_info.publisher));
				if (tag_info.lyricist != string.Empty) tags.Add("lyricist", CleanTagValue(tag_info.lyricist));
				if (tag_info.remixer != string.Empty) tags.Add("remixer", CleanTagValue(tag_info.remixer));
				if (tag_info.producer != string.Empty) tags.Add("producer", CleanTagValue(tag_info.producer));
				if (tag_info.bpm != string.Empty) tags.Add("bpm", CleanTagValue(tag_info.bpm));
				//if (tag_info.filename != string.Empty) tags.Add("filename", CleanTagValue(tag_info.filename));
				tags.Add("channelinfo", tag_info.channelinfo.ToString());
				//if (tag_info.duration > 0) tags.Add("duration", tag_info.duration.ToString());
				if (tag_info.bitrate > 0) tags.Add("bitrate", tag_info.bitrate.ToString());
				if (tag_info.replaygain_track_gain != -100f) tags.Add("replaygain_track_gain", tag_info.replaygain_track_gain.ToString());
				if (tag_info.replaygain_track_peak != -1f) tags.Add("replaygain_track_peak", tag_info.replaygain_track_peak.ToString());
				if (tag_info.conductor != string.Empty) tags.Add("conductor", CleanTagValue(tag_info.conductor));
				if (tag_info.grouping != string.Empty) tags.Add("grouping", CleanTagValue(tag_info.grouping));
				if (tag_info.mood != string.Empty) tags.Add("mood", CleanTagValue(tag_info.mood));
				if (tag_info.rating != string.Empty) tags.Add("rating", CleanTagValue(tag_info.rating));
				if (tag_info.isrc != string.Empty) tags.Add("isrc", CleanTagValue(tag_info.isrc));
				
				foreach(var nativeTag in tag_info.NativeTags) {
					string[] keyvalue = nativeTag.Split('=');
					if (keyvalue.Length > 1) {
						tags.Add(keyvalue[0], CleanTagValue(keyvalue[1]));
					} else {
						tags.Add(keyvalue[0], "");
					}					
				}
			}
			return tags;
		}
		
		/// <summary>
		/// Replace invalid characters with empty strings.
		/// </summary>
		/// <param name="uncleanValue">string</param>
		/// <returns>formatted string</returns>
		private static string CleanTagValue(string uncleanValue) {
			return StringUtils.RemoveInvalidCharacters(uncleanValue);
		}
		#endregion
		
		#region Generate Permutations used by MinHash methods

		/// <summary>
		/// Generate the permutations according to a greedy random algorithm
		/// </summary>
		/// <returns>String to be save into the file</returns>
		private static string GeneratePermutations()
		{
			// Original perm.csv file has 100 rows with 255 values in it seperated by comma
			int hashTables = fingerprintingConfigCreation.NumberOfHashTables;
			int keysPerTable = fingerprintingConfigCreation.NumberOfKeys;

			int startIndex = fingerprintingConfigCreation.StartFingerprintIndex;
			int endIndex = fingerprintingConfigCreation.EndFingerprintIndex;
			
			var final = new StringBuilder();
			Dictionary<int, int[]> perms = null;
			perms = PermutationGeneratorService.GenerateRandomPermutationsUsingUniqueIndexes(hashTables, keysPerTable, startIndex, endIndex);

			if (perms != null)
				foreach (KeyValuePair<int, int[]> perm in perms)
			{
				var permutation = new StringBuilder();
				foreach (int t in perm.Value)
					permutation.Append(t + ",");

				final.AppendLine(permutation.ToString());
			}
			return final.ToString();
		}

		/// <summary>
		/// Generate the permutations according to a greedy random algorithm and save to the outputfile
		/// </summary>
		/// <param name="outputFilePath">output file, e.g. 'perms-new.csv'</param>
		/// <example>
		/// Analyzer.GenerateAndSavePermutations("perms-new.csv");
		/// </example>
		public static void GenerateAndSavePermutations(string outputFilePath) {
			string permutations = GeneratePermutations();

			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.Write(permutations);
			}
		}
		#endregion
		
		#region Utility Methods to draw graphs, spectrograms and output text or text files
		
		public static void SaveFingerprintingDebugImages(string fileName, double[][] logSpectrogram, List<bool[]> fingerprints, FingerprintService fingerprintService, IFingerprintingConfiguration fingerprintConfig) {
			
			var imageService = new ImageService(fingerprintService.SpectrumService, fingerprintService.WaveletService);
			
			const int fingerprintsPerRow = 2;
			imageService.GetSpectrogramImage(logSpectrogram, logSpectrogram.Length, logSpectrogram[0].Length).Save(fileName + "_spectrogram.png");
			imageService.GetWaveletsImages(logSpectrogram, fingerprintConfig.Stride, fingerprintConfig.FingerprintLength, fingerprintConfig.Overlap, fingerprintsPerRow).Save(fileName + "_wavelets.png");
			imageService.GetLogSpectralImages(logSpectrogram, fingerprintConfig.Stride, fingerprintConfig.FingerprintLength, fingerprintConfig.Overlap, fingerprintsPerRow).Save(fileName + "_spectrograms.png");
			imageService.GetImageForFingerprints(fingerprints, fingerprintConfig.FingerprintLength, fingerprintConfig.LogBins, fingerprintsPerRow).Save(fileName + "_fingerprints.png");
		}
		
		/// <summary>
		/// Graphs an array of doubles varying between -1 and 1
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="fileName">filename to save png to</param>
		/// <param name="onlyCanvas">true if no borders should be printed</param>
		public static void DrawGraph(double[] data, string fileName, bool onlyCanvas=false)
		{
			var myPane = new GraphPane( new RectangleF( 0, 0, 1200, 600 ), "", "", "" );
			
			if (onlyCanvas) {
				myPane.Chart.Border.IsVisible = false;
				myPane.Chart.Fill.IsVisible = false;
				myPane.Fill.Color = Color.Black;
				myPane.Margin.All = 0;
				myPane.Title.IsVisible = false;
				myPane.XAxis.IsVisible = false;
				myPane.YAxis.IsVisible = false;
			}
			myPane.XAxis.Scale.Max = data.Length - 1;
			myPane.XAxis.Scale.Min = 0;
			
			// add pretty stuff
			myPane.Fill = new Fill( Color.WhiteSmoke, Color.Lavender, 0F );
			myPane.Chart.Fill = new Fill( Color.FromArgb( 255, 255, 245 ),
			                             Color.FromArgb( 255, 255, 190 ), 90F );
			
			var timeData = Enumerable.Range(0, data.Length)
				.Select(i => (double) i)
				.ToArray();
			myPane.AddCurve(null, timeData, data, Color.Blue, SymbolType.None);
			
			var bm = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bm ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
		/// <summary>Writes the float array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(float[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}

		/// <summary>Writes the double array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="data">data</param>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(double[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(float[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(double[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		#endregion
		
	}
}
