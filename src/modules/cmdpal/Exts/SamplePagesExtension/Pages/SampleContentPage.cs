// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace SamplePagesExtension;

internal sealed partial class SampleContentPage : ContentPage
{
    private readonly SampleContentForm sampleForm = new();
    private readonly MarkdownContent sampleMarkdown = new() { Body = "# Sample page with mixed content \n This page has both markdown, and form content" };

    public override IContent[] GetContent() => [sampleMarkdown, sampleForm];

    public SampleContentPage()
    {
        Name = "Sample Content";
        Icon = new(string.Empty);
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code")]
internal sealed partial class SampleContentForm : FormContent
{
    public SampleContentForm()
    {
        TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "size": "medium",
            "weight": "bolder",
            "text": " ${ParticipantInfoForm.title}",
            "horizontalAlignment": "center",
            "wrap": true,
            "style": "heading"
        },
        {
            "type": "Input.Text",
            "label": "Name",
            "style": "text",
            "id": "SimpleVal",
            "isRequired": true,
            "errorMessage": "Name is required",
            "placeholder": "Enter your name"
        },
        {
            "type": "Input.Text",
            "label": "Homepage",
            "style": "url",
            "id": "UrlVal",
            "placeholder": "Enter your homepage url"
        },
        {
            "type": "Input.Text",
            "label": "Email",
            "style": "email",
            "id": "EmailVal",
            "placeholder": "Enter your email"
        },
        {
            "type": "Input.Text",
            "label": "Phone",
            "style": "tel",
            "id": "TelVal",
            "placeholder": "Enter your phone number"
        },
        {
            "type": "Input.Text",
            "label": "Comments",
            "style": "text",
            "isMultiline": true,
            "id": "MultiLineVal",
            "placeholder": "Enter any comments"
        },
        {
            "type": "Input.Number",
            "label": "Quantity (Minimum -5, Maximum 5)",
            "min": -5,
            "max": 5,
            "value": 1,
            "id": "NumVal",
            "errorMessage": "The quantity must be between -5 and 5"
        },
        {
            "type": "Input.Date",
            "label": "Due Date",
            "id": "DateVal",
            "value": "2017-09-20"
        },
        {
            "type": "Input.Time",
            "label": "Start time",
            "id": "TimeVal",
            "value": "16:59"
        },
        {
            "type": "TextBlock",
            "size": "medium",
            "weight": "bolder",
            "text": "${Survey.title} ",
            "horizontalAlignment": "center",
            "wrap": true,
            "style": "heading"
        },
        {
            "type": "Input.ChoiceSet",
            "id": "CompactSelectVal",
            "label": "${Survey.questions[0].question}",
            "style": "compact",
            "value": "1",
            "choices": [
                {
                    "$data": "${Survey.questions[0].items}",
                    "title": "${choice}",
                    "value": "${value}"
                }
            ]
        },
        {
            "type": "Input.ChoiceSet",
            "id": "SingleSelectVal",
            "label": "${Survey.questions[1].question}",
            "style": "expanded",
            "value": "1",
            "choices": [
                {
                    "$data": "${Survey.questions[1].items}",
                    "title": "${choice}",
                    "value": "${value}"
                }
            ]
        },
        {
            "type": "Input.ChoiceSet",
            "id": "MultiSelectVal",
            "label": "${Survey.questions[2].question}",
            "isMultiSelect": true,
            "value": "1,3",
            "choices": [
                {
                    "$data": "${Survey.questions[2].items}",
                    "title": "${choice}",
                    "value": "${value}"
                }
            ]
        },
        {
            "type": "TextBlock",
            "size": "medium",
            "weight": "bolder",
            "text": "Input.Toggle",
            "horizontalAlignment": "center",
            "wrap": true,
            "style": "heading"
        },
        {
            "type": "Input.Toggle",
            "label": "Please accept the terms and conditions:",
            "title": "${Survey.questions[3].question}",
            "valueOn": "true",
            "valueOff": "false",
            "id": "AcceptsTerms",
            "isRequired": true,
            "errorMessage": "Accepting the terms and conditions is required"
        },
        {
            "type": "Input.Toggle",
            "label": "How do you feel about red cars?",
            "title": "${Survey.questions[4].question}",
            "valueOn": "RedCars",
            "valueOff": "NotRedCars",
            "id": "ColorPreference"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Submit",
            "data": {
                "id": "1234567890"
            }
        },
        {
            "type": "Action.ShowCard",
            "title": "Show Card",
            "card": {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "Input.Text",
                        "label": "Enter comment",
                        "style": "text",
                        "id": "CommentVal"
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Submit",
                        "title": "OK"
                    }
                ]
            }
        }
    ]
}
""";

        DataJson = $$"""
{
    "ParticipantInfoForm": {
        "title": "Input.Text elements"
    },
    "Survey": {
        "title": "Input ChoiceSet",
        "questions": [
            {
                "question": "What color do you want? (compact)",
                "items": [
                    {
                        "choice": "Red",
                        "value": "1"
                    },
                    {
                        "choice": "Green",
                        "value": "2"
                    },
                    {
                        "choice": "Blue",
                        "value": "3"
                    }
                ]
            },
            {
                "question": "What color do you want? (expanded)",
                "items": [
                    {
                        "choice": "Red",
                        "value": "1"
                    },
                    {
                        "choice": "Green",
                        "value": "2"
                    },
                    {
                        "choice": "Blue",
                        "value": "3"
                    }
                ]
            },
            {
                "question": "What color do you want? (multiselect)",
                "items": [
                    {
                        "choice": "Red",
                        "value": "1"
                    },
                    {
                        "choice": "Green",
                        "value": "2"
                    },
                    {
                        "choice": "Blue",
                        "value": "3"
                    }
                ]
            },
            {
                "question": "I accept the terms and conditions (True/False)"
            },
            {
                "question": "Red cars are better than other cars"
            }
        ]
    }
}
""";
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null)
        {
            return CommandResult.GoHome();
        }

        // Application.Current.GetService<ILocalSettingsService>().SaveSettingAsync("GlobalHotkey", formInput["hotkey"]?.ToString() ?? string.Empty);
        return CommandResult.GoHome();
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code")]
internal sealed partial class SampleTreeContentPage : ContentPage
{
    private readonly TreeContent myContentTree;

    public override IContent[] GetContent() => [myContentTree];

    public SampleTreeContentPage()
    {
        Name = "Sample Content";
        Icon = new(string.Empty);

        myContentTree = new()
        {
            RootContent = new MarkdownContent() { Body = "# This page has nested content" },
            Children = [
                new TreeContent()
                {
                    RootContent = new MarkdownContent() { Body = "Yo dawg" },
                    Children = [
                        new TreeContent()
                        {
                            RootContent = new MarkdownContent() { Body = "I heard you like content" },
                            Children = [
                                new MarkdownContent() { Body = "So we put content in your content" },
                                new FormContent() { TemplateJson = "{\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"type\":\"AdaptiveCard\",\"version\":\"1.6\",\"body\":[{\"type\":\"TextBlock\",\"size\":\"medium\",\"weight\":\"bolder\",\"text\":\"Mix and match why don't you\",\"horizontalAlignment\":\"center\",\"wrap\":true,\"style\":\"heading\"},{\"type\":\"TextBlock\",\"text\":\"You can have forms here too\",\"horizontalAlignment\":\"Right\",\"wrap\":true}],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"It's a form, you get it\",\"data\":{\"id\":\"LoginVal\"}}]}" },
                                new MarkdownContent() { Body = "Another markdown down here" },
                            ],
                        },
                        new MarkdownContent() { Body = "**slaps roof**" },
                        new MarkdownContent() { Body = "This baby can fit so much content" },

                    ],
                },
                new PostContent("Test post pls ignore")
                {
                    Replies = [
                        new PostContent("First"),
                        new PostContent("First\nEDIT: shoot"),
                    ],
                }
            ],
        };
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code")]
internal sealed partial class PostContent : TreeContent
{
    public List<IContent> Replies { get; init; } = [];

    public PostContent(string body)
    {
        RootContent = new PostForm(body, this);
    }

    public override IContent[] GetChildren() => Replies.ToArray();

    public void Post() => RaiseItemsChanged(Replies.Count);
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample code")]
internal sealed partial class PostForm : FormContent
{
    private readonly PostContent _parent;

    public PostForm(string postBody, PostContent parent)
    {
        _parent = parent;
        TemplateJson = """
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "${postBody}",
            "wrap": true
        }
    ],
    "actions": [
        {
            "type": "Action.ShowCard",
            "title": "${replyCard.title}",
            "card": {
                "type": "AdaptiveCard",
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.6",
                "body": [
                    {
                        "type": "Container",
                        "id": "${replyCard.idPrefix}Properties",
                        "items": [
                            {
                                "$data": "${replyCard.fields}",
                                "type": "Input.Text",
                                "label": "${label}",
                                "id": "${id}",
                                "isRequired": "${required}",
                                "isMultiline": true,
                                "errorMessage": "'${label}' is required"
                            }
                        ]
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Submit",
                        "title": "Post"
                    }
                ]
            }
        },
        {
            "type": "Action.Submit",
            "title": "Favorite"
        },
        {
            "type": "Action.Submit",
            "title": "View on web"
        }
    ]
}
""";
        DataJson = $$"""
{
    "postBody": {{JsonSerializer.Serialize(postBody)}},
    "replyCard": {
        "title": "Reply",
        "idPrefix": "reply",
        "fields": [
            {
                "label": "Reply",
                "id": "ReplyBody",
                "required": true,
                "placeholder": "Write a reply here"
            }
        ]
    }
}
""";
    }

    public override ICommandResult SubmitForm(string payload)
    {
        var data = JsonNode.Parse(payload);
        _ = data;
        var reply = data["ReplyBody"];
        var s = reply?.AsValue()?.ToString();
        if (!string.IsNullOrEmpty(s))
        {
            _parent.Replies.Add(new PostContent(s));
            _parent.Post();
        }

        return CommandResult.KeepOpen();
    }
}
