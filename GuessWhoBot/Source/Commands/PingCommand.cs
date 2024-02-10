
using Discord.Commands;

namespace GuessWhoBot.Commands;

public class PingCommand : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Pongs back")]
    public async Task ExecuteAsync()
    {
        await ReplyAsync("Pong!");
    }
}