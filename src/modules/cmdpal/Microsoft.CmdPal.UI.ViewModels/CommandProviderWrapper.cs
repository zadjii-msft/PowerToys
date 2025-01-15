﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CmdPal.Common.Services;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.UI.ViewModels.Models;
using Windows.Foundation;

namespace Microsoft.CmdPal.UI.ViewModels;

public sealed class CommandProviderWrapper
{
    public bool IsExtension => extensionWrapper != null;

    private readonly bool isValid;

    private readonly ExtensionObject<ICommandProvider> _commandProvider;

    private readonly IExtensionWrapper? extensionWrapper;

    public ICommandItem[] TopLevelItems { get; private set; } = [];

    public IFallbackCommandItem[] FallbackItems { get; private set; } = [];

    public string DisplayName { get; private set; } = string.Empty;

    public IExtensionWrapper? Extension => extensionWrapper;

    public CommandPaletteHost ExtensionHost { get; private set; }

    public event TypedEventHandler<CommandProviderWrapper, ItemsChangedEventArgs>? CommandsChanged;

    public string Id { get; private set; } = string.Empty;

    public IconInfo Icon { get; private set; } = new(string.Empty);

    public string ProviderId => $"{extensionWrapper?.PackageFamilyName ?? string.Empty}/{Id}";

    public CommandProviderWrapper(ICommandProvider provider)
    {
        // This ctor is only used for in-proc builtin commands. So the Unsafe!
        // calls are pretty dang safe actually.
        _commandProvider = new(provider);

        // Hook the extension back into us
        ExtensionHost = CommandPaletteHost.Instance;
        _commandProvider.Unsafe!.InitializeWithHost(ExtensionHost);

        _commandProvider.Unsafe!.ItemsChanged += CommandProvider_ItemsChanged;

        isValid = true;
        Id = provider.Id;
        DisplayName = provider.DisplayName;
        Icon = provider.Icon;
    }

    public CommandProviderWrapper(IExtensionWrapper extension)
    {
        extensionWrapper = extension;
        ExtensionHost = new CommandPaletteHost(extension);
        if (!extensionWrapper.IsRunning())
        {
            throw new ArgumentException("You forgot to start the extension. This is a coding error - make sure to call StartExtensionAsync");
        }

        var extensionImpl = extension.GetExtensionObject();
        var providerObject = extensionImpl?.GetProvider(ProviderType.Commands);
        if (providerObject is not ICommandProvider provider)
        {
            throw new ArgumentException("extension didn't actually implement ICommandProvider");
        }

        _commandProvider = new(provider);

        try
        {
            var model = _commandProvider.Unsafe!;

            // Hook the extension back into us
            model.InitializeWithHost(ExtensionHost);
            model.ItemsChanged += CommandProvider_ItemsChanged;

            DisplayName = model.DisplayName;

            isValid = true;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to initialize CommandProvider for extension.");
            Debug.WriteLine($"Extension was {extensionWrapper!.PackageFamilyName}");
            Debug.WriteLine(e);
        }

        isValid = true;
    }

    public async Task LoadTopLevelCommands()
    {
        if (!isValid)
        {
            return;
        }

        ICommandItem[]? commands = null;
        IFallbackCommandItem[]? fallbacks = null;

        try
        {
            var model = _commandProvider.Unsafe!;

            var t = new Task<ICommandItem[]>(model.TopLevelCommands);
            t.Start();
            commands = await t.ConfigureAwait(false);

            // On a BG thread here
            fallbacks = model.FallbackCommands();

            Id = model.Id;
            DisplayName = model.DisplayName;
            Icon = model.Icon;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load commands from extension");
            Debug.WriteLine($"Extension was {extensionWrapper!.PackageFamilyName}");
            Debug.WriteLine(e);
        }

        if (commands != null)
        {
            TopLevelItems = commands;
        }

        if (fallbacks != null)
        {
            FallbackItems = fallbacks;
        }
    }

    /* This is a View/ExtensionHost piece
     * public void AllowSetForeground(bool allow)
    {
        if (!IsExtension)
        {
            return;
        }

        var iextn = extensionWrapper?.GetExtensionObject();
        unsafe
        {
            PInvoke.CoAllowSetForegroundWindow(iextn);
        }
    }*/

    public override bool Equals(object? obj) => obj is CommandProviderWrapper wrapper && isValid == wrapper.isValid;

    public override int GetHashCode() => _commandProvider.GetHashCode();

    private void CommandProvider_ItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        // We don't want to handle this ourselves - we want the
        // TopLevelCommandManager to know about this, so they can remove
        // our old commands from their own list.
        //
        // In handling this, a call will be made to `LoadTopLevelCommands` to
        // retrieve the new items.
        this.CommandsChanged?.Invoke(this, args);
    }
}
