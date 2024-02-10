using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace GuessWhoBot;

public class Bot : IBot
{
    private ServiceProvider? serviceProvider;

    private readonly ILogger<Bot> logger;
    private readonly IConfiguration configuration;
    private readonly DiscordSocketClient client;
    private readonly CommandService commands;

    public Bot(ILogger<Bot> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        client = new DiscordSocketClient(config);
        commands = new CommandService();
    }

    public async Task StartAsync(ServiceProvider services)
    {
        string discordToken = configuration["DiscordToken"] ?? throw new Exception("Missing Discord token");

        logger.LogInformation("Starting up...");

        serviceProvider = services;

        await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);

        await client.LoginAsync(TokenType.Bot, discordToken);
        await client.StartAsync();

        client.MessageReceived += HandleCommandAsync;
    }

    public async Task StopAsync()
    {
        logger.LogInformation("Shutting down");

        if (client != null)
        {
            await client.LogoutAsync();
            await client.StopAsync();
        }
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - {message.Author}: {message.Content}");

        int position = 0;
        if (!message.HasCharPrefix('!', ref position))
            return;

        await commands.ExecuteAsync(
            new SocketCommandContext(client, message),
            position,
            serviceProvider);
    }
}
