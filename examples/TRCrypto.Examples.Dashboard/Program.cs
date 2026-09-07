using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TRCrypto.Examples.Dashboard;

// TRCrypto.Net piyasa panosu.
//
// Tarayicidaki sayfa borsalara dogrudan baglanmaz. Butun veri once bu surecte
// TRCrypto.Net uzerinden akar, sonra WebSocket ile sayfaya itilir. Aksi halde ekran
// guzel gorunur ama kutuphaneyi hic calistirmaz, dolayisiyla hicbir sey dogrulamaz.
//
// Yalnizca herkese acik veri kullanilir; kimlik bilgisi gerekmez.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MarketBoard>();
builder.Logging.AddSimpleConsole(options => options.SingleLine = true);

var app = builder.Build();

app.UseWebSockets();
app.UseDefaultFiles();
app.UseStaticFiles();

var board = app.Services.GetRequiredService<MarketBoard>();
await board.StartAsync(app.Lifetime.ApplicationStopping);

var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

// Sayfa buraya baglanir ve duzenli araliklarla anlik goruntu alir. Her guncellemeyi tek
// tek itmek yerine sabit araliklarla toplu gondermek, saniyede yuzlerce mesaj gelen
// akislarda tarayiciyi bogmayi onler.
app.Map("/stream", async (HttpContext context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var stopping = context.RequestAborted;

    while (socket.State == WebSocketState.Open && !stopping.IsCancellationRequested)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(board.Snapshot(), serializerOptions);

        await socket.SendAsync(
            payload, WebSocketMessageType.Text, endOfMessage: true, stopping);

        await Task.Delay(TimeSpan.FromMilliseconds(250), stopping);
    }
});

app.Lifetime.ApplicationStopping.Register(() => board.DisposeAsync().AsTask().Wait());

app.Logger.LogInformation(
    "Pano hazir. {Venues} borsasindan {Assets} parite dinleniyor.",
    MarketBoard.Venues.Count,
    MarketBoard.Assets.Count);

await app.RunAsync();
