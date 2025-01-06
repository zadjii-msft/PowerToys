$gitRoot = git rev-parse --show-toplevel

# $xamlsForStyler = (git ls-files "$gitRoot/**/*.xaml") -join ","
$xamlsForStyler = (git ls-files "$gitRoot/src/modules/cmdpal/**/*.xaml") -join ","
dotnet tool run xstyler -- -c "$gitRoot\src\Settings.XamlStyler" -f "$xamlsForStyler"