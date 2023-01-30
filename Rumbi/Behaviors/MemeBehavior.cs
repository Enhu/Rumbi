using Discord.WebSocket;
using Rumbi.Data;
using Rumbi.Data.Config;

namespace Rumbi.Behaviors
{
    public class MemeBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly RumbiContext _context;
        public MemeBehavior(DiscordSocketClient client, RumbiContext context)
        {
            _client = client;
            _context = context;
        }

        public void Initialize()
        {
            _client.MessageReceived += HandleMessageReceived;
        }

        private async Task HandleMessageReceived(SocketMessage arg)
        {
            if (arg.Channel.Id != RumbiConfig.Config.MemeChannel)
                return;

            if (!_context.Memes.Any(x => x.Trigger == arg.Content))
                return;

            await arg.Channel.SendMessageAsync(_context.Memes.FirstOrDefault(x => x.Trigger == arg.Content).Content);
        }
    }
}
