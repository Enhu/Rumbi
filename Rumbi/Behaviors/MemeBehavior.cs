using Discord.WebSocket;
using Rumbi.Data;
using Rumbi.Data.Config;

namespace Rumbi.Behaviors
{
    public class MemeBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly RumbiContext _context;
        private readonly RumbiConfig _config;

        public MemeBehavior(DiscordSocketClient client, RumbiContext context, RumbiConfig config)
        {
            _client = client;
            _context = context;
            _config = config;
        }

        public void Initialize()
        {
            _client.MessageReceived += HandleMessageReceived;
        }

        private async Task HandleMessageReceived(SocketMessage arg)
        {
            if (arg.Channel.Id != _config.ChannelConfig.Bot)
                return;

            if (!_context.Memes.Any(x => x.Trigger == arg.Content))
                return;

            await arg.Channel.SendMessageAsync(
                _context.Memes.FirstOrDefault(x => x.Trigger == arg.Content).Content
            );
        }
    }
}
