using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using TRCrypto.BtcTurk.Objects.Internal;

namespace TRCrypto.BtcTurk.Clients.MessageHandlers;

/// <summary>
/// BtcTurk yanitlarini ayristirir ve zarf icindeki is mantigi hatalarini yuzeye cikarir.
/// </summary>
/// <remarks>
/// BtcTurk basarisiz islemleri HTTP 200 icinde <c>"success": false</c> olarak dondurur.
/// Bu tur bir yanit basarili sayilirsa cagiran taraf sessizce bos/varsayilan veri isler;
/// bu nedenle zarf her yanitta kontrol edilir (spesifikasyon Bolum 10.5).
/// </remarks>
internal class BtcTurkRestMessageHandler : JsonRestMessageHandler
{
    private readonly ErrorMapping _errorMapping;

    public override JsonSerializerOptions Options { get; } = BtcTurkJsonOptions.Default;

    public BtcTurkRestMessageHandler(ErrorMapping errorMapping)
    {
        _errorMapping = errorMapping;
    }

    /// <summary>HTTP 200 donen ancak zarfinda hata bildiren yanitlari yakalar.</summary>
    public override Error? CheckDeserializedResponse<T>(HttpResponseHeaders responseHeaders, T result)
    {
        if (result is not IBtcTurkResponse response || response.Success)
            return null;

        // Bilinmeyen kodlar da dahil olmak uzere ham kod/mesaj korunur.
        var code = response.Code ?? "UNKNOWN";
        return new ServerError(code, _errorMapping.GetErrorInfo(code, response.Message));
    }

    /// <summary>HTTP hata durum kodlariyla donen yanitlari ayristirir.</summary>
    public override async ValueTask<Error> ParseErrorResponse(
        int httpStatusCode,
        HttpResponseHeaders responseHeaders,
        Stream responseStream)
    {
        var (parseError, document) = await GetJsonDocument(responseStream).ConfigureAwait(false);
        if (parseError != null)
            return parseError;

        var root = document!.RootElement;
        var message = root.TryGetProperty("message", out var messageProperty)
            ? messageProperty.GetString()
            : null;

        if (!root.TryGetProperty("code", out var codeProperty))
            return new ServerError(ErrorInfo.Unknown);

        var code = codeProperty.ValueKind == JsonValueKind.Number
            ? codeProperty.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture)
            : codeProperty.GetString();

        if (string.IsNullOrEmpty(code))
            return new ServerError(ErrorInfo.Unknown);

        return new ServerError(code!, _errorMapping.GetErrorInfo(code!, message));
    }
}
