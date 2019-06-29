﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1;
using Newtonsoft.Json;
using Discord.Commands;
using Discord;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Fun commands
        // Contributors     - Creepysin, 

        readonly string ytstartLink = "https://www.youtube.com/watch?v=";
        readonly string ytChannelStart = "https://www.youtube.com/channel/";

        readonly Color youtubeColor = new Color(229, 57, 38);
        readonly Color googleColor = new Color(53, 169, 84);
        readonly Color giphyColor = new Color(190, 101, 249);

        //Based off youtubes documentation
        [Command("youtube")]
        [Summary("Searches Youtube")]
        [Alias("yt")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task CmdYoutubeSearch([Remainder] string search = "")
        {
            await Context.Channel.SendMessageAsync("", false, YoutubeSearch(search).Build());
        }

        [Command("giphy")]
        [Summary("Searches Giphy")]
        [Alias("gy")]
        public async Task CmdGiphySearch([Remainder] string search = "")
        {
            await Context.Channel.SendMessageAsync("", false, GiphySearch(search).Build());
        }

        [Command("google")]
        [Summary("Searches Google")]
        [Alias("g")]
        public async Task CmdGoogleSearch([Remainder]string search = "")
        {
            await Context.Channel.SendMessageAsync("", false, GoogleSearch(search).Build());
        }

        #region Functions

        EmbedBuilder YoutubeSearch(string search)
        {
            if (search != "")
            {
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiYoutubeKey.Trim()))
                {
                    try
                    {
                        var youtube = new YouTubeService(new BaseClientService.Initializer
                        {
                            ApiKey = Config.bot.apis.apiYoutubeKey,
                            ApplicationName = this.GetType().ToString()
                        });

                        var searchListRequest = youtube.Search.List("snippet");
                        searchListRequest.Q = search; // Replace with your search term.
                        searchListRequest.MaxResults = 10;

                        // Call the search.list method to retrieve results matching the specified query term.
                        var searchListResponse = searchListRequest.Execute();

                        List<string> videos = new List<string>();
                        List<string> channels = new List<string>();

                        // Add each result to the appropriate list, and then display the lists of
                        // matching videos and channels.
                        foreach (var searchResult in searchListResponse.Items)
                        {
                            switch (searchResult.Id.Kind)
                            {
                                case "youtube#video":
                                    videos.Add(string.Format($"[{searchResult.Snippet.Title}]({ytstartLink + searchResult.Id.VideoId})\n"));
                                    break;

                                case "youtube#channel":
                                    channels.Add(string.Format($"[{searchResult.Snippet.Title}]({ytChannelStart + searchResult.Id.ChannelId})\n"));
                                    break;
                            }
                        }

                        string _videos = string.Format("**Videos:**\n{0}\n", string.Join("\n", videos));
                        string _channels = string.Format("**Channels:**\n{0}\n", string.Join("\n", channels));

                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                        embed.Title = $"Youtube Search '{search}'";
                        embed.WithDescription($"{_videos} {_channels}");

                        embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                        embedfoot.WithText("Commanded issued by " + Context.User);

                        embed.WithFooter(embedfoot);
                        embed.WithColor(youtubeColor);

                        return embed;
                    }
                    catch (Exception ex)
                    {
                        Global.Log($"An error occured while user '{Context.User}' tryied searching '{search}' on Youtube. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Youtube Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: **\n {ex.Message}");
                        embed.WithColor(youtubeColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Youtube Search Error"
                    };
                    embed.WithDescription($"We are sorry, but youtube search is disabled by the bot owner.");
                    embed.WithColor(youtubeColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Youtube Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}youtube Dank Memes");
                embed.WithColor(youtubeColor);

                return embed;
            }
        }

        EmbedBuilder GoogleSearch(string search)
        {
            if (search != "")
            {
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiGoogleSearchKey.Trim()) && !string.IsNullOrWhiteSpace(Config.bot.apis.googleSearchEngineID.Trim()))
                {
                    try
                    {
                        var google = new CustomsearchService(new BaseClientService.Initializer
                        {
                            ApiKey = Config.bot.apis.apiGoogleSearchKey,
                            ApplicationName = this.GetType().ToString()
                        });

                        var searchListRequest = google.Cse.List(search);
                        searchListRequest.Cx = Config.bot.apis.googleSearchEngineID;

                        var searchListResponse = searchListRequest.Execute();

                        List<string> _search = new List<string>();

                        int currentresult = 0;
                        foreach (var searchResult in searchListResponse.Items)
                        {
                            if (currentresult != 5)
                            {
                                _search.Add($"[{searchResult.Title}]({searchResult.Link})\n{searchResult.Snippet}\n");
                                currentresult += 1;
                            }
                        }

                        string response = string.Format(string.Join("\n", _search));

                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                        embed.Title = $"Google Search For '{search}'";
                        embed.WithDescription($"{response}");

                        embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                        embedfoot.WithText("Commanded issued by " + Context.User);

                        embed.WithFooter(embedfoot);
                        embed.WithColor(googleColor);

                        return embed;

                    }
                    catch (Exception ex)
                    {
                        Global.Log($"[{Global.TimeNow()}] An error occured while user '{Context.User}' tryied searching '{search}' on google. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Google Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                        embed.WithColor(googleColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Google Search Error"
                    };
                    embed.WithDescription($"We are sorry, but google search is disabled by the bot owner.");
                    embed.WithColor(googleColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Google Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}google Dank Memes");
                embed.WithColor(new Color(53, 169, 84));

                return embed;
            }
        }

        EmbedBuilder GiphySearch(string search)
        {
            if (search.Trim() != "") //Check to see if search is nothing
            {
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiGiphyKey.Trim())) //Check to see if the bot giphy api is nothing
                {
                    try
                    {
                        string input = search.Replace(" ", "+");

                        string json = "";
                        using (WebClient client = new WebClient()) //Search the term using the giphy api. More about the api here: https://developers.giphy.com/docs/
                        {
                            json = client.DownloadString($"http://api.giphy.com/v1/gifs/search?q={input}&api_key={Config.bot.apis.apiGiphyKey}");
                        }

                        var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

                        int choose = Global.RandomNumber(0, 25);

                        //Read the json file
                        string url = dataObject.data[choose].images.fixed_height.url.ToString();
                        string title = dataObject.data[choose].title.ToString();
                        string author = dataObject.data[choose].username.ToString();
                        string shorturl = dataObject.data[choose].bitly_gif_url.ToString();

                        //Build the embed and return it.
                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                        embed.Title = Global.Title(title);
                        embed.WithImageUrl(url);

                        embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                        embedfoot.WithText("Commanded issued by " + Context.User);

                        embed.WithFooter(embedfoot);
                        embed.WithDescription($"**By**: {author}\n**URL**: {shorturl}");
                        embed.WithColor(giphyColor);

                        return embed;
                    }
                    catch (Exception ex)
                    {
                        Global.Log($"An error occured while user '{Context.User}' tryied searching '{search}' on giphy. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Giphy Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: **\n {ex.Message}");
                        embed.WithColor(giphyColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Giphy Search Error"
                    };
                    embed.WithDescription($"We are sorry, but giphy search is disabled by the bot owner.");
                    embed.WithColor(giphyColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Giphy Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}giphy Funny Cats");
                embed.WithColor(giphyColor);

                return embed;
            }
        }

        #endregion
    }
}