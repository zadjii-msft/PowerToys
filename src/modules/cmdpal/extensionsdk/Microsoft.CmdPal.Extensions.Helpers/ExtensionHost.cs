namespace Microsoft.CmdPal.Extensions.Helpers;

public class ExtensionHost
{
    private static IExtensionHost? _host;

    public static IExtensionHost? Host => _host;

    public static void Initialize(IExtensionHost host)
    {
        _host = host;
    }
}