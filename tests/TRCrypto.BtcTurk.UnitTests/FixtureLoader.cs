using System.Reflection;

namespace TRCrypto.BtcTurk.UnitTests;

/// <summary>Gomulu fixture JSON dosyalarini okur.</summary>
internal static class FixtureLoader
{
    public static string Load(string name)
    {
        var asm = Assembly.GetExecutingAssembly();
        var resource = asm.GetManifestResourceNames()
            .SingleOrDefault(x => x.EndsWith("Fixtures." + name, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"'{name}' fixture'i bulunamadi. Mevcut: {string.Join(", ", asm.GetManifestResourceNames())}");

        using var stream = asm.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
