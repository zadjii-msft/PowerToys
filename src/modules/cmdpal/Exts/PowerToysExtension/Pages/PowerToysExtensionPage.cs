// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Microsoft.UI.Xaml.Controls;
using PowerToysExtension.Actions;

namespace PowerToysExtension;

internal sealed partial class PowerToysExtensionPage : ListPage
{
    public PowerToysExtensionPage()
    {
        Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "Assets\\PowerToys.ico"));
        Name = "PowerToys Utilities";
    }

    public override ISection[] GetItems()
    {
        return [
            new ListSection()
            {
                Items = [
                    new ListItem(new ColorPickerAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new CropAndLockReparentAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new CropAndLockThumbnailAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new FancyZonesEditorAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new FindMyMouseAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new MouseCrosshairsAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new MouseHighlighterAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new ScreenRulerAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new ShortcutGuideAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new TextExtractorAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                    new ListItem(new WorkspacesEditorAction())
                        {
                            Subtitle = "PowerToys",
                            Tags = [new Tag()
                                    {
                                        Text = "Utility",
                                    }
                            ],
                        },
                ],
            }
        ];
    }
}
