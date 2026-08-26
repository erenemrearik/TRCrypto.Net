using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BtcTurk.IntegrationTests;

/// <summary>
/// Socket giris imzasinin nasil uretildigini canli baglantiyla belirler.
/// </summary>
/// <remarks>
/// <para>
/// Resmi dokumantasyon socket girisinde imzanin <c>publicKey + nonce</c> uzerinden
/// hesaplandigini belirtir; REST tarafinda ise <c>apiKey + stamp</c> kullanilir. Yanlis
/// imza yalnizca "Invalid Signature" doner ve hangi varsayimin hatali oldugunu
/// gostermez, bu yuzden adaylar tek tek denenir.
/// </para>
/// <para>
/// Bu test hicbir emir olusturmaz; yalnizca giris yapar.
/// </para>
/// </remarks>
public class SocketLoginProbeTests
{
    private const string SocketUrl = "wss://ws-feed-pro.btcturk.com";
    private readonly ITestOutputHelper _output;

    public SocketLoginProbeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    public async Task Socket_giris_imzasinin_kaynagi_belirlenir()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var apiKey = TestCredentials.ApiKey!;
        var secret = Convert.FromBase64String(TestCredentials.ApiSecret!);
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        const long nonce = 3000;

        // Dokumante edilen ve REST'te kullanilan bicimler ile makul varyasyonlar.
        var candidates = new (string Name, string Message)[]
        {
            ("publicKey + nonce", apiKey + nonce),
            ("publicKey + stamp", apiKey + stamp),
            ("publicKey + nonce + stamp", apiKey + nonce + stamp),
            ("publicKey + stamp + nonce", apiKey + stamp + nonce)
        };

        string? working = null;

        foreach (var candidate in candidates)
        {
            var accepted = await TryLoginAsync(apiKey, secret, stamp, nonce, candidate.Message);
            _output.WriteLine($"{candidate.Name,-28} -> {(accepted.Ok ? "KABUL" : "RED")}  {accepted.Message}");

            if (accepted.Ok)
            {
                working = candidate.Name;
                break;
            }
        }

        _output.WriteLine("");
        _output.WriteLine(working != null
            ? $"SONUC: imza {working} uzerinden hesaplaniyor."
            : "SONUC: denenen hicbir bicim kabul edilmedi.");

        Assert.NotNull(working);
    }

    private static async Task<(bool Ok, string Message)> TryLoginAsync(
        string apiKey, byte[] secret, long stamp, long nonce, string message)
    {
        using var socket = new ClientWebSocket();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await socket.ConnectAsync(new Uri(SocketUrl), cts.Token);

        using var hmac = new HMACSHA256(secret);
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));

        var login = $$"""[114,{"type":114,"publicKey":"{{apiKey}}","timestamp":{{stamp}},"nonce":{{nonce}},"signature":"{{signature}}"}]""";
        await socket.SendAsync(Encoding.UTF8.GetBytes(login), WebSocketMessageType.Text, true, cts.Token);

        var buffer = new byte[8192];
        while (!cts.IsCancellationRequested)
        {
            var received = await socket.ReceiveAsync(buffer, cts.Token);
            var text = Encoding.UTF8.GetString(buffer, 0, received.Count);

            using var document = JsonDocument.Parse(text);
            var type = document.RootElement[0].GetInt32();
            if (type != 114)
                continue;   // Baglanti aninda gelen surum mesaji atlanir.

            var body = document.RootElement[1];
            var ok = body.TryGetProperty("ok", out var okProperty) && okProperty.GetBoolean();
            var msg = body.TryGetProperty("message", out var msgProperty) ? msgProperty.GetString() : null;
            return (ok, msg ?? string.Empty);
        }

        return (false, "yanit alinamadi");
    }
}
