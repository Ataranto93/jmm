﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using NLog;
using System.Net;
using JMMClient.Utilities;
using JMMClient.UserControls;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JMMClient
{
	public class Utils
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		extern static IntPtr GetCurrentProcess();
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		extern static IntPtr GetModuleHandle(string moduleName);
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);


		public static string DownloadWebPage(string url)
		{
			try
			{
				logger.Trace("DownloadWebPage: {0}", url);

				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
				webReq.Timeout = 30000; // 30 seconds
				webReq.Proxy = null;
				webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
				webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

				HttpWebResponse WebResponse = (HttpWebResponse)webReq.GetResponse();

				Stream responseStream = WebResponse.GetResponseStream();
				String enco = WebResponse.CharacterSet;
				Encoding encoding = null;
				if (!String.IsNullOrEmpty(enco))
					encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
				if (encoding == null)
					encoding = Encoding.Default;
				StreamReader Reader = new StreamReader(responseStream, encoding);

				string output = Reader.ReadToEnd();

				WebResponse.Close();
				responseStream.Close();

				logger.Trace("DownloadWebPage Response: {0}", output);

				return output;
			}
			catch (Exception ex)
			{
				string msg = "---------- ERROR IN DOWNLOAD WEB PAGE ---------" + Environment.NewLine +
					url + Environment.NewLine +
					ex.ToString() + Environment.NewLine + "------------------------------------";
				logger.Error(msg);

				// if the error is a 404 error it may mean that there is a bad series association
				// so lets log it to the web cache so we can investigate
				if (ex.ToString().Contains("(404) Not Found"))
				{
				}

				return "";
			}
		}

		public static void ShowErrorMessage(Exception ex)
		{
			MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			logger.ErrorException(ex.ToString(), ex);
		}

		public static void ShowErrorMessage(string msg)
		{
			MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			logger.Error(msg);
		}

		public static string FormatAniDBRating(double rat)
		{
			return String.Format("{0:0.00}", rat);

		}

		public static string FormatPercentage(double val)
		{
			return String.Format("{0:0.0}%", val);

		}

		public static string FormatSecondsToDisplayTime(int secs)
		{
			TimeSpan t = TimeSpan.FromSeconds(secs);

			if (t.Hours > 0)
				return string.Format("{0}:{1}:{2}", t.Hours, t.Minutes.ToString().PadLeft(2, '0'), t.Seconds.ToString().PadLeft(2, '0'));
			else
				return string.Format("{0}:{1}", t.Minutes, t.Seconds.ToString().PadLeft(2, '0'));
		}

		public static BackgroundWorker RunBackgroundWork(DoWorkEventHandler doWorkEvent,
			RunWorkerCompletedEventHandler completedEvent,
			ProgressChangedEventHandler progressEvent,
			object parameter)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;

			if (progressEvent != null)
			{
				worker.WorkerReportsProgress = true;
				worker.ProgressChanged += progressEvent;
			}

			if (doWorkEvent != null) worker.DoWork += doWorkEvent;
			if (completedEvent != null) worker.RunWorkerCompleted += completedEvent;

			if (parameter == null)
				worker.RunWorkerAsync();
			else
				worker.RunWorkerAsync(parameter);

			return worker;
		}

		public static string FormatFileSize(long bytes)
		{
			double mb = (bytes / 1024f) / 1024f;

			return mb.ToString("##.# MB");
		}

		public static string GetBaseImagesPath()
		{
			bool overrideFolder = false;
			if (!AppSettings.BaseImagesPathIsDefault)
			{
				if (!string.IsNullOrEmpty(AppSettings.BaseImagesPath))
				{
					if (Directory.Exists(AppSettings.BaseImagesPath)) overrideFolder = true;
				}
			}

			string filePath = "";
			if (overrideFolder)
				filePath = AppSettings.BaseImagesPath;
			else
			{
				string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				filePath = Path.Combine(appPath, "Images");
			}


			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseAniDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "AniDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetImagesTempFolder()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "_Temp_");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetAniDBImagePath(int animeID)
		{
			string subFolder = "";
			string sid = animeID.ToString();
			if (sid.Length == 1)
				subFolder = sid;
			else
				subFolder = sid.Substring(0, 2);

			string filePath = Path.Combine(GetBaseAniDBImagesPath(), subFolder);

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseTraktImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "Trakt");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetTraktImagePath()
		{
			string filePath = GetBaseTraktImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetTraktImagePath_Avatars()
		{
			string filePath = Path.Combine(GetTraktImagePath(), "Avatars");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseTvDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "TvDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetTvDBImagePath()
		{
			string filePath = GetBaseTvDBImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseMovieDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "MovieDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetMovieDBImagePath()
		{
			string filePath = GetBaseMovieDBImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static void OpenFolderAndSelectFile(string fullFilePath)
		{
			try
			{
				/*Process ExplorerWindowProcess = new Process();

				ExplorerWindowProcess.StartInfo.FileName = "explorer.exe";
				ExplorerWindowProcess.StartInfo.Arguments = string.Format("/select,\"{0}\"", fullFilePath);

				ExplorerWindowProcess.Start();*/

				ShowSelectedInExplorer.FilesOrFolders(fullFilePath);

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public static void PlayVideo(VideoDetailedVM vid)
		{
			try
			{
				Process.Start(new ProcessStartInfo(vid.FullPath));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public static void OpenFile(string fullePath)
		{
			try
			{
				Process.Start(new ProcessStartInfo(fullePath));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		/// <summary>
		/// Compute Levenshtein distance --- http://www.merriampark.com/ldcsharp.htm
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length; //length of s
			int m = t.Length; //length of t

			int[,] d = new int[n + 1, m + 1]; // matrix

			int cost; // cost

			// Step 1
			if (n == 0) return m;
			if (m == 0) return n;

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 0; j <= m; d[0, j] = j++) ;

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);

					// Step 6
					d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
							  d[i - 1, j - 1] + cost);
				}
			}

			// Step 7
			return d[n, m];
		}
		public static int InverseLevenshteinDistance(string s, string t)
		{
			int n = s.Length; //length of s
			int m = t.Length; //length of t

			int[,] d = new int[n + 1, m + 1]; // matrix

			int cost; // cost

			// Step 1
			if (n == 0) return m;
			if (m == 0) return n;

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 0; j <= m; d[0, j] = j++) ;

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					cost = (t.Substring((m - j), 1) == s.Substring((n - i), 1) ? 0 : 1);

					// Step 6
					d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
							  d[i - 1, j - 1] + cost);
				}
			}

			// Step 7
			return d[n, m];
		}

		public static string GetApplicationVersion(Assembly a)
		{
			AssemblyName an = a.GetName();
			return an.Version.ToString();
		}

		public static string GetOSInfo()
		{
			//Get Operating system information.
			OperatingSystem os = Environment.OSVersion;
			//Get version information about the os.
			Version vs = os.Version;

			//Variable to hold our return value
			string operatingSystem = "";

			if (os.Platform == PlatformID.Win32Windows)
			{
				//This is a pre-NT version of Windows
				switch (vs.Minor)
				{
					case 0:
						operatingSystem = "95";
						break;
					case 10:
						if (vs.Revision.ToString() == "2222A")
							operatingSystem = "98SE";
						else
							operatingSystem = "98";
						break;
					case 90:
						operatingSystem = "Me";
						break;
					default:
						break;
				}
			}
			else if (os.Platform == PlatformID.Win32NT)
			{
				switch (vs.Major)
				{
					case 3:
						operatingSystem = "NT 3.51";
						break;
					case 4:
						operatingSystem = "NT 4.0";
						break;
					case 5:
						if (vs.Minor == 0)
							operatingSystem = "2000";
						else
							operatingSystem = "XP";
						break;
					case 6:
						if (vs.Minor == 0)
							operatingSystem = "Vista";
						else
							operatingSystem = "7";
						break;
					default:
						break;
				}
			}
			//Make sure we actually got something in our OS check
			//We don't want to just return " Service Pack 2" or " 32-bit"
			//That information is useless without the OS version.
			if (operatingSystem != "")
			{
				//Got something.  Let's prepend "Windows" and get more info.
				operatingSystem = "Windows " + operatingSystem;
				//See if there's a service pack installed.
				if (os.ServicePack != "")
				{
					//Append it to the OS name.  i.e. "Windows XP Service Pack 3"
					operatingSystem += " " + os.ServicePack;
				}
				//Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
				operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
			}
			//Return the information we've gathered.
			return operatingSystem;
		}

		public static int getOSArchitecture()
		{
			if (Is64BitOperatingSystem)
				return 64;
			else
				return 32;
		}

		public static bool Is64BitProcess
		{
			get { return IntPtr.Size == 8; }
		}

		public static bool Is64BitOperatingSystem
		{
			get
			{
				// Clearly if this is a 64-bit process we must be on a 64-bit OS.
				if (Is64BitProcess)
					return true;
				// Ok, so we are a 32-bit process, but is the OS 64-bit?
				// If we are running under Wow64 than the OS is 64-bit.
				bool isWow64;
				return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
			}
		}

		static bool ModuleContainsFunction(string moduleName, string methodName)
		{
			IntPtr hModule = GetModuleHandle(moduleName);
			if (hModule != IntPtr.Zero)
				return GetProcAddress(hModule, methodName) != IntPtr.Zero;
			return false;
		}

		private static string[] escapes = { "SOURCE", "TAKEN", "FROM", "HTTP", "ANN", "ANIMENFO", "ANIDB", "ANIMESUKI" };

		public static string ReparseDescription(string description)
		{
			if (description == null || description.Length == 0) return "";

			string val = description;
			val = val.Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("<i>", "").
					Replace("</i>", "").Replace("<b>", "").Replace("</b>", "").Replace("[i]", "").Replace("[/i]", "").
					Replace("[b]", "").Replace("[/b]", "");
			val = val.Replace("<BR />", Environment.NewLine).Replace("<BR/>", Environment.NewLine).Replace("<I>", "").Replace("</I>", "").Replace("<B>", "").Replace("</B>", "").Replace("[I]", "").Replace("[/I]", "").
					Replace("[B]", "").Replace("[/B]", "");

			string vup = val.ToUpper();
			while ((vup.Contains("[URL")) || (vup.Contains("[/URL]")))
			{
				int a = vup.IndexOf("[URL");
				if (a >= 0)
				{
					int b = vup.IndexOf("]", a + 1);
					if (b >= 0)
					{
						val = val.Substring(0, a) + val.Substring(b + 1);
						vup = val.ToUpper();
					}
				}
				a = vup.IndexOf("[/URL]");
				if (a >= 0)
				{
					val = val.Substring(0, a) + val.Substring(a + 6);
					vup = val.ToUpper();
				}
			}
			while (vup.Contains("HTTP:"))
			{
				int a = vup.IndexOf("HTTP:");
				if (a >= 0)
				{
					int b = vup.IndexOf(" ", a + 1);
					if (b >= 0)
					{
						if (vup[b + 1] == '[')
						{
							int c = vup.IndexOf("]", b + 1);
							val = val.Substring(0, a) + " " + val.Substring(b + 2, c - b - 2) + val.Substring(c + 1);
						}
						else
						{
							val = val.Substring(0, a) + val.Substring(b);
						}
						vup = val.ToUpper();
					}
					else
					{
						break;
					}
				}
			}
			int d = -1;
			do
			{
				if (d + 1 >= vup.Length)
					break;
				d = vup.IndexOf("[", d + 1);
				if (d != -1)
				{
					int b = vup.IndexOf("]", d + 1);
					if (b != -1)
					{
						string cont = vup.Substring(d, b - d);
						bool dome = false;
						foreach (string s in escapes)
						{
							if (cont.Contains(s))
							{
								dome = true;
								break;
							}
						}
						if (dome)
						{
							val = val.Substring(0, d) + val.Substring(b + 1);
							vup = val.ToUpper();
						}
					}
				}
			} while (d != -1);
			d = -1;
			do
			{
				if (d + 1 >= vup.Length)
					break;

				d = vup.IndexOf("(", d + 1);
				if (d != -1)
				{
					int b = vup.IndexOf(")", d + 1);
					if (b != -1)
					{
						string cont = vup.Substring(d, b - d);
						bool dome = false;
						foreach (string s in escapes)
						{
							if (cont.Contains(s))
							{
								dome = true;
								break;
							}
						}
						if (dome)
						{
							val = val.Substring(0, d) + val.Substring(b + 1);
							vup = val.ToUpper();
						}
					}
				}
			} while (d != -1);
			d = vup.IndexOf("SOURCE:");
			if (d == -1)
				d = vup.IndexOf("SOURCE :");
			if (d > 0)
			{
				val = val.Substring(0, d);
			}
			return val.Trim();
		}
	}
}
