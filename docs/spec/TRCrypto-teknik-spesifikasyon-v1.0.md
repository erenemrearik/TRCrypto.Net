TRCrypto.Net
Türkiye Kripto Varlık Platformları için .NET Client Ekosistemi
Teknik Analiz, Sistem Tasarımı ve Developer-Ready Gelistirme Spesifikasyonu
| Doküman AmacıBu doküman, JKorf/CryptoExchange.Net ekosisteminin güncel 2026 mimarisi referans alınarak Türkiye odaklı borsa client kütüphanelerinin sıfırdan geliştirilmesi için iş analizi, teknik tasarım, backlog, kabul kriterleri, test ve release sürecini tek yerde tanımlar. Developer, ayrı bir analiz fazı beklemeden Bölüm 18’deki başlangıç adımlarını uygulayarak geliştirmeye başlayabilir. |


| Alan | Değer |
| Doküman tipi | Teknik Analiz + Solution Design + Development Specification |
| Durum | Approved for Development / Gelistirmeye Hazir |
| Sürüm | 1.0 |
| Tarih | 23 Ağustos 2026 |
| Hedef platform | .NET 10 öncelikli; net8.0, net9.0, net10.0, netstandard2.0/2.1 uyumluluk hedefi |
| Temel dependency | CryptoExchange.Net 12.5.0 |
| MVP borsalar | Binance TR, BtcTurk, Paribu |
| Faz 2 adayı | Bitexen |
| Çalışma adı | TRCrypto.Net ekosistemi |
| Hedef kitle | Backend/.NET developer, tech lead, QA, DevOps, ürün/iş analizi |


Araştırma referansı: JKorf / CryptoExchange.Net / Binance.Net / CryptoClients.Net / resmi borsa API dokümanları
 
# Doküman Kontrolü ve Okuma Rehberi
Bu çalışma bir fikir dokümanı değildir. Uygulama sınırlarını, mimari kararları, paket yapısını, minimum fonksiyon setini ve kabul koşullarını bağlayıcı bir başlangıç spesifikasyonu olarak ele alır. API sağlayıcılarının dokümantasyonu değişebildiği için endpoint seviyesindeki son doğrulama her exchange implementasyonu açılırken ilgili resmi kaynak üzerinden yapılacaktır.
| Rol | Bu dokümanda kullanacağı bölümler |
| Developer | 4-16 mimari/kontratlar; 18 başlangıç; 19 backlog; 22 Definition of Done |
| Tech Lead | 3 mimari karar; 5 solution yapısı; 10-16 cross-cutting tasarım; 20 riskler |
| QA | 9 capability matrix; 15 test stratejisi; 19 kabul kriterleri |
| DevOps | 14 gözlemlenebilirlik; 16 CI/CD, NuGet, release |
| Ürün/BA | 2 kapsam; 8 user scenarios; 19 epic/story yapısı |


## İçindekiler
1. Yönetici Özeti
2. Problem, Amaç, Kapsam ve Başarı Kriterleri
3. JKorf ve CryptoExchange.Net Ekosistemi Araştırması
4. Önerilen Ürün ve Paket Stratejisi
5. Hedef Solution / Repository Mimarisi
6. Temel Teknik Standartlar
7. Domain ve Shared API Tasarımı
8. Temel Kullanım Senaryoları
9. Exchange Capability ve Endpoint Kapsamı
10. REST Client Tasarımı
11. WebSocket Tasarımı
12. Authentication, Credential ve Güvenlik
13. Rate Limit, Retry, Time Sync ve Dayanıklılık
14. Logging, Telemetry ve Operasyonel Görünürlük
15. Test Stratejisi
16. CI/CD, NuGet ve Release Yönetimi
17. Dokümantasyon ve Developer Experience
18. Developer Başlangıç Paketi
19. Backlog: Epic / Story / Acceptance Criteria
20. Riskler, Varsayımlar ve Açık Konular
21. Architecture Decision Records
22. Definition of Done ve Go-Live Kriterleri
23. Kaynaklar ve Araştırma Notları
 
# 1. Yönetici Özeti
TRCrypto.Net, Türkiye’de hizmet veren kripto varlık platformlarının REST ve WebSocket API’lerini .NET uygulamalarında ortak geliştirme deneyimiyle kullanılabilir hale getiren açık kaynak bir client ekosistemi olarak tasarlanacaktır. Proje, CryptoExchange.Net’in alternatifi veya fork’u olmayacaktır; CryptoExchange.Net 12.5.0 doğrudan temel altyapı dependency’si olarak kullanılacaktır.
| Ana Mimari KararHer borsa bağımsız bir package/adapter olarak geliştirilecek; borsaya özgü tüm endpointler native client üzerinden erişilebilir olacak; ortak işlemler CryptoExchange.Net.SharedApis arayüzlerine map edilecek; en üstte TRCrypto.Clients tüm desteklenen Türkiye borsalarını tek dependency altında sunacaktır. |


| Katman | Önerilen paket | Sorumluluk |
| Base | CryptoExchange.Net 12.5.0 | REST/socket pipeline, result yapıları, shared interfaces, rate limiting altyapısı, reconnect, tracking, common abstractions |
| Exchange adapter | TRCrypto.BinanceTR | Binance TR native API + SharedApis mapping |
| Exchange adapter | TRCrypto.BtcTurk | BtcTurk native API + SharedApis mapping |
| Exchange adapter | TRCrypto.Paribu | Paribu native API + SharedApis mapping |
| Exchange adapter - Faz 2 | TRCrypto.Bitexen | Bitexen native API + SharedApis mapping |
| Bundle | TRCrypto.Clients | Tek paketle tüm clientlara erişim, multi-exchange kullanım kolaylığı |


MVP’nin başarı tanımı: Binance TR, BtcTurk ve Paribu için market data + spot trading + balance/account + temel websocket akışları çalışır; SharedApis ile en az ticker, symbols, order book, trades, balances ve spot order işlemleri exchange-independent çağrılabilir; NuGet paketleri CI üzerinden üretilebilir; credential/logging güvenlik standartları sağlanır; public testler gerçek API ile, private testler sandbox/test hesapları veya kontrollü integration pipeline ile doğrulanır.
# 2. Problem, Amaç, Kapsam ve Başarı Kriterleri
## 2.1 Problem Tanımı
Türkiye odaklı kripto platformlarında API yüzeyleri, authentication şemaları, symbol formatları, response modelleri, rate-limit kuralları ve WebSocket protokolleri birbirinden farklıdır. Global bir borsanın Türkiye operasyonu dahi ayrı endpoint ve veri modeli kullanabilir. Örneğin Binance TR; `/open/v1/...` yolları, `BTC_USDT` benzeri sembol formatı, TRY çiftlerine özel komisyon alanları ve ayrı listen-token WebSocket akışı sunar. Bu nedenle global Binance.Net client’ına yalnızca yeni bir base URL vermek yeterli ve güvenli bir çözüm değildir [R7].
## 2.2 Ürün Amacı
[ListBullet] .NET geliştiricisinin her Türkiye borsası için HttpClient, HMAC, reconnect, retry ve model mapping kodunu tekrar yazmasını önlemek.
[ListBullet] Borsa-spesifik özellikleri kaybetmeden ortak bir `SharedApis` deneyimi sağlamak.
[ListBullet] Algoritmik trading, market data aggregation, arbitraj, portföy takip ve operasyon araçları için reusable bir SDK oluşturmak.
[ListBullet] API değişikliklerini uygulama kodundan izole etmek ve yalnızca ilgili adapter paketinde yönetmek.
[ListBullet] NuGet üzerinden tüketilebilen, testli, loglanabilir ve sürümlenebilir bir açık kaynak altyapı üretmek.
## 2.3 MVP Kapsamı
| Fonksiyon grubu | MVP | Not |
| Public REST | Evet | Symbols/exchange info, ticker, trades, order book, klines uygun olduğu ölçüde |
| Private REST | Evet | Balance/account, open orders, order query, place/cancel spot order, user trades |
| Public WebSocket | Evet | Ticker, trades, order book; kline mevcut ise |
| Private WebSocket | Exchange destekliyorsa | Order/balance/user data update |
| SharedApis | Evet | Ortak desteklenen fonksiyonlar |
| Withdraw/Deposit | Kısıtlı / Faz 1.5 | Güvenlik ve Travel Rule farklılıkları nedeniyle native önce, shared sonra |
| Fiat TRY transfer | Hayır - ilk MVP dışında | Borsa bazlı ve regülasyon/operasyon bağımlılığı yüksek |
| Futures/derivatives | Hayır | Türkiye spot odaklı MVP |
| UI / Web portal | Hayır | Bu proje SDK/library projesidir |
| Database | Hayır | Stateless client; kalıcı veri consumer sorumluluğu |


## 2.4 Kapsam Dışı
[ListBullet] Trading stratejisi veya bot mantığı
[ListBullet] Kullanıcı KYC/onboarding otomasyonu
[ListBullet] Custody/cüzdan yönetimi
[ListBullet] Borsalar arası transfer orchestration
[ListBullet] Vergi/muhasebe hesaplama
[ListBullet] Piyasa yapıcılık algoritması
[ListBullet] Garanti edilen arbitraj/fiyat avantajı
[ListBullet] Borsa API SLA’sını üstlenmek
## 2.5 Ölçülebilir Başarı Kriterleri
| KPI | MVP hedefi |
| Exchange coverage | 3 production adapter: Binance TR, BtcTurk, Paribu |
| Shared coverage | Her exchange için desteklenebilen shared interface’lerin >= %80’i implemented |
| Public endpoint tests | MVP public endpointlerin %100 contract test kapsamı |
| Mapping tests | Shared mappinglerin %100 pozitif model testleri |
| Auth tests | HMAC/signature deterministic test vector ile doğrulanmış |
| WebSocket | Reconnect + unsubscribe + resubscribe senaryoları testli |
| NuGet | Her paket reproducible Release build ve symbols package üretiyor |
| Secrets | Loglarda API key/secret/signature raw değer yok |
| Docs | Her paket için README + quick-start + capability matrix mevcut |


# 3. JKorf ve CryptoExchange.Net Ekosistemi Araştırması
## 3.1 Maintainer Profili ve Ekosistem
JKorf, GitHub profilinde Jan Korf adıyla görünen Hollanda merkezli bir açık kaynak geliştiricisidir. Ağustos 2026 itibarıyla profilinde onlarca repository/package bulunmakta; en görünür projeleri Binance.Net ve CryptoExchange.Net’tir. Binance.Net yaklaşık 1.2K GitHub star seviyesinde ve 1.600+ commit geçmişine sahiptir; CryptoExchange.Net ise 1.100+ commit ile bağımsız exchange implementasyonlarının base kütüphanesidir [R1][R2][R4].
| Araştırmadan ÇıkarımBaşarının kaynağı tek bir Binance wrapper’ı değil; aynı tasarım dilini onlarca exchange’e uygulayan bir ekosistem modelidir. Türkiye versiyonu da “tek dev paket” yerine aynı prensiple modüler kurulmalıdır. |


## 3.2 JKorf Mimari Deseni
| Desen | JKorf tarafındaki örnek | TRCrypto karşılığı |
| Base framework | CryptoExchange.Net | Doğrudan dependency; yeniden yazılmayacak |
| Tek borsa package | Binance.Net, Bybit.Net, Kraken.Net | TRCrypto.BinanceTR, TRCrypto.BtcTurk, TRCrypto.Paribu |
| Bundle package | CryptoClients.Net | TRCrypto.Clients |
| Native API | SpotApi.ExchangeData / Trading / Account | Exchange-specific API yüzeyi |
| Unified API | `.SharedClient` + SharedApis | Aynı model birebir uygulanacak |
| REST + Socket ayrımı | BinanceRestClient / BinanceSocketClient | XxxRestClient / XxxSocketClient |
| Credentials | Exchange credentials + AuthenticationProvider | XxxCredentials + XxxAuthenticationProvider |
| Options | XxxRestOptions / XxxSocketOptions | Aynı convention |
| Testing | Unit test projesi + request validation | Her exchange için UnitTests + IntegrationTests |
| DX | README, examples, docs, AGENTS/llms files | MVP sonrası aynı developer experience |


## 3.3 Binance.Net Güncel Teknik Yapısı
Binance.Net 13.5.0 (21 Ağustos 2026), CryptoExchange.Net 12.5.0’a bağlıdır; net8.0, net9.0, net10.0, netstandard2.0 ve netstandard2.1 target eder, nullable aktif, Native AOT uyumluluğu tanımlar, SourceLink ve symbol package üretir [R4][R5]. Repository içinde `Clients`, `Converters`, `Enums`, `Interfaces`, `Objects`, `SymbolOrderBooks` klasörleri; root seviyesinde `AuthenticationProvider`, `Credentials`, `Environment`, `Errors`, `Exchange`, `Helpers`, tracker ve factory sınıfları bulunur [R4].
JKorf/Binance.Net referans klasör deseni
| Binance.Net/ Clients/ SpotApi/ UsdFuturesApi/ CoinFuturesApi/ GeneralApi/ BinanceRestClient.cs BinanceSocketClient.cs Converters/ Enums/ Interfaces/ Objects/ Internal/ Models/ Options/ Sockets/ SymbolOrderBooks/ BinanceAuthenticationProvider.cs BinanceCredentials.cs BinanceEnvironment.cs BinanceErrors.cs BinanceExchange.cs |


## 3.4 SharedApis Neden Kritik?
CryptoExchange.Net’in modern tasarımında farklı exchange paketleri ortak `CryptoExchange.Net.SharedApis` interface’lerini implement eder. Spot ticker, symbol, order book, recent trades, kline, balances, orders, fees, deposits/withdrawals ve socket subscriptions için ortak kontratlar vardır. `SharedSymbol`, exchange’in sembol formatını consumer kodundan saklar; `.Discover()` ise çalışma anında hangi shared capability’lerin desteklendiğini bildirir [R3][R4].
Hedef exchange-independent kullanım deseni
| ISpotTickerRestClient client = restClient.SpotApi.SharedClient; var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY"); var result = await client.GetSpotTickerAsync(new GetTickerRequest(symbol)); if (!result.Success) return; Console.WriteLine(result.Data.LastPrice); |


## 3.5 Güncel 12.5.0 Tasarımına Uyum Zorunluluğu
2026 sürümlerinde eski CryptoExchange.Net örnekleriyle uyumsuz önemli değişiklikler vardır: `HttpResult`/`WebSocketResult` result tipleri, `Discover()`, TokenManager tabanlı listen-token yönetimi, dedicated endpoint option tipleri, shared order-management socket interface’leri ve güncel request validation yaklaşımı. Bu proje eski BtcTurk.Net 5.x dönemindeki kodu taşımak yerine güncel 12.5.0 kalıbını yeniden uygulamalıdır [R2][R3].
## 3.6 Eski Türkiye Ekosistemi ve Boşluk
BtcTurk için geçmişte CryptoExchange.Net tabanlı `BtcTurk.Net` geliştirilmiştir; repository Temmuz 2024’te arşivlenmiştir ve NuGet paketi deprecated/unlisted durumdadır. Paket CryptoExchange.Net 5.x nesline bağlıdır. Bu, fikrin teknik olarak daha önce doğrulandığını fakat modern SharedApis ve 2026 CryptoExchange.Net mimarisiyle güncel bir Türkiye katmanı bulunmadığını gösterir [R15][R16].
# 4. Önerilen Ürün ve Paket Stratejisi
## 4.1 Çalışma Adı ve Namespace
| Önerilen isim`TRCrypto` bir organization/umbrella adı; package adları `TRCrypto.<Exchange>`; bundle `TRCrypto.Clients`. Böylece mevcut `BtcTurk.Net` ve `Paribu.Api` NuGet isim sahiplikleriyle çakışma önlenir ve proje JKorf ile resmi bağlantı varmış izlenimi yaratmaz. |


| Bileşen | Package ID | Root namespace |
| Binance TR | TRCrypto.BinanceTR | TRCrypto.BinanceTR |
| BtcTurk | TRCrypto.BtcTurk | TRCrypto.BtcTurk |
| Paribu | TRCrypto.Paribu | TRCrypto.Paribu |
| Bitexen | TRCrypto.Bitexen | TRCrypto.Bitexen |
| Bundle | TRCrypto.Clients | TRCrypto.Clients |


## 4.2 Repository Modeli
MVP için monorepo önerilir. Exchange paketlerinin aynı CryptoExchange.Net sürümünü, aynı analyzers ve aynı CI template’ini kullanması kolaylaşır. Proje olgunlaşıp bağımsız release cadence ihtiyacı oluşursa multi-repo’ya ayrılabilir.
| TRCrypto/ src/ TRCrypto.BinanceTR/ TRCrypto.BtcTurk/ TRCrypto.Paribu/ TRCrypto.Clients/ tests/ TRCrypto.BinanceTR.UnitTests/ TRCrypto.BinanceTR.IntegrationTests/ TRCrypto.BtcTurk.UnitTests/ TRCrypto.BtcTurk.IntegrationTests/ TRCrypto.Paribu.UnitTests/ TRCrypto.Paribu.IntegrationTests/ TRCrypto.Clients.UnitTests/ examples/ TRCrypto.Examples.Console/ TRCrypto.Examples.AspNetCore/ docs/ .github/workflows/ Directory.Build.props Directory.Packages.props TRCrypto.sln |


## 4.3 Dependency Kuralları
| Proje | İzin verilen dependency | Yasak |
| Exchange adapter | CryptoExchange.Net + BCL + analyzer | Başka exchange adapter’a reference |
| TRCrypto.Clients | Exchange adapter packages | Exchange private/internal sınıflarına coupling |
| UnitTests | İlgili project + test libs | Production secrets |
| Examples | Published public API | Internal types / reflection hack |


## 4.4 Versioning
Her exchange package SemVer kullanır. Breaking public API -> major; yeni endpoint/capability -> minor; bug/model fix -> patch. CryptoExchange.Net major/minor upgrade’ları önce compatibility branch’inde test edilir. Bundle version’ı kendi SemVer’ine sahiptir ve içerdiği adapter minimum versiyonlarını açıkça listeler.
# 5. Hedef Solution / Repository Mimarisi
## 5.1 Katmanlı Görünüm
Hedef dependency ve abstraction yapısı
| Consumer Application | +--> TRCrypto.Clients -------------------------------+ | | +--> TRCrypto.BinanceTR --> CryptoExchange.Net | +--> TRCrypto.BtcTurk --> CryptoExchange.Net | +--> TRCrypto.Paribu --> CryptoExchange.Net | | SharedApis <----------------------------------------+ | ISpotTickerRestClient | ISpotSymbolRestClient | IOrderBookRestClient | IRecentTradeRestClient | IBalanceRestClient | ISpotOrderRestClient + socket interfaces... |


## 5.2 Exchange Adapter İç Yapısı
| TRCrypto.BinanceTR/ Clients/ SpotApi/ BinanceTRRestApiClient.cs BinanceTRRestApiExchangeData.cs BinanceTRRestApiTrading.cs BinanceTRRestApiAccount.cs BinanceTRSocketApiClient.cs BinanceTRSocketApiExchangeData.cs BinanceTRSocketApiAccount.cs BinanceTRRestClient.cs BinanceTRSocketClient.cs Interfaces/Clients/... Objects/ Internal/ Models/Spot/ Models/Account/ Models/Trading/ Models/Socket/ Options/ Sockets/ Converters/ Enums/ BinanceTRAuthenticationProvider.cs BinanceTRCredentials.cs BinanceTREnvironment.cs BinanceTRExchange.cs BinanceTRErrors.cs BinanceTRHelpers.cs |


## 5.3 Public API Tasarım Kuralları
[ListBullet] Consumer’ın kullanacağı her operasyon interface üzerinden de erişilebilir olmalı; concrete client yalnız convenience/DI için kullanılmalı.
[ListBullet] Async network metodlarının tamamında `CancellationToken cancellationToken = default` bulunmalı.
[ListBullet] Result kontrolü zorunlu olacak şekilde CryptoExchange.Net `HttpResult<T>` / `WebSocketResult<T>` yaklaşımı izlenmeli.
[ListBullet] API response modelleri mümkün olduğunca immutable record/class ve nullable contract ile modellenmeli.
[ListBullet] Native response alanları anlamlı C# isimlerine çevrilmeli; JSON field metadata korunmalı.
[ListBullet] Enum mapping unknown/future value karşısında mümkün olduğunca library’yi kırmamalı.
[ListBullet] Client instance’ları reusable olacak; request başına client yaratılmasını teşvik eden API tasarlanmayacak.
[ListBullet] Her endpoint request definition içinde authentication, weight/rate-limit, HTTP method, path ve parameter-position bilgisi açık tanımlanmalı.
# 6. Temel Teknik Standartlar
| Alan | Standart |
| Language | C# latest |
| Target frameworks | net8.0;net9.0;net10.0;netstandard2.0;netstandard2.1 - feasibility doğrulanacak |
| Nullable | enable |
| Serialization | System.Text.Json / CryptoExchange.Net converter altyapısı |
| Async | ConfigureAwait(false) library code standardı; analyzer ile kontrol |
| DI | Microsoft.Extensions.DependencyInjection extension methods |
| Logging | Microsoft.Extensions.Logging abstractions; secret masking |
| Result | CryptoExchange.Net HttpResult/WebSocketResult/ExchangeCallResult |
| HTTP | CryptoExchange.Net RestApiClient |
| Socket | CryptoExchange.Net SocketApiClient |
| Version | SemVer |
| License | MIT önerisi; üçüncü taraf notices korunacak |
| Build | Deterministic release + SourceLink + snupkg |
| AOT | net8+ için IsAotCompatible=true hedefi |


## 6.1 Directory.Build.props Önerisi
| <Project> <PropertyGroup> <LangVersion>latest</LangVersion> <Nullable>enable</Nullable> <TreatWarningsAsErrors>true</TreatWarningsAsErrors> <Deterministic>true</Deterministic> <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild> </PropertyGroup> </Project> |


## 6.2 Central Package Management
| <Project> <PropertyGroup> <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally> </PropertyGroup> <ItemGroup> <PackageVersion Include="CryptoExchange.Net" Version="12.5.0" /> <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="10.0.101" /> <PackageVersion Include="xunit" Version="2.*" /> </ItemGroup> </Project> |


# 7. Domain ve Shared API Tasarımı
## 7.1 Native + Shared Çift Yüzey
Her exchange iki kullanım biçimini aynı anda sunar: (1) native API, borsanın tüm özellik ve özgün modellerine erişim; (2) Shared API, exchange-independent kod. Shared mapping hiçbir zaman native API’nin yerini almaz; sadece ortak paydayı standartlaştırır.
| // Native var native = await client.SpotApi.ExchangeData.GetTickerAsync("BTC_TRY"); // Shared ISpotTickerRestClient shared = client.SpotApi.SharedClient; var result = await shared.GetSpotTickerAsync( new GetTickerRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"))); |


## 7.2 Symbol Normalization
| Exchange | Native örnek | Shared karşılığı | Format strategy |
| Binance TR | BTC_USDT / API’ye göre bazı market-data uçlarında BTCUSDT | BTC/USDT | Base + Quote -> exchange formatter |
| BtcTurk | BTCTRY | BTC/TRY | Concatenate uppercase |
| Paribu | btc_tl / resmi API sözleşmesi doğrulanacak | BTC/TRY | Alias: TL <-> TRY gerekebilir |
| Bitexen | BTCTRY | BTC/TRY | Concatenate uppercase |


| Kritik Kural`TL`, `TRY`, stablecoin ve exchange-specific asset alias’ları string replace ile dağınık şekilde çözülmeyecek. `FormatSymbol`, `SharedSymbol` ve merkezi asset alias mapping kullanılacak. |


## 7.3 Shared Interface Öncelik Sırası
| Öncelik | Interface | MVP |
| P0 | ISpotSymbolRestClient | Zorunlu |
| P0 | ISpotTickerRestClient | Zorunlu |
| P0 | IOrderBookRestClient | Zorunlu |
| P0 | IRecentTradeRestClient | Zorunlu |
| P0 | IBalanceRestClient | Zorunlu |
| P0 | ISpotOrderRestClient | Zorunlu |
| P1 | IKlineRestClient | API destekliyorsa |
| P1 | IFeeRestClient | API destekliyorsa |
| P1 | ITradeHistoryRestClient | Destekliyorsa |
| P1 | Ticker/Trade/OrderBook socket interfaces | Zorunlu public socket |
| P1 | ISpotOrderSocketClient / IBalanceSocketClient | Private stream destekliyorsa |
| P2 | IDepositRestClient / IWithdrawalRestClient | Regülasyon ve capability review sonrası |


## 7.4 Model Mapping Kuralları
[ListBullet] Fiyat ve miktar `decimal`; floating-point (`double`) yalnız exchange response gerçekten gerektiriyorsa internal parsing aşamasında kullanılabilir.
[ListBullet] Timestamp UTC `DateTime`/`DateTimeOffset` olarak normalize edilir; raw epoch gerektiğinde native modelde tutulabilir.
[ListBullet] Order side/status/type conversion tabloları ayrı converter/test setiyle yönetilir.
[ListBullet] Native modelde bulunmayan shared alan `null` kalır; tahmin edilmez.
[ListBullet] Volume/quantity semantiği (base/quote) açıkça ayrılır; SharedOrderQuantity kullanımı CryptoExchange.Net sürümüyle uyumlu yapılır.
[ListBullet] Unknown enum/value geldiğinde JSON deserialization tüm cevabı düşürmemeli; mümkünse Unknown mapping veya tolerant converter kullanılmalı.
# 8. Temel Kullanım Senaryoları
| ID | Senaryo | Beklenen davranış |
| UC-01 | Tek borsadan ticker | Developer BTC/TRY son fiyatını exchange-native veya shared API ile alır. |
| UC-02 | Borsalar arası fiyat karşılaştırma | Aynı SharedSymbol için desteklenen borsalardan ticker paralel alınır. |
| UC-03 | Order book toplama | Bir veya birden çok exchange’ten snapshot + websocket delta alınır. |
| UC-04 | Spot emir oluşturma | Authenticated client ile market/limit emir exchange kurallarına göre gönderilir. |
| UC-05 | Emir iptali/sorgulama | OrderId/clientId destek durumuna göre native/shared çağrı yapılır. |
| UC-06 | Bakiye okuma | Free/locked bakiye normalize edilir. |
| UC-07 | Realtime ticker | Socket client reconnect sonrası subscription’ı sürdürür. |
| UC-08 | User stream | Exchange destekliyorsa balance/order değişiklikleri private socket üzerinden alınır. |
| UC-09 | Multi-account | Aynı process’te farklı credential setleri güvenli biçimde yönetilir. |
| UC-10 | Capability discovery | Consumer `.Discover()` ile exchange’in desteklediği shared operasyonları runtime’da kontrol eder. |


## 8.1 Örnek Multi-Exchange Kullanım
| var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "TRY"); var tasks = new[] { btcTurk.SpotApi.SharedClient.GetSpotTickerAsync(new GetTickerRequest(symbol)), paribu.SpotApi.SharedClient.GetSpotTickerAsync(new GetTickerRequest(symbol)), binanceTr.SpotApi.SharedClient.GetSpotTickerAsync(new GetTickerRequest(symbol)) }; var results = await Task.WhenAll(tasks); |


# 9. Exchange Capability ve Endpoint Kapsamı
## 9.1 Binance TR
Binance TR resmi dokümantasyonu ayrı bir REST/WebSocket yüzeyi tanımlar. SIGNED endpointler `X-MBX-APIKEY` header ve HMAC-SHA256 signature kullanır; `timestamp`/`recvWindow` modeli vardır. Rate limitler IP weight ve account order-count mantığıyla çalışır; 429 sonrası backoff, tekrar ihlalinde 418 IP ban davranışı dokümante edilmiştir. `GET /open/v1/common/symbols`, `POST /open/v1/orders`, `GET /open/v1/account/spot` gibi endpointler ve ayrı WebSocket API bulunur [R7].
| Capability | Resmi kanıt / endpoint | MVP mapping |
| Server time | GET /open/v1/common/time | Time sync |
| Symbols | GET /open/v1/common/symbols | ISpotSymbolRestClient |
| Market trades / klines | Public market endpoints | IRecentTradeRestClient / IKlineRestClient |
| Place order | POST /open/v1/orders | ISpotOrderRestClient |
| Query/cancel/all orders | /open/v1/orders* | ISpotOrderRestClient |
| Account | GET /open/v1/account/spot | IBalanceRestClient + fee data |
| TRY commissions | fiatMakerCommission / fiatTakerCommission | Native model + IFee mapping mümkünse |
| User data | listenToken + WebSocket API | TokenManager + private socket |


| Binance TR için tasarım kararıBinance TR, Binance.Net içindeki yeni bir `Environment` olarak modellenmemelidir. Path, symbol formatı, response envelope ve user-stream mekanizması yeterince farklıdır; ayrı adapter daha düşük coupling ve daha güvenli upgrade sağlar. |


## 9.2 BtcTurk
BtcTurk resmi dokümantasyonu base URL olarak `https://api.btcturk.com` verir. Private API V1; public market data V2 kullanır. Authentication `X-PCK` public key, `X-Stamp` millisecond nonce ve Base64 secret ile HMAC-SHA256 `X-Signature` header’larına dayanır. Public/private rate limitler endpoint ve IP/account bazında ayrı tanımlanır. WebSocket endpoint `wss://ws-feed-pro.btcturk.com` olup channel/event tabanlı JSON protokolü kullanır [R8][R9][R10][R11].
| Capability | BtcTurk özelliği | MVP mapping |
| Exchange info / symbols | Public REST | ISpotSymbolRestClient |
| Ticker | /api/v2/ticker | ISpotTickerRestClient |
| Order book | /api/v2/orderBook | IOrderBookRestClient |
| OHLC | graph/ohlc endpointleri | IKlineRestClient |
| Balance | /api/v1/users/balances | IBalanceRestClient |
| Place/cancel/query order | /api/v1/order | ISpotOrderRestClient |
| Trades/history | public + user trade endpoints | IRecentTradeRestClient / ITradeHistoryRestClient |
| WebSocket market data | channel/event protocol | Ticker/Trade/OrderBook socket |
| WebSocket auth | Ayrı HMAC key flow | Private socket varsa dedicated auth provider |


## 9.3 Paribu
Paribu, public API desteğini 5 Eylül 2025’te kullanıcılarına açmıştır. 2026 destek dokümanında API key + secret, Trading ve Withdrawal izinleri, IP allow-list, süre sınırı ve HMAC-SHA256 güvenlik modeli açıkça belirtilmektedir. Alım/satım emirleri, emir iptali ve kripto çekim otomasyonu resmi olarak desteklenen kullanım alanları arasındadır [R12][R13].
| Paribu implementation gateParibu’nun resmi teknik dokümantasyonu geliştirme başlangıcında source-of-truth olarak tekrar açılacak ve endpoint/path/schema envanteri story PAR-001 kapsamında repo içine `docs/vendor/paribu-capabilities.md` olarak dondurulacaktır. Üçüncü taraf wrapper yalnız keşif/referans amaçlıdır; kontrat kaynağı olarak kullanılmayacaktır. |


| Capability | MVP beklentisi | Doğrulama |
| Symbols/ticker/order book | Evet | Resmi API docs |
| Klines/trades | API destekliyorsa | Resmi API docs |
| Balance/user info | Evet | Resmi API docs |
| Place/cancel/query order | Evet | Resmi API docs |
| Deposit address / transfer | Native P1 | Resmi API docs + güvenlik review |
| WebSocket ticker/orderbook | Varsa P1 | Resmi API docs |
| Private socket | Varsa P1/P2 | Resmi API docs |


## 9.4 Bitexen - Faz 2
Bitexen resmi API dokümanında market info, ticker, order book, balance, orders, order create/cancel ve withdrawal endpointleri yer alır. Dokümante edilen genel limit 60 request/dakika; private requestlerde ACCESS-USER, ACCESS-PASSPHRASE, ACCESS-TIMESTAMP, ACCESS-SIGN ve ACCESS-KEY header’ları ile HMAC-SHA256 imza kullanılır [R14].
| Capability | Durum |
| Market info/ticker/orderbook | Resmi REST mevcut |
| Balance/orders | Resmi private REST mevcut |
| Place/cancel order | Resmi private REST mevcut |
| Withdraw | Resmi private REST mevcut |
| WebSocket | MVP öncesi ayrıca doğrulanmalı |
| Öncelik | MVP sonrası Faz 2 |


## 9.5 Capability Matrix - MVP
| Feature | Binance TR | BtcTurk | Paribu | Bitexen |
| Symbols | P0 | P0 | P0 | P2 |
| Ticker | P0 | P0 | P0 | P2 |
| Order Book | P0 | P0 | P0 | P2 |
| Trades | P0 | P0 | P0 | P2 |
| Klines | P1 | P1 | P1* | P2 |
| Balances | P0 | P0 | P0 | P2 |
| Place Order | P0 | P0 | P0 | P2 |
| Cancel Order | P0 | P0 | P0 | P2 |
| Open/Order Query | P0 | P0 | P0 | P2 |
| Public WS | P0 | P0 | P1* | P2* |
| Private WS | P1 | P1* | P1* | P2* |
| Withdraw/Deposit | P2 | P2 | P2 | P2 |


* = Resmi teknik dokümantasyon implementasyon story’sinde capability teyidi gerektirir.
# 10. REST Client Tasarımı
## 10.1 Client Lifecycle
REST client uzun ömürlü ve thread-safe kullanım hedefiyle tasarlanır. DI ile singleton/transient davranışı CryptoExchange.Net guidance’a göre kayıt edilir; consumer’ın her request için client oluşturması örneklerde teşvik edilmez.
## 10.2 ApiClient Sorumlulukları
[ListBullet] Base address/environment seçimi
[ListBullet] AuthenticationProvider oluşturma
[ListBullet] Symbol formatting
[ListBullet] Request definitions ve endpoint weights
[ListBullet] Server-time offset kullanımı
[ListBullet] Error parsing
[ListBullet] SharedClient implementation exposure
[ListBullet] Request validation ve optional feature discovery
## 10.3 Endpoint Method Standardı
| public Task<HttpResult<BinanceTRTicker>> GetTickerAsync( string symbol, CancellationToken cancellationToken = default) { // validate input // create request definition // SendAsync<T> through CryptoExchange.Net } |


## 10.4 Parameter Validation
| Kural | Davranış |
| Null/empty symbol | Network çağrısından önce client validation error |
| Negative quantity/price | Client validation error |
| Unsupported order type | Endpoint options/shared validation ile rejected |
| recvWindow upper bound | Exchange dokümantasyonuna göre validate |
| From/To date | Exchange max-range kurallarına göre validate |
| Pagination | Shared PageRequest + native page/token mapping |


## 10.5 Response Envelope ve Error Parsing
Exchange’in response envelope’ı native modelde doğru parse edilir; consumer’a CryptoExchange.Net result tipi döndürülür. HTTP 200 içinde business error dönen API’lerde `Success=false` üretilmelidir. Unknown error code raw code/message ile korunur. Retry kararı error parsing katmanında business error ile transient transport error ayrımına göre verilir.
# 11. WebSocket Tasarımı
## 11.1 Temel Gereksinimler
[ListBullet] Automatic reconnect sonrası subscription’ların yeniden kurulması
[ListBullet] Subscription ID/channel routing
[ListBullet] Unknown message type’ların client’ı düşürmemesi
[ListBullet] Order-book sequence/delta bütünlüğü
[ListBullet] Heartbeat/ping/pong yönetimi
[ListBullet] Connection rate limitlerine uyum
[ListBullet] Cancellation ve graceful unsubscribe
[ListBullet] Socket dispose sırasında callback yarışlarının önlenmesi
[ListBullet] Private stream token/listen-key lifecycle yönetimi
## 11.2 Binance TR User Stream
Binance TR dokümanında eski user-data-stream endpointleri 2026’da deprecate edilerek `POST /open/v1/user-listen-token` ve WebSocket API üzerinden `userDataStream.subscribe.listenToken` akışına geçiş belirtilmiştir. Token otomatik yenilenmediğinden TokenManager benzeri lifecycle yönetimi uygulanmalıdır [R7].
## 11.3 BtcTurk Socket Routing
BtcTurk WebSocket protokolü channel/event ve numeric type içeren JSON mesajları kullanır. Routing yalnız event adına bağlı olmamalı; channel + event + type kombinasyonuna toleranslı tasarlanmalı ve dokümantasyonun belirttiği gibi gelecekte eklenecek bilinmeyen message type’lar ignore edilebilmelidir [R11].
## 11.4 Order Book
| Durum | Beklenen davranış |
| Initial snapshot | REST veya socket snapshot alınır |
| Delta sequence | Sequence/update id doğrulanır |
| Gap detected | Book invalid işaretlenir ve resync yapılır |
| Reconnect | Yeni snapshot + delta sync |
| Consumer callback | Data age/timestamp korunur |
| High throughput | Allocation azaltan converter/routing tercih edilir |


# 12. Authentication, Credential ve Güvenlik
## 12.1 Credential Model
Her exchange kendi credential tipini kullanır. Generic `ApiCredentials` kabul edilebilir olsa da borsa passphrase/username/RSA/Ed25519 gibi ek bilgi gerektiriyorsa dedicated credentials zorunludur. Secret hiçbir `ToString`, exception, logger scope veya telemetry attribute içinde raw yazılmaz.
| Exchange | Credential alanları - minimum | Signing |
| Binance TR | ApiKey + Secret | HMAC-SHA256 totalParams; X-MBX-APIKEY |
| BtcTurk | PublicKey + Base64 Secret | HMAC-SHA256(publicKey+nonce), Base64 signature |
| Paribu | ApiKey + Secret + permissions dış sistemde | HMAC-SHA256; kesin payload resmi docs ile sabitlenecek |
| Bitexen | ApiKey + Secret + Username + Passphrase | HMAC-SHA256(apiKey+username+passphrase+timestamp+body) |


## 12.2 Secret Handling Kuralları
[ListBullet] Repository ve appsettings.json içine gerçek secret commit edilmez.
[ListBullet] Examples `.env.example`/User Secrets/secret manager kullanır.
[ListBullet] CI integration credential’ları environment secret olarak inject eder.
[ListBullet] Logs API key’in tamamını değil en fazla maskeli fingerprint’i gösterebilir.
[ListBullet] Signature/query body loglanacaksa signature ve sensitive field redaction uygulanır.
[ListBullet] Withdrawal permission gerektirmeyen test key’lerinde withdrawal kapalı tutulur.
[ListBullet] Production credential kullanılarak CI testi çalıştırılmaz.
## 12.3 Threat Model Özeti
| Tehdit | Kontrol |
| Secret leakage | Masking + secret scanner + no raw request logs |
| Replay | Timestamp/nonce + recvWindow + time sync |
| Rate-limit ban | Client-side limiter + Retry-After |
| Order duplication | ClientOrderId/idempotency where supported + retry policy separation |
| MITM | HTTPS/WSS only; certificate validation bypass yok |
| Malicious dependency | Pinned/central versions + Dependabot/Renovate + SBOM |
| Supply-chain package hijack | Reserved NuGet IDs, signed/reproducible pipeline, 2FA |
| Withdrawal misuse | MVP shared withdraw kapalı; least privilege docs |


# 13. Rate Limit, Retry, Time Sync ve Dayanıklılık
## 13.1 Rate Limit
Rate limits exchange adapter içinde request definitions ve CryptoExchange.Net limiter primitive’leriyle tanımlanır. Binance TR weight header’ları ve account order count; BtcTurk endpoint/ip/account limitleri; Bitexen dakikalık limit gibi farklı politikalar native olarak modellenir [R7][R10][R14].
## 13.2 Retry Policy
| Hata | Otomatik retry? | Kural |
| Network timeout/connect reset | Evet, sınırlı | Exponential backoff + jitter |
| HTTP 429 | Evet | Retry-After’a uy |
| HTTP 418 / ban | Hayır immediate | Retry-After / ban süresi boyunca dur |
| 5xx | Koşullu | GET/idempotent requestlerde sınırlı |
| Place order timeout | Varsayılan hayır | Order oluşmuş olabilir; query/reconciliation gerekir |
| 4xx validation/auth | Hayır | Consumer/config düzeltmeli |
| Business insufficient balance | Hayır | Deterministic business error |


## 13.3 Time Sync
Signed API’lerde local clock drift kritik olduğundan server-time endpointi bulunan exchange’lerde offset periyodik hesaplanır. Offset cache edilir; timestamp request anında UTC + offset üzerinden üretilir. Büyük drift detection warning log üretir; consumer’a sistem saatini düzeltme önerisi verilir.
## 13.4 Circuit Breaker
Library seviyesinde agresif global circuit breaker zorunlu değildir; consumer uygulamanın resilience policy’siyle çakışabilir. Ancak rate-limit/ban state’i exchange limiter içinde korunmalı. Opsiyonel Polly entegrasyonu adapter içine hard dependency olarak alınmamalıdır.
# 14. Logging, Telemetry ve Operasyonel Görünürlük
| Event | Seviye | İçerik |
| Request start/end | Debug/Trace | Exchange, endpoint, duration, status; secret yok |
| Rate limit wait | Debug/Warning | Limiter, wait duration |
| 429/418 | Warning/Error | RetryAfter, policy, endpoint |
| Socket reconnect | Information | Connection id, attempt |
| Resubscribe failed | Error | Subscription type/symbol |
| Deserialization unknown field | Trace | Breaking değilse low noise |
| Auth failure | Warning | Credential fingerprint only |
| Order business reject | Information/Warning | Error code/message; secret yok |


## 14.1 OpenTelemetry Hazırlığı
MVP’de doğrudan OpenTelemetry package dependency zorunlu değildir. Ancak Activity/ILogger ile uyumlu correlation alanları kullanılmalı: `exchange`, `api`, `operation`, `symbol`, `authenticated`, `http.status_code`, `retry.count`, `rate_limit.wait_ms`. OrderId gibi müşteri/işlemsel değerlerin telemetry privacy etkisi dokümante edilmelidir.
# 15. Test Stratejisi
## 15.1 Test Piramidi
| Test tipi | Amaç | CI |
| Unit | Converters, validation, signing, symbol formatting, error parsing | Her PR |
| Request contract | HTTP method/path/query/body/header üretimi | Her PR |
| Serialization | Fixture JSON -> model ve model mapping | Her PR |
| Shared API validation | Shared request/options/mapping doğrulama | Her PR |
| Public integration | Canlı public endpoints smoke/contract | Scheduled + main |
| Private integration | Test account ile read/trade controlled | Manual/scheduled protected |
| WebSocket integration | Subscribe/reconnect/unsubscribe | Scheduled |
| Package smoke | NuGet pack -> sample restore/build | Release |


## 15.2 Authentication Test Vektörleri
AuthenticationProvider için gerçek secret kullanılmadan sabit key/timestamp/request değerleriyle deterministic expected signature fixture yazılır. BtcTurk Base64 secret decode dahil; Binance TR query/body concatenation varyasyonları dahil test edilir.
## 15.3 Serialization Fixture Politikası
[ListBullet] Resmi docs örnek response’ları veya sanitized real response fixture olarak saklanır.
[ListBullet] Fixture dosya adı endpoint + senaryo belirtir.
[ListBullet] Null/missing/unknown field varyasyonları eklenir.
[ListBullet] Decimal precision kaybı testi yapılır.
[ListBullet] Enum unknown value davranışı test edilir.
[ListBullet] DateTime epoch/ms/sec ayrımı explicit fixture ile doğrulanır.
## 15.4 WebSocket Testleri
| Senaryo | Acceptance |
| Initial subscribe | Callback en az bir event alır veya timeout net error döner |
| Unsubscribe | Callback artık tetiklenmez |
| Reconnect simulation | Subscription restore edilir |
| Unknown message | Connection canlı kalır |
| Order book gap | Resync tetiklenir |
| Token expiry | Private stream token yenileme/re-subscribe akışı çalışır |


## 15.5 Coverage Hedefi
Line coverage tek başına gate değildir. Kritik hedefler: authentication/signing %100 branch; shared mappings %100; converters %95+; error parsing %90+; public endpoint request definitions %100 test edilmiş. Integration coverage capability matrix üzerinden takip edilir.
# 16. CI/CD, NuGet ve Release Yönetimi
## 16.1 PR Pipeline
[ListNumber] dotnet restore
[ListNumber] dotnet build -c Release --no-restore
[ListNumber] dotnet test -c Release --no-build
[ListNumber] format/analyzer gate
[ListNumber] secret scan
[ListNumber] pack smoke test
[ListNumber] API surface/breaking change check - mümkünse
## 16.2 Release Pipeline
[ListNumber] Git tag `vX.Y.Z` veya package-scoped tag strategy.
[ListNumber] Release build + full unit tests.
[ListNumber] Public integration tests.
[ListNumber] NuGet `.nupkg` + `.snupkg` üretimi; SourceLink doğrulama.
[ListNumber] Package metadata/README/license/icon validation.
[ListNumber] NuGet publish approval gate.
[ListNumber] GitHub Release notes + changelog.
## 16.3 Package Metadata
| <PackageId>TRCrypto.BinanceTR</PackageId> <Authors>TRCrypto Contributors</Authors> <Description>High-performance .NET client for Binance TR REST and WebSocket APIs, built on CryptoExchange.Net.</Description> <PackageLicenseExpression>MIT</PackageLicenseExpression> <RepositoryType>git</RepositoryType> <PublishRepositoryUrl>true</PublishRepositoryUrl> <IncludeSymbols>true</IncludeSymbols> <SymbolPackageFormat>snupkg</SymbolPackageFormat> <GeneratePackageOnBuild>true</GeneratePackageOnBuild> |


## 16.4 Compatibility Policy
CryptoExchange.Net upgrade’ı dependency bot tarafından otomatik PR açabilir; merge öncesi tüm exchange unit tests + shared validation + public integration suite geçmelidir. Breaking base library upgrade, tüm adapterlar aynı anda hazır değilse bundle paketine alınmamalıdır.
# 17. Dokümantasyon ve Developer Experience
## 17.1 Her Package için Zorunlu README Bölümleri
[ListBullet] Install
[ListBullet] Supported frameworks
[ListBullet] Quick start public REST
[ListBullet] Authentication setup
[ListBullet] Place/cancel order örneği
[ListBullet] WebSocket örneği
[ListBullet] SharedApis örneği
[ListBullet] Supported capabilities matrix
[ListBullet] Rate limits notes
[ListBullet] Error handling pattern
[ListBullet] DI setup
[ListBullet] Security notes
[ListBullet] Changelog/release notes
[ListBullet] Official API documentation link
## 17.2 AI Coding Assistant Dosyaları - P1
JKorf 2026 itibarıyla AGENTS.md, Cursor rules, Copilot instructions, llms.txt ve compilable AI-friendly examples sunmaktadır. TRCrypto repository de aynı yaklaşımı P1 developer-experience hedefi olarak kullanabilir; ancak bu dosyalar implementation correctness’i belgeleyen testlerin yerine geçmez [R2][R4][R18].
## 17.3 API Documentation Sync
Her exchange için `docs/vendor/<exchange>-capabilities.md` dosyasında dokümantasyon erişim tarihi, base URLs, auth scheme, endpoint listesi, rate limit ve websocket bilgisi tutulur. Borsa changelog’u varsa release öncesi kontrol edilir. Bu dosya kopyalanmış tam doküman değil, source linkli kısa envanterdir.
# 18. Developer Başlangıç Paketi
| İlk implementasyon sırası1) Solution/common build altyapısı, 2) Binance TR public REST, 3) Binance TR auth + account/order, 4) Binance TR sockets, 5) SharedApis, 6) BtcTurk, 7) Paribu, 8) TRCrypto.Clients bundle. Bu sıra en güncel ve açık resmi dokümana sahip adapter ile framework convention’larını önce oturtur. |


## 18.1 İlk Gün - Repository Bootstrap
| git init TRCrypto cd TRCrypto dotnet new sln -n TRCrypto dotnet new classlib -n TRCrypto.BinanceTR -o src/TRCrypto.BinanceTR dotnet new classlib -n TRCrypto.BtcTurk -o src/TRCrypto.BtcTurk dotnet new classlib -n TRCrypto.Paribu -o src/TRCrypto.Paribu dotnet new classlib -n TRCrypto.Clients -o src/TRCrypto.Clients dotnet new xunit -n TRCrypto.BinanceTR.UnitTests -o tests/TRCrypto.BinanceTR.UnitTests dotnet new xunit -n TRCrypto.BinanceTR.IntegrationTests -o tests/TRCrypto.BinanceTR.IntegrationTests dotnet sln add src/**/*.csproj tests/**/*.csproj |


## 18.2 Binance TR Minimum Sınıf İskeleti
| Dosya | İlk sorumluluk |
| BinanceTRExchange.cs | Name, URL, docs URL, platform info |
| BinanceTREnvironment.cs | Production base addresses + future test env |
| BinanceTRCredentials.cs | API key/secret |
| BinanceTRAuthenticationProvider.cs | HMAC signing + header |
| Objects/Options/BinanceTRRestOptions.cs | ApiCredentials, environment, request timeout |
| Clients/BinanceTRRestClient.cs | Root REST client |
| Clients/SpotApi/BinanceTRRestApiClient.cs | Spot REST base api + formatter + shared client |
| ...ExchangeData.cs | time, symbols, ticker, book, trades, klines |
| ...Trading.cs | place/cancel/query orders |
| ...Account.cs | account/balance/trade history |
| BinanceTRSocketClient.cs | Root socket client |
| ...SocketApi*.cs | market/user subscriptions |
| BinanceTRErrors.cs | Error code mapping |


## 18.3 İlk Public Endpoint Definition Örneği
| private static readonly RequestDefinition GetTime = new(HttpMethod.Get, "/open/v1/common/time", RateLimitGate: /* configured rule */, Authenticated: false); public async Task<HttpResult<BinanceTRServerTime>> GetServerTimeAsync( CancellationToken cancellationToken = default) { return await SendAsync<BinanceTRServerTime>(GetTime, cancellationToken) .ConfigureAwait(false); } |


Not: Yukarıdaki kod API şekli örneğidir; `RequestDefinition` constructor parametreleri CryptoExchange.Net 12.5.0 actual API ile IDE üzerinden doğrulanarak yazılmalıdır. Doküman eski sürüm signature’ını ezbere kopyalamayı değil güncel base-library contract’ına uymayı şart koşar.
## 18.4 DI Kullanım Hedefi
| services.AddTRCryptoBinanceTR(options => { options.ApiCredentials = new BinanceTRCredentials(apiKey, apiSecret); }); // Consumer public sealed class PriceService(IBinanceTRRestClient client) { // reuse injected client } |


## 18.5 İlk Pull Request Sınırı
PR-001 yalnız repository bootstrap + project metadata + base Binance TR client + server time + symbols + request/serialization tests içermelidir. Authentication veya order placement aynı PR’a eklenmemelidir. Amaç convention’ları küçük bir vertical slice ile stabilize etmektir.
# 19. Backlog: Epic / Story / Acceptance Criteria
## 19.1 Epic Listesi
| Epic | Başlık | Çıktı |
| E01 | Foundation & Repository | Solution, packages, analyzers, CI, conventions |
| E02 | Binance TR Public REST | Market-data native client |
| E03 | Binance TR Private REST | Auth/account/trading |
| E04 | Binance TR WebSocket | Public/private streams |
| E05 | Binance TR SharedApis | Unified interfaces |
| E06 | BtcTurk Adapter | REST/socket/shared |
| E07 | Paribu Adapter | REST/socket/shared |
| E08 | TRCrypto.Clients | Bundle + multi-exchange |
| E09 | Documentation & Examples | README/docs/samples |
| E10 | Release & Hardening | Security, integration, NuGet release |


| ID | Pri | Story | Kapsam | Acceptance Criteria |
| FND-001 | P0 | Repo bootstrap | Solution/src/tests/examples/docs oluştur; CPM ve common props ekle. | `dotnet build` tüm targetlarda geçer; warnings-as-errors; CryptoExchange.Net=12.5.0 merkezi. |
| FND-002 | P0 | CI PR pipeline | Restore/build/test/pack/secret scan. | PR üzerinde fail-fast; artifact olarak local packages. |
| FND-003 | P0 | Public API conventions | Naming, result, cancellation, XML docs standardını kodla. | Sample endpoint standarda uyuyor; analyzer temiz. |
| BTR-001 | P0 | Vendor capability freeze | Binance TR resmi API envanterini repo docs’a kaydet. | Base URLs, auth, rate limits, public/private/ws endpoint listesi kaynak linkli. |
| BTR-002 | P0 | Environment & root clients | BinanceTRExchange/Environment/RestClient/SocketClient. | Client üretilebiliyor; production address testli. |
| BTR-003 | P0 | Server time + symbols | Time ve supported symbols. | Response models parse; validation tests; symbol filters preserve. |
| BTR-004 | P0 | Ticker/trades/orderbook | Public market data. | Her endpoint request + fixture + public integration test. |
| BTR-005 | P1 | Klines | Kline endpoint/formats. | Interval mapping + pagination/range validation. |
| BTR-006 | P0 | AuthenticationProvider | X-MBX-APIKEY + HMAC SHA256. | Official sample/test vector signature birebir; secret loglanmıyor. |
| BTR-007 | P0 | Account/balances | Spot account endpoint. | Free/locked parse; TRY fee fields native modelde. |
| BTR-008 | P0 | Place order | Limit/market + required params. | Price/qty validation; no unsafe auto-retry; successful integration smoke. |
| BTR-009 | P0 | Cancel/query/orders | Cancel, detail, open/all orders. | Order state mapping fixture testleri. |
| BTR-010 | P1 | User trades | Account trade history. | Pagination/time filters docs’a uyumlu. |
| BTR-011 | P0 | Public WebSocket | Ticker/trade/orderbook subscriptions. | Subscribe/unsubscribe/reconnect test. |
| BTR-012 | P1 | User listen token | Token create + socket subscribe lifecycle. | Expiry/refresh/re-subscribe handled; deprecated listenKey yolu default değil. |
| BTR-013 | P0 | Shared symbol/ticker/book/trades | Core market SharedApis. | SharedSymbol BTC/TRY doğru native format; Discover reports capability. |
| BTR-014 | P0 | Shared balances/orders | IBalance + ISpotOrder. | Native->Shared mapping deterministic; validation tests. |
| BTC-001 | P0 | BtcTurk vendor freeze | Official REST/auth/rate/ws envanteri. | V1/V2 ayrımı ve limits kaynak linkli. |
| BTC-002 | P0 | BtcTurk auth | X-PCK/X-Stamp/X-Signature. | Fixed timestamp signature test passes. |
| BTC-003 | P0 | BtcTurk public REST | Info/ticker/book/trades/ohlc. | Fixtures + public integration. |
| BTC-004 | P0 | BtcTurk private REST | Balance/orders/user trades. | Auth tests + controlled integration. |
| BTC-005 | P0 | BtcTurk WebSocket | Channel/event router. | Unknown type tolerated; reconnect/resubscribe. |
| BTC-006 | P0 | BtcTurk SharedApis | Market/balance/order mappings. | Shared suite passes. |
| PAR-001 | P0 | Paribu vendor freeze | Official API docs capability/path/schema envanteri. | No third-party endpoint treated as source of truth. |
| PAR-002 | P0 | Paribu auth | Official HMAC algorithm. | Official example/test vector reproduced. |
| PAR-003 | P0 | Paribu public REST | Symbols/ticker/book/trades/klines supported set. | Public integration + fixtures. |
| PAR-004 | P0 | Paribu private REST | User/balance/orders. | Least-privilege test key; no withdraw permission needed. |
| PAR-005 | P1 | Paribu WebSocket | Officially supported streams. | Capability exists -> subscribe/reconnect tests; yoksa Discover unsupported. |
| PAR-006 | P0 | Paribu SharedApis | Common mappings. | Shared validation passes. |
| CLI-001 | P0 | TRCrypto.Clients bundle | All adapter clients exposed. | Tek package ile 3 exchange erişilebilir. |
| CLI-002 | P1 | Multi-exchange helpers | Common discovery/sample routing. | Unsupported operation graceful skip. |
| DOC-001 | P0 | Package READMEs | Install/auth/native/shared/socket. | Copy-paste examples compile. |
| DOC-002 | P1 | AI assistant guidance | AGENTS/llms/examples. | No invented endpoint; source-map included. |
| REL-001 | P0 | NuGet release pipeline | nupkg/snupkg + SourceLink. | Dry run package restore/build succeeds. |
| SEC-001 | P0 | Security hardening | Secret redaction, scans, credential tests. | No secret in logs/snapshots; scan clean. |
| REL-002 | P0 | v0.1.0 preview | Preview publish. | 3 exchanges P0 capability + docs + tests green. |


## 19.2 Story Uygulama Şablonu
| Story ID: BTR-XXX Business Goal: Scope: Official API Source + Access Date: Native Client Method: Request Method/Path/Auth/Weight: Request Model: Response Model: Error Cases: SharedApi Mapping (if any): Unit Tests: Integration Tests: Security/Logging Notes: Acceptance Criteria: Out of Scope: |


## 19.3 Tahmini Uygulama Fazları
| Faz | İçerik | Çıkış koşulu |
| M0 | Foundation | Build/test/pack pipeline green |
| M1 | Binance TR public | Public REST native usable |
| M2 | Binance TR trading | Auth/account/order usable |
| M3 | Binance TR sockets/shared | First complete adapter |
| M4 | BtcTurk | Second complete adapter |
| M5 | Paribu | Third complete adapter |
| M6 | Bundle/docs | Multi-exchange DX complete |
| M7 | Preview release | NuGet v0.1.0-preview + integration suite |


# 20. Riskler, Varsayımlar ve Açık Konular
| ID | Risk / Varsayım | Etki | Aksiyon |
| RISK-01 | Borsa API dokümanı sık değişebilir | Breaking runtime | Vendor capability file + changelog watch + contract tests |
| RISK-02 | Private integration için testnet olmayabilir | Trade test riski | Dedicated düşük limitli test hesap + manual protected pipeline |
| RISK-03 | NuGet package isimleri dolu olabilir | Publish engeli | TRCrypto.* prefix rezervasyonu |
| RISK-04 | Paribu teknik docs crawler erişimi kısıtlı | Spec drift | Implementation başında browser/manual official docs freeze |
| RISK-05 | Symbol alias TL/TRY farklılıkları | Wrong market/order | Central alias mapping + fixture tests |
| RISK-06 | Order retry duplicate trade yaratabilir | Finansal zarar | Non-idempotent retry kapalı; reconciliation |
| RISK-07 | WebSocket sequence bilgisi eksik olabilir | Stale book | Snapshot/resync strategy per exchange |
| RISK-08 | CryptoExchange.Net hızlı sürüm değişimi | Maintenance | Pinned version + upgrade CI matrix |
| RISK-09 | Trademark/official affiliation algısı | Legal/brand | Neutral package naming; “unofficial” disclaimer |
| RISK-10 | Withdraw API regülasyon farkları | Security/compliance | MVP dışında; ayrı threat/regulatory review |


## 20.1 Açık Kararlar
[ListBullet] GitHub organization adı ve NuGet owner kesinleştirilecek.
[ListBullet] MIT lisans önerisi maintainer tarafından onaylanacak.
[ListBullet] Paribu WebSocket/private stream capability resmi docs üzerinden story PAR-001’de kesinleştirilecek.
[ListBullet] Bitexen Faz 2 sırası kullanıcı talebi ve API canlılığına göre teyit edilecek.
[ListBullet] Full multi-target list ilk bootstrap’ta CryptoExchange.Net transitive compatibility ile doğrulanacak.
# 21. Architecture Decision Records
| ADR | Karar | Durum | Gerekçe |
| ADR-001 | CryptoExchange.Net yeniden yazılmayacak | Accepted | Mature base, SharedApis ve socket/rate-limit altyapısı zaten mevcut. |
| ADR-002 | Monorepo ile başlanacak | Accepted | Common versioning/CI/conventions hızlandırır. |
| ADR-003 | Exchange başına bağımsız NuGet | Accepted | API değişiklikleri ve dependency footprint izole edilir. |
| ADR-004 | Native + Shared API birlikte sunulacak | Accepted | Feature completeness + exchange-independent kullanım dengesi. |
| ADR-005 | Binance TR ayrı adapter olacak | Accepted | Global Binance API’den path/schema/auth/user-stream farkları var. |
| ADR-006 | TRCrypto.* package prefix | Accepted | Mevcut NuGet isimleri ve affiliation riskini önler. |
| ADR-007 | Withdraw shared MVP dışında | Accepted | Yüksek güvenlik ve regülasyon farklılığı. |
| ADR-008 | System.Text.Json + CE.Net stack | Accepted | AOT/performance/ecosystem consistency. |
| ADR-009 | Non-idempotent order auto-retry yok | Accepted | Duplicate financial action riskini azaltır. |
| ADR-010 | Official vendor docs source-of-truth | Accepted | Third-party wrappers yalnız keşif/reference. |


# 22. Definition of Done ve Go-Live Kriterleri
## 22.1 Endpoint Story DoD
[ListBullet] Public interface + implementation mevcut
[ListBullet] XML documentation mevcut
[ListBullet] Request method/path/auth/weight doğru
[ListBullet] Input validation mevcut
[ListBullet] Response fixture parse testi mevcut
[ListBullet] Error fixture testi mevcut
[ListBullet] Shared mapping varsa mapping testi mevcut
[ListBullet] CancellationToken destekli
[ListBullet] Secret/logging review tamam
[ListBullet] README/capability matrix gerekirse güncellendi
[ListBullet] Public integration test veya neden mümkün olmadığı kayıtlı
## 22.2 Exchange Adapter DoD
[ListBullet] P0 capability story’leri tamam
[ListBullet] Discover() doğru destek bilgisini veriyor
[ListBullet] Public integration suite green
[ListBullet] Private auth/signature suite green
[ListBullet] WebSocket reconnect/unsubscribe green
[ListBullet] NuGet pack + sample restore green
[ListBullet] No high/critical dependency vulnerability
[ListBullet] README quick-start compile ediyor
[ListBullet] Official API access date kaydedilmiş
[ListBullet] Known limitations listesi mevcut
## 22.3 v0.1.0-preview Go-Live Gate
| Gate | Zorunlu |
| Binance TR adapter P0 | Evet |
| BtcTurk adapter P0 | Evet |
| Paribu adapter P0 | Evet |
| TRCrypto.Clients bundle | Evet |
| CI & package signing/source link | Evet |
| Public integration smoke | Evet |
| Secret scan | Evet |
| Docs + samples | Evet |
| Withdrawal shared support | Hayır |
| Futures | Hayır |


# 23. Kaynaklar ve Araştırma Notları
Kaynaklar 23 Ağustos 2026 itibarıyla erişilen güncel public dokümantasyon ve repository’lerden derlenmiştir. Exchange implementasyonu sırasında ilgili resmi API dokümanının o günkü sürümü source-of-truth olarak tekrar kontrol edilmelidir.
R1 - Jan Korf (JKorf) GitHub Profile: https://github.com/JKorf - Maintainer profili, repository/package ekosistemi.
R2 - JKorf/CryptoExchange.Net: https://github.com/JKorf/CryptoExchange.Net - Base library, release notes, ecosystem ve 12.5.0 değişiklikleri.
R3 - CryptoExchange.Net AGENTS.md: https://github.com/JKorf/CryptoExchange.Net/blob/master/AGENTS.md - SharedApis, SharedSymbol, new exchange implementation guidance.
R4 - JKorf/Binance.Net: https://github.com/JKorf/Binance.Net - Repository structure, shared interfaces, client patterns, v13.5.0.
R5 - Binance.Net.csproj: https://github.com/JKorf/Binance.Net/blob/master/Binance.Net/Binance.Net.csproj - TFM, nullable, AOT, SourceLink, CryptoExchange.Net 12.5.0 dependency.
R6 - JKorf/CryptoClients.Net: https://github.com/JKorf/CryptoClients.Net - Bundle/multi-exchange design.
R7 - Binance TR API Documentation: https://www.binance.tr/apidocs - Auth, limits, /open/v1 endpoints, account, TRY fees, WebSocket/listen token.
R8 - BtcTurk API - General Information: https://docs.btcturk.com/ - Base URL, V1/V2 genel davranış.
R9 - BtcTurk Authentication V1: https://docs.btcturk.com/docs/authentication/authentication-v1/ - X-PCK/X-Stamp/X-Signature HMAC.
R10 - BtcTurk Rate Limits: https://docs.btcturk.com/docs/private-endpoints/rate-limits/ - Endpoint/IP/account/WebSocket limitleri.
R11 - BtcTurk WebSocket Feed & Authentication: https://docs.btcturk.com/docs/category/websocket-feed/ - Socket protocol ve ws endpoint.
R12 - Paribu API Key Support: https://www.paribu.com/destek/guvenlik/api-anahtarlari - API key/secret, permissions, IP limit, security.
R13 - Paribu API Announcement - 05.09.2025: https://www.paribu.com/blog/haberler/paribu-api-destegi-sunmaya-basladi/ - Public API availability, HMAC-SHA256, trading/cancel/withdraw use cases.
R14 - Bitexen API Reference: https://docs.bitexen.com/ - REST endpoints, 60/minute limit, HMAC headers/signing.
R15 - burakoner/BtcTurk.Net: https://github.com/burakoner/BtcTurk.Net - Eski CryptoExchange.Net tabanlı Türkiye implementation kanıtı; archived.
R16 - NuGet BtcTurk.Net: https://www.nuget.org/packages/BtcTurk.Net - Deprecated/unlisted package, legacy dependency ve isim sahipliği.
R17 - NuGet Paribu.Api: https://www.nuget.org/packages/Paribu.Api - Mevcut third-party package/isim sahipliği ve ekosistem referansı.
R18 - JKorf/cryptoexchange-skills-hub: https://github.com/JKorf/cryptoexchange-skills-hub - AI coding assistant ecosystem yaklaşımı.
## Ek A - Örnek Consumer API
| // Exchange-specific package using TRCrypto.BtcTurk.Clients; var btcTurk = new BtcTurkRestClient(); var ticker = await btcTurk.SpotApi.ExchangeData.GetTickerAsync("BTCTRY"); // Shared abstraction using CryptoExchange.Net.SharedApis; ISpotTickerRestClient shared = btcTurk.SpotApi.SharedClient; var sharedTicker = await shared.GetSpotTickerAsync( new GetTickerRequest(new SharedSymbol(TradingMode.Spot, "BTC", "TRY"))); |


## Ek B - Proje Başlangıç Checklist
☐ NuGet organization ve `TRCrypto.*` package ID’leri rezerve edildi.
☐ Repository branch protection aktif.
☐ CryptoExchange.Net 12.5.0 pinned.
☐ FND-001/FND-002 tamamlandı.
☐ Binance TR vendor capability snapshot kaydedildi.
☐ Public server time + symbols vertical slice merge edildi.
☐ Authentication fixed-vector test geçiyor.
☐ İlk order integration testi için dedicated test credential hazır.
☐ Secret scanning CI üzerinde aktif.
☐ Release dry-run package consumer sample ile restore/build edildi.
| Geliştiriciye Son NotBu projede “önce bütün abstraction’ları tasarla, sonra endpoint yaz” yaklaşımı kullanılmamalıdır. Binance TR server-time + symbols ile küçük bir vertical slice oluştur; CryptoExchange.Net 12.5.0 convention’larını çalışan kod üzerinde sabitle; daha sonra public market data -> auth/trading -> WebSocket -> SharedApis sırasıyla ilerle. Her yeni exchange aynı checklist üzerinden eklenmelidir. |


---
---

# EK — Doğrulama Notları (24 Ağustos 2026)

> Bu bölüm orijinal spesifikasyonun parçası **değildir**. Geliştirme başlarken doküman
> Bölüm 3.5'in emrettiği gibi ("eski sürüm signature'ını ezbere kopyalamak yerine güncel
> base-library contract'ına uy") dokümandaki iddialar canlı kaynaklardan teyit edilmiştir.
> Bir iddia ile bu ek arasında çelişki varsa **bu ek geçerlidir**.

## E.1 Teyit Edilen İddialar

| Doküman iddiası | Durum | Kanıt |
|---|---|---|
| `CryptoExchange.Net` 12.5.0 güncel sürüm | ✅ Doğru | NuGet: Jkorf, 5.826.187 indirme, en son sürüm |
| `Binance.Net` 13.5.0 | ✅ Doğru | NuGet |
| BtcTurk base URL `https://api.btcturk.com` | ✅ Doğru | `docs.btcturk.com/docs/general-information` |
| BtcTurk auth: `X-PCK` / `X-Stamp` / `X-Signature`, HMAC-SHA256, Base64 secret | ✅ Doğru | `docs.btcturk.com/docs/authentication/authentication-v1` |
| BtcTurk sembol formatı `BTCTRY` | ✅ Doğru (istek tarafı) | Resmi örnek: `?pairSymbol=BTCUSDT` |
| Bitexen imza şeması `HMAC(apiKey+userName+passPhrase+timestamp+body)` | ✅ Doğru | Bağımsız üretim implementasyonuyla karşılaştırıldı |
| Binance TR faaliyette | ✅ Doğru | binance.tr — lisanslı, 200+ TRY paritesi |

## E.2 Düzeltmeler

### D-1 — Bölüm 10.5 eksik: hata ayrıştırma mimarisi

Doküman "Exchange'in response envelope'ı native modelde doğru parse edilir" diyor ama
**nasıl** olduğunu söylemiyor. CryptoExchange.Net 12.5.0'da hata ayrıştırma
`MessageHandler.ParseErrorResponse()` üzerinden yapılır. `BtcTurkErrors` ve
envelope→error dönüşümü bu mekanizmaya bağlanmalıdır.

### D-2 — Bölüm 18.3'teki `RequestDefinition` örneği şekilseldir

12.5.0'ın gerçek imzası:

```csharp
protected virtual Task<HttpResult<T>> SendAsync<T>(
    RequestDefinition definition,
    Parameters? parameters,
    CancellationToken cancellationToken,
    Dictionary<string, string>? additionalHeaders = null,
    int? weight = null,
    int? weightSingleLimiter = null,
    string? rateLimitKeySuffix = null)
```

Ayrıca uri/body parametrelerini ayıran ikinci bir overload vardır. Base sınıf
`RestApiClient<TEnvironment, TAuthenticationProvider, TApiCredentials>` generic'idir ve
`protected abstract TAuthenticationProvider CreateAuthenticationProvider(TApiCredentials)`
implementasyonu ister.

### D-3 — Bölüm 7.2 eksik: BtcTurk sembol ayrıştırmaya gerek yok

Doküman BtcTurk için "Concatenate uppercase" diyor; bu **istek** tarafı için doğrudur.
Ancak **yanıt** tarafında BtcTurk parite bileşenlerini zaten ayrı alanlar olarak döndürür:

```json
{ "pair": "BTCTRY", "pairNormalized": "BTC_TRY",
  "numerator": "BTC", "denominator": "TRY" }
```

**Sonuç:** `SharedSymbol` eşlemesi `numerator`/`denominator` alanlarından yapılmalıdır.
Sembol string'ini ayrıştıran bir heuristik **yazılmayacaktır**. Bu, RISK-05'in
(TL/TRY alias karmaşası) BtcTurk özelindeki etkisini ortadan kaldırır.

### D-4 — Bölüm 7.4'e ek: iki farklı zaman damgası birimi

BtcTurk'te **OHLC hariç tüm** zaman damgaları milisaniye, **OHLC saniye** cinsindendir.
Tek bir `DateTimeConverter` varsayımı hatalıdır; iki ayrı converter gerekir ve her biri
kendi fixture'ıyla test edilmelidir.

### D-5 — Bölüm 7.3'e ek: varlık sınıflandırması zorunlu

`ISpotSymbolRestClient`, `SharedSymbolCatalog` üzerinden varlıkları `SharedAssetType`
(`Unspecified` / `Crypto` / `Fiat` / `TradFi`) ve nullable `SharedAssetSubType`
(`StableCoin` / `Equity` / `Commodity`) ile sınıflandırır. Türkiye borsaları için:

- **TRY → `SharedAssetType.Fiat`**, alt tip yok
- USDT/USDC → `Crypto` + `StableCoin`
- BTC/ETH vb. → `Crypto`, alt tip yok

Geçerli kombinasyonlar: `Crypto`+`StableCoin`, `TradFi`+`Equity`, `TradFi`+`Commodity`,
`Fiat`+(alt tip yok). Sınıflandırılmamış varlık kripto **varsayılmaz**.

### D-6 — Bölüm 17.2 önceliği yükseltildi: P1 → P0

Doküman JKorf'un AI assistant dosyalarını "P1 developer-experience hedefi" olarak
işaretlemiş. Ancak `JKorf/cryptoexchange-skills-hub` artık canlıdır ve 33 kurulabilir
skill içerir. `cryptoexchange-net` skill'i **geliştirmenin ilk gününde** kurulmuştur
(`.claude/skills/cryptoexchange-net`) çünkü 12.5.0 kalıplarını ezberden yazma riskini
azaltır. Bu, dokümanın kendi uyarısıyla (Bölüm 3.5) tutarlıdır.

## E.3 Bu Ekin Kapsamadığı

Binance TR (`/open/v1`), Paribu ve Bitexen'in endpoint envanterleri **henüz
doğrulanmamıştır**. Her biri kendi adapter'ı açılırken ADR-010 uyarınca resmi
dokümantasyondan doğrulanacak ve `docs/vendor/<borsa>-capabilities.md` altında
dondurulacaktır.

---

## E.4 İkinci Tur Bulguları (PR-002)

Piyasa verisi uçları eklenirken ortaya çıkan, **yalnızca canlı API ile tespit edilebilen**
ve resmi dokümantasyonda yer almayan davranışlar:

### D-7 — `code` alanının tipi uçlar arasında tutarsız

| Uç | `code` |
|---|---|
| exchangeinfo / ticker / trades | `0` (sayı) |
| **orderbook** | `"SUCCESS"` (metin) |

Zarf modelinde bu alanı `int` olarak tanımlamak, emir defteri çağrılarını deserialization
hatasıyla **tamamen kırar**. Alan metin olarak taşınır; sayısal değerler dönüştürülür.

Bu, doküman Bölüm 15.3'ün ("sanitized real response fixture olarak saklanır") neden
gerekli olduğunun somut kanıtıdır — resmi örnek yanıtlarla çalışılsaydı hata üretime kadar
fark edilmezdi.

### D-8 — Başarılı yanıtlarda `message` boş string

Dokümantasyon `null` gösterir; canlı API `""` döndürür. Null kontrolü yerine
`string.IsNullOrEmpty` kullanılmalıdır.

### D-9 — Alan adları uçlar arasında tutarsız

Aynı kavram farklı uçlarda farklı adlandırılır:

| Kavram | ticker | trades |
|---|---|---|
| Base varlık | `numeratorSymbol` | `numerator` |
| Quote varlık | `denominatorSymbol` | `denominator` |

Ayrıca sayısal alanlar ticker'da **sayı**, trades'te **metin** olarak gelir.

### D-10 — Ticker ucu tek parite için de dizi döner

`?pairSymbol=` verildiğinde bile yanıt bir dizidir. Boş dizi, bilinmeyen sembol anlamına
gelir ve `ErrorType.UnknownSymbol` olarak yüzeye çıkarılır.

### D-11 — Trades yanıtında dokümante edilmemiş `side` alanı

Canlı yanıt `"side": "buy"` / `"sell"` içerir; resmi örnekte yoktur. Shared katmanında
`SharedOrderSide` eşlemesi için kullanılır.

### D-12 — Emir defteri kademeleri `ISymbolOrderBookEntry` gerektirir

`SharedOrderBook` yapıcısı `ISymbolOrderBookEntry[]` ister. Bu arayüz **değiştirilebilir**
(`set`) özellikler tanımlar; modelimiz değiştirilemez olduğundan arayüz açıkça uygulanır ve
`set` erişimcileri `NotSupportedException` fırlatır.

---

## E.5 Üçüncü Tur Bulguları (M2 — kimlik doğrulama)

### D-13 — Bakiye ucu ondalık ayırıcı olarak virgül kullanır

Resmi dokümantasyondaki bakiye örneği:

```json
"balance": "27223,7283250757643288",
"free":    "22349,3654565035348765"
```

Oysa piyasa verisi ve emir uçları nokta kullanır (`"0.00269390"`, `"20000.00"`) ve
emir dokümantasyonu ondalık ayırıcı olarak noktayı açıkça şart koşar.

**Etki:** Bakiyeyi `InvariantCulture` ile ayrıştırmak virgülü binlik ayırıcı sayar ve
tutarı kat kat büyük gösterir. Bir bakiye görüntüleme kütüphanesinde bu sessiz ve
tehlikeli bir hatadır — istisna fırlatmaz, sadece yanlış sayı üretir.

**Çözüm:** `BtcTurkDecimalConverter` her iki ayırıcıyı da kabul eder. Belirsizlik yoktur:
BtcTurk binlik ayırıcı kullanmadığından (büyük sayılar `"3708000"` biçimindedir) bir
virgül her zaman ondalık ayırıcıdır. Doküman Bölüm 7.4'ün "decimal precision kaybı testi"
gereksinimi bu senaryoyu da kapsayacak biçimde uygulanmıştır.

### D-14 — Kimlik doğrulama için resmi test vektörü yok

Doküman Bölüm 15.2, imzalamanın "resmi sample/test vector ile birebir" doğrulanmasını
şart koşar. BtcTurk böyle bir vektör yayınlamaz; `authentication/usage` sayfası da kod
örneği içermez.

**Çözüm:** Dokümante edilen algoritmanın bağımsız bir uygulamasıyla deterministik vektörler
üretilip birim testlerine sabitlendi. Testlerden biri, Base64 decode adımı atlandığında
oluşacak *yanlış* imzayı da içerir; böylece zincirin bu adımının gerçekten uygulandığı
kanıtlanır. Vektörler `docs/vendor/btcturk-capabilities.md` dosyasına da kaydedildi.

### D-15 — `RestRequestValidator` özel converter'lı alanları doğrulayamıyor

CryptoExchange.Net'in contract test yardımcısı, ham JSON metnini model değeriyle birebir
karşılaştırır. Virgül ayırıcılı bakiye alanları bu karşılaştırmada başarısız olur
(`"27223,72..."` vs `27223.72...`).

**Çözüm:** Bakiye ucunun contract testi `skipResponseValidation: true` ile çalıştırılır —
istek üretimi ve imzalama doğrulanır. Yanıt eşlemesi ayrı ve daha ayrıntılı bir test
sınıfında (`BalanceTests`) kapsanır.

### D-16 — Emir iptali eşzamansızdır

`DELETE /api/v1/order` HTTP 200 döndürdüğünde istek yalnızca **alınmıştır**. Resmi
dokümantasyon iptalin kesinleşmesinin WebSocket kanalı 452 üzerinden duyurulduğunu belirtir.

Başarılı yanıtı "emir iptal edildi" olarak yorumlamak, bir işlem botunda aynı pozisyona
ikinci kez emir girilmesine yol açabilir. `CancelOrderAsync` dokümantasyonu bu davranışı
açıkça uyarı olarak taşır; kesinleşme için durum ayrıca sorgulanmalıdır.

Ayrıca bu uçta `code` alanı boş string döner — D-7'deki tip tutarsızlığının ikinci örneği.

### D-17 — Emir alan adları uçlar arasında farklı

| Kavram | open orders | all orders | single order | submit order |
|---|---|---|---|---|
| Sembol | `pairsymbol` | `pairsymbol` | `pairSymbol` | `pairSymbol` |
| Oluşturulma | `time` | `time` | `time` | **`datetime`** |
| İstemci kimliği | `orderClientId` | `orderClientId` | `orderClientId` | **`newOrderClientId`** |

Büyük/küçük harf farkları ayrıştırmayı etkilemez (eşleme duyarsızdır), ancak
`time`/`datetime` ve `orderClientId`/`newOrderClientId` gerçek isim farklarıdır ve emir
oluşturma yanıtı için ayrı bir model gerektirir (`BtcTurkOrderPlacement`).

### D-18 — Emir yöntemi yazımı uçlar arasında farklı

`exchangeinfo` ucu `STOP_MARKET` / `STOP_LIMIT` (alt çizgili, büyük harf) döndürürken emir
uçları `stopmarket` / `stoplimit` (alt çizgisiz, küçük harf) döndürür. Enum eşlemesi
büyük/küçük harfe duyarsızdır ama alt çizgi farkını çözemez; bu nedenle her iki biçim de
`[Map]` içinde tanımlanmıştır. Tek biçim eşleştirilseydi stop emirleri tanınmazdı.

---

## E.6 Dördüncü Tur Bulguları (kline + kullanıcı işlem geçmişi)

### D-19 — Kline ucu ayrı host ve ayrı format kullanır

`GET https://graph-api.btcturk.com/v1/klines/history` — `api.btcturk.com` değil.

Bu uç, projedeki ortak kalıbın **iki** varsayımını birden kırar:

1. **Tek taban adres yeterli değil.** Ortama ayrı bir `GraphBaseAddress` eklendi.
2. **Standart zarf yok.** `success`/`message`/`code`/`data` alanları bulunmaz; zarfı açan
   `SendAsync<T>` bu uçta çalışmaz. Ham yanıt için ayrı bir `SendRawAsync<T>` yolu eklendi.

Ayrıca veriler **paralel diziler** halinde gelir (`t`, `o`, `h`, `l`, `c`, `v`) ve zaman
damgaları **saniye** cinsindendir — diğer tüm uçlar milisaniye kullanır.

Dizi uzunlukları birbirini tutmadığında hangi değerin hangi muma ait olduğu belirsizdir;
eksik veriyi tahmin etmek yerine `InvalidOperationException` fırlatılır.

Canlı yanıtta, resmi dokümantasyonda geçmeyen bir `s` (durum) alanı bulunur.

### D-20 — Kullanıcı işlem geçmişinde tutarlar işaretlidir

`amount`, `fee` ve `tax` satış işlemlerinde **negatif** gelir; işaret varlığın hesaptan
çıktığını belirtir. Mutlak değer bekleyen bir hesaplama (toplam hacim, komisyon toplamı)
işareti yok sayarsa sonuç sessizce yanlış çıkar.

Native modelde işaret korunur. Borsadan bağımsız modele geçerken mutlaka mutlak değere
çevrilir, çünkü yön zaten `Side` alanında taşınır.

### D-21 — `tax` alanının shared karşılığı yok

BtcTurk her işlem için ayrı bir vergi/kesinti tutarı bildirir. Bu alan Türkiye'ye özgüdür;
`SharedUserTrade` modelinde yalnızca `Fee` bulunur ve vergi orada temsil edilemez.

Komisyona eklemek toplamı bozar, atmak bilgi kaybettirir. Seçilen yol: native modelde
korunur, shared yüzeyde yalnızca komisyon aktarılır ve bu sınır belgelenir. Vergi hesabı
yapan tüketiciler native API kullanmalıdır.

### D-22 — `orderId` filtresi diğer filtrelerle birleştirilemez

İşlem geçmişi ucunda `orderId`, diğer parametrelerle birlikte kullanılamaz. Sessizce yok
saymak yanlış sonuç döndürürdü; birlikte verildiklerinde `ArgumentException` fırlatılır.
