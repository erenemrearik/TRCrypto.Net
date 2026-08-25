using System.Text.Json;
using TRCrypto.BtcTurk.Enums;

namespace TRCrypto.BtcTurk.Converters;

/// <summary>
/// WebSocket akisindaki sayisal islem yonu kodunu <see cref="OrderSide"/> degerine cevirir.
/// </summary>
/// <remarks>
/// <para>
/// REST uclari yonu <c>"buy"</c> / <c>"sell"</c> metniyle verirken WebSocket akisi sayisal
/// bir kod kullanir. Bu kodun anlami resmi dokumantasyonda belirtilmemistir.
/// </para>
/// <para>
/// Esleme, canli akistan gelen islem kimliklerinin REST islem ucundaki karsiliklariyla
/// karsilastirilmasiyla belirlenmistir (26 Agustos 2026): <c>0</c> satis, <c>1</c> alistir.
/// Borsa bu kodlari degistirirse sonuc sessizce yanlis yon uretir; bu nedenle esleme
/// birim testiyle sabitlenmistir.
/// </para>
/// </remarks>
public class BtcTurkSocketOrderSideConverter : JsonConverter<OrderSide>
{
    private const int SellCode = 0;
    private const int BuyCode = 1;

    /// <inheritdoc />
    public override OrderSide Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var code = reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => int.TryParse(reader.GetString(), out var parsed) ? parsed : -1,
            _ => -1
        };

        return code switch
        {
            SellCode => OrderSide.Sell,
            BuyCode => OrderSide.Buy,
            // Tanimsiz bir kod ayristirmayi dusurmez; tanimli olmayan enum degeri
            // Enum.IsDefined ile tespit edilebilir.
            _ => (OrderSide)(-9)
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OrderSide value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value == OrderSide.Buy ? BuyCode : SellCode);
}
