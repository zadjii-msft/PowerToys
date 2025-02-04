# The spec is next to this script:
$pathToSpec = Join-Path (Split-Path -parent $MyInvocation.MyCommand.Definition) initial-sdk-spec.md
# First, use the mistletoe library to parse the markdown file to json
$jsonText = mistletoe $pathToSpec --renderer mistletoe.ast_renderer.AstRenderer
$json =  $jsonText | ConvertFrom-Json

$sdkContents = ""
foreach ($item in $json.children) {
    if ($item.type -eq 'CodeFence') {
        # we only care about code fences with language 'csharp'
        if ($item.language -eq 'c#') {
            $code = $item.children.content
            # Each line that starts with `runtimeclass` or `interface` should be prefixed with the contract attribute
            $code = $code -replace "(?m)^(runtimeclass|interface) ", "[contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]`n`$1 "

            # if there are two [contract] attributes, remove the second one
            $code = $code -replace "(?m)^\[contract\(Microsoft.CommandPalette.Extensions.ExtensionsContract, ([0-9]+)\)\]\n\[contract\(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1\)\]", "[contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, `$1)]"


            # all the lines that start with `(whitespace)async (T)` should be translated to `IAsyncOperation<T>`
            $code = $code -replace "(?m)^(\s*)async\s+(void)\s+([A-Za-z0-9_]+)\s*\(", "`$1Windows.Foundation.IAsyncAction `$3("
            $code = $code -replace "(?m)^(\s*)async\s+([A-Za-z0-9_<>]+)\s+([A-Za-z0-9_]+)\s*\(", "`$1Windows.Foundation.IAsyncOperation<`$2> `$3("
            $code = $code -replace ">>", "> >"

            # Add four spaces to each line of the code block
            $code = $code -replace "(?m)^", "    "

            $sdkContents += $code + "`n"
        }
    }
}
foreach ($item in $json.children) {
    if ($item.type -eq 'CodeFence') {
        # we only care about code fences with language 'csharp'
        if ($item.language -eq 'csharp') {
            $code = $item.children.content
            # Each line that starts with `runtimeclass` or `interface` should be prefixed with the contract attribute
            $code = $code -replace "(?m)^(runtimeclass|interface) ", "[contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]`n`$1 "

            # if there are two [contract] attributes, remove the second one
            $code = $code -replace "(?m)^\[contract\(Microsoft.CommandPalette.Extensions.ExtensionsContract, ([0-9]+)\)\]\n\[contract\(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1\)\]", "[contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, `$1)]"


            # all the lines that start with `(whitespace)async (T)` should be translated to `IAsyncOperation<T>`
            $code = $code -replace "(?m)^(\s*)async\s+(void)\s+([A-Za-z0-9_]+)\s*\(", "`$1Windows.Foundation.IAsyncAction `$3("
            $code = $code -replace "(?m)^(\s*)async\s+([A-Za-z0-9_<>]+)\s+([A-Za-z0-9_]+)\s*\(", "`$1Windows.Foundation.IAsyncOperation<`$2> `$3("
            $code = $code -replace ">>", "> >"

            # Add four spaces to each line of the code block
            $code = $code -replace "(?m)^", "    "

            $sdkContents += $code + "`n"
        }
    }
}

# now, write the fully formatted interface with headers and all:
Write-Output @"
namespace Microsoft.CommandPalette.Extensions
{
    [contractversion(1)]
    apicontract ExtensionsContract {}

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    interface IExtension {
        IInspectable GetProvider(ProviderType providerType);
        void Dispose();
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    enum ProviderType {
        Commands = 0,
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    runtimeclass IconData {
        IconData(String iconString);
        static IconData FromStream(Windows.Storage.Streams.IRandomAccessStreamReference stream);

        String Icon { get; };
        Windows.Storage.Streams.IRandomAccessStreamReference Data { get; };
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    runtimeclass IconInfo {
        IconInfo(String iconString);
        IconInfo(IconData lightIcon, IconData darkIcon);

        IconData Light { get; };
        IconData Dark { get; };
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    runtimeclass KeyChord
    {
        KeyChord();
        KeyChord(Windows.System.VirtualKeyModifiers modifiers, Int32 vkey, Int32 scanCode);
        KeyChord(Boolean ctrl, Boolean alt, Boolean shift, Boolean win, Int32 vkey, Int32 scanCode);
        UInt64 Hash();
        Boolean Equals(KeyChord other);
        Windows.System.VirtualKeyModifiers Modifiers;
        Int32 Vkey;
        Int32 ScanCode;
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    interface INotifyPropChanged {
        event Windows.Foundation.TypedEventHandler<Object, PropChangedEventArgs> PropChanged;
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    runtimeclass PropChangedEventArgs {
        PropChangedEventArgs(String propertyName);
        String PropertyName { get; };
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    interface INotifyItemsChanged {
        event Windows.Foundation.TypedEventHandler<Object, ItemsChangedEventArgs> ItemsChanged;
    };

    [contract(Microsoft.CommandPalette.Extensions.ExtensionsContract, 1)]
    runtimeclass ItemsChangedEventArgs {
        ItemsChangedEventArgs(Int32 totalItems);
        Int32 TotalItems { get; };
    };

$sdkContents
}
"@
