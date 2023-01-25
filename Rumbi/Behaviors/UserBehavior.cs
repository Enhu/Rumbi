﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Rumbi.Behaviors
{
    public class UserBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;

        public UserBehavior(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
        }

        public void Initialize()
        {
            _client.UserJoined += HandleUserJoined;
            _client.UserLeft += HandleUserLeft;
            _client.PresenceUpdated += HandleUserPresenceUpdated;
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();

            ulong logChannelId = _configuration.GetValue<ulong>("Channels:LoggingChannelId");
            var rumbiVersion = _configuration["Version"];

            embed.WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithColor(0, 135, 245)
                .WithFooter($"ID: {user.Id} | Rumbi {rumbiVersion}", user.GetAvatarUrl())
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Account created: <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>")
                .WithTitle("Member Joined");

            var logChannel = _client.GetChannel(logChannelId) as SocketTextChannel;
            await logChannel.SendMessageAsync(embed: embed.Build());
        }
        private async Task HandleUserLeft(SocketGuild guild, SocketUser user)
        {
            var embed = new EmbedBuilder();

            ulong logChannelId = _configuration.GetValue<ulong>("Channels:LoggingChannelId");
            var rumbiVersion = _configuration["Version"];

            embed.WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithColor(0, 135, 245)
                .WithFooter($"ID: {user.Id} | Rumbi {rumbiVersion}", user.GetAvatarUrl())
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Account created: <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>")
                .WithTitle("Member Left");


            var logChannel = _client.GetChannel(logChannelId) as SocketTextChannel;
            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        private async Task HandleUserPresenceUpdated(SocketUser user, SocketPresence oldPresence, SocketPresence newPresence)
        {
            if(oldPresence.Activities.Any(x => x.Type == ActivityType.Streaming && x.Name == "A Hat in Time"))
            {
                var guild = _client.GetGuild(_configuration.GetValue<ulong>("GuildId"));
                var streamingRole = guild.GetRole(_configuration.GetValue<ulong>("Roles:Streaming"));
                var guildUser = guild.GetUser(user.Id);
                await guildUser.RemoveRoleAsync(streamingRole);
            }

            if(newPresence.Activities.Any(x => x.Type == ActivityType.Streaming && x.Name == "A Hat in Time"))
            {
                var guild = _client.GetGuild(_configuration.GetValue<ulong>("GuildId"));
                var streamingRole = guild.GetRole(_configuration.GetValue<ulong>("Roles:Streaming"));
                var guildUser = guild.GetUser(user.Id);
                await guildUser.AddRoleAsync(streamingRole);
            }
        }
    }
}