using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rumbi.Data;
using Rumbi.Data.Config;
using Rumbi.Data.Models;

namespace Rumbi.Handlers
{
    public class MemeHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;
        private readonly AppConfig _config;

        public MemeHandler(IServiceScopeFactory scopeFactory, DiscordSocketClient client, AppConfig config)
        {
            _client = client;
            _config = config;
            _scopeFactory = scopeFactory;
        }

        public void Initialize()
        {
            _client.MessageReceived += HandleMessageReceived;
        }

        private async Task HandleMessageReceived(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<RumbiContext>();

                if (arg.Channel.Id != _config.ChannelConfig.Bot)
                    return;

                if (arg.Author.IsBot)
                    return;

                if (!dbContext.Memes.Any(x => x.Trigger == arg.Content))
                    return;

                var meme = dbContext.Memes.Where(x => x.Trigger == arg.Content).SingleOrDefault();

                if (meme == null) return;

                await arg.Channel.SendMessageAsync(meme.Content);
            }
        }
    }
}