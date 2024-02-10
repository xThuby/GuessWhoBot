using Microsoft.Extensions.DependencyInjection;

namespace GuessWhoBot;

public interface IBot
{
    Task StartAsync(ServiceProvider services);

    Task StopAsync();
}