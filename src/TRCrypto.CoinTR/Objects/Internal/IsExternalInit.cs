#if !NET8_0_OR_GREATER

using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// netstandard2.0/2.1 hedeflerinde <c>init</c> erisimcisini etkinlestiren polyfill.
/// Modelleri degistirilemez tutabilmek icin gereklidir.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}

#endif
