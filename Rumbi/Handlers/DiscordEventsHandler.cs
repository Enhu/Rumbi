using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Reflection.Metadata;

namespace Rumbi.Handlers
{
    public class DiscordEventsHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public DiscordEventsHandler(DiscordSocketClient client, IServiceProvider services, IConfiguration config)
        {
            _client = client;
            _services = services;
            _configuration = config;
        }

        public async Task InitializeAsync()
        {
            _client.UserJoined += HandleUserJoined;
            _client.UserLeft += HandleUserLeft;
        }

        private async Task HandleUserLeft(SocketGuild guild, SocketUser user)
        {
            var modsChannel = _client.GetChannel(_configuration.GetValue<ulong>("ModChannelId")) as SocketTextChannel;
            await modsChannel.SendMessageAsync("User: username: " + user.Username + " left.");
        }
        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var modsChannel = _client.GetChannel(_configuration.GetValue<ulong>("ModChannelId")) as SocketTextChannel;
            await modsChannel.SendMessageAsync("User: username: " + arg.Username + " joined.");
        }
    }
}
