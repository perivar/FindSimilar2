using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Linq;
using System.Collections.Generic; // IEnumerable

using System.Threading;
using System.Threading.Tasks; // For Parallel

using FindSimilar2.AudioProxies; // BassProxy
using CommonUtils; // Arguments

using Soundfingerprinting; // Repository
using Soundfingerprinting.Fingerprinting; // FingerprintService
using Soundfingerprinting.DbStorage; // DatabaseService
using Soundfingerprinting.Hashing; // IPermutations
using Soundfingerprinting.DbStorage.Entities; // Track

using System.Diagnostics; // Stopwatch

namespace FindSimilar2
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		public static string VERSION = "2.0.3";
		public static FileInfo FAILED_FILES_LOG = new FileInfo("failed_files_log.txt");
		public static FileInfo WARNING_FILES_LOG = new FileInfo("warning_files_log.txt");

		// Supported audio files
		public static string[] extensions = { ".wav", ".ogg", ".mp1", ".m1a", ".mp2", ".m2a", ".mpa", ".mus", ".mp3", ".mpg", ".mpeg", ".mp3pro", ".aif", ".aiff", ".bwf", ".wma", ".wmv", ".aac", ".adts", ".mp4", ".m4a", ".m4b", ".mod", ".mdz", ".mo3", ".s3m", ".s3z", ".xm", ".xmz", ".it", ".itz", ".umx", ".mtm", ".flac", ".fla", ".oga", ".ogg", ".aac", ".m4a", ".m4b", ".mp4", ".mpc", ".mp+", ".mpp", ".ac3", ".wma", ".ape", ".mac" };
		
		/// <summary>
		/// Scan a directory recursively and add all the audio files found to the database
		/// </summary>
		/// <param name="path">Path to directory</param>
		/// <param name="repository">Soundfingerprinting Repository</param>
		/// <param name="skipDurationAboveSeconds">Skip files with duration longer than this number of seconds (0 or less disables this)</param>
		/// <param name="silent">true if silent mode (reduced console output)</param>
		public static void ScanDirectory(string path, Repository repository, double skipDurationAboveSeconds, bool silent=false) {
			
			Stopwatch stopWatch = Stopwatch.StartNew();
			
			FAILED_FILES_LOG.Delete();
			WARNING_FILES_LOG.Delete();
			
			// scan directory for audio files
			try
			{
				// By some reason the IOUtils.GetFiles returns a higher count than what seams correct?!
				IEnumerable<string> filesAll =
					Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
					.Where(f => extensions.Contains(Path.GetExtension(f).ToLower()));
				Console.Out.WriteLine("Found {0} files in scan directory.", filesAll.Count());
				
				// Get all already processed files stored in the database and store in memory
				// It seems to work well with huge volumes of files (200k)
				IList<string> filesAlreadyProcessed = repository.DatabaseService.ReadTrackFilenames();
				Console.Out.WriteLine("Database contains {0} already processed files.", filesAlreadyProcessed.Count);

				// find the files that has not already been added to the database
				List<string> filesRemaining = filesAll.Except(filesAlreadyProcessed).ToList();
				Console.Out.WriteLine("Found {0} files remaining in scan directory to be processed.", filesRemaining.Count);

				int filesCounter = 1;
				int filesAllCounter = filesAlreadyProcessed.Count + 1;
				
				#if !DEBUG
				Console.Out.WriteLine("Running in multi-threaded mode!");
				Parallel.ForEach(filesRemaining, file =>
				                 {
				                 	#else
				                 	Console.Out.WriteLine("Running in single-threaded mode!");
				                 	foreach (string file in filesRemaining)
				                 	{
				                 		#endif
				                 		
				                 		var fileInfo = new FileInfo(file);

				                 		// Try to use Un4Seen Bass to check duration
				                 		BassProxy bass = BassProxy.Instance;
				                 		double duration = BassProxy.GetDurationInSeconds(fileInfo.FullName);

				                 		// check if we should skip files longer than x seconds
				                 		if ( (skipDurationAboveSeconds > 0 && duration > 0 && duration < skipDurationAboveSeconds)
				                 		    || skipDurationAboveSeconds <= 0
				                 		    || duration < 0) {

				                 			if(!Analyzer.AnalyzeAndAddComplete(fileInfo, repository)) {
				                 				Console.Out.WriteLine("Failed! Could not generate audio fingerprint for {0}!", fileInfo.Name);
				                 				IOUtils.LogMessageToFile(FAILED_FILES_LOG, fileInfo.FullName);
				                 			} else {
				                 				Console.Out.WriteLine("[{1}/{2} - {3}/{4}] Succesfully added {0} to database. (Thread: {5})", fileInfo.Name, filesCounter, filesRemaining.Count, filesAllCounter, filesAll.Count(), Thread.CurrentThread.ManagedThreadId);
				                 				
				                 				// Threadsafe increment (TODO: doesn't always seem to work?)
				                 				//filesCounter++;
				                 				//filesAllCounter++;
				                 				Interlocked.Increment(ref filesCounter);
				                 				Interlocked.Increment(ref filesAllCounter);
				                 			}
				                 		} else {
				                 			if (!silent) Console.Out.WriteLine("Skipping {0} since duration exceeds limit ({1:0.00} > {2:0.00} sec.)", fileInfo.Name, duration, skipDurationAboveSeconds);
				                 		}
				                 		
				                 		fileInfo = null;
				                 	}
				                 	
				                 	#if !DEBUG
				                 	);
				                 	#endif
				                 	int filesActuallyProcessed = filesCounter -1;
				                 	Console.WriteLine("Added {0} out of a total remaining set of {1} files. (Of {2} files found).", filesActuallyProcessed, filesRemaining.Count(), filesAll.Count());
				                 }
				                 catch (UnauthorizedAccessException UAEx)
				                 {
				                 	Console.WriteLine(UAEx.Message);
				                 }
				                 catch (PathTooLongException PathEx)
				                 {
				                 	Console.WriteLine(PathEx.Message);
				                 }
				                 catch (System.NullReferenceException NullEx) {
				                 	Console.WriteLine(NullEx.Message);
				                 }

				                 Console.WriteLine("Time used: {0}", stopWatch.Elapsed);
				}
			
			
			/// <summary>
			/// Program entry point.
			/// </summary>
			[STAThread]
			private static void Main(string[] args)
			{
				string scanPath = "";
				double skipDurationAboveSeconds = -1; // less than zero disables this
				string queryPath = "";
				int queryId = -1;
				int numToTake = 20;
				bool resetdb = false;
				bool silent = false;

				// Command line parsing
				var CommandLine = new Arguments(args);
				if(CommandLine["match"] != null) {
					queryPath = CommandLine["match"];
				}
				if(CommandLine["matchid"] != null) {
					string matchId = CommandLine["matchid"];
					queryId = int.Parse(matchId);
				}
				if(CommandLine["scandir"] != null) {
					scanPath = CommandLine["scandir"];
				}
				if(CommandLine["skipduration"] != null) {
					double.TryParse(CommandLine["skipduration"], NumberStyles.Number,CultureInfo.InvariantCulture, out skipDurationAboveSeconds);
				}
				if(CommandLine["num"] != null) {
					string num = CommandLine["num"];
					numToTake = int.Parse(num);
				}
				
				resetdb |= CommandLine["resetdb"] != null;
				silent |= CommandLine["silent"] != null;
				
				if(CommandLine["permutations"] != null) {
					Console.WriteLine("Generating hash permutations for used by the Soundfingerprinting methods.");
					Console.WriteLine("Saving to file: {0}", "Soundfingerprinting\\perms.csv");
					Console.WriteLine();
					Analyzer.GenerateAndSavePermutations("Soundfingerprinting\\perms.csv");
					return;
				}
				if(CommandLine["?"] != null) {
					PrintUsage();
					return;
				}
				if(CommandLine["help"] != null) {
					PrintUsage();
					return;
				}
				if(CommandLine["gui"] != null) {
					StartGUI();
					return;
				}
				if (queryPath == "" && queryId == -1 && scanPath == "") {
					PrintUsage();
					return;
				}
				
				// Instansiate soundfingerprinting Repository
				DatabaseService databaseService = DatabaseService.Instance;
				FingerprintService fingerprintService = Analyzer.GetSoundfingerprintingService();
				IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
				var repository = new Repository(permutations, databaseService, fingerprintService);
				
				if (scanPath != "") {
					if (IOUtils.IsDirectory(scanPath)) {
						if (resetdb) {
							// AudioFingerprinting
							databaseService.RemoveFingerprintTable();
							databaseService.AddFingerprintTable();
							databaseService.RemoveHashBinTable();
							databaseService.AddHashBinTable();
							databaseService.RemoveTrackTable();
							databaseService.AddTrackTable();
						}
						Console.WriteLine("FindSimilar. Version {0}.", VERSION);
						ScanDirectory(scanPath, repository, skipDurationAboveSeconds, silent);
					} else {
						Console.Out.WriteLine("No directory found {0}!", scanPath);
					}
				}
				
				if (queryPath != "") {
					var fi = new FileInfo(queryPath);
					FindSoundfingerprinting(fi, repository, numToTake);
					System.Console.ReadLine();
				}
				
				if (queryId != -1) {
					Track track = databaseService.ReadTrackById(queryId);
					if (track != null && track.FilePath != null) {
						var fi = new FileInfo(track.FilePath);
						FindSoundfingerprinting(fi, repository, numToTake);
					} else {
						Console.Out.WriteLine("Track {0} not found!", queryId);
					}
					System.Console.ReadLine();
				}
			}
			
			private static void PrintUsage() {
				Console.WriteLine("FindSimilar. Version {0}.", VERSION);
				Console.WriteLine("Copyright (C) 2012-2015 Per Ivar Nerseth.");
				Console.WriteLine();
				Console.WriteLine("Usage: FindSimilar.exe <Arguments>");
				Console.WriteLine();
				Console.WriteLine("Arguments:");
				Console.WriteLine("\t-scandir=<scan directory path and create audio fingerprints - ignore existing files>");
				Console.WriteLine("\t-match=<path to the wave file to find matches for>");
				Console.WriteLine("\t-matchid=<database id to the wave file to find matches for>");
				Console.WriteLine("\t-permutations\tGenerate Permutation file used by Soundfingerprinting methods");
				Console.WriteLine();
				Console.WriteLine("Optional Arguments:");
				Console.WriteLine("\t-gui\t<open up the Find Similar Client GUI>");
				Console.WriteLine("\t-resetdb\t<clean database, used together with scandir>");
				Console.WriteLine("\t-skipduration=x.x <skip files longer than x seconds, used together with scandir>");
				Console.WriteLine("\t-silent\t<do not output so much info, used together with scandir>");
				Console.WriteLine("\t-num=<number of matches to return when querying>");
				Console.WriteLine();
				Console.WriteLine("\t-? or -help=show this usage help>");

				System.Console.ReadLine();
			}
			
			private static void StartGUI() {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FindSimilarClientForm());
				//Application.Run(new WaveEditor.WaveEditor());
			}
			
			private static void FindSoundfingerprinting(FileInfo fi, Repository repository, int numToTake) {
				if (fi.Exists) {
					List<QueryResult> queryList = Analyzer.SimilarTracksSoundfingerprintingList(fi,
					                                                                            repository,
					                                                                            1,
					                                                                            false,
					                                                                            false,
					                                                                            null).Take(numToTake).ToList();

					foreach (var entry in queryList)
					{
						Console.WriteLine("[{0}]\t{1}   ({2:0.0000})", entry.Id, Path.GetFileName(entry.Path), entry.Similarity);
					}

				} else {
					Console.Out.WriteLine("No file found {0}!", fi);
				}
			}
		}
	}
