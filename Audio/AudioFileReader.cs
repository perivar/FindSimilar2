using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using FindSimilar2.AudioProxies; // BassProxy
using CommonUtils; // RiffRead
using Mirage; // Dbg

namespace FindSimilar2.Audio
{
	public static class AudioFileReader
	{
		static readonly object _locker = new object();
		
		public static float[] Decode(string fileIn, int srate, int secondsToAnalyze)
		{
			var t = new DbgTimer();
			t.Start();

			float[] floatBuffer = null;
			
			// check if file exists
			if (!string.IsNullOrEmpty(fileIn)) {
				var fi = new FileInfo(fileIn);
				if (!fi.Exists) {
					Console.Out.WriteLine("No file found {0}!", fileIn);
					return null;
				}
			}
			
			// Try to use Un4Seen Bass to check duration
			BassProxy bass = BassProxy.Instance;
			double duration = BassProxy.GetDurationInSeconds(fileIn);
			string logMessage = fileIn;
			if (duration > 0) {
				Dbg.WriteLine("Using BASS to decode the file ...");

				// duration in seconds
				if (duration > secondsToAnalyze) {
					// find segment to extract
					double startSeconds = (duration/2-(secondsToAnalyze/2));
					if (startSeconds < 0) {
						startSeconds = 0;
					}
					try {
						floatBuffer = BassProxy.ReadMonoFromFile(fileIn, srate, secondsToAnalyze*1000, (int) (startSeconds*1000));
					} catch (Exception e) {
						logMessage += " (" + e.Message + ")";
					}
					// if this failes, the duration read from the tags was wrong or it is something wrong with the audio file
					if (floatBuffer == null) {
						IOUtils.LogMessageToFile(Program.WARNING_FILES_LOG, logMessage);
					}
				} else {
					// return whole file
					try {
						floatBuffer = BassProxy.ReadMonoFromFile(fileIn, srate, 0, 0);
					} catch (Exception e) {
						logMessage += " (" + e.Message + ")";
					}
					// if this failes, the duration read from the tags was wrong or it is something wrong with the audio file
					if (floatBuffer == null) {
						IOUtils.LogMessageToFile(Program.WARNING_FILES_LOG, logMessage);
					}
				}
			}

			// Bass failed reading or never even tried, so use another alternative
			if (floatBuffer == null) {
				Dbg.WriteLine("Using MPlayer and SOX to decode the file ...");
				fileIn = Regex.Replace(fileIn, "%20", " ");
				floatBuffer = DecodeUsingMplayerAndSox(fileIn, srate, secondsToAnalyze);
			}
			return floatBuffer;
		}

		public static float[] DecodeUsingSox(string fileIn, int srate, int secondsToAnalyze) {

			lock (_locker) {
				using (var toraw = new Process())
				{
					fileIn = Regex.Replace(fileIn, "%20", " ");
					var t = new DbgTimer();
					t.Start();
					
					Dbg.WriteLine("Decoding: " + fileIn);
					
					String raw = IOUtils.GetTempFilePathWithExtension("raw");
					Dbg.WriteLine("Temporary raw file: " + raw);
					
					toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
					toraw.StartInfo.Arguments = " \"" + fileIn + "\" -r " + srate + " -e float -b 32 -G -t raw \"" + raw + "\" channels 1";
					toraw.StartInfo.UseShellExecute = false;
					toraw.StartInfo.RedirectStandardOutput = true;
					toraw.StartInfo.RedirectStandardError = true;
					toraw.Start();
					toraw.WaitForExit();
					
					int exitCode = toraw.ExitCode;
					// 0 = succesfull
					// 1 = partially succesful
					// 2 = failed
					if (exitCode != 0) {
						string standardError = toraw.StandardError.ReadToEnd();
						Console.Out.WriteLine(standardError);
						return null;
					}
					
					#if DEBUG
					string standardOutput = toraw.StandardOutput.ReadToEnd();
					Console.Out.WriteLine(standardOutput);
					#endif
					
					float[] floatBuffer;
					FileStream fs = null;
					try {
						var fi = new FileInfo(raw);
						fs = fi.OpenRead();
						int bytes = (int)fi.Length;
						int samples = bytes/sizeof(float);
						if ((samples * sizeof(float)) != bytes)
							return null;

						// if the audio file is larger than seconds to analyze,
						// find a proper section to exctract
						if (bytes > secondsToAnalyze * srate * sizeof(float)) {
							int seekto = (bytes/2) - ((secondsToAnalyze/2) * sizeof(float) * srate);
							Dbg.WriteLine("Extracting section: seekto = " + seekto);
							bytes = (secondsToAnalyze) * srate * sizeof(float);
							fs.Seek((samples/2-(secondsToAnalyze/2) * srate) * sizeof(float), SeekOrigin.Begin);
						}
						
						var br = new BinaryReader(fs);
						var bytesBuffer = new byte[bytes];
						br.Read(bytesBuffer, 0, bytesBuffer.Length);
						
						int items = (int)bytes / sizeof(float);
						floatBuffer = new float[items];
						
						for (int i = 0; i < items; i++) {
							floatBuffer[i] = BitConverter.ToSingle(bytesBuffer, i * sizeof(float)); // * 65536.0f;
						}
						
					} catch (FileNotFoundException) {
						floatBuffer = null;
						
					} finally {
						if (fs != null)
							fs.Close();
						try
						{
							File.Delete(raw);
						}
						catch (IOException io)
						{
							Console.WriteLine(io);
						}
						
						Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
					}
					return floatBuffer;
				}
			}
		}

		public static float[] DecodeUsingMplayerAndSox(string fileIn, int srate, int secondsToAnalyze) {
			
			lock (_locker) {
				using (var tosoxreadable = new Process())
				{
					fileIn = Regex.Replace(fileIn, "%20", " ");

					var t = new DbgTimer();
					t.Start();
					
					Dbg.WriteLine("Decoding: " + fileIn);
					
					String soxreadablewav = IOUtils.GetTempFilePathWithExtension("wav");
					Dbg.WriteLine("Temporary wav file: " + soxreadablewav);
					
					tosoxreadable.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
					tosoxreadable.StartInfo.Arguments = " -quiet -vc null -vo null -ao pcm:fast:waveheader \"" + fileIn + "\" -ao pcm:file=\\\"" + soxreadablewav + "\\\"";
					tosoxreadable.StartInfo.UseShellExecute = false;
					tosoxreadable.StartInfo.RedirectStandardOutput = true;
					tosoxreadable.StartInfo.RedirectStandardError = true;
					tosoxreadable.Start();
					tosoxreadable.WaitForExit();
					
					int exitCode = tosoxreadable.ExitCode;
					// 0 = succesfull
					// 1 = partially succesful
					// 2 = failed
					if (exitCode != 0) {
						string standardError = tosoxreadable.StandardError.ReadToEnd();
						Console.Out.WriteLine(standardError);
						return null;
					}
					
					#if DEBUG
					string standardOutput = tosoxreadable.StandardOutput.ReadToEnd();
					Console.Out.WriteLine(standardOutput);
					#endif
					
					float[] floatBuffer = null;
					if (File.Exists(soxreadablewav)) {
						floatBuffer = DecodeUsingSox(soxreadablewav, srate, secondsToAnalyze);
						try
						{
							File.Delete(soxreadablewav);
						}
						catch (IOException io)
						{
							Console.WriteLine(io);
						}
					}
					Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
					return floatBuffer;
				}
			}
		}
		
		public static float[] DecodeUsingMplayer(string fileIn, int srate) {
			
			lock (_locker) {
				using (var towav = new Process())
				{
					fileIn = Regex.Replace(fileIn, "%20", " ");
					
					var t = new DbgTimer();
					t.Start();
					
					Dbg.WriteLine("Decoding: " + fileIn);
					
					String wav = IOUtils.GetTempFilePathWithExtension("wav");
					Dbg.WriteLine("Temporary wav file: " + wav);
					
					towav.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
					towav.StartInfo.Arguments = " -quiet -ao pcm:fast:waveheader \"" + fileIn + "\" -format floatle -af resample="+srate+":0:2,pan=1:0.5:0.5 -channels 1 -vo null -vc null -ao pcm:file=\\\"" + wav + "\\\"";
					towav.StartInfo.UseShellExecute = false;
					towav.StartInfo.RedirectStandardOutput = true;
					towav.StartInfo.RedirectStandardError = true;
					towav.Start();
					towav.WaitForExit();
					
					int exitCode = towav.ExitCode;
					// 0 = succesfull
					// 1 = partially succesful
					// 2 = failed
					if (exitCode != 0) {
						string standardError = towav.StandardError.ReadToEnd();
						Console.Out.WriteLine(standardError);
						return null;
					}
					
					#if DEBUG
					string standardOutput = towav.StandardOutput.ReadToEnd();
					Console.Out.WriteLine(standardOutput);
					#endif
					
					var riff = new RiffRead(wav);
					riff.Process();
					float[] floatBuffer = riff.SoundData[0];
					try
					{
						File.Delete(wav);
					}
					catch (IOException io)
					{
						Console.WriteLine(io);
					}
					
					Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
					return floatBuffer;
				}
			}
		}
	}
}
