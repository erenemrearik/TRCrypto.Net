<div align="center">

<img src="assets/logo.svg" alt="" width="88" height="88">

# TRCrypto.Net

**A .NET client ecosystem for Turkish crypto asset platforms**

[![CI](https://img.shields.io/github/actions/workflow/status/erenemrearik/TRCrypto.Net/ci.yml?branch=main&label=build&style=flat-square&logo=github&logoColor=white)](https://github.com/erenemrearik/TRCrypto.Net/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/badge/nuget-unpublished-9E9E9E?style=flat-square&logo=nuget&logoColor=white)](#status)
[![.NET](https://img.shields.io/badge/.net-8%20%7C%209%20%7C%2010%20%7C%20standard%202.0%20%7C%202.1-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#target-frameworks)
[![License](https://img.shields.io/badge/license-MIT-1F3E8C?style=flat-square)](LICENSE)
[![Documentation](https://img.shields.io/badge/documentation-site-10707B?style=flat-square)](https://erenemrearik.github.io/TRCrypto.Net/)

[Türkçe](README.md) · **English**

Built on [JKorf/CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) 12.5.0.

</div>

> [!IMPORTANT]
> **This project is unofficial.** It is an independent effort with no affiliation to
> BtcTurk, Binance TR, Paribu, CoinTR or JKorf. It is not responsible for changes the
> exchanges make to their APIs or for service interruptions.

> [!NOTE]
> The library's documentation, code comments and XML docs are written in Turkish, because
> the audience is Turkish developers. This file exists so that the project can be
> understood without reading Turkish. The
> [documentation site](https://erenemrearik.github.io/TRCrypto.Net/) is in Turkish.

---

## Why

Turkish exchanges do not resemble each other. Their authentication schemes, symbol
formats, response envelopes and WebSocket protocols all differ, so every developer ends up
rewriting `HttpClient` plumbing, HMAC signing, reconnection and model mapping.

TRCrypto offers **two surfaces** per exchange:

| Surface | What it gives you |
|---|---|
| **Native API** | Every feature and every exchange-specific field |
| **Shared API** | Exchange-independent code through `CryptoExchange.Net.SharedApis` |

---

## Status

<table>
<tr><th>Package</th><th>Scope</th><th>Status</th></tr>
<tr>
  <td><code>TRCrypto.BtcTurk</code></td>
  <td>Market data · klines · account · orders · WebSocket · SharedApis</td>
  <td><img src="https://img.shields.io/badge/complete-1B6340?style=flat-square" alt="complete"></td>
</tr>
<tr>
  <td><code>TRCrypto.BinanceTR</code></td>
  <td>Market data · account · orders · user stream · WebSocket · SharedApis</td>
  <td><img src="https://img.shields.io/badge/complete-1B6340?style=flat-square" alt="complete"></td>
</tr>
<tr>
  <td><code>TRCrypto.Paribu</code></td><td>Endpoints inventoried</td>
  <td><img src="https://img.shields.io/badge/queued-1F3E8C?style=flat-square" alt="queued"></td>
</tr>
<tr>
  <td><code>TRCrypto.CoinTR</code></td><td>Endpoints inventoried</td>
  <td><img src="https://img.shields.io/badge/queued-1F3E8C?style=flat-square" alt="queued"></td>
</tr>
<tr>
  <td><code>TRCrypto.Clients</code></td><td>Bundle package</td>
  <td><img src="https://img.shields.io/badge/planned-9E9E9E?style=flat-square" alt="planned"></td>
</tr>
</table>

**BtcTurk:** market data, klines, balances, order operations, trade history and real time
streams. The read endpoints are verified against a real account.

**Binance TR:** symbols, order book, trades, account and order endpoints, and real time
streams. Ticker arrives only over WebSocket, because the exchange does not serve ticker
data over REST without a key.

Both are usable through the native and the shared surface.

**Not there yet.** User specific WebSocket streams on BtcTurk, and the Paribu and CoinTR
adapters. Binance TR's private endpoints are written but have not yet been accepted by a
live account.

Deposit and withdrawal endpoints are deliberately out of scope (ADR-007).
Detail: [docs/DURUM.md](docs/DURUM.md)

---

## Quick start

Market data needs no credentials.

```csharp
using TRCrypto.BtcTurk.Clients;

var client = new BtcTurkRestClient();

var result = await client.SpotApi.ExchangeData.GetTickerAsync("BTCTRY");
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

Console.WriteLine($"BTC/TRY: {result.Data.LastPrice:N0}");
```

### Real time

```csharp
var socket = new BtcTurkSocketClient();

var sub = await socket.SpotApi.SubscribeToTickerUpdatesAsync("BTCTRY",
    update => Console.WriteLine(update.Data.LastPrice));

// ...
await sub.Data.CloseAsync();
```

### Exchange independent

The native symbol format appears nowhere in this code:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient tickers = client.SpotApi.SharedClient;
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY");

var ticker = await tickers.GetSpotTickerAsync(new GetTickerRequest(symbol));
```

> [!TIP]
> Clients are reusable and thread safe. Do not create a new one per request.

---

## Error handling

API errors are returned as a result object, not thrown as exceptions.

> [!WARNING]
> **Always** check `Success` before touching `Data`. BtcTurk returns business logic errors
> inside an HTTP 200 as `"success": false`, and the library turns that into a failed
> result.

Invalid input such as a missing price, a negative quantity or an out of range limit is
rejected before the request reaches the network.

---

## Documentation

| Topic | Where |
|---|---|
| **Documentation site**, everything in one searchable place | [erenemrearik.github.io/TRCrypto.Net](https://erenemrearik.github.io/TRCrypto.Net/) |
| **Current state and next step** | [docs/DURUM.md](docs/DURUM.md) |
| Obtaining and connecting API keys | [docs/credentials/](docs/credentials/) |
| Exchange endpoint inventories | [docs/vendor/](docs/vendor/) |
| Technical specification and verification appendices | [docs/spec/](docs/spec/) |
| Contribution guide | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Reporting a vulnerability | [SECURITY.md](SECURITY.md) |
| Changelog | [CHANGELOG.md](CHANGELOG.md) |

---

## Development

```bash
dotnet build -c Release
dotnet test  -c Release
dotnet run --project examples/TRCrypto.Examples.Console     # live check against two exchanges
dotnet run --project examples/TRCrypto.Examples.Dashboard   # live market board, localhost:5180
```

### Secret protection

The repository holds no real credentials. Enable the pre-commit hook before contributing:

```bash
git config core.hooksPath .githooks
```

The hook scans staged changes for secrets.
Detail: [docs/credentials/README.md](docs/credentials/README.md)

---

## Target frameworks

`net8.0` · `net9.0` · `net10.0` · `netstandard2.0` · `netstandard2.1`

## License

MIT. See [LICENSE](LICENSE) for detail.
