﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupStatus : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Server setup status messages
		// Contributors     - Creepysin, 

		[Command("setup status")]
		[Summary("Displays setup info")]
		[RequireGuildOwner]
		public async Task Setup()
		{
			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			EmbedBuilder embed = new EmbedBuilder();

			await Context.Channel.SendMessageAsync("Setup status was sent to your dms.");

			//Initial embed setup
			embed.WithTitle("Setup Status");
			embed.WithColor(new Color(255, 81, 168));
			embed.WithDescription(
				$"<:Menu:537572055760109568> Here is your setup status for **{Context.Guild.Name}**.\nSee [here]({Global.websiteServerSetup}) for more help.\n\n");
			embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.WithCurrentTimestamp();

			//Welcome message 
			string welcomeMessageTitle = "<:Cross:537572008574189578> Welcome Message Disabled";
			string welcomeMessageDescription = "Welcome message is disabled\n";
			if (server.WelcomeMessageEnabled)
			{
				welcomeMessageTitle = "<:Check:537572054266806292> Welcome Message Enabled";
				welcomeMessageDescription =
					$"Welcome message is enabled and is set to the channel **{((SocketTextChannel) Context.Client.GetChannel(server.WelcomeChannelId)).Name}**\n";
			}

			embed.AddField(welcomeMessageTitle, welcomeMessageDescription, true);

			//Goodbye message
			string goodbyeTitle = "<:Cross:537572008574189578> Goodbye Message Disabled";
			string goodbyeDes = "Goodbye message is disabled\n";
			if (server.GoodbyeMessageEnabled)
			{
				goodbyeTitle = "<:Check:537572054266806292> Goodbye Message Enabled";
				goodbyeDes =
					"The goodbye message is enabled and is set to the same channel as the welcome message channel.\n";
			}

			embed.AddField(goodbyeTitle, goodbyeDes, true);

			//Rule reaction
			string ruleReactionTitle = "<:Cross:537572008574189578> Rule Reaction Disabled";
			string ruleReactionDescription = "Rule reaction is disabled.\n";
			if (server.RuleEnabled)
			{
				ruleReactionTitle = "<:Check:537572054266806292> Rule Reaction Enabled";
				ruleReactionDescription =
					$"The rule reaction feature is enabled and is set to the message ID '[{server.RuleMessageId}](https://discordapp.com/channels/{Context.Guild.Id}/{server.RuleMessageChannelId}/{server.RuleMessageId})' with the emoji '{server.RuleReactionEmoji}'";
			}

			embed.AddField(ruleReactionTitle, ruleReactionDescription);

			//Server Points
			const string serverPointsTitle = "Server Points";
			string serverPointsDescription =
				$"**Points Given**: {server.PointGiveAmount}\n**Cooldown** (seconds): {server.PointsGiveCooldownTime}";

			embed.AddField(serverPointsTitle, serverPointsDescription);

			//Warnings for commands
			const string warningsTitle = "Warnings";
			string warningsDescription = "";
			if (server.GetCommandInfo("warn") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `warn` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("makewarnable") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `makewarnable` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("makenotwarnable") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `makenotwarnable` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("ban") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `ban` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("kick") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `kick` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("mute") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `mute` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("addvcchannel") == null)
				warningsDescription +=
					"<:Cross:537572008574189578> The command `addvcchannel` doesn't have a permission added to it!\n";
			else
				warningsDescription = "You have no warnings! :smile:";
			embed.AddField(warningsTitle, warningsDescription);

			embed.WithFooter($"For support see {Global.websiteHome}", Global.BotUser.GetAvatarUrl());

			await dm.SendMessageAsync("", false, embed.Build());
		}

		[Command("setup spam")]
		[Summary("Shows setup info regarding the server's anti-spam settings")]
		[RequireGuildOwner]
		public async Task SetupSpam()
		{
			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			EmbedBuilder embed = new EmbedBuilder();

			await Context.Channel.SendMessageAsync("Setup anti-spam status was sent to your dms.");

			//Initial embed setup
			embed.WithTitle("Anti-Spam Setup Status");
			embed.WithColor(new Color(255, 81, 168));
			embed.WithDescription(
				$"<:Menu:537572055760109568> Here is your anti-spam setup status for **{Context.Guild.Name}**.\nSee [here]({Global.websiteServerSetup + "anti-spam/"}) for more help.\n\n");
			embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.WithCurrentTimestamp();

			//Mention user spam
			string mentionUserTitle = "<:Cross:537572008574189578> Mention user spam is disabled!";
			string mentionUserDes =
				$"If a message with more then {server.AntiSpamSettings.MentionUsersPercentage}% of the server's users are mentioned, they will be warned.";
			if (server.AntiSpamSettings.MentionUserEnabled)
				mentionUserTitle = "<:Check:537572054266806292> Mention user spam is enabled!";

			//Role to role mentions
			embed.AddField(mentionUserTitle, mentionUserDes);
			embed.AddField("Role to Role mention",
				$"**{server.AntiSpamSettings.RoleToRoleMentionWarnings}** mentions of the same user will result in one warning");

			await dm.SendMessageAsync("", false, embed.Build());
		}
	}
}