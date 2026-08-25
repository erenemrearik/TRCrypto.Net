# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) temel alır ve
proje [Semantic Versioning](https://semver.org/lang/tr/) kullanır.

## [Yayınlanmadı]

Proje henüz NuGet'e yayınlanmadı. İlk sürüm `0.1.0-preview` olarak planlanıyor.

Güncel durum ve sonraki adımlar: [docs/DURUM.md](docs/DURUM.md)

### Eklendi

- **Proje temeli** — solution yapısı, merkezi paket yönetimi, beş hedef platform
  (`net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`), deterministic
  Release build, SourceLink, sembol paketi
- **`TRCrypto.BtcTurk`** — BtcTurk public piyasa verisi:
  - `GetExchangeInfoAsync` — pariteler, varlıklar, sunucu saati
  - `GetServerTimeAsync`
  - `GetTickersAsync` · `GetTickerAsync` · `GetTickersByQuoteAssetAsync`
  - `GetOrderBookAsync`
  - `GetTradesAsync`
- **Borsadan bağımsız yüzey** (`SharedApis`) — `ISpotSymbolRestClient`,
  `ISpotTickerRestClient`, `IOrderBookRestClient`, `IRecentTradeRestClient` ve
  `Discover()` ile yetenek keşfi
- **İstek limitleri** — resmi dokümantasyondan alınan değerlerle uygulanıyor
- **Bağımlılık enjeksiyonu** — `services.AddTRCryptoBtcTurk(...)`
- **Belgeler** — borsa başına API anahtarı rehberi (`docs/credentials/`), resmi kaynaklı
  endpoint envanteri (`docs/vendor/`), teknik spesifikasyon ve doğrulama ekleri (`docs/spec/`)
- **Secret koruması** — `.gitignore` blokları, `gitleaks` yapılandırması, pre-commit
  kancası; kimlik bilgilerinin loglara sızmadığını doğrulayan birim testleri

### Bilinen sınırlamalar

- Kimlik doğrulama (imzalama) henüz uygulanmadı; yalnızca public uçlar kullanılabilir
- Bakiye, emir işlemleri ve WebSocket desteği yok
- OHLC / kline ucu, resmi dokümantasyondan doğrulanamadığı için eklenmedi
- Binance TR, Paribu ve Bitexen adaptörleri planlandı, başlanmadı

### Notlar

Geliştirme sırasında BtcTurk API'sinde, resmi dokümantasyonda yer almayan bazı
davranışlar tespit edildi ve `docs/spec/` ekinde belgelendi. Bunların en önemlisi,
`code` alanının uçlar arasında farklı tiplerde (sayı / metin) dönmesidir.
