using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebApplication1;

public class CharacterWebSocketHandler
{
    private readonly CharacterService _service;
    private readonly List<WebSocket> _sockets = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public CharacterWebSocketHandler(CharacterService service)
    {
        _service = service;

        // Start the loop in the background
        _ = StartGeneratingLoop();
    }

    public void AddSocket(WebSocket socket)
    {
        lock (_lock)
        {
            _sockets.Add(socket);
        }
    }

    public void RemoveSocket(WebSocket socket)
    {
        lock (_lock)
        {
            _sockets.Remove(socket);
        }
    }

    private async Task StartGeneratingLoop()
    {
        while (true)
        {
            await Task.Delay(2000);

            List<WebSocket> socketsCopy;
            lock (_lock)
            {
                if (_sockets.Count == 0)
                    continue; 

                socketsCopy = new List<WebSocket>(_sockets);
            }

            var character = GenerateRandomCharacter();
            _service.Add(character);

            var json = JsonSerializer.Serialize(character);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            List<WebSocket> disconnectedSockets = new();

            foreach (var socket in socketsCopy)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        disconnectedSockets.Add(socket);
                    }
                }
                else
                {
                    disconnectedSockets.Add(socket);
                }
            }

            if (disconnectedSockets.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var ds in disconnectedSockets)
                    {
                        _sockets.Remove(ds);
                    }
                }
            }
        }
    }

    private Character GenerateRandomCharacter()
    {
        var sampleNames = new[]
        {
            "Ahri", "Garen", "Jinx", "Thresh", "Lux", "Ezreal", "Zed", "Yasuo"
        };
        var sampleRoles = new[]
        {
            "Mage", "Fighter", "Marksman", "Support", "Assassin", "Tank"
        };
        var sampleImages = new[]
        {
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Ahri_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Garen_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Jinx_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Thresh_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Lux_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Ezreal_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Zed_0.jpg",
            "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Yasuo_0.jpg"
        };

        var rand = new Random();
        int idx = rand.Next(sampleNames.Length);

        return new Character
        {
            Id = 0, 
            Name = sampleNames[idx],
            Role = sampleRoles[rand.Next(sampleRoles.Length)],
            Image = sampleImages[idx],
            Attack = (float)Math.Round(rand.NextDouble() * (10 - 4) + 4, 2),
            Defense = (float)Math.Round(rand.NextDouble() * (8 - 2) + 2, 2)
        };
    }
}
