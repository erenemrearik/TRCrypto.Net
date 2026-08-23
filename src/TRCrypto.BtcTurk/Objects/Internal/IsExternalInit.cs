#if !NET8_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// netstandard2.0/2.1 hedeflerinde <c>init</c> erisimcisini etkinlestiren polyfill.
/// Derleyici bu tipin varligini arar; calisma zamaninda bir islevi yoktur.
/// Modelleri degistirilemez tutabilmek icin <c>set</c> yerine bu yol secilmistir
/// (spesifikasyon Bolum 5.3).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}

#endif
