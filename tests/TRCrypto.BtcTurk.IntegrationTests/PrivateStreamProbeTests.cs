using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BtcTurk.IntegrationTests;

/// <summary>
/// Giris sonrasi gelen ozel akis mesajlarinin yapisini kesfeder.
/// </summary>
/// <remarks>
/// <para>
/// Resmi dokumantasyon ozel akis mesajlarinin kodlarini listeler ama govdelerini
/// gostermez ve bu mesajlar giris yapmadan gelmez. Model yazabilmek icin alan
/// adlarinin canli akistan okunmasi gerekir.
/// </para>
/// <para>
/// <b>Bu test hesap verisi yazdirmaz.</b> Yalnizca mesaj kodlarini ve alan
/// <i>adlarini</i> raporlar; tutarlar, emir kimlikleri ve bakiyeler ciktiya girmez.
/// </para>
/// </remarks>
public class PrivateStreamProbeTests
{
    private const string SocketUrl = "wss://ws-feed-pro.btcturk.com";
    private readonly ITestOutputHelper _output;

    public PrivateStreamProbeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    public async Task Giris_sonrasi_gelen_mesaj_yapilari_kaydedilir()
    {
        Skip.IfNot(TestCredentials.Available, TestCredentials.SkipReason);

        var apiKey = TestCredentials.ApiKey!;
        var secret = Convert.FromBase64String(TestCredentials.ApiSecret!);

        using var socket = new ClientWebSocket();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));

        await socket.ConnectAsync(new Uri(SocketUrl), cts.Token);

        // Imza kaynagi SocketLoginProbeTests ile belirlenmistir: publicKey + nonce.
        const long nonce = 3000;
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        using var hmac = new HMACSHA256(secret);
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey + nonce)));

        var login = $$"""[114,{"type":114,"publicKey":"{{apiKey}}","timestamp":{{stamp}},"nonce":{{nonce}},"signature":"{{signature}}"}]""";
        await socket.SendAsync(Encoding.UTF8.GetBytes(login), WebSocketMessageType.Text, true, cts.Token);

        // Kod -> alan adlari. Ayni kod birden fazla gelirse alan kumesi birlestirilir.
        var shapes = new Dictionary<int, SortedSet<string>>();
        var counts = new Dictionary<int, int>();
        var buffer = new byte[64 * 1024];

        try
        {
            while (!cts.IsCancellationRequested)
            {
                var received = await socket.ReceiveAsync(buffer, cts.Token);
                if (received.MessageType == WebSocketMessageType.Close)
                    break;

                var text = Encoding.UTF8.GetString(buffer, 0, received.Count);
                RecordShape(text, shapes, counts);
            }
        }
        catch (OperationCanceledException)
        {
            // Dinleme suresi doldu; beklenen son.
        }

        _output.WriteLine("Giris sonrasi gorulen mesajlar (yalnizca kod ve alan adlari):");
        _output.WriteLine("");

        foreach (var (code, fields) in shapes.OrderBy(x => x.Key))
        {
            _output.WriteLine($"  kod {code}  ({counts[code]} mesaj)");
            _output.WriteLine($"    alanlar: {string.Join(", ", fields)}");
        }

        Assert.NotEmpty(shapes);
    }

    /// <summary>
    /// Mesajin kodunu ve alan adlarini kaydeder; degerleri hicbir yere yazmaz.
    /// </summary>
    private static void RecordShape(
        string text,
        Dictionary<int, SortedSet<string>> shapes,
        Dictionary<int, int> counts)
    {
        using var document = JsonDocument.Parse(text);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return;

        var code = document.RootElement[0].GetInt32();
        var body = document.RootElement[1];

        if (!shapes.TryGetValue(code, out var fields))
            shapes[code] = fields = new SortedSet<string>(StringComparer.Ordinal);

        counts[code] = counts.GetValueOrDefault(code) + 1;
        CollectFieldNames(body, fields, prefix: string.Empty, depth: 0);
    }

    /// <summary>Alan adlarini ic ice yapilar dahil toplar; degerlere dokunmaz.</summary>
    private static void CollectFieldNames(
        JsonElement element, SortedSet<string> fields, string prefix, int depth)
    {
        if (depth > 3)
            return;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var name = prefix.Length == 0 ? property.Name : $"{prefix}.{property.Name}";
                    fields.Add(name);
                    CollectFieldNames(property.Value, fields, name, depth + 1);
                }
                break;

            case JsonValueKind.Array:
                // Dizinin yalnizca ilk ogesi yapiyi temsil eder.
                foreach (var item in element.EnumerateArray())
                {
                    CollectFieldNames(item, fields, prefix + "[]", depth + 1);
                    break;
                }
                break;
        }
    }
}
