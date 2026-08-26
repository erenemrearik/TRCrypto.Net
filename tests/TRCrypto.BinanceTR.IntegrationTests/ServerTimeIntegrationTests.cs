using TRCrypto.BinanceTR.Clients;
using Xunit;
using Xunit.Abstractions;

namespace TRCrypto.BinanceTR.IntegrationTests;

/// <summary>
/// Yerel saatin borsanin kabul penceresi icinde olup olmadigini olcer.
/// </summary>
/// <remarks>
/// Binance TR imzali istekleri yalnizca <c>recvWindow</c> icinde kabul eder; varsayilan
/// deger <b>5000 ms</b>'dir. Bu, BtcTurk'un toleransina gore dardir: birkac saniyelik bir
/// kayma BtcTurk'te sorun cikarmazken burada tum imzali istekleri reddettirir.
/// <para>
/// Bu test kimlik bilgisi gerektirmez ve sorun ortaya cikmadan once uyarir.
/// </para>
/// </remarks>
public class ServerTimeIntegrationTests
{
    private const int DefaultRecvWindowMs = 5000;

    private readonly ITestOutputHelper _output;

    public ServerTimeIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Yerel_saat_kabul_penceresi_icinde()
    {
        var client = new BinanceTRRestClient();

        var before = DateTime.UtcNow;
        var result = await client.SpotApi.ExchangeData.GetServerTimeAsync();
        var after = DateTime.UtcNow;

        Assert.True(result.Success, result.Error?.ToString());

        // Istegin baslangici ve bitisi ortalanarak ag gecikmesinin buyuk bolumu elenir.
        var localMidpoint = before + (after - before) / 2;
        var offset = (localMidpoint - result.Data).TotalMilliseconds;

        _output.WriteLine($"Yerel saat sapmasi: {offset:N0} ms (gidis-donus {(after - before).TotalMilliseconds:N0} ms)");
        _output.WriteLine($"Varsayilan recvWindow: {DefaultRecvWindowMs} ms");

        Assert.True(
            Math.Abs(offset) < DefaultRecvWindowMs,
            $"Yerel saat {offset:N0} ms sapmis; varsayilan {DefaultRecvWindowMs} ms penceresi disinda " +
            "kalirsa imzali istekler reddedilir. Cozum: docs/credentials/binance-tr.md bolum 6.");
    }
}
