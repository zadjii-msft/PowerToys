---
author: Mike Griese
created on: 2024-07-19
last updated: 2024-09-06
issue id: n/a
---

# Run v2 Extensions SDK

_aka "DevPal", "PT Run v2", "DevSearch", "Windows Command Palette", this thing has many names. I'll use DevPal throughout the doc_

> [NOTE!]
> Are you here to just see what the SDK looks like? Skip to the [Actions
> SDK details](#commands-sdk-details) section.

## Abstract

"DevPal" is "PowerToys Run v2" - the graduated form of PowerToys Run, ready for
ingestion as a Windows inbox app. DevPal enables the user to launch more than
just apps and search for files - it's a highly customizable surface that allows
users to "Start _anything_".

Most importantly, DevPal is highly extensible. By exposing a simple-to-use SDK,
DevPal enables 3p developers to quickly create plugins and extend it
functionality.

- [Run v2 Extensions SDK](#run-v2-extensions-sdk)
  - [Abstract](#abstract)
  - [Background](#background)
    - [Inspiration](#inspiration)
    - [User Stories](#user-stories)
    - [Elevator Pitch](#elevator-pitch)
  - [Business Justification](#business-justification)
  - [Scenario Details](#scenario-details)
    - [Extension basics](#extension-basics)
    - [Installed extension discovery](#installed-extension-discovery)
      - [Unpackaged extensions](#unpackaged-extensions)
    - [Extension lifecycle](#extension-lifecycle)
      - [Startup](#startup)
      - [Caching](#caching)
      - [Disposing](#disposing)
  - [Installing extensions](#installing-extensions)
    - [From the Store](#from-the-store)
    - [From winget](#from-winget)
  - [Publishing extensions](#publishing-extensions)
  - [Built-in commands](#built-in-commands)
  - [SDK overview](#sdk-overview)
  - [Commands SDK details](#commands-sdk-details)
    - [Commands](#commands)
      - [Results](#results)
    - [Pages](#pages)
      - [List Pages](#list-pages)
        - [Updating the list](#updating-the-list)
        - [Filtering the list](#filtering-the-list)
      - [Fallback actions](#fallback-actions)
      - [Markdown Pages](#markdown-pages)
      - [Form Pages](#form-pages)
    - [Other types](#other-types)
      - [`ContextItem`s](#contextitems)
      - [`IconDataType`](#icondatatype)
      - [`Details`](#details)
      - [`INotifyPropChanged`](#inotifypropchanged)
      - [`ICommandProvider`](#icommandprovider)
    - [Settings](#settings)
  - [Helper SDK Classes](#helper-sdk-classes)
    - [Default implementations](#default-implementations)
    - [Using the Clipboard](#using-the-clipboard)
  - [Advanced scenarios](#advanced-scenarios)
    - [Status messages](#status-messages)
  - [Class diagram](#class-diagram)
  - [Future considerations](#future-considerations)
    - [Arbitrary parameters and arguments](#arbitrary-parameters-and-arguments)
    - [URI activation](#uri-activation)
    - [Custom "empty list" messages](#custom-empty-list-messages)
  - [Footnotes](#footnotes)


## Background

> [!NOTE]
> This is the spec specifically for the devpal SDK. For a more general overview of devpal, see the [this functional spec](https://microsoft.sharepoint-df.com/:w:/r/teams/windows/_layouts/15/Doc.aspx?sourcedoc=%7BB2D5260A-C01C-4F9E-BD58-0D69FC75FE0D%7D&file=Developer%20Search%20Functional%20Spec%20v2.docx&action=default&mobileredirect=true) (internal only).

### Inspiration

The largest inspiration for this extension SDK is the [Dev Home Extension]
model. They are the ones who pioneered the plumbing for registering COM classes
in the extension manifest, then using `CoCreateInstance` to create objects in
the host and use them as WinRT objects.

### User Stories

_(typically there'd be a long list of user stories here, but that felt better
suited for a more general DevPal dev spec, rather than the SDK doc)_

### Elevator Pitch

> "Start _anything_ here".

What if the Start Menu was more than just a launcher for apps? What if it could
be the start for all sorts of different workflows? One that apps could plug into
directly, and provide dedicated experiences for their users.

## Business Justification

It will delight ~developers~ all power users.

## Scenario Details

> [!NOTE]
>
> This document is largely concerned with the details for how 3p apps could plug
> into DevPal. However, much of the built-in devpal functionality will be built
> using the same interfaces. This will make sure that everything that we build
> keeps 3p use cases in mind. Built-in experiences, however, can be loaded
> in-proc, so they can skip pretty much all of this doc up till "[SDK
> overview](#sdk-overview)".

### Extension basics

In the simplest case, extensions for Dev Pal can register themselves using their `.appxmanifest`. As an example:

```xml
<Extensions>
    <com:Extension Category="windows.comServer">
        <com:ComServer>
            <com:ExeServer Executable="ExtensionName.exe" Arguments="-RegisterProcessAsComServer" DisplayName="Sample Extension">
                <com:Class Id="<Extension CLSID Here>" DisplayName="Sample Extension" />
            </com:ExeServer>
        </com:ComServer>
    </com:Extension>
    <uap3:Extension Category="windows.appExtension">
        <uap3:AppExtension Name="com.microsoft.windows.commandpalette"
                           Id="YourApplicationUniqueId"
                           PublicFolder="Public"
                           DisplayName="Sample Extension"
                           Description="Sample Extension for Run">
            <uap3:Properties>
                <CmdPalProvider>
                    <Activation>
                        <CreateInstance ClassId="<Extension CLSID Here>" />
                    </Activation>
                    <SupportedInterfaces>
                        <Actions />
                    </SupportedInterfaces>
                </CmdPalProvider>
            </uap3:Properties>
        </uap3:AppExtension>
    </uap3:Extension>
</Extensions>
```

Notable elements:
* The application must specify a `Extensions.comExtension.ComServer` to host
  their COM class. This allows for the OS to register that GUID as a COM class
  we can instantiate.
  * Make sure that this CLSID is unique, and matches the one in your application
* The application must specify a `Extensions.uap3Extension.AppExtension` with
  the Name set to `com.microsoft.windows.commandpalette`. This is the unique identifier which
  DevPal can use to find it's extensions.
* In the `Properties` of your `AppExtension`, you must specify a
  `CmdPalProvider` element. This is where you specify the CLSID of the COM class
  that DevPal will instantiate to interact with your extension. Also, you
  specify which interfaces you support.

Currently, only `Actions` is supported. If we need to add more
in the future, we can add them to the `SupportedInterfaces` element.

This is all exactly the same as the Dev Home Extension model, with a different
`Name` in the `AppExtension` element, and different `SupportedInterfaces`.

### Installed extension discovery

Fortunately for DevPal, it is quite trivial to enumerate installed packages that
have registered themselves as a `AppExtension` extensions. This is done by
querying the `AppExtensionCatalog` for all extensions with the `Name` set to
`com.microsoft.windows.commandpalette`.

#### Unpackaged extensions

[Sparse packages](https://nicksnettravels.builttoroam.com/sparse-package/) are
always a simple solution for adding package identity to otherwise unpackaged
applications. However, there may be apps out there that (for whatever reason)
still don't have a package identity. We need a way to allow these apps to
register themselves as extensions.

We can't just ask the COM catalog for all CLSIDs that implement the a particular
COM interface, unfortunately. This means we'll need another well-known location
in the registry for unpackaged apps to write their extension CLSID's into.

We'll create a registry key at
`HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DevPal\Extensions` [TODO!api-review]: not this path. I think that's owned by the OS
with a subkey for each extension. The subkey should be the `Name` of the
extension, and the default value should be the CLSID of the COM class that
implements the extension. On startup, DevPal will enumerate these keys in
addition to the packaged ones. Apps should not write into both locations, unless
they have two separate extension classes to load.

On uninstall, apps should remove themselves from that registry key.

### Extension lifecycle

#### Startup

When an extension is installed, DevPal will find it in the
`AppExtensionCatalog`, and parse the `AppExtension.Properties`. It will then
instantiate the COM class specified in the `CreateInstance` element. It is
ultimately up to the extension app how it wants to serve that COM class. In the
simplest case (above), COM will create the `ExtensionName.exe` process to serve
the COM object.

A more elaborate use case would have an existing process that the COM object is
created in. This is useful for extensions that need to deeply tie into a running
application. One could imagine a Windows Terminal extension that produces a list
of all the open windows, tabs and panes. This extension would be best served by
the Terminal process itself, as it has all the context it needs to produce that
list.

When DevPal launches, it will enumerate all the extensions it knows about, and
create the `IExtension` object for each one. DevPal will then get the
`ICommandProvider` for apps that register as `Actions` providers in
`SupportedInterfaces`. Extension apps should have that COM object served
quickly, for performance. That is the first object that DevPal needs, to load
the top-level list of actions.

These commands will be loaded asynchronously, and the UI will be updated as they
are loaded on a cold launch. Subsequent launches will have devpal already
running in the background.

Each individual page will be loaded as the user navigates to it. This allows for
apps to lazily load their "UI content" as needed.

#### Caching

> [!IMPORTANT]
> this section is a draft, TODO!


* We will cache the list of commands for each extension. This allows us to
  quickly load the list of commands on subsequent cold launches.
* We should have a way to let extensions opt into more agressive caching of
  their actions. So something that has a static list of top-level actions
  doesn't even need to be `CoCreateInstance`'d on launch.
  * The "Hacker News" extension, for example, only has a single top-level
    action. Once we load that once, we don't need to CreateProcess just to find
    it. We could just CoCreateInstance when the user selects it, and immediately
    load the page.
  * The "Quick Links" extension has a dynamic list of top-level actions. These
    aren't something that can be added to the appxmanifest at packaging time.
    But once we have them, we can cache them.
  * The "Media Controls" extension only has a single top-level action, but it
    needs to be running to be able to update it's title and icon. So we can't
    just cache the state of it.
  * Similarly, any top-level `IFallbackAction` need to be running to get
    real-time updates to their name.

Here are some TODO!naming bad names for the caching levels:

* "Frozen" (**default**) - After the first launch, cache the list of top-level
  actions. When we're launched, don't create the COM object, just use the cached
  list of actions. Most apps which provide a static list of top-level actions
  can use this mode.
  * For example: any app that just provides a single top level action, like the
    Hacker News extension.
* "Microwavable" - After the first launch, cache the list of top-level actions.
  When we're launched, still create the COM object, and update that list of
  commands as needed. Useful for things who's list of commands may change with
  some frequency.
  * an example of this is the "Quick Links" extension. It needs to be running to
    update the list of actions, but the list of commands doesn't change that
    often.
* "Fresh, never frozen" - always create a new instance of the COM object and
  query the list of actions. Never cache the results from the last launch. This
  is for extensions that want to provide real-time info in the top level, or
  who's list of commands changes frequently.
  * an example of this is the "Media Controls" extension. It needs to be running
    to update the title and icon for the playing music. It would never make
    sense to use the previous value.

#### Disposing

> [!IMPORTANT]
> this section is a draft, TODO!

* We shouldn't have all the extensions loaded if we don't need them.
* When we're dismissed at the home page, we should close out the extension classes we've loaded.
* Should we close out the reference to the package entirely? So that the store has a chance to update it?
* Should FreshNeverFrozen extensions be left open even while dismissed?

## Installing extensions

### From the Store

These are fairly straightforward. The user goes to the Store, finds an extension
app, and installs it. When they do that, the PackageCatalog will send us an
event which we can use to update our list of extensions.

At the time of writing, there's not an easy way to query the store for a list of
apps who's manifest specifies that they are an extension. We could launch
something like:

```
ms-windows-store://assoc/?Tags=AppExtension-com.microsoft.windows.commandpalette
```

to open the store to a list of extensions. However, we can't list those
ourselves directly. Our friends in DevHome suggested it could be possible to
stand up a azure service which could query the store for us, and return a list
of extensions. This is not something that they currently have planned, nor would
it be cheap from an engineering standpoint.

### From winget

Winget on the other hand, does allow packages to specify arbitrary tags, and let
apps query them easily. We can use that as a system to load a list of packages
available via winget directly in DevPal. We'll specify a well-known tag that
developers can use in their winget package manifest to specify that their
package is an extension. We will then query winget for all packages with that
tag, to expose a list of possible extensions.

## Publishing extensions

As a part of DevPal, we'll ship a "Sample Project" template. We will use this to
be able to quickly generate a project that can be used to create a new
extension. This will include the `sln`, `csproj`, and `appxmanifest` files that
are needed to create a new extension, as well as plumbing to get it all ready.
All the developer will need to do is open up the project to the
`MyCommandProvider` class and start implementing their commands.

As a part of this project template, we'll include a `winget` GitHub Actions
workflow that will allow the developer to push their extension to winget with
the needed tags to be discoverable by DevPal. That way, developers won't need to
worry about the details of packaging, they can just push to main and let the
pipeline take care of the rest.

## Built-in commands

Certain commands are "built-in" to DevPal. These are commands that ship directly
in the DevPal package. These don't need to be loaded as out-of-proc COM
extensions like the third-party ones. However, they will still conform to the
same interface as third-party commands. This allows us to have a consistent
experience for the user. It also cements the ability for 3p extensions to do
anything that a 1p built-in can do.

## SDK overview

The SDK for DevPal is split into two namespaces:
* `Microsoft.Windows.Run` - This namespace contains the interfaces that
  developers will implement to create extensions for DevPal.
* `Microsoft.Windows.Run.Extensions` - This namespace contains helper classes
  that developers can use to make creating extensions easier.

The first is highly abstract, and gives developers total control over the
implementation of their extension. The second contains a variety of default
implementations and helper classes that developers can use to make authoring
extensions simpler.

## Commands SDK details

Below details the SDK that developers can use to create extensions for the
DevPal. These interfaces are exposed through the `Microsoft.Windows.Run`
namespace. We'll expose an SDK with helper classes and default implementations
in the `Microsoft.CmdPal.Extensions` namespace.

> [NOTE!]
>
> In the following SDK details, `csharp` & `c#` code fences to show snippets of
> what the `Microsoft.Windows.Run` interface will look like. This is roughly
> `midl` v3 in this spec, with one modification. I'm using the made up `async`
> keyword to indicate that a method is async. In the real `.idl`, these methods
> will be replaced with `IAsyncAction` for `async void` and `IAsyncOperation<T>`
> for `async T`.
>
> `cs` code fences will be used for samples of what an extension implementaions
> may look like.

### Commands

Commands are the primary unit of functionality in the DevPal SDK. They represent
"a thing that a user can do". These can be something simple like open a URL in a
web browser. Or they can be more complex, with nested commands, custom arguments,
and more.

<!-- Note to editors:
Anything in `csharp` or `c#` code fences will be pulled into the .idl.
Use `cs` for samples.
"c#" blocks will get placed before "csharp" ones. -->

```c#
interface ICommand requires INotifyPropChanged{
    String Name{ get; };
    IconDataType Icon{ get; };
}

enum CommandResultKind {
    Dismiss,    // Reset the palette to the main page and dismiss
    GoHome,     // Go back to the main page, but keep it open
    KeepOpen,   // Do nothing.
    GoToPage,   // Go to another page. GoToPageArgs will tell you where.
};
[uuid("f9d6423b-bd5e-44bb-a204-2f5c77a72396")]
interface ICommandResultArgs{};
interface ICommandResult {
    CommandResultKind Kind { get; };
    ICommandResultArgs Args { get; };
}
interface IGoToPageArgs requires ICommandResultArgs{
    String PageId { get; };
}

// This is a "leaf" of the UI. This is something that can be "done" by the user.
// * A ListPage
// * the MoreCommands flyout of for a ListItem or a MarkdownPage
interface IInvokableCommand requires ICommand {
    ICommandResult Invoke();
}
```

If a developer wants to add a simple action to DevPal, they can create a
class that implements `ICommand`, and implement the `Invoke` method. This
method will be called when the user selects the action in DevPal.

As a simple example[^1]:

```cs
class HackerNewsAction : Microsoft.Windows.Run.Extensions.InvokableCommand {
    public string Name => "Hacker News";
    public IconDataType Icon => "https://news.ycombinator.com/favicon.ico";

    public ActionResult Invoke() {
        Process.Start(new ProcessStartInfo("https://news.ycombinator.com/") { UseShellExecute = true });
        return ActionResult.Success;
    }
}
```

This will create a single action in DevPal that, when selected, will open
Hacker News in the user's default web browser.

Actions can also be `Page`s, which represent additional "nested" pages within
DevPal. When the user selects an action that implements `IPage`, DevPal will
navigate to a page for that action, rather than calling `Invoke` on it. Skip
ahead to [Pages](#Pages) for more information on the different types of pages.

#### Results

Actions can return a `ActionResult` to indicate what DevPal should do after
the command is executed. This allows for commands to control the flow of the
DevPal. For example, an action that opens a URL might return `Kind =
ActionResult.Dismiss` to close DevPal after the URL is opened.

Use cases for each `CommandResultKind`:

* `Dismiss` - Close DevPal after the action is executed. All current state
  is dismissed as well. On the next launch, DevPal will start from the main
  page with a blank query.
  * Ex: An action that opens an application. The Puser doesn't need DevPal
    open after the application is opened, nor do they need the query they used
    to find the action.
* `GoHome` - Navigate back to the main page of DevPal, but keep it open.
  This clears out the current stack of pages, but keeps DevPal open.
  * Note: if the action navigates to another application, DevPal's default
    behavior is to hide itself when it loses focus. That will behave the same as
    `Dismiss` in that case.
  * Ex: The "Add Quick Link" command is a form. After submitting the form, the
    user should be taken back to the main page, with the query cleared, leaving
    the window open.
* `KeepOpen` - Do nothing. This leaves the palette in its current state, with the
  current page stack and query.
  * Note: if the action navigates to another application, DevPal's default
    behavior is to hide itself when it loses focus. When the user next activates
    DevPal, it will be in the same state as when it was hidden.
  * Ex: An action that opens a URL in a new tab, like the Hacker News sample.
    The user might want to open multiple URLs in a row, so DevPal should
    stay in its current state.
* `GoToPage` - Navigate to a different page in DevPal. The `GoToPageArgs`
  will specify which page to navigate to.
  * [TODO!]: Do we actually need this, now that all the commands can be pages?
    * Does this satisfy "I want to pop the stack, but then push something else
      onto the stack"? Versus the default which is just "add this to the stack"?

### Pages

Pages represent individual views in the application. They are the primary unit
of navigation and interaction. Developers can author `Action`s as a page to
provide an additional page of functionality within the application. They are not
wholly responsible for their own rendering. Rather, they provide bits of
information that the host application will then use to render the page.

```csharp
interface IPage requires ICommand {
    String Title { get; };
    Boolean Loading { get; };
}
```

When a user selects an action that implements `IPage`, DevPal will navigate
to that page, pushing it onto the UI stack.

Pages can be one of several types, each detailed below:
* [List](#List)
* [Markdown](#Markdown)
* [Form](#Form)

If a page returns a null or empty `Title`, DevPal will display the `Name` of the
`ICommand` instead.

Pages have a `Loading` property which they can use to indicate to DevPal that
the content is still loading. When `Loading` is `true`, DevPal will show an
intederminate loading bar to the user. When `Loading` is `false`, DevPal will
hide the progress bar. This allows extensions which are displaying asynchronous
content to indicate that something is happening in the background.

Pages are `ICommands`, which means they also are observable via the
`INotifyPropChanged` interface. This allows the page to set `Loading` as needed
and change the value once the results are loaded.

#### List Pages

Lists are the most common type of page. They represent a collection of items
which the user can quickly filter and search through.

![](./list-extension-mock-combined.png)

Lists can be either "static" or "dynamic":
* A **static** list leaves devpal in charge of filtering the list of items,
  based on the query the user typed.
  * These are implementations of the default `IListPage`.
  * In this case, DevPal will use a fuzzy string match over the `Name` of the
    action, the `Subtitle`, and any `Text` on the `Tag`s.
* A **dynamic** list leaves the extension in charge of filtering the list of
  items.
  * These are implementations of the `IDynamicListPage` interface.
  * In this case, `GetItems` will be called every time the query
    changes, and the extension is responsible for filtering the list of items.
    * Ex: The GitHub extension may want to allow the user to type `is:issue
      is:open`, then return a list of open issues, without string matching on
      the text.

```csharp
interface IFallbackHandler {
    void UpdateQuery(String query);
}

[uuid("c78b9851-e76b-43ee-8f76-da5ba14e69a4")]
interface IContextItem {}

interface ICommandContextItem requires IContextItem {
    ICommand Command { get; };
    String Tooltip { get; };
    Boolean IsCritical { get; }; // todo: better name for "make this red"

    // TODO-future: we should allow app developers to specify a default keybinding for each of these actions
}
[uuid("924a87fc-32fe-4471-9156-84b3b30275a6")]
interface ISeparatorContextItem requires IContextItem {}

interface IListItem requires INotifyPropChanged {
    IconDataType Icon{ get; };
    String Title{ get; };
    String Subtitle{ get; };
    ICommand Command{ get; };
    IContextItem[] MoreCommands{ get; };
    ITag[] Tags{ get; };
    IDetails Details{ get; };
    IFallbackHandler FallbackHandler{ get; };
}

interface ISection {
    String Title { get; };
    IListItem[] Items { get; };
}

interface IGridProperties  {
    Windows.Foundation.Size TileSize { get; };
}

interface IListPage requires IPage {
    String SearchText { get; };
    String PlaceholderText { get; };
    Boolean ShowDetails{ get; };
    IFilters Filters { get; };
    IGridProperties GridProperties { get; };

    ISection[] GetItems(); // DevPal will be responsible for filtering the list of items
}

interface IDynamicListPage requires IListPage {
    ISection[] GetItems(String query); // DevPal will do no filtering of these items
}
```

![A mockup of individual elements of a list page and the list items](./list-elements-mock.png)

Lists are comprised of a collection of `Section`s, each with filled with
`ListItems`s as items. Sections may have a title, though they are not required
to. Sections are displayed to the user in the order they are returned by the
extension. Many extensions will only have a single section, but if developers
want to have lots of grouped results, they're free to have as many sections as
they like.
* For example: An "Agenda" extension may want to have one section for each day,
  with each section's items containing the events for the day.

![Another mockup of the elements of a list item](./list-elements-mock-002.png)

Each ListItem has one default `Command`. This is the command that will be run
when the user selects the item. If the IListItem has a non-null `Icon`, that
icon will be displayed in the list. If the `Icon` is null, DevPal will display
the `Icon` of the list item's `Command` instead.

 ListItems may also have a list of `MoreCommands`.
These are additional commands that the user can take on the item. These will be
displayed to the user in the "More commands" flyout when the user has that item
selected. As the user moves focus through the list to select different items, we
will update the UI to show the commands for the currently selected item.

![A prototype of the ListItem context menu with commands](./context-actions-prototype.png)

For more details on the structure of the `Actions` property, see the
[`ContextItem`s](#contextitems) section below.

As an example, here's how the Media Controls extension adds play/pause, next &
previous track context commands to the list of items:

```cs
internal sealed class MediaListItem : ListItem
{
    // Theis is an example of a ListItem that displays the currently track
    // This TogglePlayMediaAction is the default action when the user selects the item.
    public MediaListItem() : base(new TogglePlayMediaAction())
    {
        // These two commands make up the "More commands" flyout for the item.
        this.MoreCommands = [
            new CommandContextItem(new PrevNextTrackAction(true)),
            new CommandContextItem(new PrevNextTrackAction(false))
        ];

        GlobalSystemMediaTransportControlsSessionManager.RequestAsync().AsTask().ContinueWith(async (task) => {
            var manager = task.Result;
            var mediaSession = manager.GetCurrentSession();
            var properties = await this.mediaSession.TryGetMediaPropertiesAsync().AsTask();
            this.Title = properties.Title;
            // update other things too
        });

    }
}
internal sealed class TogglePlayMediaAction : InvokableCommand
{
    public string Name => "Play";
    public IconDataType Icon => new("\ue768"); //play
    public ICommandResult Invoke()
    {
        _ = mediaSession.TryTogglePlayPauseAsync();
        return new ICommandResult(CommandResultKind.KeepOpen);
    }
}
// And a similar InvokableCommand for the PrevNextTrackAction
```

Lists may either be a list of items like a traditional ListView, or they can be
a grid of items. Each of these items can be grouped into sections, which will be
displayed to the user in the order they are returned by the extension. Many
extensions will only have a single section, but if developers want to have lots
of grouped results, they're free to have as many sections as they like.

When the `GridProperties` property is set to null, DevPal will display the items
as a simple list, grouping them by section. When the `GridProperties` property
is set to a non-null value, DevPal will display the items as a grid, with each
item in the grid being a `TileSize` square. Grids are useful for showing items
that are more visual in nature, like images or icons.

Each item in the list may also include an optional `Details` property. This
allows the extension to provide additional information about the item, like a
description, a preview of a file, or a link to more information. For more
information on the structure of the `Details` property, see the
[`Details`](#details) section below.

If a list page returns a value from `PlaceholderText`, that text will be shown
as the placeholder for the filter on the page.

If a list page returns a value from `SearchText`, that text will be used to
initialize the search box on the page.

If the page returns `ShowDetails = true`, the DevPal automatically expand out
the [Details](#details) for list items with `Details` set to a non-null value.
If `ShowDetails = false`, the DevPal will not expand out the details for list
items by default, but will add a "Show details" action to the item's list of
actions (if it sets `Details`).
* For example: in the Windows Search box, when you search for an app, you get a
  "details" that is pre-expanded.
* Similarly for file searches - you get a preview of the file, and metadata on
  those details for the file path, last modified time, etc.
* But something like the "GitHub" extension may not want to always fetch issue
  bodies to show their details by default. So it would set `ShowDetails =
  false`. If the user activates the automatic "Show details" action, then the
  github action can then fetch the body and show it.

An example list page for the Hacker News extension:

```cs
class NewsPost {
    string Title;
    string Url;
    string CommentsUrl;
    string Poster;
    int Points;
}
class LinkAction(NewsPost post) : Microsoft.Windows.Run.Extensions.InvokableCommand {
    public string Name => "Open link";
    public ActionResult Invoke() {
        Process.Start(new ProcessStartInfo(post.Url) { UseShellExecute = true });
        return ActionResult.KeepOpen;
    }
}
class CommentAction(NewsPost post) : Microsoft.Windows.Run.Extensions.InvokableCommand {
    public string Name => "Open comments";
    public ActionResult Invoke() {
        Process.Start(new ProcessStartInfo(post.CommentsUrl) { UseShellExecute = true });
        return ActionResult.KeepOpen;
    }
}
class NewsListItem(NewsPost post) : Microsoft.Windows.Run.Extensions.ListItem {
    public string Title => post.Title;
    public string Subtitle => post.Poster;
    public IContextItem[] Commands => [
        new CommandContextItem(new LinkAction(post)),
        new CommandContextItem(new CommentAction(post))
    ];
    public ITag[] Tags => [ new Tag(){ Text=post.Points } ];
}
class HackerNewsPage: Microsoft.Windows.Run.Extensions.ListPage {
    public bool Loading => true;
    IListSection[] GetItems() {
        List<NewsItem> items = /* do some RSS feed stuff */;
        this.Loading = false;
        return new Microsoft.Windows.Run.Extensions.ListSection() {
            Title = "Posts",
            Items = items
                        .Select((post) => new NewsListItem(post))
                        .ToList()
        };
    }
}
```
##### Updating the list

Extension developers are able to update the list of items in real-time, by
raising an PropChanged event for the `Items` property of the `IListPage`. This
will cause DevPal to re-request the list of items via `GetItems`.

Consider for example a process list which updates in real-time. As the extension
determines that the list should change, it can raise the `Items` property
changed event.

> [!IMPORTANT]
> For extension developers: Best practice would be to cache your
> `IListItems` between calls. Minimizing the time it takes to respond to
> `GetItems` will make the UI feel more responsive.


> [!WARNING]
> We chose this API surface for a few reasons:
> * `IObservableCollection`, which has quite a good mechanism for specifying
>   exactly which parts of the collection changed, isn't exposed through WinRT
> * `IObservableVector` doesn't work well across process boundaries
> * In general, all the collection WinRT objects are Considered Harmful in
>   cross-proc scenarios
>
> But we want both static and dynamic lists to be able to update the results in
> real-time. Example: A process list that updates in real-time. We want to be
> able to add and remove items from the list as they start and stop.


##### Filtering the list

Lists are able to specify a set of filters that the user can use to filter or
pivot the list of results. These are wholly controlled by the extension. To
indicate that a list page supports filtering, it should set the `Filters`
property on the list page to a non-null value.

```c#
[uuid("ef5db50c-d26b-4aee-9343-9f98739ab411")]
interface IFilterItem {}

[uuid("0a923c7f-5b7b-431d-9898-3c8c841d02ed")]
interface ISeparatorFilterItem requires IFilterItem {}

interface IFilter requires IFilterItem {
    String Id { get; };
    String Name { get; };
    IconDataType Icon { get; };
}

interface IFilters {
    String CurrentFilterId { get; set; };
    IFilterItem[] Filters();
}
```

The extension specifies the text for these filters, and gives them each a unique
ID. It is also able to specify what the "default" filter is, with whatever the
initial value of `CurrentFilterId` is.

When DevPal calls `GetItems`, the extension can filter the list of items based
on the selected filter. If the user changes the filter in the UI, DevPal will
set `CurrentFilterId` to the ID of the selected filter, and call `GetItems`
again.

For example:

* the GitHub extension might provide filters for "Issues", "Pull Requests", and
  "Repositories". When the user selects "Issues", the GitHub extension will only
  return issues in the list of items.
* The Spotify extension may want to provide filters for "Playlists", "Artists",
  "Albums", 'Podcasts'. When the user selects "Artists", the Spotify extension
  will only return artists in the list of items.

#### Fallback actions

List items may also specify a `FallbackHandler`[^2]. This is an object that will be
informed whenever the query changes in List page hosting it. This is commonly
used for commands that want to allow the user to search for something that
they've typed that doesn't match any of the commands in the list.

For example, if the user types "What's the weather?":
* the Copilot action might want to be able to update their own action's `Name`
  to be "Ask Copilot 'What's the weather'?".
* The Store application may want to show a "Search the Store" action.
* And of course, the SpongeBot extension will want to update its name to "wHaT's
  tHe wEaThEr?".

This also gives the action an opportunity to know what the query was before the
the page is navigated to.

List items with a `FallbackHandler` will be shown in a static List view even if
the query doesn't match their `Title`/`Subtitle`/`Tags`, unless their `Title` is
empty, in which case they won't be shown. This allows for:
* Fallback items that have dynamic names in response to the search query, but
  not restricted to the query.
* Fallback items that are hidden until the user types something

As an example, here's how a developer might implement a fallback action that
changes its name to be mOcKiNgCaSe.

```cs
public class SpongebotPage : Microsoft.Windows.Run.Extensions.MarkdownPage, IFallbackAction
{
    // Name, Icon, IPropertyChanged: all those are defined in the MarkdownPage base class
    public SpongebotPage()
    {
        this.Name = "";
        this.Icon = new("https://imgflip.com/s/meme/Mocking-Spongebob.jpg");
    }
    public void IFallbackAction.UpdateQuery(string query) {
        if (string.IsNullOrEmpty(query)) {
            this.Name = "";
        } else {
            this.Name = ConvertToAlternatingCase(query);
        }
        return Task.CompletedTask.AsAsyncAction();
    }
    static string ConvertToAlternatingCase(string input) {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            sb.Append(i % 2 == 0 ? char.ToUpper(input[i]) : char.ToLower(input[i]));
        }
        return sb.ToString();
    }
    public override string Body() {
        var t = _GenerateMeme(this.Name); // call out to imgflip APIs to generate the meme
        t.ConfigureAwait(false);
        return t.Result;
    }
}
internal sealed class SpongebotCommandsProvider : ICommandProvider
{
    public IListItem[] TopLevelCommands()
    {
        var spongebotPage = new SpongebotPage();
        var listItem = new Microsoft.Windows.Run.Extensions.ListItem(spongebotPage);
        // ^ The ListItem ctor will automatically set its FallbackHandler to the
        // Action passed in, if the action implements IFallbackHandler
        return [ listItem ];
    }
}
```

`Microsoft.Windows.Run.Extensions.ListItem` in the SDK helpers will automatically set
the `FallbackHandler` property on the `IListItem` to the `Action` it's
initialized with, if that action implements `IFallbackHandler`. This allows the
action to directly update itself in response to the query. You may also specify
a different `IFallbackHandler`, if needed.

We'll include specific affordances within the DevPal settings to allow the user
to configure which top-level fallbacks are enabled, and in what order. This will
give the user greater control over the apps that can respond to queries that
don't match any of the commands in the list.

#### Markdown Pages

This is a page that displays a block of markdown text. This is useful for
showing a lot of information in a small space. Markdown provides a rich set of
simple formatting options.

![](./markdown-mock.png)

```csharp
interface IMarkdownPage requires IPage {
    String[] Bodies(); // TODO! should this be an IBody, so we can make it observable?
    IDetails Details();
    IContextItem[] Commands { get; };
}
```

A markdown page may also have a `Details` property, which will be displayed in
the same way as the details for a list item. This is useful for showing
additional information about the page, like a description, a preview of a file,
or a link to more information.

Similar to the `List` page, the `Commands` property is a list of commands that the
user can take on the page. These are the commands that will be shown in the "More
actions" flyout. Unlike the `List` page, the `Commands` property is not
associated with any specific item on the page, rather, these commands are global
to the page itself.

An example markdown page for an issue on GitHub:

```cs
class GitHubIssue {
    string Title;
    string Url;
    string Body;
    string Author;
    string[] Tags;
    string[] AssignedTo;
}
class OpenLinkAction(GitHubIssue issue) : Microsoft.Windows.Run.Extensions.InvokableCommand {
    public string Name => "Open";
    public ActionResult Invoke() {
        Process.Start(new ProcessStartInfo(issue.Url) { UseShellExecute = true });
        return ActionResult.KeepOpen;
    }
}
class GithubIssuePage(GithubIssue issue): Microsoft.Windows.Run.Extensions.MarkdownPage {
    public bool Loading => true;
    public string Body() {
        issue.Body = /* fetch the body from the API */;
        this.Loading = false;
        return issue.Body;
    }
    public IContextItem[] Commands => [ new CommandContextItem(new OpenLinkAction(issue)) ];
    public IDetails Details() {
        return new Details(){
            Title = "",
            Body = "",
            Metadata = [
                new Microsoft.Windows.Run.Extensions.DetailsTags(){
                    Key = "Author",
                    Tags = new(){ new Tag(){ Text = issue.Author } }
                },
                new Microsoft.Windows.Run.Extensions.DetailsTags(){
                    Key = "Assigned To",
                    Tags = issue.AssignedTo.Select((user) => new Tag(){ Text = user }).ToArray()
                },
                new Microsoft.Windows.Run.Extensions.DetailsTags(){
                    Key = "Tags",
                    Tags = issue.Tags.Select((tag) => new Tag(){ Text = tag }).ToArray()
                }
            ]
        };
    }
}
```

#### Form Pages

A form page allows the user to input data to the extension. This is useful for
actions that might require additional information from the user. For example:
imagine a "Send Teams message" action. This action might require the user to
input the message they want to send, and give the user a dropdown to pick the
chat to send the message to.

![](./form-page-prototype.png)

```csharp

interface IForm { // TODO! should this be observable?
    String TemplateJson();
    String DataJson();
    String StateJson();
    ICommandResult SubmitForm(String payload);
}
interface IFormPage requires IPage {
    IForm[] Forms();
}
```

Form pages are powered by [Adaptive Cards](https://adaptivecards.io/). This
allows extension developers a rich set of controls to use in their forms. Each
page can have as many forms as it needs. These forms will be displayed to the
user as separate "cards", in the order they are returned by the extension.

The `TemplateJson`, `DataJson`, and `StateJson` properties should be a JSON
string that represents the Adaptive Card to be displayed to the user.

When the user submits the form, the `SubmitForm` method will be called with the
JSON payload of the form. The extension is responsible for parsing this payload
and acting on it.

[TODO!discussion]: Do we want to stick the `Actions` on this page type too? Or does that not make sense?

### Other types

The following are additional type definitions that are used throughout the SDK.

#### `ContextItem`s

This represents a collection of items that might appear in the `MoreCommands`
flyout. Mostly, these are just commands and seperators.

#### `IconDataType`

This is a wrapper type for passing information about an icon to DevPal. This
allows extensions to specify apps in a variety of ways, including:

* A URL to an image on the web or filesystem
* A string for an emoji or Segoe Fluent icon
* A path to an exe, dll or lnk file, to extract the icon from
* A `IRandomAccessStream` to an image... [TODO!api-review] This is how DevHome does it but _why_

[TODO!]: actually define this.
<!-- In .CS because it's manually added to the idl -->
```cs
struct IconDataType {
    IconDataType(String iconString);
    String Icon { get; };
}
```

Terminal already has a robust arbitrary string -> icon loader that we can easily
reuse for this.

#### `Details`

This represents additional information that can be displayed about an action or
item. These can be present on both List pages and Markdown pages. For a List
page, each element may have its own details. For a Markdown page, the details
are global to the page.

This gif includes a mockup of what the details might look like on a list of
apps. The details have a title and a hero image, which the action has set to the
app's icon.

![](https://miro.medium.com/v2/resize:fit:720/format:webp/1*Nd5fvJM8LUQ1w3DAWN-pvA.gif)

(However, the buttons in the gif for "Open", "Uninstall", etc, are not part of
the `Details`, they are part of the "more commands" dropdown. **It's a mockup**)

<!-- This block needs to appear in the idl _before_ IListItem, but from a doc
storytelling standpoint, that doesn't make sense. We've secretly made it a `C#`
block, and the generator will pull this into the file first.   -->

```c#
interface ITag {
    IconDataType Icon { get; };
    String Text { get; };
    Windows.UI.Color Color { get; };
    String ToolTip { get; };
    ICommand Command { get; };
};

[uuid("6a6dd345-37a3-4a1e-914d-4f658a4d583d")]
interface IDetailsData {}
interface IDetailsElement {
    String Key { get; };
    IDetailsData Data { get; };
}
interface IDetails {
    IconDataType HeroImage { get; };
    String Title { get; };
    String Body { get; };
    IDetailsElement[] Metadata { get; };
}
interface IDetailsTags requires IDetailsData {
    ITag[] Tags { get; };
}
interface IDetailsLink requires IDetailsData {
    Windows.Foundation.Uri Link { get; };
    String Text { get; };
}
[uuid("58070392-02bb-4e89-9beb-47ceb8c3d741")]
interface IDetailsSeparator requires IDetailsData {}
```

#### `INotifyPropChanged`

You may have noticed the presence of the `INotifyPropChanged` interface on
`ICommand`. Typically this would be a `INotifyPropertyChanged` event from XAML.
However, we don't want to require that developers use XAML to create extensions.
So we'll provide a simple `PropertyChanged` event that developers can use to
notify DevPal that a property has changed.

<!-- In .cs because it's manually added to the interface -->
```cs
interface INotifyPropChanged {
    event Windows.Foundation.TypedEventHandler<Object, PropChangedEventArgs> PropChanged;
}
runtimeclass PropChangedEventArgs {
    String PropName { get; };
}
```

It's basically exactly the event from XAML. I've named it `PropChanged` to avoid
prevent confusion with the XAML version.

[TODO!api-review]: can we do some trickery in the `idl` to have this PropertyChanged be _literally the same as the XAML one_? So that if there's both in a dll, they get merge into one?

#### `ICommandProvider`

This is the interface that an extension must implement to provide commands to DevPal.

```csharp
interface ICommandProvider requires Windows.Foundation.IClosable
{
    String DisplayName { get; };
    IconDataType Icon { get; };
    // TODO! Boolean CanBeCached { get; };
    // TODO! IFormPage SettingsPage { get; };

    IListItem[] TopLevelCommands();
};
```

`TopLevelCommands` is the method that DevPal will call to get the list of actions
that should be shown when the user opens DevPal. These are the commands that will
allow the user to interact with the rest of your extension. They can be simple
actions, or they can be pages that the user can navigate to. Because these
top-level items are `IListItem`s, they can have `MoreCommands`, `Details` and
`Tags` as well. The main L0 of DevPal acts as though `ShowDetails=true`, so
`Details` present on top-level items will be shown to the user automatically.

### Settings

Extensions may also want to provide settings to the user.

[TODO!]: write this

It would be pretty trivial to just allow apps to provide a `FormPage` as their
settings page. That would hilariously just work I think. I dunno if Adaptive
Cards is great for real-time saving of settings though.

We probably also want to provide a helper class for storing settings, so that
apps don't need to worry too much about mucking around with that. I'm especially
thinking about storing credentials securely.

## Helper SDK Classes

As a part of the `Microsoft.CmdPal.Extensions` namespace, we'll provide a set of
default implementations and helper classes that developers can use to make
authoring extensions easier.

### Default implementations

We'll provide default implementations for the following interfaces:

* `IInvokableCommand`
* `IListItem`
* `IListSection`
* `ICommandContextItem`
* `ICommandResult`
* `IGoToPageArgs`
* `IChangeQueryArgs`
* `ISeparatorContextItem`
* `ISeparatorFilterItem`
* `IFilter`
* `IListPage`
* `IMarkdownPage`
* `IFormPage`
* `IDetailsTags`
* `IDetailsLink`
* `IDetailsSeparator`

This will allow developers to quickly create extensions without having to worry
about implementing every part of the interface. You can see that reference
implementation in
`extensionsdk\Microsoft.Windows.Run.Extensions.Lib\DefaultClasses.cs`.

In addition to the default implementations we provide for the interfaces above,
we should provide a set of helper classes that make it easier for developers to
write extensions.

For example, we should have something like:

```cs
class OpenUrlAction(string targetUrl, ActionResult result) : Microsoft.Windows.Run.Extensions.InvokableCommand {
    public string Name => "Open";
    public IconDataType Icon => "\uE8A7"; // OpenInNewWindow
    public ActionResult Invoke() {
        Process.Start(new ProcessStartInfo(targetUrl) { UseShellExecute = true });
        return result;
    }
}
```

Then, the **entire** Hacker News example from above becomes the following. Note
that no longer do we need to add additional classes for the actions. We just use
the helper:

```cs
class NewsListItem(NewsPost post) : Microsoft.Windows.Run.Extensions.ListItem {
    public string Title => post.Title;
    public string Subtitle => post.Url;
    public IContextItem[] Commands => [
        new CommandContextItem(new Microsoft.Windows.Run.Extensions.OpenUrlAction(post.Url, ActionResult.KeepOpen)),
        new CommandContextItem(new Microsoft.Windows.Run.Extensions.OpenUrlAction(post.CommentsUrl, ActionResult.KeepOpen){
            Name = "Open comments",
            Icon = "\uE8F2" // ChatBubbles
        })
    ];
    public ITag[] Tags => [ new Tag(){ Text=post.Poster, new Tag(){ Text=post.Points } ];
}
class HackerNewsPage: Microsoft.Windows.Run.Extensions.ListPage {
    public bool Loading => true;
    IListSection[] GetItems(String query) {
        List<NewsItem> items = /* do some RSS feed stuff */;
        this.Loading = false;
        return [
            new ListSection() {
                Title = "Posts",
                Items = items
                            .Select((post) => new NewsListItem(post))
                            .ToArray()
            }
        ];
    }
}
```
### Using the Clipboard

Typically, developers would be expected to use the
`Clipboard` class from the `Windows.ApplicationModel.DataTransfer` namespace.
However, this class comes with restrictions around the ability to call it from a
background thread. Since extensions are always running in the background, this
presents persistent difficulties.

We'll provide a helper class that allows developers to easily use the clipboard
in their extensions.

## Advanced scenarios

### Status messages

[TODO!]: write this

[TODO!]: I'm removing the bit about status updates for now. This is tricky, because things like `InvokableCommand`s want to be able to set status messages too, and they won't necessarily be able to communicate back up to the page hosting them.
Actions should be able to display realtime feedback to the user:

<img src="./grid-actions-mock.png" height="300px" /> <img src="./grid-status-loading-mock.png" height="300px" /> <img src="./grid-status-success-mock.png" height="300px" />

* PushStatus(StatusMessage) - this will push a status message to the UI. This
  will be shown in the status bar at the bottom of the window. The status bar
  will show the most recent status message.
* PopStatus() - this will begin to fade out the active status message. If there
  are no more status messages, the status bar will disappear.

```cs
runtimeclass PageStatus {
    bool IsIndeterminate { get; };
    double ProgressPercent { get; };
    PageStatusKind Kind { get; };
    String Caption { get; };
}
enum PageStatusKind {
    Info,
    Success,
    Warning,
    Error
}
```

push pop sounds wrong. Maybe `AddStatus` and `ClearStatus`?

## Class diagram

This is a diagram attempting to show the relationships between the various types we've defined for the SDK. Some elements are omitted for clarity. (Notably, `IconDataType` and `IPropChanged`, which are used in many places.)

The notes on the arrows help indicate the multiplicity of the relationship.
* "*" means 0 or more (for arrays)
* "?" means 0 or 1 (for optional/nullable properties)
* "1" means exactly 1 (for required properties)

```mermaid
classDiagram
    class ICommand {
        String Name
        IconDataType Icon
    }
    IPage --|> ICommand
    class IPage  {
        String Title
        Boolean Loading
    }

    IInvokableCommand --|> ICommand
    class IInvokableCommand  {
        ICommandResult Invoke()
    }

    class IForm {
        String TemplateJson()
        String DataJson()
        String StateJson()
        ICommandResult SubmitForm(String payload)
    }
    IFormPage --|> IPage
    class IFormPage  {
        IForm[] Forms()
    }
    IForm "*" *-- IFormPage

    IMarkdownPage --|> IPage
    class IMarkdownPage  {
        String[] Bodies()
        IDetails Details()
        IContextItem[] Commands
    }
    %% IMarkdownPage *-- IDetails
    IContextItem "*" *-- IMarkdownPage
    IDetails "?" *-- IMarkdownPage
    %%%%%%%%%

    class IFilterItem

    ISeparatorFilterItem --|> IFilterItem
    class ISeparatorFilterItem

    IFilter --|> IFilterItem
    class IFilter  {
        String Id
        String Name
        IconDataType Icon
    }

    class IFilters {
        String CurrentFilterId
        IFilterItem[] AvailableFilters()
    }
    IFilterItem "*" *-- IFilters

    class IFallbackHandler {
        void UpdateQuery(String query)
    }


    %% IListItem --|> INotifyPropChanged
    class IListItem  {
        IconDataType Icon
        String Title
        String Subtitle
        ICommand Command
        IContextItem[] MoreCommands
        ITag[] Tags
        IDetails Details
        IFallbackHandler FallbackHandler
    }
    IContextItem "*" *-- IListItem
    IDetails "?" *-- IListItem
    ICommand "?" *-- IListItem
    ITag "*" *-- IListItem
    IFallbackHandler "?" *-- IListItem

    class ISection {
        String Title
        IListItem[] Items
    }
    IListItem "*" *-- ISection

    class IGridProperties  {
        Windows.Foundation.Size TileSize
    }

    IListPage --|> IPage
    class IListPage  {
        String SearchText
        String PlaceholderText
        Boolean ShowDetails
        IFilters Filters
        IGridProperties GridProperties

        ISection[] GetItems()
    }
    IGridProperties "?" *-- IListPage
    ISection "*" *-- IListPage
    IFilters "*" *-- IListPage

    IDynamicListPage --|> IListPage
    class IDynamicListPage  {
        ISection[] GetItems(String query)
    }

    class IDetails {
        IconDataType HeroImage
        String Title
        String Body
        IDetailsElement[] Metadata
    }

    class ITag {
        IconDataType Icon
        String Text
        Windows.UI.Color Color
        String ToolTip
        ICommand Command
    }
    ICommand "?" *-- ITag

    %%%%%%
    class IContextItem

    ISeparatorContextItem --|> IContextItem
    class ISeparatorContextItem
    ICommandContextItem --|> IContextItem
    class ICommandContextItem  {
        ICommand Command
        String Tooltip
        Boolean IsCritical
    }
    ICommand "?" *-- ICommandContextItem



    class ICommandProvider {
        String DisplayName
        IconDataType Icon

        IListItem[] TopLevelCommands()
    }
    IListItem "*" *-- ICommandProvider
```


## Future considerations

### Arbitrary parameters and arguments

Something we'll want to consider soon is how to allow for arbitrary parameters
to be passed to commands. This allows for commands to require additional info from
the user _before_ they are run. In its simplest form, this is a lightweight way
to have an action accept form data inline with the query. But this also allows
for highly complex action chaining.

I had originally started to spec this out as:

```cs
interface IInvokableCommandWithParameters requires ICommand {
    ActionParameters Parameters { get; };
    ActionResult InvokeWithArgs(ActionArguments args);
}
```

And `ActionParameters` would be a set of `{ type, name, required}` structs,
which would specify the parameters that the action needs. Simple types would be
`string`, `file`, `file[]`, `enum` (with possible values), etc.

But that may not be complex enough. We recently learned about Action Framework
and some of their plans there - that may be a good fit for this. My raw notes
follow - these are not part of the current SDK spec.

> [!NOTE]
>
> A thought: what if a action returns a `ActionResult.Entity`, then that takes
> devpal back home, but leaves the entity in the query box. This would allow for
> a Quicksilver-like "thing, do" flow. That command would pre-populate the
> parameters. So we would then filter top-level commands based on things that can
> accept the entity in the search box.
>
> For example: The user uses the "Search for file" list page. They find the file
> they're looking for. That file's ListItem has a context item "With
> {filename}..." that then returns a `ActionResult.Entity` with the file entity.
> The user is taken back to the main page, and a file picker badge (with that
> filename) is at the top of the search box. In that state, the only commands
> now shown are ones that can accept a File entity. This could be things like
> the "Remove background" action (from REDACTED), the "Open with" action, the
> "Send to Teams chat" (which would then ask for another entity). If they did
> the "Remove Background" one, that could then return _another_ entity.
>
> We'd need to solve for the REDACTED case specifically, cause I bet they want to
> stay in the REDACTED action page, rather than the main one.
>
> We'd also probably want the REDACTED one to be able to accept arbitrary
> entities... like, they probably want a `+` button that lets you add... any
> kind of entity to their page, rather than explicitly ask for a list of args.

However, we do not have enough visibility on how action framework actually
works, consumer-wise, to be able to specify more. As absolutely fun as chaining
actions together sounds, I've decided to leave this out of the official v1 spec.
We can ship a viable v0.1 of DevPal without it, and add it in post.

### URI activation

We should also consider how to allow for URI activation. This would allow for
extensions to be activated by a URI, rather than by the user typing a query.
Consider another app that may want to boot into a particular action page in
DevPal.

I'd imagine that we'd surface some URI scheme that would look like:

```
devpal://commands/{{extension_id}}/{{page_id}}?param=arg
```

that could be used to activate a particular page in an app. Apps would need to
be able to specify their own extension ID (all my homies dislike working with
PFNs). DevPal would also need to be able to control which schemes are active,
and if they're active, which apps can call through to it (if that's knowable).

This remains under-specified for now. It's a cool feature for the future, but
one that's not needed for v1.

### Custom "empty list" messages

We should consider how to allow for extensions to specify a custom element to be
shown to the user when the page stops loading and the list of elements filtered
is empty.
Is that just a `Details` object? A markdown body?


## Footnotes

The `.idl` for this SDK can be generated directly from this file. To do so, run the following command:

```ps1
.\generate-interface.ps1 > .\Microsoft.DevPalette.Extensions.idl
```

(After a `pip3 install mistletoe`)

Or, to generate straight to the place I'm consuming it from:

```ps1
.\doc\initial-sdk-spec\generate-interface.ps1 > .\extensionsdk\Microsoft.CmdPal.Extensions\Microsoft.Windows.Run.Extensions.idl
```

[^1]: In this example, as in other places, I've referenced a
    `Microsoft.DevPal.Extensions.InvokableCommand` class, as the base for that action.
    Our SDK will include partial class implementations for interfaces like
    `ICommand`, `IListPage`, etc. These partial implementations will provide
    default implementations for common properties and methods, like
    `PropertyChanged` events, and `Icon` properties. Developers can choose to
    use these partial implementations, or implement the interfaces themselves.
    See the [Default implementations](#default-implementations) section for more
    information.

[^2]: It is a little weird that `IListItem` has a `FallbackHandler` property,
    rather than just allowing `ICommands` to implement `IFallbackHandler`. This
    is unfortunately due to a quirk in Metadata-based marshalling (MBM) in
    WinRT. MBM doesn't clearly expose when a runtimeclass implements multiple
    non-derived interfaces. Something that implements `IMarkdownPage` and
    `IFallbackHandler` can't be trivially casted from one type to the other.
    However, by making the `FallbackHandler` a property of the ListItem itself,
    then the extension itself can cast between the types, without the need for
    MBM. Thanks to Mano for helping me figure this one out.

[Dev Home Extension]: https://learn.microsoft.com/en-us/windows/dev-home/extensions
