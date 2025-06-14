using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
Console.WriteLine($"Listening on port {port}");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// Allow React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<CharacterService>();
builder.Services.AddSingleton<CharacterWebSocketHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// ? Place WebSockets early in the pipeline
app.UseWebSockets();

// (Optional) Only enable redirection if you're really using HTTPS
// app.UseHttpsRedirection();

app.UseAuthorization();

// Handle WebSocket connections
app.Map("/wss/characters", async (HttpContext context, CharacterWebSocketHandler wsHandler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    wsHandler.AddSocket(webSocket);

    var buffer = new byte[1024 * 4];
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!result.CloseStatus.HasValue)
    {
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    wsHandler.RemoveSocket(webSocket);

});

app.MapControllers();

app.Run();
