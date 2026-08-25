using System.Globalization;
using System.Text.Json;

namespace TRCrypto.BtcTurk.Converters;

/// <summary>
/// BtcTurk'un ondalik degerlerini okur.
/// </summary>
/// <remarks>
/// <para>
/// Ayirici uclar arasinda degisir: piyasa verisi ve emir uclari nokta kullanirken
/// (<c>"0.00269390"</c>), bakiye ucu virgul kullanir (<c>"27223,7283250757643288"</c>).
/// </para>
/// <para>
/// Virgul her zaman ondalik ayiricidir; BtcTurk binlik ayirici kullanmaz (buyuk sayilar
/// <c>"3708000"</c> bicimindedir). Bu nedenle iki yorum arasinda belirsizlik yoktur.
/// </para>
/// <para>
/// Deger her zaman <see cref="decimal"/> uzerinden okunur; <c>double</c> araya girmedigi
/// icin hassasiyet kaybi olmaz.
/// </para>
/// </remarks>
public class BtcTurkDecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ReadValue(ref reader) ?? 0m;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));

    internal static decimal? ReadValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.Number:
                return reader.GetDecimal();

            case JsonTokenType.String:
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                // Virgul yalnizca ondalik ayirici olabilir (bkz. sinif aciklamasi).
                var normalized = text!.Replace(',', '.');

                return decimal.Parse(
                    normalized,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture);

            default:
                throw new JsonException(
                    $"Ondalik deger sayi ya da metin olmalidir; gelen: {reader.TokenType}.");
        }
    }
}

/// <summary>
/// <see cref="BtcTurkDecimalConverter"/> ile ayni kurallari uygulayan, <c>null</c>
/// kabul eden surum.
/// </summary>
public class BtcTurkNullableDecimalConverter : JsonConverter<decimal?>
{
    /// <inheritdoc />
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => BtcTurkDecimalConverter.ReadValue(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
    }
}
