using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;
using TRCrypto.CoinTR.Objects.Internal;

namespace TRCrypto.CoinTR.Clients.MessageHandlers;

/// <summary>
/// CoinTR yanitlarini ayristirir ve zarf icindeki hatalari yuzeye cikarir.
/// </summary>
/// <remarks>
/// Borsa basarisiz islemleri HTTP 200 icinde <c>"00000"</c> disinda bir <c>code</c> ile
/// bildirir. Kod bir metindir; sayi olarak okumak ayristirma hatasi verir.
/// Bu kontrol yapilmazsa cagiran taraf bos veriyi basarili sanir.
/// </remarks>
internal class CoinTRRestMessageHandler : JsonRestMessageHandler
{
    private readonly ErrorMapping _errorMapping;

    public override JsonSerializerOptions Options { get; } = CoinTRJsonOptions.Default;

    public CoinTRRestMessageHandler(ErrorMapping errorMapping)
    {
        _errorMapping = errorMapping;
    }

    /// <inheritdoc />
    public override Error? CheckDeserializedResponse<T>(HttpResponseHeaders responseHeaders, T result)
    {
        if (result is not ICoinTRResponse response || response.Success)
            return null;

        var code = response.Code ?? string.Empty;
        return new ServerError(code, _errorMapping.GetErrorInfo(code, response.Message));
    }

    /// <inheritdoc />
    public override async ValueTask<Error> ParseErrorResponse(
        int httpStatusCode,
        HttpResponseHeaders responseHeaders,
        Stream responseStream)
    {
        var (parseError, document) = await GetJsonDocument(responseStream).ConfigureAwait(false);
        if (parseError != null)
            return parseError;

        var root = document!.RootElement;
        var message = root.TryGetProperty("msg", out var messageProperty)
            ? messageProperty.GetString()
            : null;

        if (!root.TryGetProperty("code", out var codeProperty))
            return new ServerError(ErrorInfo.Unknown);

        var code = codeProperty.ValueKind == JsonValueKind.Number
            ? codeProperty.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture)
            : codeProperty.GetString();

        return string.IsNullOrEmpty(code)
            ? new ServerError(ErrorInfo.Unknown)
            : new ServerError(code!, _errorMapping.GetErrorInfo(code!, message));
    }
}
