﻿using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadServiceFiles
	{
#if WINDOWS

		/// <summary>
		/// Downloads files for Windows
		/// </summary>
		/// <param name="downloadUrls"></param>
		public static void DownloadAndPrepareWindowsFiles(AudioExternalLibFiles downloadUrls)
		{
			Logger.Log("Downloading files for Windows...");

			//Download all audio service files for Windows
			Logger.Log($"Downloading ffmpeg from {downloadUrls.FfmpegDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.FfmpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
			Logger.Log($"Downloading needed DLLs from {downloadUrls.LibsDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.LibsDownloadUrl, "Temp/dlls.zip").GetAwaiter().GetResult();

			//Extract required files
			Logger.Log("Extracting files...");
			ZipFile.ExtractToDirectory("Temp/dlls.zip", "./", true);
			ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

			//Copy the needed parts of ffmpeg to the right directory
			Logger.Log("Setting up ffmpeg");
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-latest-win64-static/bin/", "External/", true);
			File.Copy("Temp/ffmpeg/ffmpeg-latest-win64-static/LICENSE.txt", "External/ffmpeg-license.txt", true);

			//Delete unnecessary files
			Logger.Log("Cleaning up...");
			File.Delete("Temp/dlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("temp/ffmpeg", true);
		}

#elif LINUX
		public static void DownloadAndPrepareLinuxFiles(AudioExternalLibFiles downloadUrls)
		{
			Logger.Log("Downloading files for Linux...");

			//Download all audio service files for Linux
			Logger.Log($"Downloading ffmpeg from {downloadUrls.FfmpegDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.FfmpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
			Logger.Log($"Downloading needed DLLs from {downloadUrls.LibsDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.LibsDownloadUrl, "Temp/dlls.zip").GetAwaiter().GetResult();

			//Extract required files
			Logger.Log("Extracting files...");
			ZipFile.ExtractToDirectory("Temp/dlls.zip", "./", true);
			ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

			//Copy the needed parts of ffmpeg to the right directory
			Logger.Log("Setting up ffmpeg");
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-linux-64/", "External/", true);
			
			//Because linux, we need the right permissions
			ChmodFile("External/ffmpeg", "700");

			//Delete unnecessary files
			Logger.Log("Cleaning up...");
			File.Delete("Temp/dlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("Temp/ffmpeg", true);
		}

#elif OSX
#endif
		
#if LINUX || OXS
		private static void ChmodFile(string file, string flag)
		{
			Process process = new Process{StartInfo = new ProcessStartInfo
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "/bin/bash",
				Arguments = $"-c \"chmod {flag} {file}\""
			}};

			process.Start();
			process.WaitForExit();
		}
#endif
	}
}