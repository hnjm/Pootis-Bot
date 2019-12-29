﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Audio
{
	public class VoiceChannels : ModuleBase<ICommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Adds an auto voice channel
		// Contributors     - Creepysin, 

		[Command("addvcchannel")]
		[RequireBotPermission(GuildPermission.ManageChannels)]
		[RequireBotPermission(GuildPermission.MoveMembers)]
		[RequireUserPermission(GuildPermission.ManageChannels)]
		[Cooldown(5)]
		public async Task AddVoiceChannel(string baseName)
		{
			RestVoiceChannel channel =
				await ((SocketGuild) Context.Guild).CreateVoiceChannelAsync($"➕ New {baseName} VC");

			await Context.Channel.SendMessageAsync($"Added {baseName} as an auto voice channel.");

			ServerVoiceChannel voiceChannel = new ServerVoiceChannel(channel.Id, baseName);

			ServerListsManager.GetServer((SocketGuild) Context.Guild).AutoVoiceChannels.Add(voiceChannel);
			ServerListsManager.SaveServerList();
		}
	}
}