using DiscordRPC;
using DiscordRPC.Logging;

namespace SentinelAIO.Library;

public class DiscordManager
{
    private static DiscordManager _instance;
    private readonly DiscordRpcClient _client;

    private DiscordManager()
    {
        _client = new DiscordRpcClient("1201262872848769084");
        _client.Logger = new ConsoleLogger { Level = LogLevel.Warning };
        _client.Initialize();
    }

    public static DiscordManager Instance => _instance ?? (_instance = new DiscordManager());

    // This is essentially your UpdateDiscordRpcPresence method.
    public void UpdatePresence(string details, string state)
    {
        _client.SetPresence(new RichPresence
        {
            State = state,
            Details = details,
            Timestamps = new Timestamps(DateTime.UtcNow),
            Assets = new Assets
            {
                LargeImageKey = "asset1",
                SmallImageKey = "logo"
            }
        });
    }
}