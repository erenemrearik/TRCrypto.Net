using System.Reflection;
using CryptoExchange.Net.SharedApis;
using Xunit;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>
/// Paylasilan istemcinin bildirdigi yeteneklerin uyguladigi arayuzlerle ortustugunu dogrular.
/// </summary>
/// <remarks>
/// <para>
/// CryptoExchange.Net 13 ile birlikte her paylasilan istemci yeteneklerini
/// <see cref="ISharedApi.Capabilities"/> uzerinden bir liste olarak bildirir. Bu liste
/// elle yazilir ve uygulanan arayuzlerden ayrisabilir: yeni bir arayuz uygulanip listeye
/// eklenmezse <c>Discover()</c> o yetenegi hic gostermez, listede olup uygulanmayan bir
/// giris ise dogrulanmamis bir yetenegi ilan eder.
/// </para>
/// <para>
/// Beklenen kume elle yazilmaz; istemcinin gercekten uyguladigi arayuzlerden yansima ile
/// turetilir. Liste ile arayuzler ancak bu sekilde birbirini denetler.
/// </para>
/// </remarks>
internal static class CapabilityAssert
{
    public static void MatchesImplementedInterfaces(ISharedApi client, SharedTransport transport)
    {
        Assert.Equal(transport, client.Transport);

        // 13 surumunde dogrulama, her secenegin desteklenen islem turlerini bilmesine
        // dayanir. Bu bilgi kaydedilmezse liste bos kalir ve her paylasilan cagri
        // "TradingMode.Spot is not supported" ile reddedilir. Birim testleri yapisal
        // oldugu icin bunu gormuyordu; canli dogrulama yakaladi.
        foreach (var options in client.Capabilities)
        {
            Assert.True(options.SupportedTradingModes.Contains(TradingMode.Spot),
                $"{options.GetType().Name} spot islem turunu desteklemiyor gorunuyor; " +
                "secenek CryptoExchange.Net'e kaydedilmemis.");
        }

        var declared = new HashSet<object>(client.Capabilities, ReferenceEqualityComparer.Instance);
        var expected = new HashSet<object>(ReferenceEqualityComparer.Instance);

        var sharedInterfaces = client.GetType()
            .GetInterfaces()
            .Where(i => i.Namespace == typeof(ISharedApi).Namespace);

        foreach (var iface in sharedInterfaces)
        {
            foreach (var property in iface.GetProperties()
                         .Where(p => typeof(CapabilityOptions).IsAssignableFrom(p.PropertyType)))
            {
                // Arayuz uzerinden okunan ozellik, acik (explicit) uygulamayi da cagirir.
                if (property.GetValue(client) is { } options)
                    expected.Add(options);
            }
        }

        var missing = expected.Where(x => !declared.Contains(x)).Select(Name).ToList();
        var extra = declared.Where(x => !expected.Contains(x)).Select(Name).ToList();

        Assert.True(missing.Count == 0,
            "Uygulanan ama yetenek listesinde olmayan: " + string.Join(", ", missing));
        Assert.True(extra.Count == 0,
            "Yetenek listesinde olan ama uygulanmayan: " + string.Join(", ", extra));
    }

    private static string Name(object options) => options.GetType().Name;
}
