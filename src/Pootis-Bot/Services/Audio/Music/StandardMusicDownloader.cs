﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Audio.Music.Conversion;
using Pootis_Bot.Services.Audio.Music.Download;
using Pootis_Bot.Services.Google.YouTube;

namespace Pootis_Bot.Services.Audio.Music
{
	/// <summary>
	/// Standard music downloader, incorporates <see cref="IYouTubeSearcher"/>, <see cref="IMusicDownloader"/> and <see cref="IAudioConverter"/> to do all the work for you
	/// </summary>
	public class StandardMusicDownloader
	{
		private readonly string musicDirectory;
		private readonly MusicFileFormat fileFormat;
		private readonly CancellationTokenSource cancellationTokenSource;

		//Interfaces
		private readonly IAudioConverter audioConverter;		//Default: FfmpegAudioConverter
		private readonly IMusicDownloader musicDownloader;		//Default: YouTubeExplodeDownloader
		private readonly IYouTubeSearcher youTubeSearcher;		//Default: YouTubeService

		public StandardMusicDownloader(string musicDir, MusicFileFormat musicFileFormat, HttpClient httpClient, CancellationTokenSource cancelSource)
		{
			if (!Directory.Exists(musicDir))
				Directory.CreateDirectory(musicDir);

			musicDirectory = musicDir;
			fileFormat = musicFileFormat;
			cancellationTokenSource = cancelSource;

			audioConverter = new FfmpegAudioConverter(cancelSource.Token);
			musicDownloader = new YouTubeExplodeDownloader(musicDir, httpClient, cancelSource.Token);
			youTubeSearcher = new YouTubeService(httpClient);
		}

		public void CancelTask()
		{
			cancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Gets a song, directly with a YouTube URL
		/// </summary>
		/// <param name="videoUrl"></param>
		/// <param name="botMessage"></param>
		/// <returns></returns>
		public async Task<string> GetSongViaYouTubeUrl(string videoUrl, IUserMessage botMessage)
		{
			YouTubeVideo video = await youTubeSearcher.GetVideo(videoUrl);
			if (video != null)
			{
				return await GetOrDownloadSong(video.VideoTitle, botMessage);
			}

			await MessageUtils.ModifyMessage(botMessage, "Parsed in URL is incorrect or the YouTube video doesn't exist!");
			return null;
		}

		/// <summary>
		/// Gets (or downloads if necessary) a song
		/// </summary>
		/// <param name="songTitle"></param>
		/// <param name="botMessage"></param>
		/// <returns></returns>
		public async Task<string> GetOrDownloadSong(string songTitle, IUserMessage botMessage)
		{
			try
			{
				await MessageUtils.ModifyMessage(botMessage, $"Searching my audio banks for '{songTitle}'");

				//First, check if this song exists in our music DIR
				string songLocation = AudioService.SearchMusicDirectory(songTitle, fileFormat);
				if (songLocation != null)
				{
					return songLocation;
				}

				await MessageUtils.ModifyMessage(botMessage, $"Searching YouTube for '{songTitle}'");

				//It doesn't exist, search YouTube for it
				IList<YouTubeVideo> response = await youTubeSearcher.SearchForYouTube(songTitle);

				if (response == null)
				{
					await MessageUtils.ModifyMessage(botMessage, "Something went wrong while searching on YouTube!");
					return null;
				}

				//There were no results
				if (response.Count == 0)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"There were no results for '{songTitle}' on YouTube.");
					return null;
				}

				//Get the first video
				YouTubeVideo video = response[0];

				//This shouldn't ever happen
				if (video == null)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"Some issue happened while getting '{songTitle}' off from YouTube.");
					return null;
				}

				string videoTitle = video.VideoTitle.RemoveIllegalChars();

				//Do a second search with the title from YouTube
				songLocation = AudioService.SearchMusicDirectory(videoTitle, fileFormat);
				if (songLocation != null)
				{
					return songLocation;
				}

				//Make sure the song doesn't succeeds max time
				if (video.VideoDuration >= Config.bot.AudioSettings.MaxVideoTime)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"The video **{videoTitle}** by **{video.VideoAuthor}** succeeds max time of {Config.bot.AudioSettings.MaxVideoTime}");
					return null;
				}

				//Download the song
				await MessageUtils.ModifyMessage(botMessage,
					$"Downloading **{videoTitle}** by **{video.VideoAuthor}**");
				songLocation = await musicDownloader.DownloadYouTubeVideo(video.VideoId, musicDirectory);

				//Do a check here first, in case the operation was cancelled, so we don't say "Something went wrong...", when well... it was just cancelled
				if (cancellationTokenSource.IsCancellationRequested)
					return null;

				//The download must have failed
				if (songLocation == null)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"Something went wrong while downloading the song **{videoTitle}** from YouTube!");
					return null;
				}

				//If the file extension isn't the same then we need to convert it
				string audioFileExtension = Path.GetExtension(songLocation);
				if (audioFileExtension == fileFormat.GetFormatExtension()) return songLocation;

				//We need to convert it, since they are not the same file format
				songLocation = await audioConverter.ConvertFileToAudio(songLocation, musicDirectory, true, fileFormat);

				//Everything when well
				if (songLocation != null) return songLocation;

				//Do a check here first, in case the operation was cancelled, so we don't say "An issue occured...", when well... it was just cancelled
				if (cancellationTokenSource.IsCancellationRequested)
					return null;

				//Conversion failed
				await MessageUtils.ModifyMessage(botMessage,
					"An issue occured while getting the song ready for playing!");
				return null;
			}
			catch (OperationCanceledException)
			{
				//User cancelled
				return null;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
				await MessageUtils.ModifyMessage(botMessage, "An issue occured while trying to get the song!");

				return null;
			}
		}
	}
}